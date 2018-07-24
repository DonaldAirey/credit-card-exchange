namespace FluidTrade.Guardian.GCSWebServiceClient.GCSWebServiceReference
{
	using System.Collections.Generic;
	using System.Data;
	using System;

	/// <summary>
	/// 
	/// </summary>
	public partial class PaymentsWSDS : global::System.Data.DataSet
	{

		/// <summary>
		/// Constructor to initialize GCS Payment table using information from our tables
		/// </summary>
		/// <param name="gcsAccountNumber"></param>
		/// <param name="paymentInfoList"></param>
		public PaymentsWSDS(string gcsAccountNumber, List<PaymentInfo> paymentInfoList)
		{
			this.tablePAYMENTS = new PAYMENTSDataTable();
			this.tableERRORS = new ERRORSDataTable();
			this.tableMETA_INFO = new META_INFODataTable();

			int i = 0;
			foreach (var paymentInfo in paymentInfoList)
			{
				PAYMENTSRow paymentRow = this.tablePAYMENTS.NewPAYMENTSRow();
				paymentRow.ACCOUNT_ID = gcsAccountNumber;
				paymentRow.ACTIVE_FLAG = paymentInfo.IsActive.ToString();
				paymentRow.AUTHORIZE_BY_DATE = paymentInfo.EffectivePaymentDate;
				paymentRow.EFFECTIVE_DATE = paymentInfo.EffectivePaymentDate;
				paymentRow.FEE_AMOUNT = paymentInfo.Fee0.GetValueOrDefault(0.0M);
				paymentRow.MEMO_1 = paymentInfo.Memo0;
				paymentRow.MEMO_2 = paymentInfo.Memo1;
				paymentRow.MEMO_3 = paymentInfo.Memo3;
				paymentRow.PAYMENT_ID = i++;
				paymentRow.COMPANY_ID = "6036335099000460";
				paymentRow.PAYMENT_AMOUNT = paymentInfo.EffectivePaymentValue;
				paymentRow.PAYEE_ID = 165227;
				paymentRow.TRACKING_NUMBER = "-1";
				this.tablePAYMENTS.Rows.Add(paymentRow);

				//DataRow paymentRow = this.PAYMENTS.NewRow();
				//paymentRow["ACCOUNT_ID"] = "6036335099003114";
				//paymentRow["ACTIVE_FLAG"] = "Y";
				//paymentRow["COMPANY_ID"] = "6036335099000460";
				//paymentRow["AUTHORIZE_BY_DATE"] = paymentInfo.EffectivePaymentDate;
				//paymentRow["EFFECTIVE_DATE"] = paymentInfo.EffectivePaymentDate;
				//paymentRow["FEE_AMOUNT"] = 0.0;
				//paymentRow["MEMO_1"] = paymentInfo.Memo0;
				//paymentRow["MEMO_2"] = paymentInfo.Memo1;
				//paymentRow["MEMO_3"] = paymentInfo.Memo2;
				//paymentRow["PAYMENT_ID"] = -1;
				//paymentRow["PAYEE_ID"] = "165227";
				//paymentRow["PAYMENT_AMOUNT"] = paymentInfo.EffectivePaymentValue;			
				//this.PAYMENTS.Rows.Add(paymentRow);
			}

			//this.tablePAYMENTS.AcceptChanges();
			this.Tables.Add(tablePAYMENTS);
			this.Tables.Add(tableERRORS);
			this.Tables.Add(tableMETA_INFO);
		}
	}
}
