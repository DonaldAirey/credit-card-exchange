namespace FluidTrade.Guardian
{

	using System;
	using System.Data;
	using System.ServiceModel;
	using FluidTrade.Core;
	using FluidTrade.Guardian.Records;
	using System.Collections.Generic;

    /// <summary>
    /// Summary description for DestinationOrder.
    /// </summary>
    /// <copyright>Copyright © 2001-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
    public class DestinationOrder
    {

        // Hash table used to translate a state into an index into the state change matrix.
        internal static ObjectHandler[,] statusChangeMatrix;

        // Private Static Fields
        private static DataModel dataModel;

        /// <summary>
        /// Initialize the static elements of the DestinationOrder.
        /// </summary>
        static DestinationOrder()
        {

            // An instance of the data model is required to make changes to it.
            DestinationOrder.dataModel = new DataModel();

            // A matrix is used to to find a delegate that handles the change from one status to another.  Each possible status transition of the destination
            // order must have a handler installed in this table.
            Int32 statuses = Enum.GetValues(typeof(Status)).Length;
            DestinationOrder.statusChangeMatrix = new ObjectHandler[statuses, statuses];
            DestinationOrder.statusChangeMatrix[(Int32)Status.New, (Int32)Status.PartiallyFilled] = OnPartialAction;
            DestinationOrder.statusChangeMatrix[(Int32)Status.New, (Int32)Status.Filled] = OnFilledAction;
            DestinationOrder.statusChangeMatrix[(Int32)Status.New, (Int32)Status.Canceled] = OnNoAction;
            DestinationOrder.statusChangeMatrix[(Int32)Status.New, (Int32)Status.Deleted] = OnNoAction;
            DestinationOrder.statusChangeMatrix[(Int32)Status.New, (Int32)Status.Error] = SetErrorAction;
            DestinationOrder.statusChangeMatrix[(Int32)Status.PartiallyFilled, (Int32)Status.New] = OnCanceledAction;
            DestinationOrder.statusChangeMatrix[(Int32)Status.PartiallyFilled, (Int32)Status.Filled] = OnFilledAction;
            DestinationOrder.statusChangeMatrix[(Int32)Status.PartiallyFilled, (Int32)Status.Canceled] = OnCanceledAction;
            DestinationOrder.statusChangeMatrix[(Int32)Status.PartiallyFilled, (Int32)Status.Deleted] = OnCanceledAction;
            DestinationOrder.statusChangeMatrix[(Int32)Status.PartiallyFilled, (Int32)Status.Error] = SetErrorAction;
            DestinationOrder.statusChangeMatrix[(Int32)Status.Filled, (Int32)Status.New] = OnCanceledAction;
            DestinationOrder.statusChangeMatrix[(Int32)Status.Filled, (Int32)Status.PartiallyFilled] = OnPartialAction;
            DestinationOrder.statusChangeMatrix[(Int32)Status.Filled, (Int32)Status.Canceled] = OnCanceledAction;
            DestinationOrder.statusChangeMatrix[(Int32)Status.Filled, (Int32)Status.Deleted] = OnCanceledAction;
            DestinationOrder.statusChangeMatrix[(Int32)Status.Filled, (Int32)Status.Error] = SetErrorAction;
            DestinationOrder.statusChangeMatrix[(Int32)Status.Error, (Int32)Status.New] = ClearErrorAction;
            DestinationOrder.statusChangeMatrix[(Int32)Status.Error, (Int32)Status.PartiallyFilled] = ClearErrorAction;
            DestinationOrder.statusChangeMatrix[(Int32)Status.Error, (Int32)Status.Canceled] = ClearErrorAction;
            DestinationOrder.statusChangeMatrix[(Int32)Status.Error, (Int32)Status.Deleted] = ClearErrorAction;
            DestinationOrder.statusChangeMatrix[(Int32)Status.Error, (Int32)Status.Filled] = ClearErrorAction;

        }

        /// <summary>
        /// Handler for validating Destination Order records.
        /// </summary>
        /// <param name="sender">The object that originated the event.</param>
        /// <param name="e">The event arguments.</param>
        internal static void OnDestinationOrderRowValidate(object sender, DestinationOrderRowChangeEventArgs e)
        {

            Int32 currentStatusCode;
            Decimal destinationOrderQuantity;
            DataModelTransaction dataModelTransaction;
            Int32 previousStatusCode;
            Decimal sourceOrderQuantity;
            WorkingOrderRow workingOrderRow;

            // The Business Rules will be enforced on this Destination Order.  Note that it is locked at the point this handler is called.
            DestinationOrderRow destinationOrderRow = e.Row;

            // The action on the row determines which rule to evaluate.
            switch (destinationOrderRow.RowState)
            {

            case DataRowState.Added:

                // This rule will reject the operation if the Working Order is overcommitted with a destination.
                dataModelTransaction = DataModelTransaction.Current;

                // This rule will throw an exception if the quantity sent to a destination is greater than the quantity ordered.  The quantity ordered and 
                // quantity sent can only be calcuated from the owning Working Order which must be locked in order to carry out the calculations.
                workingOrderRow = e.Row.WorkingOrderRow;
                workingOrderRow.AcquireReaderLock(dataModelTransaction);
                sourceOrderQuantity = WorkingOrder.GetSourceOrderQuantity(dataModelTransaction, workingOrderRow);
                destinationOrderQuantity = WorkingOrder.GetDestinationOrderQuantity(dataModelTransaction, workingOrderRow);
                if (sourceOrderQuantity < destinationOrderQuantity)
                    throw new FaultException<DestinationQuantityFault>(
                        new DestinationQuantityFault(workingOrderRow.WorkingOrderId, sourceOrderQuantity, destinationOrderQuantity));

                break;

            case DataRowState.Modified:

                // Reject the operation if the Working Order is overcommitted with a destination.  Note that the order of the evaluation is important for
                // efficiency.  There is no need to sum up the source and Destination Orders if the quantity has been reduced as there's no chance it will be
                // over committed.
                Decimal originalQuantity = (Decimal)destinationOrderRow[DataModel.DestinationOrder.OrderedQuantityColumn, DataRowVersion.Original];
                Decimal currentQuantity = (Decimal)destinationOrderRow[DataModel.DestinationOrder.OrderedQuantityColumn, DataRowVersion.Current];
                if (originalQuantity < currentQuantity)
                {

                    // Once it's determined that the order can be overcommitted, a middle tier context is required to lock the rows so the quantities can be 
                    // aggregated.
                    dataModelTransaction = DataModelTransaction.Current;

                    // This rule will throw an exception if the quantity sent to a destination is greater than the quantity ordered.  The quantity ordered and
                    // quantity sent can only be calcuated from the owning Working Order which must be locked in order to carry out the calculations.
                    workingOrderRow = e.Row.WorkingOrderRow;
                    workingOrderRow.AcquireReaderLock(dataModelTransaction);
                    sourceOrderQuantity = WorkingOrder.GetSourceOrderQuantity(dataModelTransaction, workingOrderRow);
                    destinationOrderQuantity = WorkingOrder.GetDestinationOrderQuantity(dataModelTransaction, workingOrderRow);
                    if (sourceOrderQuantity < destinationOrderQuantity)
                        throw new FaultException<DestinationQuantityFault>(
                            new DestinationQuantityFault(workingOrderRow.WorkingOrderId, sourceOrderQuantity, destinationOrderQuantity));

                }

                // This rule will examine the effects of a state change of the Destination Order.  The stateChangeMatrix contains delegates to methods that
                // will determine what, if any, stats change should be applied to the Working Order due to the change in state of this Destination Order.
                previousStatusCode = Convert.ToInt32(StatusMap.FromId((Guid)destinationOrderRow[DataModel.DestinationOrder.StatusIdColumn, DataRowVersion.Original]));
                currentStatusCode = Convert.ToInt32(StatusMap.FromId((Guid)destinationOrderRow[DataModel.DestinationOrder.StatusIdColumn, DataRowVersion.Current]));
                if (previousStatusCode != currentStatusCode)
                    DestinationOrder.statusChangeMatrix[previousStatusCode, currentStatusCode](new Object[] { destinationOrderRow.WorkingOrderId });

                break;

            case DataRowState.Deleted:

                // This rule will examine the effects of a state change of the Destination Order.  The stateChangeMatrix contains delegates to methods that
                // will determine what, if any, stats change should be applied to the Working Order due to the change in state of this Destination Order.
                previousStatusCode = Convert.ToInt32(StatusMap.FromId((Guid)destinationOrderRow[DataModel.DestinationOrder.StatusIdColumn, DataRowVersion.Original]));
                currentStatusCode = (Int32)Status.Deleted;
                Guid workingOrderId = (Guid)destinationOrderRow[DataModel.DestinationOrder.WorkingOrderIdColumn, DataRowVersion.Original];
                DestinationOrder.statusChangeMatrix[previousStatusCode, currentStatusCode](new Object[] { workingOrderId });

                break;

            }

        }

        /// <summary>
        /// No action is performed for a status change.
        /// </summary>
        /// <param name="key">The key value of the record that was updated.</param>
        /// <param name="parameters">The event data.</param>
        private static void OnNoAction(Object[] key, params Object[] parameters)
        {
        }

        /// <summary>
        /// Changes the state of the Working Order to reflect a partially filled status.
        /// </summary>        
        private static void OnPartialAction(Object[] key, params Object[] parameters)
        {

            // A middle tier context is also required for a transacted update.
            DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

            // It is possible that the Working Order that is the object of this status update operation may have been deleted since the action was
            // created.  This is not an error condition.  If there is no Working Order to update, then the operation is just terminated prematurely.
            WorkingOrderRow workingOrderRow = DataModel.WorkingOrder.WorkingOrderKey.Find(key);
            if (workingOrderRow == null)
                return;
            workingOrderRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
            dataModelTransaction.AddLock(workingOrderRow);
            if (workingOrderRow.RowState == DataRowState.Detached)
                return;

            // The error status on a Working Order cannot be cleared with a partial fill.  Otherwise, a partial fill on any of the Destination Orders is 
            // reflected in the status of the parent Working Order.
            if (workingOrderRow.StatusId != StatusMap.FromCode(Status.Error))
                if (workingOrderRow.StatusId != StatusMap.FromCode(Status.PartiallyFilled))
                    UpdateWorkingOrderStatus(workingOrderRow, Status.PartiallyFilled);

        }

        /// <summary>
        /// Changes the state of the Working Order to reflect a filledly filled state.
        /// </summary>        
        private static void OnFilledAction(Object[] key, params Object[] parameters)
        {

            // A middle tier context is also required for a transacted update.
            DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

            // It is possible that the Working Order that is the object of this status update operation may have been deleted since the action was
            // created.  This is not an error condition.  If there is no Working Order to update, then the operation is just terminated prematurely.
            WorkingOrderRow workingOrderRow = DataModel.WorkingOrder.WorkingOrderKey.Find(key);
            if (workingOrderRow == null)
                return;
            workingOrderRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
            dataModelTransaction.AddLock(workingOrderRow);
            if (workingOrderRow.RowState == DataRowState.Detached)
                return;

            // The error status on a Working Order cannot be cleared with a fill.
            if (workingOrderRow.StatusId != StatusMap.FromCode(Status.Error))
            {

                // The Working Order is 'Filled' when the quantity executed is the same as the quantity ordered.
                Decimal quantityOrdered = WorkingOrder.GetSourceOrderQuantity(dataModelTransaction, workingOrderRow);
                Decimal quantityExecuted = WorkingOrder.GetExecutionQuantity(dataModelTransaction, workingOrderRow);
                if (quantityOrdered == quantityExecuted)
                    UpdateWorkingOrderStatus(workingOrderRow, Status.Filled);

            }

        }

        /// <summary>
        /// Changes the state of the Working Order to reflect a transition of a Destination Order back to the 'new' state.
        /// </summary>        
        private static void OnCanceledAction(Object[] key, params Object[] parameters)
        {

            // A middle tier context is also required for a transacted update.
            DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

            // It is possible that the Working Order that is the object of this status update operation may have been deleted since the action was
            // created.  This is not an error condition.  If there is no Working Order to update, then the operation is just terminated prematurely.
            WorkingOrderRow workingOrderRow = DataModel.WorkingOrder.WorkingOrderKey.Find(key);
            if (workingOrderRow == null)
                return;
            workingOrderRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
            dataModelTransaction.AddLock(workingOrderRow);
            if (workingOrderRow.RowState == DataRowState.Detached)
                return;

            // The error status cannot be cleared by cancelling a a Destination Order.
            if (workingOrderRow.StatusId != StatusMap.FromCode(Status.Error))
            {

                // The aggregates will determine the new state of the Working Order.
                Decimal quantityExecuted = WorkingOrder.GetExecutionQuantity(dataModelTransaction, workingOrderRow);
                Decimal quantityOrdered = WorkingOrder.GetSourceOrderQuantity(dataModelTransaction, workingOrderRow);

                // This restores the 'New' status when the canceled Destination Order was the only order with any fills.
                if (quantityExecuted == 0.0M && workingOrderRow.StatusId != StatusMap.FromCode(Status.New))
                    UpdateWorkingOrderStatus(workingOrderRow, Status.New);

                // This restores the 'Partially Filled' status when other executions remain.
                if (0.0M < quantityExecuted && quantityExecuted < quantityOrdered && workingOrderRow.StatusId != StatusMap.FromCode(Status.PartiallyFilled))
                    UpdateWorkingOrderStatus(workingOrderRow, Status.PartiallyFilled);

            }

        }

        /// <summary>
        /// Handles a Destination Order entering the error state.
        /// </summary>        
        private static void SetErrorAction(Object[] key, params Object[] parameters)
        {

            // A middle tier context is also required for a transacted update.
            DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

            // It is possible that the Working Order that is the object of this status update operation may have been deleted since the action was
            // created.  This is not an error condition.  If there is no Working Order to update, then the operation is just terminated prematurely.
            WorkingOrderRow workingOrderRow = DataModel.WorkingOrder.WorkingOrderKey.Find(key);
            if (workingOrderRow == null)
                return;
            workingOrderRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
            dataModelTransaction.AddLock(workingOrderRow);
            if (workingOrderRow.RowState == DataRowState.Detached)
                return;

            // The Working Order reflects an 'Error' Status if any of its Destination Orders have an error.
            if (workingOrderRow.StatusId != StatusMap.FromCode(Status.Error))
                UpdateWorkingOrderStatus(workingOrderRow, Status.Error);

        }

        /// <summary>
        /// Changes the state of the Working Order to reflect a filledly filled state.
        /// </summary>        
        private static void ClearErrorAction(Object[] key, params Object[] parameters)
        {

            // A middle tier context is also required for a transacted update.
            DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

            // It is possible that the Working Order that is the object of this status update operation may have been deleted since the action was
            // created.  This is not an error condition.  If there is no Working Order to update, then the operation is just terminated prematurely.
            WorkingOrderRow workingOrderRow = DataModel.WorkingOrder.WorkingOrderKey.Find(key);
            if (workingOrderRow == null)
                return;
            workingOrderRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
            dataModelTransaction.AddLock(workingOrderRow);
            if (workingOrderRow.RowState == DataRowState.Detached)
                return;

            // The 'Error' status is only cleared when all the Destination Orders are valid.
            Boolean isErrorStatus = false;
            foreach (DestinationOrderRow siblingOrderRow in workingOrderRow.GetDestinationOrderRows())
            {
                siblingOrderRow.AcquireReaderLock(dataModelTransaction);
                if (siblingOrderRow.StatusId == StatusMap.FromCode(Status.Error))
                {
                    isErrorStatus = true;
                    break;
                }
            }

            // The proper Working Order status must be evaluated when the error status is cleared.
            if (!isErrorStatus)
            {

                // The aggregates will determine the new state of the Working Order.
                Decimal quantityExecuted = WorkingOrder.GetExecutionQuantity(dataModelTransaction, workingOrderRow);
                Decimal quantityOrdered = WorkingOrder.GetSourceOrderQuantity(dataModelTransaction, workingOrderRow);

                // This restores the 'New' status when the canceled Destination Order was the only order with any fills.
                if (quantityExecuted == 0.0M && workingOrderRow.StatusId != StatusMap.FromCode(Status.New))
                    UpdateWorkingOrderStatus(workingOrderRow, Status.New);

                // This restores the 'Partially Filled' status when other executions remain.
                if (0.0M < quantityExecuted && quantityExecuted < quantityOrdered && workingOrderRow.StatusId != StatusMap.FromCode(Status.PartiallyFilled))
                    UpdateWorkingOrderStatus(workingOrderRow, Status.PartiallyFilled);

                // This restores the 'Filled' status when the quantity executed is the same as the quantity ordered.
                if (quantityExecuted == quantityOrdered && workingOrderRow.StatusId != StatusMap.FromCode(Status.Filled))
                    UpdateWorkingOrderStatus(workingOrderRow, Status.Filled);

            }

        }

        /// <summary>
        /// Updates the status of the Working Order.
        /// </summary>
        /// <param name="workingOrderRow">The Working Order to be modified.</param>
        /// <param name="status">The new status of the Working Order.</param>
        private static void UpdateWorkingOrderStatus(WorkingOrderRow workingOrderRow, Status status)
        {

            // It is a good idea to have a central method for updating the Working Order in the event the parameter order changes.
            DestinationOrder.dataModel.UpdateWorkingOrder(null, null, null, null, null, null, null, null, null, null, null, null,
                null, null, null, null, workingOrderRow.RowVersion, null, null, null, null, null, StatusMap.FromCode(status), null, null, null,
                null, null, null, null, null, new object[] { workingOrderRow.WorkingOrderId });

        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="blotterId"></param>
		/// <param name="modifiedByUser"></param>
		/// <param name="dataModel"></param>
		/// <param name="destinationOrderRows"></param>
		internal static void MoveToBlotter(Guid blotterId, Guid modifiedByUser, DataModel dataModel, List<BaseRecord> destinationOrderRows)
		{
			foreach (var destinationOrderRow in destinationOrderRows)
			{
				dataModel.UpdateDestinationOrder(
					blotterId,
					null,
					null,
					null,
					null,
					null,
					null,
					new object[] { destinationOrderRow.RowId },
					null,
					null,
					null,
					null,
					null,
					DateTime.UtcNow,
					modifiedByUser,
					null,
					null,
					destinationOrderRow.RowVersion,
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
					null);
			}
		}

    }

}
