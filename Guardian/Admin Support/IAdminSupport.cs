namespace FluidTrade.Guardian
{
	using System;
	using System.ServiceModel;
	using FluidTrade.Core;
	using FluidTrade.Guardian.Records;

	/// <summary>
	/// Admin interface
	/// </summary>
	[ServiceContractAttribute(ConfigurationName = "IAdminSupport")]
	public interface IAdminSupport
	{
		/// <summary>
		/// Add Organization
		/// </summary>
		/// <param name="organization"></param>
		/// <param name="parent"></param>
		/// <returns></returns>
		[OperationContract]
		[ServiceKnownType(typeof(Guid))]
		MethodResponse<Guid> AddOrganization(string organization, string parent);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="lookupID"></param>
		/// <param name="groupId"></param>
		/// <param name="tenantId"></param>
		/// <returns></returns>
		[OperationContract]
		[ServiceKnownType(typeof(ErrorCode))]
		MethodResponse<ErrorCode> AddUserToGroup(string lookupID, Guid groupId, Guid tenantId);

		/// <summary>
		/// Change the password
		/// </summary>
		/// <param name="currentPassword"></param>
		/// <param name="newPassword"></param>		
		/// <returns></returns>
		[OperationContract]
		[ServiceKnownType(typeof(ErrorCode))]
		MethodResponse<ErrorCode> ChangePassword(string currentPassword, string newPassword);



		/// <summary>
		/// Create one or more users
		/// </summary>
		/// <param name="user"></param>		
		/// <returns></returns>				
		[OperationContract]
		[TransactionFlowAttribute(TransactionFlowOption.Allowed)]
		MethodResponse<Guid[]> CreateUsers(User[] user);

		/// <summary>
		/// Create a user
		/// </summary>
		/// <param name="user"></param>		
		/// <param name="password"></param>		
		/// <returns></returns>				
		[OperationContract]
		[TransactionFlowAttribute(TransactionFlowOption.Allowed)]
		MethodResponse<Guid> CreateUser(User user, string password);

		/// <summary>
		/// Update user information
		/// </summary>
		/// <param name="lookupID"></param>
		/// <returns></returns>
		[OperationContract]
		[TransactionFlowAttribute(TransactionFlowOption.Allowed)]
		[ServiceKnownType(typeof(ErrorCode))]
		MethodResponse<ErrorCode> DeleteUserAccount(string lookupID);


		/// <summary>
		/// Update user information
		/// </summary>
		/// <param name="lookupID"></param>
		/// <returns></returns>
		[OperationContract]
		[TransactionFlowAttribute(TransactionFlowOption.Allowed)]
		[ServiceKnownType(typeof(ErrorCode))]
		MethodResponse<ErrorCode> DisableUserAccount(string lookupID);


		/// <summary>
		/// Get user context associated with current user.
		/// </summary>
		/// <returns></returns>
		[OperationContract]
		[TransactionFlowAttribute(TransactionFlowOption.Allowed)]
		MethodResponse<UserContextData> FindUserByName(string lookupId);


		/// <summary>
		/// Get tenants that current user has access to.
		/// </summary>
		/// <returns></returns>
		[OperationContract]
		[TransactionFlowAttribute(TransactionFlowOption.Allowed)]
		Organization[] GetDebtOrganizations(string lookupId);

		/// <summary>
		/// Get all users associated with organization
		/// </summary>
		/// <returns></returns>
		[OperationContract]
		[TransactionFlowAttribute(TransactionFlowOption.Allowed)]
		MethodResponse<UserContextData[]> GetAllUsers();


		/// <summary>
		/// Get user context associated with current user.
		/// </summary>
		/// <returns></returns>
		[OperationContract]
		[TransactionFlowAttribute(TransactionFlowOption.Allowed)]
		MethodResponse<UserContextData> GetUserContext();

		/// <summary>
		/// Change the password
		/// </summary>
		/// <param name="lookupID"></param>
		/// <returns></returns>
		[OperationContract]
		[ServiceKnownType(typeof(ErrorCode))]
		MethodResponse<ErrorCode> MustChangePasswordOnNextLogin(string lookupID);

		/// <summary>
		/// Remove user from group
		/// </summary>
		/// <param name="lookupID"></param>
		/// <param name="groupId"></param>
		/// <returns></returns>
		[OperationContract]
		[ServiceKnownType(typeof(ErrorCode))]
		MethodResponse<ErrorCode> RemoveUserFromGroup(string lookupID, Guid groupId);


		/// <summary>
		/// Resets user password and emails it if necessary. 
		/// </summary>
		/// <param name="lookupID"></param>
		/// <param name="password"></param>
		/// <returns></returns>
		[OperationContract]
		[ServiceKnownType(typeof(ErrorCode))]
		MethodResponse<ErrorCode> ResetPassword(string lookupID, string password);


		/// <summary>
		/// Update user information
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		[OperationContract]
		[ServiceKnownType(typeof(ErrorCode))]
		MethodResponse<ErrorCode> UpdateUser(User[] user);

	}
}
