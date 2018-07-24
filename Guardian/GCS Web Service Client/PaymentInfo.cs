namespace FluidTrade.Guardian
{


	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	
	/// <summary>
	/// Nested class to hold the data
	/// </summary>
	public class PaymentInfo
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="consumerTrustPaymentRow"></param>
		internal PaymentInfo(ConsumerTrustPaymentRow consumerTrustPaymentRow)
		{
			this.BlotterId = consumerTrustPaymentRow.BlotterId;
			this.ConsumerTrustPaymentId = consumerTrustPaymentRow.ConsumerTrustPaymentId;
			this.EffectivePaymentDate = consumerTrustPaymentRow.EffectivePaymentDate;
			this.EffectivePaymentValue = consumerTrustPaymentRow.EffectivePaymentValue;
			this.Fee0 = consumerTrustPaymentRow.Fee0;
			this.Fee1 = consumerTrustPaymentRow.Fee1;
			this.Fee2 = consumerTrustPaymentRow.Fee2;
			this.Fee3 = consumerTrustPaymentRow.Fee3;
			this.IsActive = consumerTrustPaymentRow.IsActive;
			this.IsCleared = consumerTrustPaymentRow.IsCleared;
			this.Memo0 = consumerTrustPaymentRow.IsMemo0Null() ? string.Empty : consumerTrustPaymentRow.Memo0;
			this.Memo1 = consumerTrustPaymentRow.IsMemo1Null() ? string.Empty : consumerTrustPaymentRow.Memo1;
			this.Memo2 = consumerTrustPaymentRow.IsMemo2Null() ? string.Empty : consumerTrustPaymentRow.Memo2;
			this.Memo3 = consumerTrustPaymentRow.IsMemo3Null() ? string.Empty : consumerTrustPaymentRow.Memo3;
			this.PaymentId = consumerTrustPaymentRow.ConsumerTrustPaymentId;
			this.StatusId = consumerTrustPaymentRow.StatusId;
			this.TrackingNumber = consumerTrustPaymentRow.IsTrackingNumberNull() ? String.Empty : consumerTrustPaymentRow.TrackingNumber;

		}

		internal Guid BlotterId { get; private set; }
		internal Guid ConsumerTrustPaymentId { get; private set; }
		internal DateTime EffectivePaymentDate { get; private set; }
		internal Decimal EffectivePaymentValue { get; private set; }
		internal Decimal? Fee0 { get; private set; }
		internal Decimal? Fee1 { get; private set; }
		internal Decimal? Fee2 { get; private set; }
		internal Decimal? Fee3 { get; private set; }
		internal Boolean IsActive { get; private set; }
		internal Boolean IsCleared { get; private set; }
		internal String Memo0 { get; private set; }
		internal String Memo1 { get; private set; }
		internal String Memo2 { get; private set; }
		internal String Memo3 { get; private set; }
		internal Guid PaymentId { get; private set; }
		internal Guid? StatusId { get; private set; }
		internal String TrackingNumber { get; private set; }

	}

}
