namespace FluidTrade.Core
{

	using System;

	/// <summary>
	/// Handles a generic action with its associated key and parameters.
	/// </summary>
	/// <param name="key">The key of the object on which the operation is to take place.</param>
	/// <param name="parameters">A list of parameters for the action.</param>
	public delegate void ObjectHandler(Object[] key, params Object[] parameters);

}
