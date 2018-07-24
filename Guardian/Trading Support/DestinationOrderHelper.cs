namespace FluidTrade.Guardian
{

	using System;
	using System.Collections.Generic;
	using System.Transactions;

	internal class DestinationOrderHelper
	{

		internal static void ClearDestinationOrders()
		{

            int batchCounter = 0;
            int batchSize = 1000;

			DataModel dataModel = new DataModel();

			try
			{

                DataModel.DataLock.EnterReadLock();

                // This context is used to keep track of the locks aquired for the ancillary data.
                TransactionScope transactionScope = new TransactionScope();
                DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

                List<ExecutionRow> executionRows = new List<ExecutionRow>();
                foreach (ExecutionRow executionRow in DataModel.Execution.Rows)
                    executionRows.Add(executionRow);

				// Destroy all the executions.
				foreach (ExecutionRow executionRow in executionRows)
				{

					executionRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
					dataModelTransaction.AddLock(executionRow);
					dataModel.DestroyExecution(new object[] { executionRow.ExecutionId }, executionRow.RowVersion);

                    if (batchCounter++ == batchSize)
                    {
                        batchCounter = 0;
                        transactionScope.Complete();
                        transactionScope.Dispose();
                        transactionScope = new TransactionScope();
                        dataModelTransaction = DataModelTransaction.Current;
                    }

				}

                List<DestinationOrderRow> destinationOrderRows = new List<DestinationOrderRow>();
                foreach (DestinationOrderRow destinationOrderRow in DataModel.DestinationOrder.Rows)
                    destinationOrderRows.Add(destinationOrderRow);

				// Destroy all the Destination Orders.
				foreach (DestinationOrderRow destinationOrderRow in destinationOrderRows)
				{

					destinationOrderRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
					dataModelTransaction.AddLock(destinationOrderRow);
					dataModel.DestroyDestinationOrder(new object[] { destinationOrderRow.DestinationOrderId }, destinationOrderRow.RowVersion);

                    if (batchCounter++ == batchSize)
                    {
                        batchCounter = 0;
                        transactionScope.Complete();
                        transactionScope.Dispose();
                        transactionScope = new TransactionScope();
                        dataModelTransaction = DataModelTransaction.Current;
                    }

				}

                transactionScope.Complete();
                transactionScope.Dispose();

			}
			finally
			{
				DataModel.DataLock.ExitReadLock();
			}

		}

		/// <summary>
		/// Creates one or more destination orders.
		/// </summary>
		/// <param name="destinationOrders"></param>
		internal static void CreateDestinationOrders(DestinationOrderInfo[] destinationOrders)
        {

            // An instance of the shared data model is required to use its methods.
            DataModel dataModel = new DataModel();

            // Business logic: provide the current time and the user identifier for the new destination order.
			DateTime dateTime = DateTime.UtcNow;
            Guid userId = TradingSupport.UserId;

			DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

			StatusRow statusNewRow = DataModel.Status.StatusKeyStatusCode.Find(Status.New);
			statusNewRow.AcquireReaderLock(dataModelTransaction);

			StateRow stateIntialRow = DataModel.State.StateKeyStateCode.Find(State.Initial);
			stateIntialRow.AcquireReaderLock(dataModelTransaction);

            // The entire collection of destination orders is added or rejected as a single transaction.
            foreach (DestinationOrderInfo destinationOrderInfo in destinationOrders)
            {
                Guid destinationOrderId = Guid.NewGuid();
                dataModel.CreateDestinationOrder(
					destinationOrderInfo.BlotterId, 
					0.0M, 
					null, 
					dateTime, 
					userId, 
					destinationOrderInfo.DestinationId,
                    destinationOrderId, 
					null, 
					null, 
					false, 
					false, 
					null, 
					dateTime, 
					userId,
                    destinationOrderInfo.OrderedQuantity, 
					destinationOrderInfo.OrderTypeId, 
					destinationOrderInfo.SecurityId,
                    dateTime, 
					destinationOrderInfo.SettlementId, 
					destinationOrderInfo.SideCodeId, 
					stateIntialRow.StateId,
					statusNewRow.StatusId,
                    null, 
					destinationOrderInfo.TimeInForceCodeId, 
					dateTime, 
					null, 
					destinationOrderInfo.WorkingOrderId);
            }


        }

        /// <summary>
        /// Creates one or more destination orders.
        /// </summary>        
		internal static void DestroyDestinationOrders(DestinationOrderReference[] destinationOrderReferences)
        {

            // An instance of the shared data model is required to use its methods.
            DataModel dataModel = new DataModel();

            // The entire collection of destination orders is added or rejected as a single transaction.
            foreach (DestinationOrderReference destinationOrderReference in destinationOrderReferences)
                dataModel.DestroyDestinationOrder(
                    new object[] { destinationOrderReference.DestinationId },
                    destinationOrderReference.RowVersion);

        }
    
    }

}
