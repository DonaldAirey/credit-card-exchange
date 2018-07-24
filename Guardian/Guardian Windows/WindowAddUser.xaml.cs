namespace FluidTrade.Guardian.Windows
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Data;
	using System.Windows.Documents;
	using System.Windows.Input;
	using System.Windows.Media;
	using System.Windows.Media.Imaging;
	using System.Windows.Navigation;
	using System.Windows.Shapes;
	using System.Threading;
	using FluidTrade.Core;	
	using FluidTrade.Guardian.AdminSupportReference;
	using System.ServiceModel;

	/// <summary>
	/// Interaction logic for WindowAddUser.xaml
	/// </summary>
	public partial class WindowAddUser : Window
	{

		/// <summary>
		/// Indicates the CanFinish dependency property.
		/// </summary>
		public static readonly DependencyPropertyKey CanFinishProperty =
			DependencyProperty.RegisterReadOnly("CanFinish", typeof(Boolean), typeof(WindowAddUser), new PropertyMetadata(false));
		/// <summary>
		/// Indicates the Tenant dependency property.
		/// </summary>
		public static readonly DependencyProperty TenantProperty =
			DependencyProperty.Register("Tenant", typeof(Guid), typeof(WindowAddUser), new PropertyMetadata(OnTenantChanged));

		/// <summary>
		/// Create a new add-user "wizard".
		/// </summary>
		public WindowAddUser()
		{
		
			InitializeComponent();
			this.tabs.SelectedIndex = 0;

			this.Loaded += (s, e) =>
				this.tenant.ItemsSource = new TenantList();
			this.Unloaded += (s, e) =>
				this.Dispose();

		}

		/// <summary>
		/// True if the current page is the last page.
		/// </summary>
		public Boolean CanFinish
		{

			get { return this.tabs.SelectedIndex == this.tabs.Items.Count - 1; }

		}

		/// <summary>
		/// The entity id of the tenant whose users to manage.
		/// </summary>
		public Guid Tenant
		{

			get { return (Guid)this.GetValue(WindowAddUser.TenantProperty); }
			set { this.SetValue(WindowAddUser.TenantProperty, value); }

		}

		/// <summary>
		/// Determine whether we can go forward a screen.
		/// </summary>
		/// <param name="sender">The Next button.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void CanOkay(object sender, CanExecuteRoutedEventArgs eventArgs)
		{

			eventArgs.CanExecute = this.CanFinish;

		}

		/// <summary>
		/// Determine whether we can "finish" (yet).
		/// </summary>
		/// <param name="sender">The Finish button.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void CanNextPage(object sender, CanExecuteRoutedEventArgs eventArgs)
		{

			eventArgs.CanExecute = !this.CanFinish && !this.userName.IsVisible || !String.IsNullOrEmpty(this.emailAddress.Text);

		}

		/// <summary>
		/// Determine whether we can go back a screen.
		/// </summary>
		/// <param name="sender">The previous button.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void CanPreviousPage(object sender, CanExecuteRoutedEventArgs eventArgs)
		{

			eventArgs.CanExecute = this.tabs.SelectedIndex != 0;

		}

		/// <summary>
		/// Create the user with the information we've collected.
		/// </summary>
		/// <param name="email">The identity or login name of the user.</param>
		/// <param name="name">The user's real name name.</param>
		/// <param name="description">A description of the user.</param>
		/// <param name="groupId">The user's initial group.</param>
		/// <param name="organization">The organization the user belongs to.</param>		
		private void Create(string email, string name, string description, Guid groupId, Guid organization)
		{

			try
			{

				AdminSupportClient client = new AdminSupportClient(Guardian.Properties.Settings.Default.AdminSupportEndpoint);
				AdminSupportReference.User record = new AdminSupportReference.User();				

				record.FullName = name;
				record.EmailAddress = email;
				record.Description = description;
				record.LookupId = email;
				record.Organization = organization;
				record.GroupId = groupId;

				MethodResponseguid response = client.CreateUser(record, null);

				client.Close();

				if (!response.IsSuccessful)
					this.Dispatcher.BeginInvoke(new Action(() =>
						MessageBox.Show(this, String.Format(Properties.Resources.CreateUserFailed, name), this.Title)));

			}
			catch (Exception exception)
			{

				// Any issues trying to communicate to the server are logged.
				EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);
				this.Dispatcher.BeginInvoke(new Action(() =>
					MessageBox.Show(this, String.Format(Properties.Resources.CreateUserFailed, name), this.Title)));

			}

		}

		/// <summary>
		/// Dispose of the tenant list.
		/// </summary>
		private void Dispose()
		{

			if (this.tenant.ItemsSource != null)
				(this.tenant.ItemsSource as TenantList).Dispose();

		}
		
		/// <summary>
		/// Handle the Cancel command.
		/// </summary>
		/// <param name="sender">The Cancel button.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnCancel(object sender, RoutedEventArgs eventArgs)
		{

			this.Close();

		}

		/// <summary>
		/// Handle the NextPage command.
		/// </summary>
		/// <param name="sender">The Next button.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnNextPage(object sender, RoutedEventArgs eventArgs)
		{

			this.tabs.SelectedIndex += 1;
			this.SetValue(WindowAddUser.CanFinishProperty, this.tabs.SelectedIndex == this.tabs.Items.Count - 1);
			eventArgs.Handled = true;
			this.finishButton.Visibility = this.CanFinish ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

		}

		/// <summary>
		/// Handle the Okay command.
		/// </summary>
		/// <param name="sender">The Finish button.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnOkay(object sender, RoutedEventArgs eventArgs)
		{

			string name = this.userName.Text;
			string email = this.emailAddress.Text;
			string description = this.description.Text;
			Guid organization = this.Tenant;
			Guid group = this.group.Group.GroupId;

			FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(data => this.Create(email, name, description, group, organization));

			this.Close();
			eventArgs.Handled = true;

		}

		/// <summary>
		/// Handle the PreviousPage command.
		/// </summary>
		/// <param name="sender">The Back button.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnPreviousPage(object sender, RoutedEventArgs eventArgs)
		{

			this.tabs.SelectedIndex -= 1;
			eventArgs.Handled = true;
			this.SetValue(WindowAddUser.CanFinishProperty, this.tabs.SelectedIndex == this.tabs.Items.Count - 1);
			this.finishButton.Visibility = this.CanFinish ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

		}

		/// <summary>
		/// Handle a change in selected tenant.
		/// </summary>
		/// <param name="sender">The wizard.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private static void OnTenantChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			WindowAddUser wizard = sender as WindowAddUser;
			
		}

	}

}
