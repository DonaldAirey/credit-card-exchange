namespace FluidTrade.Guardian.Windows
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using FluidTrade.Guardian.Windows;

	/// <summary>
	/// Exception thrown when a user is not found.
	/// </summary>
	public class UserNotFoundException : Exception
	{

		User user = null;

		/// <summary>
		/// Create a new exception with a message.
		/// </summary>
		/// <param name="message">The message.</param>
		public UserNotFoundException(String message)
			: base(message)
		{


		}

		/// <summary>
		/// Create a new exception with a message and a user.
		/// </summary>
		/// <param name="user">The user who is missing.</param>
		/// <param name="message">The message.</param>
		public UserNotFoundException(User user, String message)
			: base(message)
		{

			this.user = user;

		}

		/// <summary>
		/// The user that caused the exception.
		/// </summary>
		public User User
		{
			get { return this.user; }

		}

	}

}
