namespace FluidTrade.Core
{

	using System;

	/// <summary>
	/// Contains event data for indicating progress.
	/// </summary>
	public class ProgressEventArgs : EventArgs
	{

		/// <summary>
		/// The current count.
		/// </summary>
		private Double current;

		/// <summary>
		/// The end of the progress range.
		/// </summary>
		private Double maximum;

		/// <summary>
		/// The start of the progress range.
		/// </summary>
		private Double minimum;

		/// <summary>
		/// Creates event arguments that indicate how much progress has been made.
		/// </summary>
		/// <param name="rowsSoFar">The current number of items.</param>
		/// <param name="totalRows">The total number of items.</param>
		public ProgressEventArgs(Double minimum, Double maximum, Double current)
		{

			// Initialize the object.
			this.maximum = maximum;
			this.minimum = minimum;
			this.current = current;

		}

		/// <summary>
		/// The current progress.
		/// </summary>
		public Double Current
		{
			get
			{
				return this.current;
			}
		}

		/// <summary>
		/// The maximum of the progress range.
		/// </summary>
		public Double Maximum
		{
			get
			{
				return this.maximum;
			}
		}

		/// <summary>
		/// The minimum of the progress range.
		/// </summary>
		public Double Minimum
		{
			get
			{
				return this.minimum;
			}
		}

	}
}
