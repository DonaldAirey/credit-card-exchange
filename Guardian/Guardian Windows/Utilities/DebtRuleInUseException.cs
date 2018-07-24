namespace FluidTrade.Guardian
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// An exception thrown when an operation cannot be completed by because a related debt rule is in use.
	/// </summary>
	public class DebtRuleInUseException : Exception
	{

		String name = null;

		/// <summary>
		/// Create a new exception with a message.
		/// </summary>
		/// <param name="message">The message.</param>
		public DebtRuleInUseException(String message)
			: base(message)
		{


		}

		/// <summary>
		/// Create a new exception with a message and rule name.
		/// </summary>
		/// <param name="name">The name of the rule involved.</param>
		/// <param name="message">The message.</param>
		public DebtRuleInUseException(String name, String message)
			: base(message)
		{

			this.name = name;

		}

		/// <summary>
		/// The name of the rule involved in the exception.
		/// </summary>
		public String Name
		{
			get { return this.name; }

		}

	}
}
