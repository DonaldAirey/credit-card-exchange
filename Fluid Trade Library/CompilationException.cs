namespace FluidTrade.Core
{

	using System;
    using System.Runtime.Serialization;

    /// <summary>
	/// Thrown when an error prevents a CodeDOM module from compiling and loading into memory.
	/// </summary>
	[Serializable]
	public class CompilationException : Exception
	{

		/// <summary>
		/// Create an exception that indicates an error compiling and loading a module.
		/// </summary>
		/// <param name="format">The formatted output.</param>
		/// <param name="args">Arguments for the formatted output.</param>
		public CompilationException(string format, params object[] args) : base(string.Format(format, args)) { }

		/// <summary>
		/// Serializes a compilation exception.
		/// </summary>
		/// <param name="info">The data to be formatted.</param>
		/// <param name="context">The context of the format operation.</param>
		protected CompilationException(SerializationInfo info, StreamingContext context) : base(info, context) { }

	}
}
