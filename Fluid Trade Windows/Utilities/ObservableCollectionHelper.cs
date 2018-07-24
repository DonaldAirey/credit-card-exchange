namespace FluidTrade.Core.Windows.Utilities
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Collections.ObjectModel;

	/// <summary>
	/// Extension methods for observable collecitons.
	/// </summary>
	public static class ObservableCollectionHelper
	{
	
		/// <summary>
		/// A replacement for Clear that reports what items were removed.
		/// </summary>
		/// <param name="list">The collection</param>
		public static void RemoveAll<T>(this ObservableCollection<T> list)
		{

			while (list.Count > 0)
				list.RemoveAt(0);

		}

	}

}
