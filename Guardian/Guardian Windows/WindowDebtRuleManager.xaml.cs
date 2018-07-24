namespace FluidTrade.Guardian.Windows
{

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Transactions;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using FluidTrade.Guardian.Windows;
    using FluidTrade.Guardian;
	using System.Windows.Input;
	using System.Windows.Threading;
	using FluidTrade.Core;
	using System.ServiceModel.Security;

    /// <summary>
    /// Interaction logic for WindowDebtRuleManager.xaml
    /// </summary>
    public partial class WindowDebtRuleManager : Window
    {

        /// <summary>
        /// Identifies the CanApply dependency property.
        /// </summary>
        public static DependencyProperty CanApplyProperty =
            DependencyProperty.Register("CanApply", typeof(Boolean), typeof(WindowDebtRuleManager), new PropertyMetadata(false));
        /// <summary>
        /// Identifies the DebtRule dependency property.
        /// </summary>
        public static DependencyProperty DebtRuleProperty =
            DependencyProperty.Register("DebtRule", typeof(DebtRule), typeof(WindowDebtRuleManager),
                new PropertyMetadata(null, WindowDebtRuleManager.OnDebtRuleChanged));

		private Entity entity = null;
        private Boolean populating = false;
        private delegate void populateRules(List<DebtRule> rules);
		private delegate void finishApply(DebtRule rule, Exception exception);
		// This member is strictly for the DataModel thread.
		private Guid entityId;

        /// <summary>
        /// Create a new debt rule manager.
        /// </summary>
        public WindowDebtRuleManager()
        {

            InitializeComponent();
			this.Loaded += this.OnLoaded;
            this.Unloaded += (s, e) => DataModel.EndMerge -= this.OnEndMerge;

        }

		/// <summary>
		/// True if there are changes that can be 'applied', false otherwise.
		/// </summary>
		public Boolean CanApply
		{

			get { return (Boolean)this.GetValue(WindowDebtRuleManager.CanApplyProperty); }
			set
			{

				if (!this.populating)
					this.SetValue(WindowDebtRuleManager.CanApplyProperty, value);

			}

		}

		/// <summary>
		/// Get or set the currently selected DebtRule.
		/// </summary>
		public DebtRule DebtRule
		{

			get { return this.GetValue(WindowDebtRuleManager.DebtRuleProperty) as DebtRule; }
			set { this.SetValue(WindowDebtRuleManager.DebtRuleProperty, value); }

		}

		/// <summary>
		/// The entity new rules and rule updates will be attached to.
		/// </summary>
		public Entity Entity
		{

			get { return this.entity; }
			set
			{

				if (this.entity == null)
				{

					this.entityId = value.EntityId;
					this.entity = value;

				}
				else
				{

					throw new Exception("Entity can only be set once");

				}

			}

		}

        /// <summary>
        /// Update rules that have been modified, destroy rules that have been deleted, and create rules that have been created.
        /// </summary>
		/// <paraparam name="finish">The method to invoke committing is finished.</paraparam>
		/// <paraparam name="rules">The list of rules to commit.</paraparam>
		private void Apply(finishApply finish, DebtRule[] rules)
        {

			Exception exception = null;
			DebtRule currentRule = null;

			// We may change some debt rules here, so we need to set populating. Otherwise, changing a DebtRule will raise a property changed event,
			// triggering a change in the CanApply state, which is owned by another thread.
			this.populating = true;

			try
			{

				foreach (DebtRule rule in rules)
					if (rule.Modified || rule.Delete)
					{

						currentRule = rule;

						if (rule.DebtRuleId != null)
						{

							Guid ruleOwner = Guid.Empty;
							String oldName = null;

							lock (DataModel.SyncRoot)
							{

								ruleOwner = this.GetRuleOwner(rule.DebtRuleId.Value);
								oldName = DataModel.DebtRule.DebtRuleKey.Find(rule.DebtRuleId.Value).Name;

							}

							if (ruleOwner != this.Entity.EntityId)
							{

								rule.DebtRuleId = null;

								if (oldName.Equals(rule.Name))
									rule.Name += " - Copy";

							}

						}

						// If we're committing a rule, we have to own it. If, in the future, we're able to edit the rules of our children, this
						// decision will need to get more sophisticated.
						rule.Commit(this.Entity.EntityId);

					}

				currentRule = null;

			}
			catch (Exception e)
			{

				exception = e;

			}

			this.populating = false;

			finish(currentRule, exception);

        }

		/// <summary>
		/// Re-enable the window and reset the cursor. If data is null, also set CanApply to false. If it is not null, it is an exception that
		/// occured in the background that the user should be alerted to.
		/// </summary>
		/// <param name="rule">The rule that triggered the exception (if any).</param>
		/// <param name="exception">Any exception that should be brought to the user's attention.</param>
		private void BackgroundEnable(DebtRule rule, Exception exception)
		{

			this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate()
				{

					if (exception != null)
						if (exception is DebtRuleInUseException)
							MessageBox.Show(
								this,
								String.Format(Properties.Resources.DeleteFailedDebtRuleInUse, rule.Name),
								this.Title);
						else if (exception is SecurityAccessDeniedException)
							MessageBox.Show(
								this,
								String.Format(Properties.Resources.DeleteFailedAccessDenied, rule.Name),
								this.Title);
						else
							MessageBox.Show(this, Properties.Resources.OperationFailed, this.Title);
					else
						this.CanApply = false;

					this.Cursor = Cursors.Arrow;

				}));

		}

		/// <summary>
		/// Close the window and reset the cursor. If data is not null, it is an exception that occured in the background that the user should be
		/// alerted to. In this case, the window will not be closed so the user may try again.
		/// </summary>
		/// <param name="rule">The rule that triggered the exception (if any).</param>
		/// <param name="exception">Any exception that should be brought to the user's attention.</param>
		private void BackgroundClose(DebtRule rule, Exception exception)
		{

			this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate()
			{

				if (exception != null)
					if (exception is DebtRuleInUseException)
						MessageBox.Show(
							this,
							String.Format(Properties.Resources.DeleteFailedDebtRuleInUse, rule.Name),
							this.Title);
					else if (exception is SecurityAccessDeniedException)
						MessageBox.Show(
							this,
							String.Format(Properties.Resources.DeleteFailedAccessDenied, rule.Name),
							this.Title);
					else
						MessageBox.Show(this, Properties.Resources.OperationFailed, this.Title);
				else
					this.Close();

				this.Cursor = Cursors.Arrow;

			}));

		}

        /// <summary>
        /// Find the owner of a rule. If there isn't one, return Guid.Empty.
        /// </summary>
		/// <param name="ruleId">The DebtRuleId of the rule to get the owner of.</param>
        public Guid GetRuleOwner(Guid ruleId)
        {

            DebtRuleRow rule = DataModel.DebtRule.DebtRuleKey.Find(ruleId);

            if (rule != null)
                return GetRuleOwner(rule);
            else
                return Guid.Empty;

        }

        /// <summary>
        /// Find the owner of a rule. If there isn't one, return null.
        /// </summary>
		/// <param name="rule">The debt rule to find the owner of, or Guid.Empty if no proper owner can be found.</param>
        public Guid GetRuleOwner(DebtRuleRow rule)
        {

			Guid debtClassId = Guid.Empty;

			if (rule != null)
			{
				DebtClassRow debtClass = null;
				DebtRuleMapRow map = DataModel.DebtRuleMap.FirstOrDefault(row => row.DebtRuleId == rule.DebtRuleId);

				if (map != null)
				{

					 debtClass = DataModel.DebtClass.DebtClassKey.Find(map.DebtClassId);
					 debtClassId = debtClass.DebtClassId;

				}

			}

            return debtClassId;

        }

        /// <summary>
        /// Handling the apply button being clicked.
        /// </summary>
        /// <param name="sender">The apply button.</param>
        /// <param name="e">The event arguments.</param>
        private void OnApply(object sender, RoutedEventArgs e)
        {

			if (this.CanApply)
			{

				DebtRule[] rules = new DebtRule[this.rules.Items.Count];

				this.Cursor = Cursors.Wait;
				this.rules.Items.CopyTo(rules, 0);

				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(data => this.Apply(new finishApply(this.BackgroundEnable), rules));

			}

        }

        /// <summary>
        /// Handle the close button being clicked. Disgard any changes and close the window.
        /// </summary>
        /// <param name="sender">The cancel button.</param>
        /// <param name="e">The event arguments.</param>
        private void OnCancel(object sender, RoutedEventArgs e)
        {

            this.Close();

        }

        /// <summary>
        /// Handle the debt rule changing. Enable the apply button.
        /// </summary>
        /// <param name="sender">The DebtRule object.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void OnDebtRuleChanged(object sender, EventArgs eventArgs)
        {

            this.CanApply = true;

        }

        /// <summary>
        /// Handle the DebtRule changing. Register with DebtRule property change events and populate the window.
        /// </summary>
        /// <param name="sender">The debt rule manager.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private static void OnDebtRuleChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
        {

            WindowDebtRuleManager manager = sender as WindowDebtRuleManager;

            if (manager.DebtRule != null)
            {

                if (eventArgs.OldValue is DebtRule)
                    (eventArgs.OldValue as DebtRule).PropertyChanged -= manager.OnDebtRuleChanged;

                if (manager.DebtRule != null)
                {

                    manager.DebtRule.PropertyChanged += manager.OnDebtRuleChanged;
                    FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(manager.Populate, manager.DebtRule.DebtRuleId);

                }

            }

        }

        /// <summary>
        /// Handle the delete-this-rule check box being clicked. Mark the current rule as deleted.
        /// </summary>
        /// <param name="sender">The delete check box.</param>
        /// <param name="e">The event arguments.</param>
        private void OnDelete(object sender, RoutedEventArgs e)
        {

            this.DebtRule.Delete = (Boolean)this.delete.IsChecked;
			this.edit.IsEnabled = !this.DebtRule.Delete;

        }

		/// <summary>
		/// Handle the Loaded event. Queue the rule retrieval and lock the size of the window.
		/// </summary>
		/// <param name="sender">The rules manager.</param>
		/// <param name="eventArgs">The event arguments.</param>
        private void OnLoaded(object sender, RoutedEventArgs eventArgs)
        {

			if (this.Entity == null)
				throw new ArgumentNullException("Entity");

			DataModel.EndMerge += this.OnEndMerge;
			this.SizeToContent = SizeToContent.Manual;
			FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(this.RetrieveRules, this.Entity.EntityId);

        }

        /// <summary>
        /// Handle the create button being clicked. Add a new DebtRule to the rules combobox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnNew(object sender, EventArgs e)
        {

			DebtRule rule = new DebtRule() { Name = "New Debt Rule" };

			this.rules.Items.Add(rule);
			this.DebtRule = rule;
			this.CanApply = true;

        }

        /// <summary>
        /// Handle the okay button being clicked. Apply any changes and close the window.
        /// </summary>
        /// <param name="sender">The okay button.</param>
        /// <param name="e">The event arguments.</param>
        private void OnOkay(object sender, RoutedEventArgs e)
        {

			if (this.CanApply)
			{

				DebtRule[] rules = new DebtRule[this.rules.Items.Count];

				this.Cursor = Cursors.Wait;
				this.rules.Items.CopyTo(rules, 0);
				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(data => this.Apply(new finishApply(this.BackgroundClose), rules));

			}
			else
			{

				this.Close();

			}

        }

        /// <summary>
        /// Handle the currently selected debt-rule changing. Reset the other fields to match that debt rule.
        /// </summary>
        /// <param name="sender">The rules combobox</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void OnRulesSelectionChanged(object sender, SelectionChangedEventArgs eventArgs)
        {

            if (this.DebtRule != null)
            {

                foreach (DebtRule rule in eventArgs.RemovedItems)
                    rule.PropertyChanged -= this.OnDebtRuleChanged;

                if (this.DebtRule != null)
                {

					this.DebtRule.PropertyChanged += this.OnDebtRuleChanged;
					FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(this.Populate, this.DebtRule.DebtRuleId);

                }

            }

        }

		/// <summary>
		/// Determine who owns the current rule and start-off the real populate.
		/// </summary>
		/// <param name="data">State data - ignored.</param>
		private void Populate(object data)
		{

			Guid? debtRule = (Guid?)data;
			Guid owner = Guid.Empty;

			if (debtRule != null)
				lock (DataModel.SyncRoot)
					owner = this.GetRuleOwner(DataModel.DebtRule.DebtRuleKey.Find(debtRule.Value));

			this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => Populate(owner == this.Entity.EntityId)));

		}

        /// <summary>
        /// Populate the tab item's controls with information from a debt rule.
        /// </summary>
		/// <param name="canDelete">Whether debt rule can be deleted (from the current entity).</param>
        private void Populate(Boolean canDelete)
        {

            if (this.DebtRule != null)
            {

				this.populating = true;
                this.delete.IsChecked = this.DebtRule.Delete;
				this.delete.IsEnabled = canDelete;
				this.edit.IsEnabled = !this.DebtRule.Delete;
				this.populating = false;

            }
            else
            {

                this.delete.IsChecked = false;
                this.delete.IsEnabled = false;

            }

        }

		/// <summary>
		/// Populate the rules drop-down with a list of rules.
		/// </summary>
		/// <param name="rules">The list of rules.</param>
		void PopulateRules(List<DebtRule> rules)
		{

			DebtRule selected = this.DebtRule;
			DebtRule newSelected = null;
			Int64 oldVersion = selected == null ? 0 : selected.RowVersion;

			this.populating = true;

			for (int index = 0; index < this.rules.Items.Count; index += 1)
			{

				DebtRule rule = this.rules.Items[index] as DebtRule;

				if (rules.Contains(rule))
				{

					if (rule.Equals(selected))
						newSelected = rule;
					rule.Update(rules.FirstOrDefault((r) => r.Equals(rule)));
					rules.Remove(rule);

				}
				else if (!rule.New)
				{

					this.rules.Items.Remove(rule);
					index -= 1;

				}

			}
			foreach (DebtRule rule in rules)
				this.rules.Items.Add(rule);

			if (selected == null || !this.rules.Items.Contains(selected))
			{

				this.rules.SelectedItem = null;
				this.rules.SelectedIndex = 0;

			}
			else if (newSelected!= null && oldVersion != newSelected.RowVersion)
			{

				this.rules.SelectedItem = null;
				this.rules.SelectedValue = selected.DebtRuleId;

			}

			this.populating = false;

		}

        /// <summary>
        /// Retrieve the debt rules from the database and fill in the rules combo box.
        /// </summary>
        /// <param name="sender">The originator of the event.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void OnEndMerge(Object sender, EventArgs eventArgs)
        {

            FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(this.RetrieveRules, this.entityId);

        }

        /// <summary>
        /// Retrieve the debt rules from the database and fill in the rules combo box.
        /// </summary>
        private void RetrieveRules(Guid entityId)
        {

            try
            {

                List<DebtRule> rules = new List<DebtRule>();

                lock (DataModel.SyncRoot)
                {

					DebtClassRow debtClassRow = DataModel.DebtClass.DebtClassKey.Find(entityId);

                    // Crawl up the tree to find all of the debt class ancestors of this debt class.
                    while (debtClassRow != null)
                    {

                        EntityTreeRow parentRelationship = DataModel.EntityTree.FirstOrDefault(row => row.ChildId == debtClassRow.DebtClassId);
                        var maps = DataModel.DebtRuleMap.Where(row => row.DebtClassId == debtClassRow.DebtClassId);

                        // Add all the rules owned by debtClass.
                        foreach (DebtRuleMapRow map in maps)
                        {

                            // In the near future, this should check for IsNameNull.
                            if (map.DebtRuleRow.Name != "")
                                rules.Add(new DebtRule(map.DebtRuleRow));

                        }

                        debtClassRow = null;

                        if (parentRelationship != null)
                            debtClassRow = DataModel.DebtClass.DebtClassKey.Find(parentRelationship.ParentId);

                    }

                }

				this.Dispatcher.BeginInvoke(new populateRules(this.PopulateRules), DispatcherPriority.Normal, rules);

            }
            catch
            {
            }

        }

        /// <summary>
        /// Initialize the rule list.
        /// </summary>
        /// <param name="data">Ignored.</param>
        private void RetrieveRules(object data)
        {

            this.RetrieveRules((Guid)data);

        }

    }

}
