namespace FluidTrade.Guardian
{

	using System;
	using System.Collections.Generic;
	using System.Windows;
	using System.Windows.Input;

	public class Commands
	{

		/// Gets the value that represents the ClearCross command.
		/// </summary>
		public static readonly System.Windows.Input.RoutedUICommand ClearCross;

		/// <summary>
		/// Gets the value that represents the CreateAccessControlList command.
		/// </summary>
		public static readonly System.Windows.Input.RoutedUICommand CreateAccessControlList;

		/// <summary>
		/// Gets the value that represents the CreateEquityOrders command.
		/// </summary>
		public static readonly System.Windows.Input.RoutedUICommand CreateEquityOrders;

		/// <summary>
		/// Gets the value that represents the CreateFixedIncomeOrders command.
		/// </summary>
		public static readonly System.Windows.Input.RoutedUICommand CreateFixedIncomeOrders;

		/// <summary>
		/// Gets the value that represents the DestroyOrders command.
		/// </summary>
		public static readonly System.Windows.Input.RoutedUICommand DestroyOrders;

		/// <summary>
		/// Gets the value that represents the LoadDebtReport command.
		/// </summary>
		public static readonly System.Windows.Input.RoutedUICommand LoadDebtReport;

		/// <summary>
		/// Gets the value that represents the LoadEquityReport command.
		/// </summary>
		public static readonly System.Windows.Input.RoutedUICommand LoadEquityReport;

		/// <summary>
		/// Gets the value that represents the LoadDebtNegoriatorInventory command.
		/// </summary>
		public static readonly System.Windows.Input.RoutedUICommand LoadDebtNegotiatorInventory;

		/// <summary>
		/// Gets the value that represents the LoadDebtHolderInventory command.
		/// </summary>
		public static readonly System.Windows.Input.RoutedUICommand LoadDebtHolderInventory;

		/// <summary>
		/// Gets the value that represents the SimulateMarket command.
		/// </summary>
		public static readonly System.Windows.Input.RoutedUICommand SimulateMarket;

		/// <summary>
		/// Create the static resources used by this test module.
		/// </summary>
		static Commands()
		{

			// Routed Commands
			Commands.ClearCross = new RoutedUICommand("Clear Cross", "ClearCross", typeof(Commands));
			Commands.CreateAccessControlList = new RoutedUICommand("Create Access Control List", "CreateAccessControlList", typeof(Commands));
			Commands.CreateEquityOrders = new RoutedUICommand("Create Equity Orders...", "CreateEquityOrders", typeof(Commands));
			Commands.CreateFixedIncomeOrders = new RoutedUICommand("Create Fixed Income Orders...", "CreateFixedIncomeOrders", typeof(Commands));
			Commands.DestroyOrders = new RoutedUICommand("Destroy Orders", "DestroyOrders", typeof(Commands));
			Commands.LoadDebtReport = new RoutedUICommand("Load Debt Report...", "LoadDebtReport", typeof(Commands));
			Commands.LoadEquityReport = new RoutedUICommand("Load Equity Report...", "LoadEquityReport", typeof(Commands));
			Commands.LoadDebtNegotiatorInventory = new RoutedUICommand("Load Debt Negotiator Inventory...", "LoadDebtNegotiatorData", typeof(Commands));
			Commands.LoadDebtHolderInventory = new RoutedUICommand("Load Debt Holder Inventory...", "LoadDebtHolderData", typeof(Commands));
			Commands.SimulateMarket = new RoutedUICommand("Simulate Market...", "SimulateMarket", typeof(Commands));

			// Global Key Gestures
			Commands.ClearCross.InputGestures.Add(new KeyGesture(Key.C, ModifierKeys.Alt));
			Commands.CreateEquityOrders.InputGestures.Add(new KeyGesture(Key.Q, ModifierKeys.Alt));
			Commands.CreateFixedIncomeOrders.InputGestures.Add(new KeyGesture(Key.F, ModifierKeys.Alt));
			Commands.DestroyOrders.InputGestures.Add(new KeyGesture(Key.D, ModifierKeys.Alt));
			Commands.LoadEquityReport.InputGestures.Add(new KeyGesture(Key.L, ModifierKeys.Alt));
			Commands.SimulateMarket.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Alt));

		}

	}

}
