namespace FluidTrade.Guardian
{

	using System;
	using FluidTrade.Guardian.Windows;

	/// <summary>
	/// Exception thrown when deletes fail.
	/// </summary>
	public class DeleteException : Exception
	{

		private GuardianObject failedObject;

		/// <summary>
		/// Create a new exception.
		/// </summary>
		/// <param name="failedObject">The object that failed.</param>
		/// <param name="innerException">The original exception.</param>
		public DeleteException(GuardianObject failedObject, Exception innerException)
			: base("delete error", innerException)
		{

			this.failedObject = failedObject;

		}

		/// <summary>
		/// The object for which deleting failed.
		/// </summary>
		public GuardianObject FailedObject
		{
			get { return this.failedObject; }
		}

	}

}
