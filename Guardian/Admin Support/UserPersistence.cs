namespace FluidTrade.Guardian
{
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.DirectoryServices;
	using System.Linq;
	using System.Reflection;
	using System.ServiceModel;
	using FluidTrade.Core;
	using FluidTrade.Guardian.Records;


	/// <remarks>
	/// Class for user provisioning.
	/// </remarks>
	public class UserPersistence
	{

		/// <summary>
		/// Constructor
		/// </summary>
		public UserPersistence()
		{
		}

		/// <summary>
		/// Add default permissions to an object.
		/// </summary>
		/// <param name="dataModel">The data model.</param>
		/// <param name="transaction">The current transaction.</param>
		/// <param name="includeUsers">Whether to give users access as well.</param>
		/// <param name="rootOrganizationId">The TenentId of the organization where the scan should start (eg. where the entity is).</param>
		/// <param name="entityId">The entityId of the object to modify.</param>
		/// <param name="tenantId">TenantId of accessControl</param>
		public static void AddGroupPermissions(DataModel dataModel, DataModelTransaction transaction, Boolean includeUsers, Guid rootOrganizationId, Guid entityId, Guid tenantId)
		{

			TenantRow tenantRow = DataModel.Tenant.TenantKey.Find(rootOrganizationId);
			RightsHolderRow[] rightsHolders;
			TenantTreeRow[] tenantTreeRows;

			if (tenantRow == null)
				throw new FaultException<RecordNotFoundFault>(
					new RecordNotFoundFault("Organization", new object[] { rootOrganizationId }),
					"The organization has been deleted.");

			tenantRow.AcquireReaderLock(transaction.TransactionId, DataModel.LockTimeout);
			try
			{
				tenantTreeRows = tenantRow.GetTenantTreeRowsByFK_Tenant_TenantTree_ChildId();
				rightsHolders = tenantRow.GetRightsHolderRows();
			}
			finally
			{
				tenantRow.ReleaseLock(transaction.TransactionId);
			}


			foreach (RightsHolderRow rightsHolderRow in rightsHolders)
			{

				GroupRow[] groupRows;

				rightsHolderRow.AcquireReaderLock(transaction.TransactionId, DataModel.LockTimeout);
				try
				{
					groupRows = rightsHolderRow.GetGroupRows();
				}
				finally
				{
					rightsHolderRow.ReleaseLock(transaction.TransactionId);
				}

				if (groupRows.Length > 0)
				{

					Guid groupId;
					GroupRow group = groupRows[0];
					GroupTypeRow groupType;

					group.AcquireReaderLock(transaction.TransactionId, DataModel.LockTimeout);
					try
					{
						groupId = group.GroupId;
						groupType = group.GroupTypeRow;
					}
					finally
					{
						group.ReleaseReaderLock(transaction.TransactionId);
					}

					groupType.AcquireReaderLock(transaction);
					if (groupType.GroupTypeCode == GroupType.ExchangeAdmin ||
						groupType.GroupTypeCode == GroupType.FluidTradeAdmin ||
						groupType.GroupTypeCode == GroupType.SiteAdmin)
						dataModel.CreateAccessControl(
							Guid.NewGuid(), 
							AccessRightMap.FromCode(AccessRight.FullControl), 
							entityId, 
							groupId,
							tenantId);
					else if (includeUsers && groupType.GroupTypeCode == GroupType.User)
						dataModel.CreateAccessControl(
							Guid.NewGuid(), 
							AccessRightMap.FromCode(AccessRight.Read), 
							entityId, 
							groupId,
							tenantId);

				}

			}

			foreach (TenantTreeRow tenantTreeRow in tenantTreeRows)
			{

				tenantTreeRow.AcquireReaderLock(transaction.TransactionId, DataModel.LockTimeout);
				try
				{
					rootOrganizationId = tenantTreeRow.ParentId;
				}
				finally
				{
					tenantTreeRow.ReleaseLock(transaction.TransactionId);
				}
				UserPersistence.AddGroupPermissions(dataModel, transaction, includeUsers, rootOrganizationId, entityId, tenantId);

			}

		}

		/// <summary>
		/// Adds organization to the hierachy
		/// </summary>
		/// <param name="organization"></param>
		/// <param name="parent"></param>
		internal static Guid AddOrganization(string organization, string parent)
		{
			//Hack check access			
			Guid organizationId = Guid.Empty;

			try
			{
				ConnectionStringSettings connectionStringSettings = ConfigurationManager.ConnectionStrings["ADConnectionString"];

				//Create the DB record first. This way if AD fails - rollback will remove this.			
				organizationId = CreateOrganization(organization, parent);

				//Only try to connect if valid connectionstring is specified in the config
				if (String.IsNullOrEmpty(connectionStringSettings.ConnectionString) == false)
				{
					AddOrganizationToAD(organization, parent);
				}

				return organizationId;

			}
			catch (Exception exception)
			{
				FluidTrade.Core.EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);
				throw exception;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="organization"></param>
		/// <param name="parent"></param>
		/// <returns></returns>
		private static Guid CreateOrganization(string organization, string parent)
		{
			DataModel dataModel = new DataModel();
			DataModelTransaction dataModelTransaction = DataModelTransaction.Current;
			DateTime createdTime = DateTime.UtcNow;
			Guid currentUserId, currentOrganizationId;
			Guid? parentId = null;

			TradingSupport.GetCurrentUserContext(out currentUserId, out currentOrganizationId);

			//Get parent Organization Id if appropriate.
			if (String.IsNullOrEmpty(parent) == false)
			{
				TenantRow organizationRow = DataModel.Tenant.TenantKeyExternalId0.Find(parent);
				if (organizationRow != null)
				{
					organizationRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
					try
					{
						parentId = organizationRow.TenantId;
					}
					finally
					{
						organizationRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
					}
				}

				//Sanity and permission checks
				if (parentId.HasValue)
				{

					UserRow userRow = DataModel.User.UserKey.Find(currentUserId);
					GroupUsersRow[] groups = null;
					Boolean isExchangeAdmin = false;

					userRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

					try
					{
						groups = userRow.GetGroupUsersRows();
					}
					finally
					{
						userRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
					}

					foreach (GroupUsersRow groupUsersRow in groups)
					{

						GroupRow groupRow;
						GroupTypeRow groupTypeRow;

						groupUsersRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

						try
						{
							groupRow = groupUsersRow.GroupRow;
						}
						finally
						{
							groupUsersRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
						}

						groupRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

						try
						{
							groupTypeRow = groupRow.GroupTypeRow;
						}
						finally
						{
							groupRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
						}

						groupTypeRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

						try
						{
							if (groupTypeRow.GroupTypeCode == GroupType.ExchangeAdmin || groupTypeRow.GroupTypeCode == GroupType.FluidTradeAdmin)
							{
								isExchangeAdmin = true;
								break;
							}
						}
						finally
						{
							groupTypeRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
						}

					}

					if (!isExchangeAdmin)
						throw new FaultException<FluidTrade.Core.SecurityFault>(new SecurityFault("You do not have proper access to the selected object."));

				}
				else
				{
					throw new FaultException<RecordNotFoundFault>(new RecordNotFoundFault("Organization", new object[] { parent }));
				}
			}
			else
			{
				//Permission checks
				if (!DataModelFilters.HasAccess(dataModelTransaction, currentUserId, currentOrganizationId, AccessRight.FullControl))
					throw new FaultException<FluidTrade.Core.SecurityFault>(new SecurityFault("You do not have proper access to the selected object."));
			}

			
			
			Guid organizationEntityId = Guid.NewGuid();
			GroupPersistence groupPersistence = new GroupPersistence();
			Group administrators = new Group
				{
					Description = "Administrators have complete and unrestricted access to the system",
					GroupType = GroupType.SiteAdmin,
					Name = "Administrators",
					TenantId = organizationEntityId,
				};
			Group users = new Group
				{
					Description = "Users are prevented from making accidental or intentional system-wide changes",
					GroupType = GroupType.User,
					Name = "Users",
					TenantId = organizationEntityId,
				};

			//Create the organization
			dataModel.CreateTenant(				
				organization,  //ExternalId
				null,          //ExternalId1
				false,
				organization,  //Name
				organizationEntityId);
			// Create the default groups

			if (parentId.HasValue)
			{
				dataModel.CreateTenantTree(
					organizationEntityId,
					null,
					parentId.Value,
					Guid.NewGuid());
			}

			groupPersistence.Create(administrators);
			groupPersistence.Create(users);

            // This may be important [HACK]
            // AccessControlHelper.GrantDefaultAccess(administrators.RowId);
			// AccessControlHelper.GrantDefaultAccess(users.RowId);

			return organizationEntityId;
		}

		/// <summary>
		/// Add Orgination to Active Directory
		/// </summary>
		/// <param name="organization"></param>
		/// <param name="parent"></param>
		private static void AddOrganizationToAD(string organization, string parent)
		{

			ConnectionStringSettings connectionStringSettings = ConfigurationManager.ConnectionStrings["ADConnectionString"];
			//HACK check access			
			string domainName = FluidTrade.Core.Properties.Settings.Default.ADDomain;

			// Binding object.
			DirectoryEntry adDirectory;
			// Binding path.
			string strPath = connectionStringSettings.ConnectionString + domainName;

			// Get AD LDS object.            
			adDirectory = new DirectoryEntry(strPath
				, null, null, AuthenticationTypes.Secure);
			adDirectory.RefreshCache();

			DirectoryEntry rootEntry = null;
			if (String.IsNullOrEmpty(parent) == false)
			{
				string parentOrganizationUnit = String.Format("OU={0}", parent);
				rootEntry = ActiveDiretoryHelper.FindOrganizationUnit(adDirectory, parentOrganizationUnit);
			}

			if (rootEntry != null)
				adDirectory = rootEntry;

			string organizationUnit = String.Format("OU={0}", organization);

			if (ActiveDiretoryHelper.FindOrganizationUnit(adDirectory, organizationUnit) == null)
				ActiveDiretoryHelper.AddOrganizationUnit(adDirectory, organizationUnit);

		}

		/// <summary>
		/// Add a user to a group
		/// </summary>
		/// <param name="lookupID"></param>
		/// <param name="groupId"></param>
		/// <param name="tenantId"></param>
		
		internal static void AddUserToGroup(string lookupID, Guid groupId, Guid tenantId)
		{
			DataModel dataModel = new DataModel();
			DataModelTransaction dataModelTransaction = DataModelTransaction.Current;
			Guid userId;

			UserRow userRow = DataModel.User.UserKeyIdentityName.Find(lookupID.ToLower());
			if (userRow == null)
			{
				if (userRow == null)
					throw new FaultException<RecordNotFoundFault>(
						new RecordNotFoundFault("User", new object[] { lookupID }));
			}
			userRow.AcquireReaderLock(dataModelTransaction);
			userId = userRow.UserId;

			if (!DataModelFilters.HasAccess(dataModelTransaction, TradingSupport.UserId, groupId, AccessRight.Write))
				throw new FaultException<SecurityFault>(new SecurityFault("You do not have proper access to the selected object."));
			if (DataModel.GroupUsers.GroupUsersKey.Find(groupId, userId) != null)
				throw new FaultException<RecordExistsFault>(new RecordExistsFault("GroupUsers", new object[] { groupId, userRow.UserId }));

			dataModel.CreateGroupUsers(
				null, 
				groupId, 
				tenantId,
				userId);
		}

		/// <summary>
		/// Reset password.  
		/// </summary>
		/// <param name="lookupId"></param>		
		internal static void AutoGeneratePassword(string lookupId)
		{
			DataModelTransaction dataModelTransaction = DataModelTransaction.Current;
			DirectoryEntry adUser = ActiveDiretoryHelper.FindUser(lookupId);
			UserRow userRow;
			String password;

			if (adUser == null)
			{
				throw new FaultException<RecordNotFoundFault>(new RecordNotFoundFault("AdUser", new object[] { lookupId }));
			}

			password = CryptoHelper.GenerateStrongPassword(10);

			userRow = DataModel.User.UserKeyIdentityName.Find(lookupId.ToLower());
			if (userRow == null)
			{
				throw new FaultException<RecordNotFoundFault>(new RecordNotFoundFault("User", new object[] { lookupId }));
			}
			userRow.AcquireReaderLock(dataModelTransaction);

			if (!DataModelFilters.HasAccess(dataModelTransaction, TradingSupport.UserId, userRow.UserId, AccessRight.Write))
				throw new FaultException<FluidTrade.Core.SecurityFault>(new SecurityFault("You do not have proper access to the selected user."));
	
			ResetPassword(adUser, password);

		}


		/// <summary>
		/// Adds an user to the AD and Database
		/// </summary>
		/// <param name="userRecord"></param>
		/// <param name="password"></param>
		internal static Guid AddUser(User userRecord, string password)
		{

			 DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

			// The current user's identity (most likely an admin or local admin) is required to grant access to this object.
			Guid currentUserId = TradingSupport.UserId;
			Guid createdUserId = Guid.Empty;

			// We need write access to an "organization" in order to add a user to it.
			if (!DataModelFilters.HasAccess(dataModelTransaction, currentUserId, userRecord.GroupId, AccessRight.Write))
				throw new FaultException<FluidTrade.Core.SecurityFault>(new SecurityFault("You do not have proper access to the selected object."));

			try
			{
				TenantRow tenantRow = DataModel.Tenant.TenantKey.Find(userRecord.Organization);
				ConnectionStringSettings connectionStringSettings = ConfigurationManager.ConnectionStrings["ADConnectionString"];

				if (tenantRow == null)
					throw new FaultException<RecordNotFoundFault>(new RecordNotFoundFault("Tenant", new object[] { userRecord.Organization }));

				tenantRow.AcquireReaderLock(dataModelTransaction);
				userRecord.LookupId = userRecord.LookupId;

				//Create the DB record first. This way if AD fails - rollback will remove this.			
				createdUserId = Create(userRecord);

				//Only try to connect if valid connectionstring is specified in the config
				if (String.IsNullOrEmpty(connectionStringSettings.ConnectionString) == false)
				{
					AddUserToAD(userRecord, password, dataModelTransaction);
				}
			}
			catch (Exception exception)
			{
				FluidTrade.Core.EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);
				throw exception;
			}

			return createdUserId;
		}

		/// <summary>
		/// Adds a user to the Active Directory
		/// </summary>
		/// <param name="userRecord"></param>
		/// <param name="password"></param>
		/// <param name="dataModelTransaction"></param>
		private static void AddUserToAD(User userRecord, string password, DataModelTransaction dataModelTransaction)
		{
			ConnectionStringSettings connectionStringSettings = ConfigurationManager.ConnectionStrings["ADConnectionString"];
			string domainName = FluidTrade.Core.Properties.Settings.Default.ADDomain;

			TenantRow tenantRow = DataModel.Tenant.TenantKey.Find(userRecord.Organization);
			if (tenantRow == null)
				throw new FaultException<RecordNotFoundFault>(new RecordNotFoundFault("Tenant", new object[] { userRecord.Organization }));

			tenantRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
			string organizationUnit = String.Empty;
			try
			{
				organizationUnit = String.Format("OU={0}", tenantRow.Name);
			}
			finally
			{
				tenantRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
			}

			// Binding object.
			DirectoryEntry adDirectory;
			// User object.
			DirectoryEntry adUser;
			// Display name of user.
			string strDisplayName = userRecord.FullName;
			// Binding path.
			string strPath = connectionStringSettings.ConnectionString + domainName;
			// User to create.				
			string strUser = String.Format("CN={0}", userRecord.LookupId);

			// Get AD LDS object.            
			adDirectory = new DirectoryEntry(strPath
				, null, null, AuthenticationTypes.Secure);
			adDirectory.RefreshCache();

			//Determine if OU exists
			DirectoryEntry organizationEntry = ActiveDiretoryHelper.FindOrganizationUnit(adDirectory, organizationUnit);

			if (organizationEntry == null)
				organizationEntry = ActiveDiretoryHelper.AddOrganizationUnit(adDirectory, organizationUnit);

			// Create User.
			try
			{
				adUser = organizationEntry.Children.Add(strUser, "user");
				if (String.IsNullOrEmpty(userRecord.Description) == false)
					adUser.Properties["description"].Add(userRecord.Description);
				adUser.Properties["displayName"].Add(strDisplayName);
				adUser.Properties["givenName"].Add(strDisplayName);
				if (String.IsNullOrEmpty(userRecord.EmailAddress) == false)
					adUser.Properties["mail"].Add(userRecord.EmailAddress);
				adUser.Properties["sn"].Add(strDisplayName);
				//This is what we authenticate against
				adUser.Properties["userPrincipalName"].Add(userRecord.LookupId);
				adUser.CommitChanges();

				if (String.IsNullOrEmpty(password) == false)
				{
					ResetPassword(userRecord.LookupId, password);
				}
			}
			catch (DirectoryServicesCOMException deCOMException)
			{
				if (deCOMException.ExtendedErrorMessage.Contains("ENTRY_EXISTS"))
				{
					EventLog.Warning(userRecord.LookupId + " already exists in AD");
				}
				else
					throw deCOMException;
			}
		}

		/// <summary>
		/// Change the user password
		/// </summary>		
		/// <param name="currentPassword"></param>
		/// <param name="newPassword"></param>
		internal static void ChangePassword(string currentPassword, string newPassword)
		{

			try
			{
				//Find the user
				DirectoryEntry adUser = ActiveDiretoryHelper.FindCurrentUser();
				if (adUser == null)
				{
					throw new FaultException<RecordNotFoundFault>(new RecordNotFoundFault("AdUSer", null));
				}

				//adUser.UsePropertyCache = true;
				adUser.Options.PasswordEncoding = PasswordEncodingMethod.PasswordEncodingClear;
				adUser.Invoke("ChangePassword", new object[] { currentPassword, newPassword });				
				adUser.CommitChanges();

			}
			catch (TargetInvocationException)
			{
				throw new FaultException<ArgumentFault>(new ArgumentFault(""),
					new FaultReason("Password does not meet the constraints specified in Active Directory"));
			}
			catch (Exception)
			{
				throw;
			}
		}

		/// <summary>
		/// Create a new DebtHolder
		/// </summary>
		/// <returns></returns>
		internal static Guid Create(User record)
		{
			DataModel dataModel = new DataModel();
			DateTime createdTime = DateTime.UtcNow;
			Guid userEntityId = Guid.Empty;
			Guid userFolderId = Guid.Empty;
			Guid currentUserId = TradingSupport.UserId;

			DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

			//Gather all the information we need to create the user.  We will use
			//explicit locks.
			EntityRow entityRow = DataModel.Entity.EntityKey.Find(currentUserId);
			entityRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
			EntityTreeRow entityTreeRow = null;
			TypeRow userTypeRow = null;
			try
			{
				userTypeRow = DataModel.Type.TypeKey.Find(entityRow.TypeId);
				userTypeRow.AcquireReaderLock(dataModelTransaction);

				entityTreeRow = entityRow.GetEntityTreeRowsByFK_Entity_EntityTree_ParentId().First();
			}
			finally
			{
				entityRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				//This is no longer needed.
				entityRow = null;
			}


			EntityRow userFolderEntityRow = null;
			entityTreeRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
			try
			{
				userFolderEntityRow = DataModel.Entity.EntityKey.Find(entityTreeRow.ChildId);
			}
			finally
			{
				entityTreeRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				entityTreeRow = null;
			}

			//Get the typeRow of folder
			userFolderEntityRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
			TypeRow userFolderRow = null;
			try
			{
				userFolderRow = DataModel.Type.TypeKey.Find(userFolderEntityRow.TypeId);
			}
			finally
			{
				userFolderEntityRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				userFolderEntityRow = null;
			}

			userFolderRow.AcquireReaderLock(dataModelTransaction);
			Guid userImageId = Guid.Empty;
			Guid folderImageId = Guid.Empty;

			if (userTypeRow.IsImageIdNull())
			{
				DataModel.Image[0].AcquireReaderLock(dataModelTransaction);
				userImageId = DataModel.Image[0].ImageId;
			}
			else
			{
				userImageId = userTypeRow.ImageId;
			}

			if (userFolderRow.IsImageIdNull())
			{
				DataModel.Image[0].AcquireReaderLock(dataModelTransaction);
				folderImageId = DataModel.Image[0].ImageId;
			}
			else
			{
				folderImageId = userFolderRow.ImageId;
			}

			// This provides a base object (an Entity) for all users added.
			userEntityId = Guid.NewGuid();
			dataModel.CreateEntity(
				createdTime,
				record.Description,
				userEntityId,
				record.LookupId,
				null,
				null,
				null,
				null,
				null,
				null,
				Guid.NewGuid().ToString(),
				userImageId,
				false,
				false,
				createdTime,
				record.FullName,
				record.Organization,
				userTypeRow.TypeId);

			dataModel.CreateRightsHolder(
				userEntityId,
				record.Organization);

			dataModel.CreateUser(
				record.LookupId.ToLower(),
				null,
				null,
				record.Organization,
				userEntityId);


			dataModel.CreateTrader(null,
				null,
				null,
				null,
				null,
				null,
				null,
				Guid.NewGuid(),
				null,
				null,
				record.EmailAddress,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				record.Organization,
				userEntityId);

			UserPersistence.AddGroupPermissions(dataModel, dataModelTransaction, true, record.Organization, userEntityId, record.Organization);

			//Create a folder
			userFolderId = Guid.NewGuid();
			dataModel.CreateEntity(
				createdTime,
				"Personal Folder of "+record.FullName,
				userFolderId,
				record.FullName + " Folder",
				null,
				null,
				null,
				null,
				null,
				null,
				Guid.NewGuid().ToString(),
				folderImageId,
				false,
				false,
				createdTime,
				record.FullName,
				record.Organization,
				userFolderRow.TypeId);

			//Create a new root folder for the user
			dataModel.CreateFolder(
				userFolderId,
				record.Organization);

			dataModel.CreateSystemFolder(
				userFolderId,
				record.Organization);

			UserPersistence.AddGroupPermissions(dataModel, dataModelTransaction, false, record.Organization, userFolderId, record.Organization);

			dataModel.CreateGroupUsers(
				null,
				record.GroupId, 
				record.Organization,
				userEntityId);

			// Supply the user rights for this new object.
			AccessRightRow readWriteRightRow = DataModel.AccessRight.AccessRightKeyAccessRightCode.Find(AccessRight.ReadWriteBrowse);

			dataModel.CreateEntityTree(userFolderId,
				Guid.NewGuid(),
				null,
				userEntityId);

			dataModel.CreateAccessControl(Guid.NewGuid(),
				AccessRightMap.FromCode(AccessRight.FullControl),
				userFolderId,
				userEntityId,
				record.Organization);

			dataModel.CreateAccessControl(Guid.NewGuid(),
				AccessRightMap.FromCode(AccessRight.ReadWriteBrowse),
				userEntityId,
				userEntityId,
				record.Organization);


			return userEntityId;

		}

		/// <summary>
		/// Deletes the user account
		/// </summary>
		/// <param name="lookupId"></param>
		internal static void DeleteUserAccount(string lookupId)
		{
			if (String.IsNullOrEmpty(lookupId))
				throw new FaultException<ArgumentFault>(new ArgumentFault("No LookupId specified"));

			UserRow userRow = DataModel.User.UserKeyIdentityName.Find(lookupId.ToLower());
			if (userRow == null)
			{
				throw new FaultException<RecordNotFoundFault>(new RecordNotFoundFault("User", new object[] { lookupId }));
			}

			//Delete from the server first so if the AD operation fails then this will be rolled back.
			DataModelTransaction dataModelTransaction = DataModelTransaction.Current;
			userRow.AcquireReaderLock(dataModelTransaction);

			if (!DataModelFilters.HasAccess(dataModelTransaction, TradingSupport.UserId, userRow.UserId, AccessRight.Write))
				throw new FaultException<FluidTrade.Core.SecurityFault>(new SecurityFault("You do not have proper access to the selected user."));

			DataModel datamodel = new DataModel();
			datamodel.UpdateUser(
				null,
				true,  //IsRemoved
				null,
				userRow.RowVersion,				
				null,
				null,
				new object[] { userRow.UserId });


			//Let any exceptions propogate up
			ActiveDiretoryHelper.DeleteUser(lookupId);

		}

		/// <summary>
		/// Disables an AD account.  We do not delete the user.
		/// </summary>
		/// <param name="lookupId"></param>
		internal static void DisableUserAccount(string lookupId)
		{			
			DirectoryEntry adUser = ActiveDiretoryHelper.FindUser(lookupId);
			DataModelTransaction dataModelTransaction = DataModelTransaction.Current;
			UserRow userRow;

			if (adUser == null)
			{
				throw new FaultException<RecordNotFoundFault>(new RecordNotFoundFault("AdUser", new object[] { lookupId }));
			}

			//Lookup id.  We store all the values in the DB as lowercase.
			userRow = DataModel.User.UserKeyIdentityName.Find(lookupId.ToLower());
			if (userRow == null)
			{
				throw new FaultException<RecordNotFoundFault>(new RecordNotFoundFault("User", new object[] { lookupId }));
			}
			userRow.AcquireReaderLock(dataModelTransaction);

			if (!DataModelFilters.HasAccess(dataModelTransaction, TradingSupport.UserId, userRow.UserId, AccessRight.Write))
				throw new FaultException<FluidTrade.Core.SecurityFault>(new SecurityFault("You do not have proper access to the selected user."));

			using (adUser)
			{
				adUser.Properties["msDS-UserAccountDisabled"].Value = true;
				adUser.CommitChanges();
			}
		}


		/// <summary>
		/// Grab all the Debt organizations that a user has access to
		/// </summary>
		/// <param name="lookupId"></param>
		/// <returns></returns>
		public static Organization[] GetDebtOrganizations(string lookupId)
		{
			if (string.IsNullOrEmpty(lookupId))
				throw new FaultException<FluidTrade.Core.ArgumentFault>(new ArgumentFault(""),
				new FaultReason("No User Id specified."));

			DataModelTransaction dataModelTransaction = DataModelTransaction.Current;
			Guid currentUserId = TradingSupport.UserId;
			Guid rightsHolderTenantId, rightsHolderId;

			//Lookup id.  We store all the values in the DB as lowercase.
			UserRow userRow = DataModel.User.UserKeyIdentityName.Find(lookupId.ToLower());
			if (userRow == null)
			{
				throw new FaultException<RecordNotFoundFault>(new RecordNotFoundFault("User", new object[] { lookupId }));
			}
	
			userRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
			try
			{
				rightsHolderTenantId = userRow.TenantId;
				rightsHolderId = userRow.UserId;
			}
			finally
			{
				userRow.ReleaseLock(dataModelTransaction.TransactionId);
			}


			// Determine whether current user's tenant is upstream from rights holder we're trying to access.
			if (currentUserId != rightsHolderId)
			{
				if (!DataModelFilters.HasTenantAccess(dataModelTransaction, currentUserId, rightsHolderTenantId))
					throw new FaultException<SecurityFault>(
						new SecurityFault(String.Format("{0} does not control over tenant {1}", rightsHolderId, rightsHolderTenantId)));
			}

			List<Organization> organizationList = new List<Organization>();

			foreach (DebtClassRow dataRow in DataModel.DebtClass.Rows)
			{
				dataRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
				try
				{
					//Make sure the user has access to see the organization Info
					if (!DataModelFilters.HasAccess(dataModelTransaction, rightsHolderId, dataRow.DebtClassId, AccessRight.Browse))
						continue;
					
					//Determine the Tenant Information for this DebtClass.  Since there is no tenantId on the 
					//debtclass itself, we will use the Entity parent relationship to get it.
					EntityRow entityRow = DataModel.Entity.EntityKey.Find(dataRow.DebtClassId);
					if (entityRow == null)
						continue;
					
					TenantRow tenantRow = null;
					entityRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
					try
					{
						tenantRow = entityRow.TenantRow;
					}
					finally
					{
						entityRow.ReleaseLock(dataModelTransaction.TransactionId);
					}

					//Sanity checks
					if (tenantRow == null)
						continue;

					tenantRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
					try
					{
						Organization organization = new Organization();
						organization.ContactName = dataRow.ContactName;
						organization.OrganizationId = dataRow.DebtClassId;
						organization.Name = dataRow.CompanyName;
						organization.TenantId = tenantRow.TenantId;
						organization.TenantName = tenantRow.Name;
						organization.TenantIdExternalId0 = tenantRow.ExternalId0;
						organizationList.Add(organization);
					}
					finally
					{
						tenantRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
					}
				}
				finally
				{
					dataRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				}

				
			}

			return organizationList.ToArray();
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="lookupID"></param>		
		/// <returns></returns>
		private static Guid GetUserId(string lookupID)
		{
			DataModel dataModel = new DataModel();
			DataModelTransaction dataModelTransaction = DataModelTransaction.Current;
			UserRow userRow = DataModel.User.UserKeyIdentityName.Find(lookupID.ToLower());
			if (userRow == null)
			{
				if (userRow == null)
					throw new FaultException<RecordNotFoundFault>(
						new RecordNotFoundFault("User", new object[] { lookupID }));
			}
			userRow.AcquireReaderLock(dataModelTransaction);
			return userRow.UserId;
		}

		/// <summary>
		/// Must change password on next login
		/// </summary>
		/// <param name="lookupId"></param>
		internal static void MustChangePasswordOnNextLogin(string lookupId)
		{
			DataModelTransaction dataModelTransaction = DataModelTransaction.Current;
			DirectoryEntry adUser = ActiveDiretoryHelper.FindUser(lookupId);
			UserRow userRow;

			if (adUser == null)
			{
				throw new FaultException<RecordNotFoundFault>(new RecordNotFoundFault("AdUser", new object[] { lookupId }));
			}

			//Lookup id.  We store username values in the DB as lowercase.
			//Determine if the current user has access to modify target user's properties.
			userRow = DataModel.User.UserKeyIdentityName.Find(lookupId.ToLower());
			if (userRow == null)
			{
				throw new FaultException<RecordNotFoundFault>(new RecordNotFoundFault("User", new object[] { lookupId }));
			}
			userRow.AcquireReaderLock(dataModelTransaction);

			if (!DataModelFilters.HasAccess(dataModelTransaction, TradingSupport.UserId, userRow.UserId, AccessRight.Write))
				throw new FaultException<FluidTrade.Core.SecurityFault>(new SecurityFault("You do not have proper access to the selected user."));

			using (adUser)
			{
				adUser.Properties["pwdLastSet"].Value = DateTime.MinValue;
				adUser.CommitChanges();
			}

		}

		/// <summary>
		/// Remove user from a group
		/// </summary>
		/// <param name="lookupID"></param>
		/// <param name="groupId"></param>
		internal static void RemoveUserFromGroup(string lookupID, Guid groupId)
		{
			DataModel dataModel = new DataModel();
			//Implicit transaction 
			DataModelTransaction dataModelTransaction = DataModelTransaction.Current;
			UserRow userRow;
			Guid userId = GetUserId(lookupID);
			GroupUsersRow groupUserRow = DataModel.GroupUsers.GroupUsersKey.Find(groupId, userId);

			if (!DataModelFilters.HasAccess(dataModelTransaction, TradingSupport.UserId, groupId, AccessRight.Write))
				throw new FaultException<SecurityFault>(new SecurityFault("You do not have proper access to the selected object."));

			if (groupUserRow == null)
			{
				throw new FaultException<RecordNotFoundFault>(
						new RecordNotFoundFault("GroupUsers", new object[] { groupId, userId }));
			}

			//Lookup using lowercase value.
			userRow = DataModel.User.UserKeyIdentityName.Find(lookupID.ToLower());
			if (userRow == null)
			{
				throw new FaultException<RecordNotFoundFault>(new RecordNotFoundFault("User", new object[] { lookupID }));
			}
			userRow.AcquireReaderLock(dataModelTransaction);

			//We should probably just remove the group.
			if (userRow.GetGroupUsersRows().Length == 1)
				throw new FaultException<SecurityFault>(new SecurityFault("No user can be in fewer than one group"));
			
			groupUserRow.AcquireWriterLock(dataModelTransaction);
			dataModel.DestroyGroupUsers(new object[] { groupUserRow.GroupId, groupUserRow.UserId }, groupUserRow.RowVersion);
		}


		/// <summary>
		/// Reset the password
		/// </summary>
		/// <param name="lookupId"></param>
		/// <param name="password"></param>		
		public static void ResetPassword(string lookupId, string password)
		{
			
			UserRow userRow;
			
			if(string.IsNullOrEmpty(lookupId))
				throw new FaultException<FluidTrade.Core.ArgumentFault>(new ArgumentFault(""),
				new FaultReason("No User Id specified."));


			DirectoryEntry adUser = ActiveDiretoryHelper.FindUser(lookupId);
			DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

			if (adUser == null)
			{
				throw new FaultException<RecordNotFoundFault>(new RecordNotFoundFault("AdUser", new object[] { lookupId }));
			}

			//Use lowercase to lookup the user.  Determine if the current user has accessrights to change the target
			// user's attributes.
			userRow = DataModel.User.UserKeyIdentityName.Find(lookupId.ToLower());
			if (userRow == null)
			{
				throw new FaultException<RecordNotFoundFault>(new RecordNotFoundFault("User", new object[] { lookupId }));
			}
			userRow.AcquireReaderLock(dataModelTransaction);

			//Only users who have access rights over to change the password are allowed to continue.
			//Also allow the current user to change their own passwords.
			if (TradingSupport.UserId != userRow.UserId && !DataModelFilters.HasAccess(dataModelTransaction, TradingSupport.UserId, userRow.UserId, AccessRight.Write))
				throw new FaultException<FluidTrade.Core.SecurityFault>(new SecurityFault("Access Denied"),
				new FaultReason("You do not have proper access to the selected user."));


			ResetPassword(adUser, password);
		}


		/// <summary>
		/// Reset AD password.  Should be only called by an authenticated user.
		/// </summary>
		/// <param name="adUser"></param>
		/// <param name="password"></param>
		private static void ResetPassword(DirectoryEntry adUser, string password)
		{
			try
			{
				adUser.Options.PasswordEncoding = PasswordEncodingMethod.PasswordEncodingClear;
				adUser.Invoke("SetPassword", new object[] { password });
				adUser.Properties["msds-useraccountdisabled"].Value = false;
				adUser.CommitChanges();

			}
			catch (TargetInvocationException)
			{
				throw new FaultException<ArgumentFault>( new ArgumentFault(""),
					new FaultReason("Password does not meet the constraints specified in Active Directory"));
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message);
			}
		}



		/// <summary>
		/// Upate User attributes.
		/// </summary>
		/// <param name="userRecord"></param>
		internal static void UpdateUser(User userRecord)
		{

			Guid userId;
			Int64 userRowVersion;

			//Sanity checks
			if (String.IsNullOrEmpty(userRecord.LookupId))
				throw new FaultException<ArgumentFault>(new ArgumentFault("No LookupId specified"));

			DirectoryEntry adUser = ActiveDiretoryHelper.FindUser(userRecord.LookupId);
			if (adUser == null)
			{
				throw new FaultException<RecordNotFoundFault>(new RecordNotFoundFault("Active Directory User", new object[] { userRecord.LookupId }));
			}

			//Use lowercase to lookup user
			UserRow userRow = DataModel.User.UserKeyIdentityName.Find(userRecord.LookupId.ToLower());
			if (userRow == null)
			{
				throw new FaultException<RecordNotFoundFault>(new RecordNotFoundFault("User", new object[] { userRecord.LookupId }));
			}

			//Use implicit transaction/
			DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

			userRow.AcquireReaderLock(dataModelTransaction);
			userId = userRow.UserId;
			userRowVersion = userRow.RowVersion;

			if (!DataModelFilters.HasAccess(dataModelTransaction, TradingSupport.UserId, userRow.UserId, AccessRight.Write))
				throw new FaultException<FluidTrade.Core.SecurityFault>(new SecurityFault("You do not have proper access to the selected object."));
			
			DataModel datamodel = new DataModel();
			datamodel.UpdateUser(
				String.IsNullOrEmpty(userRecord.EmailAddress) ? null : userRecord.EmailAddress.ToLower(),
				null,
				null,
				userRowVersion,
				userRecord.Organization,
				null,
				new object[] { userId });


			String strDisplayName = userRecord.FullName;
			if (String.IsNullOrEmpty(userRecord.Description) == false)
				adUser.Properties["description"].Value = userRecord.Description;

			if (String.IsNullOrEmpty(strDisplayName) == false)
			{
				adUser.Properties["displayName"].Value = strDisplayName;
				adUser.Properties["givenName"].Value = strDisplayName;
				adUser.Properties["sn"].Value = strDisplayName;				
			}

			if (String.IsNullOrEmpty(userRecord.EmailAddress) == false)
			{				
				adUser.Properties["mail"].Value = userRecord.EmailAddress;
				adUser.Properties["userPrincipalName"].Value = userRecord.EmailAddress;

			}
	 
			adUser.CommitChanges();

			if (String.IsNullOrEmpty(userRecord.EmailAddress) == false)
			{
				if (String.Compare(userRecord.LookupId, userRecord.EmailAddress, true) != 0)
				{
					string strNewUser = String.Format("CN={0}", userRecord.EmailAddress);
					adUser.Rename(strNewUser);
				}
			}
		}
	}
}
