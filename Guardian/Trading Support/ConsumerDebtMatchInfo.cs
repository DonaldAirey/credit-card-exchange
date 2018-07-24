namespace FluidTrade.Guardian
{

	using FluidTrade.Core;
	using System;
	using System.Configuration;
	using System.Collections.Generic;
	using System.Data;
	using System.Data.SqlClient;
	using System.IdentityModel.Policy;
	using System.IdentityModel.Claims;
	using System.Reflection;
	using System.Security.Principal;
	using System.Threading;
	using System.Transactions;

	internal class ConsumerDebtMatchInfo
	{
		public Decimal AccountBalance;
		public Guid BlotterId;
		public Decimal PaymentLength;
		public List<Guid> PaymentMethodTypes;
		public Decimal PaymentStartDateLength;
		public Guid PaymentStartDateUnitId;
		public Decimal SettlementValue;
		public Guid SettlementUnitId;
		public Guid WorkingOrderId;
		public Int64 Version;


		public ConsumerDebtMatchInfo(DataModelTransaction dataModelTransaction, Guid workingOrderId)
			: this(dataModelTransaction, DataModel.WorkingOrder.WorkingOrderKey.Find(workingOrderId))
		{
		}

		public ConsumerDebtMatchInfo(DataModelTransaction dataModelTransaction, WorkingOrderRow workingOrderRow)
		{
			// Initialize the object
			this.PaymentMethodTypes = new List<Guid>();

			// These rows are required for navigating through the asset.  Locks are acquired temporarily for them and released as soon as the Consumer Debt
			// information is collected.
			BlotterRow blotterRow = null;
			SecurityRow securityRow = null;
			ConsumerDebtRow consumerDebtRow = null;
			CreditCardRow creditCardRow = null;

			// The working order row is where everything starts. This is the asset that is to be matched against another.
			workingOrderRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
			try
			{
				// The working order identifier is used when creating matches.
				this.WorkingOrderId = workingOrderRow.WorkingOrderId;

				// The blotter row needs to be examined for this cross.
				blotterRow = workingOrderRow.BlotterRow;

				// The underlying security is a Consumer Debt record.
				securityRow = workingOrderRow.SecurityRowByFK_Security_WorkingOrder_SecurityId;
			}
			finally
			{
				workingOrderRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
			}

			blotterRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
			try
			{
				// The blotter is required for adding matches to the data model once they are found.
				this.BlotterId = blotterRow.BlotterId;
			}
			finally
			{
				blotterRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
			}

			securityRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
			try
			{
				consumerDebtRow = securityRow.GetConsumerDebtRows()[0] as ConsumerDebtRow;
			}
			finally
			{
				securityRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
			}

			// This row contains the actual asset that is to be matched.
			DebtRuleRow debtRuleRow = null;
			consumerDebtRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
			try
			{
				creditCardRow = consumerDebtRow.CreditCardRow;
				if(consumerDebtRow.IsDebtRuleIdNull() == false)
					debtRuleRow = consumerDebtRow.DebtRuleRow;
			}
			finally
			{
				consumerDebtRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
			}
			// The credit card record has the actual balance on the card.
			creditCardRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
			try
			{
				// Extract values that are used to determine whether the contra account has the funds required to settle this account.
				this.AccountBalance = creditCardRow.AccountBalance;
			}
			finally
			{
				creditCardRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
			}

			// This will attempt to find the debt rule associated with this credit card.  If there is no rule explicitly associated with the asset, 
			// then the entity hierarchy is searched until a debt class is found that contains a rule.  When the rule is found, the values in that rule
			// will become the opening bid.
			if(debtRuleRow == null)
			{
				DebtClassRow[] blotterRowDebtClassRows;
				blotterRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
				try
				{
					blotterRowDebtClassRows = blotterRow.GetDebtClassRows();
				}
				finally
				{
					blotterRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				}
				// At this point, the asset hasn't got an explicit rule, so the hierarchy will need to be searched.  There is only going to be one Debt
				// Class element associated with this blotter, but the iteration is an easier construct to work with than the equivalent array logic
				// for a single element.
				foreach(DebtClassRow debtClassRow in blotterRowDebtClassRows)
				{

					// This variable will keep track of our current location as we crawl up the hierarchy.
					DebtClassRow currentDebtClassRow = debtClassRow;
					DebtClassRow nextDebtClassRow = null;

					// This will crawl up the hierarchy until a Debt Class is found with a rule.  This rule will provide the opening values for the
					// bid on this negotiation.
					do
					{
						Guid currentDebtClassRowDebtClassId;
						// This will lock the current item in the hierarchy so it can be examined for a rule or, failing that, a parent element.
						currentDebtClassRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
						try
						{
							currentDebtClassRowDebtClassId = currentDebtClassRow.DebtClassId;
							if(currentDebtClassRow.IsDebtRuleIdNull() == false)
							{
								// At this point we have finally found a debt rule that can be used to start the negotiations.
								debtRuleRow = currentDebtClassRow.DebtRuleRow;
							}
						}
						finally
						{

							// The current Debt Class can be released.  At this point, every record that was locked to read the hiearchy has been
							// released and the loop can either exit (when a rule is found) or move on to the parent Debt Class.
							currentDebtClassRow.ReleaseReaderLock(dataModelTransaction.TransactionId);

						}

						// If the current Debt Class has no rule then the Entity Hierarchy is used to find the parent element.
						if(debtRuleRow == null)
						{

							// The entity is the root of all objects in the hierarchy.  From this object the path to the parent Debt Class can be 
							// navigated.
							EntityTreeRow[] treeRowAr;
							EntityRow entityRow = DataModel.Entity.EntityKey.Find(currentDebtClassRowDebtClassId);
							// Each entity needs to be locked before the relation can be used.
							entityRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
							try
							{
								treeRowAr = entityRow.GetEntityTreeRowsByFK_Entity_EntityTree_ChildId();
							}
							finally
							{

								// Finaly, the current entity is released.  This allows us to finally move on to the next level of the hierarchy 
								// without having to hold the locks for the entire transaction.
								entityRow.ReleaseReaderLock(dataModelTransaction.TransactionId);

							}

							// This will find each relation in the hierarchy which uses the current node as a child.
							foreach(EntityTreeRow entityTreeRow in treeRowAr)
							{
								EntityRow parentEntityRow;
								// Lock the relation down before navigating to the parent.
								entityTreeRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
								try
								{
									// This is the parent entity of the current entity in the crawl up the hierarchy.
									parentEntityRow = entityTreeRow.EntityRowByFK_Entity_EntityTree_ParentId;

								}
								finally
								{

									// The relationship record is released after each level of the hierarchy is examined for a parent.
									entityTreeRow.ReleaseReaderLock(dataModelTransaction.TransactionId);

								}


								BlotterRow[] parentEntityRowBlotterRows;
								// The parent entity must be locked befor it can be checked for blotters and then, in turn, debt 
								// classes.
								parentEntityRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

								try
								{
									parentEntityRowBlotterRows = parentEntityRow.GetBlotterRows();
								}
								finally
								{
									// The parent Entity record is released after each level of the hiearchy is examined for a parent.
									parentEntityRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
								}


								// In practice, there will be zero or one blotter rows.  The iteration makes it easier to check both 
								// conditions.
								foreach(BlotterRow parentBlotterRow in parentEntityRowBlotterRows)
								{

									DebtClassRow[] parentBlotterRowDebtClassRows;
									// The blotter must be locked before iterating through the Debt Classes that may be associated 
									// with the blotter.
									parentBlotterRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

									try
									{
										parentBlotterRowDebtClassRows = parentBlotterRow.GetDebtClassRows();
									}
									finally
									{

										// The locks are released after the each level of the hierarchy is checked.
										parentBlotterRow.ReleaseReaderLock(dataModelTransaction.TransactionId);

									}



									// Each blotter can have zero or one Debt Classes associated with it.  This is a long an 
									// tortuous way to finally get to the parent Debt Class.
									foreach(DebtClassRow parentDebtClassRow in parentBlotterRowDebtClassRows)
									{

										// Now that we've finally found the parent Debt Class, it will become the parent on 
										// the next pass through the hierarchy.  Note that the locks are released each time we
										// pass through a level of the hierarchy.
										nextDebtClassRow = parentDebtClassRow;


									}

								}



							}
						}

						// Now that all the locks are released, the parent Debt Class becomes the current one for the next level up in the hierarchy.
						// This algorithm will keep on climbing through the levels until a rule is found or the hierarchy is exhausted.
						currentDebtClassRow = nextDebtClassRow;

					} while(debtRuleRow == null && currentDebtClassRow != null);

				}
			}

			// The data is copied out of the Debt Rule when one is found in the Entity hierarchy.
			if(debtRuleRow != null)
			{

				DebtRulePaymentMethodRow[] debtRuleRowDebtRulePaymentMethodRows;
				// At this point we found a rule that can be used for the opening bid of a settlement.
				debtRuleRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

				try
				{
					// These values are used in the opening bid of a negotiated settlement.
					this.PaymentLength = debtRuleRow.PaymentLength;
					this.PaymentStartDateLength = debtRuleRow.PaymentStartDateLength;
					this.PaymentStartDateUnitId = debtRuleRow.PaymentStartDateUnitId;
					this.SettlementValue = debtRuleRow.SettlementValue;
					this.SettlementUnitId = debtRuleRow.SettlementUnitId;

					debtRuleRowDebtRulePaymentMethodRows = debtRuleRow.GetDebtRulePaymentMethodRows();
				}
				finally
				{
					// These locks are held temporarly while the data is collected.
					debtRuleRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				}


				// This will copy each of the payment methods that are available for this settlement.  Note that the table rows are locked only long 
				// enough to acquire the item, then released.
				foreach(DebtRulePaymentMethodRow debtRulePaymentMethodRow in debtRuleRowDebtRulePaymentMethodRows)
				{

					debtRulePaymentMethodRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
					try
					{
						this.PaymentMethodTypes.Add(debtRulePaymentMethodRow.PaymentMethodTypeId);
					}
					finally
					{
						debtRulePaymentMethodRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
					}

				}
			}

		}
	}

}
