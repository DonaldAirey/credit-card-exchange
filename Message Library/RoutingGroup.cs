using System;

namespace FluidTrade.Core
{
	/// <summary>
	/// Defines a FIX repeating group for RoutingId/RoutingType entries.
	/// </summary>
	[Serializable()]
	public class RoutingGroup : RepeatingGroup
	{
		/// <summary>
		/// Entries in a RoutingGroup consist of RoutingId and RoutingType.
		/// </summary>
		[Serializable()]
		public class Entry
		{
			public Entry(RoutingType routingType, string routingId)
			{
				this.RoutingType = routingType;
				this.RoutingId = routingId;
			}
			public readonly RoutingType RoutingType;
			public readonly string RoutingId;
		}

		// Used to add routing list entries.
		public void Add(RoutingType routingType, string routingId)
		{
			base.Add( new Entry(routingType, routingId) );
		}

	}

}
