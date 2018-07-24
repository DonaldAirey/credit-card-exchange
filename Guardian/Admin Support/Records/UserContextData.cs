namespace FluidTrade.Guardian
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Diagnostics;
	using System.DirectoryServices;
	using System.Reflection;
	using System.Runtime.Serialization;
	using FluidTrade.Core;
	using FluidTrade.Guardian.Records;
	using System.DirectoryServices.ActiveDirectory;
	using System.ServiceModel;

	/// <summary>
	/// User context data associated with the current authenticated user
	/// </summary>
	[DataContract]
	public class UserContextData : User
	{
		/// <summary>
		/// 
		/// </summary>
		[DataMember]
		public bool AccountDisabled { get; set; }

		
		/// <summary>
		/// Available operations for the user.
		/// </summary>
		[DataMember]
		public List<String> AvailabeleOperations { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[DataMember]
		public DateTime PasswordExpires { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[DataMember]
		public bool IsPasswordExpired { get; set; }

		/// <summary>
		/// Version of the server running.
		/// </summary>
		[DataMember]
		public String ServerVersion { get; set; }


		/// <summary>
		/// GetUserContext
		/// </summary>
		/// <returns></returns>
		internal static MethodResponse<UserContextData> GetUserContext(string lookupId)
		{
			UserContextData userContextData = null;
			MethodResponse<UserContextData> response = new MethodResponse<UserContextData>();

			DataModel dataModel = new DataModel();
			DataModelTransaction dataModelTransaction = DataModelTransaction.Current;


			try
			{
				UserRow userRow = DataModel.User.UserKeyIdentityName.Find(lookupId.ToLower());
				if (userRow == null)
				{					
					response.AddError("Record Not Found", ErrorCode.RecordNotFound);
					return response;
				}

				userRow.AcquireReaderLock(dataModelTransaction);

				//Find AD entry
				DirectoryEntry adUser = ActiveDiretoryHelper.FindUser(userRow.IdentityName);
				if (adUser == null)
				{
					response.AddError("Record Not Found", ErrorCode.RecordNotFound);
					return response;
				}

				//Get the information from AD and the database record
				using (adUser)
				{					
					//Call refresh to update compiled fields.
					adUser.RefreshCache();

					userRow.AcquireReaderLock(dataModelTransaction);
					userContextData = new UserContextData();
					userContextData.UserId = userRow.UserId;
					userContextData.LookupId = userRow.IdentityName;
					try
					{
						userContextData.PasswordExpires = GetExpiration(adUser);
					}
					catch
					{
					}


					userContextData.AccountDisabled = (bool)adUser.Properties["msDS-USerAccountDisabled"].Value;
					if (adUser.Properties["description"].Value != null)
					{
						userContextData.Description = adUser.Properties["description"].Value.ToString();
					}
																	
					if (adUser.Properties["displayName"].Value != null)
					{
						userContextData.FullName = adUser.Properties["displayName"].Value.ToString();
					}

					if (adUser.Properties["mail"].Value != null)
					{
						userContextData.EmailAddress = adUser.Properties["mail"].Value.ToString();
					}

					userContextData.ServerVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion.ToString();					

					//This is a compiled field and can be null
					//string msDS = "msDS-User-Account-Control-Computed";
					if (adUser.Properties["msDS-UserPasswordExpired"].Value != null)
					{
						userContextData.IsPasswordExpired = (bool)adUser.Properties["msDS-UserPasswordExpired"].Value;
					}
					else
						userContextData.IsPasswordExpired = false;


				}

			}
			catch (Exception ex)
			{
				if (userContextData != null)
					userContextData = null;
				response.AddError(ex, 0);
			}

			response.Result = userContextData;
			return response;

		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		private static DateTime GetExpiration(DirectoryEntry adUser)
		{
			DirectoryEntry root = ActiveDiretoryHelper.GetCurrentDomain();
			
			using (root)
			{
				DomainPolicy policy = new DomainPolicy(root);

				//pwdLastSet is a COMObject  so we need to marshal it.
				long ticks = GetInt64(adUser, "pwdLastSet");

				if (ticks == 0)
					return DateTime.MinValue;

				//password has never been set
				if (ticks == -1)
				{
					throw new InvalidOperationException(
					  "User does not have a password"
					  );
				}

				//get when the user last set their password;
				DateTime pwdLastSet = DateTime.FromFileTime(ticks);

				//use our policy class to determine when
				//it will expire
				return pwdLastSet.Add(policy.MaxPasswordAge);
		
			}
		}

		private static Int64 GetInt64(DirectoryEntry entry, string attr)
		{
			//Use the marshaling behavior of the searcher			
			DirectorySearcher ds = new DirectorySearcher(
			  entry,
			  String.Format("({0}=*)", attr),
			  new string[] { attr },
			  SearchScope.Base
			  );

			SearchResult sr = ds.FindOne();

			if (sr != null)
			{
				if (sr.Properties.Contains(attr))
				{
					return (Int64)sr.Properties[attr][0];
				}
			}
			return -1;
		}
	}
}
