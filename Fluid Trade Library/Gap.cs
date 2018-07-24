namespace FluidTrade.Core
{

	using System;

    /// <summary>
	/// A gap in the row versions.
	/// </summary>
	public class Gap
	{

		// Private Fields
		public DateTime DateTime;
		private long start;
		private long end;

		/// <summary>
		/// Creates a gap.
		/// </summary>
		/// <param name="start">The starting row version of the gap.</param>
		/// <param name="end">The ending row version of the gap.</param>
		public Gap(long start, long end)
		{

			// Initialize the object
			this.DateTime = DateTime.Now;
			this.start = start;
			this.end = end;

		}

		/// <summary>
		/// Gets or sets the end of the gap.
		/// </summary>
		public long End { get { return this.end; } set { this.DateTime = DateTime.Now; this.end = value; } }

		/// <summary>
		/// Gets or sets the start of the gap.
		/// </summary>
		public long Start { get { return this.start; } set { this.DateTime = DateTime.Now; this.start = value; } }

		/// <summary>
		/// Returns a String that represents the current Gap.
		/// </summary>
		/// <returns>A string used to represent the current gap.</returns>
		public override string ToString()
		{

			// Format the gap into a readable form.
			return string.Format("{0} to {1}",
				this.start == long.MinValue ? (object)"Minimum" : (object)this.start,
				this.end == long.MaxValue ? (object)"Maximum" : (object)this.end);

		}

	}

}
