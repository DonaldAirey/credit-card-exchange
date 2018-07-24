namespace FluidTrade.Guardian
{	

	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Reflection;
	using System.Threading;
	using System.Transactions; 
    using FluidTrade.Core;

	/// <summary>Finds matching orders and negotiations the exchange of assets.</summary>
	/// <copyright>Copyright (C) 1998-2005 Fluid Trade -- All Rights Reserved.</copyright>
	internal class CrossingManager
	{

		// Private Static Methods
		private static Boolean isCrossingActive;
        private static List<IExchange> exchanges;
		private static Object syncRoot;

		/// <summary>
		/// Manages the exchanges used to effect the transfer of assets.
		/// </summary>
		static CrossingManager()
		{

			// This object is used for multithreaded coordination.
			CrossingManager.syncRoot = new Object();

            // This list keeps a reference of all the exchanges loaded here.  These exchanges will take care of listening to the data model for changes and
            // matching orders when possible.  The resources used by these resources will be reclaimed when the references here are destroyed.
            CrossingManager.exchanges = new List<IExchange>();

		}

		/// <summary>
		/// Gets or sets the status of the thread that handles the actions that enforce the business rules.
		/// </summary>
		internal static Boolean IsCrossingActive
		{

			get
			{

				// This is the current status of the crossing activity.
				lock (CrossingManager.syncRoot)
					return CrossingManager.isCrossingActive;

			}

			set
			{

				// The idea here is to start the thread and install the event handlers when the crossingn is activated and to kill the thread and uninstall the
				// handlers when the crossing is deactivated.
				lock (CrossingManager.syncRoot)
				{

					try
					{

						// This will turn on crossing when it has been inactive.
						if (!CrossingManager.isCrossingActive && value)
						{

							// Installing the exchanges is tricky.  If this property is accessed before the data model is loaded, then the tables will be empty
							// because the data loads up in a background thread. The only solid way to start the exchange when the tables are empty is to wait for 
							// data to become available.
							try
							{
								DataModel.DataLock.EnterWriteLock();
								DataModel.CrossingManager.CrossingManagerRowChanging += new CrossingManagerRowChangeEventHandler(OnCrossingManagerRowChanging);
							}
							finally
							{
								DataModel.DataLock.ExitWriteLock();
							}

							// This will collect the fully qualified type names of all the types that need to be constructed.  The list must be constructed 
							// before creating the exchanges because the constructor for the exchanges will likely try to use a self-contained transaction to
							// access the data model.  This basically means that the constructor for an exchange can't be called from inside of another
							// transaction.
							List<String> exchangeTypes = new List<string>();

							// In the event that there is already data in the Type table that tells us about the exchanges we can run, the Type table is or
							// scanned favailable exchanges and the are started en masse.
							using (TransactionScope transactionScope = new TransactionScope())
							{

								// This variable holds context information for the current transaction.
								DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

								// This will read each Type in the data model and start up an exchange for each one declared.
								foreach (CrossingManagerRow crossingManagerRow in DataModel.CrossingManager)
								{

									// The exchanges are loaded into memory from the information about the assembly and type found in the Type table.
									try
									{
										crossingManagerRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
										exchangeTypes.Add(crossingManagerRow.CrossingType);
									}
									finally
									{
										crossingManagerRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
									}

								}

							}

							// When each of the exchanges is loaded into memory from the information in the data model, they can be opened for business.
							foreach (String exchangeType in exchangeTypes)
							{
								IExchange iExchange = LoadExchange(exchangeType);
								if (iExchange != null)
									iExchange.Start();
							}

						}

						// This wil turn off crossing when it has been active.
						if (CrossingManager.isCrossingActive && !value)
						{

							// Remove the event handlers that install the exchanges individually.
							try
							{
								DataModel.DataLock.EnterWriteLock();
								DataModel.CrossingManager.CrossingManagerRowChanging -= new CrossingManagerRowChangeEventHandler(OnCrossingManagerRowChanging);
							}
							finally
							{
								DataModel.DataLock.ExitWriteLock();
							}

							// Once each of the exchanges is loaded from the information in the type table, they can be opened for business.
							foreach (IExchange iExchange in CrossingManager.exchanges)
							{
								iExchange.Stop();
							}

							// Purging this list will de-reference the exchange and allow the resources to be reclaimed.
							CrossingManager.exchanges.Clear();

						}

						// This field is used to keep track of the state of the crossing engine.
						CrossingManager.isCrossingActive = value;

					}
					catch (Exception exception)
					{

						// Make sure any errors starting up an exchange are logged.
						EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);

					}

				}

			}

		}

		/// <summary>
		/// Loads
		/// </summary>
		/// <param name="exchangeType"></param>
		/// <returns></returns>
		private static IExchange LoadExchange(String exchangeType)
		{

			// The main idea is to create an instance of the exchange from the fully qualified name.  If anything fails during this process, then this is what
			// is returned to the caller.
			IExchange iExchange = null;

			// Pull apart the fully qualified type name to find the assembly where the type can be found.
			String[] typeParts = exchangeType.Split(',');
			if (typeParts.Length < 2)
				throw new Exception(string.Format("Can't create an Exchange Handler for '{0}'.", exchangeType));
			String assemblyName = typeParts[1].Trim();
			for (int index = 2; index < typeParts.Length; index++)
				assemblyName += ',' + typeParts[index].Trim();

			// This type is used to dynamically load the exchange handler.
			String typeName = typeParts[0].Trim();

			try
			{

				// Load or get the assembly where the Exchange logic is kept.
				Assembly assembly = Assembly.Load(assemblyName);

				// This will create an instance of the exchange.
				iExchange = assembly.CreateInstance(
						typeName,
						false,
						BindingFlags.CreateInstance,
						null,
						null,
						System.Globalization.CultureInfo.CurrentCulture,
						null) as IExchange;

				// Successfully created exchanges are added to the list managed by this class.
				if (iExchange != null)
					CrossingManager.exchanges.Add(iExchange);

			}
			catch (Exception exception)
			{
				EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);
			}

			// This is an interface to the generic exchange.
			return iExchange;

		}

		/// <summary>
		/// Handles a change made to the 'Type' table.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event argument.</param>
		private static void OnCrossingManagerRowChanging(object sender, CrossingManagerRowChangeEventArgs e)
		{

			// Loading up exchanges should not slow down the data model.  This event handler will detect any new exchanges and invoke a background thread to
			// start it up.
			if (e.Action == DataRowAction.Commit && e.Row.RowState != DataRowState.Detached)
			{
				if (!e.Row.HasVersion(DataRowVersion.Original))
				{
					Object exchangeType = e.Row[DataModel.CrossingManager.CrossingTypeColumn];
					if (exchangeType != DBNull.Value)
						ThreadPool.QueueUserWorkItem(StartExchangeThread, exchangeType);
				}
			}

		}

		/// <summary>
		/// Loads an individual exchange into memory and starts it.
		/// </summary>
		/// <param name="state">The generic thread initialization parameter.</param>
		private static void StartExchangeThread(Object state)
		{

			// Convert the generic parameter into the fully qualified name of the type used to implement the exchange.
			String exchangeType = (String)state;

			// Adding an exchange is a critical operation.  Because it requires a lock it must be done outside of any event handlers that deal with the data
			// model.
			lock (CrossingManager.syncRoot)
			{

				// Start the exchange if it loads successfully.
				IExchange iExchange = LoadExchange(exchangeType);
				if (iExchange != null)
					iExchange.Start();

			}

		}

	}

}
