namespace FluidTrade.Guardian.Records
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Runtime.Serialization;

	/// <summary>
	/// The bits of a payment summary that can be updated.
	/// </summary>
	public class ConsumerDebtPayment : BaseRecord
	{

		/// <summary>
		/// The amount actually paid for the interval.
		/// </summary>
		[DataMember]
		public Decimal? ActualPaymentValue { get; set; }

		/// <summary>
		/// The date the payment was actually made.
		/// </summary>
		[DataMember]
		public DateTime? ActualPaymentDate { get; set; }

		/// <summary>
		/// The number of the check that was sent.
		/// </summary>
		[DataMember]
		public String CheckId { get; set; }

		/// <summary>
		/// The date the payment cleared.
		/// </summary>
		[DataMember]
		public DateTime ClearedDate { get; set; }

		/// <summary>
		/// The fee associated with the payment.
		/// </summary>
		[DataMember]
		public Decimal? Fee0 { get; set; }

		/// <summary>
		/// True if the payment is active.
		/// </summary>
		[DataMember]
		public Boolean IsActive { get; set; }

		/// <summary>
		/// True if the payment has cleared.
		/// </summary>
		[DataMember]
		public Boolean IsCleared { get; set; }

		/// <summary>
		/// A memo about the payment.
		/// </summary>
		[DataMember]
		public String Memo0 { get; set; }

		/// <summary>
		/// A memo about the payment.
		/// </summary>
		[DataMember]
		public String Memo1 { get; set; }

		/// <summary>
		/// A memo about the payment.
		/// </summary>
		[DataMember]
		public String Memo2 { get; set; }

		/// <summary>
		/// A memo about the payment.
		/// </summary>
		[DataMember]
		public String Memo3 { get; set; }

		/// <summary>
		/// Current status of the payment.
		/// </summary>
		[DataMember]
		public Guid StatusId { get; set; }

			
		/// <summary>
		/// A free form nullable text field.
		/// </summary>
		[DataMember]
		public String TrackingNumber { get; set; }



	}

}
