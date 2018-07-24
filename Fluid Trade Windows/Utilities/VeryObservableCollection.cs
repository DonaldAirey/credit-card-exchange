namespace FluidTrade.Core
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Collections.Specialized;

	/// <summary>
	/// The base observable collection doesn't report the removed items when Clear is called. this fixes that.
	/// </summary>
	/// <typeparam name="T">The type of the items in the collection.</typeparam>
	public class VeryObservableCollection<T> : ObservableCollection<T>
	{

		/// <summary>
		/// Report the items as removed then remove them.
		/// </summary>
		protected override void ClearItems()
		{

			this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, this));
			base.ClearItems();

		}

	}

}
