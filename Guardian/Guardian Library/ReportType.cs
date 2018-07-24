namespace FluidTrade.Guardian
{
	/// <summary>
	/// Summary description for ReportTypes.
	/// </summary>
	public enum ReportType
	{
		/// <summary>Block Order Documents</summary>
		WorkingOrder,
		/// <summary>DestinationOrder Ledgers</summary>
		DestinationOrder,
		/// <summary>DestinationOrder Ledgers</summary>
		DestinationOrderDetail,
		/// <summary>Execution Reports</summary>
		Execution,
		/// <summary>Execution Reports</summary>
		ExecutionDetail,
		/// <summary>Source Order Reports</summary>
		SourceOrder,
		/// <summary>Source Order Reports</summary>
		SourceOrderDetail,
		/// <summary>Matching Viewers</summary>
		Match,
		/// <summary>Payment Summary</summary>
		PaymentSummary,
		/// <summary>Matching Viewers</summary>
		Quote,
		/// <summary>Settlement Report</summary>
		Settlement,
		//
		CreditCardDetail,
		/// <summary>A static report that can be printed ie:crystal report</summary>
		StaticReport
	}
}
