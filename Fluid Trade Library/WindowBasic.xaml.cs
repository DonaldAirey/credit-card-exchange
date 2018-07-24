namespace FluidTrade.Core
{

	using System;
    using System.Net;
	using System.ServiceModel;
	using System.Threading;
	using System.Windows;

	/// <summary>
	/// Interaction logic for UserControlCertificate.xaml
	/// </summary>
	public partial class WindowBasic : Window
	{

		// Private Instance Fields
		private System.ServiceModel.EndpointAddress endpointAddress;

		public WindowBasic()
		{

			// If this window is running in the same thread apartment as the main application window, then it is run modally.  This
			// is not an option were the channels running in a console application.
			if (Application.Current.Dispatcher.Thread == Thread.CurrentThread && Application.Current.MainWindow != null)
				this.Owner = Application.Current.MainWindow;

			// The designer maintaned components are initialized here.
			InitializeComponent();

		}

		/// <summary>
		/// Gets or sets the EndpointAddress for the window.
		/// </summary>
		public EndpointAddress EndpointAddress
		{

			get { return this.endpointAddress; }

			set
			{

				// The EndpointAddress is used to give the user feedback about the destination authentication authority for the
				// credentials.
				this.endpointAddress = value;
				this.Title = string.Format("Connect to {0}", this.endpointAddress.Uri.Host);
				this.textBlockGreeting.Text = string.Format("The server {0} requires a username and password.",
					this.endpointAddress.Uri.Host);

			}

		}

		/// <summary>
		/// Gets or sets the NetworkCredentials.
		/// </summary>
		public NetworkCredential NetworkCredentials
		{

			get
			{

				// This will extract the NetworkCredentials from the dialog box fields.
				NetworkCredential networkCredentials = new NetworkCredential();
				string[] parts = this.textBoxUserName.Text.Split('\\');
				networkCredentials.Domain = parts.Length == 1 ? Environment.UserDomainName : parts[0];
				networkCredentials.UserName = parts.Length == 1 ? parts[0] : parts[1];
				networkCredentials.Password = this.textBoxPassword.Password;
				return networkCredentials;

			}

			set
			{

				// This will set the elements of the window and provide defaults where appropriate.
				this.textBoxUserName.Text = string.Format("{0}\\{1}", value.Domain, value.UserName);
				this.textBoxPassword.Password = value.Password;

				// This will force the focus into the most natural user interface element.
				if (this.textBoxPassword.Password == string.Empty)
					this.textBoxPassword.Focus();
				else
					this.buttonOK.Focus();

			}

		}

		public bool AreCredentialsPersistent
		{

			get { return this.checkBoxRememberPassword.IsChecked == true; }
			set { this.checkBoxRememberPassword.IsChecked = value; }

		}

		/// <summary>
		/// Handles the 'Cancel' button.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The routed event arguments.</param>
		private void buttonCancel_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
		}

		/// <summary>
		/// Handles the 'OK' button.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The routed event arguments.</param>
		private void buttonOK_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
		}

	}

}
