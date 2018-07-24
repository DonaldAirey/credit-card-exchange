namespace FluidTrade.Guardian
{

    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Windows;
    using System.Windows.Controls;

	/// <summary>
	/// Interaction logic for WindowUser.xaml
	/// </summary>
	public partial class WindowUser : Window
	{

		// Constants
		private const int retries = 5;
		private const string myStoreName = "My";

		// Private Static Fields
		private static System.Security.Cryptography.Oid oidClientAuthentication;

		static WindowUser()
		{


			// This OID is used to find on Client Authentication certificates to present to the user from the certificate store.
			WindowUser.oidClientAuthentication = new Oid("1.3.6.1.5.5.7.3.2", "Client Authentication");

		}

		public WindowUser()
		{

			InitializeComponent();

			List<X509Certificate2> clientCertificates = new List<X509Certificate2>();

			// This will select a list of valid certificates from the store that can be used for client authentication.
			X509Store store = new X509Store(WindowUser.myStoreName, StoreLocation.CurrentUser);
			store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
			X509Certificate2Collection validCertificates = store.Certificates.Find(X509FindType.FindByTimeValid,
				DateTime.Now, true);

			// Finally, the list is culled down to the ones containing only client authentication information.  This is a 
			// more difficult filter than the previous ones because there doesn't seem to be a convinient API for detecting
			// the certificate usage.
			foreach (X509Certificate2 x509Certificate in validCertificates)
				foreach (X509Extension x509Extension in x509Certificate.Extensions)
					if (x509Extension is X509EnhancedKeyUsageExtension)
					{
						X509EnhancedKeyUsageExtension x509EnhancedKeyUsageExtension =
							x509Extension as X509EnhancedKeyUsageExtension;
						foreach (System.Security.Cryptography.Oid oid in
							x509EnhancedKeyUsageExtension.EnhancedKeyUsages)
							if (oid.Value == WindowUser.oidClientAuthentication.Value)
								clientCertificates.Add(x509Certificate);
					}

			this.listViewCertificate.ItemsSource = clientCertificates;

			// Give the focus to the text box.
			this.textBoxName.Focus();

		}

		public string UserName
		{

			get
			{

				if (tabControl.SelectedItem is TabItem)
				{
					TabItem tabItem = tabControl.SelectedItem as TabItem;

					if (tabItem.Name == "UserNameTab")
						return this.textBoxName.Text;

					if (tabItem.Name == "CertificateTab")
					{

						if (this.listViewCertificate.SelectedItem is X509Certificate2)
						{
							X509Certificate2 x509Certificate2 = this.listViewCertificate.SelectedItem as X509Certificate2;
							return x509Certificate2.Subject + "; " + x509Certificate2.Thumbprint;
						}

					}

				}

				return string.Empty;

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

	}

}
