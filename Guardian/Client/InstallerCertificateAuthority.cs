namespace FluidTrade.Guardian
{

	using System;
	using System.Collections;
    using System.ComponentModel;
	using System.Configuration.Install;
    using System.Security.Cryptography.X509Certificates;

	/// <summary>
	/// Installs the Certificate Authority for this application.
	/// </summary>
	[RunInstaller(true)]
	public partial class InstallerCertificateAuthority : Installer
	{

		/// <summary>
		/// Creates an object that installs a certificate authority for this application.
		/// </summary>
		public InstallerCertificateAuthority()
		{

			// The IDE supported components are initialized here.
			InitializeComponent();

		}

		/// <summary>
		/// When overriden in a derived class, installs the component.
		/// </summary>
		/// <param name="stateSaver"></param>
		public override void Install(IDictionary stateSaver)
		{

			// The Certificate Authority is installed only if it doesn't already exists.  Also note that there is no need to uninstall it.  If the owner of 
			// the target computer decides sometime in the future not to trust our CA, they can remove it manually.  For now, this certificate is on a one-way 
			// trip.
			String subjectName = this.Context.Parameters["subject"];
			if (subjectName != null)
			{
				X509Store store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
				if (store.Certificates.Find(X509FindType.FindBySubjectName, subjectName, false).Count == 0)
				{

					// This will install the certificate that tells this computer to trust any certificates issued by our authority.
					String certificatePath = this.Context.Parameters["certificate"];
					if (certificatePath != null)
					{
						store.Open(OpenFlags.MaxAllowed | OpenFlags.ReadWrite | OpenFlags.OpenExistingOnly);
						X509Certificate2 fluidTradeCA = new X509Certificate2(certificatePath);
						store.Add(fluidTradeCA);
						store.Close();
					}

				}
			}

			// Allow the base class to complete the installation.
			base.Install(stateSaver);

		}

		public override void Uninstall(IDictionary savedState)
		{
			base.Uninstall(savedState);
		}

	}

}
