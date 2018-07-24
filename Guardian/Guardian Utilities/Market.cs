namespace FluidTrade.Guardian
{

	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Data;
	using System.Windows.Input;
	using System.Windows.Threading;
	using System.Xml.Linq;
	using System.Threading;

	class Market
	{

		/// <summary>
		/// Adjusts the parameters of the market simulation.
		/// </summary>
		/// <param name="state">The generic thread initialization parameter.</param>
		public static void Simulate(object state)
		{

			// Extract the strongly typed parameter from the generic thread parameter.
			SimulationParameters simulationParameters = state as SimulationParameters;

			// This creates a client to communicate with the server.
			TradingSupportClient tradingSupportClient = new TradingSupportClient(Properties.Settings.Default.TradingSupportEndpoint);

			tradingSupportClient.SetSimulationParameters(simulationParameters);

			// Release the resources used to communicate with the server.
			tradingSupportClient.Close();

		}
	
	}

}
