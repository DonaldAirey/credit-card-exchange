using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluidTrade.Guardian
{
	internal class ConsumerMatchInfo
	{
		/// <summary>
		/// other orderId
		/// </summary>
		public Guid contraOrderId = Guid.Empty;

		/// <summary>
		/// other blotterId
		/// </summary>
		public Guid contraBlotterId = Guid.Empty;

		/// <summary>
		/// credit card id
		/// </summary>
		public Guid creditCardId = Guid.Empty;

		/// <summary>
		/// heat index of this matchInfo
		/// </summary>
		public Decimal heatIndex = 0.0M;	
	}
}
