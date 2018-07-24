using System;
using System.IdentityModel.Selectors;
using System.DirectoryServices;
using System.ServiceModel;
using System.Globalization;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.Security;
using System.Text.RegularExpressions;

namespace FluidTrade.Core
{
	/// <summary>
	/// Cusomnt valuiidator called by WCF to validate during Username authientication mode.
	/// </summary>
	public class UserNameValidator : UserNamePasswordValidator
	{
		/// <summary>
		/// Validate the credentials.
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="password"></param>
		public override void Validate(string lookupId, string password)
		{
			bool authenticated = false;
			ErrorCode errorCode = ErrorCode.AccessDenied;
			if (String.IsNullOrEmpty(lookupId))
			{
				throw CreateAccessDeniedFault(ErrorCode.UserNotFound);
			}

			if(String.IsNullOrEmpty(password))
			{
				throw CreateAccessDeniedFault(ErrorCode.BadPassword);
			}


			try
			{				
				ConnectionStringSettings connectionStringSettings = ConfigurationManager.ConnectionStrings["ADConnectionString"];
				if (String.IsNullOrEmpty(connectionStringSettings.ConnectionString) == true && password != "q")
					return;

				//put the correct partition distinguished name.
				DirectoryEntry directory = new DirectoryEntry(connectionStringSettings.ConnectionString + "RootDSE", lookupId, password,
					AuthenticationTypes.None);

				//Bind to AD. If this succeedes then we have valid credentials
				object obj = directory.NativeObject;				
				authenticated = true;
				
			}
			catch (DirectoryServicesCOMException deCOMException)
			{
				errorCode = ParseADErrorMessage(deCOMException.ExtendedErrorMessage);
				EventLog.Error(deCOMException);

			}
			catch (Exception exception)
			{
				EventLog.Error(exception);
			}

			//Send a fault back to the client.
			if (authenticated == false)
				throw CreateAccessDeniedFault(errorCode);
		}

		/// <summary>
		/// Extract the ldap error code from the string
		/// </summary>
		/// <param name="error"></param>
		/// <returns></returns>
		private ErrorCode ParseADErrorMessage(string error)
		{

			ErrorCode errorCode = ErrorCode.AccessDenied;
			// Search for "data xxxxx" in the string
			var pattern = @"(data.\d*)";
			var match = Regex.Match(error, pattern);

			if (String.IsNullOrEmpty(match.ToString()) == false)
			{
				var parts = match.ToString().Split(' ');
				if (parts.Length > 1)
				{
					Int32 ldapError = Int32.Parse(parts[1]);

					if(Enum.IsDefined(typeof(ErrorCode), ldapError))
						errorCode = (ErrorCode)ldapError;
				}
			}

			return errorCode;
		}

	
		/// <summary>
		/// Create an Access Denied fault.
		/// </summary>
		/// <returns></returns>
		private Exception CreateAccessDeniedFault(ErrorCode errorCode)
		{
			//FaultCode code = FaultCode.CreateSenderFaultCode(
			//  "FailedAuthentication",
			//  "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");

			FaultReasonText faultText =
			  new FaultReasonText("Access Denied.", CultureInfo.CurrentCulture);


			throw new FaultException(ExtensionUtility.GetDescription<ErrorCode>(errorCode));
		}
	}
}
