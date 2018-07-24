namespace FluidTrade.Guardian
{
	using System;
	using System.Data;
	using System.Security.Principal;
	using System.Threading;
	using System.Transactions;
	using FluidTrade.Core;
	using FluidTrade.Guardian.GCSWebServiceClient.GCSWebServiceReference;
	
	/// <remarks>
	/// Daemon process to pull savings balance from GCS.
	/// </remarks>
	public sealed class SavingsBalance : IExchange
	{	
		internal Thread savingsBalanceThread;
		internal WaitQueue<Guid> actionQueue;
		//Sleep for a minute
		internal const int ThreadSleep = 60000;
		//internal System.Timers.Timer reconcileScheduler;
		internal DateTime lastRunTime = DateTime.Now.Date.AddDays(-1);

		/// <summary>
		/// Construction
		/// </summary>
		public SavingsBalance()
		{
			this.actionQueue = new WaitQueue<Guid>();
		}

		/// <summary>;
		/// Part of the IExchange interface.  This is the entry point for the deaemon.
		/// </summary>
		public void Start()
		{

			DataModel.ConsumerTrust.ConsumerTrustRowChanged += new ConsumerTrustRowChangeEventHandler(OnConsumerTrustRowChanged);			
			savingsBalanceThread = new Thread(new ThreadStart(this.GetBalanceThread));			
			savingsBalanceThread.IsBackground = true;
			savingsBalanceThread.Start();
			//long timerInterval = CalculateNextInterval();

			//reconcileScheduler = new System.Timers.Timer(timerInterval);
			//reconcileScheduler.Elapsed += new System.Timers.ElapsedEventHandler(OnReconcileSchedulerElapsed);
			//reconcileScheduler.Start();
		}

		void OnReconcileSchedulerElapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			//reconcileScheduler.Stop();
			try
			{
				//Sanity Check
				TimeSpan reconcileTime = FluidTrade.Guardian.GCSWebServiceClient.Properties.Settings.Default.BalanceCheckTime;
				DateTime timeToRun = DateTime.Now.Date + reconcileTime;

				double interval = (DateTime.Now - timeToRun).TotalSeconds;
				

				EventLog.Information("Payment Schedule Reconciling started");

				EventLog.Information("Payment Schedule Reconciling stopped");
		
			}
			finally
			{
				//reconcileScheduler.Interval = CalculateNextInterval();
				//reconcileScheduler.Start();
			}
			

		}

		/// <summary>
		/// 
		/// </summary>
		private  long CalculateNextInterval()
		{
			TimeSpan reconcileTime = FluidTrade.Guardian.GCSWebServiceClient.Properties.Settings.Default.BalanceCheckTime;
			long timerInterval;

			try
			{
				DateTime midnightToday = DateTime.Now.Date;
				DateTime timeToRun = midnightToday + reconcileTime;
				timerInterval = (timeToRun - DateTime.Now).Milliseconds;
			}
			catch
			{
				timerInterval = (60 * 60 * 1000);
			}

			return timerInterval;
		}
		
		/// <summary>
		/// Stop the the daemon
		/// </summary>
		public void Stop()
		{
			savingsBalanceThread.Abort();
		}


		/// <summary>
		/// Pulls actions and their parameters off the queue and executes them.
		/// </summary>
		void GetBalanceThread()
		{

			// All the actions added to the generic list of actions and parameter will execute with this claims principal.
			Thread.CurrentPrincipal = new ClaimsPrincipal(WindowsIdentity.GetCurrent(), null);
			Thread.CurrentPrincipal = new ClaimsPrincipal(new GenericIdentity("NT AUTHORITY\\NETWORK SERVICE"), null);

			Thread.Sleep(ThreadSleep);
			bool timedOut = false;
		
			// The event handlers for the data model can't wait on locks and resources outside the data model.  There would simply be too many resources that 
			// could deadlock.  This code will pull requests off of a generic queue of actions and parameters and execute them using the authentication created
			// above.
			while (true)
			{
				//Sleep if there is nothing in the Queue.
				if (this.actionQueue.Count == 0)
				{
					Thread.Sleep(ThreadSleep);
				}
				else
				{

					try
					{
						// The thread will wait here for the timedoutPeriod. 
						Guid consumerTrustId = this.actionQueue.Dequeue(ThreadSleep, out timedOut);

						if (timedOut == false)
							GetSavingsBalance(consumerTrustId);
						else
							System.Diagnostics.Debug.Assert(timedOut, "We should not be here if there is nothing in the queue");

					}
					catch (ThreadAbortException)
					{
                        return;
					}
					catch (Exception exception)
					{

						// This will catch any exceptions thrown during the processing of the generic actions.
						EventLog.Error("{0} {1}: {2}\r\n{3}", this.ThreadName, exception.Message, exception.ToString(), exception.StackTrace);

					}
				}
			}
		}

		/// <summary>
		/// Prototype method.  This will eventually take list of guids to update
		/// </summary>
		/// <param name="consumerTrustId"></param>
		private void GetSavingsBalance(Guid consumerTrustId)
		{
			string gcsAccountNumber = String.Empty;
			using (TransactionScope transactionScope = new TransactionScope())
			{				
				// This provides a context for any transactions.
				DataModelTransaction dataModelTransaction = DataModelTransaction.Current;
				ConsumerTrustRow consumerTrustRow = DataModel.ConsumerTrust.ConsumerTrustKey.Find(consumerTrustId);
				consumerTrustRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
				try
				{					
					if (consumerTrustRow.IsSavingsAccountNull() == false) 
						gcsAccountNumber = consumerTrustRow.SavingsAccount;						
				}
				finally
				{						
					consumerTrustRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				}

			}

			//If there is no GCS account number then we have no need to continue processing.
			//HACK  add logging for empty account numbers
			if (String.IsNullOrEmpty(gcsAccountNumber) == false)
			{
				Decimal savingsBalance = GetSavingsBalance(gcsAccountNumber);
				if (savingsBalance != -1.0M)
					UpdateSavingsBalance(consumerTrustId, savingsBalance);

			}

		}

		/// <summary>
		/// Update the SavingsBalance
		/// </summary>
		/// <param name="consumerTrustId"></param>
		/// <param name="savingsBalance"></param>
		private void UpdateSavingsBalance(Guid consumerTrustId, decimal savingsBalance)
		{
			// An instance of the data model is required for CRUD operations.
			DataModel dataModel = new DataModel();

			using (TransactionScope transactionScope = new TransactionScope())
			{
				Int64 consumerTrustRowVersion;
				// This provides a context for any transactions.
				DataModelTransaction dataModelTransaction = DataModelTransaction.Current;
				ConsumerTrustRow consumerTrustRow = DataModel.ConsumerTrust.ConsumerTrustKey.Find(consumerTrustId);
				consumerTrustRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
				try
				{
					consumerTrustRowVersion = consumerTrustRow.RowVersion;
				}
				finally
				{
					//Always release the lock.
					consumerTrustRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				}

				dataModel.UpdateConsumerTrust(
					null,
					null,
					new Object[] { consumerTrustId },
					null,
					null,
					consumerTrustRowVersion,
					null,
					savingsBalance,
					null,
					null,
					null);
			}

		}

		/// <summary>
		/// Get savings Balance
		/// </summary>
		/// <param name="gcsAccountNumber"></param>
		/// <returns></returns>
		private Decimal GetSavingsBalance(string gcsAccountNumber)
		{
			try
			{
				//Initialize and call 
				//HACK --- secure user name password.
				WebServicesSoapClient client = new WebServicesSoapClient("GCSWebService");
				int? rowcount = 0, pagecount = 0;
				AccountsWSDS ds = client.AccountsGetADO("testwebsrvcDT", "53rV1c3", gcsAccountNumber, String.Empty, String.Empty, String.Empty, null, null, ref rowcount, ref pagecount);

				AccountsWSDS.ACCOUNTSDataTable accountsTable = ds.Tables["ACCOUNTS"] as AccountsWSDS.ACCOUNTSDataTable;
				return (Decimal)accountsTable.Rows[0][accountsTable.ACCOUNT_BALANCEColumn.ColumnName];
				
			}
			catch
			{
			}

			return -1.0M;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="consumerTrustRowChangeEventArgs"></param>
		void OnConsumerTrustRowChanged(object sender, ConsumerTrustRowChangeEventArgs consumerTrustRowChangeEventArgs)
		{
			if (consumerTrustRowChangeEventArgs.Action != DataRowAction.Add)
				return;

			// Extract the unique working order identifier from the generic event arguments.  The identifier is needed for the handler that creates crosses
			// when the right conditions occur.
			ConsumerTrustRow consumerTrustRow = consumerTrustRowChangeEventArgs.Row;

			//get the consumerId before the row has a chance to go away			
			this.actionQueue.Enqueue(consumerTrustRow.ConsumerTrustId);
		}

		/// <summary>
		/// 
		/// </summary>
		string ThreadName
		{
			get { return "SavingsBalance"; }
		}
	}
}
