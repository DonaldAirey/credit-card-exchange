namespace FluidTrade.Guardian
{

	using System;

	/// <summary>
	/// Actions that can be performed on the payment types.
	/// </summary>
	public enum PaymentTypeUpdateAction
	{
		/// <summary>
		/// Indicates that a new payment method has been added. 
		/// </summary>
		Add,
		/// <summary>
		/// Indicates that a payment method has been deleted.
		/// </summary>
		Delete
	}

}
