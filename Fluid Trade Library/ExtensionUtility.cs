using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace FluidTrade.Core
{
	public static class ExtensionUtility
	{
		/// <summary>
		/// Extension method to extract "description" of an enum.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="enumerationValue"></param>
		/// <returns></returns>
		public static string GetDescription<T>(this object enumerationValue)
				where T : struct
		{
			Type type = enumerationValue.GetType();
			if (!type.IsEnum)
			{
				throw new ArgumentException("EnumerationValue must be of Enum type", "enumerationValue");
			}

			//Tries to find a DescriptionAttribute for a potential friendly name
			//for the enum
			MemberInfo[] memberInfo = type.GetMember(enumerationValue.ToString());
			if (memberInfo != null && memberInfo.Length > 0)
			{
				object[] attrs = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

				if (attrs != null && attrs.Length > 0 && attrs.Where(t => t.GetType() == typeof(DescriptionAttribute)).FirstOrDefault() != null)
				{
					//Pull out the description value
					return ((DescriptionAttribute)attrs.Where(t => t.GetType() == typeof(DescriptionAttribute)).FirstOrDefault()).Text;
				}
			}
			//If we have no description attribute, just return the ToString of the enum
			return enumerationValue.ToString();

		}
	}
}
