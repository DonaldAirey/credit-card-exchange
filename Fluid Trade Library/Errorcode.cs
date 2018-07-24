using System.Runtime.Serialization;

namespace FluidTrade.Core
{
	/// <summary>
	/// Status codes.
	/// </summary>
	[DataContract]
	public enum ErrorCode : int
	{
		/// <summary>
		/// Success - everything is good
		/// </summary>
		[EnumMember]
		Success = 0,

		/// <summary>
		/// No Joy
		/// </summary>
		[EnumMember]
		NoJoy = 1,

		/// <summary>
		/// Homage to the past.
		/// </summary>
		[EnumMember]
		GeneralProtectionFault = 3,

		/// <summary>
		/// Another, similar record already exist.
		/// </summary>
		[EnumMember]
		RecordExists = 4,

		/// <summary>
		/// The record to do the action on was not found.
		/// </summary>
		[EnumMember]
		RecordNotFound = 5,

		/// <summary>
		/// The operation was done two incompatible types.  Like moving one record to a different location.
		/// </summary>
		[EnumMember]
		IncompatibleTypes = 6,

		/// <summary>
		/// The operation caused a sql error.
		/// </summary>
		[EnumMember]
		SqlError = 7,

		/// <summary>
		/// The operation caused a sql deadlock.
		/// </summary>
		[EnumMember]
		Deadlock = 7,

		/// <summary>
		/// One of the arguments provided are incorrect.
		/// </summary>
		[EnumMember]
		ArgumentError = 8,

		/// <summary>
		/// A required field was missing.
		/// </summary>
		[EnumMember]
		FieldRequired = 9,

		//Admin errors
		///Active Directory error codes		
		/// <summary>
		/// Password Expired
		/// </summary>
		[EnumMember]
		[Description("Username/Password combination not recognized.")]
		BadPassword = 52,

		/// <summary>
		/// General Login failure. 
		/// </summary>
		[EnumMember]
		[Description("Access Denied.")]
		AccessDenied = 100,

		///Active Directory error codes		
		/// <summary>
		/// Password Expired
		/// </summary>
		[EnumMember]
		[Description("Password has expired.")]
		PasswordExpired = 532,

		/// <summary>
		/// Account Disabled
		/// </summary>
		[EnumMember]
		[Description("Account is disabled.")]
		AccountDisabled = 533,

		/// <summary>
		/// A major problem that probably requires a restart..
		/// All hell has broken loose this call has failed and so too will any subsequent calls.
		/// </summary>
		[EnumMember]
		[Description("A major problem that probably requires a restart.")]
		Fatal = 666,

		/// <summary>
		/// Account Expired
		/// </summary>
		[EnumMember]
		[Description("Account is expired.")]
		AccountExpired = 701,

		/// <summary>
		/// User must reset password
		/// </summary>
		[EnumMember]
		[Description("Password must be Reset.")]
		UserMustResetPassword = 773,

		/// <summary>
		/// User account locked
		/// </summary>
		[EnumMember]
		[Description("Account is locked.")]
		UserAccountLocked = 775,

		/// <summary>
		/// User not found
		/// </summary>
		[EnumMember]
		[Description("Username/Password combination not recognized.")]
		UserNotFound = 2030,
	}
}


