namespace FluidTrade.Guardian
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.ServiceModel;
	using FluidTrade.Core;
	using FluidTrade.Guardian.Records;

	/// <summary>
	/// Methods to assist in handling actions related to Consumer Debt Negotiations.
	/// </summary>
	internal class ConsumerDebtNegotiationHelper
	{

		/// <summary>
		/// Information about an existing payment method associated with a negotiation.
		/// </summary>
		private class ConsumerDebtNegotiationPaymentMethodTypeInfo
		{

			/// <summary>
			/// The unique identifier for the payment method.
			/// </summary>
			public Guid PaymentMethodInfoId;

			/// <summary>
			/// The unique identifier of the payment method within the scope of a negotiation.
			/// </summary>
			public Guid ConsumerDebtNegotiationOfferPaymentMethodId;

			/// <summary>
			/// The RowVersion of the record.
			/// </summary>
			public Int64 RowVersion;

			/// <summary>
			/// Create a structure that contains information about the payment types available in a negotiation.
			/// </summary>
			/// <param name="paymentMethodId">The payment method identifier.</param>
			/// <param name="consumerDebtNegotiationOfferPaymentId">The unique identifier of the payment method within the negotiation.</param>
			/// <param name="rowVersion">The Row Version of the record.</param>
			public ConsumerDebtNegotiationPaymentMethodTypeInfo(Guid paymentMethodId, Guid consumerDebtNegotiationOfferPaymentId, Int64 rowVersion)
			{

				// Initialize the object.
				this.PaymentMethodInfoId = paymentMethodId;
				this.ConsumerDebtNegotiationOfferPaymentMethodId = consumerDebtNegotiationOfferPaymentId;
				this.RowVersion = rowVersion;

			}

		}

		/// <summary>
		/// Reset the negotiation to a "in negotiation" state.
		/// </summary>
		/// <param name="consumerDebtNegotiations">The the negotiations to reset.</param>
		internal static void Reject(ConsumerDebtNegotiationInfo[] consumerDebtNegotiations)
		{
			// An instance of the shared data model is required to use its methods.
			DataModel dataModel = new DataModel();

			// The business logic requires the current time and the user identifier for auditing.
			Guid createUserId = TradingSupport.UserId;
			DateTime createDateTime = DateTime.UtcNow;
			DateTime modifiedTime = createDateTime;
			Guid modifiedUserId = createUserId;


			// This Web Method comes with an implicit transaction that is linked to its execution.
			DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

			// This method can handle a batch of updates in a single transaction.
			foreach (ConsumerDebtNegotiationInfo consumerDebtNegotiationInfo in consumerDebtNegotiations)
			{

				// The blotter is not passed in from the client but is used  
				Guid blotterId = Guid.Empty;
				Status negotiationStatus;
				DebtNegotiationInfo debtNegotiationInfo = null;
				// This is the next negotiation in the batch to be updated.				
				ConsumerDebtNegotiationRow consumerDebtNegotiationRow =
					DataModel.ConsumerDebtNegotiation.ConsumerDebtNegotiationKey.Find(consumerDebtNegotiationInfo.ConsumerDebtNegotiationId);

				// The payment methods available to this negotiation is a vector.  Rather than delete everything and re-add it anytime an update is made, a
				// list of changes is constructed: new payment methods are added, obsolete payment methods are deleted and the ones that haven't changed are
				// left alone.  These list help to work out the differences.
				List<ConsumerDebtNegotiationPaymentMethodTypeInfo> counterItems = new List<ConsumerDebtNegotiationPaymentMethodTypeInfo>();

				try
				{

					// Lock the current negotation record for reading.  The data model doesn't support reader lock promotion, so the programming model is to
					// lock the database, collect the data, release the locks and then write.  This model is especially important when iterating through a
					// large batch to prevent the number of locks from growing to large.
					consumerDebtNegotiationRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

					// The blotter identifier is used for access control and is not passed in by the client.
					blotterId = consumerDebtNegotiationRow.BlotterId;
					debtNegotiationInfo = new DebtNegotiationInfo(consumerDebtNegotiationRow);
					negotiationStatus = StatusMap.FromId(consumerDebtNegotiationRow.StatusId);

					// Determine whether the client has the right to modify this record.
					if (!TradingSupport.HasAccess(dataModelTransaction, blotterId, AccessRight.Write))
						throw new FaultException<FluidTrade.Core.SecurityFault>(new SecurityFault("You do not have write access to the selected object."));

					MatchRow matchRow = DataModel.Match.MatchKey.Find(debtNegotiationInfo.MatchId);
					try
					{
						matchRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
						debtNegotiationInfo.MatchRowVersion = matchRow.RowVersion;
						debtNegotiationInfo.ContraMatchId = matchRow.ContraMatchId;
					}
					finally
					{
						matchRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
					}

					MatchRow contraMatchRow = DataModel.Match.MatchKey.Find(debtNegotiationInfo.ContraMatchId);
					try
					{
						contraMatchRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
						debtNegotiationInfo.ContraMatchRowVersion = contraMatchRow.RowVersion;
					}
					finally
					{
						contraMatchRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
					}

					// The payment methods are maintained as a vector associated with the negotiation record.  This will lock each of the records and read the 
					// payment methods into a data structure so the locks don't need to be held when it is time to write.
					foreach (var consumerDebtNegotiationOfferPaymentMethodRow
						in consumerDebtNegotiationRow.GetConsumerDebtNegotiationCounterPaymentMethodRows())
					{

						try
						{

							// Temporarily lock the record containing the payment method.
							consumerDebtNegotiationOfferPaymentMethodRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

							// This will construct a mask of items that are already part of this negotiation.  The mask is used to prevent an item from being
							// added if it's already there.
							// This list is used to delete the payment methods that are no longer part of this negotiation.
							counterItems.Add(
								new ConsumerDebtNegotiationPaymentMethodTypeInfo(
									consumerDebtNegotiationOfferPaymentMethodRow.PaymentMethodTypeId,
									consumerDebtNegotiationOfferPaymentMethodRow.ConsumerDebtNegotiationCounterPaymentMethodId,
									consumerDebtNegotiationOfferPaymentMethodRow.RowVersion));
						}
						finally
						{

							// At this point the payment method isn't needed.
							consumerDebtNegotiationOfferPaymentMethodRow.ReleaseReaderLock(dataModelTransaction.TransactionId);

						}

					}

				}
				finally
				{

					// At this point, the negotiation record isn't needed.  It is critical to release the reader locks before attempting a write.
					consumerDebtNegotiationRow.ReleaseReaderLock(dataModelTransaction.TransactionId);

				}


				// At this point, all the data for this operation has been collected and the CRUD operations can be invoked to finish the update.  Note that
				// the counter party information is not modified here, but is done through the Chinese wall.
				Guid newConsureDebtNegotiationId = Guid.NewGuid();
				dataModel.CreateConsumerDebtNegotiation(
				debtNegotiationInfo.AccountBalance,
				debtNegotiationInfo.BlotterId,
				newConsureDebtNegotiationId,
				debtNegotiationInfo.CounterPaymentLength,
				debtNegotiationInfo.CounterPaymentStartDateLength,
				debtNegotiationInfo.CounterPaymentStartDateUnitId,
				debtNegotiationInfo.CounterSettlementUnitId,
				debtNegotiationInfo.CounterSettlementValue,
				modifiedTime,
				modifiedUserId,
				debtNegotiationInfo.IsRead,
				debtNegotiationInfo.IsReply,
				debtNegotiationInfo.MatchId,
				modifiedTime,
				modifiedUserId,
				consumerDebtNegotiationInfo.PaymentLength,
				consumerDebtNegotiationInfo.PaymentStartDateLength,
				consumerDebtNegotiationInfo.PaymentStartDateUnitId,
				consumerDebtNegotiationInfo.SettlementUnitId,
				consumerDebtNegotiationInfo.SettlementValue,
				StatusMap.FromCode(Status.Rejected),
				out debtNegotiationInfo.Version);

				// This will add the payment methods to the negotiation that are not already there.
				foreach (Guid paymentMethodTypeId in consumerDebtNegotiationInfo.PaymentMethodTypes)
				{
					dataModel.CreateConsumerDebtNegotiationOfferPaymentMethod(
						blotterId,
						newConsureDebtNegotiationId,
						Guid.NewGuid(),
						paymentMethodTypeId);

				}

				//This will delete those payment methods that are no longer part of the negotiation.
				foreach (ConsumerDebtNegotiationPaymentMethodTypeInfo consumerDebtNegotiationPaymentMethodTypeInfo in counterItems)
				{
					dataModel.UpdateConsumerDebtNegotiationCounterPaymentMethod(blotterId,
						null,
						new Object[] { consumerDebtNegotiationPaymentMethodTypeInfo.ConsumerDebtNegotiationOfferPaymentMethodId },
						newConsureDebtNegotiationId,
						null,
						consumerDebtNegotiationPaymentMethodTypeInfo.RowVersion);

				}

				//Reset the Match Status Id.  This is required so the match engine will redo the match.  The
				//match engine does not recalculate if it is not in the initial three stages of - Valid, Partial, ValidwithFunds
				dataModel.UpdateMatch(
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					new object[] { debtNegotiationInfo.MatchId },
					debtNegotiationInfo.MatchRowVersion,
					StatusMap.FromCode(Status.ValidMatch),
					null);

				//Reset the Contra Match Status Id
				dataModel.UpdateMatch(
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					new object[] { debtNegotiationInfo.ContraMatchId },
					debtNegotiationInfo.ContraMatchRowVersion,
					StatusMap.FromCode(Status.ValidMatch),
					null);
			}

		}


		/// <summary>
		/// Updates a collection of Debt Negotiation settlement records.
		/// </summary>
		internal static void Update(ConsumerDebtNegotiationInfo[] consumerDebtNegotiations)
		{

			// An instance of the shared data model is required to use its methods.
			DataModel dataModel = new DataModel();

			// The business logic requires the current time and the user identifier for auditing.
			Guid createUserId = TradingSupport.UserId;
			DateTime createDateTime = DateTime.UtcNow;
			DateTime modifiedTime = createDateTime;
			Guid modifiedUserId = createUserId;

			// This Web Method comes with an implicit transaction that is linked to its execution.
			DataModelTransaction dataModelTransaction = DataModelTransaction.Current;
			DebtNegotiationInfo debtNegotiationInfo = null;
			Guid blotterId = Guid.Empty;

			// This method can handle a batch of updates in a single transaction.
			foreach (ConsumerDebtNegotiationInfo consumerDebtNegotiationInfo in consumerDebtNegotiations)
			{

				// The payment methods available to this negotiation is a vector.  Rather than delete everything and re-add it anytime an update is made, a
				// list of changes is constructed: new payment methods are added, obsolete payment methods are deleted and the ones that haven't changed are
				// left alone.  These list help to work out the differences.
				List<Guid> existingItems = new List<Guid>();
				List<ConsumerDebtNegotiationPaymentMethodTypeInfo> counterItems = new List<ConsumerDebtNegotiationPaymentMethodTypeInfo>();

				// This is the next negotiation in the batch to be updated.
				ConsumerDebtNegotiationRow consumerDebtNegotiationRow =
					DataModel.ConsumerDebtNegotiation.ConsumerDebtNegotiationKey.Find(consumerDebtNegotiationInfo.ConsumerDebtNegotiationId);


				Guid matchId = Guid.Empty;	
				Int64 originalVersion = Int64.MinValue;
				// Lock the current negotation record for reading.  The data model doesn't support reader lock promotion, so the programming model is to
				// lock the database, collect the data, release the locks and then write.  This model is especially important when iterating through a
				// large batch to prevent the number of locks from growing to large.								
				consumerDebtNegotiationRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);				
				try
				{					
					matchId = consumerDebtNegotiationRow.MatchId;
					originalVersion = consumerDebtNegotiationRow.Version;
				}
				finally				
				{
					consumerDebtNegotiationRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
					consumerDebtNegotiationRow = null;
				}

				//Determine the most recent Negotiation to grab the counter payment methods.
				Int64 maxVersion = Int64.MinValue;				
				MatchRow matchRow = DataModel.Match.MatchKey.Find(matchId);
				matchRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
				try
				{						
					foreach (ConsumerDebtNegotiationRow versionRow in matchRow.GetConsumerDebtNegotiationRows())
					{
						try
						{
							versionRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
							if (versionRow.Version > maxVersion)
							{
								maxVersion = versionRow.Version;
								consumerDebtNegotiationRow = versionRow;
							}
						}
						finally
						{
							versionRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
						}
					}
				}
				finally
				{
					matchRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				}

				// Lock the current negotation record for reading.  The data model doesn't support reader lock promotion, so the programming model is to
				// lock the database, collect the data, release the locks and then write.  This model is especially important when iterating through a
				// large batch to prevent the number of locks from growing to large.
				consumerDebtNegotiationRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
				
				try
				{

					//Check for rowversion
					if (consumerDebtNegotiationRow.Version != originalVersion)
						throw new global::System.ServiceModel.FaultException<FluidTrade.Core.OptimisticConcurrencyFault>(
							new global::FluidTrade.Core.OptimisticConcurrencyFault("ConsumerTrustNegotiation",
								new object[] { consumerDebtNegotiationInfo.ConsumerDebtNegotiationId }),
								new FaultReason("Negotiation is busy.  Please try again!"));
 


					// The blotter identifier is used for access control and is not passed in by the client.
					blotterId = consumerDebtNegotiationRow.BlotterId;
					debtNegotiationInfo = new DebtNegotiationInfo(consumerDebtNegotiationRow);

					// Determine whether the client has the right to modify this record.
					if (!TradingSupport.HasAccess(dataModelTransaction, blotterId, AccessRight.Write))
						throw new FaultException<FluidTrade.Core.SecurityFault>(new SecurityFault("You do not have write access to the selected object."));

					// The payment methods are maintained as a vector associated with the negotiation record.  This will lock each of the records and read the 
					// payment methods into a data structure so the locks don't need to be held when it is time to write.
					foreach (var consumerDebtNegotiationOfferPaymentMethodRow
						in consumerDebtNegotiationRow.GetConsumerDebtNegotiationCounterPaymentMethodRows())
					{

						try
						{

							// Temporarily lock the record containing the payment method.
							consumerDebtNegotiationOfferPaymentMethodRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

							// This will construct a mask of items that are already part of this negotiation.  The mask is used to prevent an item from being
							// added if it's already there.
							// This list is used to delete the payment methods that are no longer part of this negotiation.
							counterItems.Add(
								new ConsumerDebtNegotiationPaymentMethodTypeInfo(
									consumerDebtNegotiationOfferPaymentMethodRow.PaymentMethodTypeId,
									consumerDebtNegotiationOfferPaymentMethodRow.ConsumerDebtNegotiationCounterPaymentMethodId,
									consumerDebtNegotiationOfferPaymentMethodRow.RowVersion));
						}
						finally
						{

							// At this point the payment method isn't needed.
							consumerDebtNegotiationOfferPaymentMethodRow.ReleaseReaderLock(dataModelTransaction.TransactionId);

						}

					}

				}
				finally
				{

					// At this point, the negotiation record isn't needed.  It is critical to release the reader locks before attempting a write.
					consumerDebtNegotiationRow.ReleaseReaderLock(dataModelTransaction.TransactionId);

				}

				// At this point, all the data for this operation has been collected and the CRUD operations can be invoked to finish the update.  Note that
				// the counter party information is not modified here, but is done through the Chinese wall.
				Guid newConsureDebtNegotiationId = Guid.NewGuid();
				dataModel.CreateConsumerDebtNegotiation(
				debtNegotiationInfo.AccountBalance,
				debtNegotiationInfo.BlotterId,
				newConsureDebtNegotiationId,
				debtNegotiationInfo.CounterPaymentLength,
				debtNegotiationInfo.CounterPaymentStartDateLength,
				debtNegotiationInfo.CounterPaymentStartDateUnitId,
				debtNegotiationInfo.CounterSettlementUnitId,
				debtNegotiationInfo.CounterSettlementValue,
				modifiedTime,
				modifiedUserId,
				debtNegotiationInfo.IsRead,
				debtNegotiationInfo.IsReply,
				debtNegotiationInfo.MatchId,
				modifiedTime,
				modifiedUserId,
				consumerDebtNegotiationInfo.PaymentLength,
				consumerDebtNegotiationInfo.PaymentStartDateLength,
				consumerDebtNegotiationInfo.PaymentStartDateUnitId,
				consumerDebtNegotiationInfo.SettlementUnitId,
				consumerDebtNegotiationInfo.SettlementValue,
				consumerDebtNegotiationInfo.StatusId,
				out debtNegotiationInfo.Version);

				// This will add the payment methods to the negotiation that are not already there.
				foreach (Guid paymentMethodTypeId in consumerDebtNegotiationInfo.PaymentMethodTypes)
				{
					dataModel.CreateConsumerDebtNegotiationOfferPaymentMethod(
						blotterId,
						newConsureDebtNegotiationId,
						Guid.NewGuid(),
						paymentMethodTypeId);

				}

				//This will delete those payment methods that are no longer part of the negotiation.
				foreach (ConsumerDebtNegotiationPaymentMethodTypeInfo consumerDebtNegotiationPaymentMethodTypeInfo in counterItems)
				{
					dataModel.UpdateConsumerDebtNegotiationCounterPaymentMethod(blotterId,
						null,
						new Object[] { consumerDebtNegotiationPaymentMethodTypeInfo.ConsumerDebtNegotiationOfferPaymentMethodId },
						newConsureDebtNegotiationId,
						null,
						consumerDebtNegotiationPaymentMethodTypeInfo.RowVersion);

				}

			}

		}

		/// <summary>
		/// Updates the state of a collection of Debt Negotiation settlement records.
		/// </summary>
		internal static void Update(ConsumerDebtNegotiationIsReadInfo[] consumerDebtNegotiationIsReads)
		{

			// An instance of the shared data model is required to use its methods.
			DataModel dataModel = new DataModel();

			// This Web Method comes with an implicit transaction that is linked to its execution.
			DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

			// This method can handle a batch of updates in a single transaction.
			foreach (ConsumerDebtNegotiationIsReadInfo consumerDebtNegotiationIsReadInfo in consumerDebtNegotiationIsReads)
			{

				try
				{

					if (consumerDebtNegotiationIsReadInfo != null &&
						consumerDebtNegotiationIsReadInfo.ConsumerDebtNegotiationId != Guid.Empty)
					{

						// The blotter is not passed in from the client but is used to validate the users access to this record.
						Guid blotterId = Guid.Empty;
						Int64 version;
						Int64 rowVersion;

						// This is the next negotiation in the batch to be updated.
						ConsumerDebtNegotiationRow consumerDebtNegotiationRow =
							DataModel.ConsumerDebtNegotiation.ConsumerDebtNegotiationKey.Find(consumerDebtNegotiationIsReadInfo.ConsumerDebtNegotiationId);


						try
						{

							// Lock the current negotiation record for reading.  The data model doesn't support reader lock promotion, so the programming model is to
							// lock the database, collect the data, release the locks and then write.  This model is especially important when iterating through a
							// large batch to prevent the number of locks from growing to large.
							consumerDebtNegotiationRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
							blotterId = consumerDebtNegotiationRow.BlotterId;
							version = consumerDebtNegotiationRow.Version;

							//Use the current rowversion to update.  This is to avoid an concurrency error. The client may not have
							//gotten the updated row if they made a change before the IsRead fired off.  Since this is a benign change
							//we can safely update the row and let the change trickle back on update.
							rowVersion = consumerDebtNegotiationRow.RowVersion;

							if (rowVersion != consumerDebtNegotiationRow.RowVersion)
							{
								EventLog.Warning("ConsumerTrustNegotiaion ISRead incompatible rowVersions. Expected {0}, Got {1}. Using {0}",
									consumerDebtNegotiationRow.RowVersion, rowVersion );
							}

							// Determine whether the client has the right to modify this record.
							if (!TradingSupport.HasAccess(dataModelTransaction, blotterId, AccessRight.Read))
								throw new FaultException<FluidTrade.Core.SecurityFault>(new SecurityFault("You do not have read access to the selected object."));

						}

						finally
						{

							// At this point, the negotiation record isn't needed.  It is critical to release the reader locks before attempting a write.
							if (consumerDebtNegotiationRow != null)
								consumerDebtNegotiationRow.ReleaseReaderLock(dataModelTransaction.TransactionId);

						}

						// At this point, all the data for this operation has been collected and the CRUD operations can be invoked to finish the update.  Note that
						// the counter party information is not modified here, but is done through the Chinese wall.
						dataModel.UpdateConsumerDebtNegotiation(
							null,
							null,
							null,
							new object[] { consumerDebtNegotiationIsReadInfo.ConsumerDebtNegotiationId },
							null,
							null,
							null,
							null,
							null,
							null,
							null,
							consumerDebtNegotiationIsReadInfo.IsRead,
							false,
							null,
							null,
							null,
							null,
							null,
							null,
							null,
							null,
							rowVersion,
							null,
							version);
					}
				}
				catch (FaultException<FluidTrade.Core.SecurityFault> faultException)
				{
					throw faultException;
				}
				catch (Exception ex)
				{

					//We will log the exception and try to carry on.
					EventLog.Error(ex);
				}

			}
		}


		/// <summary>
		/// Information about a Consumer Debt Settlement
		/// </summary>
		private class DebtNegotiationInfo
		{
			public Decimal AccountBalance { get; set; }
			public Guid BlotterId { get; set; }
			public Guid MatchId { get; set; }
			public Guid ContraMatchId { get; set; }
			public Int64 ContraMatchRowVersion { get; set; }
			public Int64 MatchRowVersion { get; set; }
			public Decimal CounterPaymentLength { get; set; }
			public Decimal CounterPaymentStartDateLength { get; set; }
			public Guid CounterPaymentStartDateUnitId { get; set; }
			public Guid CounterSettlementUnitId { get; set; }
			public Decimal CounterSettlementValue { get; set; }
			public Boolean IsRead { get; set; }
			public Boolean IsReply { get; set; }
			public Int64 Version;


			public DebtNegotiationInfo(ConsumerDebtNegotiationRow consumerDebtNegotiationRow)
			{
				this.AccountBalance = consumerDebtNegotiationRow.AccountBalance;
				this.BlotterId = consumerDebtNegotiationRow.BlotterId;
				this.MatchId = consumerDebtNegotiationRow.MatchId;
				this.CounterPaymentLength = consumerDebtNegotiationRow.CounterPaymentLength;
				this.CounterPaymentStartDateLength = consumerDebtNegotiationRow.CounterPaymentStartDateLength;
				this.CounterPaymentStartDateUnitId = consumerDebtNegotiationRow.CounterPaymentStartDateUnitId;
				this.CounterSettlementUnitId = consumerDebtNegotiationRow.CounterSettlementUnitId;
				this.CounterSettlementValue = consumerDebtNegotiationRow.CounterSettlementValue;
				this.IsRead = consumerDebtNegotiationRow.IsRead;
				this.IsReply = consumerDebtNegotiationRow.IsReply;
			}
		}

	}

}
