namespace FluidTrade.Guardian
{

	using FluidTrade.Core;
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.IdentityModel.Policy;
	using System.IdentityModel.Claims;
	using System.Security.Principal;
	using System.ServiceModel;
	using System.Threading;
	using System.Transactions;

	/// <summary>
	/// Manages the business logic for the server data model.
	/// </summary>
	/// <copyright>Copyright (C) 2001-2008 Fluid Trade, Inc. -- All Rights Reserved.</copyright>
	internal class BusinessRules
	{

		// Private Static Fields
		private static Boolean areBusinessRulesActive;
		private static DataModel dataModel;
		private static Object syncRoot;

		/// <summary>
		/// This object will enforce the business rules on the shared data model.
		/// </summary>
		static BusinessRules()
		{

			// This object is used to lock the fields in this class during multithreaded operations.
			BusinessRules.syncRoot = new Object();

			// An instance of the data model is required to modify it.
			BusinessRules.dataModel = new DataModel();

		}

		/// <summary>
		/// Gets or sets the status of the thread that executes the asynchronous actions that enforce the business rules.
		/// </summary>
		internal static Boolean AreBusinessRulesActive
		{

			get
			{

				// Return the thread-safe status of the business rules thread.
				lock (BusinessRules.syncRoot)
					return BusinessRules.areBusinessRulesActive;

			}

			set
			{

				// Set the thread-safe status of the business rules thread and start or stop the thread that handles those rules.
				lock (BusinessRules.syncRoot)
				{

					// Install the business logic handlers if they are not already installed.
					if (!BusinessRules.areBusinessRulesActive && value)
					{
						DataModel.DestinationOrder.DestinationOrderRowValidate += DestinationOrder.OnDestinationOrderRowValidate;
						DataModel.Execution.ExecutionRowValidate += Execution.OnExecutionRowValidate;
						DataModel.SourceOrder.SourceOrderRowValidate += SourceOrder.OnSourceOrderRowValidate;
						DataModel.WorkingOrder.WorkingOrderRowValidate += WorkingOrder.OnWorkingOrderRowValidate;
					}

					// Remove the business logic handlers if they are installed.
					if (BusinessRules.areBusinessRulesActive && !value)
					{
						DataModel.DestinationOrder.DestinationOrderRowValidate -= DestinationOrder.OnDestinationOrderRowValidate;
						DataModel.Execution.ExecutionRowValidate -= Execution.OnExecutionRowValidate;
						DataModel.SourceOrder.SourceOrderRowValidate -= SourceOrder.OnSourceOrderRowValidate;
						DataModel.WorkingOrder.WorkingOrderRowValidate -= WorkingOrder.OnWorkingOrderRowValidate;
					}

					// This thread-safe flag indicates whether or not the business logic is active.
					BusinessRules.areBusinessRulesActive = value;

				}

			}

		}

	}

}
