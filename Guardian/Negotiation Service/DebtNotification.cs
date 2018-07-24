namespace FluidTrade.Guardian
{

    using System;
    using System.IO;
	using System.Threading;
    using System.Windows.Media.Imaging;
    using FluidTrade.Core;
    using FluidTrade.Guardian.Windows;

	/// <summary>
	/// Notifies the user of matching opportunities.
	/// </summary>
	public class DebtNotification
	{

		// Private Instance Fields
		private NegotiationService negotiationService;

		// Private Delegates
		private delegate void NotificationDelegate(NotificationInfo notificationInfo);
		private delegate void DeclineDelegate(Guid matchId);

		public DebtNotification(NegotiationService negotiationService, NotificationInfo notificationInfo)
		{

			this.negotiationService = negotiationService;
			this.negotiationService.Dispatch(new Action(() => this.LaunchPopup(notificationInfo)));

		}

		private void LaunchPopup(NotificationInfo notificationInfo)
		{

			try
			{

				if (notificationInfo.Status == Status.Active)
				{

					// The notification window looks and acts like the Microsoft Instant Messaging window.  It will pop up in the lower
					// right hand corner of the screen with a title, the corporate logo and a chance to either accept or decline the
					// opportunity for a match.
					PopupNotification popupNotification = new PopupNotification();
					popupNotification.MatchId = notificationInfo.MatchId;
					popupNotification.Symbol = notificationInfo.Symbol;
					popupNotification.Title = notificationInfo.Message;

					// Create a bitmap image for the logo.
					if (notificationInfo.Logo != string.Empty)
					{
						BitmapImage bitmapImage = new BitmapImage();
						bitmapImage.BeginInit();
						bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
						bitmapImage.StreamSource = new MemoryStream(Convert.FromBase64String(notificationInfo.Logo));
						bitmapImage.EndInit();
						popupNotification.Logo = bitmapImage;
					}

					popupNotification.Accept += new MatchEventHandler(AcceptNegotiation);
					popupNotification.Decline += new MatchEventHandler(DeclineNegotiation);
					popupNotification.ChangeOptions += new EventHandler(ChangeOptions);
					popupNotification.IsOpen = true;

					this.negotiationService.NotificationPopupTable.Add(notificationInfo.MatchId, popupNotification);

				}
				else
				{
					if (notificationInfo.Status == Status.Declined)
					{
						PopupNotification popupNotification;
						if (this.negotiationService.NotificationPopupTable.TryGetValue(notificationInfo.MatchId, out popupNotification))
						{
							popupNotification.Close();
							this.negotiationService.NotificationPopupTable.Remove(notificationInfo.MatchId);
						}
					}

				}

			}
			catch (Exception exception)
			{
				EventLog.Error("{0}: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);
			}

		}

		/// <summary>
		/// Accepts the opportunity to match an order.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="matchEventArgs">The event arguments.</param>
		private void AcceptNegotiation(Object sender, MatchEventArgs matchEventArgs)
		{

			// The next step involves extracting data from the data model and must be done in a different thread than the window
			// thread.
			FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(new WaitCallback(AcceptNegotiationThread), matchEventArgs.MatchId);

		}

		/// <summary>
		/// Marshals the data needed to navigate the client application to the accepted item in the Match blotter.
		/// </summary>
		/// <param name="parameter">The thread initialization data.</param>
		private void AcceptNegotiationThread(Object parameter)
		{

			// Extract the parameters.
			Guid matchId = (Guid)parameter;

			// The main goal of this section of code is to construct an object that can be used to navigate the
			// client container to the selected item in the blotter.
			OpenObjectEventArgs openObjectEventArgs = null;

			lock (DataModel.SyncRoot)
			{

				// The 'BlotterMatchDetail' can be used to open up a Blotter in a viewer and select the Match.
				MatchRow matchRow = DataModel.Match.MatchKey.Find(matchId);
				if (matchRow != null)
					openObjectEventArgs = new OpenObjectEventArgs(
						new Blotter(matchRow.WorkingOrderRow.BlotterRow.EntityRow),
						new Match[] { new Match(matchRow.MatchId) });

			}

			this.negotiationService.OnOpenObject(openObjectEventArgs);

		}

		/// <summary>
		/// Refuse the opportunity to match an order.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="matchEventArgs">The event arguments.</param>
		private void DeclineNegotiation(Object sender, MatchEventArgs matchEventArgs)
		{

			// The next step involves extracting data from the data model and must be done in a different thread than the window
			// thread.
			FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(new WaitCallback(DeclineNegotiationThread), matchEventArgs.MatchId);

		}

		/// <summary>
		/// Marshals the data needed refuse a chance to negotiate a trade.
		/// </summary>
		/// <param name="parameter">The thread initialization data.</param>
		private void DeclineNegotiationThread(Object parameter)
		{

			// Extract the thread parameters
			Guid matchId = (Guid)parameter;

			try
			{

				// Lock the data model while the tables are read.
				Monitor.Enter(DataModel.SyncRoot);

				// Find the Match record.
				MatchRow matchRow = DataModel.Match.MatchKey.Find(matchId);

			}
			finally
			{

				// Allow other threads to access the data model.
				Monitor.Exit(DataModel.SyncRoot);

			}

			// If the command batch was built successfully, then execute it.
			try
			{

				// Call the web server to rename the object on the database.  Note that this method must be called when there
				// are no locks to prevent deadlocking.  That is why it appears in it's own 'try/catch' block.

			}
			catch (Exception exception)
			{

				// Write any server generated error messages to the event log.
				EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);

			}

		}

		private void ChangeOptions(Object sender, EventArgs eventArgs)
		{


		}

	}

}
