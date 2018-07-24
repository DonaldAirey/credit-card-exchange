namespace FluidTrade.Guardian
{

	using System;
	using System.Linq;
	using System.Collections.Generic;
	using System.ServiceModel;
	using FluidTrade.Core;
	using FluidTrade.Guardian.Records;


	/// <summary>
	/// Methods to assist in handling actions related to Consumer Trust Negotiations.
	/// </summary>
	internal class ConsumerTrustNegotiationHelper
	{

		/// <summary>
		/// Information about an existing payment method associated with a negotiation.
		/// </summary>
		private class ConsumerTrustNegotiationPaymentMethodTypeInfo
		{

			/// <summary>
			/// The unique identifier for the payment method.
			/// </summary>
			public Guid PaymentMethodInfoId;

			/// <summary>
			/// The unique identifier of the payment method within the scope of a negotiation.
			/// </summary>
			public Guid ConsumerTrustNegotiationOfferPaymentMethodId;

			/// <summary>
			/// The RowVersion of the record.
			/// </summary>
			public Int64 RowVersion;

			/// <summary>
			/// Create a structure that contains information about the payment types available in a negotiation.
			/// </summary>
			/// <param name="paymentMethodId">The payment method identifier.</param>
			/// <param name="consumerTrustNegotiationOfferPaymentId">The unique identifier of the payment method within the negotiation.</param>
			/// <param name="rowVersion">The Row Version of the record.</param>
			public ConsumerTrustNegotiationPaymentMethodTypeInfo(Guid paymentMethodId, Guid consumerTrustNegotiationOfferPaymentId, Int64 rowVersion)
			{

				// Initialize the object.
				this.PaymentMethodInfoId = paymentMethodId;
				this.ConsumerTrustNegotiationOfferPaymentMethodId = consumerTrustNegotiationOfferPaymentId;
				this.RowVersion = rowVersion;

			}

		}

		/// <summary>
		/// Reset the negotiation to a "in negotiation" state.
		/// </summary>
		/// <param name="consumerTrustNegotiations">The the negotiations to reset.</param>
		internal static void Reject(ConsumerTrustNegotiationInfo[] consumerTrustNegotiations)
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
			foreach (ConsumerTrustNegotiationInfo consumerTrustNegotiationInfo in consumerTrustNegotiations)
			{

				List<ConsumerTrustNegotiationPaymentMethodTypeInfo> counterItems = new List<ConsumerTrustNegotiationPaymentMethodTypeInfo>();				

				// The blotter is not passed in from the client but is used  
				Guid blotterId = Guid.Empty;
				Status negotiationStatus;
				TrustNegotiationInfo trustNegotiationInfo = null;
				// This is the next negotiation in the batch to be updated.
				ConsumerTrustNegotiationRow consumerTrustNegotiationRow =
					DataModel.ConsumerTrustNegotiation.ConsumerTrustNegotiationKey.Find(consumerTrustNegotiationInfo.ConsumerTrustNegotiationId);

				try
				{

					// Lock the current negotation record for reading.  The data model doesn't support reader lock promotion, so the programming model is to
					// lock the database, collect the data, release the locks and then write.  This model is especially important when iterating through a
					// large batch to prevent the number of locks from growing to large.
					consumerTrustNegotiationRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

					// The blotter identifier is used for access control and is not passed in by the client.
					blotterId = consumerTrustNegotiationRow.BlotterId;
					negotiationStatus = StatusMap.FromId(consumerTrustNegotiationRow.StatusId);					
					trustNegotiationInfo = new TrustNegotiationInfo(consumerTrustNegotiationRow);

					// Determine whether the client has the right to modify this record.
					if (!TradingSupport.HasAccess(dataModelTransaction, blotterId, AccessRight.Write))
						throw new FaultException<FluidTrade.Core.SecurityFault>(new SecurityFault("You do not have write access to the selected object."));					

					// The payment methods are maintained as a vector associated with the negotiation record.  This will lock each of the records and read the 
					// payment methods into a data structure so the locks don't need to be held when it is time to write
					foreach (var consumerTrustNegotiationOfferPaymentMethodRow
						in consumerTrustNegotiationRow.GetConsumerTrustNegotiationCounterPaymentMethodRows())
					{

						try
						{

							// Temporarily lock the record containing the payment method.
							consumerTrustNegotiationOfferPaymentMethodRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);


							// This list is used to delete the payment methods that are no longer part of this negotiation.
							counterItems.Add(
								new ConsumerTrustNegotiationPaymentMethodTypeInfo(
									consumerTrustNegotiationOfferPaymentMethodRow.PaymentMethodTypeId,
									consumerTrustNegotiationOfferPaymentMethodRow.ConsumerTrustNegotiationCounterPaymentMethodId,
									consumerTrustNegotiationOfferPaymentMethodRow.RowVersion));
						}
						finally
						{

							// At this point the payment method isn't needed.
							consumerTrustNegotiationOfferPaymentMethodRow.ReleaseReaderLock(dataModelTransaction.TransactionId);

						}
					}

					MatchRow matchRow = DataModel.Match.MatchKey.Find(trustNegotiationInfo.MatchId);
					try
					{
						matchRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
						trustNegotiationInfo.MatchRowVersion = matchRow.RowVersion;
						trustNegotiationInfo.ContraMatchId = matchRow.ContraMatchId;
					}
					finally
					{
						matchRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
					}

					MatchRow contraMatchRow = DataModel.Match.MatchKey.Find(trustNegotiationInfo.ContraMatchId);
					try
					{
						contraMatchRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
						trustNegotiationInfo.ContraMatchRowVersion = contraMatchRow.RowVersion;						
					}
					finally
					{
						contraMatchRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
					}
				}
				finally
				{

					// At this point, the negotiation record isn't needed.  It is critical to release the reader locks before attempting a write.
					consumerTrustNegotiationRow.ReleaseReaderLock(dataModelTransaction.TransactionId);

				}


				// At this point, all the data for this operation has been collected and the CRUD operations can be invoked to finish the update.  Note that
				// the counter party information is not modified here, but is done through the Chinese wall.
				Guid newNegotiationId = Guid.NewGuid();				
				dataModel.CreateConsumerTrustNegotiation(
					trustNegotiationInfo.AccountBalance,
					trustNegotiationInfo.BlotterId,
					newNegotiationId,
					trustNegotiationInfo.CounterPaymentLength,
					trustNegotiationInfo.CounterPaymentStartDateLength,
					trustNegotiationInfo.CounterPaymentStartDateUnitId,
					trustNegotiationInfo.CounterSettlementUnitId,
					trustNegotiationInfo.CounterSettlementValue,
					createDateTime,
					createUserId,
					trustNegotiationInfo.CreditCardId,
					trustNegotiationInfo.IsRead,
					trustNegotiationInfo.IsReply,
					trustNegotiationInfo.MatchId,					
					modifiedTime,
					modifiedUserId,
					consumerTrustNegotiationInfo.PaymentLength,
					consumerTrustNegotiationInfo.PaymentStartDateLength,
					consumerTrustNegotiationInfo.PaymentStartDateUnitId,
					consumerTrustNegotiationInfo.SettlementUnitId,
					consumerTrustNegotiationInfo.SettlementValue,
					StatusMap.FromCode(Status.Rejected),
					out trustNegotiationInfo.Version);

				// This will add the payment methods to the negotiation that are not already there.
				foreach (Guid paymentMethodTypeId in consumerTrustNegotiationInfo.PaymentMethodTypes)
				{					
					dataModel.CreateConsumerTrustNegotiationOfferPaymentMethod(
						blotterId,
						newNegotiationId,
						Guid.NewGuid(),
						paymentMethodTypeId);
				}

				foreach (ConsumerTrustNegotiationPaymentMethodTypeInfo consumerTrustNegotiationPaymentMethodTypeInfo in counterItems)
				{
					
					dataModel.UpdateConsumerTrustNegotiationCounterPaymentMethod(
						blotterId,
						null,
						new Object[] { consumerTrustNegotiationPaymentMethodTypeInfo.ConsumerTrustNegotiationOfferPaymentMethodId },
						newNegotiationId,
						consumerTrustNegotiationPaymentMethodTypeInfo.PaymentMethodInfoId,
						consumerTrustNegotiationPaymentMethodTypeInfo.RowVersion);
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
					new object[] { trustNegotiationInfo.MatchId },
					trustNegotiationInfo.MatchRowVersion,
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
					new object[] { trustNegotiationInfo.ContraMatchId },
					trustNegotiationInfo.ContraMatchRowVersion,
					StatusMap.FromCode(Status.ValidMatch),
					null);

			}

		}

		/// <summary>
		/// Update a Consumer Trust Negotiation Record.
		/// </summary>
		internal static void Update(ConsumerTrustNegotiationInfo[] consumerTrustNegotiations)
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
			foreach (ConsumerTrustNegotiationInfo consumerTrustNegotiationInfo in consumerTrustNegotiations)
			{

				// The payment methods available to this negotiation is a vector.  Rather than delete everything and re-add it anytime an update is made, a
				// list of changes is constructed: new payment methods are added, obsolete payment methods are deleted and the ones that haven't changed are
				// left alone.  These list help to work out the differences.
				List<ConsumerTrustNegotiationPaymentMethodTypeInfo> counterItems = new List<ConsumerTrustNegotiationPaymentMethodTypeInfo>();				

				// The blotter is not passed in from the client but is used  
				Guid blotterId = Guid.Empty;
				TrustNegotiationInfo trustNegotiationInfo = null;
				// This is the next negotiation in the batch to be updated.
				ConsumerTrustNegotiationRow consumerTrustNegotiationRow =
					DataModel.ConsumerTrustNegotiation.ConsumerTrustNegotiationKey.Find(consumerTrustNegotiationInfo.ConsumerTrustNegotiationId);

				Guid matchId = Guid.Empty;
				Int64 originalVersion = Int64.MinValue;
				// Lock the current negotation record for reading.  The data model doesn't support reader lock promotion, so the programming model is to
				// lock the database, collect the data, release the locks and then write.  This model is especially important when iterating through a
				// large batch to prevent the number of locks from growing to large.								
				consumerTrustNegotiationRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);				
				try
				{					
					matchId = consumerTrustNegotiationRow.MatchId;
					originalVersion = consumerTrustNegotiationRow.Version;
				}
				finally				
				{
					consumerTrustNegotiationRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
					consumerTrustNegotiationRow = null;
				}

				//Determine the most recent Negotiation to grab the counter payment methods.
			

				Int64 maxVersion = Int64.MinValue;				
				MatchRow matchRow = DataModel.Match.MatchKey.Find(matchId);
				matchRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
				try
				{						
					foreach (ConsumerTrustNegotiationRow versionRow in matchRow.GetConsumerTrustNegotiationRows())
					{
						try
						{
							versionRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
							if (versionRow.Version > maxVersion)
							{
								maxVersion = versionRow.Version;
								consumerTrustNegotiationRow = versionRow;
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
				

				consumerTrustNegotiationRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
				try
				{
					//Check for rowversion
					if (originalVersion != consumerTrustNegotiationRow.Version)
						throw new global::System.ServiceModel.FaultException<FluidTrade.Core.OptimisticConcurrencyFault>(
							new global::FluidTrade.Core.OptimisticConcurrencyFault("ConsumerTrustNegotiation",
								new object[] { consumerTrustNegotiationInfo.ConsumerTrustNegotiationId }),
								new FaultReason("Negotiation is busy.  Please try again!"));


					// The blotter identifier is used for access control and is not passed in by the client.
					blotterId = consumerTrustNegotiationRow.BlotterId;
					trustNegotiationInfo = new TrustNegotiationInfo(consumerTrustNegotiationRow);

					// Determine whether the client has the right to modify this record.
					if (!TradingSupport.HasAccess(dataModelTransaction, blotterId, AccessRight.Write))
						throw new FaultException<FluidTrade.Core.SecurityFault>(new SecurityFault("You do not have write access to the selected object."));
					
					// The payment methods are maintained as a vector associated with the negotiation record.  This will lock each of the records and read the 
					// payment methods into a data structure so the locks don't need to be held when it is time to write
					foreach (var consumerTrustNegotiationOfferPaymentMethodRow
						in consumerTrustNegotiationRow.GetConsumerTrustNegotiationCounterPaymentMethodRows())
					{						

						try
						{
							
							// Temporarily lock the record containing the payment method.
							consumerTrustNegotiationOfferPaymentMethodRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

							
							// This list is used to delete the payment methods that are no longer part of this negotiation.
							counterItems.Add(
								new ConsumerTrustNegotiationPaymentMethodTypeInfo(
									consumerTrustNegotiationOfferPaymentMethodRow.PaymentMethodTypeId,
									consumerTrustNegotiationOfferPaymentMethodRow.ConsumerTrustNegotiationCounterPaymentMethodId,
									consumerTrustNegotiationOfferPaymentMethodRow.RowVersion));
						}
						finally
						{

							// At this point the payment method isn't needed.
							consumerTrustNegotiationOfferPaymentMethodRow.ReleaseReaderLock(dataModelTransaction.TransactionId);

						}

					}

				}
				finally
				{

					// At this point, the negotiation record isn't needed.  It is critical to release the reader locks before attempting a write.
					consumerTrustNegotiationRow.ReleaseReaderLock(dataModelTransaction.TransactionId);

				}

				// At this point, all the data for this operation has been collected and the CRUD operations can be invoked to finish the update.  Note that
				// the counter party information is not modified here, but is done through the Chinese wall.				
				Guid newNegotiationId = Guid.NewGuid();				
				dataModel.CreateConsumerTrustNegotiation(
					trustNegotiationInfo.AccountBalance,
					trustNegotiationInfo.BlotterId,
					newNegotiationId,
					trustNegotiationInfo.CounterPaymentLength,
					trustNegotiationInfo.CounterPaymentStartDateLength,
					trustNegotiationInfo.CounterPaymentStartDateUnitId,
					trustNegotiationInfo.CounterSettlementUnitId,
					trustNegotiationInfo.CounterSettlementValue,
					createDateTime,
					createUserId,
					trustNegotiationInfo.CreditCardId,
					trustNegotiationInfo.IsRead,
					trustNegotiationInfo.IsReply,
					trustNegotiationInfo.MatchId,					
					modifiedTime,
					modifiedUserId,
					consumerTrustNegotiationInfo.PaymentLength,
					consumerTrustNegotiationInfo.PaymentStartDateLength,
					consumerTrustNegotiationInfo.PaymentStartDateUnitId,
					consumerTrustNegotiationInfo.SettlementUnitId,
					consumerTrustNegotiationInfo.SettlementValue,
					consumerTrustNegotiationInfo.StatusId,
					out trustNegotiationInfo.Version);
				
				// This will add the payment methods to the negotiation that are not already there.
				foreach (Guid paymentMethodTypeId in consumerTrustNegotiationInfo.PaymentMethodTypes)
				{
					dataModel.CreateConsumerTrustNegotiationOfferPaymentMethod(
						blotterId,
						newNegotiationId,
						Guid.NewGuid(),
						paymentMethodTypeId);
				}

				//Since we cannot create new counter payments, we will update the existing ones.
				foreach (ConsumerTrustNegotiationPaymentMethodTypeInfo consumerTrustNegotiationPaymentMethodTypeInfo in counterItems)
				{					
					dataModel.UpdateConsumerTrustNegotiationCounterPaymentMethod(
						blotterId,
						null,
						new Object[] { consumerTrustNegotiationPaymentMethodTypeInfo.ConsumerTrustNegotiationOfferPaymentMethodId },
						newNegotiationId,
						consumerTrustNegotiationPaymentMethodTypeInfo.PaymentMethodInfoId,
						consumerTrustNegotiationPaymentMethodTypeInfo.RowVersion);
				}
			}
		}

		/// <summary>
		/// Updates the state of a collection of Trust Negotiation settlement records.
		/// </summary>
		internal static void Update(ConsumerTrustNegotiationIsReadInfo[] consumerTrustNegotiationIsReads)
		{

			// An instance of the shared data model is required to use its methods.
			DataModel dataModel = new DataModel();

			// This Web Method comes with an implicit transaction that is linked to its execution.
			DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

			// This method can handle a batch of updates in a single transaction.
			foreach (ConsumerTrustNegotiationIsReadInfo consumerTrustNegotiationIsReadInfo in consumerTrustNegotiationIsReads)
			{

				try
				{

					if (consumerTrustNegotiationIsReadInfo != null && consumerTrustNegotiationIsReadInfo.ConsumerTrustNegotiationId != Guid.Empty)
					{

						// The blotter is not passed in from the client but is used to validate the users access to this record.
						Guid blotterId = Guid.Empty;						
						Int64 version = 0;
						Int64 rowVersion = 0;

						// This is the next negotiation in the batch to be updated.
						ConsumerTrustNegotiationRow consumerTrustNegotiationRow =
							DataModel.ConsumerTrustNegotiation.ConsumerTrustNegotiationKey.Find(consumerTrustNegotiationIsReadInfo.ConsumerTrustNegotiationId);

						try
						{

							// Lock the current negotation record for reading.  The data model doesn't support reader lock promotion, so the programming model is to
							// lock the database, collect the data, release the locks and then write.  This model is especially important when iterating through a
							// large batch to prevent the number of locks from growing to large.
							consumerTrustNegotiationRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

							// The blotter identifier is used for access control and is not passed in by the client.
							blotterId = consumerTrustNegotiationRow.BlotterId;
							version = consumerTrustNegotiationRow.Version;
							//Use the current rowversion to update.  This is to avoid an concurrency error. The client may not have
							//gotten the updated row if they made a change before the IsRead fired of.  Since this is a benign change
							//we can safely update the row and let the change trickle back on update.
							rowVersion = consumerTrustNegotiationRow.RowVersion;

							if (rowVersion != consumerTrustNegotiationIsReadInfo.RowVersion)
							{
								EventLog.Warning("ConsumerTrustNegotiaion ISRead incompatible rowVersions. Expected {0}, Got {1}. Using {0}",
									consumerTrustNegotiationIsReadInfo.RowVersion, rowVersion);
							}

							// Determine whether the client has the right to modify this record.
							if (!TradingSupport.HasAccess(dataModelTransaction, blotterId, AccessRight.Read))
								throw new FaultException<FluidTrade.Core.SecurityFault>(new SecurityFault("You do not have read access to the selected object."));

						}
						finally
						{

							// At this point, the negotiation record isn't needed.  It is critical to release the reader locks before attempting a write.
							if(consumerTrustNegotiationRow != null)
								consumerTrustNegotiationRow.ReleaseReaderLock(dataModelTransaction.TransactionId);

						}

						// At this point, all the data for this operation has been collected and the CRUD operations can be invoked to finish the update.  Note that
						// the counter party information is not modified here, but is done through the Chinese wall.
						dataModel.UpdateConsumerTrustNegotiation(
							null,
							null,
							null,
							new object[] { consumerTrustNegotiationIsReadInfo.ConsumerTrustNegotiationId },
							null,
							null,
							null,
							null,
							null,
							null,
							null,
							null,
							consumerTrustNegotiationIsReadInfo.IsRead,
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
					//Throw this back to the client because we cannot handle this
					throw faultException;
				}
				catch(Exception ex)
				{
					//We will log the exception and try to carry on.
					EventLog.Error(ex);

				}
			}

		}

		/// <summary>
		/// Information about a Consumer Debt Settlement
		/// </summary>
		private class TrustNegotiationInfo
		{
			public Decimal AccountBalance { get; set; }
			public Guid BlotterId { get; set; }
			public Guid CreditCardId { get; set; }
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
			
			public TrustNegotiationInfo(ConsumerTrustNegotiationRow consumerTrustNegotiationRow)
			{
				this.AccountBalance = consumerTrustNegotiationRow.AccountBalance;
				this.BlotterId = consumerTrustNegotiationRow.BlotterId;
				this.CreditCardId = consumerTrustNegotiationRow.CreditCardId;
				this.MatchId = consumerTrustNegotiationRow.MatchId;
				this.CounterPaymentLength = consumerTrustNegotiationRow.CounterPaymentLength;
				this.CounterPaymentStartDateLength = consumerTrustNegotiationRow.CounterPaymentStartDateLength;
				this.CounterPaymentStartDateUnitId = consumerTrustNegotiationRow.CounterPaymentStartDateUnitId;
				this.CounterSettlementUnitId = consumerTrustNegotiationRow.CounterSettlementUnitId;
				this.CounterSettlementValue = consumerTrustNegotiationRow.CounterSettlementValue;
				this.IsRead = consumerTrustNegotiationRow.IsRead;
				this.IsReply = consumerTrustNegotiationRow.IsReply;
			}
		}
	}

}
