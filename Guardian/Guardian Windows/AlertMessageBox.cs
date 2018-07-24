namespace FluidTrade.Guardian
{
	using System;
	using System.Diagnostics;
	using System.ServiceModel;
	using System.Threading;
	using System.Windows;
	using FluidTrade.Core;

	/// <summary>
	/// Queued message box to prevent multiple alerts.
	/// </summary>
	public class AlertMessageBox
	{
		//Synchronization members for alerts
		private ManualResetEvent showMessageBox = new ManualResetEvent(false);
		private AlertMessageBoxType currentAlertType;
		private DateTime lastDispayTime;
		private const int MinimumSeconds = 30;

		/// <summary>		
		/// Eager initialization since we will need it for Singleton		
		/// </summary>
		public static readonly AlertMessageBox Instance = new AlertMessageBox();

		///<summary>
		///C# compiler marks "beforefieldinit" on classes with Static constructor ensuring lazy initiailization.,
		///</summary>
		static AlertMessageBox()
		{
		}

		//private constructor for Singleton
		private AlertMessageBox() 
		{
			lastDispayTime = DateTime.MinValue;
		}

		/// <summary>
		/// Show MessageBox if appropriate of alertType
		/// </summary>
		/// <param name="alertType"></param>
		public void Show(AlertMessageBoxType alertType)
		{
			Display(String.Empty, alertType);
		}


		/// <summary>
		/// Show messagebox if not already showing
		/// </summary>
		/// <param name="message"></param>
		public void Show(String message)
		{
			Display(message, AlertMessageBoxType.General);
		}

		/// <summary>
		/// Show messagebox if not already showing
		/// </summary>
		/// <param name="message"></param>
		/// <param name="alertType"></param>
		public void Show(String message, AlertMessageBoxType alertType)
		{
			Display(message, alertType);
		}

		/// <summary>
		/// Show messagebox if not already showing
		/// </summary>
		/// <param name="message"></param>
		/// <param name="alertType"></param>
		/// <param name="messageBoxButtons"></param>
		public MessageBoxResult Show(String message, AlertMessageBoxType alertType, MessageBoxButton messageBoxButtons)
		{
			return Display(message, alertType, messageBoxButtons);
		}


		/// <summary>
		/// Show an exeption alert
		/// </summary>
		/// <param name="optimisticConcurrencyException"></param>
		public void Show(FaultException<OptimisticConcurrencyFault> optimisticConcurrencyException)
		{
			Display(String.Format(FluidTrade.Core.Properties.Resources.OptimisticConcurrencyError,
								optimisticConcurrencyException.Detail.TableName), AlertMessageBoxType.OptimisticConcurrencyError);
		}
		
		/// <summary>
		/// Show an exeption alert
		/// </summary>
		/// <param name="recordNotFoundException"></param>
		public void Show(FaultException<RecordNotFoundFault> recordNotFoundException)
		{
			Display(String.Format(FluidTrade.Core.Properties.Resources.RecordNotFoundError,
														CommonConversion.FromArray(recordNotFoundException.Detail.Key),
														recordNotFoundException.Detail.TableName),
														AlertMessageBoxType.RecordNotFoundError);
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		/// <param name="alertType"></param>
		private void Display(String message, AlertMessageBoxType alertType)
		{
			Display(message, alertType, MessageBoxButton.OK);
		}


		/// <summary>
		/// The engine
		/// </summary>
		/// <param name="message"></param>
		/// <param name="alertType"></param>
		/// <param name="messageBoxButton"></param>
		private MessageBoxResult Display(String message, AlertMessageBoxType alertType, MessageBoxButton messageBoxButton)
		{

			
			
			//Only show this dialog if this type has not shown for last 30 seconds.  This is to prevent multiple 
			//threads from popping up the same error.
			if (alertType != AlertMessageBoxType.General &&  alertType == currentAlertType)
			{
				if ((DateTime.Now - lastDispayTime).Seconds < MinimumSeconds)
					return MessageBoxResult.None;
			}

			//Show only one message box at a time. Block until all the other alerts are done.
			lock (showMessageBox)
			{
				//Start locking all the other threads.
				showMessageBox.Reset();
				currentAlertType = alertType;					
			}

			
			if(String.IsNullOrEmpty(message))
			{
				message = alertType.GetDescription<AlertMessageBoxType>();
			}

			MessageBoxResult result = MessageBoxResult.None;
			//Sanity Check. Don't show empty message boxes.
			if (String.IsNullOrEmpty(message) == false)
			{
				lastDispayTime = DateTime.Now;
				result = MessageBox.Show(message, Application.Current.MainWindow.Title, messageBoxButton, MessageBoxImage.Error);
			}
			else
			{
				Debug.Assert(false, "Empty alert message.  Please fix!");
			}
			
			showMessageBox.Set();
			return result;
		}
		
	}

	/// <summary>
	/// AlertMessageTypes
	/// </summary>
	public enum AlertMessageBoxType : int
	{		
		/// <summary>
		/// Default
		/// </summary>		
		General,
		/// <summary>
		/// AuthenticationError
		/// </summary>
		[Description("Could not authenticate.")]
		AuthenticationError,
		/// <summary>
		/// Server cut connection
		/// </summary>
		[Description("The server is unresponsive, please try again.")]
		LostConnectionToServer,
		/// <summary>
		/// Server cut connection
		/// </summary>
		[Description("Someone got to it before you.")]
		OptimisticConcurrencyError,
		/// <summary>
		/// Concurrency Error
		/// </summary>
		[Description("Record not found in the database.")]
		RecordNotFoundError,

	}
}