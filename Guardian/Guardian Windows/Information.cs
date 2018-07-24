namespace FluidTrade.Guardian.Windows
{

	using System;
	using System.ServiceModel.Security;
    using System.Windows;
	using FluidTrade.Guardian.TradingSupportReference;

	/// <summary>
	/// Global information regarding the application state.
	/// </summary>
	public class Information
	{

		/// <summary>
		/// The unique identifier of the current user.
		/// </summary>
		public static Guid UserId
		{
			//TODO: Rectify all places this can be called or have this method handle all exceptions.
			// Rolled back to original code.
			// TODO: Add caching of the UserId.

			get
			{

				// An useless user id is returned if there are any problems getting the real version from the middle tier.
				Guid userId = Guid.Empty;

				// This provides a strongly typed channel to the middle tier.
				TradingSupportClient tradingSupportClient =  new TradingSupportClient(FluidTrade.Guardian.Properties.Settings.Default.TradingSupportEndpoint);

				try
				{
					// This asks the middle tier server for the identification by which the current user is known to the server.
					userId = tradingSupportClient.GetUserId();

				}
				catch (SecurityAccessDeniedException securityAccessDeniedException)
				{

					// The user's credentials are not valid on the server.
					MessageBox.Show(securityAccessDeniedException.Message);

				}
				// COMMENTED OUT FOR NOW.
				//catch (System.ServiceModel.CommunicationObjectAbortedException coae)
				//{
				//    // User cancelled the operation or there really was a problem with the communication. Regardless log this to the event log.
				//    FluidTrade.Core.EventLog.Error(coae);
				//}
				//catch (Exception e)
				//{
				//    // TODO: We might consider other more specific exception handling and let this not be handled here by a general exception 
				//    // but then every calling method will need to have exception handling. For now we handle all exceptions here.
				//    FluidTrade.Core.EventLog.Error(e);
				//    MessageBox.Show("Get user id from server failed. Please try again.", "User information verification", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				//}
			finally
			{

				if (tradingSupportClient != null && tradingSupportClient.State == System.ServiceModel.CommunicationState.Opened)
					tradingSupportClient.Close();
			}

				// This uniquely identifies the user in the data model.
				return userId;

			}

		}

	}

}
