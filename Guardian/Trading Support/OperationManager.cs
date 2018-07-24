namespace FluidTrade.Guardian
{

	using FluidTrade.Core;
	using System;
	using System.ComponentModel;
	using System.Configuration;
	using System.Collections;
	using System.Data;
	using System.Threading;
	using System.Transactions;

	/// <summary>
	/// Manages the business logic for the server data model.
	/// </summary>
	/// <copyright>Copyright (C) 1998-2005 Fluid Trade -- All Rights Reserved.</copyright>
	public class OperationManager
	{

		// Private Members
		private static OperationParameters operatingParameters;
		private static Object syncRoot;

		/// <summary>
		/// This object will simulate an auction market on equities.
		/// </summary>
		static OperationManager()
		{

			// This object is used by the System.Monitor methods that control multithreaded access to the data in this class.
			OperationManager.syncRoot = new Object();

			// These are the initial operating parameters for the business rules.
			OperationParameters operatingParameters = new OperationParameters();
			operatingParameters.AreBusinessRulesActive = true;
			operatingParameters.IsCrossingActive = true;
			operatingParameters.IsChatActive = true;
			OperationManager.OperatingParameters = operatingParameters;

		}

		/// <summary>
		/// Gets or sets the thread safe parameters that control the trading operations.
		/// </summary>
		public static OperationParameters OperatingParameters
		{

			get
			{

				// These parameters are shared by different threads and must be locked before reading.
				lock (OperationManager.syncRoot)
					return OperationManager.operatingParameters;

			}
			set
			{

				// These parameters are shared by different threads.
				lock (OperationManager.syncRoot)
				{

					// The saved parameters reflect the state of the simulation.
					OperationManager.operatingParameters = value;

					// Start or stop the business rules.
					BusinessRules.AreBusinessRulesActive = OperationManager.operatingParameters.AreBusinessRulesActive;

					// Start or stop the crossing.
					CrossingManager.IsCrossingActive = OperationManager.operatingParameters.IsCrossingActive;

					// Start or stop the crossing.
					ChatManager.IsChatActive = OperationManager.operatingParameters.IsChatActive;

				}

			}

		}

	}

}
