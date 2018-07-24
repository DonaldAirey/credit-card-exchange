namespace FluidTrade.Guardian.Windows
{

	using System;
	using System.Collections.Generic;
	using System.ServiceModel;
	using System.ServiceModel.Security;
	using System.Threading;
	using System.Windows;
	using System.Windows.Data;
	using System.Windows.Threading;
	using FluidTrade.Core;

	/// <summary>
	/// A window showing the progress of actually deleting an entity sub-tree.
	/// </summary>
	public class WindowDeleteProgress : WindowProgress
	{

		private delegate void setProgressLength(int count);

		/// <summary>
		/// Indicates the DeleteList dependency property.
		/// </summary>
		public static readonly DependencyProperty DeleteListProperty = DependencyProperty.Register("DeleteList", typeof(List<GuardianObject>), typeof(WindowDeleteProgress));

		private Thread deleteThread = null;
		private Stack<GuardianObject> deleteList = new Stack<GuardianObject>();
		private System.Timers.Timer timer;
		private DateTime startTime;
		private Int32 batchSize = 1000;

		/// <summary>
		/// Create a new delete window.
		/// </summary>
		public WindowDeleteProgress() : base()
		{

			this.Maximum = 0;
			this.Loaded += this.OnLoaded;
			this.Message = Properties.Resources.ProgressInitializing;
			this.Title = Properties.Resources.DeletingTitle;
			this.SetBinding(WindowImport.HeaderProperty, new Binding("Maximum") { Source = this, StringFormat = "{0:Deleting 0 items;Discovering;Discovering}" });

		}

		/// <summary>
		/// The entity to delete. Note that all children of the Entity will be delete prior to deleting the Entity itself.
		/// </summary>
		public List<GuardianObject> DeleteList
		{

			get { return this.GetValue(WindowDeleteProgress.DeleteListProperty) as List<GuardianObject>; }
			set { this.SetValue(WindowDeleteProgress.DeleteListProperty, value); }

		}

		/// <summary>
		/// Delete the delete list, updating the progress bar as we go.
		/// </summary>
		private void Delete()
		{
			

			List<GuardianObject> objects = null;
			GuardianObject failedObject = null;

			try
			{

				try
				{

					// Run through the delete list in LIFO order, maintaining the integrity of the entity tree as we delete.
					while (this.deleteList.Count > 0)
					{

						objects = this.GetBatch();

						this.Dispatcher.BeginInvoke(new WaitCallback(name =>
							this.Message = String.Format(FluidTrade.Guardian.Properties.Resources.DeletingObject, name as String)),
							DispatcherPriority.Normal, objects[0].ToString());

						if (objects.Count == 1)
						{

							objects[0].Commit();

						}
						else
						{

							Int32 attemptedSize = objects.Count;
							Int32 sentSize = objects[0].Commit(objects);

							if (sentSize < attemptedSize)
								this.batchSize = sentSize;

						}

						// The progress bar properties belong to the foreground, so toss the increment up to the main thread.
						this.Dispatcher.BeginInvoke(new WaitCallback(delegate(object count)
							{
								this.TimeLeftVisibility = Visibility.Visible;
								this.Value += (Int32)count;
								TimeSpan left = new TimeSpan((long)((this.Maximum - this.Value) * ((DateTime.Now - this.startTime).Ticks / this.Value)));
								this.TimeLeft = left < TimeSpan.Zero ? TimeSpan.Zero : left;
							}),
							DispatcherPriority.Normal, objects.Count);

					}

				}
				catch (DeleteException capturedException)
				{

					failedObject = capturedException.FailedObject;
					throw capturedException.InnerException;

				}

			}
			catch (IsSettledException exception)
			{

				Application.Current.Dispatcher.BeginInvoke(new WaitCallback(data =>
					MessageBox.Show(
						Application.Current.MainWindow,
						String.Format(FluidTrade.Guardian.Properties.Resources.DeleteFailedIsSettledAccount, data),
						Application.Current.MainWindow.Title)),
					DispatcherPriority.Normal,
					failedObject);

				EventLog.Warning("{0}: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);

			}
			catch (HasSettlementsException exception)
			{

				Application.Current.Dispatcher.BeginInvoke(new WaitCallback(data =>
					MessageBox.Show(
						Application.Current.MainWindow,
						String.Format(FluidTrade.Guardian.Properties.Resources.DeleteFailedHasSettledAccounts, data),
						Application.Current.MainWindow.Title)),
					DispatcherPriority.Normal,
					failedObject);

				EventLog.Warning("{0}: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);

			}
			catch (SecurityAccessDeniedException exception)
			{

				Application.Current.Dispatcher.BeginInvoke(new WaitCallback(data =>
					MessageBox.Show(
						Application.Current.MainWindow,
						String.Format(FluidTrade.Guardian.Properties.Resources.DeleteFailedAccessDenied, data),
						Application.Current.MainWindow.Title)),
					DispatcherPriority.Normal,
					failedObject);

				EventLog.Warning("{0}: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);

			}
			catch (CommunicationObjectFaultedException exception)
			{

				Application.Current.Dispatcher.BeginInvoke(new WaitCallback(data =>
					MessageBox.Show(
						Application.Current.MainWindow,
						String.Format(FluidTrade.Guardian.Properties.Resources.DeleteFailedConnectionDropped),
						Application.Current.MainWindow.Title)),
					DispatcherPriority.Normal);

				EventLog.Warning("{0}: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);

			}
			catch (Exception exception)
			{

				if (failedObject != null)
					Application.Current.Dispatcher.BeginInvoke(new WaitCallback(data =>
						MessageBox.Show(
							Application.Current.MainWindow,
							String.Format(FluidTrade.Guardian.Properties.Resources.DeleteFailed, data),
							Application.Current.MainWindow.Title)),
						DispatcherPriority.Normal,
						failedObject);
				else
					Application.Current.Dispatcher.BeginInvoke(new WaitCallback(data =>
						MessageBox.Show(
							Application.Current.MainWindow,
							String.Format(FluidTrade.Guardian.Properties.Resources.DeleteFailedCompletely),
							Application.Current.MainWindow.Title)),
						DispatcherPriority.Normal);


				EventLog.Warning("{0}: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);

			}

			// We're done. Windows Explorer doesn't throw up an alert, so we won't either.
			this.Dispatcher.BeginInvoke(new Action(this.Close), DispatcherPriority.Normal);

		}

		/// <summary>
		/// Build the list of entities to delete.
		/// </summary>
		/// <param name="deleteList">The list of items to delete.</param>
		private void GenerateObjectList(List<GuardianObject> deleteList)
		{

			foreach (GuardianObject deleteObj in deleteList)
			{

				foreach (GuardianObject depObj in deleteObj.GetDependents())
					this.deleteList.Push(depObj);

			}

			//if (this.deleteList.Count / 10 < 100)
			//    this.batchSize = this.deleteList.Count / 10;
			//if (this.batchSize < 1)
			//    this.batchSize = 1;

		}

		/// <summary>
		/// Get the next batch of objects to delete.
		/// </summary>
		/// <returns>The next batch of like-typed objects.</returns>
		private List<GuardianObject> GetBatch()
		{

			List<GuardianObject> batch = new List<GuardianObject>();
			Type type = this.deleteList.Peek().GetType();

			while (this.deleteList.Count > 0 && batch.Count < this.batchSize && type == this.deleteList.Peek().GetType())
			{

				GuardianObject obj = this.deleteList.Pop();

				obj.Deleted = true;
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

				if (this.deleteThread != null)
					this.deleteThread.Abort();

			}
			catch (Exception exception)
			{

				EventLog.Warning("Aborting the delete thread failed: {0}\n{1}", exception.Message, exception.StackTrace);

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

			this.Time = TimeSpan.Zero;
			this.TimeLeftVisibility = Visibility.Hidden;
			this.TimeLeft = TimeSpan.Zero;

			this.timer = new System.Timers.Timer(1000);
			this.timer.Elapsed += this.UpdateTime;

			deleteThread = new Thread(new ParameterizedThreadStart(data => this.StartDelete(data as List<GuardianObject>)));
			deleteThread.Name = string.Concat("DoDelete_", DateTime.Now.ToString("hh_mm"));
			deleteThread.IsBackground = true;

			deleteThread.Start(this.DeleteList);

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
		/// Initiate the delete process of deleting the objects.
		/// </summary>
		private void StartDelete(List<GuardianObject> deleteList)
		{
			try
			{
				lock (DataModel.SyncRoot)
					this.GenerateObjectList(deleteList);

				this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new setProgressLength(this.SetProgressLength), this.deleteList.Count);

				this.Delete();
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
