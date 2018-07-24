namespace FluidTrade.Guardian.Windows
{

    using System;
    using System.Linq;
    using System.Threading;
    using System.Transactions;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Collections.Generic;
	using System.Windows.Data;

    /// <summary>
    /// Interaction logic for ControlEditDebtRule.xaml
    /// </summary>
    public partial class ControlEditDebtRule : UserControl
    {

        /// <summary>
        /// Dependency property for the DebtRule property.
        /// </summary>
		public static readonly DependencyProperty DebtRuleProperty =
			DependencyProperty.Register("DebtRule", typeof(DebtRule), typeof(ControlEditDebtRule),
				new PropertyMetadata(ControlEditDebtRule.OnDebtRuleChanged));
		/// <summary>
		/// Dependency property for the IsReadOnly property.
		/// </summary>
		public static readonly DependencyProperty IsReadOnlyProperty =
			DependencyProperty.Register("IsReadOnly", typeof(Boolean), typeof(ControlEditDebtRule));
		/// <summary>
		/// Dependency property for the SharedSizeGroup property.
		/// </summary>
		public static readonly DependencyProperty SharedSizeGroupProperty =
			DependencyProperty.Register("SharedSizeGroup", typeof(string), typeof(ControlEditDebtRule));

        private Boolean populating = false;

        /// <summary>
        /// Initialize the DebtRule editor.
        /// </summary>
        public ControlEditDebtRule()
        {
        
            InitializeComponent();
			this.SetBinding(ControlEditDebtRule.DataContextProperty, new Binding("DebtRule") { Source = this });

        }

        /// <summary>
        /// The debt rule currently being edited.
        /// </summary>
        public DebtRule DebtRule
        {

            get { return this.GetValue(ControlEditDebtRule.DebtRuleProperty) as DebtRule; }
            set { this.SetValue(ControlEditDebtRule.DebtRuleProperty, value); }

        }

		/// <summary>
		/// Get or set whether the control is read-only. If set, the child controls' IsReadOnly is set.
		/// </summary>
		public Boolean IsReadOnly
		{

			get { return (Boolean)this.GetValue(ControlEditDebtRule.IsReadOnlyProperty); }
			set { this.SetValue(ControlEditDebtRule.IsReadOnlyProperty, value); }

		}

		/// <summary>
		/// Handle the changing of the auto-settle flag.
		/// </summary>
		/// <param name="sender">The auto-settle checkbox.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnAutoSettleChanged(object sender, RoutedEventArgs eventArgs)
		{

			if (!this.populating)
				this.DebtRule.IsAutoSettled = (Boolean)this.autoSettle.IsChecked;

		}

		/// <summary>
		/// Handle the debt rule changing. We'll need to repopulate the controls.
		/// </summary>
		/// <param name="sender">The debt rule.</param>
		/// <param name="eventArgs">The event arguments.</param>
        private static void OnDebtRuleChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
        {

            ControlEditDebtRule control = sender as ControlEditDebtRule;

        }

		/// <summary>
		/// The name field has been changed. Update the DebtRule object with the new name.
		/// </summary>
		/// <param name="sender">The name text box.</param>
		/// <param name="e">The event arguments.</param>
		private void OnNameChanged(object sender, TextChangedEventArgs e)
		{

			if (!this.populating)
				this.DebtRule.Name = name.Text;

		}

        /// <summary>
        /// The payment method changes. Update the DebtRule object's PaymentMethod to reflect the changes.
        /// </summary>
        /// <param name="sender">The payment method combo box.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void OnPaymentMethodChanged(object sender, SelectionChangedEventArgs eventArgs)
        {

			if (!this.populating)
			{

				foreach (PaymentMethodType method in eventArgs.AddedItems)
					if (!this.DebtRule.PaymentMethod.Contains(method.PaymentMethodTypeId))
						this.DebtRule.PaymentMethod.Add(method.PaymentMethodTypeId);

				foreach (PaymentMethodType method in eventArgs.RemovedItems)
					if (this.DebtRule.PaymentMethod.Contains(method.PaymentMethodTypeId))
						this.DebtRule.PaymentMethod.Remove(method.PaymentMethodTypeId);

			}

		}

        /// <summary>
        /// The size group of the left column.
        /// </summary>
        public string SharedSizeGroup
        {

            get { return this.GetValue(SharedSizeGroupProperty) as string; }
            set { this.SetValue(SharedSizeGroupProperty, value); }

        }

    }

}
