namespace FluidTrade.Guardian
{

	using FluidTrade.Core;
	using Microsoft.Win32;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Data;
	using System.Windows.Input;
	using System.Windows.Threading;
	using System.Xml.Linq;
	using System.Reflection;
	using System.Threading;

	/// <summary>
	/// The main widow for the application.
	/// </summary>
	public partial class WindowMain : Window
	{

		// Private Instance Fields
		private Boolean isDataModelLoaded;

		/// <summary>
		/// Create the main window of the application.
		/// </summary>
		public WindowMain()
		{

			// The IDE maintained resources are initialized here.
			InitializeComponent();

			// The data model is empty when the application first loads itself into the visual tree hierarchy.  Since many of the
			// commands depend on a loaded data model, a signal is required that enables these commands once the model is loaded.
			// This value is set to true once that happens.
			this.isDataModelLoaded = false;

			// This thread will wait for the data model to update for the first time and then will enable the menu items that
			// require a loaded data model.
			ThreadPool.QueueUserWorkItem(WaitForUpdate);

		}

		/// <summary>
		/// Handles a request to terminate the application.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The unused event arguments.</param>
		private void OnClose(object sender, RoutedEventArgs e)
		{

			// Closing the main window will set of a series of events that will shut down the application.
			this.Close();

		}


		/// <summary>
		/// Gets an indication of whether the user can execute the 'GenerateTrades' command.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="canExecuteRoutedEventArgs">Parameters that indicate whether a command can be executed.</param>
		private void OnCanUseDataModel(object sender, CanExecuteRoutedEventArgs canExecuteRoutedEventArgs)
		{

			// This enables the menu items when the data model is finished loading.
			canExecuteRoutedEventArgs.CanExecute = this.isDataModelLoaded;

		}

		/// <summary>
		/// Enabled the menu items that require a loaded data model.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="eventArgs">The event data.</param>
		private void EnableDataModelCommands(object sender, EventArgs eventArgs)
		{

			// This enables those menu items that depend on a loaded data model.
			this.menuItemAction.IsEnabled = true;

			// This flag will prevent commands from executing that depend on a loaded data model.
			this.isDataModelLoaded = true;

		}

		/// <summary>
		/// Handles a request to create an access control list.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event data.</param>
		private void OnCreateAccessControlList(Object sender, RoutedEventArgs e)
		{

			// Generate the access control list in a background thread so the data model can be locked.
			ThreadPool.QueueUserWorkItem(AccessControlList.CreateAccessControlList);

		}
		
		/// <summary>
		/// Handles a request to create orders.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event data.</param>
		private void OnCreateEquityOrders(object sender, RoutedEventArgs e)
		{

			// Prompt the user for the parameters for generating the new trades and then go and create them.
			WindowOrder windowOrder = new WindowOrder();
			if (windowOrder.ShowDialog() == true)
				ThreadPool.QueueUserWorkItem(
					EquityOrders.CreateOrders,
					new GenerateTradeInfo(windowOrder.OrderCount, windowOrder.BlotterId, windowOrder.FileName));

		}

		/// <summary>
		/// Handles a request to create orders.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event data.</param>
		private void OnCreateFixedIncomeOrders(object sender, RoutedEventArgs e)
		{

			// Prompt the user for the parameters for generating the new trades and then go and create them.
			WindowOrder windowOrder = new WindowOrder();
			if (windowOrder.ShowDialog() == true)
				ThreadPool.QueueUserWorkItem(
					FixedIncomeOrders.CreateOrders,
					new GenerateTradeInfo(windowOrder.OrderCount, windowOrder.BlotterId, windowOrder.FileName));

		}

		/// <summary>
		/// Handles a request to destroy all the trades.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event data.</param>
		private void OnDestroyOrders(object sender, RoutedEventArgs e)
		{

			// Prompt the user for the parameters for generating the new trades and then go and create them.
			ThreadPool.QueueUserWorkItem(Orders.DestroyOrders);

		}

		/// <summary>
		/// Handles a request to clear all crosses.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event data.</param>
		private void OnClearCross(object sender, RoutedEventArgs e)
		{

			// Prompt the user for the parameters for generating the new trades and then go and create them.
			ThreadPool.QueueUserWorkItem(Orders.ClearCross);

		}

		/// <summary>
		/// Handles a request to destroy all the trades.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event data.</param>
		private void OnLoadDebtReport(object sender, RoutedEventArgs e)
		{

			// Configure the 'Open File' dialog box to look for the available XML files.
			OpenFileDialog openFile = new OpenFileDialog();
			openFile.DefaultExt = ".xaml";
			openFile.Filter = "XAML Documents (.xaml)|*.xaml";

			// Show open file dialog box
			Nullable<bool> result = openFile.ShowDialog();
			if (result == true)
			{
				XDocument xDocument = XDocument.Load(openFile.FileName);
				DataModelClient dataModelClient = new DataModelClient(Properties.Settings.Default.DataModelEndpoint);
				dataModelClient.UpdateReportEx("Default", null, null, null, new object[] { "DEBT WORKING ORDER REPORT" }, null, xDocument.ToString());
				dataModelClient.Close();
			}

		}

		/// <summary>
		/// Handles a request to destroy all the trades.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event data.</param>
		private void OnLoadEquityReport(object sender, RoutedEventArgs e)
		{

			// Configure the 'Open File' dialog box to look for the available XML files.
			OpenFileDialog openFile = new OpenFileDialog();
			openFile.DefaultExt = ".xaml";
			openFile.Filter = "XAML Documents (.xaml)|*.xaml";

			// Show open file dialog box
			Nullable<bool> result = openFile.ShowDialog();
			if (result == true)
			{
				XDocument xDocument = XDocument.Load(openFile.FileName);
				DataModelClient dataModelClient = new DataModelClient(Properties.Settings.Default.DataModelEndpoint);
				dataModelClient.UpdateReportEx("Default", null, null, null, new object[] { "EQUITY WORKING ORDER REPORT" }, null, xDocument.ToString());
				dataModelClient.Close();
			}

		}

		/// <summary>
		/// Handles a request to destroy all the trades.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event data.</param>
		private void OnLoadDebtNegotiatorInventory(object sender, RoutedEventArgs e)
		{

			// Configure the 'Open File' dialog box to look for the available XML files.
			OpenFileDialog openFile = new OpenFileDialog();
			openFile.DefaultExt = ".xlsx";
			openFile.Filter = "XML Documents (.xml)|*.xml";

			// Show open file dialog box
			Nullable<bool> result = openFile.ShowDialog();
			if (result == true)
			{
				// It doesn't matter what the user selects, we're going to load the data from a predetermined file.  This makes it look like we're
				// importing from Excel, when in fact we're just loading an XML formatted file with the Consumer Trust data.
				ScriptLoader scriptLoader = new ScriptLoader();
				scriptLoader.FileName = openFile.FileName;
				scriptLoader.Load();
			}

		}

		/// <summary>
		/// Handles a request to destroy all the trades.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event data.</param>
		private void OnLoadDebtHolderInventory(object sender, RoutedEventArgs e)
		{

			// Configure the 'Open File' dialog box to look for the available XML files.
			OpenFileDialog openFile = new OpenFileDialog();
			openFile.DefaultExt = ".xlsx";
			openFile.Filter = "XML Documents (.xml)|*.xml";

			// Show open file dialog box
			Nullable<bool> result = openFile.ShowDialog();
			if (result == true)
			{
				// It doesn't matter what the user selects, we're going to load the data from a predetermined file.  This makes it look like we're
				// importing from Excel, when in fact we're just loading an XML formatted file with the Consumer Trust data.
				ScriptLoader scriptLoader = new ScriptLoader();
				scriptLoader.FileName = openFile.FileName;
				scriptLoader.Load();
			}

		}

		/// <summary>
		/// Turn the market simulator on or off.
		/// </summary>
		/// <param name="sender">The object that created this event.</param>
		/// <param name="e">The event arguments.</param>
		private void OnSimulateMarket(object sender, ExecutedRoutedEventArgs e)
		{

			// Prompt the user for the simulation parameters and then adjust the simulator on the server.
			WindowMarket windowMarket = new WindowMarket();
			if (windowMarket.ShowDialog() == true)
			{
				SimulationParameters simulationParameters = new SimulationParameters();
				simulationParameters.IsBrokerSimulationRunning = windowMarket.IsBrokerSimulationRunning;
				simulationParameters.IsPriceSimulationRunning = windowMarket.IsPriceSimulationRunning;
				simulationParameters.Frequency = windowMarket.Frequency;
				ThreadPool.QueueUserWorkItem(Market.Simulate, simulationParameters);
			}

		}

		/// <summary>
		/// Waits for the data model to be loaded and then signals the foreground.
		/// </summary>
		/// <param name="state">The thread initialization parameters.</param>
		private void WaitForUpdate(object state)
		{

			// The foreground can't be held up waiting for an event, so this background thread will wait here until the model has
			// updated for the first time and then signal the foreground that it can begin using the commands that depend on a
			// loaded data model.
			DataModel.WaitForUpdate();
			Dispatcher.BeginInvoke(DispatcherPriority.Normal, new EventHandler(EnableDataModelCommands), this, EventArgs.Empty);

		}

	}

}
