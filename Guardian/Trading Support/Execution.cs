namespace FluidTrade.Guardian
{

	using FluidTrade.Core;
	using System;
	using System.Data;
	using System.ServiceModel;

	/// <summary>
	/// Summary description for Execution.
	/// </summary>
	internal class Execution
	{

		// Private Static Fields
		private static DataModel dataModel;

		/// <summary>
		/// Initialize the static elements of the DestinationOrder.
		/// </summary>
		static Execution()
		{

			// An instance of the data model is required to make changes to it.
			Execution.dataModel = new DataModel();

		}

		/// <summary>
		/// Handler for validating Execution records.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
		internal static void OnExecutionRowValidate(object sender, ExecutionRowChangeEventArgs e)
		{

			DestinationOrderRow destinationOrderRow;
			Decimal executedQuantity;
			DataModelTransaction dataModelTransaction;
			Decimal orderedQuantity;

			// The Business Rules will be enforced on this Execution.  Note that it is locked at the point this handler is called.
			ExecutionRow executionRow = e.Row;

			// The action on the row determines which rule to evaluate.
			switch (e.Action)
			{

			case DataRowAction.Add:

				// This rule will update the status of the owning Destination Order based on the quantity executed.
				dataModelTransaction = DataModelTransaction.Current;

				// The aggregate of the quantities executed will determine whether the Destination Order requires a status change.
				destinationOrderRow = executionRow.DestinationOrderRow;
				destinationOrderRow.AcquireReaderLock(dataModelTransaction);
				if (destinationOrderRow.RowState == DataRowState.Detached)
					return;

				// This calculates the quantity outstanding on an order and the quantity executed on the order.
				orderedQuantity = destinationOrderRow.OrderedQuantity - destinationOrderRow.CanceledQuantity;
				executedQuantity = 0.0M;
				foreach (ExecutionRow siblingRow in destinationOrderRow.GetExecutionRows())
				{
					siblingRow.AcquireReaderLock(dataModelTransaction);
					executedQuantity += siblingRow.ExecutionQuantity;
				}

				// This rule will set the status on the owning Destination Order based on the amont outstanding and the amount ordered.  First, if the quantity
				// executed is greater than the quantity ordered, the Order goes into an error state.  We can't reject executions as they come from an external
				// source, but the order can be flag and put into an error state.
				if (executedQuantity > orderedQuantity)
				{
					if (destinationOrderRow.StatusId != StatusMap.FromCode(Status.Error))
						UpdateDestinationOrderStatus(destinationOrderRow, Status.Error);
				}
				else
				{

					// When the total quantity executed is reduced to zero then the order goes back into the initial state.
					if (executedQuantity == 0.0m)
					{
						if (destinationOrderRow.StatusId != StatusMap.FromCode(Status.New))
							UpdateDestinationOrderStatus(destinationOrderRow, Status.New);
					}
					else
					{

						// While the executed quantity is still less than the outstanding quantity the order is consdered to be partially filled.  Otherwise,
						// the order is completely filled.
						if (executedQuantity < orderedQuantity)
						{
							if (destinationOrderRow.StatusId != StatusMap.FromCode(Status.PartiallyFilled))
								UpdateDestinationOrderStatus(destinationOrderRow, Status.PartiallyFilled);
						}
						else
						{
							if (destinationOrderRow.StatusId != StatusMap.FromCode(Status.Filled))
								UpdateDestinationOrderStatus(destinationOrderRow, Status.Filled);
						}

					}

				}

				break;

			case DataRowAction.Delete:

				// A middle tier context is required to lock the rows so the quantities can be aggregated.
				dataModelTransaction = DataModelTransaction.Current;

				// The aggregate of the quantities executed will determine whether the Destination Order requires a status change.
				destinationOrderRow = (DestinationOrderRow)executionRow.GetParentRow(
					DataModel.Execution.DestinationOrderExecutionRelation,
					DataRowVersion.Original);
				destinationOrderRow.AcquireReaderLock(dataModelTransaction);
				if (destinationOrderRow.RowState == DataRowState.Detached)
					return;

				// This calculates the quantity outstanding on an order and the quantity executed on the order.
				orderedQuantity = destinationOrderRow.OrderedQuantity - destinationOrderRow.CanceledQuantity;
				executedQuantity = 0.0M;
				foreach (ExecutionRow siblingRow in destinationOrderRow.GetExecutionRows())
				{
					siblingRow.AcquireReaderLock(dataModelTransaction);
					executedQuantity += siblingRow.ExecutionQuantity;
				}

				// When the total quantity executed is reduced to zero then the order goes back into the initial state.  Note that it is impossible for the
				// order to transition into a error state by deleting an execution.
				if (executedQuantity == 0.0m)
				{
					if (destinationOrderRow.StatusId != StatusMap.FromCode(Status.New))
						UpdateDestinationOrderStatus(destinationOrderRow, Status.New);
				}
				else
				{

					// While the executed quantity is still less than the outstanding quantity the order is consdered to be partially filled.  Note that it is
					// impossible for the order to transition to a filled state by deleting an execution.
					if (executedQuantity < orderedQuantity)
					{
						if (destinationOrderRow.StatusId != StatusMap.FromCode(Status.PartiallyFilled))
							UpdateDestinationOrderStatus(destinationOrderRow, Status.PartiallyFilled);
					}

				}

				break;

			}

		}

		/// <summary>
		/// Updates the status of the Destination Order.
		/// </summary>
		/// <param name="destinationOrderRow">The Working Order to be modified.</param>
		/// <param name="status">The new status of the Working Order.</param>
		private static void UpdateDestinationOrderStatus(DestinationOrderRow destinationOrderRow, Status status)
		{

			// It is a good idea to have a central method for updating the Destination Order in the event the parameter order changes.
			Execution.dataModel.UpdateDestinationOrder(null, null, null, null, null, null, null,
				new object[] { destinationOrderRow.DestinationOrderId }, null, null, null, null, null, null, null, null, null,
				destinationOrderRow.RowVersion, null, null, null, null, null, StatusMap.FromCode(status), null, null, null, null, null);

		}

	}

}
