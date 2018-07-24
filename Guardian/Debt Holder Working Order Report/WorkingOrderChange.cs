namespace FluidTrade.Guardian
{


    // Intenral Delegates
	internal delegate void WorkingOrderClientDelegate(DataModelClient dataModelClient, WorkingOrderRow workingOrderRow, object value);

	/// <summary>
	/// Used to carry out an update action on a Working Order record.
	/// </summary>
	internal class WorkingOrderChange
	{

		// Public Fields
		public WorkingOrderClientDelegate Handler;
		public WorkingOrderRow WorkingOrderRow;
		public object Value;

		/// <summary>
		/// Describes a handler and the data used to change a WorkingOrder row in the middle tier.
		/// </summary>
		/// <param name="handler">A handler to update the Working Order record.</param>
		/// <param name="workingOrderRow">The current Working Order record.</param>
		/// <param name="value">The new value of an element of the Working Order record.</param>
		public WorkingOrderChange(WorkingOrderClientDelegate handler, WorkingOrderRow workingOrderRow, object value)
		{
			this.Handler = handler;
			this.WorkingOrderRow = workingOrderRow;
			this.Value = value;
		}

	}

}
