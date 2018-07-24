namespace FluidTrade.Guardian.Windows
{

	using System;
	using System.Threading;
	using System.Windows;
	using FluidTrade.Core;
	using FluidTrade.Guardian.AdminSupportReference;
	using System.ServiceModel.Security;
	using System.ServiceModel;

	/// <summary>
	/// Interaction logic for WindowResetPassword.xaml
	/// </summary>
	public partial class WindowResetPassword : Window
	{

		/// <summary>
		/// Indicates the User dependency property.
		/// </summary>
		public static readonly DependencyProperty UserProperty = DependencyProperty.Register("User", typeof(User), typeof(WindowResetPassword), new PropertyMetadata(OnUserChanged));
		/// <summary>
		/// Indicates the IsCurrentUser dependency property.
		/// </summary>
		public static readonly DependencyProperty IsCurrentUserProperty = DependencyProperty.Register("IsCurrentUser", typeof(Boolean), typeof(WindowResetPassword));

		/// <summary>
		/// Create a new reset password prompt/window.
		/// </summary>
		public WindowResetPassword()
		{

			InitializeComponent();
	
		}

		/// <summary>
		/// True if the user whose password we're changing is the current user.
		/// </summary>
		public Boolean IsCurrentUser
		{

			get { return (Boolean)this.GetValue(WindowResetPassword.IsCurrentUserProperty); }
			set { this.SetValue(WindowResetPassword.IsCurrentUserProperty, value); }

		}

		/// <summary>
		/// The user whose password we're changing.
		/// </summary>
		public User User
		{

			get { return this.GetValue(WindowResetPassword.UserProperty) as User; }
			set { this.SetValue(WindowResetPassword.UserProperty, value); }

		}

	
		/// <summary>
		/// Handle the Cancel command.
		/// </summary>
		/// <param name="sender">The Cancel button.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnCancel(object sender, System.Windows.Input.ExecutedRoutedEventArgs eventArgs)
		{

			this.Close();

		}

		/// <summary>
		/// Handle the Okay command. Make sure the two passwords match and update the user.
		/// </summary>
		/// <param name="sender">The Okay button.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnOkay(object sender, System.Windows.Input.ExecutedRoutedEventArgs eventArgs)
		{

			string oldPassword = this.oldPassword.Password;
			string password = this.password.Password;
			string confirmation = this.confirm.Password;

			if (password.Equals(confirmation))
			{

				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(data => this.ResetPassword(data as User, oldPassword, password), this.User.Clone());

				this.Close();

			}
			else
			{

				MessageBox.Show(this, Properties.Resources.ResetPasswordFailedPasswordMismatch,
					this.Title, MessageBoxButton.OK, MessageBoxImage.Error);

			}

		}

		/// <summary>
		/// Set the value of IsCurrentUser when the user changes.
		/// </summary>
		/// <param name="sender">The password dialog.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private static void OnUserChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			WindowResetPassword window = sender as WindowResetPassword;

			window.IsCurrentUser = window.User.UserId == UserContext.Instance.UserId;

		}

		/// <summary>
		/// Set the user's password to the new password.
		/// </summary>
		/// <param name="user">The user to change.</param>
		/// <param name="oldPassword">The current password.</param>
		/// <param name="password">The new password.</param>
		private void ResetPassword(User user, string oldPassword, string password)
		{

			try
			{

				AdminSupportClient adminSupportClient = new AdminSupportClient(Guardian.Properties.Settings.Default.AdminSupportEndpoint);
				AdminSupportReference.User userRecord = new AdminSupportReference.User();
				MethodResponseErrorCode response = null;

				DataModel.IsReading = false;

				if (user.UserId == UserContext.Instance.UserId)
				{

					response = adminSupportClient.ChangePassword(oldPassword, password);

					if (response.IsSuccessful)
					{

						ChannelStatus.LoginEvent.Set();
						ChannelStatus.IsPrompted = false;
						ChannelStatus.Secret = password;
						ChannelStatus.LogggedInEvent.Set();

					}

				}
				else
				{

					response = adminSupportClient.ResetPassword(user.IdentityName, password);

				}

				if (!response.IsSuccessful)
					GuardianObject.ThrowErrorInfo(response.Errors[0]);

				adminSupportClient.Close();

			}
			catch (FaultException<ArgumentFault>)
			{
				
				this.Dispatcher.BeginInvoke(new Action(() =>
					MessageBox.Show(this, String.Format(Properties.Resources.ResetPasswordFailedPoorComplexity, user), this.Title)));

			}
			catch (SecurityAccessDeniedException)
			{

				this.Dispatcher.BeginInvoke(new Action(() =>
					MessageBox.Show(this, String.Format(Properties.Resources.UserNotFound, user), this.Title)));

			}
			catch (FaultException<RecordNotFoundFault>)
			{

				this.Dispatcher.BeginInvoke(new Action(() =>
					MessageBox.Show(this, String.Format(Properties.Resources.ResetPasswordFailedPermissionDenied, user), this.Title)));

			}
			catch (Exception exception)
			{

				// Any issues trying to communicate to the server are logged.
				EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);
				this.Dispatcher.BeginInvoke(new Action(() =>
					MessageBox.Show(this, String.Format(Properties.Resources.ResetPasswordFailed, user.Name), this.Title)));

			}
			finally
			{
				
				DataModel.IsReading = true;

			}

		}

	}

}
