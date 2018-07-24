namespace FluidTrade.Guardian
{

	using System;
	using System.Data;

	/// <summary>
	/// Provides Business Logic for Working Orders.
	/// </summary>
	internal class WorkingOrder
	{

		// Private Static Fields
		private static DataModel dataModel;

		/// <summary>
		/// Initialize the static elements of the DestinationOrder.
		/// </summary>
		static WorkingOrder()
		{

			// An instance of the data model is required to make changes to it.
			WorkingOrder.dataModel = new DataModel();

		}

		
		/// <summary>
		/// The sum total of the quantities of all the destination orders in a given working order.
		/// </summary>
		/// <param name="dataModelTransaction"></param>
		/// <param name="workingOrderRow">A working order row.</param>
		/// <returns>The total quantity of all the destination orders associated with the working order.</returns>
		internal static Decimal GetDestinationOrderQuantity(DataModelTransaction dataModelTransaction, WorkingOrderRow workingOrderRow)
		{

			// This is the accumulator for the quantity held in the destination orders.
			Decimal destinationOrderQuantity = 0.0m;

			// This will aggregate all the destination order quantities.  The tables must be locked in succession as they are
			// read from the table.
			foreach (DestinationOrderRow destinationOrderRow in workingOrderRow.GetDestinationOrderRows())
			{
				destinationOrderRow.AcquireReaderLock(dataModelTransaction);
				destinationOrderQuantity += destinationOrderRow.OrderedQuantity;
			}

			// This is the sum total of all the destination orders in the given working order.
			return destinationOrderQuantity;

		}

		/// <summary>
		/// The sum total of the quantities of all the destination orders in a given working order.
		/// </summary>
		/// <param name="dataModelTransaction"></param>
		/// <param name="workingOrderRow">A working order row.</param>
		/// <returns>The total quantity of all the destination orders associated with the working order.</returns>
		internal static Decimal GetExecutionQuantity(DataModelTransaction dataModelTransaction, WorkingOrderRow workingOrderRow)
		{

			// This will aggregate all the execution quantities.  Note that the records are locked for the duration of the transaction to insure the integrity
			// of the aggregates.
			Decimal executionQuantity = 0.0m;
			foreach (DestinationOrderRow destinationOrderRow in workingOrderRow.GetDestinationOrderRows())
			{
				destinationOrderRow.AcquireReaderLock(dataModelTransaction);
				if (destinationOrderRow.StatusId != StatusMap.FromCode(Status.Canceled))
					foreach (ExecutionRow executionRow in destinationOrderRow.GetExecutionRows())
					{
						executionRow.AcquireReaderLock(dataModelTransaction);
						executionQuantity += executionRow.ExecutionQuantity;
					}
			}

			// This is the aggregate quantity executed for the given working order.
			return executionQuantity;

		}

		/// <summary>
		/// The sum total of the quantities of all the source orders in a given working order.
		/// </summary>
		/// <param name="dataModelTransaction"></param>
		/// <param name="workingOrderRow">A working order row.</param>
		/// <returns>The total quantity of all the source orders associated with the working order.</returns>
		internal static Decimal GetSourceOrderQuantity(DataModelTransaction dataModelTransaction, WorkingOrderRow workingOrderRow)
		{

			// This will aggregate all the source order quantities.  Note that the rows are kept locked for the duration of the transaction.  This guarantees
			// the integrity of the aggregate values.
			Decimal sourceOrderQuantity = 0.0m;
			foreach (SourceOrderRow sourceOrderRow in workingOrderRow.GetSourceOrderRows())
			{
				try
				{
					sourceOrderRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
					sourceOrderQuantity += sourceOrderRow.OrderedQuantity;
				}
				finally
				{
					sourceOrderRow.ReleaseLock(dataModelTransaction.TransactionId);
				}
			}

			// This is the sum total of all the source orders in the given working order.
			return sourceOrderQuantity;

		}
	

		/// <summary>
		/// Handler for validating Source Order records.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
		internal static void OnWorkingOrderRowValidate(object sender, WorkingOrderRowChangeEventArgs e)
		{

			// The Business Rules will be enforced on this Working Order.  Note that it is locked at the point this handler is called.
			WorkingOrderRow workingOrderRow = e.Row;

			// The action on the row determines which rule to evaluate.
			switch (e.Action)
			{

			case DataRowAction.Add:
			case DataRowAction.Change:

				// This will evaluate whether the given Working Order is a candidate for matching.
				Boolean isSubmitted = false;

				// The 'Away' flag is used to inhibit matching without loosing the original preference for matching.  It is used when the trader wants to walk
				// away from his blotter.  When the trader returns, they want to flip a switch and have all the previous settings active for the orders on the
				// blotter.  This skips the matching logic for any trade marked 'Away'.
				if ((CrossingMap.FromId(workingOrderRow.CrossingId) & Crossing.Away) == Crossing.None)
				{

					// This determines if the trade is eligible for crossing.
					switch (CrossingMap.FromId(workingOrderRow.CrossingId) & ~Crossing.Away)
					{

					case Crossing.AlwaysMatch:

						// The trade is eligible for crossing.
						isSubmitted = true;
						break;

					case Crossing.UsePreferences:

						// The trade is only eligible for crossing if it passes a set of preferences properties set by the user.
						isSubmitted = workingOrderRow.IsAwake;

						// The start and stop time must be extracted from the record using the base class.  It is safe here because the triggers are only
						// invoked while the record is locked.
						DateTime startTime = workingOrderRow.StartTime;
						DateTime stopTime = workingOrderRow.StopTime;

						// The order is only eligible for crossing if the time of day is after the start time.
						if (startTime != null && DateTime.UtcNow.TimeOfDay < startTime.TimeOfDay)
							isSubmitted = false;

						// The order is only eligible for crossing if the time if day is before the end time.
						if (stopTime != null && DateTime.UtcNow.TimeOfDay > stopTime.TimeOfDay)
							isSubmitted = false;

						break;

					}

				}

				// This will submit the order for crossing when it isn't already submitted.
				if (isSubmitted && workingOrderRow.StatusId != StatusMap.FromCode(Status.Submitted))
					UpdateWorkingOrderStatus(workingOrderRow, Status.Submitted);

				// This removes the order for consideration in the crossing pool when it has been submitted.
				if (!isSubmitted && workingOrderRow.StatusId == StatusMap.FromCode(Status.Submitted))
					UpdateWorkingOrderStatus(workingOrderRow, Status.New);

				break;

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
			WorkingOrder.dataModel.UpdateWorkingOrder(null, null, null, null, null, null, null, null, null, null, null, null,
				null, null, null, null, workingOrderRow.RowVersion, null, null, null, null, null, StatusMap.FromCode(status), null, null, null,
				null, null, null, null, null, new object[] { workingOrderRow.WorkingOrderId });

		}


	}

}
