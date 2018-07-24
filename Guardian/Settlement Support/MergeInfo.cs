namespace FluidTrade.Guardian
{

	using System;
	using System.Collections.Generic;
	
	internal class MergeInfo
	{

		public Int64 RowVersion;
		public Guid ConsumerDebtSettlementId;
		public Byte[] SourceDocument;
		public Dictionary<String, Object> Dictionary;
		public Guid StatusId;

		public MergeInfo()
		{

			// Initialize the object.
			this.RowVersion = 0L;
			this.ConsumerDebtSettlementId = Guid.Empty;
			this.StatusId = Guid.Empty;
			this.SourceDocument = new Byte[0];
			this.Dictionary = new Dictionary<String, Object>();

		}

	}

}
