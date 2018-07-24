namespace FluidTrade.Guardian
{

    using FluidTrade.Core;
    using FluidTrade.Guardian.Windows;
    using System;
	using System.Collections.Generic;
	using System.Data;
    using System.Reflection;
	using System.Threading;
    using System.Windows.Threading;
   

	/// <summary>
	/// Notifies the user of matching opportunities.
	/// </summary>
	public class NegotiationService
	{

		// Private Instance Fields
		private NotificationDelegate notificationHandler;
		private Dispatcher foreground;
		private Dictionary<Guid, PopupNotification> notificationPopupTable;
		private Dictionary<String, Dispatcher> backgroundDispatcher;

		// Private Delegates
		private delegate void NotificationDelegate(NotificationInfo notificationInfo);
		private delegate void DeclineDelegate(Guid matchId);

		/// <summary>
		/// Will raise an event when the user has elected to negotiate a match.
		/// </summary>
		public event OpenObjectEventHandler OpenObject;

		/// <summary>
		/// Provides a notification message when a match opportunity is present.
		/// </summary>
		/// <param name="iContainer">The containing object.</param>
		public NegotiationService()
		{

			ManualResetEvent dispatcherCreated = new ManualResetEvent(false);

			// The initialization of this service requires a background thread to access the data model.
			FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(new WaitCallback(InitializeData));

			this.foreground = Dispatcher.CurrentDispatcher;
			this.backgroundDispatcher = new Dictionary<String, Dispatcher>();

			this.notificationHandler = this.OnNotification;

			this.notificationPopupTable = new Dictionary<Guid, PopupNotification>();

		}

		/// <summary>
		/// The set of popups currently displayed (or soon to be displayed).
		/// </summary>
		public Dictionary<Guid, PopupNotification> NotificationPopupTable
		{
			get
			{
				return this.notificationPopupTable;
			}
		}

		/// <summary>
		/// Initialize the background notification thread.
		/// </summary>
		/// <param name="dispatcherCreated">The "dispatcher created" ManualResetEvent object.</param>
		private void Background(String type, ManualResetEvent dispatcherCreated)
		{
			Dispatcher current = Dispatcher.CurrentDispatcher;
			this.backgroundDispatcher[type] = Dispatcher.CurrentDispatcher;
			current.UnhandledException += new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(FluidTrade.Core.UnhandledExceptionHelper.Dispatcher_UnhandledException);
			current.UnhandledExceptionFilter += new System.Windows.Threading.DispatcherUnhandledExceptionFilterEventHandler(FluidTrade.Core.UnhandledExceptionHelper.Dispatcher_UnhandledExceptionFilter);

			dispatcherCreated.Set();
			Dispatcher.Run();

		}

		/// <summary>
		/// Launch a delegate in current dispatcher thread.
		/// </summary>
		/// <param name="function">The delegate to call.</param>
		/// <param name="args">The arguments to call the delegate on.</param>
		public void Dispatch(Delegate function, params object[] args)
		{

			this.foreground.BeginInvoke(function, DispatcherPriority.Normal, args);

		}

		/// <summary>
		/// Create a dispatcher for a type.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns>The dispatcher.</returns>
		private Dispatcher GetDispatcher(String type)
		{

			ManualResetEvent dispatcherCreated = new ManualResetEvent(false);
			Thread thread = new Thread(() => this.Background(type, dispatcherCreated));

			thread.Priority = ThreadPriority.Lowest;
			thread.IsBackground = true;
			thread.Name = "Negotiation Notification for " + type;
			thread.Start();
			dispatcherCreated.WaitOne();

			return this.backgroundDispatcher[type];

		}
		
		/// <summary>
		/// Initializes the data required for this component.
		/// </summary>
		/// <param name="parameter">Thread initialization data (not used).</param>
		private void InitializeData(Object parameter)
		{

			try
			{

				// Lock the data model while the tables are read.
				Monitor.Enter(DataModel.SyncRoot);

				// Hook this service into the data model.  It will watch for any new matches and create a pop-up window when an
				// opportunity arises.
				DataModel.Match.MatchRowChanged += new MatchRowChangeEventHandler(OnMatchRowChanged);

			}
			catch (Exception exception)
			{

				// Write the error and stack trace out to the debug listener
				EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);

			}
			finally
			{

				// Allow other threads to access the data model.
				Monitor.Exit(DataModel.SyncRoot);

			}

		}

		/// <summary>
		/// Event handler for a match.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
		private void OnMatchRowChanged(Object sender, MatchRowChangeEventArgs e)
		{

			// When a new, pending match record has been added to the data mode, start a thread that will
			// display the notification window.
			if (e.Action == DataRowAction.Commit && e.Row.RowState != DataRowState.Detached)
			{

				if (e.Row.StatusRow.StatusCode == Status.Active ||
					e.Row.StatusRow.StatusCode == Status.PartialMatch ||
					e.Row.StatusRow.StatusCode == Status.ValidMatch ||
					e.Row.StatusRow.StatusCode == Status.ValidMatchFunds ||
					e.Row.StatusRow.StatusCode == Status.Declined)
				{

					NotificationInfo notificationInfo = new NotificationInfo();

					// The match record, working order, order type and security records are used to construct the title, symbol and
					// logo used by the notification window.
					MatchRow matchRow = e.Row;
					WorkingOrderRow workingOrderRow = matchRow.WorkingOrderRow;
					OrderTypeRow orderTypeRow = workingOrderRow.OrderTypeRow;
					SecurityRow securityRow = workingOrderRow.SecurityRowByFK_Security_WorkingOrder_SecurityId;

					// This is the primary method of identifying a match between two working orders.
					notificationInfo.MatchId = matchRow.MatchId;

					// The current status of the match is used to tell whether we're coming or going.
					notificationInfo.Status = matchRow.StatusRow.StatusCode;

					// Get the security symbol.
					notificationInfo.Symbol = securityRow.Symbol;

					// Create a logo bitmap.
					notificationInfo.Logo = securityRow.IsLogoNull() ? String.Empty : securityRow.Logo;

					// Provide a handler for the notification.
					if (!securityRow.EntityRow.TypeRow.IsNotifyingTypeNull())
					{

						notificationInfo.NotifierType = securityRow.EntityRow.TypeRow.NotifyingType;

						// Construct the title for the notification window.
						notificationInfo.Message = String.Format("{0} of {1}", workingOrderRow.SideRow.Description, securityRow.Symbol);

						// Now that the information has been extracted for the data model, the background execution queue can handle the rest of the
						// notification of the user.
						this.OnNotification(notificationInfo);

					}

				}

			}

		}

		/// <summary>
		/// Build and run a notification object.
		/// </summary>
		/// <param name="notificationInfo">The notificiation information.</param>
		private void OnNotification(NotificationInfo notificationInfo)
		{

			// The tree node contains a specification for the page that can handle this type of object.  The is loaded up dynamically from that
			// specification.
			String[] typeParts = notificationInfo.NotifierType.Split(',');
			if (typeParts.Length < 2)
				throw new Exception(string.Format("Can't create an Notification '{0}'", notificationInfo.NotifierType));
			String assemblyName = typeParts[1].Trim();
			for (int index = 2; index < typeParts.Length; index++)
				assemblyName += ',' + typeParts[index].Trim();
			String type = typeParts[0].Trim();
			Assembly assembly = Assembly.Load(assemblyName);
			Dispatcher dispatcher = backgroundDispatcher.ContainsKey(type) ? backgroundDispatcher[type] : this.GetDispatcher(type);

			// Now that the assembly containing the crossing logic is loaded, an object can be created that will dynamically handle the cross.
			dispatcher.BeginInvoke(new Action(delegate()
				{
					try
					{
						assembly.CreateInstance(
							typeParts[0].Trim(),
							false,
							BindingFlags.CreateInstance,
							null,
							new object[] { this, notificationInfo },
							System.Globalization.CultureInfo.CurrentCulture,
							null);
					}
					catch (Exception exception)
					{
						EventLog.Warning("Negotiation error: ({0}) {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);
					}
				}), DispatcherPriority.Normal);

		}

		/// <summary>
		/// Open/select an it in the main window.
		/// </summary>
		/// <param name="openObjectEventArgs">The event arguments.</param>
		public void OnOpenObject(OpenObjectEventArgs openObjectEventArgs)
		{

			// Notify the owner of this service that it should navigate to the item in the matching blotter that was selected by the user.
			if (this.OpenObject != null)
				this.OpenObject(this, openObjectEventArgs);

		}

	}

}
