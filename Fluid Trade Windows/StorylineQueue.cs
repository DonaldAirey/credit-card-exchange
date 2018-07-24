namespace FluidTrade.Core.Windows
{

    using System;
	using System.Collections.Generic;
    using System.Windows.Threading;

	/// <summary>
	/// A queue of TimelineSegments.
	/// </summary>
	public class StorylineQueue
	{

		// Private Instance Fields
		private Queue<TimelineSegment> storylineQueue;

		// Private Delegates
		private delegate void StartCommandDelegate();

		/// <summary>
		/// Creates a queue of TimelineSegments.
		/// </summary>
		public StorylineQueue()
		{

			// Initialize the object
			this.storylineQueue = new Queue<TimelineSegment>();

		}

		/// <summary>
		/// Removes all objects from the MarkThree.Windows.StorylineQueue
		/// </summary>
		public void Clear()
		{

			// Clear the object.
			this.storylineQueue.Clear();

		}

		/// <summary>
		/// Starts the next command on the queue.
		/// </summary>
		private void StartCommand()
		{

			// Extract the event handler and the arguments from the queue.
			TimelineSegment timelineSegment = this.storylineQueue.Peek();

			try
			{

				// This will execute the command in the queue and trap any errors.  When the Storyline is completed, the next
				// command in the queue will be executed.
				timelineSegment.Storyline = timelineSegment.TimelineHandler(this, timelineSegment.GenericEventArgs);
				timelineSegment.Storyline.Completed += DequeueCommand;
				timelineSegment.Storyline.Begin();

			}
			catch (Exception exception)
			{

				// Exceptions are written to the event log.
				EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);

				// It is critical that the chain of animation continues after an exception.
				DequeueCommand(this, timelineSegment.GenericEventArgs);

			}

		}

		/// <summary>
		/// Execute a command to perform an animation sequence.
		/// </summary>
		/// <param name="eventHandler">The handler for the animation timeline.</param>
		/// <param name="arguments">The arguments for the animation timeline.</param>
		public void Enqueue(StorylineHandler timelineHandler, GenericEventArgs genericEventArgs)
		{

			// Place the new command in a list of items that will be executed in the order they are place in the queue.
			this.storylineQueue.Enqueue(new TimelineSegment(timelineHandler, genericEventArgs));

			// Animation is accomplished through a series of event handlers that run as each animation before it completes.  The
			// queue is the starting point for each of these animation sequences.  It holds commands that have queued up while the
			// previous animations -- which take a finite amount of time to complete -- finish their time lines.  This action
			// 'primes' the queue that drives the animations.  Note that the command is invoked through the dispatcher at a very
			// low proprity.  This allows the windows subsystem to update.
			if (storylineQueue.Count == 1)
				Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
					(StartCommandDelegate)(() => { StartCommand(); }));

		}

		/// <summary>
		/// Execute a command to perform an animation sequence.
		/// </summary>
		/// <param name="eventHandler">The handler for the animation timeline.</param>
		/// <param name="arguments">The arguments for the animation timeline.</param>
		public void Enqueue(StorylineHandler timelineHandler, params object[] arguments)
		{

			// Place the new command in a list of items that will be executed in the order they are place in the queue.
			this.storylineQueue.Enqueue(new TimelineSegment(timelineHandler, arguments));

			// Animation is accomplished through a series of event handlers that run as each animation before it completes.  The
			// queue is the starting point for each of these animation sequences.  It holds commands that have queued up while the
			// previous animations -- which take a finite amount of time to complete -- finish their time lines.  This action
			// 'primes' the queue that drives the animations.  Note that the command is invoked through the dispatcher at a very
			// low proprity.  This allows the windows subsystem to update.
			if (storylineQueue.Count == 1)
				Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
					(StartCommandDelegate)(() => { StartCommand(); }));

		}

		/// <summary>
		/// Finishes an animation sequence and launches the next one in the queue if it exists.
		/// </summary>
		private void DequeueCommand(object sender, EventArgs eventArgs)
		{

			// The main idea here is to remove the current command from an animation queue because all segments have finished
			// running.  Then, if the queue is not empty, launch the next animation sequence.  Note that the command is invoked 
			// through the dispatcher at a very low proprity.  This allows the windows subsystem to update.
			this.storylineQueue.Dequeue();
			if (this.storylineQueue.Count != 0)
				Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
					(StartCommandDelegate)(() => { StartCommand(); }));

		}

	}

}
