using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluidTrade.Core.Utilities
{
	public static class ApplicationHelper
	{
		/// <summary>
		/// is the application exiting
		/// </summary>
		private static bool isAppExiting;
		/// <summary>
		/// True if the app is closing, 
		/// Technically not thread safe but testing/setting a bool is good enough
		/// </summary>
		public static bool IsAppExiting { get { return ApplicationHelper.isAppExiting; } set { ApplicationHelper.isAppExiting = value; } }
	}
}
