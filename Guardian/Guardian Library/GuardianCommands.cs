namespace FluidTrade.Guardian
{

	using System;
	using System.Collections.Generic;
	using System.Windows;
	using System.Windows.Input;

	/// <summary>
	/// The commands used for the Fluid Trade application.
	/// </summary>
	public class GuardianCommands
	{

		/// <summary>
		/// Open the "advanced" display.
		/// </summary>
		public static readonly RoutedUICommand Advanced;

		/// <summary>
		/// Apply the 'Filled' filter to the display.
		/// </summary>
		public static readonly RoutedCommand ApplyFilledFilter;

		/// <summary>
		/// Apply the 'Running' filter to the display.
		/// </summary>
		public static readonly RoutedCommand ApplyRunningFilter;

		/// <summary>
		/// Approve a settlement.
		/// </summary>
		public static readonly RoutedCommand ApproveSettlement;

		
		/// <summary>
		/// Collapse a level of a tree structure.
		/// </summary>
        public static readonly RoutedUICommand Collapse;

		/// <summary>
		/// Copy data into the clipboard.
		/// </summary>
		public static readonly RoutedCommand Copy;

		/// <summary>
		/// Creates a slice from the selected orders and sends them to a destiantion.
		/// </summary>
		public static readonly RoutedCommand CreateSlice;

		/// <summary>
		/// Adds a new organization to an exchange.
		/// </summary>
		public static readonly RoutedCommand CreateOrganization;

		/// <summary>
		/// Cuts data from one place and places it in the clipboard.
		/// </summary>
		public static readonly RoutedCommand Cut;

		/// <summary>
		/// Deletes an object.
		/// </summary>
		public static readonly RoutedCommand Delete;

		/// <summary>
		/// Opens the user accounts manager.
		/// </summary>
		public static readonly RoutedUICommand ManageUserAccounts;

		/// <summary>
		/// Expands the nodes of a tree structure.
		/// </summary>
        public static readonly RoutedUICommand Expand;

		/// <summary>
		/// Explores the selected item.
		/// </summary>
		public static readonly RoutedCommand Explore;

		/// <summary>
		/// Finds something in the application.
		/// </summary>
		public static readonly RoutedUICommand Find;

		/// <summary>
		/// Formats an item.
		/// </summary>
		public static readonly RoutedCommand Format;

		/// <summary>
		/// Goes one level up in a hierarchical structure.
		/// </summary>
		public static readonly RoutedUICommand GoUp;

		/// <summary>
		/// Brings up Help About.
		/// </summary>
		public static readonly RoutedUICommand HelpAbout;

		/// <summary>
		/// Imports data into the application.
		/// </summary>
		public static readonly RoutedUICommand Import;

		/// <summary>
		/// The command executed to add an EntityTree entry to an entity.
		/// </summary>
		public static readonly RoutedUICommand LinkEntity;

		/// <summary>
		/// Loads a report from the XAML source.
		/// </summary>
		public static readonly RoutedUICommand LoadReport;

		/// <summary>
		/// Opens the user accounts manager.
		/// </summary>
		public static readonly RoutedUICommand ManageUsers;

		/// <summary>
		/// Creates a new item.
		/// </summary>
		public static readonly RoutedCommand New;

		/// <summary>
		/// Opens the selected item.
		/// </summary>
		public static readonly RoutedCommand Open;

		/// <summary>
		/// Opens the properties dialog for the selected item.
		/// </summary>
		public static readonly RoutedCommand Properties;

		/// <summary>
		/// Regenerate a settlement.
		/// </summary>
		public static readonly RoutedCommand RegenerateSettlement;

		/// <summary>
		/// Renames the selected item.
		/// </summary>
		public static readonly RoutedCommand Rename;

		/// <summary>
		/// The command sent to reset the selected user's password.
		/// </summary>
		public static readonly RoutedUICommand ResetPassword;

		/// <summary>
		/// Restores the previous version of the selected item.
		/// </summary>
		public static readonly RoutedCommand RestorePreviousVersions;

		/// <summary>
		/// Selects the address bar in the frame.
		/// </summary>
		public static readonly RoutedUICommand SelectAddressBar;

		/// <summary>
		/// Sends an item to a given destination.
		/// </summary>
		public static readonly RoutedUICommand SendTo;

		/// <summary>
		/// The command executed to remove an EntityTree entry to an entity.
		/// </summary>
		public static readonly RoutedUICommand UnlinkEntity;

		/// <summary>
		/// The command executed to set or change the current view.
		/// </summary>
		public static readonly RoutedCommand View;

		/// <summary>
		/// Create the static resources used by this test module.
		/// </summary>
		static GuardianCommands()
		{

			// Routed Commands
			GuardianCommands.Advanced = new RoutedUICommand("Advanced", "Advanced", typeof(GuardianCommands));
			GuardianCommands.ApplyFilledFilter = new RoutedCommand("ApplyFilledFilter", typeof(GuardianCommands));
			GuardianCommands.ApplyRunningFilter = new RoutedCommand("ApplyRunningFilter", typeof(GuardianCommands));
			GuardianCommands.ApproveSettlement = new RoutedCommand("ApproveSettlement", typeof(GuardianCommands));			
			GuardianCommands.Collapse = new RoutedUICommand("Collapse", "Collapse", typeof(GuardianCommands));
			GuardianCommands.Copy = new RoutedCommand("Copy", typeof(GuardianCommands));
			GuardianCommands.CreateSlice = new RoutedCommand("CreateSlice", typeof(GuardianCommands));
			GuardianCommands.CreateOrganization = new RoutedUICommand("Add organization...", "CreateOrganization", typeof(GuardianCommands));
			GuardianCommands.Cut = new RoutedCommand("Cut", typeof(GuardianCommands));
			GuardianCommands.Delete = new RoutedCommand("Delete", typeof(GuardianCommands));
            GuardianCommands.Expand = new RoutedUICommand("Expand", "Expand", typeof(GuardianCommands));
			GuardianCommands.Explore = new RoutedCommand("Explore", typeof(GuardianCommands));
			GuardianCommands.Find = new RoutedUICommand("Find", "Find", typeof(GuardianCommands));
			GuardianCommands.Format = new RoutedCommand("Format", typeof(GuardianCommands));
			GuardianCommands.GoUp = new RoutedUICommand("Go Up", "GoUp", typeof(GuardianCommands));
			GuardianCommands.HelpAbout = new RoutedUICommand("About", "HelpAbout", typeof(GuardianCommands));
			GuardianCommands.Import = new RoutedUICommand("Import", "Import", typeof(GuardianCommands));
			GuardianCommands.LinkEntity = new RoutedUICommand("Link to folder...", "LinkEntity", typeof(GuardianCommands));
			GuardianCommands.UnlinkEntity = new RoutedUICommand("Unlink folder...", "UnlinkEntity", typeof(GuardianCommands));
			GuardianCommands.LoadReport = new RoutedUICommand("Open Report", "LoadReport", typeof(GuardianCommands));
			GuardianCommands.ManageUsers = new RoutedUICommand("User Accounts...", "ManageUsers", typeof(GuardianCommands));
			GuardianCommands.New = new RoutedCommand("New", typeof(GuardianCommands));
            GuardianCommands.Open = new RoutedCommand("Open", typeof(GuardianCommands));
			GuardianCommands.Properties = new RoutedCommand("Properties", typeof(GuardianCommands));
			GuardianCommands.RegenerateSettlement = new RoutedCommand("RegenerateSettlement", typeof(GuardianCommands));
            GuardianCommands.Rename = new RoutedCommand("Rename", typeof(GuardianCommands));
			GuardianCommands.ResetPassword = new RoutedUICommand("Reset Password", "ResetPassword", typeof(GuardianCommands));
			GuardianCommands.RestorePreviousVersions = new RoutedCommand("RestorePrevousVersions", typeof(GuardianCommands));
			GuardianCommands.SelectAddressBar = new RoutedUICommand("Select Address Bar", "SelectAddressBar", typeof(GuardianCommands));
            GuardianCommands.SendTo = new RoutedUICommand("Move to...", "SendTo", typeof(GuardianCommands));
			GuardianCommands.View = new RoutedCommand("View", typeof(GuardianCommands));

            
			// Global Key Gestures
			GuardianCommands.ApplyFilledFilter.InputGestures.Add(new KeyGesture(Key.F, ModifierKeys.Alt));
			GuardianCommands.ApplyRunningFilter.InputGestures.Add(new KeyGesture(Key.R, ModifierKeys.Alt));
			GuardianCommands.CreateSlice.InputGestures.Add(new KeyGesture(Key.E, ModifierKeys.Alt));
            GuardianCommands.Delete.InputGestures.Add(new KeyGesture(Key.Delete));
			//GuardianCommands.Find.InputGestures.Add(new KeyGesture(Key.F, ModifierKeys.Control));
			GuardianCommands.Find.InputGestures.Add(new KeyGesture(Key.F3));
			//GuardianCommands.Find.InputGestures.Add(new KeyGesture(Key.E, ModifierKeys.Control));
			GuardianCommands.GoUp.InputGestures.Add(new KeyGesture(Key.Up, ModifierKeys.Alt));
			GuardianCommands.Properties.InputGestures.Add(new KeyGesture(Key.Enter, ModifierKeys.Alt));
            GuardianCommands.Rename.InputGestures.Add(new KeyGesture(Key.F2));
			GuardianCommands.SelectAddressBar.InputGestures.Add(new KeyGesture(Key.D, ModifierKeys.Alt));

        }

	}

}
