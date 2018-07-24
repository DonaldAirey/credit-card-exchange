using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Transactions;
using FluidTrade.Core;

namespace FluidTrade.Guardian
{
	/// <summary>
	/// 
	/// </summary>
	public class ConsumerCrossDebt : ConsumerCross
	{
		/// <summary>
		/// 
		/// </summary>
		public ConsumerCrossDebt()
			:base()
		{

			//could have multiple threads to do the matching. and have
			//one instance of the consumerCross.  If do this
			//should have the first thread build the matcher
			//and the other threads should use it.
			instance = this;

			consumerCrossDebtCreationResetEvent.Set();

			lock(consumerCrossDebtCreationResetEventSyncObject)
			{
				ManualResetEvent tmpEvent = consumerCrossDebtCreationResetEvent;
				consumerCrossDebtCreationResetEvent = null;
				tmpEvent.Close();
			}
		}


		private static ManualResetEvent consumerCrossDebtCreationResetEvent = new ManualResetEvent(false);
		private static object consumerCrossDebtCreationResetEventSyncObject = new object();

		private static ConsumerCrossDebt instance;
		/// <summary>
		/// 
		/// </summary>
		public static ConsumerCrossDebt Instance
		{
			get
			{
				if(instance == null)
				{
					lock(consumerCrossDebtCreationResetEventSyncObject)
					{
						if(consumerCrossDebtCreationResetEvent != null)
							consumerCrossDebtCreationResetEvent.WaitOne();
					}
				}

				return instance;
			}
		}


		/// <summary>
		/// 
		/// </summary>
		protected override void SubscribeToContraNegotiationTableEvents()
		{
			DataModel.ConsumerTrustNegotiation.ConsumerTrustNegotiationRowValidate += new ConsumerTrustNegotiationRowChangeEventHandler(ConsumerTrustNegotiationRowValidate);
			DataModel.ConsumerTrustNegotiationOfferPaymentMethod.ConsumerTrustNegotiationOfferPaymentMethodRowValidate += new ConsumerTrustNegotiationOfferPaymentMethodRowChangeEventHandler(ConsumerTrustNegotiationOfferPaymentMethodRowValidate);
		}

		/// <summary>
		/// 
		/// </summary>
		protected override void SubscribeToNegotiationTableEvents()
		{
			DataModel.ConsumerDebt.ConsumerDebtRowValidate += new ConsumerDebtRowChangeEventHandler(ConsumerDebt_ConsumerDebtRowValidate);
		}

		/// <summary>
		/// event to handle updating the debtRuleId change
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ConsumerDebt_ConsumerDebtRowValidate(object sender, ConsumerDebtRowChangeEventArgs e)
		{
			DataRowState rowState = e.Row.RowState;

			//only looking for changes in the debtRule/debtClass in the debtRule
			if(rowState == DataRowState.Deleted ||
				rowState == DataRowState.Detached ||
				(e.Row.HasVersion(DataRowVersion.Original) == false ||
				e.Row.HasVersion(DataRowVersion.Current) == false) ||
					(object.Equals(e.Row[DataModel.ConsumerDebt.DebtRuleIdColumn, DataRowVersion.Original], e.Row[DataModel.ConsumerDebt.DebtRuleIdColumn, DataRowVersion.Current])))
				return;

			this.Consumer_ConsumerRowChanging(this, new ConsumerRowChangeEventArgs(e.Row.ConsumerRow, DataRowAction.Commit));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="debtRuleRow"></param>
		/// <param name="retList"></param>
		protected override void GetConsumerRowsFromDebtRule(DebtRuleRow debtRuleRow, List<ConsumerRow> retList)
		{
			ConsumerDebtRow[] consumerDebtRows = debtRuleRow.GetConsumerDebtRows();
			if(consumerDebtRows.Length == 0)
				return;

			DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

			foreach(ConsumerDebtRow consumerDebtRow in consumerDebtRows)
			{
				consumerDebtRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

				try
				{
					ConsumerRow consumerRow = consumerDebtRow.ConsumerRow_NoLockCheck;
					if(consumerRow != null)
						retList.Add(consumerRow);
				}
				finally
				{
					consumerDebtRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="consumerDebtMatchInfo"></param>
		/// <param name="consumerTrustMatchInfo"></param>
		/// <returns></returns>
		internal override bool GetHasFunds(ConsumerDebtMatchInfo consumerDebtMatchInfo, ConsumerTrustMatchInfo consumerTrustMatchInfo)
		{
			return consumerTrustMatchInfo.SavingsBalance >= consumerDebtMatchInfo.AccountBalance * consumerDebtMatchInfo.SettlementValue;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="matchCreateParam"></param>
		/// <returns></returns>
		internal override Status? GetStatus(MatchCreateParms matchCreateParam)
		{
			// Calculate the status based on how much of a match we have and how much money is in the account.
			Boolean hasFunds = this.GetHasFunds(matchCreateParam.consumerDebtMatchInfo, matchCreateParam.consumerTrustMatchInfo);
			Status? status;

			if(matchCreateParam.matchStatusId != Guid.Empty &&
				matchCreateParam.matchStatusId != ValidMatchFundsStatus &&
				matchCreateParam.matchStatusId != ValidMatchStatus &&
				matchCreateParam.matchStatusId != PartialMatchStatus)
			{
				status = null;
			}
			else
			{
				status = (matchCreateParam.matchStrength == 1.0M && hasFunds) ? Status.ValidMatchFunds :
									(matchCreateParam.matchStrength == 1.0M) ? Status.ValidMatch : Status.PartialMatch;
			}

			return status;
		}


		/// <summary>
		/// Validates that a Consumer Trust Negotiation offer is reflected in the counter party's offer.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
		private void ConsumerTrustNegotiationRowValidate(object sender, ConsumerTrustNegotiationRowChangeEventArgs e)
		{

			// This is the record that is to be validated.  The main idea of this trigger is to make sure that if anything has change in the offer, that the
			// counter party's negotiation record reflects the changes.
			ConsumerTrustNegotiationRow consumerTrustNegotiationRow = e.Row;
			if(consumerTrustNegotiationRow.RowState == DataRowState.Deleted ||
				consumerTrustNegotiationRow.RowState == DataRowState.Detached)
				return;

			// The counter party is informed of an update to this record on every event except when only the 'IsRead' flag has changed.			
			Boolean isNegotiationChanged = false;

			// The action determine which fields need to be examined.
			switch (e.Action)
			{
			case DataRowAction.Add:

				// When adding a negotiation row, everything needs to be synchronized with the counter party's negotiation.
				isNegotiationChanged = true;
				break;

			case DataRowAction.Change:

				//Check the Blotter has changed.  This can happen on Move To operation.  So do not bother updating
				if (!consumerTrustNegotiationRow[DataModel.ConsumerTrustNegotiation.BlotterIdColumn, DataRowVersion.Current].Equals(
					consumerTrustNegotiationRow[DataModel.ConsumerTrustNegotiation.BlotterIdColumn, DataRowVersion.Original]))
					isNegotiationChanged = false;

				// Check the state of the '	ad' flag.
				if (!consumerTrustNegotiationRow[DataModel.ConsumerTrustNegotiation.IsReadColumn, DataRowVersion.Current].Equals(
					consumerTrustNegotiationRow[DataModel.ConsumerTrustNegotiation.IsReadColumn, DataRowVersion.Original]))
					isNegotiationChanged = false;
	
				// Check the number of payments in the offer.
				if (!consumerTrustNegotiationRow[DataModel.ConsumerTrustNegotiation.OfferPaymentLengthColumn, DataRowVersion.Current].Equals(
					consumerTrustNegotiationRow[DataModel.ConsumerTrustNegotiation.OfferPaymentLengthColumn, DataRowVersion.Original]))
					isNegotiationChanged = true;

				// Check the amount of time until the first payment is made.
				if (!consumerTrustNegotiationRow[DataModel.ConsumerTrustNegotiation.OfferPaymentStartDateLengthColumn, DataRowVersion.Current].Equals(
					consumerTrustNegotiationRow[DataModel.ConsumerTrustNegotiation.OfferPaymentStartDateLengthColumn, DataRowVersion.Original]))
					isNegotiationChanged = true;

				// Check the units in which the time to start the payments is quoted.
				if (!consumerTrustNegotiationRow[DataModel.ConsumerTrustNegotiation.OfferPaymentStartDateUnitIdColumn, DataRowVersion.Current].Equals(
					consumerTrustNegotiationRow[DataModel.ConsumerTrustNegotiation.OfferPaymentStartDateUnitIdColumn, DataRowVersion.Original]))
					isNegotiationChanged = true;

				// Check the units in which the settlement amount is quoted.
				if (!consumerTrustNegotiationRow[DataModel.ConsumerTrustNegotiation.OfferSettlementUnitIdColumn, DataRowVersion.Current].Equals(
					consumerTrustNegotiationRow[DataModel.ConsumerTrustNegotiation.OfferSettlementUnitIdColumn, DataRowVersion.Original]))
					isNegotiationChanged = true;

				// Check the settlement amount.
				if (!consumerTrustNegotiationRow[DataModel.ConsumerTrustNegotiation.OfferSettlementValueColumn, DataRowVersion.Current].Equals(
					consumerTrustNegotiationRow[DataModel.ConsumerTrustNegotiation.OfferSettlementValueColumn, DataRowVersion.Original]))
					isNegotiationChanged = true;

				break;

			}

			// This condition prevents recursive updates from one side of the trade to the other and back again.  If the update came from the thread that
			// handles the background tasks for the trust, we don't need to execute any business rules.
			if (Thread.CurrentThread.Name != "ConsumerCrossTrust")
			{

				// This condition prevents an update of the counter party when just the 'IsRead' flag has changed.
				if (isNegotiationChanged)
				{

					// When the negotiation item is changed, make a call out to a thread that will synchronize the counter party to the new values. Note that 
					// this method is done on a seperate thread in order to simplify the locking that must be done.  There's no guarantee of the state of the
					// locks when this trigger is called, so any method called directly would need to check the state of every lock before it tried to use a
					// record.
					this.actionQueue.Enqueue(
						new ObjectAction(
							CrossConsumerTrustNegotiation,
							new Object[] { consumerTrustNegotiationRow[DataModel.ConsumerTrustNegotiation.ConsumerTrustNegotiationIdColumn], e.Row }));

				}

			}

		}

		/// <summary>
		/// Validates that a Consumer Trust Negotiation Offer Payment Method is reflected in the counter party's offer.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
		private void ConsumerTrustNegotiationOfferPaymentMethodRowValidate(object sender, ConsumerTrustNegotiationOfferPaymentMethodRowChangeEventArgs e)
		{

			// This is the record that is to be validated.  The main idea of this trigger is to make sure that if anything has change in the list of payment
			// methods available on this offer, that the counter party's negotiation reflects the same payment methods.
			ConsumerTrustNegotiationOfferPaymentMethodRow consumerTrustNegotiationOfferPaymentMethodRow = e.Row;
			if(consumerTrustNegotiationOfferPaymentMethodRow.RowState == DataRowState.Deleted ||
				consumerTrustNegotiationOfferPaymentMethodRow.RowState == DataRowState.Detached)
				return;

			// Deleted records aren't easy to access.  The row version must be used to extract the parent record.  This variable will hold the parent record 
			// in all actions to the list of payment methods.
			Guid consumerTrustNegotiationId = Guid.Empty;

			// There is no need to update the counter party's list of payment methods if this is still false when all the conditions have been tested.
			Boolean isNegotiationChanged = false;

			switch(e.Action)
			{
				case DataRowAction.Add:

					// The counter party is updated when a payment method is added.
					isNegotiationChanged = true;
					consumerTrustNegotiationId = (Guid)consumerTrustNegotiationOfferPaymentMethodRow[
						DataModel.ConsumerTrustNegotiationOfferPaymentMethod.ConsumerTrustNegotiationIdColumn, DataRowVersion.Current];
					break;

				case DataRowAction.Delete:

					// The counter party is udpate when a payment method is deleted.
					isNegotiationChanged = true;
					consumerTrustNegotiationId = (Guid)consumerTrustNegotiationOfferPaymentMethodRow[
						DataModel.ConsumerTrustNegotiationOfferPaymentMethod.ConsumerTrustNegotiationIdColumn, DataRowVersion.Original];
					break;

			}

			// If any of the payment methods in the record have been changed, call out to a thread that will synchronize the counter party's payment methods 
			// to the new list. Note that this method is done on a seperate thread in order to simplify the locking that must be done.  There's no guarantee of
			// the state of the locks when this trigger is called, so any method called directly would need to check the state of every lock before it tried to
			// use a record.
			if(isNegotiationChanged)
				this.actionQueue.Enqueue(new ObjectAction(CrossConsumerTrustNegotiation, new Object[] { consumerTrustNegotiationId, e.Row}));

		}

		/// <summary>
		/// Evaluates whether a given working order is eligible for a cross with another order.
		/// </summary>		
		private void CrossConsumerTrustNegotiation(Object[] key, params Object[] parameters)
		{
			// An instance of the data model is required for CRUD operations.
			DataModel dataModel = new DataModel();

			// The locking model for the middle tier is optimized for performance.  The lightweight Reader/Writer Locks do not allow for recursive 'Reader'
			// locks or promotion of 'Reader' locks to 'Writer' locks.  Therefor, the programming model is to collect all the information required for a
			// transaction during a 'Read' phase into a data structure that is used during the 'Write' phase.  All 'Read' locks should be released by the time
			// the server logic is ready to write.
			ConsumerDebtCrossInfo consumerDebtCrossInfo = new ConsumerDebtCrossInfo();

			// The logic below will examine the order and see if a contra order is available for a match.  These values will indicate whether a match is
			// possible after all the locks have been released.
			Guid consumerTrustNegotiationId = (Guid)key[0];
			ConsumerTrustNegotiationRow consumerTrustNegotiationRow = key[1] as ConsumerTrustNegotiationRow;

			// A transaction is required to lock the records and change the data model.
			using(TransactionScope transactionScope = new TransactionScope())
			{
				// This variable holds context information for the current transaction.
				DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

				// These variables are used for auditing the changes to this record.
				consumerDebtCrossInfo.ModifiedTime = DateTime.UtcNow;
				consumerDebtCrossInfo.ModifiedUserId = TradingSupport.UserId;

				MatchRow contraMatchRow = null;
				MatchRow consumerTrustNegotiationRowMatchRow = null;				
				Guid consumerTrustNegotiationRowMatchRowContraMatchId;

				// This is the working order that will be tested for a possible buyer or seller (contra party).
				if(consumerTrustNegotiationRow == null)
					consumerTrustNegotiationRow = DataModel.ConsumerTrustNegotiation.ConsumerTrustNegotiationKey.Find(consumerTrustNegotiationId);
				if(consumerTrustNegotiationRow == null)
				{
					//this is can be a vaild case since another thread could delete the row
					return;
				}

				consumerTrustNegotiationRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
				try
				{
					consumerTrustNegotiationRowMatchRow = consumerTrustNegotiationRow.MatchRow;

					if(consumerTrustNegotiationRowMatchRow == null)
						return;
					
					consumerDebtCrossInfo.CounterPaymentLength = consumerTrustNegotiationRow.OfferPaymentLength;
					consumerDebtCrossInfo.CounterPaymentStartDateLength = consumerTrustNegotiationRow.OfferPaymentStartDateLength;
					consumerDebtCrossInfo.CounterPaymentStartDateUnitId = consumerTrustNegotiationRow.OfferPaymentStartDateUnitId;
					consumerDebtCrossInfo.CounterSettlementUnitId = consumerTrustNegotiationRow.OfferSettlementUnitId;
					consumerDebtCrossInfo.CounterSettlementValue = consumerTrustNegotiationRow.OfferSettlementValue;
					consumerDebtCrossInfo.StatusId = consumerTrustNegotiationRow.StatusId;
					
				}
				finally
				{
					consumerTrustNegotiationRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				}


				consumerTrustNegotiationRowMatchRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
				try
				{
					if (consumerTrustNegotiationRowMatchRow.RowState == DataRowState.Deleted ||
						consumerTrustNegotiationRowMatchRow.RowState == DataRowState.Detached)
					{
						return;
					}
					consumerTrustNegotiationRowMatchRowContraMatchId = consumerTrustNegotiationRowMatchRow.ContraMatchId;

				}
				finally
				{
					consumerTrustNegotiationRowMatchRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				}
					
				ConsumerTrustNegotiationOfferPaymentMethodRow[] consumerTrustNegotiationOfferPaymentMethodRows;
				consumerTrustNegotiationRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
				try
				{
					consumerTrustNegotiationOfferPaymentMethodRows = consumerTrustNegotiationRow.GetConsumerTrustNegotiationOfferPaymentMethodRows();
				}
				finally
				{
					consumerTrustNegotiationRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				}

				// Extract the information from the negotiation record that's about to be modified by this new counter offer.  This info can't be taked
				// from the record when the call is made because the record must be locked for a write and there is no mechanism for a promotion.
				foreach(
					ConsumerTrustNegotiationOfferPaymentMethodRow consumerTrustNegotiationOfferPaymentMethodRow
					in consumerTrustNegotiationOfferPaymentMethodRows)
				{
					try
					{
						consumerTrustNegotiationOfferPaymentMethodRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
						consumerDebtCrossInfo.AddedCounterPaymentMethodTypes.Add(
							consumerTrustNegotiationOfferPaymentMethodRow.PaymentMethodTypeId);
					}
					finally
					{
						consumerTrustNegotiationOfferPaymentMethodRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
					}
				}

				// The PaymentMethodType is a vector that needs to be synchronized to the current offer.  This means making several passes at the data to
				// determine what is new, what has to be deleted and what hasn't changed.  These buckets are used to collect those payment methods that
				// apply to the counter offer.
				List<Guid> addedList = new List<Guid>();
				List<ConsumerDebtNegotiationPaymentMethodInfo> deletedList = new List<ConsumerDebtNegotiationPaymentMethodInfo>();

				// This is the counter party to the match.  There are no direct links between the order and the contra order due to the Chinese wall that
				// prevents data from one party being accessible to another party.
				ConsumerDebtNegotiationRow[] contraMatchRowConsumerDebtNegotiationRows;
				contraMatchRow = DataModel.Match.MatchKey.Find(consumerTrustNegotiationRowMatchRowContraMatchId);
				bool rowAlreadyLocked = contraMatchRow.IsLockHeld(dataModelTransaction.TransactionId);
				if(rowAlreadyLocked == false)
					contraMatchRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
				try
				{
					contraMatchRowConsumerDebtNegotiationRows = contraMatchRow.GetConsumerDebtNegotiationRows();
				}
				finally
				{
					if(rowAlreadyLocked == false)
						contraMatchRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				}
				// There is only one order on the other side of this offer but the iteration is a cleaner way to access it.  This will extract the
				// housekeeping values from the counter offer that are necessary to update it with the data from this order.
				foreach(ConsumerDebtNegotiationRow consumerDebtNegotiationRow in contraMatchRowConsumerDebtNegotiationRows)
				{

					ConsumerDebtNegotiationCounterPaymentMethodRow[] consumerDebtNegotiationCounterPaymentMethodRows;
					try
					{
						// One of the compromises for speed in this data model is that the ReaderWriterLocks do not keep track of the number of times they
						// are called.  Also, Reader locks are not promoted to Writer locks.  The goal here is to call the internal method to update this
						// negotiation record with the information copied from the modified negotiation record.  To do this, the key and the row version
						// are needed.  The rub is, before the update method is called, all the locks on this record need to be released.						
						consumerDebtNegotiationRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
						consumerDebtCrossInfo.BlotterId = consumerDebtNegotiationRow.BlotterId;
						consumerDebtCrossInfo.ConsumerDebtNegotiationId = consumerDebtNegotiationRow.ConsumerDebtNegotiationId;
						consumerDebtCrossInfo.ContraRowVersion = consumerDebtNegotiationRow.RowVersion;
						consumerDebtCrossInfo.MatchId = consumerDebtNegotiationRow.MatchId;
						consumerDebtNegotiationCounterPaymentMethodRows = consumerDebtNegotiationRow.GetConsumerDebtNegotiationCounterPaymentMethodRows();
						
					}
					finally
					{

						// For those who are about to be modified, we salute you.
						consumerDebtNegotiationRow.ReleaseReaderLock(dataModelTransaction.TransactionId);

					}


					//See if this is a rejected negotiation. Then regenerate a match.
					StatusRow statusRow = DataModel.Status.StatusKey.Find(consumerDebtCrossInfo.StatusId.GetValueOrDefault());
					try
					{
						statusRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
						if (statusRow.StatusCode == Status.Rejected)
						{
							MatchRow matchRow = DataModel.Match.MatchKey.Find(consumerDebtCrossInfo.MatchId);
							matchRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
							bool workingOderRowDeleted = false;
							try
							{

								ConsumerRow consumerRow = GetConsumerRowFromWorkingOrderRow(matchRow.WorkingOrderRow, dataModelTransaction,
									out workingOderRowDeleted);
								
								if(workingOderRowDeleted == false)
									this.Consumer_ConsumerRowChanging(this, new ConsumerRowChangeEventArgs(consumerRow, DataRowAction.Commit));
							}
							finally
							{
								matchRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
							}

						}
					}
					finally
					{
						statusRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
					}

					// Each of the available payment method types must be communicated to the counter party.  The payment methods are a vector then
					// associated with the negotiation.  In order to optimize the the number of disk operations only the differences between the
					// current collection of payment methods and the new ones is committed.
					foreach(
						ConsumerDebtNegotiationCounterPaymentMethodRow consumerDebtNegotiationCounterPaymentMethodRow
						in consumerDebtNegotiationCounterPaymentMethodRows)
					{

						// The main idea here is to collect the information about the payment methods already associated with the counter party
						// during the read phase.  These locks can't be in place when it is time to update the negotiation for the counter party.
						consumerDebtNegotiationCounterPaymentMethodRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
						try
						{

							// To optimize the number of writes to the disk, the payment methods that are already associated with the counter party's
							// negotiation record are left in place.  Only the new ones will be written.
							foreach(Guid paymentMethodTypeId in consumerDebtCrossInfo.AddedCounterPaymentMethodTypes)
							{
								if(consumerDebtNegotiationCounterPaymentMethodRow.PaymentMethodTypeId == paymentMethodTypeId)
								{
									addedList.Add(paymentMethodTypeId);
								}
							}

							// This list contains all the items currently in the counter party's negotiation record.  The list will be culled down 
							// after the collection is made to delete only the payment methods that are no longer part of the negotation.
							deletedList.Add(
								new ConsumerDebtNegotiationPaymentMethodInfo(
									consumerDebtNegotiationCounterPaymentMethodRow.ConsumerDebtNegotiationCounterPaymentMethodId,
									consumerDebtNegotiationCounterPaymentMethodRow.PaymentMethodTypeId,
									consumerDebtNegotiationCounterPaymentMethodRow.RowVersion));

						}
						finally
						{

							// It is important to release locks when they are no longer needed.
							consumerDebtNegotiationCounterPaymentMethodRow.ReleaseReaderLock(dataModelTransaction.TransactionId);

						}

					}

				}

				// This loop will create a list of Payment Methods that need to be removed from the counter offer.
				foreach(ConsumerDebtNegotiationPaymentMethodInfo oldPaymentMethodInfo in deletedList)
				{

					// This will search the list of existing payment types trying to find the ones that are no longer part of the counter offer.
					Boolean isFound = false;
					foreach(Guid paymentMethodTypeId in consumerDebtCrossInfo.AddedCounterPaymentMethodTypes)
					{
						if(oldPaymentMethodInfo.PaymentMethodTypeId == paymentMethodTypeId)
							isFound = true;
					}

					// If the old payment type isn't part of the new list of payment types then it will be purged from the vector.
					if(!isFound)
						consumerDebtCrossInfo.DeletedCounterPaymentMethodTypes.Add(oldPaymentMethodInfo);

				}

				// This will remove any items that are already in the counter offer so they aren't added twice.
				foreach(Guid paymentMethodTypeId in addedList)
				{
					consumerDebtCrossInfo.AddedCounterPaymentMethodTypes.Remove(paymentMethodTypeId);
				}

				// At this point all the information has been collected to allow for an update of the counter party's negotiation to make it match this party's
				// offer.  In this way the bids and offers are communicated instantly to both parties.
				dataModel.UpdateConsumerDebtNegotiation(
					null,
					null,
					null,
					new Object[] { consumerDebtCrossInfo.ConsumerDebtNegotiationId },
					consumerDebtCrossInfo.CounterPaymentLength,
					consumerDebtCrossInfo.CounterPaymentStartDateLength,
					consumerDebtCrossInfo.CounterPaymentStartDateUnitId,
					consumerDebtCrossInfo.CounterSettlementUnitId,
					consumerDebtCrossInfo.CounterSettlementValue,
					null,
					null,
					false,
					true,
					null,
					consumerDebtCrossInfo.ModifiedTime,
					consumerDebtCrossInfo.ModifiedUserId,
					null,
					null,
					null,
					null,
					null,
					consumerDebtCrossInfo.ContraRowVersion,
					consumerDebtCrossInfo.StatusId,
					null);

				// This will remove any payment metohds from the counter party's negotiation record that are not part of this party's negotiation record.
				foreach(ConsumerDebtNegotiationPaymentMethodInfo paymentMethodInfo in consumerDebtCrossInfo.DeletedCounterPaymentMethodTypes)
				{
					dataModel.DestroyConsumerDebtNegotiationCounterPaymentMethod(
						new Object[] { paymentMethodInfo.ConsumerDebtNegotiationPaymentMethodId },
						paymentMethodInfo.RowVersion);
				}

				// This will add only the new payment methods to the counter party's negotiation keeping it synchronized with this negotiation.
				foreach(Guid paymentMethodTypeId in consumerDebtCrossInfo.AddedCounterPaymentMethodTypes)
				{
					dataModel.CreateConsumerDebtNegotiationCounterPaymentMethod(
						consumerDebtCrossInfo.BlotterId,
						Guid.NewGuid(),
						consumerDebtCrossInfo.ConsumerDebtNegotiationId,
						paymentMethodTypeId);
				}

				// The working order that triggered this action has completed a scan of the data model and has notified all possible counter parties of the its
				// new status.  Once the transaction is completed, a negotiation session will be started if a new counter party is found.
				transactionScope.Complete();
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="validConsumerRowList"></param>
		/// <param name="validCreditCardRowList"></param>
		/// <param name="dataModelTransaction"></param>
		protected override void GetAllValidConsumerAndCreditCardRows(List<RowLockingWrapper<ConsumerRow>> validConsumerRowList, List<RowLockingWrapper<CreditCardRow>> validCreditCardRowList,
			IDataModelTransaction dataModelTransaction)
		{
			//!!!RM 90% sure enumerating over the ConsumertTrust records is not threadsafe.. need to test
			DataModel.DataLock.EnterReadLock();
			List<ConsumerDebtRow> list;
			try
			{
				list = new List<ConsumerDebtRow>(DataModel.ConsumerDebt);
			}
			finally
			{
				DataModel.DataLock.ExitReadLock();
			}

			//!!!RM do locking and use lower level calls to save time on the IsLockHeld calls
			foreach(ConsumerDebtRow consumerDebtRow in list)
			{
				ConsumerRow curConsumerRow;
				CreditCardRow curCreditCardRow;
				SecurityRow secRow;
				
				consumerDebtRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
				try
				{
					curConsumerRow = consumerDebtRow.ConsumerRow;
					curCreditCardRow = consumerDebtRow.CreditCardRow;

					if(curConsumerRow == null &&
						curCreditCardRow == null)
						continue;

					secRow = consumerDebtRow.SecurityRow;
					if(secRow == null)
						continue;
				}
				finally
				{
					consumerDebtRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				}

				List<WorkingOrderRow> tmpWorkingOrderList = new List<WorkingOrderRow>();
				this.GetSubmittedWorkingOrderList(secRow, dataModelTransaction, tmpWorkingOrderList, null);
				if(tmpWorkingOrderList.Count == 0)
					continue;

				if(curConsumerRow != null)
					validConsumerRowList.Add(new RowLockingWrapper<ConsumerRow>(curConsumerRow, dataModelTransaction));

				if(curCreditCardRow != null)
					validCreditCardRowList.Add(new RowLockingWrapper<CreditCardRow>(curCreditCardRow, dataModelTransaction));
				
			}
		}

		/// <summary>
		/// 
		/// </summary>
		protected override string ThreadName
		{
			get { return "ConsumerCrossDebt"; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="consumerRow"></param>
		/// <param name="dataModelTransaction"></param>
		/// <param name="consumerContainerRowList"></param>
		protected override void GetValidConsumerRelatedRows(RowLockingWrapper<ConsumerRow> consumerRow, IDataModelTransaction dataModelTransaction,
														out List<ConsumerContainerRow> consumerContainerRowList)
		{

			ConsumerDebtRow[] consumerDebtRowAr;
			consumerRow.AcquireReaderLock();
			try
			{
				consumerDebtRowAr = consumerRow.TypedRow.GetConsumerDebtRows_NoLockCheck();
			}
			finally
			{
				consumerRow.ReleaseReaderLock();
			}

			consumerContainerRowList = new List<ConsumerContainerRow>();
			for(int i = consumerDebtRowAr.Length - 1; i >= 0; i--)
			{
				ConsumerDebtRow consumerDebtRow = consumerDebtRowAr[i];
				SecurityRow secRow;
				consumerDebtRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
				try
				{
					if(consumerDebtRow.RowState == System.Data.DataRowState.Deleted ||
					consumerDebtRow.RowState == System.Data.DataRowState.Detached)
					continue;
				
					secRow = consumerDebtRow.SecurityRow_NoLockCheck;
					if(secRow == null)
						continue;
				}
				finally
				{
					consumerDebtRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				}

				List<WorkingOrderRow> tmpWorkingOrderList = new List<WorkingOrderRow>();
				List<MatchRow[]> workingOrdersMatchRowsList = new List<MatchRow[]>();
				this.GetSubmittedWorkingOrderList(secRow, dataModelTransaction, tmpWorkingOrderList, workingOrdersMatchRowsList);
				if(tmpWorkingOrderList.Count == 0)
					continue;

				ConsumerContainerRow newConainerRow = new ConsumerContainerRow();
				newConainerRow.ContainerRow = consumerDebtRow;
				newConainerRow.WorkingOrderRow = tmpWorkingOrderList[0];
				newConainerRow.WorkingOrderRowMatchRowsList = workingOrdersMatchRowsList;

				consumerDebtRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
				try
				{
					newConainerRow.CreditCardRow = consumerDebtRow.CreditCardRow_NoLockCheck;
				}
				finally
				{
					consumerDebtRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				}

				consumerContainerRowList.Add(newConainerRow);
			}
			
		}


		/// <summary>
		/// 
		/// </summary>
		protected override ConsumerCross ContraConsumerCross
		{
			get
			{
				return ConsumerCrossTrust.Instance;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="consumerRow"></param>
		/// <param name="consumerContainerRowList"></param>
		/// <param name="dataModelTransaction"></param>
		/// <returns></returns>
		protected override void FindMaxContraMatches(RowLockingWrapper<ConsumerRow> consumerRow,
							List<ConsumerContainerRow> consumerContainerRowList,
							IDataModelTransaction dataModelTransaction)
		{
			//on debt side want one match per container row (consumerDebt)					
			foreach(ConsumerContainerRow containerRow in consumerContainerRowList)
			{
				List<FluidTrade.Core.Matching.CardSocialLastNameFuzzyMatcher.MatchResult> tmpList =
												this.ContraConsumerCross.FindMatch(new RowLockingWrapper<CreditCardRow>(containerRow.CreditCardRow, dataModelTransaction),
																					consumerRow, dataModelTransaction);
				if(tmpList != null && tmpList.Count > 0)
				{
					decimal maxStrength = 0M;
					FluidTrade.Core.Matching.CardSocialLastNameFuzzyMatcher.MatchResult maxResult = null;
					foreach(FluidTrade.Core.Matching.CardSocialLastNameFuzzyMatcher.MatchResult result in tmpList)
					{
						if(result.Strength > maxStrength)
						{
							maxStrength = result.Strength;
							maxResult = result;
						}
					}
					containerRow.MaxMatchResult = maxResult;

					if(containerRow.MaxMatchResult != null && containerRow.MaxMatchResult.MatchedConsumerRow != null)
					{
						ConsumerRow matchConsumerRow = containerRow.MaxMatchResult.MatchedConsumerRow;
						ConsumerTrustRow[] contraMatchRowAr;

						bool rowAlreadyLocked = matchConsumerRow.IsLockHeld(dataModelTransaction.TransactionId);
						if(rowAlreadyLocked == false)
							matchConsumerRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
						try
						{
							contraMatchRowAr = matchConsumerRow.GetConsumerTrustRows_NoLockCheck();
							if(contraMatchRowAr.Length == 0)
								continue;
						}
						finally
						{
							if(rowAlreadyLocked == false)
								matchConsumerRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
						}

						SecurityRow secRow;
						rowAlreadyLocked = contraMatchRowAr[0].IsLockHeld(dataModelTransaction.TransactionId);
						if(rowAlreadyLocked == false)
							 contraMatchRowAr[0].AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
						try
						{
							if(contraMatchRowAr[0].RowState == System.Data.DataRowState.Deleted ||
								contraMatchRowAr[0].RowState == System.Data.DataRowState.Detached)
								continue;
							
							secRow = contraMatchRowAr[0].SecurityRow_NoLockCheck;
						}
						finally
						{
							if(rowAlreadyLocked == false)
								contraMatchRowAr[0].ReleaseReaderLock(dataModelTransaction.TransactionId);
						}

						secRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
						try
						{
							if(secRow.RowState == System.Data.DataRowState.Deleted ||
								secRow.RowState == System.Data.DataRowState.Detached)
								continue;
						}
						finally
						{
							secRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
						}
						List<WorkingOrderRow> tmpWorkingOrderList = new List<WorkingOrderRow>();
						this.GetSubmittedWorkingOrderList(secRow, dataModelTransaction, tmpWorkingOrderList, null);
						if(tmpWorkingOrderList.Count == 0)
							continue;

						containerRow.ContraWorkingOrderRow = tmpWorkingOrderList[0];
					}
				}
			}
		}

		/// <summary>
		/// !!!RM this is just for testing should remove once we figure out deadlock issues
		/// abstract method to lock all the orders related to the match that is about to be operated on
		/// </summary>
		/// <param name="dataModelTransaction"></param>
		/// <param name="rawConsumerRow"></param>
		/// <param name="rawWorkingOrderRow"></param>
		/// <param name="rawCreditCardRow"></param>
		/// <returns></returns>
		internal override bool LockWorkingOrderBeforeMatch(DataModelTransaction dataModelTransaction,
																ConsumerRow rawConsumerRow,
																WorkingOrderRow rawWorkingOrderRow,
																CreditCardRow rawCreditCardRow)
		{
			if(rawConsumerRow == null)
			{
				Guid consumerId;
				if(rawWorkingOrderRow != null)
				{
					Guid secId;
					rawWorkingOrderRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
					try
					{
						if(rawWorkingOrderRow.RowState == DataRowState.Deleted ||
							rawWorkingOrderRow.RowState == DataRowState.Detached)
							return false;

						secId = rawWorkingOrderRow.SecurityId;
					}
					finally
					{
						rawWorkingOrderRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
					}

					ConsumerDebtRow cdRow = DataModel.ConsumerDebt.ConsumerDebtKey.Find(secId);
					if(cdRow == null)
						return false;


					cdRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
					try
					{
						if(cdRow.RowState == DataRowState.Deleted ||
							cdRow.RowState == DataRowState.Detached)
							return false;

						consumerId = cdRow.ConsumerId;
					}
					finally
					{
						cdRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
					}
				}
				else if(rawCreditCardRow != null)
				{
					rawCreditCardRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
					try
					{
						if(rawCreditCardRow.RowState == DataRowState.Deleted ||
							rawCreditCardRow.RowState == DataRowState.Detached)
							return false;

						consumerId = rawCreditCardRow.ConsumerId;
					}
					finally
					{
						rawCreditCardRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
					}
				}
				else
				{
					return false;
				}
				rawConsumerRow = DataModel.Consumer.ConsumerKey.Find(consumerId);
			}

			if(rawConsumerRow == null)
				return false;

			List<WorkingOrderRow> workingOrderToLock = new List<WorkingOrderRow>();
			rawConsumerRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
			try
			{
				if(rawConsumerRow.RowState == DataRowState.Deleted ||
					rawConsumerRow.RowState == DataRowState.Detached)
					return false;

				ConsumerDebtRow[] cdRows = rawConsumerRow.GetConsumerDebtRows_NoLockCheck();
				foreach(ConsumerDebtRow cdRow in cdRows)
				{
					SecurityRow secRow = null;
					cdRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
					try
					{
						if(cdRow.RowState == DataRowState.Deleted ||
							cdRow.RowState == DataRowState.Detached)
							continue;

						secRow = cdRow.SecurityRow;
						if(secRow == null)
							continue;
					}
					finally
					{
						cdRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
					}

					secRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
					try
					{
						if(secRow.RowState == DataRowState.Deleted ||
							secRow.RowState == DataRowState.Detached)
							continue;

						foreach(WorkingOrderRow workingOrderRow in secRow.GetWorkingOrderRowsByFK_Security_WorkingOrder_SecurityId())
						{
							workingOrderRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
							try
							{
								if(workingOrderRow.RowState == DataRowState.Deleted ||
									workingOrderRow.RowState == DataRowState.Detached)
									continue;
							
								workingOrderToLock.Add(workingOrderRow);
								foreach(MatchRow matchRow in workingOrderRow.GetMatchRows())
								{
									matchRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
									try
									{
										if(matchRow.RowState == DataRowState.Deleted ||
											matchRow.RowState == DataRowState.Detached)
											continue;

										WorkingOrderRow contraRow = DataModel.WorkingOrder.WorkingOrderKey.Find(matchRow.ContraOrderId);
										if(contraRow != null)
											workingOrderToLock.Add(workingOrderRow);
									}
									finally
									{
										matchRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
									}
								}
							}
							finally
							{
								workingOrderRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
							}


						}
					}
					finally
					{
						secRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
					}
				}
			}
			finally
			{
				rawConsumerRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
			}

			if(workingOrderToLock.Count == 0)
				return false;
			workingOrderToLock.Sort();
			foreach(WorkingOrderRow orderRow in workingOrderToLock)
			{
				try
				{
					orderRow.AcquireWriterLock(dataModelTransaction);
				}
				catch(Exception ex)
				{
					EventLog.Information("Deleted working order {0} {1}", this.ThreadName, ex);
				}
			}

			return true;
		}

		/// <summary>
		/// abstract method to determine if consumer is one that the side cares about
		/// this is called from the commit change event of the consumer row
		/// this is important to know because the consumer orderRow is locked
		/// and the datamodelTransaction can be tricky to use
		/// </summary>
		/// <param name="consumerRow"></param>
		/// <returns>true if consumer row is for this side</returns>
		protected override bool IsConsumerRowForSide_InChangeEvent(ConsumerRow consumerRow)
		{
			return consumerRow.GetConsumerDebtRows_NoLockCheck().Length > 0;
		}

		/// <summary>
		/// abstract method to determine if working order is one that the side cares about
		/// this is called from the commit change event of the workingOrder row
		/// this is important to know because the working orderRow is locked
		/// and the datamodelTransaction can be tricky to use
		/// </summary>
		/// <param name="row"></param>
		/// <returns>true if working order is for this side</returns>
		protected override bool IsWorkingOrderForSide_InChangeEvent(WorkingOrderRow row)
		{
			return ((FluidTrade.Core.ClusteredIndex)DataModel.ConsumerDebt.ConsumerDebtKey).Find(row[DataModel.WorkingOrder.SecurityIdColumn]) != null;
		}

		internal override ConsumerDebtMatchInfo CreateConsumerDebtInfo(ConsumerContainerRow containerRow, DataModel dataModel, DataModelTransaction dataModelTransaction)
		{
			return new ConsumerDebtMatchInfo(dataModelTransaction, containerRow.WorkingOrderRow);
		}

		internal override ConsumerTrustMatchInfo CreateConsumerTrustInfo(ConsumerContainerRow containerRow, DataModel dataModel, DataModelTransaction dataModelTransaction)
		{
			return new ConsumerTrustMatchInfo(dataModelTransaction, containerRow.ContraWorkingOrderRow);
		}

		internal override void CreateNewMatchWorkingOrderKeys(ConsumerCross.MatchCreateParms createParam, out ConsumerCross.PendingWorkingOrderKey key, out ConsumerCross.PendingWorkingOrderKey contraKey)
		{
			key = new PendingWorkingOrderKey(createParam.consumerDebtMatchInfo.WorkingOrderId, createParam.consumerTrustMatchInfo.WorkingOrderId);
			contraKey = new PendingWorkingOrderKey(createParam.consumerTrustMatchInfo.WorkingOrderId, createParam.consumerDebtMatchInfo.WorkingOrderId);
		}

		internal override void CreateNewMatch(DataModel dataModel, DataModelTransaction dataModelTransaction, MatchCreateParms matchCreateParam)
		{
			Status? status = this.GetStatus(matchCreateParam);
			
			// The Match is the basic association between two working orders that can participate in a transaction.
			dataModel.CreateMatch(
				matchCreateParam.consumerDebtMatchInfo.BlotterId,
				matchCreateParam.contraMatchId,
				matchCreateParam.consumerTrustMatchInfo.WorkingOrderId,
				matchCreateParam.containerRow.MaxMatchResult.Strength,
				string.Format("CD1,{0}", matchCreateParam.containerRow.MaxMatchResult.StrengthDetails),
				matchCreateParam.matchTime,
				matchCreateParam.matchId,
				StatusMap.FromCode((Status)status),
				matchCreateParam.consumerDebtMatchInfo.WorkingOrderId);

			// This record is used to negotiate the terms of a possible transaction.  In order to prevent information from leaking across the Chinese
			// Wall, a set of reflective records is made for each side of the trade.
			dataModel.CreateConsumerDebtNegotiation(
				matchCreateParam.consumerDebtMatchInfo.AccountBalance,
				matchCreateParam.consumerDebtMatchInfo.BlotterId,
				matchCreateParam.consumerDebtNegotiationId,
				matchCreateParam.consumerTrustMatchInfo.PaymentLength,
				matchCreateParam.consumerTrustMatchInfo.PaymentStartDateLength,
				matchCreateParam.consumerTrustMatchInfo.PaymentStartDateUnitId,
				matchCreateParam.consumerTrustMatchInfo.SettlementUnitId,
				matchCreateParam.consumerTrustMatchInfo.SettlementValue,
				matchCreateParam.createdTime,
				matchCreateParam.createdUserId,
				false,
				false,
				matchCreateParam.matchId,
				matchCreateParam.modifiedTime,
				matchCreateParam.modifiedUserId,
				matchCreateParam.consumerDebtMatchInfo.PaymentLength,
				matchCreateParam.consumerDebtMatchInfo.PaymentStartDateLength,
				matchCreateParam.consumerDebtMatchInfo.PaymentStartDateUnitId,
				matchCreateParam.consumerDebtMatchInfo.SettlementUnitId,
				matchCreateParam.consumerDebtMatchInfo.SettlementValue,
				StatusMap.FromCode(Status.New),
				out matchCreateParam.consumerDebtMatchInfo.Version);

			// The payment types are a vector and each of the payment methods needs to be copied into the newly created negotiation record and
			// associated with the Consumer Debt side of the negotiation.
			foreach(Guid paymentMethodTypeId in matchCreateParam.consumerDebtMatchInfo.PaymentMethodTypes)
			{
				dataModel.CreateConsumerDebtNegotiationOfferPaymentMethod(
					matchCreateParam.consumerDebtMatchInfo.BlotterId,
					matchCreateParam.consumerDebtNegotiationId,
					Guid.NewGuid(),
					paymentMethodTypeId);
			}

			// The payment types are a vector and each of the payment methods needs to be copied into the newly created negotiation record and
			// associated with the Consumer Trust side of the negotiation.
			foreach(Guid paymentMethodTypeId in matchCreateParam.consumerTrustMatchInfo.PaymentMethodTypes)
			{
				dataModel.CreateConsumerDebtNegotiationCounterPaymentMethod(
					matchCreateParam.consumerDebtMatchInfo.BlotterId,
					Guid.NewGuid(),
					matchCreateParam.consumerDebtNegotiationId,
					paymentMethodTypeId);
			}
		}


		internal override void CreateNewMatchFromContra(DataModel dataModel, DataModelTransaction dataModelTransaction, MatchCreateParms createParam)
		{
			Status? status = this.GetStatus(createParam);

			// This creates a reflective match for the other side of the possible transaction.
			dataModel.CreateMatch(
				createParam.consumerDebtMatchInfo.BlotterId,
				createParam.matchId,
				createParam.consumerTrustMatchInfo.WorkingOrderId,
				createParam.containerRow.MaxMatchResult.Strength,
				string.Format("CD1,{0}", createParam.containerRow.MaxMatchResult.StrengthDetails),
				createParam.matchTime,
				createParam.contraMatchId,
				StatusMap.FromCode((Status)status),
				createParam.consumerDebtMatchInfo.WorkingOrderId);

			// This record is used to negotiate the terms of a possible transaction.  In order to prevent information from leaking across the Chinese
			// Wall, a set of reflective records is made for each side of the trade.
			dataModel.CreateConsumerDebtNegotiation(
				createParam.consumerDebtMatchInfo.AccountBalance,
				createParam.consumerDebtMatchInfo.BlotterId,
				createParam.consumerDebtNegotiationId,
				createParam.consumerTrustMatchInfo.PaymentLength,
				createParam.consumerTrustMatchInfo.PaymentStartDateLength,
				createParam.consumerTrustMatchInfo.PaymentStartDateUnitId,
				createParam.consumerTrustMatchInfo.SettlementUnitId,
				createParam.consumerTrustMatchInfo.SettlementValue,
				createParam.createdTime,
				createParam.createdUserId,
				false,
				false,
				createParam.contraMatchId,
				createParam.modifiedTime,
				createParam.modifiedUserId,
				createParam.consumerDebtMatchInfo.PaymentLength,
				createParam.consumerDebtMatchInfo.PaymentStartDateLength,
				createParam.consumerDebtMatchInfo.PaymentStartDateUnitId,
				createParam.consumerDebtMatchInfo.SettlementUnitId,
				createParam.consumerDebtMatchInfo.SettlementValue,
				StatusMap.FromCode(Status.New),
				out createParam.consumerDebtMatchInfo.Version);

			// The payment types are a vector and each of the payment methods needs to be copied into the newly created negotiation record and 
			// associated with the Consumer Debt side of the negotiation.
			foreach(Guid paymentMethodTypeId in createParam.consumerDebtMatchInfo.PaymentMethodTypes)
			{
				dataModel.CreateConsumerDebtNegotiationOfferPaymentMethod(
					createParam.consumerDebtMatchInfo.BlotterId,
					createParam.consumerDebtNegotiationId,
					Guid.NewGuid(),
					paymentMethodTypeId);
			}

			// The payment types are a vector and each of the payment methods needs to be copied into the newly created negotiation record and 
			// associated with the Consumer Trust side of the negotiation.
			foreach(Guid paymentMethodTypeId in createParam.consumerTrustMatchInfo.PaymentMethodTypes)
			{
				dataModel.CreateConsumerDebtNegotiationCounterPaymentMethod(
					createParam.consumerDebtMatchInfo.BlotterId,
					Guid.NewGuid(),
					createParam.consumerDebtNegotiationId,
					paymentMethodTypeId);
			}
		}

		internal override bool UpdateMatch(DataModel dataModel, DataModelTransaction dataModelTransaction, MatchCreateParms matchCreateParam)
		{
			Guid matchId;
			Int64 matchRowVersion;
			
			

			Status? status = this.GetStatus(matchCreateParam);
			string newDetails = string.Format("CD1,{0}", matchCreateParam.containerRow.MaxMatchResult.StrengthDetails);

			//only update if something changed
			Guid statusCode = Guid.Empty;
			if(status != null)
				statusCode = StatusMap.FromCode((Status)status);
				
			// If the unique combination of WorkingOrder identifiers between the current party and the conter party already exists then the 
			// records are updated.  The current key and row version are required to update a match.
			bool rowAlreadyLocked = matchCreateParam.matchRow.IsLockHeld(dataModelTransaction.TransactionId);
			if(rowAlreadyLocked == false)
				matchCreateParam.matchRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
			try
			{
				matchId = matchCreateParam.matchRow.MatchId;
				matchRowVersion = matchCreateParam.matchRow.RowVersion;
				if(status == null || object.Equals(matchCreateParam.matchRow[DataModel.Match.StatusIdColumn], statusCode) &&
						object.Equals(matchCreateParam.matchRow[DataModel.Match.HeatIndexDetailsColumn], newDetails) &&
						object.Equals(matchCreateParam.matchRow[DataModel.Match.BlotterIdColumn], matchCreateParam.consumerDebtMatchInfo.BlotterId) &&
						object.Equals(matchCreateParam.matchRow[DataModel.Match.ContraOrderIdColumn], matchCreateParam.consumerTrustMatchInfo.WorkingOrderId) &&
						object.Equals(matchCreateParam.matchRow[DataModel.Match.WorkingOrderIdColumn], matchCreateParam.consumerDebtMatchInfo.WorkingOrderId))
					return false; //no update
			}
			finally
			{
				if(rowAlreadyLocked == false)
					matchCreateParam.matchRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
			}
			

			// The Match is the basic association between two working orders that can participate in a transaction.
			dataModel.UpdateMatch(
				matchCreateParam.consumerDebtMatchInfo.BlotterId,
				null,
				matchCreateParam.consumerTrustMatchInfo.WorkingOrderId,
				matchCreateParam.containerRow.MaxMatchResult.Strength,
				newDetails,
				matchCreateParam.matchTime,
				null,
				new Object[] { matchId },
				matchRowVersion,
				(status == null) ? null : (object)statusCode,
				matchCreateParam.consumerDebtMatchInfo.WorkingOrderId);

			return true;
		}


		internal override void UpdateMatchFromContra(DataModel dataModel, DataModelTransaction dataModelTransaction, MatchCreateParms createParam)
		{
			Guid contraMatchId;
			Int64 contraMatchRowVersion;
			
			// If the unique combination of WorkingOrder identifiers between the current party and the conter party already exists then the 
			// records are updated.  The current key and row version are required to update a match.
			MatchRow contraMatchRow = DataModel.Match.MatchKey.Find(createParam.contraMatchId);
			if(contraMatchRow == null)
			{
				EventLog.Information("cannot find contra matchRow {0} contraId:{1} matchId:{2}", this.ThreadName, createParam.contraMatchId, createParam.matchId);
				return;
			}

			bool rowAlreadyLocked = contraMatchRow.IsLockHeld(dataModelTransaction.TransactionId);
			if(rowAlreadyLocked == false)
				 contraMatchRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
			try
			{
				contraMatchId = createParam.contraMatchId;
				contraMatchRowVersion = contraMatchRow.RowVersion;
			}
			finally
			{
				if(rowAlreadyLocked == false)
					contraMatchRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
			}

			Status? status = this.GetStatus(createParam);

			// This creates a reflective match for the other side of the possible transaction.
			dataModel.UpdateMatch(
				createParam.consumerDebtMatchInfo.BlotterId,
				null,
				createParam.consumerTrustMatchInfo.WorkingOrderId,
				createParam.containerRow.MaxMatchResult.Strength,
				string.Format("CD1,{0}",createParam.containerRow.MaxMatchResult.StrengthDetails),
				createParam.matchTime,
				null,
				new Object[] { contraMatchId },
				contraMatchRowVersion,
				(status == null) ? null : (object)StatusMap.FromCode((Status)status),
				createParam.consumerDebtMatchInfo.WorkingOrderId);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="workingOrderRow"></param>
		/// <param name="dataModelTransaction"></param>
		/// <param name="workingOrderRowDeleted"></param>
		/// <returns></returns>
		protected override ConsumerRow GetConsumerRowFromWorkingOrderRow(WorkingOrderRow workingOrderRow, DataModelTransaction dataModelTransaction, out bool workingOrderRowDeleted)
		{
			workingOrderRowDeleted = false;
			ConsumerDebtRow consumerDebtRow;
			workingOrderRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
			try
			{
				DataRowState workingOrderRowState = workingOrderRow.RowState;
				if(workingOrderRowState == DataRowState.Deleted ||
					workingOrderRowState == DataRowState.Detached)
				{
					workingOrderRowDeleted = true;
					return null;
				}

				consumerDebtRow = DataModel.ConsumerDebt.ConsumerDebtKey.Find(workingOrderRow.SecurityId_NoLockCheck);
				if(consumerDebtRow == null)
					return null;
			}
			finally	
			{
				workingOrderRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
			}

				consumerDebtRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

				try
				{
					return consumerDebtRow.ConsumerRow_NoLockCheck;
				}
				finally
				{
					consumerDebtRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				}
			
		}


		/// <summary>
		/// A method for paying a settlement.
		/// </summary>
		public class ConsumerDebtNegotiationPaymentMethodInfo
		{

			/// <summary>
			/// The unique identifier of the payment method within a settlement.
			/// </summary>
			public Guid ConsumerDebtNegotiationPaymentMethodId;

			/// <summary>
			/// The unique identifier of the payment method.
			/// </summary>
			public Guid PaymentMethodTypeId;

			/// <summary>
			/// The Row Version of the payment record.
			/// </summary>
			public Int64 RowVersion;

			/// <summary>
			/// Construct a Payment Method record.
			/// </summary>
			/// <param name="consumerDebtNegotiationPaymentMethodId">The unique identifier of the payment method within a settlement.</param>
			/// <param name="paymentMethodTypeId">The unique identifier of the payment method.</param>
			/// <param name="rowVersion">The Row Version of the payment record.</param>
			public ConsumerDebtNegotiationPaymentMethodInfo(Guid consumerDebtNegotiationPaymentMethodId, Guid paymentMethodTypeId, Int64 rowVersion)
			{

				// Initialize the object
				this.ConsumerDebtNegotiationPaymentMethodId = consumerDebtNegotiationPaymentMethodId;
				this.PaymentMethodTypeId = paymentMethodTypeId;
				this.RowVersion = rowVersion;

			}

		}

		/// <summary>
		/// Information about the counter party in a negotiation.
		/// </summary>
		public class ConsumerDebtCrossInfo
		{

			/// <summary>
			/// The time the record was modified.
			/// </summary>
			public DateTime ModifiedTime;

			/// <summary>
			/// The Blotter to which this negotiation belongs.
			/// </summary>
			public Guid BlotterId;

			/// <summary>
			/// The last user who modified the negotiation.
			/// </summary>
			public Guid ModifiedUserId;

			/// <summary>
			/// The unique identifier of the negotiation.
			/// </summary>
			public Guid ConsumerDebtNegotiationId;

			/// <summary>
			/// The amount of time given to the consumer to pay.
			/// </summary>
			public Decimal CounterPaymentLength;

			/// <summary>
			/// The time until the consumer's payments start.
			/// </summary>
			public Decimal CounterPaymentStartDateLength;

			/// <summary>
			/// The units in which the starting time is quoted.
			/// </summary>
			public Guid CounterPaymentStartDateUnitId;

			/// <summary>
			/// The units of the settlement value.
			/// </summary>
			public Guid CounterSettlementUnitId;

			/// <summary>
			/// The settlement value.
			/// </summary>
			public Decimal CounterSettlementValue;

			/// <summary>
			/// The row version of the record.
			/// </summary>
			public Int64 ContraRowVersion;

			/// <summary>
			/// The payment types added to this negotiation.
			/// </summary>
			public List<Guid> AddedCounterPaymentMethodTypes;

			/// <summary>
			/// The payment types removed from this negotiation.
			/// </summary>
			public List<ConsumerDebtNegotiationPaymentMethodInfo> DeletedCounterPaymentMethodTypes;
			
			/// <summary>
			/// The payment types added to this negotiation.
			/// </summary>			
			public Guid MatchId;

			/// <summary>
			/// The payment types removed from this negotiation.
			/// </summary>
			public Guid? StatusId;

			/// <summary>
			/// Create a record to hold the counter offer information for a settlement.
			/// </summary>
			public ConsumerDebtCrossInfo()
			{

				// Initialize the object
				this.AddedCounterPaymentMethodTypes = new List<Guid>();
				this.DeletedCounterPaymentMethodTypes = new List<ConsumerDebtNegotiationPaymentMethodInfo>();

			}

		}	
	}
}
