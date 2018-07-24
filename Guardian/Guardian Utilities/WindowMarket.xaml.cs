namespace FluidTrade.Guardian
{

	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Threading;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Data;
	using System.Windows.Input;
	using System.Windows.Media;
	using System.Windows.Media.Imaging;
	using System.Windows.Threading;

	/// <summary>
	/// Prompts the user for parameters used to control the market simulation.
	/// </summary>
	public partial class WindowMarket : Window
	{

		// Private Delegates
		private delegate void SetSimulationParametersHandler(SimulationParameters simulationParameters);

		/// <summary>
		/// Creates a dialog that prompts for market simulator parameters.
		/// </summary>
		public WindowMarket()
		{

			// The IDE created resources are managed here.
			InitializeComponent();

			this.Loaded += new RoutedEventHandler(OnLoaded);

		}

		void OnLoaded(object sender, RoutedEventArgs e)
		{

			ThreadPool.QueueUserWorkItem(InitializeData);
			
		}

		public Boolean IsBrokerSimulationRunning
		{
			get { return this.checkBoxIsBrokerSimulatorRunning.IsChecked == true; }
			set { this.checkBoxIsBrokerSimulatorRunning.IsChecked = value; }
		}

		public Boolean IsPriceSimulationRunning
		{
			get { return this.checkBoxIsPriceSimulatorRunning.IsChecked == true; }
			set { this.checkBoxIsPriceSimulatorRunning.IsChecked = value; }
		}

		public Double Frequency
		{
			get { return Convert.ToDouble(this.textBoxFrequency.Text); }
			set { this.textBoxFrequency.Text = Convert.ToString(value); }
		}

		private void InitializeData(object state)
		{

			// This creates a client to communicate with the server.
			TradingSupportClient tradingSupportClient = new TradingSupportClient(Properties.Settings.Default.TradingSupportEndpoint);

			SimulationParameters simulationParameters = tradingSupportClient.GetSimulationParameters();

			// Release the resources used to communicate with the server.
			tradingSupportClient.Close();

			this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SetSimulationParametersHandler(OnSetSimulationParameters), simulationParameters);

		}

		/// <summary>
		/// Handles a click event on the Cancel button.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event data.</param>
		private void OnCancelButtonClick(object sender, RoutedEventArgs e)
		{

			this.DialogResult = false;
			this.Close();

		}

		/// <summary>
		/// Handles a click event on the OK button.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event data.</param>
		private void OnOkButtonClick(object sender, RoutedEventArgs e)
		{

			this.DialogResult = true;
			this.Close();

		}

		private void OnSetSimulationParameters(SimulationParameters simulationParameters)
		{

			// Initialize the state of the dialog box from the state of the simulation recovered from the server.
			this.IsBrokerSimulationRunning = simulationParameters.IsBrokerSimulationRunning;
			this.IsPriceSimulationRunning = simulationParameters.IsPriceSimulationRunning;
			this.Frequency = simulationParameters.Frequency;

			// Enable the dialog box controls when data has been obtained from the server that reflects the state of the simulator.
			this.buttonOK.IsEnabled = true;
			this.checkBoxIsBrokerSimulatorRunning.IsEnabled = true;
			this.checkBoxIsPriceSimulatorRunning.IsEnabled = true;
			this.labelFrequency.IsEnabled = true;
			this.textBoxFrequency.IsEnabled = true;

		}

	}

}
