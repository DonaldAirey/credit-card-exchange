namespace FluidTrade.Guardian.Windows
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using FluidTrade.Guardian.Windows;

	/// <summary>
	/// Exception thrown when a group is not found.
	/// </summary>
	public class GroupNotFoundException : Exception
	{

		Group group = null;

		/// <summary>
		/// Create a new exception with a message.
		/// </summary>
		/// <param name="message">The message.</param>
		public GroupNotFoundException(String message)
			: base(message)
		{


		}

		/// <summary>
		/// Create a new exception with a message and a group.
		/// </summary>
		/// <param name="group">The group who is missing.</param>
		/// <param name="message">The message.</param>
		public GroupNotFoundException(Group group, String message)
			: base(message)
		{

			this.group = group;

		}

		/// <summary>
		/// The group that caused the exception.
		/// </summary>
		public Group Group
		{
			get { return this.group; }

		}

	}

}
