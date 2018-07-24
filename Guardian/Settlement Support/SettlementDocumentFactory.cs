namespace FluidTrade.Guardian
{

	using System;
	using System.Configuration;
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using System.IdentityModel.Policy;
	using System.IdentityModel.Claims;
	using System.IO;
	using System.Security.Principal;
	using System.Threading;
	using System.Transactions;
	using System.Reflection;
	using FluidTrade.Core;
	using FluidTrade.Guardian;
    using FluidTrade.Office;

    /// <summary>Finds matching orders and negotiations the exchange of assets.</summary>
    /// <copyright>Copyright (C) 1998-2005 Fluid Trade -- All Rights Reserved.</copyright>	
    public class SettlementDocumentFactory
	{

		// Private Constants
		private const Int32 threadWait = 100;

		// Private Static Methods
		private static WaitQueue<ObjectAction> actionQueue;
		private static AuthorizationPolicy authorizationPolicy;
		private static ClaimsPrincipal claimsPrincipal;
		private static Thread factoryThread;
		private static IMailMerge iMailMerge;

		/// <summary>
		/// Brings buyers and sellers of equities together.
		/// </summary>
		static SettlementDocumentFactory()
		{

			try
			{

				// This is normally created by a channel when it establishes a connection.  But this is a daemon process without a connection, so an
				// authorization policy needs be created explicilty in order to have a thread that can run as an authorized user.
				SettlementDocumentFactory.authorizationPolicy = new AuthorizationPolicy();

				// This queue is filled up with Working Orders that need to be serviced because something changed the matching criteria.
				SettlementDocumentFactory.actionQueue = new WaitQueue<ObjectAction>();

				// This identity and set of claims gives the worker threads access to do anything to the data model.
				List<Claim> listClaims = new List<Claim>();
				listClaims.Add(new Claim(FluidTrade.Core.ClaimTypes.Create, Resources.Application, Rights.PossessProperty));
				listClaims.Add(new Claim(FluidTrade.Core.ClaimTypes.Update, Resources.Application, Rights.PossessProperty));
				listClaims.Add(new Claim(FluidTrade.Core.ClaimTypes.Read, Resources.Application, Rights.PossessProperty));
				listClaims.Add(new Claim(FluidTrade.Core.ClaimTypes.Destroy, Resources.Application, Rights.PossessProperty));
				ClaimSet adminClaims = new DefaultClaimSet(ClaimsAuthorizationPolicy.IssuerClaimSet, listClaims.ToArray());
				SettlementDocumentFactory.claimsPrincipal = new ClaimsPrincipal(
					new GenericIdentity("superuser@fluidtrade.com"),
					adminClaims);

                // This will create an instance of the Mail Merge factory that takes templates and mail merge fields and generates a complete document.  It is 
                // loaded up dymanically so that servers that don't have Microsoft Office installed on them will be able to run, but will not be able to
                // generate settlement letters.
                SettlementDocumentFactory.iMailMerge = new MailMerge();

				if (SettlementDocumentFactory.iMailMerge == null)
					EventLog.Information("This server is unable to process Mail Merge functions.");
				else
				{

					// These business rules will move negotiation information across the Chinese wall if a counter party exists.
					DataModel.ConsumerDebtSettlement.ConsumerDebtSettlementRowChanged += OnConsumerDebtSettlementRowChanged;

					// This thread will execution the actions that are created by changes to the data model.  The triggers themselves can't modify the data
					// model because the triggers are called from the commit handlers.
					SettlementDocumentFactory.factoryThread = new Thread(new ThreadStart(SettlementDocumentFactory.FactoryThread));
					SettlementDocumentFactory.factoryThread.Name = "Crossing Thread";
					SettlementDocumentFactory.factoryThread.IsBackground = true;
					SettlementDocumentFactory.factoryThread.Start();

				}

			}
			catch (Exception exception)
			{
                if (exception.InnerException != null)
                {
                    EventLog.Error("{0}, {1}", exception.InnerException.Message, exception.InnerException.StackTrace);
                }

                EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);

			}

		}

		/// <summary>
		/// Pulls actions and their parameters off the queue and executes them.
		/// </summary>
		private static void FactoryThread()
		{

			// All the actions added to the generic list of actions and parameter will execute with this claims principal.
			Thread.CurrentPrincipal = SettlementDocumentFactory.claimsPrincipal;

			// The event handlers for the data model can't wait on locks and resources outside the data model.  There would simply be too many resources that 
			// could deadlock.  This code will pull requests off of a generic queue of actions and parameters and execute them using the authentication created
			// above.
			while (true)
			{

				try
				{

					// The thread will wait here until an action has been placed in the queue to be processed in this thread context.
					ObjectAction objectAction = SettlementDocumentFactory.actionQueue.Dequeue();
					objectAction.DoAction(objectAction.Key, objectAction.Parameters);

				}
				catch (Exception exception)
				{

					// This will catch any exceptions thrown during the processing of the generic actions.
					EventLog.Error("{0}: {1}", exception.Message, exception.StackTrace);

				}

			}

		}

		/// <summary>
		/// Evaluates whether a given working order is eligible for a cross with another order.
		/// </summary>
		/// <param name="key">The key of the object to be handled.</param>
		/// <param name="parameters">A generic list of paraneters to the handler.</param>
		public static void MergeDocument(Object[] key, params Object[] parameters)
		{

			// Extract the strongly typed variables from the generic parameters.
			Guid consumerDebtSettlementId = (Guid)key[0];

			// This structure will collect the information required for the merge operation.
			MergeInfo mergeInfo = new MergeInfo();
			mergeInfo.ConsumerDebtSettlementId = consumerDebtSettlementId;

			// An instance of the data model is required for CRUD operations.
			DataModel dataModel = new DataModel();

			// If two counterparties agree on a transaction then a settlement report is generated from the Word Template associated with the Consumer Debt
			// Entity.
			using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew, TimeSpan.FromHours(1)))
			{

				// This provides a context for any transactions.
				DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

				// It is important to minimize the locking for these transactions since they will drag the system performance down and create deadlock 
				// sitiations if too many are held for too long.
				BlotterRow blotterRow = null;
				ConsumerDebtSettlementRow consumerDebtSettlementRow = null;
				ConsumerDebtNegotiationRow consumerDebtNegotiationRow = null;
				MatchRow matchRow = null;
				CreditCardRow creditCardRow = null;
				WorkingOrderRow workingOrderRow = null;

				try
				{

					// The ConsumerDebtSettlement row is where the search for the Settlement Information begins.
					consumerDebtSettlementRow = DataModel.ConsumerDebtSettlement.ConsumerDebtSettlementKey.Find(consumerDebtSettlementId);
					consumerDebtSettlementRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

					// There is no need to generate a report on a settlement that isn't new.  This will momentarily lock the status table so we
					// can see if the settlement is new.
					try
					{
						consumerDebtSettlementRow.StatusRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
						if (consumerDebtSettlementRow.StatusRow.StatusCode != Status.New)
							return;
					}
					finally
					{
						consumerDebtSettlementRow.StatusRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
					}

					// The RowVersion is needed to update the record with the new PDF report.
					mergeInfo.RowVersion = consumerDebtSettlementRow.RowVersion;

					// The negotiation row contains the link to the base matching row.
					consumerDebtNegotiationRow = consumerDebtSettlementRow.ConsumerDebtNegotiationRow;
					consumerDebtNegotiationRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

					// The base matching row is where we'll find the working order.
					matchRow = consumerDebtNegotiationRow.MatchRow;
					matchRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

					// The working order row is where the blotter can be found.
					workingOrderRow = matchRow.WorkingOrderRow;
					workingOrderRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

					// And the blotter will lead us to the Entity hierarchy which is where we'll find the rules.
					blotterRow = workingOrderRow.BlotterRow;
					blotterRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

					// The 'Pending' status is applied to the Settlement after the letter has been generated.  The status needs to be picked up while we're
					// still locking and reading tables.
					StatusRow statusRow = DataModel.Status.StatusKeyStatusCode.Find(Status.Pending);
					try
					{
						statusRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
						mergeInfo.StatusId = statusRow.StatusId;
					}
					finally
					{
						statusRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
					}

					//Find the consumerDebt to find the credit Card to get the origianl creditor
					ConsumerTrustNegotiationRow trustNegotiation = null;
					MatchRow contraWorMatchRow = DataModel.Match.MatchKey.Find(matchRow.ContraMatchId);
					contraWorMatchRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
					try
					{
						trustNegotiation = contraWorMatchRow.GetConsumerTrustNegotiationRows()[0];
											
					}
					finally
					{
						if (contraWorMatchRow != null)
							contraWorMatchRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
						
					}

					//Consumer Debt Row
					trustNegotiation.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
					try
					{						
						creditCardRow = trustNegotiation.CreditCardRow;

					}
					finally
					{
						if (trustNegotiation != null)
							trustNegotiation.ReleaseReaderLock(dataModelTransaction.TransactionId);					
					}
					

					creditCardRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

					// There is only going to be one Debt Class record associated with this blotter, but the iteration is an easier construct to work with than
					// the equivalent array logic for a single element.
					foreach (DebtClassRow debtClassRow in blotterRow.GetDebtClassRows())
					{

						// This variable will keep track of our current location as we crawl up the hierarchy.  It is important to release the records as soon
						// as possible to reduce the likelyhood of a deadlock.
						DebtClassRow currentDebtClassRow = debtClassRow;
						DebtClassRow nextDebtClassRow = null;

						// This flag will be set when a Debt Class in the hierarchy contains a Debt Rule.
						Boolean isFound = false;

						// This will crawl up the hierarchy until a Debt Class is found with a rule.  This rule will provide the opening values for the bid on
						// this negotiation.
						do
						{

							try
							{

								// This will lock the current item in the hierarchy so it can be examined for a rule or, failing that, a parent element.
								currentDebtClassRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

								// If the current Debt Class has no rule then the Entity Hierarchy is used to find the parent element.
								if (currentDebtClassRow.IsSettlementTemplateNull())
								{

									// The entity is the root of all objects in the hierarchy.  From this object the path to the parent Debt Class can be
									// navigated.
									EntityRow entityRow = DataModel.Entity.EntityKey.Find(currentDebtClassRow.DebtClassId);

									try
									{

										// Each entity needs to be locked before the relation can be used.
										entityRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

										// This will find each relation in the hierarchy which uses the current node as a child.
										foreach (EntityTreeRow entityTreeRow in entityRow.GetEntityTreeRowsByFK_Entity_EntityTree_ChildId())
										{

											try
											{

												// Lock the relation down before navigating to the parent.
												entityTreeRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

												// This is the parent entity of the current entity in the crawl up the hierarchy.
												EntityRow parentEntityRow = entityTreeRow.EntityRowByFK_Entity_EntityTree_ParentId;

												try
												{

													// The parent entity must be locked befor it can be checked for blotters and then, in turn, debt 
													// classes.
													parentEntityRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

													// In practice, there will be zero or one blotter rows.  The iteration makes it easier to check both 
													// conditions.
													foreach (BlotterRow parentBlotterRow in parentEntityRow.GetBlotterRows())
													{

														try
														{

															// The blotter must be locked before iterating through the Debt Classes that may be associated 
															// with the blotter.
															parentBlotterRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

															// Each blotter can have zero or one Debt Classes associated with it.  This is a long an 
															// tortuous way to finally get to the parent Debt Class.
															foreach (DebtClassRow parentDebtClassRow in parentBlotterRow.GetDebtClassRows())
															{

																try
																{

																	// Now that we've finally found the parent Debt Class, it will become the parent on the
																	// next pass through the hierarchy.  Note that the locks are released each time we pass
																	// through a level of the hierarchy.
																	parentDebtClassRow.AcquireReaderLock(
																		dataModelTransaction.TransactionId,
																		DataModel.LockTimeout);
																	nextDebtClassRow = parentDebtClassRow;

																}
																finally
																{

																	// The locks are released after the parent Debt Class is found.
																	parentDebtClassRow.ReleaseReaderLock(dataModelTransaction.TransactionId);

																}

															}

														}
														finally
														{

															// The locks are released after the each level of the hierarchy is checked.
															parentBlotterRow.ReleaseReaderLock(dataModelTransaction.TransactionId);

														}

													}

												}
												finally
												{

													// The parent Entity record is released after each level of the hiearchy is examined for a parent.
													parentEntityRow.ReleaseReaderLock(dataModelTransaction.TransactionId);

												}

											}
											finally
											{

												// The relationship record is released after each level of the hierarchy is examined for a parent.
												entityTreeRow.ReleaseReaderLock(dataModelTransaction.TransactionId);

											}

										}

									}
									finally
									{

										// Finaly, the current entity is released.  This allows us to finally move on to the next level of the hierarchy
										// without having to hold the locks for the entire transaction.
										entityRow.ReleaseReaderLock(dataModelTransaction.TransactionId);

									}

								}
								else
								{

									// The template has been found and converted back to a Microsoft Word template.
									mergeInfo.SourceDocument = Convert.FromBase64String(currentDebtClassRow.SettlementTemplate);

									// This will cause the loop to exit.
									isFound = true;

								}

							}
							finally
							{

								// The current Debt Class can be released.  At this point, every record that was locked to read the hiearchy has been
								// released and the loop can either exit (when a rule is found) or move on to the parent Debt Class.
								currentDebtClassRow.ReleaseReaderLock(dataModelTransaction.TransactionId);

							}

							// Now that all the locks are released, the parent Debt Class becomes the current one for the next level up in the hierarchy.
							// This algorithm will keep on climbing through the levels until a rule is found or the hierarchy is exhausted.
							currentDebtClassRow = nextDebtClassRow;

						} while (isFound == false && currentDebtClassRow != null);

					}

					// AccountBalance
					mergeInfo.Dictionary.Add(
						"AccountBalance", 
						String.Format("{0:$#,##0.00}", 
						consumerDebtSettlementRow.AccountBalance));

					// CreatedDate
					mergeInfo.Dictionary.Add("CreatedDate", consumerDebtSettlementRow.CreatedTime);

					// Original Creditor
					mergeInfo.Dictionary.Add(
						"DebtHolder",
						creditCardRow.IsDebtHolderNull() ? null : creditCardRow.DebtHolder);

					// DebtorAccountNumber
					mergeInfo.Dictionary.Add(
						"DebtorAccountNumber",
						consumerDebtSettlementRow.IsDebtorAccountNumberNull() ? null : consumerDebtSettlementRow.DebtorAccountNumber);

					// DebtorBankAccountNumber
					mergeInfo.Dictionary.Add(
						"DebtorBankAccountNumber",
						consumerDebtSettlementRow.IsDebtorBankAccountNumberNull() ? null : consumerDebtSettlementRow.DebtorBankAccountNumber);

					// DebtorBankRoutingNumber
					mergeInfo.Dictionary.Add(
						"DebtorBankRoutingNumber",
						consumerDebtSettlementRow.IsDebtorBankRoutingNumberNull() ? null : consumerDebtSettlementRow.DebtorBankRoutingNumber);

					// DebtorAddress1
					mergeInfo.Dictionary.Add(
						"DebtorAddress1",
						consumerDebtSettlementRow.IsDebtorAddress1Null() ? null : consumerDebtSettlementRow.DebtorAddress1);

					// DebtorAddress2
					mergeInfo.Dictionary.Add(
						"DebtorAddress2",
						consumerDebtSettlementRow.IsDebtorAddress2Null() ? null : consumerDebtSettlementRow.DebtorAddress2);

					// DebtorCity
					mergeInfo.Dictionary.Add(
						"DebtorCity",
						consumerDebtSettlementRow.IsDebtorCityNull() ? null : consumerDebtSettlementRow.DebtorCity);

					// DebtorFirstName
					mergeInfo.Dictionary.Add(
						"DebtorFirstName",
						consumerDebtSettlementRow.IsDebtorFirstNameNull() ? null : consumerDebtSettlementRow.DebtorFirstName);

					// DebtorLastName
					mergeInfo.Dictionary.Add(
						"DebtorLastName",
						consumerDebtSettlementRow.IsDebtorLastNameNull() ? null : consumerDebtSettlementRow.DebtorLastName);

					// DebtorMiddleName
					mergeInfo.Dictionary.Add(
						"DebtorMiddleName",
						consumerDebtSettlementRow.IsDebtorMiddleNameNull() ? null : consumerDebtSettlementRow.DebtorMiddleName);

					// DebtorOriginalAccountNumber
					mergeInfo.Dictionary.Add(
						"DebtorOriginalAccountNumber", 
						consumerDebtSettlementRow.DebtorOriginalAccountNumber);

					// DebtorPostalCode
					mergeInfo.Dictionary.Add(
						"DebtorPostalCode",
						consumerDebtSettlementRow.IsDebtorPostalCodeNull() ? null : consumerDebtSettlementRow.DebtorPostalCode);

					// DebtorProvince
					String debtorProvince = null;
					if (!consumerDebtSettlementRow.IsDebtorProvinceIdNull())
					{
						ProvinceRow provinceRow = consumerDebtSettlementRow.ProvinceRowByFK_Province_ConsumerDebtSettlement_DebtorProvinceId;
						try
						{
							provinceRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
							debtorProvince = provinceRow.Abbreviation;
						}
						finally
						{
							provinceRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
						}
					}
					mergeInfo.Dictionary.Add("DebtorProvinceAbbreviation", debtorProvince);

					// DebtorSalutation
					mergeInfo.Dictionary.Add(
						"DebtorSalutation",
						consumerDebtSettlementRow.IsDebtorSalutationNull() ? null : consumerDebtSettlementRow.DebtorSalutation);

					// DebtorSuffix
					mergeInfo.Dictionary.Add(
						"DebtorSuffix",
						consumerDebtSettlementRow.IsDebtorSuffixNull() ? null : consumerDebtSettlementRow.DebtorSuffix);

				
					// PayeeAddress1
					mergeInfo.Dictionary.Add(
						"PayeeAddress1",
						consumerDebtSettlementRow.IsPayeeAddress1Null() ? null : consumerDebtSettlementRow.PayeeAddress1);

					// PayeeAddress2
					mergeInfo.Dictionary.Add(
						"PayeeAddress2",
						consumerDebtSettlementRow.IsPayeeAddress2Null() ? null : consumerDebtSettlementRow.PayeeAddress2);

					// PayeeCity
					mergeInfo.Dictionary.Add(
						"PayeeCity", 
						consumerDebtSettlementRow.IsPayeeCityNull() ? null : consumerDebtSettlementRow.PayeeCity);

					// PayeeCompanyName
					mergeInfo.Dictionary.Add(
						"PayeeCompanyName",
						consumerDebtSettlementRow.IsPayeeCompanyNameNull() ? null : consumerDebtSettlementRow.PayeeCompanyName);

					// PayeeContactName
					mergeInfo.Dictionary.Add(
						"PayeeContactName",
						consumerDebtSettlementRow.IsPayeeContactNameNull() ? null : consumerDebtSettlementRow.PayeeContactName);

					// PayeeDepartment
					mergeInfo.Dictionary.Add(
						"PayeeDepartment",
						consumerDebtSettlementRow.IsPayeeDepartmentNull() ? null : consumerDebtSettlementRow.PayeeDepartment);

					// PayeeEmail
					mergeInfo.Dictionary.Add(
						"PayeeEmail",
						consumerDebtSettlementRow.IsPayeeEmailNull() ? null : consumerDebtSettlementRow.PayeeEmail);

					// PayeeFax
					mergeInfo.Dictionary.Add(
						"PayeeFax",
						consumerDebtSettlementRow.IsPayeeFaxNull() ? null : consumerDebtSettlementRow.PayeeFax);

					// PayeeForBenefitOf
					mergeInfo.Dictionary.Add(
						"PayeeForBenefitOf",
						consumerDebtSettlementRow.IsPayeeForBenefitOfNull() ? null : consumerDebtSettlementRow.PayeeForBenefitOf);

					// PayeePhone
					mergeInfo.Dictionary.Add(
						"PayeePhone",
						consumerDebtSettlementRow.IsPayeePhoneNull() ? null : consumerDebtSettlementRow.PayeePhone);

					// PayeePostalCode
					mergeInfo.Dictionary.Add(
						"PayeePostalCode",
						consumerDebtSettlementRow.IsPayeePostalCodeNull() ? null : consumerDebtSettlementRow.PayeePostalCode);

					// PayeeProvince
					String payeeProvince = null;
					if (!consumerDebtSettlementRow.IsPayeeProvinceIdNull())
					{
						ProvinceRow provinceRow = consumerDebtSettlementRow.ProvinceRowByFK_Province_ConsumerDebtSettlement_PayeeProvinceId;
						try
						{
							provinceRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
							payeeProvince = provinceRow.Abbreviation;
						}
						finally
						{
							provinceRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
						}
					}
					mergeInfo.Dictionary.Add("PayeeProvinceAbbreviation", payeeProvince);

					// PaymentLength
					mergeInfo.Dictionary.Add("PaymentLength", consumerDebtSettlementRow.PaymentLength);

					// PaymentStartDate
					mergeInfo.Dictionary.Add("PaymentStartDate", consumerDebtSettlementRow.PaymentStartDate.ToLocalTime().ToLongDateString());

					// PayeeBankAccountNumber
					mergeInfo.Dictionary.Add(
						"PayeeBankAccountNumber",
						consumerDebtSettlementRow.IsPayeeBankAccountNumberNull() ? null : consumerDebtSettlementRow.PayeeBankAccountNumber);

					// PayeeBankRoutingNumber
					mergeInfo.Dictionary.Add(
						"PayeeBankRoutingNumber",
						consumerDebtSettlementRow.IsPayeeBankRoutingNumberNull() ? null : consumerDebtSettlementRow.PayeeBankRoutingNumber);

					// SettlementAmount
					mergeInfo.Dictionary.Add("SettlementAmount", consumerDebtSettlementRow.SettlementAmount);

					// SettlementPercent
					mergeInfo.Dictionary.Add("SettlementPercent", consumerDebtSettlementRow.SettlementAmount / consumerDebtSettlementRow.AccountBalance);

					// TermPaymentAmount
					mergeInfo.Dictionary.Add("TermPaymentAmount", consumerDebtSettlementRow.SettlementAmount / consumerDebtSettlementRow.PaymentLength);

					// The payment methods is modeled as a vector which makes it difficult to add as a single merge field.  To work around this, each of the
					// possible payment methods are described in the data dictionary using the form 'Is{PaymentMethodName}'.  The Word Merge process should look for
					// the presence of these fields to generate a block of text for the instructions for each of the payment methods.  This iteration will
					// collect all the possible payment method types in an array and assume that they don't exist (i.e. set them to a Boolean value of 'false')
					// until they're found in the settlement instructions.
					foreach (PaymentMethodTypeRow paymentMethodTypeRow in DataModel.PaymentMethodType)
					{
						try
						{
							paymentMethodTypeRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
							mergeInfo.Dictionary.Add(String.Format("Is{0}", paymentMethodTypeRow.Name.Replace(" ", String.Empty)), false);
						}
						finally
						{
							paymentMethodTypeRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
						}
					}
					
					// This iteration will cycle through all the payment methods in the settlement and set them to be true.  The result is a dictionary of all
					// possible payment methods with the ones included in this settlement set to be the Boolean value of 'true'.
					foreach (ConsumerDebtSettlementPaymentMethodRow consumerDebtSettlementPaymentMethodRow in
						consumerDebtSettlementRow.GetConsumerDebtSettlementPaymentMethodRows())
					{

						try
						{

							// Each of the payment methods in the settlement are modeled as a list that is associated with the settlement.  This will lock each
							// of the items in the list in turn and examine the parent 'PaymentMethodType' record to construct a mail-merge tag.
							consumerDebtSettlementPaymentMethodRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
							PaymentMethodTypeRow paymentMethodTypeRow = DataModel.PaymentMethodType.PaymentMethodTypeKey.Find(
								consumerDebtSettlementPaymentMethodRow.PaymentMethodTypeId);

							try
							{

								// Once the parent MethodType is found a tag is added to the dictionary.  The presence of the 'Is<PaymentMethodType>' item in
								// the dictionary means that the given payment method is acceptable for this settlement.
								paymentMethodTypeRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
								mergeInfo.Dictionary[String.Format("Is{0}", paymentMethodTypeRow.Name.Replace(" ", String.Empty))] = true;

							}
							finally
							{

								// The parent payment method type row is not needed any longer.
								paymentMethodTypeRow.ReleaseReaderLock(dataModelTransaction.TransactionId);

							}

						}
						finally
						{

							// Release the payment method row.
							consumerDebtSettlementPaymentMethodRow.ReleaseReaderLock(dataModelTransaction.TransactionId);

						}

					}

				}
				finally
				{

					// The CreditCardRow is no longer needed.
					if (creditCardRow != null && creditCardRow.IsReaderLockHeld(dataModelTransaction.TransactionId))
						creditCardRow.ReleaseReaderLock(dataModelTransaction.TransactionId);

					// The ConsumerDebtSettlementRow is no longer needed.
					if (consumerDebtSettlementRow != null)
						consumerDebtSettlementRow.ReleaseReaderLock(dataModelTransaction.TransactionId);

					// The ConsumerDebtNegotiation Row is no longer needed.
					if (consumerDebtNegotiationRow != null)
						consumerDebtNegotiationRow.ReleaseReaderLock(dataModelTransaction.TransactionId);

					// The MatchRow is no longer needed.
					if (matchRow != null)
						matchRow.ReleaseReaderLock(dataModelTransaction.TransactionId);

					// The WorkingOrderRow is no longer needed.
					if (workingOrderRow != null)
						workingOrderRow.ReleaseReaderLock(dataModelTransaction.TransactionId);

					// The BlotterRow is no longer needed.
					if (blotterRow != null)
						blotterRow.ReleaseReaderLock(dataModelTransaction.TransactionId);


				}

				MemoryStream memoryStream = null;

				try
				{
					// At this point, all the data has been collected and the record locks released.  It is time to merge the document.
					memoryStream = SettlementDocumentFactory.iMailMerge.CreateDocument(mergeInfo.SourceDocument, mergeInfo.Dictionary);
				}
				catch (Exception exception)
				{
					EventLog.Error("There was a problem creating the settlement letter. \n Details: {0}, {1}", exception.Message, exception.StackTrace);
				}

				if (memoryStream != null)
				{
					// Update the settlement with the newly generated PFD file.
					dataModel.UpdateConsumerDebtSettlement(
						null,
						null,
						null,
						null,
						new Object[] { consumerDebtSettlementId },
						null,
						null,
						null,
						null,
						null,
						null,
						null,
						null,
						null,
						null,
						null,
						null,
						null,
						null,
						null,
						null,
						null,
						null,
						null,
						null,
						null,
						null,
						null,
						null,
						null,
						null,
						null,
						null,
						null,
						null,
						null,
						null,
						null,
						null,
						null,
						mergeInfo.RowVersion,
						null,
						Convert.ToBase64String(memoryStream.ToArray()),
						mergeInfo.StatusId);
				}

				// If we reached here the transaction was successful.
				transactionScope.Complete();

			}

		}

		/// <summary>
		/// Handles a change to a record in the Consumer table.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="consumerRowChangeEventArgs">The event data.</param>
		static void OnConsumerDebtSettlementRowChanged(Object sender, ConsumerDebtSettlementRowChangeEventArgs consumerDebtSettlementRowChangeEventArgs)
		{

			// When a working order is committed it will be examined to see if any of the properties that control crossing have changed.  A change in any of
			// these parameters indicates that the order should be re-examined for possible matches.
			if (consumerDebtSettlementRowChangeEventArgs.Action == DataRowAction.Commit &&
				consumerDebtSettlementRowChangeEventArgs.Row.RowState != DataRowState.Deleted)
			{

				// Extract the unique working order identifier from the generic event arguments.  The identifier is needed for the handler that creates crosses
				// when the right conditions occur.
				ConsumerDebtSettlementRow consumerDebtSettlementRow = consumerDebtSettlementRowChangeEventArgs.Row;
				Guid consumerDebtSettlementId = (Guid)consumerDebtSettlementRow[DataModel.ConsumerDebtSettlement.ConsumerDebtSettlementIdColumn];
				SettlementDocumentFactory.actionQueue.Enqueue(new ObjectAction(MergeDocument, new Object[] { consumerDebtSettlementId }));

			}

		}

	}

}
