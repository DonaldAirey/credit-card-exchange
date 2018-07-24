namespace FluidTrade.Guardian
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Collections.Specialized;
	using System.Collections;

	/// <summary>
	/// A list whose contents are initialized from the data model and are updated by changes to the data model.
	/// </summary>
	/// <typeparam name="T">The type contained by the list.</typeparam>
	public interface IDataBoundList<T> : IList<T>, IDataBoundList
	{
	}

	/// <summary>
	/// A list whose contents are initialized from the data model and are updated by changes to the data model.
	/// </summary>
	public interface IDataBoundList : IList, INotifyCollectionChanged, IDisposable
	{
		/// <summary>
		/// Raised after the list has been initialized.
		/// </summary>
		event EventHandler Initialized;
		/// <summary>
		/// Raised before the list changes.
		/// </summary>
		event EventHandler CollectionChanging;
		/// <summary>
		/// True if the list has been initialized.
		/// </summary>
		Boolean IsInitialized { get; }
	}
}
