using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace FluidTrade.Core
{
	public class DescriptionAttribute : Attribute
	{
		/// <summary>
		/// 
		/// </summary>
		public string Text { get; set; }

		public DescriptionAttribute(string text)
		{
			this.Text = text;
		}				
	}
}
