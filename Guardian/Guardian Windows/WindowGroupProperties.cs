namespace FluidTrade.Guardian.Windows
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using FluidTrade.Guardian.Windows.Controls;
	using System.Windows;
	using System.Windows.Data;
	using System.Data;
	using FluidTrade.Core;
	using System.Collections.Specialized;

	/// <summary>
	/// Properties window for Group objects.
	/// </summary>
	public class WindowGroupProperties : WindowEntityProperties
	{

		/// <summary>
		/// Identifies the Group dependency property.
		/// </summary>
		public static readonly DependencyProperty GroupProperty =
			DependencyProperty.Register("Group", typeof(Group), typeof(WindowGroupProperties), new PropertyMetadata(null));

		private RightsHolderListBox groups = new RightsHolderListBox();

		
		// Background Fields:
		private Guid entityId;

        /// <summary>
        /// Create a new properties dialog box.
        /// </summary>
		public WindowGroupProperties()
            : base()
        {

			this.SetBinding(WindowGroupProperties.GroupProperty,
				new Binding("Entity") { Source = this, Converter = new IdentityConverter() });
			this.SetBinding(WindowGroupProperties.DataContextProperty, new Binding("Group") { Source = this });

			this.BuildMembersTab();

			this.Loaded += delegate(object sender, RoutedEventArgs eventArgs)
			{
				//this.groups.ItemsSource = (this.Entity as User).Groups;
				DataModel.RightsHolder.RowChanging += this.FilterRow;
				DataModel.Group.RowChanging += this.FilterRow;
			};
			this.Unloaded += delegate(object sender, RoutedEventArgs eventArgs)
			{
				DataModel.RightsHolder.RowChanging -= this.FilterRow;
				DataModel.Group.RowChanging -= this.FilterRow;
			};

        }

		/// <summary>
		/// The user whose properties we will display.
		/// </summary>
		public Group Group
		{

			get { return this.GetValue(WindowGroupProperties.GroupProperty) as Group; }
			set { this.SetValue(WindowGroupProperties.GroupProperty, value); }

		}

		/// <summary>
		/// Create the tab containing the groups this user is a member of.
		/// </summary>
		private void BuildMembersTab()
		{

			GroupMembersTab memberOf = new GroupMembersTab() { Owner = this };

			memberOf.SetBinding(GroupMembersTab.GroupProperty, new Binding("Group") { Source = this });
			this.tabControl.Items.Add(memberOf);

		}

		/// <summary>
		/// Determine whether a row is one of the rows being displayed by the window.
		/// </summary>
		/// <param name="sender">The table that sent the event.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void FilterRow(object sender, EventArgs eventArgs)
		{

			DataRow row = eventArgs is DataRowChangeEventArgs ? (eventArgs as DataRowChangeEventArgs).Row : (eventArgs as DataTableNewRowEventArgs).Row;

			if (row is GroupRow && (row as GroupRow).GroupId == this.entityId)
				this.MustRedisplay = true;
			else if (row is RightsHolderRow && (row as RightsHolderRow).RightsHolderId == this.entityId)
				this.MustRedisplay = true;

		}

		/// <summary>
		/// Handle the Entity changing.
		/// </summary>
		protected override void OnEntityChanged()
		{

			base.OnEntityChanged();

			ThreadPoolHelper.QueueUserWorkItem(delegate(object data)
			{
				lock (DataModel.SyncRoot)
					this.entityId = (Guid)data;
			},
				this.Entity.EntityId);

		}

	}

}
