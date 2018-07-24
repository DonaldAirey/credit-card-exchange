namespace FluidTrade.Core
{

	using System;
	using System.Runtime.Serialization;

	/// <summary>
	/// Contains information about a failure to have compatible payment methods.
	/// </summary>
	[DataContract]
	public class PaymentMethodFault
	{

		// Private Instance Fields
		private String message;

		/// <summary>
		/// Create information about a failure to have compatible payment methods.
		/// </summary>
		/// <param name="message">A message describing the fault.</param>
		public PaymentMethodFault(string message)
		{

			// Initialize the object
			this.message = message;

		}

		/// <summary>
		/// Gets or sets the message describing the fault.
		/// </summary>
		[DataMemberAttribute]
		public string Message
		{
			get { return this.message; }
			set { this.message = value; }
		}

	}

}
