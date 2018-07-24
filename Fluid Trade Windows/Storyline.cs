namespace FluidTrade.Core.Windows
{

    using System;
    using System.Windows;
	using System.Windows.Media.Animation;

	/// <summary>
	/// A Collection of timelines that execute as a unit.
	/// </summary>
	public class Storyline : ParallelTimeline
	{

		// Public Static Fields
		public static readonly DependencyProperty TargetPropertyProperty;
		public static readonly DependencyProperty TargetObjectProperty;

		// Private Instance Fields
		private Int32 clockCounter;
		private ClockGroup clockGroup;

		// Public Events
		public new EventHandler Completed;

		/// <summary>
		/// Creates the static resources of this class.
		/// </summary>
		static Storyline()
		{

			// This attached property specifies the object that has been animated by a Timeline.
			Storyline.TargetObjectProperty = DependencyProperty.RegisterAttached("TargetObject", typeof(IAnimatable), typeof(Storyline));

			// This attached property specifies the property that is animated by a Timeline.
			Storyline.TargetPropertyProperty = DependencyProperty.RegisterAttached("TargetProperty", typeof(DependencyProperty), typeof(Storyline));

		}

		/// <summary>
		/// Gets the target of an animation.
		/// </summary>
		/// <param name="dependencyObject">The target object for the attached property.</param>
		/// <returns>The target of a Timeline.</returns>
		public static IAnimatable GetTargetObject(DependencyObject dependencyObject)
		{
			return (IAnimatable)dependencyObject.GetValue(Storyline.TargetObjectProperty);
		}

		/// <summary>
		/// Sets the target of an animation.
		/// </summary>
		/// <param name="dependencyObject">The target object for the attached property.</param>
		/// <param name="iAnimatable">The value of the attached property.</param>
		public static void SetTargetObject(DependencyObject dependencyObject, IAnimatable iAnimatable)
		{
			dependencyObject.SetValue(Storyline.TargetObjectProperty, iAnimatable);
		}

		/// <summary>
		/// Gets the property that is animated.
		/// </summary>
		/// <param name="dependencyObject">The target object for the attached property.</param>
		/// <returns>The animated property.</returns>
		public static DependencyProperty GetTargetProperty(DependencyObject dependencyObject)
		{
			return (DependencyProperty)dependencyObject.GetValue(Storyline.TargetPropertyProperty);
		}

		/// <summary>
		/// Gets the property that is animated.
		/// </summary>
		/// <param name="dependencyObject">The target object for the attached property.</param>
		/// <param name="dependencyProperty">The animated property.</param>
		public static void SetTargetProperty(DependencyObject dependencyObject, DependencyProperty dependencyProperty)
		{
			dependencyObject.SetValue(Storyline.TargetPropertyProperty, dependencyProperty);
		}

		/// <summary>
		/// <summary>
		/// Recursively counts up active clocks in this Timeline.
		/// </summary>
		/// <param name="clock">The current clock in a tree of clocks.</param>
		private void InitializeTimeline(Timeline timeline)
		{

			// The primary purpose of this method is to count up the number of clocks.
			this.clockCounter++;

			// This event handler will count down the number of completed clocks.  Only when the counter reaches zero will the new 'Completed' event be raised
			// for this Timeline.
			timeline.Completed += this.ClockEnd;

			// If this clock is the parent of a group, then each of the children in the group will be examined for clocks.  By this method, all the clocks in a
			// tree are recursively counted.
			if (timeline is TimelineGroup)
			{
				TimelineGroup timelineGroup = timeline as TimelineGroup;
				foreach (Timeline childTimeline in timelineGroup.Children)
					InitializeTimeline(childTimeline);
			}

		}

		/// Starts the next command on the queue.
		/// </summary>
		public void Begin()
		{

			// The original 'Completed' event for a ParallelTimeline appears to trigger before all the child clocks have finished. To work around this feature
			// (problem), each clock in this group is counted.  A event will trigger at the completion of each of the component clocks that will decrement this
			// counter.  Only when all the events have triggered will the new 'Completed' event be raised.
			InitializeTimeline(this);

			// The Timeline contains the logic to create a clock for itself and any children of that timeline.  Note that a reference to the created clock is
			// required here so that the clock won't be garbage collected while the animation is running.  This field has no other purpose.  Without the
			// reference, the clock sometimes miss the 'Completed' event because it has been garbage collected while running.
			this.clockGroup = this.CreateClock();

			// The clocks created above are abstract: they count, but don't really change anything.  This will apply the output of the clocks to real
			// properties on real objects.  The interface appears clumsy to this author; my guess is that Storyboards were meant to do most of the work of
			// animation.  Storyboards, however, require names and a namescope, which implies a more-or-less static existence for the animated objects.  The
			// more dynamic parts of an application require a more flexible approach.  The timelines handled here use attached properties to make the
			// associations between the clocks and their target properties.
			for (int clockIndex = 0; clockIndex < clockGroup.Children.Count; clockIndex++)
			{

				// Each of the child clocks has a child timeline associated with it.  Note that the child Timeline here is a frozen copy of the original
				// timeline.
				Clock childClock = clockGroup.Children[clockIndex];
				Timeline childTimeline = this.Children[clockIndex];

				// Child clocks that animate properties are associated with the target property here.
				if (childClock is AnimationClock)
				{

					// The Attached Properties of the Timeline are used to find the target for the animation clock.  The 'Target' and 'Property' direct the
					// output of the clock to a specific property.
					AnimationClock animationClock = childClock as AnimationClock;
					IAnimatable iAnimatable = Storyline.GetTargetObject(childTimeline);
					DependencyProperty dependencyProperty = Storyline.GetTargetProperty(childTimeline);
					if (iAnimatable != null && dependencyProperty != null)
						iAnimatable.ApplyAnimationClock(dependencyProperty, animationClock);

				}

			}

			// This is essentially a bug workaround.  The clocks should start up at a time determined by the 'BeginTime'. However, the NullTimelines, which
			// have a duration of zero, will not start spontaneously.  This call is unnecessary for most clocks but serves to kick the NullTimelines into gear.
			this.clockGroup.Controller.Begin();

		}

		/// <summary>
		/// Handles the completion of an individual clock.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="eventArgs">The unused event arguments.</param>
		private void ClockEnd(object sender, EventArgs eventArgs)
		{

			// Only when all the clocks in this Timeline have completed will the 'Completed' event be raised.
			if (--this.clockCounter == 0)
				if (this.Completed != null)
					this.Completed(sender, eventArgs);

		}

	}

}
