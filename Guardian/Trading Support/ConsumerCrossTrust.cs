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
	public class ConsumerCrossTrust : ConsumerCross
	{
		/// <summary>
		/// 
		/// </summary>
		public ConsumerCrossTrust()
			:base()
		{
			//could have multiple threads to do the matching. and have
			//one instance of the consumerCross.  If do this
			//should have the first thread build the matcher
			//and the other threads should use it.
			instance = this;

			consumerCrossTrustCreationResetEvent.Set();

			lock(consumerCrossTrustCreationResetEventSyncObject)
			{
				ManualResetEvent tmpEvent = consumerCrossTrustCreationResetEvent;
				consumerCrossTrustCreationResetEvent = null;
				tmpEvent.Close();
			}
		}


		private static ManualResetEvent consumerCrossTrustCreationResetEvent = new ManualResetEvent(false);
		private static object consumerCrossTrustCreationResetEventSyncObject = new object();

		private static ConsumerCrossTrust instance;
		/// <summary>
		/// 
		/// </summary>
		public static ConsumerCrossTrust Instance
		{
			get
			{
				if(instance == null)
				{
					lock(consumerCrossTrustCreationResetEventSyncObject)
					{
						if(consumerCrossTrustCreationResetEvent != null)
							consumerCrossTrustCreationResetEvent.WaitOne();
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
			DataModel.ConsumerDebtNegotiation.ConsumerDebtNegotiationRowValidate += new ConsumerDebtNegotiationRowChangeEventHandler(ConsumerDebtNegotiationRowValidate);
			DataModel.ConsumerDebtNegotiationOfferPaymentMethod.ConsumerDebtNegotiationOfferPaymentMethodRowValidate += new ConsumerDebtNegotiationOfferPaymentMethodRowChangeEventHandler(ConsumerDebtNegotiationOfferPaymentMethodRowValidate);
		}

		/// <summary>
		/// 
		/// </summary>
		protected override void SubscribeToNegotiationTableEvents()
		{
			DataModel.ConsumerTrust.ConsumerTrustRowValidate += new ConsumerTrustRowChangeEventHandler(ConsumerTrust_ConsumerTrustRowValidate);
		}

		/// <summary>
		/// event to handle updating the savings balance or debtRuleId change
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ConsumerTrust_ConsumerTrustRowValidate(object sender, ConsumerTrustRowChangeEventArgs e)
		{
			DataRowState rowState = e.Row.RowState;
			
			//only looking for changes in the debtRule/debtClass/savingsBalance in the debtRule
			if(rowState == DataRowState.Deleted ||
				rowState == DataRowState.Detached ||
				(e.Row.HasVersion(DataRowVersion.Original) == false ||
				e.Row.HasVersion(DataRowVersion.Current) == false) ||
					(object.Equals(e.Row[DataModel.ConsumerTrust.SavingsBalanceColumn, DataRowVersion.Original], e.Row[DataModel.ConsumerTrust.SavingsBalanceColumn, DataRowVersion.Current]) &&
					object.Equals(e.Row[DataModel.ConsumerTrust.DebtRuleIdColumn, DataRowVersion.Original], e.Row[DataModel.ConsumerTrust.DebtRuleIdColumn, DataRowVersion.Current]))
					)
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
			ConsumerTrustRow[] consumerTrustRows = debtRuleRow.GetConsumerTrustRows();
			if(consumerTrustRows.Length == 0)
				return;

			DataModelTransaction dataModelTransaction = DataModelTransaction.Current;
			
			foreach(ConsumerTrustRow consumerTrustRow in consumerTrustRows)
			{
				consumerTrustRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

				try
				{
					ConsumerRow consumerRow = consumerTrustRow.ConsumerRow_NoLockCheck;
					if(consumerRow != null)
						retList.Add(consumerRow);
				}
				finally
				{
					consumerTrustRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
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
			return consumerTrustMatchInfo.SavingsBalance >= consumerDebtMatchInfo.AccountBalance * consumerTrustMatchInfo.SettlementValue;
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
		/// Validates that a Consumer Debt Negotiation offer is reflected in the counter party's offer.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
		private void ConsumerDebtNegotiationRowValidate(object sender, ConsumerDebtNegotiationRowChangeEventArgs e)
		{

			// This is the record that is to be validated.  The main idea of this trigger is to make sure that if anything has change in the offer, that the
			// counter party's negotiation record reflects the changes.
			ConsumerDebtNegotiationRow consumerDebtNegotiationRow = e.Row;
			if(consumerDebtNegotiationRow.RowState == DataRowState.Deleted ||
				consumerDebtNegotiationRow.RowState == DataRowState.Detached)
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
				if (!consumerDebtNegotiationRow[DataModel.ConsumerDebtNegotiation.BlotterIdColumn, DataRowVersion.Current].Equals(
					consumerDebtNegotiationRow[DataModel.ConsumerDebtNegotiation.BlotterIdColumn, DataRowVersion.Original]))
					isNegotiationChanged = false;

				// Check the state of the 'IsRead' flag.
				if (!consumerDebtNegotiationRow[DataModel.ConsumerDebtNegotiation.IsReadColumn, DataRowVersion.Current].Equals(
					consumerDebtNegotiationRow[DataModel.ConsumerDebtNegotiation.IsReadColumn, DataRowVersion.Original]))
					isNegotiationChanged = false;

				// Check the number of payments in the offer.
				if (!consumerDebtNegotiationRow[DataModel.ConsumerDebtNegotiation.OfferPaymentLengthColumn, DataRowVersion.Current].Equals(
					consumerDebtNegotiationRow[DataModel.ConsumerDebtNegotiation.OfferPaymentLengthColumn, DataRowVersion.Original]))
					isNegotiationChanged = true;

				// Check the amount of time until the first payment is made.
				if (!consumerDebtNegotiationRow[DataModel.ConsumerDebtNegotiation.OfferPaymentStartDateLengthColumn, DataRowVersion.Current].Equals(
					consumerDebtNegotiationRow[DataModel.ConsumerDebtNegotiation.OfferPaymentStartDateLengthColumn, DataRowVersion.Original]))
					isNegotiationChanged = true;

				// Check the units in which the time to start the payments is quoted.
				if (!consumerDebtNegotiationRow[DataModel.ConsumerDebtNegotiation.OfferPaymentStartDateUnitIdColumn, DataRowVersion.Current].Equals(
					consumerDebtNegotiationRow[DataModel.ConsumerDebtNegotiation.OfferPaymentStartDateUnitIdColumn, DataRowVersion.Original]))
					isNegotiationChanged = true;

				// Check the units in which the settlement amount is quoted.
				if (!consumerDebtNegotiationRow[DataModel.ConsumerDebtNegotiation.OfferSettlementUnitIdColumn, DataRowVersion.Current].Equals(
					consumerDebtNegotiationRow[DataModel.ConsumerDebtNegotiation.OfferSettlementUnitIdColumn, DataRowVersion.Original]))
					isNegotiationChanged = true;

				// Check the settlement amount.
				if (!consumerDebtNegotiationRow[DataModel.ConsumerDebtNegotiation.OfferSettlementValueColumn, DataRowVersion.Current].Equals(
					consumerDebtNegotiationRow[DataModel.ConsumerDebtNegotiation.OfferSettlementValueColumn, DataRowVersion.Original]))
					isNegotiationChanged = true;

				break;

			}

			// This condition prevents recursive updates from one side of the trade to the other and back again.  If the update came from the thread that
			// handles the background tasks for the trust, we don't need to execute any business rules.
			if (Thread.CurrentThread.Name != "ConsumerCrossDebt")
			{

				// This condition prevents an update of the counter party when just the 'IsRead' flag has changed.
				if (isNegotiationChanged)
				{

					// When the negoitiation is changed, make a call out to a thread that will synchronize the counter party to the new values. Note that this 
					// method is done on a seperate thread in order to simplify the locking that must be done.  There's no guarantee of the state of the locks
					// when this trigger is called, so any method called directly would need to check the state of every lock before it tried to use a record.
					this.actionQueue.Enqueue(
						new ObjectAction(
							CrossConsumerDebtNegotiation,
							new Object[] { consumerDebtNegotiationRow[DataModel.ConsumerDebtNegotiation.ConsumerDebtNegotiationIdColumn], e.Row }));

				}

			}

		}

		/// <summary>
		/// Validates that a Consumer Debt Negotiation Offer Payment Method is reflected in the counter party's offer.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
		private void ConsumerDebtNegotiationOfferPaymentMethodRowValidate(object sender, ConsumerDebtNegotiationOfferPaymentMethodRowChangeEventArgs e)
		{

			// This is the record that is to be validated.  The main idea of this trigger is to make sure that if anything has change in the list of payment
			// methods available on this offer, that the counter party's negotiation reflects the same payment methods.
			ConsumerDebtNegotiationOfferPaymentMethodRow consumerDebtNegotiationOfferPaymentMethodRow = e.Row;
			if(consumerDebtNegotiationOfferPaymentMethodRow.RowState == DataRowState.Deleted ||
				consumerDebtNegotiationOfferPaymentMethodRow.RowState == DataRowState.Detached)
				return;

			// Deleted records aren't easy to access.  The row version must be used to extract the parent record.  This variable will hold the parent record 
			// in all actions to the list of payment methods.
			Guid consumerDebtNegotiationId = Guid.Empty;

			// There is no need to update the counter party's list of payment methods if this is still false when all the conditions have been tested.
			Boolean isNegotiationChanged = false;

			switch(e.Action)
			{
				case DataRowAction.Add:

					// The counter party is updated when a payment method is added.
					isNegotiationChanged = true;
					consumerDebtNegotiationId = (Guid)consumerDebtNegotiationOfferPaymentMethodRow[
						DataModel.ConsumerDebtNegotiationOfferPaymentMethod.ConsumerDebtNegotiationIdColumn, DataRowVersion.Current];
					break;

				case DataRowAction.Delete:

					// The counter party is udpate when a payment method is deleted.
					isNegotiationChanged = true;
					consumerDebtNegotiationId = (Guid)consumerDebtNegotiationOfferPaymentMethodRow[
						DataModel.ConsumerDebtNegotiationOfferPaymentMethod.ConsumerDebtNegotiationIdColumn, DataRowVersion.Original];
					break;

			}

			// If any of the payment methods in the record have been changed, call out to a thread that will synchronize the counter party's payment methods 
			// to the new list. Note that this method is done on a seperate thread in order to simplify the locking that must be done.  There's no guarantee of
			// the state of the locks when this trigger is called, so any method called directly would need to check the state of every lock before it tried to
			// use a record.
			if(isNegotiationChanged)
				this.actionQueue.Enqueue(new ObjectAction(CrossConsumerDebtNegotiation, new Object[] { consumerDebtNegotiationId, e.Row }));

		}

		/// <summary>
		/// Evaluates whether a given working order is eligible for a cross with another order.
		/// </summary>		
		private void CrossConsumerDebtNegotiation(Object[] key, params Object[] parameters)
		{

			// An instance of the data model is required for CRUD operations.
			DataModel dataModel = new DataModel();

			// The locking model for the middle tier is optimized for performance.  The lightweight Reader/Writer Locks do not allow for recursive 'Reader'
			// locks or promotion of 'Reader' locks to 'Writer' locks.  Therefor, the programming model is to collect all the information required for a
			// transaction during a 'Read' phase into a data structure that is used during the 'Write' phase.  All 'Read' locks should be released by the time
			// the server logic is ready to write.
			ConsumerTrustCrossInfo consumerTrustCrossInfo = new ConsumerTrustCrossInfo();

			// The logic below will examine the order and see if a contra order is available for a match.  These values will indicate whether a match is
			// possible after all the locks have been released.
			Guid consumerDebtNegotiationId = (Guid)key[0];
			ConsumerDebtNegotiationRow consumerDebtNegotiationRow = key[1] as ConsumerDebtNegotiationRow;

			// A transaction is required to lock the records and change the data model.
			using(TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew, TimeSpan.FromHours(1)))
			{

				// This variable holds context information for the current transaction.
				DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

				// These variables are used for auditing the changes to this record.
				consumerTrustCrossInfo.ModifiedTime = DateTime.UtcNow;
				consumerTrustCrossInfo.ModifiedUserId = TradingSupport.UserId;

				MatchRow contraMatchRow = null;
				MatchRow consumerDebtNegotiationRowMatchRow = null;

				// This is the working order that will be tested for a possible buyer or seller (contra party).
				if(consumerDebtNegotiationRow == null)
					consumerDebtNegotiationRow = DataModel.ConsumerDebtNegotiation.ConsumerDebtNegotiationKey.Find(consumerDebtNegotiationId);
				if(consumerDebtNegotiationRow == null)
				{
					//this is can be a vaild case since another thread could delete the row
					return;
				}
				consumerDebtNegotiationRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
				try
				{
					//since another thread could delete the row need to check the validity of the row after it is locked
					if(consumerDebtNegotiationRow.RowState == DataRowState.Deleted ||
						consumerDebtNegotiationRow.RowState == DataRowState.Detached)
					{
						return;
					}

					consumerDebtNegotiationRowMatchRow = consumerDebtNegotiationRow.MatchRow;
					if(consumerDebtNegotiationRowMatchRow == null)
					{
						return;
					}
					consumerTrustCrossInfo.CounterPaymentLength = consumerDebtNegotiationRow.OfferPaymentLength;
					consumerTrustCrossInfo.CounterPaymentStartDateLength = consumerDebtNegotiationRow.OfferPaymentStartDateLength;
					consumerTrustCrossInfo.CounterPaymentStartDateUnitId = consumerDebtNegotiationRow.OfferPaymentStartDateUnitId;
					consumerTrustCrossInfo.CounterSettlementUnitId = consumerDebtNegotiationRow.OfferSettlementUnitId;
					consumerTrustCrossInfo.CounterSettlementValue = consumerDebtNegotiationRow.OfferSettlementValue;
					consumerTrustCrossInfo.StatusId = consumerDebtNegotiationRow.StatusId;
				}
				finally
				{
					consumerDebtNegotiationRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				}

				Guid consumerDebtNegotiationRowMatchRowContraMatchId;
				bool rowAlreadyLocked = consumerDebtNegotiationRowMatchRow.IsLockHeld(dataModelTransaction.TransactionId);
				if(rowAlreadyLocked == false)
					consumerDebtNegotiationRowMatchRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
				try
				{
					if(consumerDebtNegotiationRowMatchRow.RowState == DataRowState.Deleted ||
						consumerDebtNegotiationRowMatchRow.RowState == DataRowState.Detached)
					{
						return;
					}
					consumerDebtNegotiationRowMatchRowContraMatchId = consumerDebtNegotiationRowMatchRow.ContraMatchId;
				}
				finally
				{
					if(rowAlreadyLocked == false)
						consumerDebtNegotiationRowMatchRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				}

				ConsumerDebtNegotiationOfferPaymentMethodRow[] consumerDebtNegotiationOfferPaymentMethodRows;
				consumerDebtNegotiationRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
				try
				{
					consumerDebtNegotiationOfferPaymentMethodRows= consumerDebtNegotiationRow.GetConsumerDebtNegotiationOfferPaymentMethodRows();
				}
				finally
				{
					consumerDebtNegotiationRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				}


				// Extract the information from the negotiation record that's about to be modified by this new counter offer.  This info can't be taked
				// from the record when the call is made because the record must be locked for a write and there is no mechanism for a promotion.
				foreach(
					ConsumerDebtNegotiationOfferPaymentMethodRow consumerDebtNegotiationOfferPaymentMethodRow
					in consumerDebtNegotiationOfferPaymentMethodRows)
				{
					try
					{
						consumerDebtNegotiationOfferPaymentMethodRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
						consumerTrustCrossInfo.AddedCounterPaymentMethodTypes.Add(
							consumerDebtNegotiationOfferPaymentMethodRow.PaymentMethodTypeId);
					}
					finally
					{
						consumerDebtNegotiationOfferPaymentMethodRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
					}
				}

				// The PaymentMethodType is a vector that needs to be synchronized to the current offer.  This means making several passes at the data to
				// determine what is new, what has to be deleted and what hasn't changed.  These buckets are used to collect those payment methods that
				// apply to the counter offer.
				List<Guid> addedList = new List<Guid>();
				List<ConsumerTrustNegotiationPaymentMethodInfo> deletedList = new List<ConsumerTrustNegotiationPaymentMethodInfo>();

				// This is the counter party to the match.  There are no direct links between the order and the contra order due to the Chinese wall that
				// prevents data from one party being accessible to another party.
				contraMatchRow = DataModel.Match.MatchKey.Find(consumerDebtNegotiationRowMatchRowContraMatchId);
				if(contraMatchRow == null)
				{
					//this is can be a vaild case since another thread could delete the row
					return;
				}
				ConsumerTrustNegotiationRow[] consumerTrustNegotiationRows;
				rowAlreadyLocked = contraMatchRow.IsLockHeld(dataModelTransaction.TransactionId);
				if(rowAlreadyLocked == false)
					contraMatchRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
				try
				{
					if(contraMatchRow.RowState == DataRowState.Deleted ||
						contraMatchRow.RowState == DataRowState.Detached)
					{
						//this is can be a vaild case since another thread could delete the row
						return;
					}

					consumerTrustNegotiationRows = contraMatchRow.GetConsumerTrustNegotiationRows();
				}
				finally
				{
					if(rowAlreadyLocked == false)
						contraMatchRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				}
				// There is only one order on the other side of this offer but the iteration is a cleaner way to access it.  This will extract the
				// housekeeping values from the counter offer that are necessary to update it with the data from this order.
				foreach(ConsumerTrustNegotiationRow consumerTrustNegotiationRow in consumerTrustNegotiationRows)
				{
					ConsumerTrustNegotiationCounterPaymentMethodRow[] consumerTrustNegotiationCounterPaymentMethodRows;
					consumerTrustNegotiationRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
					try
					{
						// One of the compromises for speed in this data model is that the ReaderWriterLocks do not keep track of the number of times they
						// are called.  Also, Reader locks are not promoted to Writer locks.  The goal here is to call the internal method to update this
						// negotiation record with the information copied from the modified negotiation record.  To do this, the key and the row version
						// are needed.  The rub is, before the update method is called, all the locks on this record need to be released.
						consumerTrustCrossInfo.BlotterId = consumerTrustNegotiationRow.BlotterId;
						consumerTrustCrossInfo.ConsumerTrustNegotiationId = consumerTrustNegotiationRow.ConsumerTrustNegotiationId;
						consumerTrustCrossInfo.ContraRowVersion = consumerTrustNegotiationRow.RowVersion;
						consumerTrustCrossInfo.MatchId = consumerTrustNegotiationRow.MatchId;
						consumerTrustNegotiationCounterPaymentMethodRows = consumerTrustNegotiationRow.GetConsumerTrustNegotiationCounterPaymentMethodRows();
					}
					finally
					{
						consumerTrustNegotiationRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
					}

					//See if this is a rejected negotiation. Then regenerate a match.
					StatusRow statusRow = DataModel.Status.StatusKey.Find(consumerTrustCrossInfo.StatusId);
					try
					{
						statusRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
						if (statusRow.StatusCode == Status.Rejected)
						{
							MatchRow matchRow = DataModel.Match.MatchKey.Find(consumerTrustCrossInfo.MatchId);
							matchRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
							bool workingOderRowDeleted = false;
							try
							{

								ConsumerRow consumerRow = GetConsumerRowFromWorkingOrderRow(matchRow.WorkingOrderRow, dataModelTransaction,
									out workingOderRowDeleted);

								if (workingOderRowDeleted == false)
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
							ConsumerTrustNegotiationCounterPaymentMethodRow consumerTrustNegotiationCounterPaymentMethodRow
							in consumerTrustNegotiationCounterPaymentMethodRows)
						{

							// The main idea here is to collect the information about the payment methods already associated with the counter party
							// during the read phase.  These locks can't be in place when it is time to update the negotiation for the counter party.
							consumerTrustNegotiationCounterPaymentMethodRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
							try
							{

								// To optimize the number of writes to the disk, the payment methods that are already associated with the counter party's
								// negotiation record are left in place.  Only the new ones will be written.
								foreach(Guid paymentMethodTypeId in consumerTrustCrossInfo.AddedCounterPaymentMethodTypes)
								{
									if(consumerTrustNegotiationCounterPaymentMethodRow.PaymentMethodTypeId == paymentMethodTypeId)
									{
										addedList.Add(paymentMethodTypeId);
									}
								}

								// This list contains all the items currently in the counter party's negotiation record.  The list will be culled down 
								// after the collection is made to delete only the payment methods that are no longer part of the negotation.
								deletedList.Add(
									new ConsumerTrustNegotiationPaymentMethodInfo(
										consumerTrustNegotiationCounterPaymentMethodRow.ConsumerTrustNegotiationCounterPaymentMethodId,
										consumerTrustNegotiationCounterPaymentMethodRow.PaymentMethodTypeId,
										consumerTrustNegotiationCounterPaymentMethodRow.RowVersion));

							}
							finally
							{

								// It is important to release locks when they are no longer needed.
								consumerTrustNegotiationCounterPaymentMethodRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
							}

						}
				}

				// This loop will create a list of Payment Methods that need to be removed from the counter offer.
				foreach(ConsumerTrustNegotiationPaymentMethodInfo oldPaymentMethodInfo in deletedList)
				{

					// This will search the list of existing payment types trying to find the ones that are no longer part of the counter offer.
					Boolean isFound = false;
					foreach(Guid paymentMethodTypeId in consumerTrustCrossInfo.AddedCounterPaymentMethodTypes)
					{
						if(oldPaymentMethodInfo.PaymentMethodTypeId == paymentMethodTypeId)
							isFound = true;
					}

					// If the old payment type isn't part of the new list of payment types then it will be purged from the vector.
					if(!isFound)
						consumerTrustCrossInfo.DeletedCounterPaymentMethodTypes.Add(oldPaymentMethodInfo);

				}

				// This will remove any items that are already in the counter offer so they aren't added twice.
				foreach(Guid paymentMethodTypeId in addedList)
				{
					consumerTrustCrossInfo.AddedCounterPaymentMethodTypes.Remove(paymentMethodTypeId);
				}

				// At this point all the information has been collected to allow for an update of the counter party's negotiation to make it match this party's
				// offer.  In this way the bids and offers are communicated instantly to both parties.
				dataModel.UpdateConsumerTrustNegotiation(
					null,
					null,
					null,
					new Object[] { consumerTrustCrossInfo.ConsumerTrustNegotiationId },
					consumerTrustCrossInfo.CounterPaymentLength,
					consumerTrustCrossInfo.CounterPaymentStartDateLength,
					consumerTrustCrossInfo.CounterPaymentStartDateUnitId,
					consumerTrustCrossInfo.CounterSettlementUnitId,
					consumerTrustCrossInfo.CounterSettlementValue,
					null,
					null,
					null,
					false,
					true,
					null,
					consumerTrustCrossInfo.ModifiedTime,
					consumerTrustCrossInfo.ModifiedUserId,
					null,
					null,
					null,
					null,
					null,
					consumerTrustCrossInfo.ContraRowVersion,
					null,
					 null);

				// This will remove any payment metohds from the counter party's negotiation record that are not part of this party's negotiation record.
				foreach(ConsumerTrustNegotiationPaymentMethodInfo paymentMethodInfo in consumerTrustCrossInfo.DeletedCounterPaymentMethodTypes)
				{
					dataModel.DestroyConsumerTrustNegotiationCounterPaymentMethod(
						new Object[] { paymentMethodInfo.ConsumerTrustNegotiationPaymentMethodId },
						paymentMethodInfo.RowVersion);
				}

				// This will add only the new payment methods to the counter party's negotiation keeping it synchronized with this negotiation.
				foreach(Guid paymentMethodTypeId in consumerTrustCrossInfo.AddedCounterPaymentMethodTypes)
				{
					dataModel.CreateConsumerTrustNegotiationCounterPaymentMethod(
						consumerTrustCrossInfo.BlotterId,
						Guid.NewGuid(),
						consumerTrustCrossInfo.ConsumerTrustNegotiationId,
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
		protected override void GetAllValidConsumerAndCreditCardRows(List<RowLockingWrapper<ConsumerRow>> validConsumerRowList,
																		List<RowLockingWrapper<CreditCardRow>> validCreditCardRowList,
																		IDataModelTransaction dataModelTransaction)
		{
			//!!!RM 90% sure enumerating over the ConsumertTrust records is not threadsafe.. need to test
			DataModel.DataLock.EnterReadLock();
			List<ConsumerTrustRow> list;
			try
			{
				list = new List<ConsumerTrustRow>(DataModel.ConsumerTrust);
			}
			finally
			{
				DataModel.DataLock.ExitReadLock();
			}


			foreach(ConsumerTrustRow consumerTrustRow in list)
			{
				SecurityRow secRow;
				ConsumerRow curConsumerRow;
				consumerTrustRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
				try
				{
					curConsumerRow = consumerTrustRow.ConsumerRow;
					if(curConsumerRow == null)
						continue;

					secRow = consumerTrustRow.SecurityRow;
					if(secRow == null)
						continue;
				}
				finally
				{
					consumerTrustRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				}

				List<WorkingOrderRow> tmpWorkingOrderList = new List<WorkingOrderRow>();
				this.GetSubmittedWorkingOrderList(secRow, dataModelTransaction, tmpWorkingOrderList, null);
				if(tmpWorkingOrderList.Count == 0)
					continue;

				CreditCardRow[] creditCardAr;
				curConsumerRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
				try
				{
					creditCardAr = curConsumerRow.GetCreditCardRows();
				}
				finally
				{
					curConsumerRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				}
					
				validConsumerRowList.Add(new RowLockingWrapper<ConsumerRow>(curConsumerRow, dataModelTransaction));

				foreach(CreditCardRow curCreditCardRow in creditCardAr)
				{
					validCreditCardRowList.Add(new RowLockingWrapper<CreditCardRow>(curCreditCardRow, dataModelTransaction));
				}	
			}
		}

		/// <summary>
		/// 
		/// </summary>
		protected override string ThreadName
		{
			get { return "ConsumerCrossTrust"; }
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
			ConsumerTrustRow[] consumerTrustRowAr;
			CreditCardRow[] creditCardRowAr;
			consumerRow.AcquireReaderLock();
			try
			{
				consumerTrustRowAr = consumerRow.TypedRow.GetConsumerTrustRows_NoLockCheck();
				creditCardRowAr = consumerRow.TypedRow.GetCreditCardRows_NoLockCheck();
			}
			finally
			{
				consumerRow.ReleaseReaderLock();
			}

			if(creditCardRowAr.Length == 0)
			{
				consumerContainerRowList = null;
				return;
			}

			consumerContainerRowList = new List<ConsumerContainerRow>();

			foreach(ConsumerTrustRow consumerTrustRow in consumerTrustRowAr)
			{
				SecurityRow secRow;
				consumerTrustRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
				try
				{
					if(consumerTrustRow.RowState == System.Data.DataRowState.Deleted ||
					consumerTrustRow.RowState == System.Data.DataRowState.Detached)
					continue;
				
					secRow = consumerTrustRow.SecurityRow_NoLockCheck;
					if(secRow == null)
						continue;
				}
				finally
				{
					consumerTrustRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				}

				List<WorkingOrderRow> tmpWorkingOrderList = new List<WorkingOrderRow>();
				List<MatchRow[]> workingOrdersMatchRowsList = new List<MatchRow[]>();
				this.GetSubmittedWorkingOrderList(secRow, dataModelTransaction, tmpWorkingOrderList, workingOrdersMatchRowsList);
				if(tmpWorkingOrderList.Count == 0)
					continue;

				foreach(CreditCardRow creditCardRow in creditCardRowAr)
				{
					ConsumerContainerRow newConainerRow = new ConsumerContainerRow();
					newConainerRow.ContainerRow = consumerTrustRow;
					newConainerRow.WorkingOrderRow = tmpWorkingOrderList[0];
					newConainerRow.WorkingOrderRowMatchRowsList = workingOrdersMatchRowsList;
					newConainerRow.CreditCardRow = creditCardRow;

					consumerContainerRowList.Add(newConainerRow);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		protected override ConsumerCross ContraConsumerCross
		{
			get
			{
				return ConsumerCrossDebt.Instance;
			}
		}


		/// <summary>
		/// consumer row already locked
		/// </summary>
		/// <param name="consumerRow"></param>
		/// <param name="consumerContainerRowList"></param>
		/// <param name="dataModelTransaction"></param>
		/// <returns></returns>
		protected override void FindMaxContraMatches(RowLockingWrapper<ConsumerRow> consumerRow,
							List<ConsumerContainerRow> consumerContainerRowList,
							IDataModelTransaction dataModelTransaction)
		{
			
			Dictionary<CreditCardRow, FluidTrade.Core.Matching.CardSocialLastNameFuzzyMatcher.MatchResult> creditCardToMaxMap = new Dictionary<CreditCardRow, FluidTrade.Core.Matching.CardSocialLastNameFuzzyMatcher.MatchResult>();
			//will be one of these for each credit card eventhought there is only one for all in the db
			foreach(ConsumerContainerRow containerRow in consumerContainerRowList)
			{
				
				List<FluidTrade.Core.Matching.CardSocialLastNameFuzzyMatcher.MatchResult> tmpList = 
									this.ContraConsumerCross.FindMatch(new RowLockingWrapper<CreditCardRow>(containerRow.CreditCardRow, dataModelTransaction),
																										consumerRow, dataModelTransaction);
				if(tmpList != null && tmpList.Count > 0)
				{
					decimal maxStrength = 0M;
					FluidTrade.Core.Matching.CardSocialLastNameFuzzyMatcher.MatchResult maxResult = null;
					//on trust side only want one match per credit card
					foreach(FluidTrade.Core.Matching.CardSocialLastNameFuzzyMatcher.MatchResult result in tmpList)
					{
						if(result.Strength > maxStrength)
						{
							maxStrength = result.Strength;
							maxResult = result;
						}
					}

					FluidTrade.Core.Matching.CardSocialLastNameFuzzyMatcher.MatchResult existingMatch;
					
					//can only match to one credit card
					if(creditCardToMaxMap.TryGetValue(maxResult.MatchedCreditCardRow, out existingMatch) == false ||
						existingMatch.Strength < maxResult.Strength)
					{
						creditCardToMaxMap[maxResult.MatchedCreditCardRow] = maxResult;
					}
				}
			}

			List<ConsumerContainerRow> goodContainerRowList = new List<ConsumerContainerRow>();

			foreach(ConsumerContainerRow containerRow in consumerContainerRowList)
			{
				foreach(KeyValuePair<CreditCardRow, FluidTrade.Core.Matching.CardSocialLastNameFuzzyMatcher.MatchResult> pair in creditCardToMaxMap)
				{
					if(containerRow.CreditCardRow == pair.Value.CreditCardRowToFind)
					{
						containerRow.MaxMatchResult = pair.Value;

						if(containerRow.MaxMatchResult != null &&
							containerRow.MaxMatchResult.MatchedConsumerRow != null)
						{
							CreditCardRow matchCreditCardRow = containerRow.MaxMatchResult.MatchedCreditCardRow;
							ConsumerDebtRow[] contraMatchRowAr;
							matchCreditCardRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
							try
							{
								contraMatchRowAr = matchCreditCardRow.GetConsumerDebtRows_NoLockCheck();
							}
							finally
							{
								matchCreditCardRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
							}

							if(contraMatchRowAr.Length == 0)
								continue;

							ConsumerDebtRow contraDebtRow = contraMatchRowAr[0];
							SecurityRow secRow;

							contraDebtRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
							try
							{
								if(contraDebtRow.RowState == System.Data.DataRowState.Deleted ||
									contraDebtRow.RowState == System.Data.DataRowState.Detached)
									continue;

								secRow = contraDebtRow.SecurityRow_NoLockCheck;
							}
							finally
							{
								contraDebtRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
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
							goodContainerRowList.Add(containerRow);
						}
					}
				}
			}

			consumerContainerRowList.Clear();
			consumerContainerRowList.AddRange(goodContainerRowList);
		}

		/// <summary>
		/// !!!RM this is just for testing should remove once we figure out deadlock issues
		/// abstract method to lock all the orders related to the match that is about to be operated on
		/// 
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

					ConsumerTrustRow ctRow = DataModel.ConsumerTrust.ConsumerTrustKey.Find(secId);
					if(ctRow == null)
						return false;


					ctRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
					try
					{
						if(ctRow.RowState == DataRowState.Deleted ||
							ctRow.RowState == DataRowState.Detached)
							return false;

						consumerId = ctRow.ConsumerId;
					}
					finally
					{
						ctRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
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

				ConsumerTrustRow[] ctRows = rawConsumerRow.GetConsumerTrustRows_NoLockCheck();
				foreach(ConsumerTrustRow ctRow in ctRows)
				{
					SecurityRow secRow = null;
					ctRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
					try
					{
						if(ctRow.RowState == DataRowState.Deleted ||
							ctRow.RowState == DataRowState.Detached)
							continue;

						secRow = ctRow.SecurityRow;
						if(secRow == null)
							continue;
					}
					finally
					{
						ctRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
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
			return consumerRow.GetConsumerTrustRows_NoLockCheck().Length > 0;
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
			return ((FluidTrade.Core.ClusteredIndex)DataModel.ConsumerTrust.ConsumerTrustKey).Find(row[DataModel.WorkingOrder.SecurityIdColumn]) != null;
		}

		internal override ConsumerDebtMatchInfo CreateConsumerDebtInfo(ConsumerContainerRow containerRow, DataModel dataModel, DataModelTransaction dataModelTransaction)
		{
			return new ConsumerDebtMatchInfo(dataModelTransaction, containerRow.ContraWorkingOrderRow);
		}

		internal override ConsumerTrustMatchInfo CreateConsumerTrustInfo(ConsumerContainerRow containerRow, DataModel dataModel, DataModelTransaction dataModelTransaction)
		{
			return new ConsumerTrustMatchInfo(dataModelTransaction, containerRow.WorkingOrderRow);
		}

		internal override void CreateNewMatch(DataModel dataModel, DataModelTransaction dataModelTransaction, MatchCreateParms matchCreateParam)
		{
			Guid trustCreditCardRowId;
			matchCreateParam.containerRow.CreditCardRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
			try
			{
				trustCreditCardRowId = matchCreateParam.containerRow.CreditCardRow.CreditCardId_NoLockCheck;
			}
			finally
			{
				matchCreateParam.containerRow.CreditCardRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
			}

			Status? status = this.GetStatus(matchCreateParam);



			// The Match is the basic association between two working orders that can participate in a transaction.
			dataModel.CreateMatch(
				matchCreateParam.consumerTrustMatchInfo.BlotterId,
				matchCreateParam.contraMatchId,
				matchCreateParam.consumerDebtMatchInfo.WorkingOrderId,
				matchCreateParam.containerRow.MaxMatchResult.Strength,
				string.Format("CT1,{0}", matchCreateParam.containerRow.MaxMatchResult.StrengthDetails),
				matchCreateParam.matchTime,
				matchCreateParam.matchId,
				StatusMap.FromCode((Status)status),
				matchCreateParam.consumerTrustMatchInfo.WorkingOrderId);


			
			
			// This record is used to negotiate the terms of a possible transaction.  In order to prevent information from leaking across the Chinese
			// Wall, a set of reflective records is made for each side of the trade.
			System.Diagnostics.Debug.WriteLine("CreateConsumerTrustNegotiation from CreateNewMatch");
			dataModel.CreateConsumerTrustNegotiation(
				matchCreateParam.consumerDebtMatchInfo.AccountBalance,
				matchCreateParam.consumerTrustMatchInfo.BlotterId,
				matchCreateParam.consumerTrustNegotiationId,
				matchCreateParam.consumerDebtMatchInfo.PaymentLength,
				matchCreateParam.consumerDebtMatchInfo.PaymentStartDateLength,
				matchCreateParam.consumerDebtMatchInfo.PaymentStartDateUnitId,
				matchCreateParam.consumerDebtMatchInfo.SettlementUnitId,
				matchCreateParam.consumerDebtMatchInfo.SettlementValue,
				matchCreateParam.createdTime,
				matchCreateParam.createdUserId,
				trustCreditCardRowId,
				false,
				false,
				matchCreateParam.matchId,
				matchCreateParam.modifiedTime,
				matchCreateParam.modifiedUserId,
				matchCreateParam.consumerTrustMatchInfo.PaymentLength,
				matchCreateParam.consumerTrustMatchInfo.PaymentStartDateLength,
				matchCreateParam.consumerTrustMatchInfo.PaymentStartDateUnitId,
				matchCreateParam.consumerTrustMatchInfo.SettlementUnitId,
				matchCreateParam.consumerTrustMatchInfo.SettlementValue,
				StatusMap.FromCode(Status.New),
				out matchCreateParam.consumerDebtMatchInfo.Version);

			// The payment types are a vector and each of the payment methods needs to be copied into the newly created negotiation record and
			// associated with the Consumer Trust side of the negotiation.			
			foreach(Guid paymentMethodTypeId in matchCreateParam.consumerTrustMatchInfo.PaymentMethodTypes)
			{
				dataModel.CreateConsumerTrustNegotiationOfferPaymentMethod(
					matchCreateParam.consumerTrustMatchInfo.BlotterId,
					matchCreateParam.consumerTrustNegotiationId,
					Guid.NewGuid(),
					paymentMethodTypeId);
			}

			// The payment types are a vector and each of the payment methods needs to be copied into the newly created negotiation record and
			// associated with the Consumer Trust side of the negotiation.
			foreach(Guid paymentMethodTypeId in matchCreateParam.consumerDebtMatchInfo.PaymentMethodTypes)
			{
				dataModel.CreateConsumerTrustNegotiationCounterPaymentMethod(
					matchCreateParam.consumerTrustMatchInfo.BlotterId,
					Guid.NewGuid(),
					matchCreateParam.consumerTrustNegotiationId,
					paymentMethodTypeId);
			}
			
		}

		internal override void CreateNewMatchWorkingOrderKeys(ConsumerCross.MatchCreateParms createParam, out ConsumerCross.PendingWorkingOrderKey key, out ConsumerCross.PendingWorkingOrderKey contraKey)
		{
			key = new PendingWorkingOrderKey(createParam.consumerTrustMatchInfo.WorkingOrderId, createParam.consumerDebtMatchInfo.WorkingOrderId);
			contraKey = new PendingWorkingOrderKey(createParam.consumerDebtMatchInfo.WorkingOrderId, createParam.consumerTrustMatchInfo.WorkingOrderId);
		}

		internal override void CreateNewMatchFromContra(DataModel dataModel, DataModelTransaction dataModelTransaction, MatchCreateParms createParam)
		{
			Guid trustCreditCardRowId;
			createParam.containerRow.MaxMatchResult.MatchedCreditCardRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
			try
			{
				trustCreditCardRowId = createParam.containerRow.MaxMatchResult.MatchedCreditCardRow.CreditCardId_NoLockCheck;
			}
			finally
			{
				createParam.containerRow.MaxMatchResult.MatchedCreditCardRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
			}

			Status? status = this.GetStatus(createParam);

			/*
			 * !!!RM do create the match row*/
			// This creates a reflective match for the other side of the possible transaction.
			dataModel.CreateMatch(
				createParam.consumerTrustMatchInfo.BlotterId,
				createParam.matchId,
				createParam.consumerDebtMatchInfo.WorkingOrderId,
				createParam.containerRow.MaxMatchResult.Strength,
				string.Format("CT1,{0}", createParam.containerRow.MaxMatchResult.StrengthDetails),
				createParam.matchTime,
				createParam.contraMatchId,
				StatusMap.FromCode((Status)status),
				createParam.consumerTrustMatchInfo.WorkingOrderId);

			// This record is used to negotiate the terms of a possible transaction.  In order to prevent information from leaking across the Chinese
			// Wall, a set of reflective records is made for each side of the trade.
			dataModel.CreateConsumerTrustNegotiation(
				createParam.consumerDebtMatchInfo.AccountBalance,
				createParam.consumerTrustMatchInfo.BlotterId,
				createParam.consumerTrustNegotiationId,
				createParam.consumerDebtMatchInfo.PaymentLength,
				createParam.consumerDebtMatchInfo.PaymentStartDateLength,
				createParam.consumerDebtMatchInfo.PaymentStartDateUnitId,
				createParam.consumerDebtMatchInfo.SettlementUnitId,
				createParam.consumerDebtMatchInfo.SettlementValue,
				createParam.createdTime,
				createParam.createdUserId,
				trustCreditCardRowId,
				false,
				false,
				createParam.contraMatchId,
				createParam.modifiedTime,
				createParam.modifiedUserId,
				createParam.consumerTrustMatchInfo.PaymentLength,
				createParam.consumerTrustMatchInfo.PaymentStartDateLength,
				createParam.consumerTrustMatchInfo.PaymentStartDateUnitId,
				createParam.consumerTrustMatchInfo.SettlementUnitId,
				createParam.consumerTrustMatchInfo.SettlementValue,
				StatusMap.FromCode(Status.New),
				out createParam.consumerDebtMatchInfo.Version);

			// The payment types are a vector and each of the payment methods needs to be copied into the newly created negotiation record and 
			// associated with the Consumer Trust side of the negotiation.
			foreach(Guid paymentMethodTypeId in createParam.consumerTrustMatchInfo.PaymentMethodTypes)
			{
				dataModel.CreateConsumerTrustNegotiationOfferPaymentMethod(
					createParam.consumerTrustMatchInfo.BlotterId,
					createParam.consumerTrustNegotiationId,
					Guid.NewGuid(),
					paymentMethodTypeId);
			}

			// The payment types are a vector and each of the payment methods needs to be copied into the newly created negotiation record and 
			// associated with the Consumer Trust side of the negotiation.
			foreach(Guid paymentMethodTypeId in createParam.consumerDebtMatchInfo.PaymentMethodTypes)
			{
				dataModel.CreateConsumerTrustNegotiationCounterPaymentMethod(
					createParam.consumerTrustMatchInfo.BlotterId,
					Guid.NewGuid(),
					createParam.consumerTrustNegotiationId,
					paymentMethodTypeId);
			}
		}

		internal override bool UpdateMatch(DataModel dataModel, DataModelTransaction dataModelTransaction, MatchCreateParms matchCreateParam)
		{
			Guid matchId;
			Int64 matchRowVersion;	

			Status? status = this.GetStatus(matchCreateParam);
			string newDetails = string.Format("CT1,{0}", matchCreateParam.containerRow.MaxMatchResult.StrengthDetails);
			
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
						object.Equals(matchCreateParam.matchRow[DataModel.Match.BlotterIdColumn], matchCreateParam.consumerTrustMatchInfo.BlotterId) &&
						object.Equals(matchCreateParam.matchRow[DataModel.Match.ContraOrderIdColumn], matchCreateParam.consumerDebtMatchInfo.WorkingOrderId) &&
						object.Equals(matchCreateParam.matchRow[DataModel.Match.WorkingOrderIdColumn], matchCreateParam.consumerTrustMatchInfo.WorkingOrderId))
					return false; //no update
			}
			finally
			{
				if(rowAlreadyLocked == false)
					matchCreateParam.matchRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
			}
			

			// The Match is the basic association between two working orders that can participate in a transaction.
			dataModel.UpdateMatch(
				matchCreateParam.consumerTrustMatchInfo.BlotterId,
				null,
				matchCreateParam.consumerDebtMatchInfo.WorkingOrderId,
				matchCreateParam.containerRow.MaxMatchResult.Strength,
				newDetails,
				matchCreateParam.matchTime,
				null,
				new Object[] { matchId },
				matchRowVersion,
				(status == null) ? null : (object)statusCode,
				matchCreateParam.consumerTrustMatchInfo.WorkingOrderId);

			return true;
		}


		internal override void UpdateMatchFromContra(DataModel dataModel, DataModelTransaction dataModelTransaction, MatchCreateParms createParam)
		{
			Guid contraMatchId;
			Int64 contraMatchRowVersion;

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
				createParam.consumerTrustMatchInfo.BlotterId,
				null,
				createParam.consumerDebtMatchInfo.WorkingOrderId,
				createParam.containerRow.MaxMatchResult.Strength,
				string.Format("CT1,{0}", createParam.containerRow.MaxMatchResult.StrengthDetails),
				createParam.matchTime,
				null,
				new Object[] { contraMatchId },
				contraMatchRowVersion,
				(status == null) ? null : (object)StatusMap.FromCode((Status)status),
				createParam.consumerTrustMatchInfo.WorkingOrderId);
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
			ConsumerTrustRow consumerTrustRow;
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
				consumerTrustRow = DataModel.ConsumerTrust.ConsumerTrustKey.Find(workingOrderRow.SecurityId_NoLockCheck);

				if(consumerTrustRow == null)
					return null;
			}
			finally
			{
				workingOrderRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
			}

				consumerTrustRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
				try
				{
					return consumerTrustRow.ConsumerRow_NoLockCheck;
				}
				finally
				{
					consumerTrustRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				}
			
		}

		/// <summary>
		/// A method for paying a settlement.
		/// </summary>
		public class ConsumerTrustNegotiationPaymentMethodInfo
		{

			/// <summary>
			/// The unique identifier of the payment method within a settlement.
			/// </summary>
			public Guid ConsumerTrustNegotiationPaymentMethodId;

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
			/// <param name="consumerTrustNegotiationPaymentMethodId">The unique identifier of the payment method within a settlement.</param>
			/// <param name="paymentMethodTypeId">The unique identifier of the payment method.</param>
			/// <param name="rowVersion">The Row Version of the payment record.</param>
			public ConsumerTrustNegotiationPaymentMethodInfo(Guid consumerTrustNegotiationPaymentMethodId, Guid paymentMethodTypeId, Int64 rowVersion)
			{

				// Initialize the object
				this.ConsumerTrustNegotiationPaymentMethodId = consumerTrustNegotiationPaymentMethodId;
				this.PaymentMethodTypeId = paymentMethodTypeId;
				this.RowVersion = rowVersion;

			}

		}

		/// <summary>
		/// Information about the counter party in a negotiation.
		/// </summary>
		public class ConsumerTrustCrossInfo
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
			public Guid ConsumerTrustNegotiationId;

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
			/// The units of the settlement value.
			/// </summary>
			public Guid MatchId;

			/// <summary>
			/// The units of the settlement value.
			/// </summary>
			public Guid StatusId;

			/// <summary>
			/// The payment types added to this negotiation.
			/// </summary>
			public List<Guid> AddedCounterPaymentMethodTypes;

			/// <summary>
			/// The payment types removed from this negotiation.
			/// </summary>
			public List<ConsumerTrustNegotiationPaymentMethodInfo> DeletedCounterPaymentMethodTypes;

			/// <summary>
			/// Create a record to hold the counter offer information for a settlement.
			/// </summary>
			public ConsumerTrustCrossInfo()
			{

				// Initialize the object
				this.AddedCounterPaymentMethodTypes = new List<Guid>();
				this.DeletedCounterPaymentMethodTypes = new List<ConsumerTrustNegotiationPaymentMethodInfo>();

			}

		}
	}
}
