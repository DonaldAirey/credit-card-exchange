namespace FluidTrade.Guardian
{

	using System;

	/// <summary>
	/// Describes the payment methods used to settle a negotiation.
	/// </summary>
	public class PaymentMethodItem
	{

		/// <summary>
		/// Payment method typeId.
		/// </summary>
		public Guid PaymentMethodTypeId { get; set; }

		/// <summary>
		/// Action to perform on the PaymentMethodTypeId.
		/// </summary>
		public PaymentTypeUpdateAction Action { get; set; }

		/// <summary>
		/// Overloaded constructor.
		/// </summary>
		/// <param name="party">Identifies the side of the negotiation party.</param>
		/// <param name="paymentMethodTypeId">PaymentMethodTypeId</param>
		/// <param name="action">Action to perform.</param>
		public PaymentMethodItem(Guid paymentMethodTypeId, PaymentTypeUpdateAction paymentTypeUpdateAction)
		{

			// Initialize the object.
			this.PaymentMethodTypeId = paymentMethodTypeId;
			this.Action = paymentTypeUpdateAction;

		}

	}

}