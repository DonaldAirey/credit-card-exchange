namespace FluidTrade.Guardian.Windows
{

	using System;
	using System.Data;
	using System.Linq;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Input;
	using System.Windows.Threading;
	using FluidTrade.Core;
	using FluidTrade.Guardian.Utilities;

    /// <summary>
    /// Generic properties for any blotter.
    /// </summary>
    public class WindowBlotterProperties : WindowEntityProperties
    {

		/// <summary>
		/// Command fired when the edit commission schedule button is pressed.
		/// </summary>
		public static readonly RoutedUICommand EditCommissionSchedule = new RoutedUICommand("Edit Commission Schedule...", "EditCommissionSchedule", typeof(WindowBlotterProperties));
		
		private User currentUser;

		// Background Fields:
		private Guid entityId;

        /// <summary>
        /// Create a new properties dialog box.
        /// </summary>
        public WindowBlotterProperties()
            : base()
        {

			this.BuildPaymentScheduleTab();
			this.CommandBindings.Add(new CommandBinding(WindowBlotterProperties.EditCommissionSchedule, this.OnEditCommissionSchedule, this.CanEditCommissionSchedule));
			ThreadPoolHelper.QueueUserWorkItem(data =>
				this.InitializeUser());

			this.Loaded += delegate(object sender, RoutedEventArgs eventArgs)
			{
				DataModel.Blotter.RowChanging += this.FilterRow;
			};
			this.Unloaded += delegate(object sender, RoutedEventArgs eventArgs)
			{
				DataModel.Blotter.RowChanging -= this.FilterRow;
			};

        }

		/// <summary>
		/// Create the PaymentScheduleControl and add it to the Customize tab.
		/// </summary>
		private void BuildPaymentScheduleTab()
		{

			GroupBox groupBox = new GroupBox { Header = "Commission Schedule" };
			StackPanel panel = new StackPanel() { Orientation = Orientation.Horizontal };
			Button button = new Button { Command = WindowBlotterProperties.EditCommissionSchedule };

			panel.Children.Add(button);
			groupBox.Content = panel;

			this.customizePanel.Children.Add(groupBox);

		}

		/// <summary>
		/// Get information about the current user.
		/// </summary>
		private void InitializeUser()
		{

			User user = Entity.New(DataModel.Entity.EntityKey.Find(UserContext.Instance.UserId)) as User;

			this.Dispatcher.BeginInvoke(new Action(() =>
				this.currentUser = user),
				DispatcherPriority.Normal);

		}

		/// <summary>
		/// Determine whether the user is allowed to edit commissions.
		/// </summary>
		/// <param name="source">The edit commissions button.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void CanEditCommissionSchedule(object source, CanExecuteRoutedEventArgs eventArgs)
		{

			eventArgs.CanExecute = this.currentUser != null &&
				this.currentUser.Groups.Any(g =>
					g.GroupType == GroupType.FluidTradeAdmin || g.GroupType == GroupType.ExchangeAdmin);

		}


		/// <summary>
		/// Determine whether a row is one of the rows being displayed by the window.
		/// </summary>
		/// <param name="sender">The table that sent the event.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void FilterRow(object sender, EventArgs eventArgs)
		{

			DataRow row = eventArgs is DataRowChangeEventArgs ? (eventArgs as DataRowChangeEventArgs).Row : (eventArgs as DataTableNewRowEventArgs).Row;

			if (!(row.RowState == DataRowState.Detached || row.RowState == DataRowState.Deleted))
				if (row is BlotterRow && (row as BlotterRow).BlotterId == this.entityId)
					this.MustRedisplay = true;

		}

		/// <summary>
		/// Handle the edit commission schedule command.
		/// </summary>
		/// <param name="source">The edit commission schedule button.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnEditCommissionSchedule(object source, ExecutedRoutedEventArgs eventArgs)
		{

			WindowPaymentSchedule dialog = new WindowPaymentSchedule();

			dialog.Owner = this;
			dialog.Entity = this.Entity as Blotter;

			dialog.Show();
			dialog.Closed += (s, e) => Win32Interop.EnableWindow(this);
			Win32Interop.DisableWindow(this);

		}

		/// <summary>
		/// Handle the Entity changing.
		/// </summary>
		protected override void OnEntityChanged()
		{

			base.OnEntityChanged();

			ThreadPoolHelper.QueueUserWorkItem(delegate(object data)
				{
					lock(DataModel.SyncRoot)
						this.entityId = (Guid)data;
				},
				this.Entity.EntityId);

		}

    }

}
