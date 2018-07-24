namespace FluidTrade.Core
{

	using System;

	/// <summary>
	/// Describes a dynamically created WCF client channel.
	/// </summary>
	internal class ClientChannel
	{

		// Public Instance Fields
		public System.Object ChannelObject;
		public System.Type ChannelType;

		/// <summary>
		/// Creates a dynamically generated client channel.
		/// </summary>
		/// <param name="channelObject">A dynamically created channel to a middle tier proxy.</param>
		/// <param name="channelType">The type information used to create the dynamic channel.</param>
		public ClientChannel(object channelObject, Type channelType)
		{

			// Initialize the object
			this.ChannelObject = channelObject;
			this.ChannelType = channelType;

		}

	}

}
