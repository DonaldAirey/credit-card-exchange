namespace FluidTrade.Guardian
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Linq;
	using System.Security.Principal;
	using System.Threading;
	using System.Transactions;
	using FluidTrade.Core;
	using FluidTrade.Guardian.GCSWebServiceClient.GCSWebServiceReference;

	/// <summary>
	/// 
	/// </summary>
	public class PaymentDealer : IExchange
	{
			
		internal Thread paymentDealerThread;
		internal WaitQueue<ObjectAction> actionQueue;
		//Sleep for a minute
		internal const int ThreadSleep = 60000;
		internal const Int32 deadlockRetiesMax = 3;

		/// <summary>
		/// 
		/// </summary>
		public PaymentDealer()
		{
			this.actionQueue = new WaitQueue<ObjectAction>();
		}


		/// <summary>
		/// 
		/// </summary>
		public void Start()
		{

			DataModel.ConsumerTrustSettlement.ConsumerTrustSettlementRowChanged += new ConsumerTrustSettlementRowChangeEventHandler(OnConsumerTrustSettlementRowChanged);
			paymentDealerThread = new Thread(new ThreadStart(this.PaymentDealerThread));
			paymentDealerThread.IsBackground = true;
			paymentDealerThread.Start();
			
		}

		void ConsumerTrustSettlement_ConsumerTrustSettlementRowChanged(object sender, ConsumerTrustSettlementRowChangeEventArgs e)
		{
			throw new NotImplementedException();
		}

		
		
		/// <summary>
		/// 
		/// </summary>
		public void Stop()
		{
			paymentDealerThread.Abort();
		}
		


		/// <summary>
		/// Pulls actions and their parameters off the queue and executes them.
		/// </summary>
		void PaymentDealerThread()
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
						ObjectAction objectAction = this.actionQueue.Dequeue(ThreadSleep, out timedOut);

						if (timedOut == false)
							objectAction.DoAction(objectAction.Key, objectAction.Parameters);
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
		/// This is what we care about.  If there is a new ConsumerTrustSettlement then we know that it has been approved
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="consumerTrustRowChangeEventArgs"></param>
		void OnConsumerTrustSettlementRowChanged(object sender, ConsumerTrustSettlementRowChangeEventArgs consumerTrustRowChangeEventArgs)
		{
			//HACK ***** We only care about addition at this point. Delete will be handled later
			if (consumerTrustRowChangeEventArgs.Action == DataRowAction.Commit )
			{
				
				//get the settlementId before the row has a chance to go away
				Guid settlementId = (Guid)consumerTrustRowChangeEventArgs.Row[DataModel.ConsumerTrustSettlement.ConsumerTrustNegotiationIdColumn, DataRowVersion.Current];
				this.actionQueue.Enqueue(new ObjectAction(SettlementRowAddProc, new Object[] { settlementId, consumerTrustRowChangeEventArgs, null }));
			}
		}


		/// <summary>
		/// Delegate for Settlement row change event
		/// </summary>
		/// <param name="key"></param>
		/// <param name="parameters"></param>
		private void SettlementRowAddProc(Object[] key, params Object[] parameters)
		{
			Guid settlementId = (Guid)key[0];
			ConsumerTrustSettlementRow rawSettlementRow = ((ConsumerTrustSettlementRowChangeEventArgs)key[1]).Row;
			ProcessSettlement(rawSettlementRow);
		}

		/// <summary>
		/// Extract information from the settlementRow to send to GCS
		/// </summary>
		/// <param name="rawSettlementRow"></param>
		private void ProcessSettlement(ConsumerTrustSettlementRow rawSettlementRow)
		{
			List<PaymentInfo> paymentInfoList = new List<PaymentInfo>();
			String gcsAccountNumber = String.Empty;

			try
			{
				using (TransactionScope transactionScope = new TransactionScope())
				{
					// This provides a context for any transactions.
					DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

					if (rawSettlementRow != null)
					{
						rawSettlementRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
						bool settlementDeletedOrDetached = false;
						try
						{
							settlementDeletedOrDetached = rawSettlementRow.RowState == DataRowState.Deleted ||
															rawSettlementRow.RowState == DataRowState.Detached;

						}
						finally
						{
							rawSettlementRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
						}

						//If the settlement was removed by the time we got to this, then there is no need to continue
						if (settlementDeletedOrDetached == true)
						{
							return;
						}
					}

					RowLockingWrapper<ConsumerTrustSettlementRow> settlementRow = new RowLockingWrapper<ConsumerTrustSettlementRow>(rawSettlementRow, dataModelTransaction);
					settlementRow.AcquireReaderLock();
					try
					{

						//We need GCS account id for this payment.  We cannot find a GCS account number then we cannot process this payment.
						Guid creditCardId = Guid.Empty;
						ConsumerTrustNegotiationRow negotiationRow = settlementRow.TypedRow.ConsumerTrustNegotiationRow;
						negotiationRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
						try
						{
							creditCardId = negotiationRow.CreditCardId;
						}
						finally
						{
							negotiationRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
							//no longer usable
							negotiationRow = null;
						}

						//HACK - add error reporting
						if (creditCardId == Guid.Empty)
							return;

						//Determine the consumer
						Guid consumerId = Guid.Empty;
						CreditCardRow creditcardRow = DataModel.CreditCard.CreditCardKey.Find(creditCardId);
						creditcardRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
						try
						{
							consumerId = creditcardRow.ConsumerId;
						}
						finally
						{
							creditcardRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
							//no longer usable
							creditcardRow = null;
						}

						//HACK - add error reporting
						if (consumerId == Guid.Empty)
							return;

						//Determine the consumerTrust
						ConsumerTrustRow consumerTrustRow = null;
						ConsumerRow consumerRow = DataModel.Consumer.ConsumerKey.Find(consumerId);
						consumerRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
						try
						{
							consumerTrustRow = consumerRow.GetConsumerTrustRows_NoLockCheck().First();
						}
						finally
						{
							consumerRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
							//no longer usable
							consumerRow = null;
						}

						//HACK - add error reporting
						if (consumerTrustRow == null)
							return;

						consumerTrustRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
						try
						{
							gcsAccountNumber = (consumerTrustRow.IsSavingsAccountNull()) ? String.Empty : consumerTrustRow.SavingsAccount;
						}
						finally
						{
							consumerTrustRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
							//no longer usable
							consumerTrustRow = null;
						}

						//HACK - add error reporting
						if (String.IsNullOrEmpty(gcsAccountNumber))
							return;


						foreach (ConsumerTrustPaymentRow paymentRow in settlementRow.TypedRow.GetConsumerTrustPaymentRows())
						{
							paymentRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
							try
							{
								if (paymentRow.RowState != DataRowState.Deleted &&
									paymentRow.RowState != DataRowState.Detached)
								{
									paymentInfoList.Add(new PaymentInfo(paymentRow));
								}

							}
							finally
							{
								paymentRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
							}
						}


					}
					finally
					{
						settlementRow.ReleaseReaderLock();
					}
				}
			}
			catch(Exception ex)
			{
				EventLog.Error(ex);
			}

			if (String.IsNullOrEmpty(gcsAccountNumber) == false && paymentInfoList.Count != 0)
			{
				SendPaymentsToGCS(gcsAccountNumber, paymentInfoList);
			}
		}

		/// <summary>
		/// Sends the payment info to GCS
		/// </summary>
		/// <param name="gcsAccountNumber"></param>
		/// <param name="paymentInfoList"></param>
		/// <returns>PaymentWSDS</returns>
		private PaymentsWSDS SendPaymentsToGCS(string gcsAccountNumber, List<PaymentInfo> paymentInfoList)
		{
			PaymentsWSDS returnedPayment = null;
			try
			{
				WebServicesSoapClient client = new WebServicesSoapClient("GCSWebService");
				//String webServiceUserName, webServicePassword;
				
				int? rowcount = 0, pagecount = 0;
				PaymentsWSDS   test = client.PaymentsDirectPayGetADO("testwebsrvcDT", "53rV1c3","", null, "6036335099003114", "", "",
					"", "", null, null, "", "", "", "", null, "", null, "", "", "", "", "", "", "",
					"", "", "", "", null, null, ref rowcount, ref pagecount);

				DataTable t = test.Tables[0];
				DataTable t1 = test.Tables[1];
				DataTable t2 = test.Tables[2];

				PaymentsWSDS paymentDataAdo = new PaymentsWSDS(gcsAccountNumber, paymentInfoList);
				returnedPayment = client.PaymentsSetADO("testwebsrvcDT", "53rV1c3", paymentDataAdo);

			}
			catch (Exception ex)
			{
				EventLog.Error(ex);
			}

			return returnedPayment;

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
		/// Name of the Thread for diagnostic purposes
		/// </summary>
		string ThreadName
		{
			get { return "PaymentDealer"; }
		}

	}
}

