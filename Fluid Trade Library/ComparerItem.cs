namespace FluidTrade.Core
{

    using System.Collections.Generic;

	/// <summary>
	/// A set of objects that describes a comparison between two object and a sort order.
	/// </summary>
	/// <typeparam name="Type"></typeparam>
	public class ComparerItem<Type>
	{

		// Public Instance Fields
		public IComparer<Type> IComparer;
		public SortOrder SortOrder;

		/// <summary>
		/// Create an object that describes the comparison operation and the sort direction.
		/// </summary>
		/// <param name="iComparer">An object that compares two objects.</param>
		/// <param name="sortOrder">The direction for the sort.</param>
		public ComparerItem(IComparer<Type> iComparer, SortOrder sortOrder)
		{

			// Initialize the object.
			this.IComparer = iComparer;
			this.SortOrder = sortOrder;

		}

	}

}
