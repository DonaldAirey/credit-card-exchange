namespace FluidTrade.Guardian.Windows
{

	using System;
	using System.Collections.Generic;
	using System.ServiceModel.Security;
	using System.Threading;
	using System.Windows;
	using System.Windows.Data;
	using System.Windows.Threading;
	using FluidTrade.Core;

	/// <summary>
	/// 
	/// </summary>
	public class WindowMoveProgress : WindowProgress
	{

		private delegate void setProgressLength(int count);

		/// <summary>
		/// Indicates the MoveList dependency property.
		/// </summary>
		public static readonly DependencyProperty MoveListProperty = DependencyProperty.Register("MoveList", typeof(List<IMovableObject>), typeof(WindowMoveProgress));
		/// <summary>
		/// Indicates the Target dependency property.
		/// </summary>
		public static readonly DependencyProperty TargetProperty = DependencyProperty.Register("Target", typeof(GuardianObject), typeof(WindowMoveProgress));

		private Thread moveThread = null;
		private Stack<IMovableObject> moveList = new Stack<IMovableObject>();
		private System.Timers.Timer timer;
		private DateTime startTime;
		private Int32 batchSize = 1000;

		/// <summary>
		/// Create a new delete window.
		/// </summary>
		public WindowMoveProgress()
			: base()
		{

			this.Maximum = 0;
			this.Loaded += this.OnLoaded;
			this.Message = Properties.Resources.ProgressInitializing;
			this.Title = Properties.Resources.MovingTitle;
			this.SetBinding(WindowMoveProgress.HeaderProperty, new Binding("Maximum") { Source = this, StringFormat = "{0:Moving 0 items;Discovering;Discovering}" });

		}

		/// <summary>
		/// The list of items to move.
		/// </summary>
		public List<IMovableObject> MoveList
		{

			get { return this.GetValue(WindowMoveProgress.MoveListProperty) as List<IMovableObject>; }
			set { this.SetValue(WindowMoveProgress.MoveListProperty, value); }

		}

		/// <summary>
		/// The container to move the objects into.
		/// </summary>
		public GuardianObject Target
		{

			get { return this.GetValue(WindowMoveProgress.TargetProperty) as GuardianObject; }
			set { this.SetValue(WindowMoveProgress.TargetProperty, value); }

		}

		/// <summary>
		/// Move the move list, updating the progress bar as we go.
		/// </summary>
		private void Move(GuardianObject target)
		{
			

			List<IMovableObject> objects = null;
			List<FluidTrade.Guardian.TradingSupportReference.ErrorInfo> errors = new List<FluidTrade.Guardian.TradingSupportReference.ErrorInfo>();
			try
			{

				// Run through the delete list in LIFO order, maintaining the integrity of the entity tree as we delete.
				while (this.moveList.Count > 0)
				{

					objects = this.GetBatch();

					this.Dispatcher.BeginInvoke(new WaitCallback(name =>
						this.Message = String.Format(FluidTrade.Guardian.Properties.Resources.MovingObject, name as String)),
						DispatcherPriority.Normal, objects[0].ToString());

					if (objects.Count == 1)
						objects[0].Move(target, errors);
					else
						objects[0].Move(objects, target, errors);

					// The progress bar properties belong to the foreground, so toss the increment up to the main thread.
					this.Dispatcher.BeginInvoke(new WaitCallback(
						delegate(object count)
						{
							this.TimeLeftVisibility = Visibility.Visible;
							this.Value += (Int32)count;
							TimeSpan left = new TimeSpan((long)((this.Maximum - this.Value) * ((DateTime.Now - this.startTime).Ticks / this.Value)));
							this.TimeLeft = left < TimeSpan.Zero ? TimeSpan.Zero : left;
						}),
						DispatcherPriority.Normal, objects.Count);

				}

			}
			catch (SecurityAccessDeniedException exception)
			{

				Application.Current.Dispatcher.BeginInvoke(new WaitCallback(data =>
					MessageBox.Show(
						Application.Current.MainWindow,
						String.Format(FluidTrade.Guardian.Properties.Resources.MoveFailedAccessDenied, data),
						Application.Current.MainWindow.Title)),
					DispatcherPriority.Normal,
					objects[0]);

				EventLog.Warning("{0}: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);

			}
			catch (Exception exception)
			{

				Application.Current.Dispatcher.BeginInvoke(new WaitCallback(data =>
					MessageBox.Show(
						Application.Current.MainWindow,
						String.Format(FluidTrade.Guardian.Properties.Resources.MoveFailed, data),
						Application.Current.MainWindow.Title)),
					DispatcherPriority.Normal,
					objects[0]);

				EventLog.Warning("{0}: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);

			}

			if (errors.Count > 0)
			{
				Application.Current.Dispatcher.BeginInvoke(new WaitCallback(data =>
					MessageBox.Show(
						Application.Current.MainWindow,
						FluidTrade.Guardian.Properties.Resources.MovePartialFail,
						Application.Current.MainWindow.Title)),
					DispatcherPriority.Normal,
					objects[0]);
			}

			// We're done. Windows Explorer doesn't throw up an alert, so we won't either.
			this.Dispatcher.BeginInvoke(new Action(this.Close), DispatcherPriority.Normal);
			
		}

		/// <summary>
		/// Get the next batch of objects to move.
		/// </summary>
		/// <returns>The next batch of like-typed objects.</returns>
		private List<IMovableObject> GetBatch()
		{

			List<IMovableObject> batch = new List<IMovableObject>();
			Type type = this.moveList.Peek().GetType();

			while (this.moveList.Count > 0 && batch.Count < this.batchSize && type == this.moveList.Peek().GetType())
			{

				IMovableObject obj = this.moveList.Pop();

				batch.Add(obj);

			}

			batch.Reverse();

			return batch;

		}

		/// <summary>
		/// Abort the delete thread and close the window.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="eventArgs"></param>
		protected override void OnCancel(object sender, EventArgs eventArgs)
		{

			try
			{

				if (this.moveThread != null)
					this.moveThread.Abort();

			}
			catch (Exception exception)
			{

				EventLog.Warning("Aborting the move thread failed: {0}\n{1}", exception.Message, exception.StackTrace);

			}

			base.OnCancel(sender, eventArgs);

		}

		/// <summary>
		/// Handle the Loaded event. Start off the delete thread.
		/// </summary>
		/// <param name="sender">The delete window.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnLoaded(object sender, EventArgs eventArgs)
		{

			GuardianObject target = this.Target.Clone();

			this.Time = TimeSpan.Zero;
			this.TimeLeftVisibility = Visibility.Hidden;
			this.TimeLeft = TimeSpan.Zero;

			this.timer = new System.Timers.Timer(1000);
			this.timer.Elapsed += this.UpdateTime;

			moveThread = new Thread(data =>
				this.StartMove(data as List<IMovableObject>, target));

			moveThread.Name = string.Concat("DoMove_", DateTime.Now.ToString("hh_mm"));
			moveThread.IsBackground = true;
			
			moveThread.Start(this.MoveList);

		}

		/// <summary>
		/// Set the maximum count of the progress bar start it ticking up.
		/// </summary>
		/// <param name="count"></param>
		private void SetProgressLength(int count)
		{

			this.Maximum = count;
			this.IsIndeterminate = false;
			this.startTime = DateTime.Now;
			this.timer.Start();
		
		}

		/// <summary>
		/// Initiate the process of moving the objects.
		/// </summary>
		private void StartMove(List<IMovableObject> moveList, GuardianObject target)
		{
			try
			{
				foreach (IMovableObject obj in moveList)
					this.moveList.Push(obj);

				//this.batchSize = Math.Max(this.moveList.Count / 10, 1000);
				//if (this.batchSize < 1)
				//    this.batchSize = 1;
				
				this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new setProgressLength(this.SetProgressLength), this.moveList.Count);

				this.Move(target);
			}
			catch (Exception ex)
			{
				//if dont catch ex, app could come down
				FluidTrade.Core.EventLog.Error("{0}\r\b{1}", ex.Message, ex.StackTrace);
			}
		}

		/// <summary>
		/// Update our timer and countdown.
		/// </summary>
		/// <param name="sender">The Timer.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void UpdateTime(object sender, EventArgs eventArgs)
		{

			this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate()
			{

				TimeSpan left = this.TimeLeft - new TimeSpan(0, 0, 1);
				this.TimeLeft = left < TimeSpan.Zero ? TimeSpan.Zero : left;
				this.Time = DateTime.Now - this.startTime;

			}));

		}

	}

}
