using System;

namespace FluidTrade.Core
{
	/// <summary>
	/// Defines a FIX repeating group for IoiQualifier entries.
	/// </summary>
	[Serializable()]
	public class IoiQualifierGroup : RepeatingGroup
	{
		/// <summary>
		/// Entries in a IoiQualifierGroup consist only of IoiQualifier.
		/// </summary>
		[Serializable()]
		public class Entry
		{
			public Entry(IOIQualifier ioiQualifier)
			{
				this.IoiQualifier = ioiQualifier;
			}
			public readonly IOIQualifier IoiQualifier;
		}

		// Used to add routing list entries.
		public void Add(IOIQualifier ioiQualifier)
		{
			base.Add( new Entry(ioiQualifier) );
		}

	}

}
