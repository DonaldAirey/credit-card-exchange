using System;
using System.Collections;

namespace FluidTrade.Core
{
	/// <summary>
	/// A base class for FIX repeating groups.
	/// This class is enumerable via foreach.
	/// </summary>
	[Serializable()]
	public class RepeatingGroup
	{
		private ArrayList list;

		// Used by derived classes to add entries to the group.
		protected int Add(object entry)
		{
			return list.Add(entry);
		}

		public RepeatingGroup() 
		{
			list = new ArrayList();
		}

		/// <summary>
		/// Returns an enumerator for the group.
		/// </summary>
		/// <returns></returns>
		public IEnumerator GetEnumerator()
		{
			return list.GetEnumerator();
		}

		/// <summary>
		/// Gets the number of entries in the group.
		/// </summary>
		public int Count
		{
			get { return list.Count;}
		}
	}
}
