namespace FluidTrade.Guardian.Windows
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Windows;
	using System.Windows.Data;
	using System.Windows.Input;
	using System.Windows.Threading;
	using FluidTrade.Guardian.Windows.Controls;
	using FluidTrade.Guardian.Utilities;
	using System.Windows.Controls;
	using System.Data;
	using FluidTrade.Core;

    /// <summary>
    /// Properties for any debt class.
    /// </summary>
    public class WindowDebtClassProperties : WindowBlotterProperties
    {

		/// <summary>
		/// Command sent when the "Manage Rules" button is clicked.
		/// </summary>
		public static readonly RoutedUICommand ManageRules = new RoutedUICommand("ManageRules", "Manage Rules...", typeof(WindowDebtClassProperties));

        // Private Instance Fields
		private ControlDebtRule debtRuleTab = new ControlDebtRule();
		private CustomizeDebtClassControl customize = new CustomizeDebtClassControl();
        private ComboBox debtRule = new ComboBox();

		// Background Fields:
		private Guid entityId;
		private Guid? debtRuleId;

        private delegate void populate(Boolean enabled, List<DebtRule> rules, DebtRule currentRule, Boolean inherited);

        /// <summary>
        /// Create a new properties dialog box.
        /// </summary>
        public WindowDebtClassProperties() : base()
        {

			this.CommandBindings.Add(
				new CommandBinding(
					WindowDebtClassProperties.ManageRules,
					this.OnManageRules,
					(s,e) => e.CanExecute = !this.IsEntityDeleted));
			this.BuildDebtRuleTab();
			this.BuildDetailTab();
			this.BuildSettlementTemplateBox();

			this.Loaded += delegate(object sender, RoutedEventArgs eventArgs)
			{
				DataModel.DebtClass.RowChanging += this.FilterRow;
				DataModel.DebtRule.RowChanging += this.FilterRow;
			};
			this.Unloaded += delegate(object sender, RoutedEventArgs eventArgs)
			{
				DataModel.DebtClass.RowChanging -= this.FilterRow;
				DataModel.DebtRule.RowChanging -= this.FilterRow;
			};

        }

		/// <summary>
		/// The customize control.
		/// </summary>
		protected CustomizeDebtClassControl Customize
		{
			get { return this.customize; }
		}

		/// <summary>
		/// Create the CustomizeDebtClassControl and add it to the Customize tab.
		/// </summary>
		private void BuildDetailTab()
		{

			TabItem tab = new TabItem() { Header = "Detail", FontSize = 11 };
			Border border = new Border { Padding = new Thickness(10) };

			this.customize.HorizontalAlignment = HorizontalAlignment.Stretch;
			border.HorizontalAlignment = HorizontalAlignment.Stretch;
			border.Child = this.customize;
			tab.Content = border;
			this.tabControl.Items.Add(tab);
			this.customize.SetBinding(
				CustomizeDebtClassControl.DebtClassProperty,
				new Binding("Entity") { Source = this, Converter = this.Resources["entityIdentity"] as IValueConverter });
			
		}

		/// <summary>
		/// Construct the Debt Rule tab and add it to the dialog box.
		/// </summary>
		private void BuildDebtRuleTab()
		{

			TabItem debtRule = new TabItem() { Header = "Debt Rule", FontSize = 11 };
			Border border = new Border { Padding = new Thickness(10) };
			Grid selectRulePanel = new Grid();
			StackPanel outerPanel = new StackPanel();
			Button manageRules = new Button()
			{
				Content = "Manage Rules...",
				Command = WindowDebtClassProperties.ManageRules,
				HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
				VerticalAlignment = System.Windows.VerticalAlignment.Center,
				MinHeight = 23,
				MinWidth = 85,
				Margin = new Thickness(5, 0, 0, 0)
			};

			this.debtRule.IsEditable = true;
			this.debtRule.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
			this.debtRule.SetValue(Grid.ColumnProperty, 0);
			this.debtRule.DisplayMemberPath = "Name";
			this.debtRule.SelectedValuePath = "DebtRuleId";
			this.debtRule.SetBinding(ComboBox.SelectedValueProperty, new Binding("DebtRuleId") { Mode = BindingMode.TwoWay });
			this.debtRule.SetBinding(ComboBox.SelectedItemProperty, new Binding("DebtRule") { Source = this.debtRuleTab });

			selectRulePanel.Children.Add(this.debtRule);
			manageRules.SetValue(Grid.ColumnProperty, 1);
			selectRulePanel.Children.Add(manageRules);
			selectRulePanel.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
			selectRulePanel.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
			outerPanel.Children.Add(selectRulePanel);
			outerPanel.Children.Add(new Separator() { Margin = new Thickness(0, 7, 0, 5) });
			outerPanel.Children.Add(this.debtRuleTab);
			border.Child = outerPanel;
			debtRule.Content = border;
			this.tabControl.Items.Add(debtRule);

		}

		/// <summary>
		/// Build the interface for interacting with settlement templates.
		/// </summary>
		private void BuildSettlementTemplateBox()
		{

			EditSettlementTemplateControl templateControl = new EditSettlementTemplateControl();
			GroupBox groupBox = new GroupBox() { Header = "Settlement Letter Template", Padding = new Thickness(5) };

			groupBox.Content = templateControl;
			this.customizePanel.Children.Add(groupBox);
			templateControl.SetBinding(
				EditSettlementTemplateControl.DebtClassProperty,
				new Binding("Entity") { Source = this, Converter = this.Resources["entityIdentity"] as IValueConverter });

		}

		/// <summary>
		/// Determine whether a row is one of the rows being displayed by the window.
		/// </summary>
		/// <param name="sender">The table that sent the event.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void FilterRow(object sender, EventArgs eventArgs)
		{

			DataRow row = eventArgs is DataRowChangeEventArgs ? (eventArgs as DataRowChangeEventArgs).Row : (eventArgs as DataTableNewRowEventArgs).Row;

			if (!(row.RowState == DataRowState.Detached || row.RowState == DataRowState.Deleted))
				if (row is DebtClassRow && (row as DebtClassRow).DebtClassId == this.entityId)
					this.MustRedisplay = true;
				else if (row is DebtRuleRow)
					this.MustRedisplay = true;

		}

        /// <summary>
        /// Get a list of the rules available to this debt class.
        /// </summary>
        /// <returns>A list of the available rules.</returns>
        private List<DebtRule> GetAvailableRules(Entity entity)
        {

            List<DebtRule> rules = new List<DebtRule>();
            DebtClassRow debtClass = DataModel.DebtClass.DebtClassKey.Find(entity.EntityId);
			DebtClassRow parent = this.RetrieveParent(entity.EntityId);

			if (parent != null)
			{

				DebtRule inherited = DebtClass.GetDebtRule(parent.DebtClassId, entity.TypeId);

				inherited.Name = "<inherit from parent>";
				inherited.DebtRuleId = null;
				rules.Add(inherited);

			}

            while (debtClass != null)
            {

                var maps = DataModel.DebtRuleMap.Where(row => row.DebtClassId == debtClass.DebtClassId);

                foreach (DebtRuleMapRow map in maps)
                {

                    if (map.DebtRuleRow.Name != "")
                        rules.Add(new DebtRule(map.DebtRuleRow));

                }

				debtClass = this.RetrieveParent(debtClass.DebtClassId);

            }

            return rules;

        }

        /// <summary>
        /// Check for a parent parent of same entity type as this entity.
        /// </summary>
        /// <returns>True if a parent was found - false otherwise.</returns>
		private DebtClassRow RetrieveParent(Guid entityId)
        {

			EntityRow entityRow = DataModel.Entity.EntityKey.Find(entityId);
			EntityTreeRow[] entityTreeRows = entityRow.GetEntityTreeRowsByFK_Entity_EntityTree_ChildId();

			foreach (EntityTreeRow entityTreeRow in entityTreeRows)
				if (entityTreeRow.EntityRowByFK_Entity_EntityTree_ParentId.TypeId == entityRow.TypeId)
					return DataModel.DebtClass.DebtClassKey.Find(entityTreeRows[0].ParentId);

			return null;

        }

		/// <summary>
		/// Change data contexts when the entity changes.
		/// </summary>
		protected override void OnEntityChanged()
		{

			Guid entityId = this.Entity.EntityId;
			Guid? debtRuleId = (this.Entity as DebtClass).DebtRuleId;

			base.OnEntityChanged();

			ThreadPoolHelper.QueueUserWorkItem(delegate(object data)
				{
					lock(DataModel.SyncRoot)
					{

						this.entityId = entityId;
						this.debtRuleId = debtRuleId;

					}
				});

		}

        /// <summary>
        /// Open the rules manager.
        /// </summary>
        /// <param name="sender">The manage rules button.</param>
		/// <param name="eventArgs">The event arguments.</param>
        private void OnManageRules(object sender, RoutedEventArgs eventArgs)
        {

            WindowDebtRuleManager manager = new WindowDebtRuleManager();
            manager.Owner = this;
            manager.Entity = this.Entity;
            //manager.DebtRule = this.debtRule.SelectedItem as DebtRule;
			manager.Closed += this.OnRuleManagerClosed;
            manager.Show();
			Win32Interop.DisableWindow(this);

        }

		/// <summary>
		/// When the rule manager closes, re-activate the properties window.
		/// </summary>
		/// <param name="sender">The rule manager.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnRuleManagerClosed(object sender, EventArgs eventArgs)
		{

			Win32Interop.EnableWindow(this);
			this.Activate();

		}

        /// <summary>
        /// Populate the debt rule information in the debt rule tab.
        /// </summary>
        protected override void Populate(Entity entity)
        {

			base.Populate(entity);

            try
            {

                lock (DataModel.SyncRoot)
                {

					DebtClass debtClass = entity as DebtClass;
					List<DebtRule> rules = null;
					DebtRule currentRule = null;
					Boolean enabled = true;
					Boolean inherited = true;

					// Only populate the window if the entity still exists. (If it doesn't, it's been deleted since the window was opened.)
                    if (entity != null)
                    {

						rules = GetAvailableRules(entity);
                        currentRule = debtClass.GetDebtRule();
						inherited = debtClass.DebtRuleId == null;

                    }
                    else
                    {

                        enabled = false;

                    }

					this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new populate(this.Populate), enabled, rules, currentRule, inherited);

                }

            }
            catch
            {


            }

        }

        /// <summary>
        /// Populate the drop-down list of rules and the debt rule display. If possible, keep the selection.
        /// </summary>
		/// <param name="enabled">Whether the window is enabled.</param>
        /// <param name="rules">The rules to populate the drop-down with.</param>
        /// <param name="currentRule">The currently selected rule.</param>
		/// <param name="inherited">Whether the selected debt rule is inherited from a parent entity.</param>
        private void Populate(Boolean enabled, List<DebtRule> rules, DebtRule currentRule, Boolean inherited)
        {

            DebtRule oldRule = this.debtRule.SelectedItem as DebtRule;

            this.Populating = true;

            if (enabled)
            {

                this.debtRule.ItemsSource = rules;

                if (oldRule != null && rules.Contains(oldRule))
                {

                    this.debtRule.SelectedItem = null;
                    this.debtRule.SelectedValue = oldRule.DebtRuleId;

                }
				else if (inherited)
				{

					this.debtRule.SelectedItem = null;
					this.debtRule.SelectedIndex = 0;

				}
				else if (currentRule != null)
				{

					this.debtRule.SelectedItem = null;
					this.debtRule.SelectedItem = currentRule;

				}

            }

            this.Populating = false;

        }

    }

}
