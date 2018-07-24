namespace FluidTrade.Guardian
{

	using System;
	using System.Data;
	using System.ServiceModel;
	using FluidTrade.Guardian.Records;
	using System.Collections.Generic;

	/// <summary>
	/// Summary description for SourceOrder.
	/// </summary>
	internal class SourceOrder
	{

		// Private Static Fields
		private static DataModel dataModel;

		/// <summary>
		/// Initialize the static elements of the DestinationOrder.
		/// </summary>
		static SourceOrder()
		{

			// An instance of the data model is required to make changes to it.
			SourceOrder.dataModel = new DataModel();

		}

		/// <summary>
		/// Handler for validating Source Order records.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
		internal static void OnSourceOrderRowValidate(object sender, SourceOrderRowChangeEventArgs e)
		{

			Decimal currentQuantity;
			Decimal destinationOrderQuantity;
			DataModelTransaction dataModelTransaction;
			Decimal originalQuantity;
			Decimal sourceOrderQuantity;
			WorkingOrderRow workingOrderRow;

			// The Business Rules will be enforced on this Source Order.  Note that it is locked at the point this handler is called.
			SourceOrderRow sourceOrderRow = e.Row;

			// The action on the row determines which rule to evaluate.
			switch (e.Action)
			{

				case DataRowAction.Change:

					// Reject the operation if the Working Order is overcommitted with a destination.  Note that the order of the evaluation is important for 
					// efficiency.  That is, there is no need to sum up the Source and Destination Orders, which requires locking several records, if the Source 
					// Order quantity has been incremented as there's no chance it will be over committed.
					originalQuantity = (Decimal)sourceOrderRow[DataModel.SourceOrder.OrderedQuantityColumn, DataRowVersion.Original];
					currentQuantity = (Decimal)sourceOrderRow[DataModel.SourceOrder.OrderedQuantityColumn, DataRowVersion.Current];
					if (originalQuantity < currentQuantity)
					{

						// A middle tier context is required to lock the rows so the quantities can be aggregated.
						dataModelTransaction = DataModelTransaction.Current;

						// Once its determined that the quantity has increased, the Working Order row will need to be locked to examine the aggregate values of the
						// source and Destination Orders.
						workingOrderRow = e.Row.WorkingOrderRow;
						workingOrderRow.AcquireReaderLock(dataModelTransaction);

						// This is an illegal operation if the quantity sent to a destination is greater than the quantity ordered.
						sourceOrderQuantity = WorkingOrder.GetSourceOrderQuantity(dataModelTransaction, workingOrderRow);
						destinationOrderQuantity = WorkingOrder.GetDestinationOrderQuantity(dataModelTransaction, workingOrderRow);
						if (sourceOrderQuantity < destinationOrderQuantity)
							throw new FaultException<DestinationQuantityFault>(
								new DestinationQuantityFault(workingOrderRow.WorkingOrderId, sourceOrderQuantity, destinationOrderQuantity));

					}
					break;

				case DataRowAction.Delete:

					// A middle tier context is required to lock the rows so the quantities can be aggregated.
					dataModelTransaction = DataModelTransaction.Current;

					// The main idea of this trigger is to prevent the over commitment of a Working Order when a Source Order is deleted.  At this point, the
					// Source Order is deleted, so the original record must be used to find the parent Working Order that is effected.
					sourceOrderRow = e.Row;
					try
					{
						DataModel.DataLock.EnterReadLock();
						workingOrderRow = ((WorkingOrderRow)(sourceOrderRow.GetParentRow(DataModel.SourceOrder.WorkingOrderSourceOrderRelation, DataRowVersion.Original)));
					}
					finally
					{
						DataModel.DataLock.ExitReadLock();
					}

					// Lock the working order row while some aggregate values are calculated.
					workingOrderRow.AcquireReaderLock(dataModelTransaction);

					// Reject the operation if the Working Order is overcommitted with a destination.
					sourceOrderQuantity = WorkingOrder.GetSourceOrderQuantity(dataModelTransaction, workingOrderRow);
					destinationOrderQuantity = WorkingOrder.GetDestinationOrderQuantity(dataModelTransaction, workingOrderRow);
					if (sourceOrderQuantity < destinationOrderQuantity)
						throw new FaultException<DestinationQuantityFault>(
							new DestinationQuantityFault(workingOrderRow.WorkingOrderId, sourceOrderQuantity, destinationOrderQuantity));

					break;

			}
		}

		/// <summary>
		/// Move the sourceOrders to another blotter
		/// </summary>
		/// <param name="blotterId"></param>
		/// <param name="modifiedByUser"></param>
		/// <param name="dataModel"></param>
		/// <param name="sourceOrderRows"></param>
		internal static void MoveToBlotter(Guid blotterId, Guid modifiedByUser, DataModel dataModel, List<BaseRecord> sourceOrderRows)
		{

			foreach (BaseRecord sourceOrderRow in sourceOrderRows)
			{
				dataModel.UpdateSourceOrder(
					blotterId,
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
					null,
					null,
					DateTime.UtcNow,
					modifiedByUser,
					null,
					null,
					null,
					null,
					sourceOrderRow.RowVersion,
					null,
					null,
					null,
					null,
					null,
					new object[] { sourceOrderRow.RowId },
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
