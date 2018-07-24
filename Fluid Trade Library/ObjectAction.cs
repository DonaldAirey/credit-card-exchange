namespace FluidTrade.Core
{

	using System;

	/// <summary>
	/// A generic handler for a generic action with a generic key and parameters.
	/// </summary>
	public struct ObjectAction
	{

		// Public Members
		public ObjectHandler DoAction;
		public Object[] Key;
		public Object[] Parameters;

		/// <summary>
		/// A generic handler for a generic action with a generic key and parameters.
		/// </summary>
		/// <param name="doAction">The generic handler for some action.</param>
		/// <param name="key">The generic key to identify the object on which the action is to take place.</param>
		/// <param name="parameters">The parameters for the action.</param>
		public ObjectAction(ObjectHandler doAction, Object[] key, params Object[] parameters)
		{

			// Initialize the object
			this.DoAction = doAction;
			this.Key = key;
			this.Parameters = parameters;

		}

	}

}
