namespace FluidTrade.Guardian
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// Exception thrown when data import cannot find any column headers.
	/// </summary>
	class ImportHeaderNotFoundException: Exception
	{

		ICollection<String> headers;
		ICollection<String> missingHeaders;

		/// <summary>
		/// Create an empty exception.
		/// </summary>
		/// <param name="headers">The set expected headers.</param>
		public ImportHeaderNotFoundException(ICollection<String> headers)
		{

			this.headers = headers;
			this.missingHeaders = null;

		}

		/// <summary>
		/// Create an empty exception.
		/// </summary>
		/// <param name="headers">The set expected headers.</param>
		/// <param name="missingHeaders">The set headers that are missing, or null if they're all missing.</param>
		public ImportHeaderNotFoundException(ICollection<String> headers, ICollection<String> missingHeaders)
		{

			this.headers = headers;
			this.missingHeaders = missingHeaders;

		}

		/// <summary>
		/// Create an exception with a message.
		/// </summary>
		/// <param name="message">The message to include.</param>
		/// <param name="headers">The set expected headers.</param>
		public ImportHeaderNotFoundException(String message, ICollection<String> headers)
			: base(message)
		{

			this.headers = headers;
			this.missingHeaders = null;

		}

		/// <summary>
		/// Create an exception with a message.
		/// </summary>
		/// <param name="message">The message to include.</param>
		/// <param name="headers">The set expected headers.</param>
		/// <param name="missingHeaders">The set headers that are missing, or null if they're all missing.</param>
		public ImportHeaderNotFoundException(String message, ICollection<String> headers, ICollection<String> missingHeaders)
			: base(message)
		{

			this.headers = headers;
			this.missingHeaders = missingHeaders;

		}

		/// <summary>
		/// The set of expected headers.
		/// </summary>
		public ICollection<String> Headers
		{

			get { return this.headers; }

		}

		/// <summary>
		/// The subset of headers that are missing. If null, they're all missing.
		/// </summary>
		public ICollection<String> MissingHeaders
		{

			get { return this.missingHeaders; }

		}

	}

}
