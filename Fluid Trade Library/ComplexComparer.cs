namespace FluidTrade.Core
{

    using System.Collections.Generic;

	/// <summary>
	/// Compares two FluidTrade.GuardianWorkingOrder.WorkingOrder records when sorting a list.
	/// </summary>
	public class ComplexComparer<Type> : System.Collections.Generic.IComparer<Type>
	{

		// Private Instance Fields
		private System.Collections.Generic.List<ComparerItem<Type>> comparerItemList;

		/// <summary>
		/// Creates a object that describes the sorting on several keys and directions.
		/// </summary>
		public ComplexComparer()
		{

			// Initialize the object.
			this.comparerItemList = new List<ComparerItem<Type>>();

		}

		/// <summary>
		/// Gets a list of the child ComparerItems.
		/// </summary>
		public System.Collections.Generic.List<ComparerItem<Type>> Children
		{
			get { return this.comparerItemList; }
		}

		/// <summary>
		/// Adds a comparison operation and an order to the complex sort.
		/// </summary>
		/// <param name="iComparer"></param>
		/// <param name="sortOrder"></param>
		public void Add(IComparer<Type> iComparer, SortOrder sortOrder)
		{

			// Add the combination to the list. 
			this.comparerItemList.Add(new ComparerItem<Type>(iComparer, sortOrder));

		}

		/// <summary>
		/// Clears a complex comparer of all comparison operations.
		/// </summary>
		public void Clear()
		{

			// Clear out the list.
			this.comparerItemList.Clear();

		}

		/// <summary>
		/// Compares two records.
		/// </summary>
		/// </param name="operand1">The first record to be compared.</param>
		/// </param name="operand2">The second record to be compared.</param>
		/// </summary>
		public int Compare(Type operand1, Type operand2)
		{

			// The sort operation and direction are used to determine the relative order of the two records.
			foreach (ComparerItem<Type> comparisonPair in this.comparerItemList)
			{
				int comparison = comparisonPair.IComparer.Compare(operand1, operand2);
				if (comparison != 0)
					return comparisonPair.SortOrder == SortOrder.Ascending ? comparison : -comparison;
			}

			// If none of the comparisons turns up a difference, then the two records are equal.
			return 0;

		}

	}

}
