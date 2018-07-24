namespace FluidTrade.Core
{

	using System;
	using System.Net;
	using System.ServiceModel;
	using System.Threading;
	using System.Windows;
	using System.Windows.Input;
	using System.Windows.Controls;
	using System.Windows.Controls.Primitives;
	using System.Diagnostics;
	using System.Runtime.InteropServices;
	using System.Windows.Interop;

	/// <summary>
	/// Interaction logic for UserControlCertificate.xaml
	/// </summary>
	public partial class WindowUserName : Window
	{


		/// <summary>
		/// Brings up Login About.
		/// </summary>		
		public static EventHandler LoginAbout;


		[DllImport("user32.dll")]
		private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

		[DllImport("user32.dll")]
		private static extern bool InsertMenu(IntPtr hMenu, Int32 wPosition, Int32 wFlags, Int32 wIDNewItem, string lpNewItem);
		/// Define our Constants we will use
		public const Int32 WM_SYSCOMMAND = 0x112;
		public const Int32 MF_SEPARATOR = 0x800;
		public const Int32 MF_BYPOSITION = 0x400;
		public const Int32 MF_STRING = 0x0;
		public const Int32 AboutSysMenuID = 1001;

		// Private Instance Fields
		private System.ServiceModel.EndpointAddress endpointAddress;

		

		public WindowUserName()
		{

			// If this window is running in the same thread apartment as the main application window, then it is run modally.  This
			// is not an option were the channels running in a console application.
			//if (Application.Current.Dispatcher.Thread == Thread.CurrentThread && Application.Current.MainWindow != null)
			//    this.Owner = Application.Current.MainWindow;

			// The designer maintaned components are initialized here.
			this.Initialized += this.OnLoaded;
			this.Loaded += new RoutedEventHandler(OnWindowUserNameLoaded);
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
				this.serverName.Text = this.endpointAddress.Uri.Host;

			}

		}

		/// <summary>
		/// This is the Win32 Interop Handle for this Window
		/// </summary>
		public IntPtr Handle
		{
			get
			{
				return new WindowInteropHelper(this).Handle;
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
				networkCredentials.UserName = String.IsNullOrEmpty(this.textBoxUserName.Text) == true ? Environment.UserName : this.textBoxUserName.Text;
				networkCredentials.Password = this.textBoxPassword.Password;
				return networkCredentials;

			}

			set
			{

				// This will set the elements of the window and provide defaults where appropriate.
				this.textBoxUserName.Text = value.UserName;
				this.textBoxPassword.Password = value.Password;

				// This will force the focus into the most natural user interface element.
				if (this.textBoxUserName.Text == string.Empty)
					this.textBoxUserName.Focus();
				else if (this.textBoxPassword.Password == string.Empty)
					this.textBoxPassword.Focus();

			}

		}

		/// <summary>
		/// Handle the loaded event.
		/// </summary>
		/// <param name="sender">This.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnLoaded(object sender, EventArgs eventArgs)
		{

			// If this window is running in the same thread apartment as the main application window, then it is run modally.  This
			// is not an option were the channels running in a console application.
			if (Application.Current.Dispatcher.Thread == Thread.CurrentThread && Application.Current.MainWindow != null && Application.Current.MainWindow.IsLoaded)
			{

				this.Owner = Application.Current.MainWindow;
				this.Owner.Activate();

			}
			else
			{

				this.Activate();

			}

			this.textBoxPassword.LostFocus += new RoutedEventHandler(textBoxPassword_LostFocus);
			this.textBoxPassword.GotFocus += new RoutedEventHandler(textBoxPassword_GotFocus);
			this.textBoxPassword.KeyDown += new KeyEventHandler(textBoxPassword_KeyDown);

		}


		/// <summary>
		/// OnLoaded event handler
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void OnWindowUserNameLoaded(object sender, RoutedEventArgs e)
		{
			// Get the Handle for the Forms System Menu
			IntPtr systemMenuHandle = GetSystemMenu(this.Handle, false);
			// Add a menu seperator			
			InsertMenu(systemMenuHandle, 5, MF_BYPOSITION | MF_SEPARATOR, 0, string.Empty);
			InsertMenu(systemMenuHandle, 6, MF_BYPOSITION, AboutSysMenuID, "About...");


			// Attach our WndProc handler to this Window
			HwndSource source = HwndSource.FromHwnd(this.Handle);
			source.AddHook(new HwndSourceHook(WndProc));
		}

		/// <summary>
		/// Poor man's CapsLock warning tooltip.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void textBoxPassword_LostFocus(object sender, RoutedEventArgs e)
		{
			// Hide the warning when we are no longer entering a password
			capsLockWarning.Visibility = Visibility.Hidden;
		}

		/// <summary>
		/// Check for CapsLock
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void textBoxPassword_GotFocus(object sender, RoutedEventArgs e)
		{
			//Display the warning if necessary when we start to enter a password
			CapsLockWarningCheck();
		}

		/// <summary>
		/// Check for CapsLock
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void textBoxPassword_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.CapsLock)
				CapsLockWarningCheck();
		}

		/// <summary>
		/// Check for CapsLock
		/// </summary>
		private void CapsLockWarningCheck()
		{
			if (((Int16)Keyboard.GetKeyStates(Key.CapsLock) & (Int16)KeyStates.Toggled) != 0)
			{
				capsLockWarning.Visibility = Visibility.Visible;
			}
			else
			{
				capsLockWarning.Visibility = Visibility.Hidden;
			}
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

		private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			// Check if a System Command has been executed
			if (msg == WM_SYSCOMMAND)
			{
				// Execute the appropriate code for the System Menu item that was clicked
				switch (wParam.ToInt32())
				{
					case AboutSysMenuID:

						if (LoginAbout != null)
						{
							LoginAbout(null, global::System.EventArgs.Empty);
						}

						handled = true;
						break;
				}
			}

			return IntPtr.Zero;
		}
	}

}
