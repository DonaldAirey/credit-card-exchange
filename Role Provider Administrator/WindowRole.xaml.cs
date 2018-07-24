namespace FluidTrade.Guardian
{

    using System.Windows;

    /// <summary>
	/// Interaction logic for WindowRole.xaml
	/// </summary>
	public partial class WindowRole : Window
	{

		public WindowRole()
		{

			InitializeComponent();

			// Give the focus to the text box.
			this.textBoxName.Focus();

		}

		public string RoleName
		{

			get { return this.textBoxName.Text; }

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
