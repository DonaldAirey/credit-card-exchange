namespace FluidTrade.Guardian
{

	using System;

	class GenerateTradeInfo
	{

		public Int32 OrderCount;
		public Guid BlotterId;
		public String FileName;

		public GenerateTradeInfo(Int32 orderCount, Guid blotterId, String fileName)
		{
			// Initialize the object
			this.OrderCount = orderCount;
			this.BlotterId = blotterId;
			this.FileName = fileName;
		}

	}
}
