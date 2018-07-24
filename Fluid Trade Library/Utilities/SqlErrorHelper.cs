using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace FluidTrade.Core.Utilities
{
	/// <summary>
	/// helper class to figure out what type of sql exception we have
	/// </summary>
	public static class SqlErrorHelper
	{
		private static readonly int[] knownDeadlockErrorNumberAr = new int[] { 1205, 3536, 17888 };

		/// <summary>
		/// check if sql exception is a result of a sql deadlock
		/// </summary>
		/// <param name="exception"></param>
		/// <returns></returns>
		public static bool IsDeadlockException(Exception exception)
		{
			if(exception is DeadlockException)
				return true;

			SqlException sqlEx = exception as SqlException;
			if(sqlEx != null)
				foreach(int knownDeadlockNumber in knownDeadlockErrorNumberAr)
				{
					if(sqlEx.Number == knownDeadlockNumber)
						return true;
				}

			return false;
		}
	}

	/// <summary>
	/// Deadlock exception class for interal deadlocks
	/// </summary>
	public class DeadlockException : Exception
	{
		public DeadlockException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
