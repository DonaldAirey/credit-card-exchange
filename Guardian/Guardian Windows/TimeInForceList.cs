namespace FluidTrade.Guardian.Windows
{

	using System.Collections.Generic;
	using System.Data;

	/// <summary>
	/// Represents a list of TimeInForceItems that is bound to the client data model.
	/// </summary>
	public class TimeInForceList : DataBoundList<TimeInForceItem>
	{
		
		/// <summary>
		/// The global TimeInForceList
		/// </summary>
		public static readonly TimeInForceList Default = new TimeInForceList();

		/// <summary>
		/// Keep a single comparer for all lists.
		/// </summary>
		private static TimeInForceList.CompareItems comparer = new TimeInForceList.CompareItems();

		/// <summary>
		/// Create a new time-in-force list.
		/// </summary>
		private TimeInForceList()
		{

			this.InitializeList();

		}

		/// <summary>
		/// The Comparer to use to compare to objects in the list.
		/// </summary>
		protected override IComparer<TimeInForceItem> Comparer
		{
			get { return TimeInForceList.comparer; }
		}

		/// <summary>
		/// The TimeInForce table.
		/// </summary>
		protected override DataTable Table
		{
			get { return DataModel.TimeInForce; }
		}

		/// <summary>
		/// A comparer to compare two items by mnemonic.
		/// </summary>
		private class CompareItems : IComparer<TimeInForceItem>
		{

			/// <summary>
			/// Compare two items.
			/// </summary>
			/// <param name="left">The left item.</param>
			/// <param name="right">The right item.</param>
			/// <returns>The relative order of the item.</returns>
			public int Compare(TimeInForceItem left, TimeInForceItem right)
			{

				return left.Mnemonic.CompareTo(right.Mnemonic);

			}

		}

		/// <summary>
		/// Create a new time-in-force item from a table row.
		/// </summary>
		/// <param name="dataRow">A row from the time-in-force table.</param>
		/// <returns>A new TimeInForceItem object.</returns>
		protected override TimeInForceItem New(DataRow dataRow)
		{

			TimeInForceItem item;
			TimeInForceRow baseRow = dataRow as TimeInForceRow;

			item = new TimeInForceItem()
			{
				Description = baseRow.Description,
				Mnemonic = baseRow.Mnemonic,
				SortOrder = baseRow.SortOrder,
				TimeInForceCode = baseRow.TimeInForceCode,

			};

			return item;

		}

		/// <summary>
		/// Update a TimeInForceItem object with another.
		/// </summary>
		/// <param name="old">The original object to update.</param>
		/// <param name="update">The new object to update with</param>
		protected override void Update(TimeInForceItem old, TimeInForceItem update)
		{

			old.Description = update.Description;
			old.Mnemonic = update.Mnemonic;
			old.SortOrder = update.SortOrder;
			old.TimeInForceCode = update.TimeInForceCode;

		}

	}

}
