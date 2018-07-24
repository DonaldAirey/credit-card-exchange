namespace FluidTrade.Core.Windows
{

	using System;
	using System.Windows;

    /// <summary>
	/// An empty timeline.
	/// </summary>
	public class NullTimeline : Storyline
	{

		// Private Static Fields
		private static Duration duration;

		/// <summary>
		/// Creates the static resources required by the NullTimeline class.
		/// </summary>
		static NullTimeline()
		{

			// The duration used by this Timeline is constant.
			NullTimeline.duration = new Duration(TimeSpan.Zero);

		}

		/// <summary>
		/// Creates an empty timeline.
		/// </summary>
		public NullTimeline()
		{

			// This creates a timeline that does nothing in no time at all and waits to be started.
			this.Duration = NullTimeline.duration;

		}

		/// <summary>
		/// Creates a freezable copy of this NullTimeline.
		/// </summary>
		/// <returns>A freezable copy of this NullTimeline.</returns>
		protected override System.Windows.Freezable CreateInstanceCore()
		{

			// This copy of the Timeline is used by the clock, leaving the original in a state that can be modified.
			return new NullTimeline();

		}

	}

}
