namespace FluidTrade.Core.Windows
{

	using System;
	using System.Collections.Generic;
	using System.Windows;
	using System.Windows.Input;

	public class FluidTradeCommands
	{

		/// <summary>
		/// Applies the current action but doesn't dismiss the prompt.
		/// </summary>
		public static readonly RoutedCommand Apply;

		/// <summary>
		/// Cancels the current action and dismisses the prompt.
		/// </summary>
		public static readonly RoutedCommand Cancel;
		
		/// <summary>
		/// Freezes the headers at their current position.
		/// </summary>
		public static readonly RoutedCommand FreezeHeaders;

		/// <summary>
		/// Freezes the panes of the report in their current position.
		/// </summary>
		public static readonly RoutedCommand FreezePanes;

		/// <summary>
		/// Mark the element as having been read.
		/// </summary>
        public static readonly RoutedCommand MarkAsRead;

		/// <summary>
		/// Mark the element has not having been read.
		/// </summary>
        public static readonly RoutedCommand MarkAsUnread;

		/// <summary>
		/// What?
		/// </summary>
        public static readonly RoutedCommand MaximizeRestore;

		/// <summary>
		/// Changes the authentication used for the current user.
		/// </summary>
        public static readonly RoutedCommand Login;

		/// <summary>
		/// Accepts the current action and dismisses the prompt.
		/// </summary>
		public static readonly RoutedCommand OK;

		/// <summary>
		/// Create a preview of the report before it is printed.
		/// </summary>
		public static readonly RoutedCommand PrintPreview;

		/// <summary>
		/// Reset the application parameters to their factory defaults.
		/// </summary>
		public static readonly RoutedCommand ResetSettings;

		/// <summary>
		/// Resync Data 
		/// </summary>
		public static readonly RoutedCommand ResyncData;

		/// <summary>
		/// Select one or more columns in the report.
		/// </summary>
        public static readonly RoutedCommand SelectColumns;

		/// <summary>
		/// Set the animation to the fastest setting.
		/// </summary>
        public static readonly RoutedCommand SetAnimationFast;

		/// <summary>
		/// Set the animation to the medium setting.
		/// </summary>
		public static readonly RoutedCommand SetAnimationMedium;

		/// <summary>
		/// Turn off the animation effects.
		/// </summary>
		public static readonly RoutedCommand SetAnimationOff;

		/// <summary>
		/// Set the animation effects to the slowest settings.
		/// </summary>
		public static readonly RoutedCommand SetAnimationSlow;

		/// <summary>
		/// Set the Frame.
		/// </summary>
		public static readonly RoutedCommand SetFrame;

		/// <summary>
		/// Enables the navigation pane.
		/// </summary>
		public static readonly RoutedCommand SetIsNavigationPaneVisible;

		/// <summary>
		/// Sorts the report using a selected column.
		/// </summary>
		public static readonly RoutedCommand SortReport;

		/// <summary>
		/// Shows a window in the report.
		/// </summary>
		public static readonly RoutedCommand ReportShowWindow;

		/// <summary>
		/// Create the static resources used by this test module.
		/// </summary>
		static FluidTradeCommands()
		{

			// Routed Commands
			FluidTradeCommands.Apply = new RoutedCommand("Apply", typeof(FluidTradeCommands));
			FluidTradeCommands.Cancel = new RoutedCommand("Cancel", typeof(FluidTradeCommands));			
			FluidTradeCommands.FreezeHeaders = new RoutedCommand("FreezeHeaders", typeof(FluidTradeCommands));
			FluidTradeCommands.FreezePanes = new RoutedCommand("FreezePanes", typeof(FluidTradeCommands));
			FluidTradeCommands.Login = new RoutedCommand("Login", typeof(FluidTradeCommands));
            FluidTradeCommands.MarkAsRead = new RoutedCommand("MarkAsRead", typeof(FluidTradeCommands));
            FluidTradeCommands.MarkAsUnread = new RoutedCommand("MarkAsUnread", typeof(FluidTradeCommands));
            FluidTradeCommands.MaximizeRestore = new RoutedCommand("MaximizeRestore", typeof(FluidTradeCommands));
			FluidTradeCommands.OK = new RoutedCommand("OK", typeof(FluidTradeCommands));
			FluidTradeCommands.PrintPreview = new RoutedCommand("PrintPreview", typeof(FluidTradeCommands));
			FluidTradeCommands.ResetSettings = new RoutedCommand("ResetSettings", typeof(FluidTradeCommands));
			FluidTradeCommands.ResyncData = new RoutedCommand("ResyncData", typeof(FluidTradeCommands));			
            FluidTradeCommands.SelectColumns = new RoutedCommand("SelectColumns", typeof(FluidTradeCommands));
            FluidTradeCommands.SetAnimationFast = new RoutedCommand("SetAnimationFast", typeof(FluidTradeCommands));
			FluidTradeCommands.SetAnimationMedium = new RoutedCommand("SetAnimationMedium", typeof(FluidTradeCommands));
			FluidTradeCommands.SetAnimationOff = new RoutedCommand("SetAnimationOff", typeof(FluidTradeCommands));
			FluidTradeCommands.SetAnimationSlow = new RoutedCommand("SetAnimationSlow", typeof(FluidTradeCommands));
			FluidTradeCommands.SetFrame = new RoutedCommand("SetFrame", typeof(FluidTradeCommands));
			FluidTradeCommands.SetIsNavigationPaneVisible = new RoutedCommand("SetIsNavigationPaneVisible", typeof(FluidTradeCommands));
			FluidTradeCommands.SortReport = new RoutedCommand("SortReport", typeof(FluidTradeCommands));
			FluidTradeCommands.ReportShowWindow = new RoutedCommand("ReportShowWindow", typeof(FluidTradeCommands));

            // Global Key Gestures
            FluidTradeCommands.MarkAsRead.InputGestures.Add(new KeyGesture(Key.Q, ModifierKeys.Control));
            FluidTradeCommands.MarkAsUnread.InputGestures.Add(new KeyGesture(Key.U, ModifierKeys.Control));
            FluidTradeCommands.MaximizeRestore.InputGestures.Add(new KeyGesture(Key.F11));

		}

	}

}
