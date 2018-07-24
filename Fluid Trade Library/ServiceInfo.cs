namespace FluidTrade.Core
{

	using System;

	/// <summary>
	/// Information about the base type behind a service.
	/// </summary>
	public class ServiceInfo
	{

		// Public Instance Fields
		public readonly System.Type Type;
		public readonly System.Uri[] BaseAddresses;

		/// <summary>
		/// Initializes information about a type
		/// </summary>
		/// <param name="TypeName">The base type behind a service.</param>
		public ServiceInfo(Type type, Uri[] baseAddresses)
		{

			// Initialize the object.
			this.Type = type;
			this.BaseAddresses = baseAddresses;

		}

	}
	
}
