using System;
using System.IO;

namespace FluidTrade.Guardian
{
	/// <summary>
	/// This is an interim extension helper. 
	/// </summary>
	public static class GuidHelper
	{
		/// <summary>
		/// Extension to convert guid to decimal.  
		/// </summary>		
		/// <param name="guidValue"></param>
		/// <returns></returns>
		public static Decimal ToDecimal(this Guid guidValue)
		{
			// Guid is 128 bit while a Int64 is half that, so you are going to loose precision.
			byte[] bytes = guidValue.ToByteArray();
			Int64 convertedValue = BitConverter.ToInt64(bytes, 0);
			return convertedValue;
		}
	}

}
