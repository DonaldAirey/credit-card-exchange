namespace FluidTrade.Core
{

	using System;
	using System.Collections.Generic;

    /// <summary>
	/// A series of filters that can be used in a 'Where' clause.
	/// </summary>
	/// <typeparam name="Type">The target type for this filter.</typeparam>
	public class ComplexFilter<Type>
	{

		// Private Instance Fields
		private Func<Type, Boolean> filter;
		private List<Func<Type, Boolean>> filterList;

		/// <summary>
		/// Create an object that can apply several filters against an object in a 'Where' clause.
		/// </summary>
		public ComplexFilter()
		{

			// Initialize the object
			this.filter = new Func<Type, Boolean>(FilterData);
			this.filterList = new List<Func<Type, Boolean>>();

		}

		/// <summary>
		/// Gets the primary filter that applies all the other filters.
		/// </summary>
		public Func<Type, Boolean> Filter
		{
			get { return this.filter; }
		}

		/// <summary>
		/// Adds a Filter to the end of the collection of filters.
		/// </summary>
		/// <param name="filter">This filter is applied along with the other filters.</param>
		public void Add(Func<Type, Boolean> filter)
		{
			this.filterList.Add(filter);
		}

		/// <summary>
		/// Determins whether a filter is in the collection of filters.
		/// </summary>
		/// <param name="filter">A delegate that filters objects in a 'Where' clause.</param>
		/// <returns>true if the delegate is already part of the complex filter.</returns>
		public Boolean Contains(Func<Type, Boolean> filter)
		{
			return this.filterList.Contains(filter);
		}

		/// <summary>
		/// Removes a filter from the collection of filters.
		/// </summary>
		/// <param name="filter">The filter to be removed.</param>
		public void Remove(Func<Type, Boolean> filter)
		{
			this.filterList.Remove(filter);
		}

		/// <summary>
		/// Applies all the filters to a record.
		/// </summary>
		/// <param name="type">The type of record that is to be tested.</param>
		/// <returns>true if the object passes all the tests in the collection of filters.</returns>
		private Boolean FilterData(Type type)
		{

			// If the record fails any one of the test, then it has failed the complex filter.
			foreach (Func<Type, Boolean> filter in this.filterList)
				if (!filter(type))
					return false;

			// At this point the record has passed all the tests in the complex filter.
			return true;

		}

	}

}
