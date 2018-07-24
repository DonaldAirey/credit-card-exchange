namespace FluidTrade.Guardian
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// An exception thrown when an operation cannot be completed because a debt class contains settled accounts.
	/// </summary>
	public class HasSettlementsException : Exception
	{

		/// <summary>
		/// Create a new exception with a message.
		/// </summary>
		/// <param name="message">The message.</param>
		public HasSettlementsException(String message)
			: base(message)
		{


		}

	}

}
