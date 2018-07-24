namespace FluidTrade.Core
{

    using System.Security.Cryptography.X509Certificates;
	using System.Windows;

	/// <summary>
	/// Interaction logic for UserControlCertificate.xaml
	/// </summary>
	public partial class WindowCertificate : Window
	{

		// Private Instance Fields
		private System.Security.Cryptography.X509Certificates.X509Certificate2 x509Certificate2;

		public WindowCertificate()
		{
			InitializeComponent();
		}

		public X509Certificate2Collection X509Certificate2Collection
		{

			get { return this.listViewCertificate.ItemsSource as X509Certificate2Collection; }

			set
			{

				this.listViewCertificate.ItemsSource = value;
				this.listViewCertificate.SelectedItem = this.x509Certificate2;

			}

		}

		public X509Certificate2 X509Certificate2
		{

			get
			{
				return this.listViewCertificate.SelectedItem as X509Certificate2;
			}

			set
			{
				this.x509Certificate2 = value;
				this.listViewCertificate.SelectedItem = this.x509Certificate2;
			}

		}

		/// <summary>
		/// Handles the 'View Certificate' button.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The routed event arguments.</param>
		private void buttonViewCertificate_Click(object sender, RoutedEventArgs e)
		{

			// Display the selected certificate.
			X509Certificate2 selectedCertificate = this.listViewCertificate.SelectedItem as X509Certificate2;
			if (selectedCertificate != null)
				X509Certificate2UI.DisplayCertificate(selectedCertificate);

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
