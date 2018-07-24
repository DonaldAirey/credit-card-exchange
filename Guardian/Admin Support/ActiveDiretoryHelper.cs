namespace FluidTrade.Guardian
{

	using System;
	using System.Configuration;
	using System.DirectoryServices;
	using System.Runtime.InteropServices;
	using System.Threading;
	using FluidTrade.Core;


	/// <summary>
	/// ActiveDirectory helper class
	/// </summary>
	internal sealed class ActiveDiretoryHelper
	{

		/// <summary>
		/// Adds an OU.
		/// </summary>
		/// <param name="rootEntry"></param>
		/// <param name="organizationUnit"></param>
		/// <returns></returns>
		internal static DirectoryEntry AddOrganizationUnit(DirectoryEntry rootEntry, string organizationUnit)
		{
			DirectoryEntry organizationEntry = rootEntry.Children.Add(organizationUnit,
				"OrganizationalUnit");
			organizationEntry.CommitChanges();

			return organizationEntry;

		}

		/// <summary>
		/// Delete User from AD
		/// </summary>
		/// <param name="lookupId"></param>
		internal static void DeleteUser(string lookupId)
		{
			//Let the exceptions propogate up
			DirectoryEntry adUser = FindUser(lookupId);
			adUser.DeleteTree();
			adUser.CommitChanges();

		}


		/// <summary>
		/// Find the current user
		/// </summary>
		/// <returns></returns>
		internal static DirectoryEntry FindCurrentUser()
		{
			IClaimsPrincipal iClaimsPrincipal = Thread.CurrentPrincipal as IClaimsPrincipal;
			return FindUser(iClaimsPrincipal.Identity.Name);

		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="rootEntry"></param>
		/// <param name="organizationUnit"></param>
		/// <returns></returns>
		internal static DirectoryEntry FindOrganizationUnit(DirectoryEntry rootEntry, string organizationUnit)
		{
			DirectoryEntry organizationEntry = null;
			try
			{
				organizationEntry = rootEntry.Children.Find(organizationUnit);
			}
			catch (Exception)
			{
				return null;

			}
			return organizationEntry;
		}


		/// <summary>
		/// Find the user
		/// </summary>
		/// <param name="lookupId"></param>
		/// <returns></returns>
		internal static DirectoryEntry FindUser(string lookupId)
		{
			ConnectionStringSettings connectionStringSettings = ConfigurationManager.ConnectionStrings["ADConnectionString"];
			//Only try to connect if valid connectionstring is specified in the config
			if (String.IsNullOrEmpty(connectionStringSettings.ConnectionString))	
				return null;

			string domainName = FluidTrade.Core.Properties.Settings.Default.ADDomain;
			string strPath = connectionStringSettings.ConnectionString + domainName;

			AuthenticationTypes AuthTypes = AuthenticationTypes.Signing | AuthenticationTypes.Sealing | AuthenticationTypes.Secure;
			DirectoryEntry adDirectory = new DirectoryEntry(strPath,
				null, null, AuthTypes);

			DirectoryEntry adUser = null;
			DirectorySearcher dsSearcher = new DirectorySearcher(adDirectory);
			dsSearcher.SearchScope = SearchScope.Subtree;
			dsSearcher.Filter = String.Format("CN={0}", lookupId);
			dsSearcher.CacheResults = false;

			try
			{
				SearchResult srResult = dsSearcher.FindOne();

				if (srResult != null)
					adUser = srResult.GetDirectoryEntry();
				else
					return null;
			}
			catch (DirectoryServicesCOMException)
			{
				return null;
			}
			catch (COMException)
			{
				return null;
			}
			//Let the other exeptions propogate up

			return adUser;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		internal static DirectoryEntry GetCurrentDomain()
		{			
			string strPath = "LDAP://RootDSE";

			using (DirectoryEntry root = new DirectoryEntry(strPath, null, null, AuthenticationTypes.Secure))
			{
				root.RefreshCache();
				string adsPath = String.Format("LDAP://{0}", root.Properties["defaultNamingContext"][0]);
				DirectoryEntry adDirectory = new DirectoryEntry(adsPath, null, null, AuthenticationTypes.Secure);
				return adDirectory;
			}

		}
		
	}
}
