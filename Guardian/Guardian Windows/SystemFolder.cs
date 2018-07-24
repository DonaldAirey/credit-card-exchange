namespace FluidTrade.Guardian.Windows
{

	using System;
	using System.Configuration;
	using FluidTrade.Guardian.TradingSupportReference;
	using FluidTrade.Core;
	using System.ServiceModel;
	using System.Collections.Generic;
	using System.Windows.Controls;
using System.Windows.Input;

    /// <summary>
	/// Summary description for SystemFolder.
	/// </summary>
	public class SystemFolder : Folder
	{

		/// <summary>
		/// The url of the document to display when this folder is selected.
		/// </summary>
		public readonly String Url;

		/// <summary>
		/// Create a new SystemFolder based on an entity row.
		/// </summary>
		/// <param name="entityRow">An entity row in the DataModel.</param>
		public SystemFolder(EntityRow entityRow) : base(entityRow)
		{

			SystemFolderRow systemFolderRow = DataModel.SystemFolder.SystemFolderKey.Find(entityRow.EntityId);

			// Start the application off by opening up the start page.
			string onOpenText = ConfigurationManager.AppSettings["OnOpen"];
			if (onOpenText != null)
			{
				string[] onOpenArguments = onOpenText.Split(new char[] {','});
				this.Url = onOpenArguments[2].Trim();
			}

		}

		/// <summary>
		/// Retrieve the custom menu items for this debt class.
		/// </summary>
		/// <returns>The list of custom menu items.</returns>
		public override List<Control> GetCustomMenuItems()
		{

			List<Control> menuItems = base.GetCustomMenuItems();
			MenuItem linkMenuItem = new MenuItem();
			MenuItem unlinkMenuItem = new MenuItem();

			unlinkMenuItem.Command = GuardianCommands.UnlinkEntity;
			unlinkMenuItem.CommandParameter = this;
			unlinkMenuItem.SetBinding(MenuItem.IsEnabledProperty, "HasItems");
			menuItems.Add(unlinkMenuItem);
			linkMenuItem.Command = GuardianCommands.LinkEntity;
			linkMenuItem.CommandParameter = this;
			menuItems.Add(linkMenuItem);

			return menuItems;

		}

	}

}
