namespace FluidTrade.Guardian
{

	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Data.SqlClient;
	using System.Security.Principal;
	using System.Threading;
	using System.Transactions;
	using FluidTrade.Core;
	using FluidTrade.Core.Matching;

	/// <summary>Finds matching orders and negotiations the exchange of assets.</summary>
	/// <copyright>Copyright (C) 1998-2005 Fluid Trade -- All Rights Reserved.</copyright>	
	public abstract class ConsumerCross :IExchange
	{
		// Private Constants
		internal const Int32 threadWait = 100;
		internal const Int32 deadlockRetiesMax = 3;

		// Private Static Methods
		internal static Decimal DefaultMatchThreshold = 0.70M;
		internal WaitQueue<ObjectAction> actionQueue;
		//internal ClaimsPrincipal claimsPrincipal;
		internal Thread crossingThread;
		private static Dictionary<PendingWorkingOrderKey, bool> pendingMatchMap;


		internal const int CrossingThreadSleep = 2000;

		/// <summary>
		/// 
		/// </summary>
		protected static readonly Guid SubmittedOrderStatus;

		/// <summary>
		/// 
		/// </summary>
		protected static readonly Guid ValidMatchFundsStatus;

		/// <summary>
		/// 
		/// </summary>
		protected static readonly Guid ValidMatchStatus;

		/// <summary>
		/// 
		/// </summary>
		protected static readonly Guid PartialMatchStatus;


		static ConsumerCross()
		{
			SubmittedOrderStatus = StatusMap.FromCode(Status.Submitted);
			PartialMatchStatus = StatusMap.FromCode(Status.PartialMatch);
			ValidMatchStatus = StatusMap.FromCode(Status.ValidMatch);
			ValidMatchFundsStatus = StatusMap.FromCode(Status.ValidMatchFunds);
			pendingMatchMap = new Dictionary<PendingWorkingOrderKey, bool>();
		}
		/// <summary>
		/// A method for paying a settlement.
		/// </summary>
		protected abstract class ConsumerNegotiationPaymentMethodInfo
		{
			/// <summary>
			/// The unique identifier of the payment method within a settlement.
			/// </summary>
			public Guid ConsumerPaymentMethodId;


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
			/// <param name="consumerPaymentMethodId">The unique identifier of the payment method within a settlement.</param>
			/// <param name="paymentMethodTypeId">The unique identifier of the payment method.</param>
			/// <param name="rowVersion">The Row Version of the payment record.</param>
			protected ConsumerNegotiationPaymentMethodInfo(Guid consumerPaymentMethodId, Guid paymentMethodTypeId, Int64 rowVersion)
			{
				// Initialize the object
				this.ConsumerPaymentMethodId = consumerPaymentMethodId;
				this.PaymentMethodTypeId = paymentMethodTypeId;
				this.RowVersion = rowVersion;
			}
		}

		/// <summary>
		/// Information about the counter party in a negotiation.
		/// </summary>
		protected abstract class ConsumerCrossInfo
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
			public Guid ConsumerNegotiationId;

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
			public List<ConsumerNegotiationPaymentMethodInfo> DeletedCounterPaymentMethodTypes;

			/// <summary>
			/// Create a record to hold the counter offer information for a settlement.
			/// </summary>
			protected ConsumerCrossInfo()
			{

				// Initialize the object
				this.AddedCounterPaymentMethodTypes = new List<Guid>();
				this.DeletedCounterPaymentMethodTypes = new List<ConsumerNegotiationPaymentMethodInfo>();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		protected ConsumerCross()
		{
			// This queue is filled up with Working Orders that need to be serviced because something changed the matching criteria.
			this.actionQueue = new WaitQueue<ObjectAction>();
		}

		/// <summary>
		/// 
		/// </summary>
		protected abstract void SubscribeToContraNegotiationTableEvents();

		/// <summary>
		/// 
		/// </summary>
		protected abstract void SubscribeToNegotiationTableEvents();

		private CardSocialLastNameFuzzyMatcher matcher;
		private ManualResetEvent matcherCreationResetEvent = new ManualResetEvent(false);
		private object matcherCreationResetEventSyncObject = new object();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="creditCardRowToFind"></param>
		/// <param name="consumerRowToFind"></param>
		/// <param name="dataModelTransaction"></param>
		/// <returns></returns>
		public List<FluidTrade.Core.Matching.CardSocialLastNameFuzzyMatcher.MatchResult> FindMatch(RowLockingWrapper<CreditCardRow> creditCardRowToFind, 
											RowLockingWrapper<ConsumerRow> consumerRowToFind,
											IDataModelTransaction dataModelTransaction)
		{
			return Matcher.FindMatch(creditCardRowToFind, consumerRowToFind, dataModelTransaction, DefaultMatchThreshold);	
		}

		/// <summary>
		/// 
		/// </summary>
		protected CardSocialLastNameFuzzyMatcher Matcher
		{
			get
			{
				if(matcher == null)
				{
					lock(this.matcherCreationResetEventSyncObject)
					{
						if(this.matcherCreationResetEvent != null)
							matcherCreationResetEvent.WaitOne();
					}
				}

				return matcher;
			}
		}

		/// <summary>
		/// Pulls actions and their parameters off the queue and executes them.
		/// </summary>
		protected void CrossingThread()
		{

			// All the actions added to the generic list of actions and parameter will execute with this claims principal.
			Thread.CurrentPrincipal = new ClaimsPrincipal(WindowsIdentity.GetCurrent(), null);

			//using (Impersonator imp = new Impersonator(BuiltInAccount.NetworkService))
			Thread.CurrentPrincipal = new ClaimsPrincipal(new GenericIdentity("NT AUTHORITY\\NETWORK SERVICE"), null);

			Thread.Sleep(CrossingThreadSleep);

			try
			{
				this.GetMatcher();
			}
			catch (Exception exception)
			{

				// This will catch any exceptions thrown during the processing of the generic actions.
				EventLog.Error("Error Creating Matcher\r\n{0} {1}: {2}", this.ThreadName, exception.Message, exception.StackTrace);
				return;
			}

			// The event handlers for the data model can't wait on locks and resources outside the data model.  There would simply be too many resources that 
			// could deadlock.  This code will pull requests off of a generic queue of actions and parameters and execute them using the authentication created
			// above.
			while (true)
			{
				if (this.actionQueue.Count == 0)
					Thread.Sleep(CrossingThreadSleep);
				try
				{
					// The thread will wait here until an action has been placed in the queue to be processed in this thread context.
					ObjectAction objectAction = this.actionQueue.Dequeue();
					objectAction.DoAction(objectAction.Key, objectAction.Parameters);

				}
				catch (ThreadAbortException)
				{
                    return;
				}
				catch (Exception exception)
				{

					// This will catch any exceptions thrown during the processing of the generic actions.
					EventLog.Error("{0} {1}: {2}", this.ThreadName, exception.Message, exception.StackTrace);

				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		protected virtual void GetMatcher()
		{
			//build the matcher
			List<RowLockingWrapper<ConsumerRow>> validConsumerRowList = new List<RowLockingWrapper<ConsumerRow>>();
			List<RowLockingWrapper<CreditCardRow>> validCreditCardRowList = new List<RowLockingWrapper<CreditCardRow>>();

			using(TransactionScope transactionScope = new TransactionScope())
			{
				// This provides a context for any transactions.
				DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

				this.GetAllValidConsumerAndCreditCardRows(validConsumerRowList, validCreditCardRowList, dataModelTransaction);

				CardSocialLastNameFuzzyMatcher tmpMatcher = new CardSocialLastNameFuzzyMatcher(DataModel.CreditCard, DataModel.Consumer,
																	validCreditCardRowList, validConsumerRowList, dataModelTransaction);

				//dont set to memberVar until all initialized
				this.matcher = tmpMatcher;
				this.matcherCreationResetEvent.Set();

				lock(this.matcherCreationResetEventSyncObject)
				{
					ManualResetEvent tmpEvent = matcherCreationResetEvent;
					this.matcherCreationResetEvent = null;
					tmpEvent.Close();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="validConsumerRowList"></param>
		/// <param name="validCreditCardRowList"></param>
		/// <param name="dataModelTransaction"></param>
		protected abstract void GetAllValidConsumerAndCreditCardRows(List<RowLockingWrapper<ConsumerRow>> validConsumerRowList, 
			List<RowLockingWrapper<CreditCardRow>> validCreditCardRowList,
			IDataModelTransaction dataModelTransaction);


		/// <summary>
		/// Evaluates whether a given working order is eligible for a cross with another order.
		/// </summary>
		/// <param name="key">The key of the object to be handled.</param>
		/// <param name="parameters">A generic list of paraneters to the handler.</param>
		private void CrossWorkingOrderProc(Object[] key, params Object[] parameters)
		{
			Guid workingOrderId = (Guid)key[0];
			bool useWorkingOrderWriterLock = true;
			if(key.Length >2)
				useWorkingOrderWriterLock = (bool)key[2];

			WorkingOrderRowChangeEventArgs workingOrderRowChangeEventArgs = (WorkingOrderRowChangeEventArgs)key[1];

			this.ConsumerRowChangeProc(null, workingOrderRowChangeEventArgs.Row, null, useWorkingOrderWriterLock);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="workingOrderRow"></param>
		/// <param name="dataModelTransaction"></param>
		/// <param name="workingOrderRowDeleted"></param>
		/// <returns></returns>
		protected abstract ConsumerRow GetConsumerRowFromWorkingOrderRow(WorkingOrderRow workingOrderRow, DataModelTransaction dataModelTransaction, out bool workingOrderRowDeleted);

		/// <summary>
		/// Handles a change to a record in the WorkingOrder table.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="workingOrderRowChangeEventArgs">The event data.</param>
		private void WorkingOrder_WorkingOrderRowChanging(object sender, WorkingOrderRowChangeEventArgs workingOrderRowChangeEventArgs)
		{
			try
			{
				// When a working order is committed it will be examined to see if any of the properties that control crossing have changed.  A change in any of
				// these parameters indicates that the order should be re-examined for possible matches.
				if(workingOrderRowChangeEventArgs.Action == DataRowAction.Commit)
				{

					// Extract the unique working order identifier from the generic event arguments.  The identifier is needed for the handler that creates crosses
					// when the right conditions occur.
					WorkingOrderRow workingOrderRow = workingOrderRowChangeEventArgs.Row;
					Guid workingOrderId;

					// The idea here is to determine whether a change to an order should initiate a pass through the crossing engine.  Deleted orders need to be
					// considered because they could be crossed with an existing order.  If this is the case, the cross needs to be deleted.
					if(workingOrderRow.RowState == DataRowState.Deleted)
					{

						// Extract the unique identifier from the order.
						workingOrderId = (Guid)workingOrderRow[DataModel.WorkingOrder.WorkingOrderIdColumn, DataRowVersion.Original];
						this.actionQueue.Enqueue(new ObjectAction(this.CrossWorkingOrderProc, new Object[] { workingOrderId, workingOrderRowChangeEventArgs }));

					}
					else
					{

						// Existing orders will check for a state change or a blotterId change
						// before calling out to the thread that looks for crosses.  Brand new orders will be 
						// examined for crosses automatically.
						if(workingOrderRow.HasVersion(DataRowVersion.Original))
						{
							// Extract the unique identifier from the order.
							workingOrderId = (Guid)workingOrderRow[DataModel.WorkingOrder.WorkingOrderIdColumn, DataRowVersion.Original];

							// Extract the status.
							Guid previousStatus = (Guid)workingOrderRow[DataModel.WorkingOrder.StatusIdColumn, DataRowVersion.Original];
							Guid currentStatus = (Guid)workingOrderRow[DataModel.WorkingOrder.StatusIdColumn, DataRowVersion.Current];

							//extract the blotterId
							Guid previousBlotterId = (Guid)workingOrderRow[DataModel.WorkingOrder.BlotterIdColumn, DataRowVersion.Original];
							Guid currentBlotterId = (Guid)workingOrderRow[DataModel.WorkingOrder.BlotterIdColumn, DataRowVersion.Current];

							// This will ask the crossing thread to examine the order for possible matches when any of the critical properties for crossing have
							// changed.
							if(previousStatus != currentStatus ||
								previousBlotterId != currentBlotterId &&
								IsWorkingOrderForSide_InChangeEvent(workingOrderRow))
								this.actionQueue.Enqueue(new ObjectAction(this.CrossWorkingOrderProc, new Object[] { workingOrderId, workingOrderRowChangeEventArgs }));
						}
						else
						{

							// Extract the unique identifier from the new order.
							workingOrderId = (Guid)workingOrderRow[DataModel.WorkingOrder.WorkingOrderIdColumn, DataRowVersion.Current];

							// Automatically add all newly submitted orders for consideration in the cross.
							Guid currentStatusId = (Guid)workingOrderRow[DataModel.WorkingOrder.StatusIdColumn, DataRowVersion.Current];
							if(currentStatusId == StatusMap.FromCode(Status.Submitted)&&
								IsWorkingOrderForSide_InChangeEvent(workingOrderRow))
								this.actionQueue.Enqueue(new ObjectAction(this.CrossWorkingOrderProc, new Object[] { workingOrderId, workingOrderRowChangeEventArgs }));

						}

					}
				}
				else if(workingOrderRowChangeEventArgs.Action == DataRowAction.Change &&
					workingOrderRowChangeEventArgs.Row.HasVersion(DataRowVersion.Original))
				{

					// Extract the unique working order identifier from the generic event arguments.  The identifier is needed for the handler that creates crosses
					// when the right conditions occur.
					WorkingOrderRow workingOrderRow = workingOrderRowChangeEventArgs.Row;

					//setting the date time in the future always means re-match the order
					if((DateTime)workingOrderRow[DataModel.WorkingOrder.ModifiedTimeColumn] > DateTime.UtcNow)
					{
						// Extract the unique identifier from the order.
						Guid workingOrderId = (Guid)workingOrderRow[DataModel.WorkingOrder.WorkingOrderIdColumn];

						if(IsWorkingOrderForSide_InChangeEvent(workingOrderRow))
							this.actionQueue.Enqueue(new ObjectAction(this.CrossWorkingOrderProc, new Object[] { workingOrderId, workingOrderRowChangeEventArgs, false }));
						return;
					}
				}
			}
			catch(Exception ex)
			{
				EventLog.Information("Error in WorkingOrder_WorkingOrderRowChanging {0}\r\n{1}", ex.Message, ex.StackTrace);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		protected abstract string ThreadName { get; }

		/// <summary>
		/// Starts the exchange.
		/// </summary>
		public void Start()
		{
			this.SubscribeToNegotiationTableEvents();
			this.SubscribeToContraNegotiationTableEvents();

			// These event handlers will update the matching conditions as the underlying records change.
			DataModel.Consumer.ConsumerRowChanging += new ConsumerRowChangeEventHandler(Consumer_ConsumerRowChanging);
			DataModel.CreditCard.CreditCardRowChanging += new CreditCardRowChangeEventHandler(CreditCard_CreditCardRowChanging);

			DataModel.WorkingOrder.WorkingOrderRowChanging += WorkingOrder_WorkingOrderRowChanging;
			
			DataModel.DebtRule.DebtRuleRowValidate += new DebtRuleRowChangeEventHandler(DebtRule_DebtRuleRowValidate);

			DataModel.DebtClass.DebtClassRowValidate += new DebtClassRowChangeEventHandler(DebtClass_DebtClassRowValidate);

			DataModel.Match.RowChanging += new DataRowChangeEventHandler(Match_RowChanging);
			
			// This thread will execution the actions that are created by changes to the data model.  The triggers themselves can't modify the data
			// model because the triggers are called from the commit handlers.
			crossingThread = new Thread(new ThreadStart(this.CrossingThread));
			crossingThread.Name = ThreadName;
			crossingThread.IsBackground = true;
			crossingThread.Start();
		}

		private void Match_RowChanging(object sender, DataRowChangeEventArgs e)
		{
			try
			{
				if(e.Action == System.Data.DataRowAction.Commit &&
					e.Row.RowState == System.Data.DataRowState.Deleted &&
					e.Row.HasVersion(System.Data.DataRowVersion.Original))
				{
					Guid workingOrderId = (Guid)e.Row[DataModel.Match.WorkingOrderIdColumn, DataRowVersion.Original];
					Guid contraWorkingOrderId = (Guid)e.Row[DataModel.Match.ContraOrderIdColumn, DataRowVersion.Original];
					Guid matchId = (Guid)e.Row[DataModel.Match.MatchIdColumn, DataRowVersion.Original];
					Guid contraMatchId = (Guid)e.Row[DataModel.Match.ContraMatchIdColumn, DataRowVersion.Original];

					this.actionQueue.Enqueue(new ObjectAction(this.DeleteMatchProc, new Object[] { matchId, contraMatchId, workingOrderId, contraWorkingOrderId }));
				}
			}
			catch(Exception ex)
			{
				EventLog.Information("Error in Match_RowChanging {0}\r\n{1}", ex.Message, ex.StackTrace);
			}

		}

		private void DeleteMatchProc(Object[] key, params Object[] parameters)
		{
			Guid matchId = (Guid)key[0];
			Guid contraMatchId = (Guid)key[1];
			Guid workingOrderId = (Guid)key[2];
			Guid contraWorkingOrderId = (Guid)key[3];

			for(int deadlockRetry = 0; deadlockRetry < deadlockRetiesMax; deadlockRetry++)
			{
				try
				{
					using(TransactionScope transactionScope = new TransactionScope())
					{
						// This provides a context for any transactions.
						DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

						WorkingOrderRow workingOrderRow = DataModel.WorkingOrder.WorkingOrderKey.Find(workingOrderId);
						bool workingOrderRowDeleted = false;
						if(workingOrderRow != null)
						{
							ConsumerRow consumerRow = GetConsumerRowFromWorkingOrderRow(workingOrderRow, dataModelTransaction, out workingOrderRowDeleted);
							if(consumerRow != null)
							{
								//delete is for this side, so need to rematch the order
								WorkingOrderRowChangeEventArgs args = new WorkingOrderRowChangeEventArgs(workingOrderRow, DataRowAction.Change);

								//queue it right back to this thread, because don't want
								//to give the rematch task priority over other items
								this.actionQueue.Enqueue(new ObjectAction(this.CrossWorkingOrderProc, new Object[] { workingOrderId, args }));
								return;
							}
							else if(workingOrderRowDeleted == false)
							{
								//the delete if from the other side so need to delete match on this side
								MatchRow contraMatchRow = DataModel.Match.MatchKey.Find(contraMatchId);
								if(contraMatchRow != null)
								{
									contraMatchRow.AcquireWriterLock(dataModelTransaction);
									if(contraMatchRow.RowState == DataRowState.Detached ||
											contraMatchRow.RowState == DataRowState.Deleted)
										return;

									DataModel dataModel = new DataModel();
									dataModel.DestroyMatch(new object[] { contraMatchId }, contraMatchRow.RowVersion);

									transactionScope.Complete();
									return;
								}
							}
						}

						//if the working order row is deleted check the contra working order
						//if the contra working order is Not this side then need to delete the match
						WorkingOrderRow contraWorkingOrderRow = DataModel.WorkingOrder.WorkingOrderKey.Find(contraWorkingOrderId);
						bool contraWorkingOrderRowDeleted = false;
						if(contraWorkingOrderRow != null)
						{
							ConsumerRow consumerRow = GetConsumerRowFromWorkingOrderRow(contraWorkingOrderRow, dataModelTransaction, out contraWorkingOrderRowDeleted);
							if(consumerRow == null && contraWorkingOrderRowDeleted == false)
							{
								//the delete if from the other side so need to delete match on this side
								MatchRow contraMatchRow = DataModel.Match.MatchKey.Find(contraMatchId);
								if(contraMatchRow != null)
								{
									contraMatchRow.AcquireWriterLock(dataModelTransaction);
									if(contraMatchRow.RowState == DataRowState.Detached ||
											contraMatchRow.RowState == DataRowState.Deleted)
										return;

									DataModel dataModel = new DataModel();
									dataModel.DestroyMatch(new object[] { contraMatchId }, contraMatchRow.RowVersion);

									transactionScope.Complete();
									return;
								}
							}
						}

					}//end using

					break;
				}
				catch(Exception sqlEx)
				{
					if(FluidTrade.Core.Utilities.SqlErrorHelper.IsDeadlockException(sqlEx))
					{
						if(deadlockRetry == deadlockRetiesMax - 1)
							throw;

						if(EventLog.IsLoggingEnabledFor(EventLog.ErrorLogLevel.Verbose))
							EventLog.Warning("Deadlock exception\r\n{0} {1}: {2}", this.ThreadName, sqlEx.Message, sqlEx.StackTrace);
						Thread.Sleep(2000 * deadlockRetry + 1);
					}
					else
					{
						throw;
					}
				}
			}
		}

		private void DebtClass_DebtClassRowValidate(object sender, DebtClassRowChangeEventArgs e)
		{
			DataRowState rowState = e.Row.RowState;

			//only looking for changes in the debtRule in the debtClass
			if(rowState == DataRowState.Deleted ||
				rowState == DataRowState.Detached ||
				(e.Row.HasVersion(DataRowVersion.Original) == false ||
				e.Row.HasVersion(DataRowVersion.Current) == false) ||
				object.Equals(e.Row[DataModel.DebtClass.DebtRuleIdColumn, DataRowVersion.Original], e.Row[DataModel.DebtClass.DebtRuleIdColumn, DataRowVersion.Current]))
				return;

			//e.Row.GetDebtClassRows
			List<ConsumerRow> consumerRowList = new List<ConsumerRow>();
			DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

			this.GetConsumerRowsFromDebtClass(e.Row, dataModelTransaction, consumerRowList);
			
			foreach(ConsumerRow consumerRow in consumerRowList)
			{
				this.Consumer_ConsumerRowChanging(this, new ConsumerRowChangeEventArgs(consumerRow, DataRowAction.Commit));
			}
		}

		private void DebtRule_DebtRuleRowValidate(object sender, DebtRuleRowChangeEventArgs e)
		{
			DataRowState rowState = e.Row.RowState;

			//only looking for changes in the settlementValue in the debtRule
			if(rowState == DataRowState.Deleted ||
				rowState == DataRowState.Detached ||
				(e.Row.HasVersion(DataRowVersion.Original) == false ||
				e.Row.HasVersion(DataRowVersion.Current) == false) ||
				object.Equals(e.Row[DataModel.DebtRule.SettlementValueColumn, DataRowVersion.Original], e.Row[DataModel.DebtRule.SettlementValueColumn, DataRowVersion.Current]))
				return;
			
			//e.Row.GetDebtClassRows
			List<ConsumerRow> consumerRowList = new List<ConsumerRow>();
			this.GetConsumerRowsFromDebtRule(e.Row, consumerRowList);

			DebtClassRow[] debtClassRowAr = e.Row.GetDebtClassRows();
			if(debtClassRowAr.Length > 0)
			{
				DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

				foreach(DebtClassRow debtClassRow in debtClassRowAr)
				{
					debtClassRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
					try
					{
						this.GetConsumerRowsFromDebtClass(debtClassRow, dataModelTransaction, consumerRowList);
					}
					finally
					{
						debtClassRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
					}
				}
			}
			foreach(ConsumerRow consumerRow in consumerRowList)
			{
				this.Consumer_ConsumerRowChanging(this, new ConsumerRowChangeEventArgs(consumerRow, DataRowAction.Commit));
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="debtClassRow"></param>
		/// <param name="dataModelTransaction"></param>
		/// <param name="listToPopulate"></param>
		private void GetConsumerRowsFromDebtClass(DebtClassRow debtClassRow, DataModelTransaction dataModelTransaction, List<ConsumerRow> listToPopulate)
		{
			BlotterRow blotterRow = DataModel.Blotter.BlotterKey.Find((Guid)debtClassRow[DataModel.DebtClass.DebtClassIdColumn]);

			blotterRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
			WorkingOrderRow[] workingOrderAr;
			try
			{
				workingOrderAr = blotterRow.GetWorkingOrderRows();
			}
			finally
			{
				blotterRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
			}

			foreach(WorkingOrderRow workingOrderRow in workingOrderAr)
			{
				bool workingOrderRowDeleted;
				ConsumerRow consumerRow = this.GetConsumerRowFromWorkingOrderRow(workingOrderRow, dataModelTransaction, out workingOrderRowDeleted);
				if(consumerRow == null)
					continue;

				listToPopulate.Add(consumerRow);
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="debtRuleRow"></param>
		/// <param name="listToPopulate"></param>
		protected abstract void GetConsumerRowsFromDebtRule(DebtRuleRow debtRuleRow, List<ConsumerRow> listToPopulate);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="consumerRowChangeEventArgs"></param>
		protected void Consumer_ConsumerRowChanging(object sender, ConsumerRowChangeEventArgs consumerRowChangeEventArgs)
		{
			try
			{
				if(consumerRowChangeEventArgs.Action != DataRowAction.Commit)
					return;

				// Extract the unique working order identifier from the generic event arguments.  The identifier is needed for the handler that creates crosses
				// when the right conditions occur.
				ConsumerRow consumerRow = consumerRowChangeEventArgs.Row;

				//get the consumerId before the row has a chance to go away
				Guid consumerId = Guid.Empty;
				if(consumerRow.HasVersion(DataRowVersion.Current))
					consumerId = (Guid)consumerRow[DataModel.Consumer.ConsumerIdColumn, DataRowVersion.Current];
				else if(consumerRow.HasVersion(DataRowVersion.Original))
					consumerId = (Guid)consumerRow[DataModel.Consumer.ConsumerIdColumn, DataRowVersion.Original];
				else
					return;

				if(consumerRow.RowState == DataRowState.Deleted ||
					consumerRow.RowState == DataRowState.Detached ||
					this.IsConsumerRowForSide_InChangeEvent(consumerRow))
				this.actionQueue.Enqueue(new ObjectAction(ConsumerRowChangeProc, new Object[] { consumerId, consumerRowChangeEventArgs, null }));
			}
			catch(Exception ex)
			{
				EventLog.Information("Error in Consumer_ConsumerRowChanging {0}\r\n{1}", ex.Message, ex.StackTrace);
			}

		}

		private void ConsumerRowChangeProc(Object[] key, params Object[] parameters)
		{
			Guid consumerId = (Guid)key[0];
			ConsumerRowChangeEventArgs consumerRowChangeEventArgs = (ConsumerRowChangeEventArgs)key[1];
			ConsumerRow consumerRow = consumerRowChangeEventArgs.Row;

			ConsumerRowChangeProc(consumerRow, null, null, true);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="rawConsumerRow"></param>
		/// <param name="rawWorkingOrderRow"></param>
		/// <param name="rawCreditCardRow"></param>
		/// <param name="useWorkingOrderWriterLock"></param>
		private void ConsumerRowChangeProc(ConsumerRow rawConsumerRow, 
									WorkingOrderRow rawWorkingOrderRow, 
									CreditCardRow rawCreditCardRow,
									bool useWorkingOrderWriterLock)
		{
			Guid consumerId = Guid.Empty;
			bool matcherUpdated = false;
			List<ConsumerContainerRow> consumerContainerRowList = null;
			
			MatchCreateParms newMatchCreateParam = null;
			try
			{
				for(int deadlockRetry = 0; deadlockRetry < deadlockRetiesMax; deadlockRetry++)
				{
					try
					{
						using(TransactionScope transactionScope = new TransactionScope())
						{
							// This provides a context for any transactions.
							DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

							// !!!RM this is just for testing should remove once we figure out deadlock issues
							// abstract method to lock all the orders related to the match that is about to be operated on
							//////lock as fast as possible
							//if(this.LockWorkingOrderBeforeMatch(dataModelTransaction,
							//                                rawConsumerRow,
							//                                rawWorkingOrderRow,
							//                                rawCreditCardRow) == false)
							//    return;

							if(rawConsumerRow == null)
							{
								if(rawWorkingOrderRow != null)
								{
									bool workingOrderRowDeleted = false;
									rawConsumerRow = this.GetConsumerRowFromWorkingOrderRow(rawWorkingOrderRow, dataModelTransaction, out workingOrderRowDeleted);
									if(rawConsumerRow == null)
										return;
								}
								else if(rawCreditCardRow != null)
								{
									rawCreditCardRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
									bool ccDeletedOrDetached = rawCreditCardRow.RowState == DataRowState.Deleted ||
																		rawCreditCardRow.RowState == DataRowState.Detached;

									try
									{
										if(ccDeletedOrDetached == false)
										{
											rawConsumerRow = rawCreditCardRow.ConsumerRow;
										}
									}
									finally
									{
										rawCreditCardRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
									}

									if(rawConsumerRow == null ||
										ccDeletedOrDetached == true)
									{
										matcher.UpdateCreditCard(new RowLockingWrapper<CreditCardRow>(rawCreditCardRow, dataModelTransaction), dataModelTransaction, false);
										return;
									}
								}
								else
								{
									return;
								}
							}
							RowLockingWrapper<ConsumerRow> consumerRow = new RowLockingWrapper<ConsumerRow>(rawConsumerRow, dataModelTransaction);
							DataRowState rowState;
							consumerRow.AcquireReaderLock();
							try
							{
								rowState = consumerRow.Row.RowState;

								if(rowState != DataRowState.Detached &&
									rowState != DataRowState.Deleted)
									consumerId = (Guid)consumerRow.Row[DataModel.Consumer.ConsumerIdColumn];
								else
									consumerId = Guid.Empty;

							}
							finally
							{
								consumerRow.ReleaseReaderLock();
							}

							//check to see if this consumer row is something we care about
							//!!!RM need to play with this.
							//it could be faster to do the match and then validate 
							//the matches that are above threshold to see if we 
							//care about it. 
							// right now checking if care first and then doing match
							//this may or may not be needed in order to update the matcher
							bool isValid;
							if(rowState == DataRowState.Detached ||
								rowState == DataRowState.Deleted)
							{
								isValid = false;
							}
							else
							{
								this.GetValidConsumerRelatedRowsInternal(consumerRow, dataModelTransaction,
																		out consumerContainerRowList);

								isValid = consumerContainerRowList != null && consumerContainerRowList.Count != 0;
							}

							//update the Matcher first 
							if(isValid ||
								rowState == DataRowState.Detached ||
								rowState == DataRowState.Deleted)
							{
								//update matcher in all cases if row is deleted or detached
								//matcher will remove it from its internal storage
								//it might make sense for matcher to subscibe to the rowValidate events
								//so that it can remove the deleted rows ASAP
								matcher.UpdateConsumer(consumerRow, dataModelTransaction, isValid);

								if(rawCreditCardRow != null)
									matcher.UpdateCreditCard(new RowLockingWrapper<CreditCardRow>(rawCreditCardRow, dataModelTransaction), dataModelTransaction, isValid);

								matcherUpdated = true;
							}

							if(isValid == false)
								return;

							HashSet<WorkingOrderRow> orderRowsToLock = new HashSet<WorkingOrderRow>();

							Dictionary<Guid, MatchRow> existingMatchRows = new Dictionary<Guid, MatchRow>();
							foreach(ConsumerContainerRow containerRow in consumerContainerRowList)
							{
								foreach(MatchRow[] matchAr in containerRow.WorkingOrderRowMatchRowsList)
								{
									foreach(MatchRow matchRow in matchAr)
									{
										matchRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
										try
										{
											if(matchRow.RowState != DataRowState.Deleted &&
												matchRow.RowState != DataRowState.Detached)
												existingMatchRows[matchRow.MatchId] = matchRow;

											WorkingOrderRow workingOrderRow = matchRow.WorkingOrderRow;
											if(workingOrderRow != null)
												orderRowsToLock.Add(workingOrderRow);

											//WorkingOrderRow contraWorkingOrderRow = DataModel.WorkingOrder.WorkingOrderKey.Find(matchRow.ContraOrderId);
											//if(contraWorkingOrderRow != null)
											//    orderRowsToLock.Add(contraWorkingOrderRow);

										}
										finally
										{
											matchRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
										}
									}
								}
							}

							if(useWorkingOrderWriterLock == true)
								foreach(WorkingOrderRow lockOrderRow in orderRowsToLock)
								{
									lockOrderRow.AcquireWriterLock(dataModelTransaction);
								}

							this.FindMaxContraMatches(consumerRow, consumerContainerRowList, dataModelTransaction);

							if(consumerContainerRowList != null)
							{
								DataModel dataModel = new DataModel();

								foreach(ConsumerContainerRow containerRow in consumerContainerRowList)
								{
									if(containerRow.WorkingOrderRow == null ||
										containerRow.ContraWorkingOrderRow == null)
										continue;

									if(containerRow.MaxMatchResult == null)
										continue;

									FluidTrade.Core.Matching.CardSocialLastNameFuzzyMatcher.MatchResult maxResult = containerRow.MaxMatchResult;
									////create a new match
									this.CreateNewOrUpdateMatch(containerRow, dataModel, dataModelTransaction, out newMatchCreateParam);
								}

								foreach(ConsumerContainerRow containerRow in consumerContainerRowList)
								{
									if(containerRow == null)
										continue;

									if(containerRow.UpdatedMatchGuid != Guid.Empty)
										existingMatchRows.Remove(containerRow.UpdatedMatchGuid);

									if(containerRow.CreatedMatchGuid != Guid.Empty)
										existingMatchRows.Remove(containerRow.CreatedMatchGuid);
								}

								if(existingMatchRows.Count > 0)
								{
									foreach(KeyValuePair<Guid, MatchRow> existingMatchRowPair in existingMatchRows)
									{
										Guid existingMatchId;
										long rowVersion;
										bool rowAlreadyLocked = existingMatchRowPair.Value.IsLockHeld(dataModelTransaction.TransactionId);
										if(rowAlreadyLocked == false)
											existingMatchRowPair.Value.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
										try
										{
											if(existingMatchRowPair.Value.RowState == DataRowState.Detached ||
												existingMatchRowPair.Value.RowState == DataRowState.Deleted)
												continue;

											existingMatchId = existingMatchRowPair.Key;
											rowVersion = existingMatchRowPair.Value.RowVersion;
											Guid matchStatusId = existingMatchRowPair.Value.StatusId;

											//do not delete match if status has progressed
											if(!(matchStatusId == ValidMatchFundsStatus ||
												matchStatusId == ValidMatchStatus ||
												matchStatusId == PartialMatchStatus))
											{
												continue;
											}
										}
										finally
										{
											if(rowAlreadyLocked == false)
												existingMatchRowPair.Value.ReleaseReaderLock(dataModelTransaction.TransactionId);
										}

										dataModel.DestroyMatch(new object[] { existingMatchId }, rowVersion);
									}
								}
							}

							transactionScope.Complete();
						}
					}
					catch(Exception sqlEx)
					{
						if(FluidTrade.Core.Utilities.SqlErrorHelper.IsDeadlockException(sqlEx))
						{
							if(deadlockRetry == deadlockRetiesMax - 1)
								throw;

							if(EventLog.IsLoggingEnabledFor(EventLog.ErrorLogLevel.Verbose))
								EventLog.Warning("Deadlock exception\r\n{0} {1}: {2}", this.ThreadName, sqlEx.Message, sqlEx.StackTrace);
							Thread.Sleep(2000 * deadlockRetry + 1);
						}
						else
						{
							throw;
						}
					}
				}
			}
			catch
			{
				if(newMatchCreateParam != null)
				{
					this.ClearCreatedMatchKeys(newMatchCreateParam, true);
				}

				throw;
			}

			if(newMatchCreateParam != null)
				this.ClearCreatedMatchKeys(newMatchCreateParam, false);

			if(matcherUpdated == true  &&
				consumerId != Guid.Empty) //!!!RM figure out if this is needed createdNewMatch == false to notify other side it should try a match
			{
				//tell other side that matcher has changed
				NotifyContraOnMatcherUpdate(consumerId, rawConsumerRow);
			}
		}

		internal abstract ConsumerDebtMatchInfo CreateConsumerDebtInfo(ConsumerContainerRow containerRow, DataModel dataModel, DataModelTransaction dataModelTransaction);
		internal abstract ConsumerTrustMatchInfo CreateConsumerTrustInfo(ConsumerContainerRow containerRow, DataModel dataModel, DataModelTransaction dataModelTransaction);

		internal abstract void CreateNewMatch(DataModel dataModel, DataModelTransaction dataModelTransaction, MatchCreateParms matchCreateParam);

		internal abstract bool UpdateMatch(DataModel dataModel, DataModelTransaction dataModelTransaction, MatchCreateParms matchCreateParam);

		internal abstract void UpdateMatchFromContra(DataModel dataModel, DataModelTransaction dataModelTransaction, MatchCreateParms createParam);
		internal abstract void CreateNewMatchFromContra(DataModel dataModel, DataModelTransaction dataModelTransaction, MatchCreateParms createParam);

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
		internal abstract bool LockWorkingOrderBeforeMatch(DataModelTransaction dataModelTransaction,
																ConsumerRow rawConsumerRow,
																WorkingOrderRow rawWorkingOrderRow,
																CreditCardRow rawCreditCardRow);

		//internal abstract ConsumerTrustMatchInfo CreateConsumerTrustInfo(ConsumerContainerRow containerRow, DataModel dataModel, IDataModelTransaction dataModelTransaction);

		private void CreateNewOrUpdateMatch(ConsumerContainerRow containerRow, DataModel dataModel, DataModelTransaction dataModelTransaction,
												out MatchCreateParms newMatchCreateParam)
		{
			newMatchCreateParam = null;
			// This will collect the information about both sides of the trade.  These data structures are used to negotiate a settlement between the two
			// parties to this match.

			MatchRow matchRow = null;
			Guid matchRowMatchId;
			Guid matchRowContraMatchId;
			Guid matchStatusId = Guid.Empty;
			matchRow = this.FindMatchRowFromConsumerContainer(containerRow, dataModelTransaction);
			if(matchRow != null)
			{
				bool rowAlreadyLocked = matchRow.IsLockHeld(dataModelTransaction.TransactionId);
				if(rowAlreadyLocked == false)
					matchRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
				try
				{
					if(matchRow.RowState == DataRowState.Detached ||
						matchRow.RowState == DataRowState.Deleted)
					{
						matchRow = null;
						matchRowMatchId = Guid.Empty;
						matchRowContraMatchId = Guid.Empty;
					}
					else
					{
						matchRowMatchId = matchRow.MatchId;
						matchRowContraMatchId = matchRow.ContraMatchId;
						matchStatusId = matchRow.StatusId;
						//there is a chance that this might not be created yet so just wait to get 
						//it until on the other thread
						//contraMatchRow = DataModel.Match.MatchKey.Find(matchRow.ContraMatchId);
					}
				}
				finally
				{
					if(rowAlreadyLocked == false)
						matchRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				}
			}
			else
			{
				matchRowMatchId = Guid.Empty;
				matchRowContraMatchId = Guid.Empty;
			}

		
			ConsumerDebtMatchInfo consumerDebtMatchInfo = this.CreateConsumerDebtInfo(containerRow, dataModel, dataModelTransaction);
			ConsumerTrustMatchInfo consumerTrustMatchInfo = this.CreateConsumerTrustInfo(containerRow, dataModel, dataModelTransaction);

			// Both parties are given a timer that will implicitly decline the transaction when it expires.
			//DateTime matchTime = DateTime.UtcNow;
			MatchCreateParms createParam = new MatchCreateParms(containerRow, consumerDebtMatchInfo, consumerTrustMatchInfo,
																containerRow.MaxMatchResult.Strength,
																matchStatusId, matchRow, 
																matchRowMatchId, matchRowContraMatchId);

			if(matchRow == null)
			{
				containerRow.CreatedMatchGuid = createParam.matchId;
				//check to see if the match already is pending
				newMatchCreateParam = createParam;
				PendingWorkingOrderKey key;
				PendingWorkingOrderKey contraKey;
				this.CreateNewMatchWorkingOrderKeys(createParam, out key, out contraKey);

				lock(pendingMatchMap)
				{
					//check if this match is already pending
					if(pendingMatchMap.ContainsKey(key))
						return;

					//not pending so make it so
					pendingMatchMap.Add(key, false);
					pendingMatchMap.Add(contraKey, false);
				}


				//System.Diagnostics.Debug.WriteLine(
				//    string.Format("{0}  SendNotifyContraCreateNew  matchId:{1}  contraId{2}",
				//                    this.ThreadName, createParam.matchId, createParam.contraMatchId));

				//need to notify the other side first because the creation will
				//first a Consumer_NegotiationRowValidate event and
				//when that gets to the other thread it will need the match to 
				//have been created first
				this.NotifyContraOnMatchCreate(createParam);

				//System.Diagnostics.Debug.WriteLine(
				//    string.Format("{0}  CreateNew  matchId:{1}  contraId{2}",
				//                    this.ThreadName, createParam.matchId, createParam.contraMatchId));
				this.CreateNewMatch(dataModel, dataModelTransaction, createParam);
			}
			else
			{
				containerRow.UpdatedMatchGuid = createParam.matchId;
				createParam.matchTime = DateTime.UtcNow;
				
				//System.Diagnostics.Debug.WriteLine(
				//    string.Format("{0}  SendNotifyContraUpdate  matchId:{1}  contraId{2}",
				//                    this.ThreadName, createParam.matchId, createParam.contraMatchId));


				if(this.UpdateMatch(dataModel, dataModelTransaction, createParam))
				//need to notify the other side first because the creation will
				//first a Consumer_NegotiationRowValidate event and
				//when that gets to the other thread it will need the match to 
				//have been created first
				this.NotifyContraOnMatchUpdate(createParam);

				//System.Diagnostics.Debug.WriteLine(
				//        string.Format("{0}  Update  matchId:{1}  contraId{2}",
				//                        this.ThreadName, createParam.matchId, createParam.contraMatchId));

			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="consumerDebtMatchInfo"></param>
		/// <param name="consumerTrustMatchInfo"></param>
		/// <returns></returns>
		internal abstract bool GetHasFunds(ConsumerDebtMatchInfo consumerDebtMatchInfo, ConsumerTrustMatchInfo consumerTrustMatchInfo);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="matchCreateParam"></param>
		/// <returns></returns>
		internal abstract Status? GetStatus(MatchCreateParms matchCreateParam);


		/// <summary>
		/// 
		/// </summary>
		/// <param name="createParam"></param>
		/// <param name="key"></param>
		/// <param name="contraKey"></param>
		internal abstract void CreateNewMatchWorkingOrderKeys(MatchCreateParms createParam, out PendingWorkingOrderKey key, out PendingWorkingOrderKey contraKey);

		private MatchRow FindMatchRowFromConsumerContainer(ConsumerContainerRow containerRow, IDataModelTransaction dataModelTransaction)
		{
			if(containerRow.ContraWorkingOrderRow == null)
				return null;

			Guid contraWorkingOrderGuid;
			containerRow.ContraWorkingOrderRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
			try
			{
				if(containerRow.ContraWorkingOrderRow.RowState == DataRowState.Detached ||
					containerRow.ContraWorkingOrderRow.RowState == DataRowState.Deleted)
					return null;

				contraWorkingOrderGuid = containerRow.ContraWorkingOrderRow.WorkingOrderId_NoLockCheck;
			}
			finally
			{
				containerRow.ContraWorkingOrderRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
			}

			foreach(MatchRow[] matchRowAr in containerRow.WorkingOrderRowMatchRowsList)
			{
				foreach(MatchRow curMatchRow in matchRowAr)
				{
					bool rowAlreadyLocked = curMatchRow.IsLockHeld(dataModelTransaction.TransactionId);
					if(rowAlreadyLocked == false)
						curMatchRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
					try
					{
						if(curMatchRow.RowState == DataRowState.Deleted ||
							curMatchRow.RowState == DataRowState.Detached)
							continue;

						if(curMatchRow.ContraOrderId == contraWorkingOrderGuid)
						{
							return curMatchRow;
						}
					}
					finally
					{
						if(rowAlreadyLocked == false)
							curMatchRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
					}
				}
			}

			return null;
		}


		/// <summary>
		/// consumerRow is already locked
		/// </summary>
		/// <param name="consumerRow"></param>
		/// <param name="consumerContainerRowList"></param>
		/// <param name="dataModelTransaction"></param>
		/// <returns></returns>
		protected abstract void FindMaxContraMatches(RowLockingWrapper<ConsumerRow> consumerRow, 
													List<ConsumerContainerRow> consumerContainerRowList,
													IDataModelTransaction dataModelTransaction);

		/// <summary>
		/// 
		/// </summary>
		protected abstract ConsumerCross ContraConsumerCross
		{
			get;
		}

		//protected abstract void NotifyContraOnMatchCreated();

		//public void ContraMatcherUpdate(Guid consumerId, ConsumerRowChangeEventArgs consumerRowChangeEventArgs)
		//{
		//    this.actionQueue.Enqueue(new ObjectAction(ContraMatcherUpdateProc, new Object[] { consumerId, consumerRowChangeEventArgs }));
		//}

		//private void ContraMatcherUpdateProc(Object[] key, params Object[] parameters)
		//{
		//    Guid consumerId = (Guid)key[0];
		//    ConsumerRowChangeEventArgs consumerRowChangeEventArgs = (ConsumerRowChangeEventArgs)key[1];
		//    ConsumerRow consumerRow = consumerRowChangeEventArgs.Row;

		//}

		private void NotifyContraOnMatcherUpdate(Guid consumerId, ConsumerRow row)
		{
			this.ContraConsumerCross.ContraMatcherUpdate(consumerId, row);
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="consumerId"></param>
		/// <param name="row"></param>
		public void ContraMatcherUpdate(Guid consumerId, ConsumerRow row)
		{
			this.actionQueue.Enqueue(new ObjectAction(ContraMatcherUpdateProc, new Object[] { consumerId, row }));
		}

		private void ContraMatcherUpdateProc(Object[] key, params Object[] parameters)
		{
			Guid consumerId = (Guid)key[0];
			ConsumerRow row = (ConsumerRow)key[1];
			
		}

		private void NotifyContraOnMatchCreate(MatchCreateParms createParam)
		{
			this.ContraConsumerCross.ContraMatchCreate(createParam);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="createParam"></param>
		internal void ContraMatchCreate(MatchCreateParms createParam)
		{
			this.actionQueue.Enqueue(new ObjectAction(ContraMatchCreateProc, new Object[] { createParam }));
		}

		
		private void ContraMatchCreateProc(Object[] key, params Object[] parameters)
		{
			MatchCreateParms createParam = (MatchCreateParms)key[0];

			try
			{
				using(TransactionScope transactionScope = new TransactionScope())
				{
					// This provides a context for any transactions.
					DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

					DataModel dataModel = new DataModel();

					//System.Diagnostics.Debug.WriteLine(
					//        string.Format("{0}  CreateNewFromContra  matchId:{1}  contraId{2}",
					//                        this.ThreadName, createParam.matchId, createParam.contraMatchId));

					CreateNewMatchFromContra(dataModel, dataModelTransaction, createParam);

					transactionScope.Complete();
				}
			}
			catch
			{
				this.ClearCreatedMatchKeys(createParam, true);
				
				throw;
			}

			this.ClearCreatedMatchKeys(createParam, false);
		}


		private void ClearCreatedMatchKeys(MatchCreateParms createParam, bool forceClear)
		{
			PendingWorkingOrderKey key;
			PendingWorkingOrderKey contraKey;
			this.CreateNewMatchWorkingOrderKeys(createParam, out key, out contraKey);
			if(forceClear == true)
			{
				lock(pendingMatchMap)
				{
					pendingMatchMap.Remove(key);
					pendingMatchMap.Remove(contraKey);
					return;
				}
			}

			lock(pendingMatchMap)
			{
				//if both sides of the matches are done remove them both
				bool hasBeenCreated = false;
				if(pendingMatchMap.TryGetValue(contraKey, out hasBeenCreated) == true &&
					hasBeenCreated == true &&

					pendingMatchMap.TryGetValue(contraKey, out hasBeenCreated) == true &&
					hasBeenCreated == true)
				{
					pendingMatchMap.Remove(key);
					pendingMatchMap.Remove(contraKey);
				}
				else
				{
					//the other side has not be marked as created, so no delete,
					//but need to mark this side created so the other side 
					//knows it is done
					pendingMatchMap[key] = true;
				}
			}
		}
		private void NotifyContraOnMatchUpdate(MatchCreateParms createParam)
		{
			this.ContraConsumerCross.ContraMatchUpdate(createParam);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="createParam"></param>
		internal void ContraMatchUpdate(MatchCreateParms createParam)
		{
			this.actionQueue.Enqueue(new ObjectAction(ContraMatchUpdateProc, new Object[] { createParam }));
		}
	
		private void ContraMatchUpdateProc(Object[] key, params Object[] parameters)
		{
			MatchCreateParms createParam = (MatchCreateParms)key[0];

			using(TransactionScope transactionScope = new TransactionScope())
			{
				// This provides a context for any transactions.
				DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

				DataModel dataModel = new DataModel();

				this.UpdateMatchFromContra(dataModel, dataModelTransaction, createParam);

				transactionScope.Complete();
			}
		}


		private void GetValidConsumerRelatedRowsInternal(RowLockingWrapper<ConsumerRow> consumerRow, IDataModelTransaction dataModelTransaction,
														out List<ConsumerContainerRow> consumerContainerRowList)
		{
				this.GetValidConsumerRelatedRows(consumerRow, dataModelTransaction,
												out consumerContainerRowList);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="consumerRow"></param>
		/// <param name="dataModelTransaction"></param>
		/// <param name="consumerContainerRowList"></param>
		protected abstract void GetValidConsumerRelatedRows(RowLockingWrapper<ConsumerRow> consumerRow, IDataModelTransaction dataModelTransaction,
														out List<ConsumerContainerRow> consumerContainerRowList);

		/// <summary>
		/// 
		/// </summary>
		public class ConsumerContainerRow
		{
			/// <summary>
			/// 
			/// </summary>
			public DataRow ContainerRow;
			
			/// <summary>
			/// 
			/// </summary>
			public WorkingOrderRow WorkingOrderRow;

			/// <summary>
			/// 
			/// </summary>
			public List<MatchRow[]> WorkingOrderRowMatchRowsList;
			
			/// <summary>
			/// 
			/// </summary>
			public CreditCardRow CreditCardRow;

			/// <summary>
			/// 
			/// </summary>
			public FluidTrade.Core.Matching.CardSocialLastNameFuzzyMatcher.MatchResult MaxMatchResult;

			/// <summary>
			/// 
			/// </summary>
			public WorkingOrderRow ContraWorkingOrderRow;

			/// <summary>
			/// 
			/// </summary>
			public Guid UpdatedMatchGuid;

			/// <summary>
			/// 
			/// </summary>
			public Guid CreatedMatchGuid;

		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="creditCardRowChangeEventArgs"></param>
		protected virtual void CreditCard_CreditCardRowChanging(object sender, CreditCardRowChangeEventArgs creditCardRowChangeEventArgs)
		{
			try
			{
			if(creditCardRowChangeEventArgs.Action != DataRowAction.Commit)
				return;

			// Extract the unique working order identifier from the generic event arguments.  The identifier is needed for the handler that creates crosses
			// when the right conditions occur.
			CreditCardRow creditCardRow = creditCardRowChangeEventArgs.Row;
			if(creditCardRow.RowState == DataRowState.Deleted ||
				creditCardRow.RowState == DataRowState.Detached ||
				this.IsCreditCardRowForSide_InChangeEvent(creditCardRow))
				this.actionQueue.Enqueue(new ObjectAction(CreditCardRowChangeProc, new Object[] { creditCardRow, creditCardRowChangeEventArgs }));
			}
			catch(Exception ex)
			{
				EventLog.Information("Error in CreditCard_CreditCardRowChanging {0}\r\n{1}", ex.Message, ex.StackTrace);
			}
		}

		/// <summary>
		/// abstract method to determine if consumer is one that the side cares about
		/// this is called from the commit change event of the consumer row
		/// this is important to know because the consumer orderRow is locked
		/// and the datamodelTransaction can be tricky to use
		/// </summary>
		/// <param name="consumerRow"></param>
		/// <returns>true if consumer row is for this side</returns>
		protected abstract bool IsConsumerRowForSide_InChangeEvent(ConsumerRow consumerRow);

		/// <summary>
		/// method to determine if credit card is one that the side cares about
		/// this is called from the commit change event of the creditCard row
		/// this is important to know because the creditCard row is locked
		/// and the datamodelTransaction can be tricky to use
		/// </summary>
		/// <param name="creditCardRow"></param>
		/// <returns></returns>
		private bool IsCreditCardRowForSide_InChangeEvent(CreditCardRow creditCardRow)
		{
			Guid consumerRowId = (Guid)creditCardRow[DataModel.CreditCard.ConsumerIdColumn];

			ConsumerRow consumerRow;
			bool acquiredReader = false;
			if(DataModel.DataLock.IsWriteLockHeld == false &&
				DataModel.DataLock.IsReadLockHeld == false)
			{
				DataModel.DataLock.EnterReadLock();
				acquiredReader = true;
			}
			try
			{
				consumerRow = (ConsumerRow)((FluidTrade.Core.ClusteredIndex)DataModel.Consumer.ConsumerKey).Find(consumerRowId);
				if(consumerRow == null)
					return false;

				return this.IsConsumerRowForSide_InChangeEvent(consumerRow);

			}
			finally
			{
				if(acquiredReader == true)
					DataModel.DataLock.ExitReadLock();
			}
		}

		/// <summary>
		/// abstract method to determine if working order is one that the side cares about
		/// this is called from the commit change event of the workingOrder row
		/// this is important to know because the working orderRow is locked
		/// and the datamodelTransaction can be tricky to use
		/// </summary>
		/// <param name="row"></param>
		/// <returns>true if working order is for this side</returns>
		protected abstract bool IsWorkingOrderForSide_InChangeEvent(WorkingOrderRow row);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="parameters"></param>
		protected virtual void CreditCardRowChangeProc(Object[] key, params Object[] parameters)
		{
			CreditCardRow creditCardRow = (CreditCardRow)key[0];
			CreditCardRowChangeEventArgs creditCardRowChangeEventArgs = (CreditCardRowChangeEventArgs)key[1];
			this.ConsumerRowChangeProc(null, null, creditCardRow, true);
		}

		/// <summary>
		/// Stops the exchange.
		/// </summary>
		public void Stop()
		{

			// These event handlers must be removed from the data model.
			DataModel.Consumer.ConsumerRowChanging -= new ConsumerRowChangeEventHandler(Consumer_ConsumerRowChanging);
			DataModel.CreditCard.CreditCardRowChanging -= new CreditCardRowChangeEventHandler(CreditCard_CreditCardRowChanging);


			DataModel.WorkingOrder.WorkingOrderRowChanging -= WorkingOrder_WorkingOrderRowChanging;

			// Shut down thread that handles the trigger driven actions.
			if(!this.crossingThread.Join(threadWait))
				this.crossingThread.Abort();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="secRow"></param>
		/// <param name="dataModelTransaction"></param>
		/// <param name="returnWorkingOrderList"></param>
		/// <param name="workingOrdersMatchRowsList"></param>
		protected void GetSubmittedWorkingOrderList(SecurityRow secRow, IDataModelTransaction dataModelTransaction, 
																		List<WorkingOrderRow> returnWorkingOrderList, List<MatchRow[]> workingOrdersMatchRowsList)
		{
			WorkingOrderRow[] workingOrderRowAr;
			secRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
			try
			{
				//check that there is a working order with the submitted status
				workingOrderRowAr = secRow.GetWorkingOrderRowsByFK_Security_WorkingOrder_SecurityId_NoLockCheck();
				if(workingOrderRowAr.Length == 0)
					return;
			}
			finally
			{
				secRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
			}

			foreach(WorkingOrderRow curWorkingOrderRow in workingOrderRowAr)
			{
				curWorkingOrderRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
				try
				{
					if(curWorkingOrderRow.StatusId_NoLockCheck == ConsumerCross.SubmittedOrderStatus)
					{
						returnWorkingOrderList.Add(curWorkingOrderRow);

						if(workingOrdersMatchRowsList != null)
						{
							workingOrdersMatchRowsList.Add(curWorkingOrderRow.GetMatchRows_NoLockCheck());
						}
					}
				}
				finally
				{
					curWorkingOrderRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				}
			}
		

		}


		/// <summary>
		/// 
		/// </summary>
		internal class MatchCreateParms
		{
			public MatchCreateParms(ConsumerContainerRow containerRow,
				ConsumerDebtMatchInfo consumerDebtMatchInfo,
				ConsumerTrustMatchInfo consumerTrustMatchInfo,
				decimal matchStrength,
				Guid matchStatusId,
				MatchRow matchRow,
				Guid matchRowMatchId,
				Guid contraMatchRowMatchId)
			{
				this.matchStrength = matchStrength;
				this.matchStatusId = matchStatusId;
				this.containerRow = containerRow;
				this.consumerDebtMatchInfo = consumerDebtMatchInfo;
				this.consumerTrustMatchInfo = consumerTrustMatchInfo;
				modifiedTime = createdTime;
				modifiedUserId = createdUserId;
				this.matchRow = matchRow;
				//this.contraMatchRow = contraMatchRow;

				if(matchRow == null)
				{
					matchId = Guid.NewGuid();
					contraMatchId = Guid.NewGuid();
				}
				else
				{
					matchId = matchRowMatchId;
					contraMatchId = contraMatchRowMatchId;
				}

				consumerTrustNegotiationId = Guid.NewGuid();
				consumerDebtNegotiationId = Guid.NewGuid();
			
			}
			internal decimal matchStrength;
			internal Guid matchStatusId;
				
			internal MatchRow matchRow;
			//internal MatchRow contraMatchRow;
			internal ConsumerContainerRow containerRow;
			internal ConsumerDebtMatchInfo consumerDebtMatchInfo;
			internal ConsumerTrustMatchInfo consumerTrustMatchInfo;

		
			// These identifiers are used to match working orders without information leaking across the 'Chinese Wall'.
			internal Guid matchId;
			internal Guid contraMatchId;
			internal Guid consumerTrustNegotiationId;
			internal Guid consumerDebtNegotiationId;
			internal Int64 matchRowVersion = Int64.MinValue;
			internal Int64 contraMatchRowVersion = Int64.MinValue;

			// These variables are used for auditing the changes to this record.
			internal DateTime createdTime = DateTime.UtcNow;
			internal Guid createdUserId = TradingSupport.UserId;
			internal DateTime modifiedTime;
			internal Guid modifiedUserId;
			internal Int64 version = Int64.MinValue;

			// Both parties are given a timer that will implicitly decline the transaction when it expires.
			internal DateTime matchTime = DateTime.UtcNow;
			

		}

		/// <summary>
		/// 
		/// </summary>
		public struct PendingWorkingOrderKey
		{
			private Guid workingOrderId;
			private Guid contraWorkingOrderId;

			/// <summary>
			/// 
			/// </summary>
			/// <param name="workingOrderId"></param>
			/// <param name="contraWorkingOrderId"></param>
			public PendingWorkingOrderKey(Guid workingOrderId, Guid contraWorkingOrderId)
			{
				this.workingOrderId = workingOrderId;
				this.contraWorkingOrderId = contraWorkingOrderId;
			}

			/// <summary>
			/// 
			/// </summary>
			public Guid WorkingOrderId { get { return this.workingOrderId; } }
			
			/// <summary>
			/// 
			/// </summary>
			public Guid ContraWorkingOrderId { get { return this.contraWorkingOrderId; } }
		}
	}
}