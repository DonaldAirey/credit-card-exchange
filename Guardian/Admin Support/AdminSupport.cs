namespace FluidTrade.Guardian
{
	using System;
	using System.Collections.Generic;
	using System.ServiceModel;
	using System.Threading;
	using System.Transactions;
	using FluidTrade.Core;
	using FluidTrade.Guardian.Records;

	/// <summary>
	/// Admin support Web service interface implementation
	/// </summary>
	public class AdminSupport : IAdminSupport	
	{

		/// <summary>
		/// AddUserToGroup
		/// </summary>
		/// <param name="lookupID"></param>
		/// <param name="groupId"></param>
		/// <param name="tenantId"></param>
		/// <returns></returns>
		[OperationBehaviorAttribute(TransactionScopeRequired = true)]
		public MethodResponse<ErrorCode> AddUserToGroup(string lookupID, Guid groupId, Guid tenantId)
		{
			MethodResponse<ErrorCode> response = new MethodResponse<ErrorCode>();
			try
			{
				UserPersistence.AddUserToGroup(lookupID, groupId, tenantId);
				response.Result = ErrorCode.Success;
			}
			catch (Exception ex)
			{
				EventLog.Error(ex);
				response.AddError(ex, 0);
				response.Result = ErrorCode.NoJoy;
			}

			return response;
			
		}
		
		/// <summary>
		/// Add Organization
		/// </summary>
		/// <param name="organization"></param>
		/// <param name="parent"></param>
		/// <returns></returns>
		public MethodResponse<Guid> AddOrganization(string organization, string parent)
		{
			MethodResponse<Guid> response = new MethodResponse<Guid>();

			using (TransactionScope transactionScope = new TransactionScope())
			{
				try
				{
					response.Result = UserPersistence.AddOrganization(organization, parent);
					transactionScope.Complete();
				}
				catch (Exception ex)
				{
					EventLog.Error(ex);
					response.AddError(ex, 0);
				}
			}
			return response;	
		}

		/// <summary>
		/// Change current user's password
		/// </summary>
		/// <param name="currentPassword"></param>		
		/// <param name="newPassword"></param>
		/// <returns></returns>
		public MethodResponse<ErrorCode> ChangePassword(string currentPassword, string newPassword)
		{
			MethodResponse<ErrorCode> response = new MethodResponse<ErrorCode>();
			try
			{
				UserPersistence.ChangePassword(currentPassword, newPassword);
				response.Result = ErrorCode.Success;
			}
			catch (Exception ex)
			{
				EventLog.Error(ex);
				response.AddError(ex, 0);
				response.Result = ErrorCode.NoJoy;
			}

			return response;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="users"></param>
		/// <returns></returns>		
		public MethodResponse<Guid[]> CreateUsers(User[] users)
		{
			List<Guid> guids = new List<Guid>();
			MethodResponse<Guid[]> returnCodes = new MethodResponse<Guid[]>();
			foreach (var user in users)
			{
				try
				{
					using (TransactionScope transactionScope = new TransactionScope())
					{						
						guids.Add(UserPersistence.AddUser(user, null));
						transactionScope.Complete();
					}

				}
				catch (Exception exception)
				{
					EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);
					returnCodes.AddError(exception, 0);
					guids.Add(Guid.Empty);
				}
			}
			returnCodes.Result = guids.ToArray();
			return returnCodes;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="user"></param>
		/// <param name="password"></param>
		/// <returns></returns>		
		public MethodResponse<Guid> CreateUser(User user, string password)
		{
			MethodResponse<Guid> response = new MethodResponse<Guid>();
			
			using (TransactionScope transactionScope = new TransactionScope())
			{									
				try
				{
					response.Result = UserPersistence.AddUser(user, password);
					transactionScope.Complete();
				}
				catch(Exception ex)
				{
					EventLog.Error(ex);
					response.AddError(ex, 0);
					response.Result = Guid.Empty;				
				}
			}
			
			return response;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="lookupID"></param>
		/// <returns></returns>
		public MethodResponse<ErrorCode> DeleteUserAccount(string lookupID)
		{

			MethodResponse<ErrorCode> response = new MethodResponse<ErrorCode>();
			using (TransactionScope transactionScope = new TransactionScope())
			{
				try
				{
					UserPersistence.DeleteUserAccount(lookupID);
					response.Result = ErrorCode.Success;
					transactionScope.Complete();
				}
				catch (Exception ex)
				{
					EventLog.Error(ex);
					response.AddError(ex, 0);
					response.Result = ErrorCode.NoJoy;
				}
			}

			return response;
		}

			
		/// <summary>
		/// 
		/// </summary>
		/// <param name="lookupID"></param>
		/// <returns></returns>
		[OperationBehaviorAttribute(TransactionScopeRequired = true)]
		public MethodResponse<ErrorCode> DisableUserAccount(string lookupID)
		{
			MethodResponse<ErrorCode> response = new MethodResponse<ErrorCode>();
			try
			{
				UserPersistence.DisableUserAccount(lookupID);
				response.Result = ErrorCode.Success;
			}
			catch (Exception ex)
			{
				EventLog.Error(ex);
				response.AddError(ex, 0);
				response.Result = ErrorCode.NoJoy;
			}


			return response;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="lookupId"></param>
		/// <returns></returns>
		[OperationBehaviorAttribute(TransactionScopeRequired = true)]
		public MethodResponse<UserContextData> FindUserByName(string lookupId)
		{
			MethodResponse<UserContextData> response = new MethodResponse<UserContextData>();
			try
			{
				response = UserContextData.GetUserContext(lookupId);				
			}
			catch (Exception ex)
			{
				EventLog.Error(ex);
				response.AddError(ex, 0);				
			}


			return response;
			
		}

		/// <summary>
		/// Get all users in an orgaznization
		/// </summary>
		/// <returns></returns>
		public MethodResponse<UserContextData[]> GetAllUsers()
		{
			throw new NotImplementedException();
		}
		
		/// <summary>
		/// Return current user's context data.
		/// </summary>
		/// <returns></returns>
		[OperationBehaviorAttribute(TransactionScopeRequired = true)]
		public MethodResponse<UserContextData> GetUserContext()
		{
			IClaimsPrincipal iClaimsPrincipal = Thread.CurrentPrincipal as IClaimsPrincipal;
			return UserContextData.GetUserContext(iClaimsPrincipal.Identity.Name);
		}

		/// <summary>
		/// Get tenants that current user has access to.
		/// </summary>
		/// <returns></returns>		
		[OperationBehaviorAttribute(TransactionScopeRequired = true)]
		public Organization[] GetDebtOrganizations(string lookupId)
		{
			return UserPersistence.GetDebtOrganizations(lookupId);
		}

		/// <summary>
		/// Change the password
		/// </summary>
		/// <param name="lookupID"></param>
		/// <returns></returns>
		[OperationBehaviorAttribute(TransactionScopeRequired = true)]
		public MethodResponse<ErrorCode> MustChangePasswordOnNextLogin(string lookupID)
		{

			MethodResponse<ErrorCode> response = new MethodResponse<ErrorCode>();
			try
			{
				UserPersistence.MustChangePasswordOnNextLogin(lookupID);
				response.Result = ErrorCode.Success;
			}
			catch (Exception ex)
			{
				EventLog.Error(ex);
				response.AddError(ex, 0);
				response.Result = ErrorCode.NoJoy;
			}


			return response;
		}

		/// <summary>
		/// Remove the User from the Group
		/// </summary>
		/// <param name="lookupID"></param>
		/// <param name="groupId"></param>
		/// <returns></returns>
		[OperationBehaviorAttribute(TransactionScopeRequired = true)]
		public MethodResponse<ErrorCode> RemoveUserFromGroup(string lookupID, Guid groupId)
		{
			MethodResponse<ErrorCode> response = new MethodResponse<ErrorCode>();
			try
			{
				UserPersistence.RemoveUserFromGroup(lookupID, groupId);
				response.Result = ErrorCode.Success;
			}
			catch (Exception ex)
			{
				EventLog.Error(ex);
				response.AddError(ex, 0);
				response.Result = ErrorCode.NoJoy;
			}

			return response;
			
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="lookupID"></param>
		/// <param name="passPhrase"></param>		
		/// <returns></returns>
		[OperationBehaviorAttribute(TransactionScopeRequired = true)]
		public MethodResponse<ErrorCode> ResetPassword(string lookupID, string passPhrase)
		{
			MethodResponse<ErrorCode> response = new MethodResponse<ErrorCode>();
			try
			{
				UserPersistence.ResetPassword(lookupID, passPhrase);
				response.Result = ErrorCode.Success;
			}
			catch(Exception ex)
			{
				EventLog.Error(ex);
				response.AddError(ex, 0);
				response.Result = ErrorCode.NoJoy;				
			}

			
			return response;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="users"></param>
		/// <returns></returns>
		[OperationBehaviorAttribute(TransactionScopeRequired = true)]
		public MethodResponse<ErrorCode> UpdateUser(User[] users)
		{
			MethodResponse<ErrorCode> returnCodes = new MethodResponse<ErrorCode>();
			foreach (var user in users)
			{
				try
				{
					using (TransactionScope transactionScope = new TransactionScope())
					{
						UserPersistence.UpdateUser(user);
						transactionScope.Complete();
					}

				}
				catch (Exception exception)
				{
					EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);
					returnCodes.AddError(exception, 0);
				}
			}
			returnCodes.Result = (returnCodes.HasErrors()) ? ErrorCode.NoJoy : ErrorCode.Success;
			return returnCodes;
		}
		
	}
}


