using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace FluidTrade.Core.Matching
{
	public class MatchInfo
	{
		public IRow row;
		public Decimal strength;
		public object value;
	}
}
