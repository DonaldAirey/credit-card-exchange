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

	/// <summary>
	/// Interaction logic for WindowLogin.xaml
	/// </summary>
	public partial class WindowLogin : Window
	{

		/// <summary>
		/// Create a new login window.
		/// </summary>
		public WindowLogin()
		{

			InitializeComponent();

		}

		/// <summary>
		/// The username that the user entered.
		/// </summary>
		public String UserName
		{

			get { return this.userName.Text; }

		}

		/// <summary>
		/// The password that the user entered.
		/// </summary>
		public String Password
		{

			get { return this.password.Password; }

		}

		/// <summary>
		/// Handle the Cancel command.
		/// </summary>
		/// <param name="sender">The Cancel button.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnCancel(object sender, EventArgs eventArgs)
		{

			this.Close();

		}

		/// <summary>
		/// Handle the Okay command.
		/// </summary>
		/// <param name="sender">The OK button.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnOkay(object sender, EventArgs eventArgs)
		{

			this.Close();

		}
	
	}

}
