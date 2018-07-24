namespace FluidTrade.Guardian.TradingSupportReference
{

	using System;
	using FluidTrade.Guardian.TradingSupportReference;

	partial class ConsumerDebtPayment : BaseRecord
	{

		/// <summary>
		/// Create a new ConsumerDebtPayment record.
		/// </summary>
		/// <param name="row"></param>
		public ConsumerDebtPayment()
		{

		}
	
		/// <summary>
		/// Create a new ConsumerDebtPayment record.
		/// </summary>
		/// <param name="row"></param>
		public ConsumerDebtPayment(ConsumerDebtPaymentRow row)
		{

			this.RowId = row.ConsumerDebtPaymentId;
			this.RowVersion = row.RowVersion;
			this.ActualPaymentDate = row.IsActualPaymentDateNull() ? null : (DateTime?)row.ActualPaymentDate;
			this.ActualPaymentValue = row.IsActualPaymentValueNull() ? null : (Decimal?)row.ActualPaymentValue;
			this.CheckId = row.IsCheckIdNull() ? null : row.CheckId;
			this.ClearedDate = row.ClearedDate;
			this.Fee0 = row.IsFee0Null() ? (Decimal?)null : (Decimal?)row.Fee0;
			this.IsActive = row.IsActive;
			this.IsCleared = row.IsCleared;
			this.Memo0 = row.IsMemo0Null() ? null : row.Memo0;
			this.Memo1 = row.IsMemo1Null() ? null : row.Memo1;
			this.Memo2 = row.IsMemo2Null() ? null : row.Memo2;
			this.Memo3 = row.IsMemo3Null() ? null : row.Memo3;
			this.StatusId = row.StatusId;

		}

	}
}
