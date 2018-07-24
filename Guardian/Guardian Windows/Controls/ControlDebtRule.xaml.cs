namespace FluidTrade.Guardian.Windows
{

    using System;
    using System.Linq;
    using System.Windows;
    using System.Text.RegularExpressions;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Collections.Generic;
    using System.Text;
	using System.Collections.ObjectModel;
	using System.Windows.Threading;
	using System.Threading;

    /// <summary>
    /// Interaction logic for TabItemDebtRule.xaml
    /// </summary>
    public partial class ControlDebtRule : UserControl
    {

        /// <summary>
        /// Dependency property for the Rule property.
        /// </summary>
        public static readonly DependencyProperty DebtRuleProperty;

        private delegate object DebtRuleProperty1(DebtRule rule);

        static ControlDebtRule()
        {

			DebtRuleProperty = DependencyProperty.Register("DebtRule", typeof(DebtRule), typeof(ControlDebtRule), new PropertyMetadata(OnRuleChanged));

        }

        /// <summary>
        /// Create a new debt rule tab item.
        /// </summary>
        public ControlDebtRule()
        {

            InitializeComponent();

        }

        /// <summary>
        /// Re-populate the tab when the rule changes.
        /// </summary>
        /// <param name="sender">The debt rule tab to update.</param>
        /// <param name="eventArgs">The event arguments.</param>
        public static void OnRuleChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
        {

            ControlDebtRule tab = sender as ControlDebtRule;

            tab.Populate();

        }

		/// <summary>
		/// Create a string representation of a list of payment methods.
		/// </summary>
		/// <param name="methods">The list of payment methods.</param>
		/// <returns>The string representation of the list.</returns>
		private string PaymentMethodToString(List<PaymentMethod> methods)
		{

			StringBuilder str = new StringBuilder();

			for (int index = 0; index < methods.Count; ++index)
				if (!methods[index].Delete)
				{

					str.Append(methods[index].ToString());
					str.Append(", ");

				}

			// We can't be sure where the list of not-deleted methods ends, so we need to chop off the last comma.
			if (str.Length >= 2)
				str.Length = str.Length - 2;

			return str.ToString();

		}

        /// <summary>
        /// Populate the tab item's controls with information from a debt rule.
        /// </summary>
        private void Populate()
        {

			if (this.DebtRule != null)
			{

				List<Guid> paymentMethods = this.DebtRule.PaymentMethod.ToList();
				Guid? ruleId = this.DebtRule.DebtRuleId;

				this.paymentLength.Text = Decimal.ToInt32(this.DebtRule.PaymentLength).ToString();
				this.startDate.Length = this.DebtRule.PaymentStartDateLength;
				this.startDate.Units = this.DebtRule.PaymentStartDateUnitId;
				this.settlementValue.Text = String.Format("{0:#0.#%}", this.DebtRule.SettlementValue);

				if (this.DebtRule.IsAutoSettled)
				{

					this.autoSettle.Visibility = System.Windows.Visibility.Visible;
					this.manualSettle.Visibility = System.Windows.Visibility.Collapsed;

				}
				else
				{

					this.autoSettle.Visibility = System.Windows.Visibility.Collapsed;
					this.manualSettle.Visibility = System.Windows.Visibility.Visible;

				}

				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(data => this.RetrievePaymentMethod(ruleId, paymentMethods));

			}

        }

		/// <summary>
		/// Populate the payment methods dropdown.
		/// </summary>
		/// <param name="methods">The list of payment method names.</param>
		private void PopulatePaymentMethod(object methods)
		{

			string separator = ", ";
			string text = "";

			this.paymentMethod.ItemsSource = methods as List<string>;

			for (int item = 0; item < this.paymentMethod.Items.Count; ++item)
			{

				text += this.paymentMethod.Items[item].ToString();

				if (item < this.paymentMethod.Items.Count - 1)
					text += separator;

			}

			this.paymentMethod.Text = text;

		}

		/// <summary>
		/// Get the payment methods from the data model.
		/// </summary>
		/// <param name="ruleId">The id of the debt rule to display.</param>
		/// <param name="methodIds">The list payment method type ids.</param>
		private void RetrievePaymentMethod(Guid? ruleId, List<Guid> methodIds)
		{

			List<string> methodList = new List<string>();

			lock (DataModel.SyncRoot)
				foreach (Guid methodId in methodIds)
				{

					PaymentMethodTypeRow type = DataModel.PaymentMethodType.PaymentMethodTypeKey.Find(methodId);

					if (!methodList.Contains(type.Name))
						methodList.Insert(~methodList.BinarySearch(type.Name), type.Name);

				}

			this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new WaitCallback(PopulatePaymentMethod), methodList);

		}

        /// <summary>
        /// The current debt rule to display.
        /// </summary>
        public DebtRule DebtRule
        {

            get { return this.GetValue(ControlDebtRule.DebtRuleProperty) as DebtRule; }
            set { this.SetValue(ControlDebtRule.DebtRuleProperty, value); }

        }

    }

}
