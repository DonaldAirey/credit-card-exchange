namespace FluidTrade.Guardian.Windows
{

	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Collections.Specialized;
	using System.ComponentModel;
	using System.Linq;
	using FluidTrade.Core;
	using FluidTrade.Guardian.TradingSupportReference;
	using System.ServiceModel;
	using System.Threading;
	using System.Windows;
	using System.Windows.Threading;

    /// <summary>
    /// An in-memory representation of a debt rule.
    /// </summary>
    public class DebtRule : IComparable<DebtRule>, INotifyPropertyChanged
    {

        private Guid? debtRuleId;
        private Boolean delete;
		private Boolean isAutoSettled;
		private Boolean modified;
        private String name;
        private Decimal paymentLength;
        private ObservableCollection<Guid> paymentMethod;
        private Decimal paymentStartDateLength;
		private Guid paymentStartDateUnitId;
		private Guid settlementUnitId;
		private Decimal settlementValue;
        private Int64 rowVersion;

		/// <summary>
		/// Raised when one of the properties of DebtRule changes.
		/// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Create a new debt rule.
        /// </summary>
        public DebtRule()
        {

			this.isAutoSettled = false;
            this.modified = true;
            this.delete = false;
            this.debtRuleId = null;
			this.settlementUnitId = SettlementUnitList.Default.Find(SettlementUnit.Percent).SettlementUnitId;
			this.paymentStartDateUnitId = TimeUnitList.Default.Find(TimeUnit.Days).TimeUnitId;
			this.paymentMethod = new ObservableCollection<Guid>();
			this.paymentMethod.CollectionChanged += OnPaymentMethodChanged;

        }

        /// <summary>
        /// Create a new debt rule based on an already existing debt rule.
        /// </summary>
		/// <param name="debtRuleRow">The debt rule from the DataModel to base this debt rule on.</param>
        public DebtRule(DebtRuleRow debtRuleRow)
        {

			this.isAutoSettled = debtRuleRow.IsAutoSettled;
            this.modified = false;
            this.delete = false;
            this.debtRuleId = debtRuleRow.DebtRuleId;
            this.name = debtRuleRow.Name;
            this.paymentLength = debtRuleRow.PaymentLength;
			this.paymentMethod = new ObservableCollection<Guid>();
            this.paymentStartDateLength = debtRuleRow.PaymentStartDateLength;
			this.paymentStartDateUnitId = debtRuleRow.TimeUnitRow.TimeUnitId;
			this.settlementUnitId = debtRuleRow.SettlementUnitId;
			this.settlementValue = debtRuleRow.SettlementValue;
            this.rowVersion = debtRuleRow.RowVersion;

			this.UpdatePaymentMethod(this.RetrievePaymentMethods());
			this.paymentMethod.CollectionChanged += OnPaymentMethodChanged;

        }

        /// <summary>
        /// The id of the debt rule. If the debt rule is new and not in the data model, DebtRuleId is Guid.Empty.
        /// </summary>
        public Guid? DebtRuleId
        {
            get { return this.debtRuleId; }
            set
            {

				if (this.debtRuleId != value)
				{

					this.debtRuleId = value;
					this.modified = true;
					this.RaisePropertyChanged("DebtRuleId");

				}

            }

        }

        /// <summary>
        /// If true, this debt rule should be removed from the data model (if it exists there).
        /// </summary>
        public Boolean Delete
        {
            
            get { return this.delete; }
            set
            {

				if (this.delete != value)
				{

					this.delete = value;
					this.RaisePropertyChanged("Delete");

				}

            }

        }

		/// <summary>
		/// Whether to auto-settle matches.
		/// </summary>
		public Boolean IsAutoSettled
		{

			get { return this.isAutoSettled; }
			set
			{

				if (this.isAutoSettled != value)
				{

					this.isAutoSettled = value;
					this.modified = true;
					this.RaisePropertyChanged("IsAutoSettled");

				}

			}

		}

        /// <summary>
        /// True if any of the properties of the debt rule (other than Delete) have been modified. If this rule is a newly created rule, Modified is
        /// true by default.
        /// </summary>
        public Boolean Modified
        {
            
            get { return modified; }

        }

        /// <summary>
        /// The name of the debt rule.
        /// </summary>
        public String Name
        {
           
            get { return this.name; }
            set
            {

				if (this.name != value)
				{

					this.name = value;
					this.modified = true;
					this.RaisePropertyChanged("Name");

				}

            }

        }

        /// <summary>
        /// True if this debt rule is newly created and not yet commited.
        /// </summary>
        public Boolean New
        {

            get { return this.DebtRuleId == Guid.Empty; }

        }

        /// <summary>
        /// The number of payments field of the debt rule.
        /// </summary>
        public Decimal PaymentLength
        {
            get { return this.paymentLength; }
            set
            {

				if (this.paymentLength != value)
				{

					this.paymentLength = value;
					this.modified = true;
					this.RaisePropertyChanged("NumerOfPayments");

				}

            }

        }

		/// <summary>
		/// The payment term start date field of the debt rule.
		/// </summary>
		public decimal PaymentStartDateLength
		{

			get { return this.paymentStartDateLength; }
			set
			{

				if (this.paymentStartDateLength != value)
				{

					this.paymentStartDateLength = value;
					this.modified = true;
					this.RaisePropertyChanged("PaymentStartDateLength");

				}

			}

		}

		/// <summary>
		/// The TimeUnitId of the units the PaymentStartDateLength is in.
		/// </summary>
		public Guid PaymentStartDateUnitId
		{

			get { return this.paymentStartDateUnitId; }
			set
			{

				if (this.paymentStartDateUnitId != value)
				{

					this.paymentStartDateUnitId = value;
					this.modified = true;
					this.RaisePropertyChanged("PaymentStartDateUnit");

				}

			}

		}

		/// <summary>
		/// The row version this in-memory debt rule corresponds to.
		/// </summary>
		public Int64 RowVersion
		{

			get { return this.rowVersion; }

		}

		/// <summary>
		/// The settlement percentage field of the debt rule.
		/// </summary>
		public Guid SettlementUnitId
		{

			get { return this.settlementUnitId; }
			set
			{

				if (this.settlementUnitId != value)
				{

					this.settlementUnitId = value;
					this.modified = true;
					this.RaisePropertyChanged("SettlementUnitId");

				}

			}

		}

		/// <summary>
		/// The settlement percentage field of the debt rule.
		/// </summary>
		public Decimal SettlementValue
		{

			get { return this.settlementValue; }
			set
			{

				if (this.settlementValue != value)
				{

					this.settlementValue = value;
					this.modified = true;
					this.RaisePropertyChanged("SettlementValue");

				}

			}

		}

		/// <summary>
		/// Create, update, or destroy the debt rule in the datamodel that corresponds to this DebtRule object.
		/// </summary>
		/// <param name="owner">The debt class that owns the debt rule.</param>
		public void Commit(Guid owner)
		{

			TradingSupportClient tradingSupport = new TradingSupportClient(Properties.Settings.Default.TradingSupportEndpoint);
			Int32 tries = 0;

			do
			{

				try
				{

					if (this.Delete)
					{

						this.CommitDelete(tradingSupport);

					}
					else if (this.Modified)
					{

						this.CommitModified(tradingSupport, owner);
						this.modified = false;

					}

					break;

				}
				catch (FaultException<DeadlockFault>)
				{

					if (tries > 3)
						throw;

				}

				tries += 1;

			} while (tries < 3);

			tradingSupport.Close();

		}

		/// <summary>
		/// Do actual delete.
		/// </summary>
		/// <param name="tradingSupport">The trading support client.</param>
		private void CommitDelete(TradingSupportClient tradingSupport)
		{

			if (this.DebtRuleId != null)
			{

				Boolean inUse = false;

				lock (DataModel.SyncRoot)
					inUse = DataModel.DebtClass.Any(row => !row.IsDebtRuleIdNull() && row.DebtRuleId == this.DebtRuleId);

				if (!inUse)
				{

					MethodResponseErrorCode response = tradingSupport.DeleteDebtRule(new TradingSupportReference.DebtRule[] {
							new TradingSupportReference.DebtRule() { RowId = this.DebtRuleId.Value, RowVersion = this.RowVersion }
						});

					if (!response.IsSuccessful && (response.Errors.Length == 0 || response.Errors[0].ErrorCode != ErrorCode.Deadlock))
						GuardianObject.ThrowErrorInfo(response.Errors[0]);

				}
				else
				{

					throw new DebtRuleInUseException(this.Name, "Cannot delete a debt rule that is currently in use.");

				}

			}

		}

		/// <summary>
		/// Commit changes or create a new rule.
		/// </summary>
		/// <param name="tradingSupport">The trading support client.</param>
		/// <param name="owner">The debt class that owns the rule.</param>
		private void CommitModified(TradingSupportClient tradingSupport, Guid owner)
		{

			Guid[] paymentMethod = new Guid[this.PaymentMethod.Count];

			this.PaymentMethod.CopyTo(paymentMethod, 0);

			if (this.DebtRuleId != null)
			{

				TradingSupportReference.MethodResponseErrorCode response = tradingSupport.UpdateDebtRule(new TradingSupportReference.DebtRule[] { new TradingSupportReference.DebtRule () {
								IsAutoSettled = this.IsAutoSettled,
								Name = this.Name,
								Owner = owner,
								PaymentLength = this.PaymentLength,
								PaymentMethod = paymentMethod,
								PaymentStartDateLength = this.PaymentStartDateLength,
								PaymentStartDateUnitId = this.PaymentStartDateUnitId,
								RowId = this.DebtRuleId.Value,
								RowVersion = this.RowVersion,
								SettlementUnitId = this.settlementUnitId,
								SettlementValue = this.SettlementValue} });

				if (!response.IsSuccessful && (response.Errors.Length == 0 || response.Errors[0].ErrorCode != ErrorCode.Deadlock))
					GuardianObject.ThrowErrorInfo(response.Errors[0]);

			}
			else
			{

				MethodResponseArrayOfguid guids = tradingSupport.CreateDebtRule(new TradingSupportReference.DebtRule[] { new TradingSupportReference.DebtRule () {
						IsAutoSettled = this.isAutoSettled,
						Name = this.Name,
						Owner = owner,
						PaymentLength = this.PaymentLength,
						PaymentMethod = paymentMethod,
						PaymentStartDateLength = this.PaymentStartDateLength,
						PaymentStartDateUnitId = this.PaymentStartDateUnitId,
						SettlementUnitId = this.settlementUnitId,
						SettlementValue = this.SettlementValue} });

				if (guids.IsSuccessful)
					this.DebtRuleId = guids.Result[0];
				else
					throw new Exception("Failed to create debt rule.");

			}

		}

		/// <summary>
		/// Handle one of the payment methods changing. Raise the our PropertyChanged event.
		/// </summary>
		/// <param name="sender">The payment method that changed.</param>
		/// <param name="eventArgs">The event arguments.</param>
		void OnPaymentMethodChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
		{

			this.modified = true;
			this.RaisePropertyChanged("PaymentMethod");

		}

        /// <summary>
        /// The payment method field of the debt rule.
        /// </summary>
		public ObservableCollection<Guid> PaymentMethod
        {

            get { return this.paymentMethod; }
            set
            {

				if (this.paymentMethod != null)
					this.paymentMethod.CollectionChanged -= OnPaymentMethodChanged;
                this.paymentMethod = value;
				this.paymentMethod.CollectionChanged += OnPaymentMethodChanged;
				this.modified = true;
                this.RaisePropertyChanged("PaymentMethod");

            }

        }

        /// <summary>
        /// Raise the property changed event.
        /// </summary>
        /// <param name="property">The name of the property that changed.</param>
        protected void RaisePropertyChanged(String property)
        {

			if (this.PropertyChanged != null)
				if (Thread.CurrentThread != Application.Current.Dispatcher.Thread)
					Application.Current.Dispatcher.BeginInvoke(new Action(() =>
						this.PropertyChanged(this, new PropertyChangedEventArgs(property))),
						DispatcherPriority.Normal);
				else
					this.PropertyChanged(this, new PropertyChangedEventArgs(property));

        }

		/// <summary>
		/// Retrieve the payment methods for this debt rule. The returned list actually contains all available payment methods - the ones not currently set
		/// in the rule are marked as Delete.
		/// </summary>
		/// <returns>The list of payment methods.</returns>
		private List<Guid> RetrievePaymentMethods()
		{

			List<Guid> methods = new List<Guid>();

			// Retrieve the payment methods that are actually set for this rule.
			foreach (DebtRulePaymentMethodRow debtRulePaymentMethodRow in DataModel.DebtRulePaymentMethod.Where(m => m.DebtRuleId == this.DebtRuleId))
			{

				methods.Add(debtRulePaymentMethodRow.PaymentMethodTypeId);

			}

			return methods;

		}

        /// <summary>
        /// If the rule has not been modified, update it with an existing debt rule.
        /// </summary>
        /// <param name="rule">The debt rule row to update from.</param>
        public void Update(DebtRuleRow rule)
        {

            if (!this.modified && this.rowVersion != this.RowVersion)
            {

                this.DebtRuleId = rule.DebtRuleId;
                this.Name = rule.Name;
				this.PaymentLength = rule.PaymentLength;
                this.PaymentStartDateLength = rule.PaymentStartDateLength;
				this.PaymentStartDateUnitId = rule.PaymentStartDateUnitId;
				this.SettlementValue = rule.SettlementValue;
                this.modified = false;

				this.UpdatePaymentMethod(this.RetrievePaymentMethods());

            }

            this.rowVersion = rule.RowVersion;

        }

        /// <summary>
        /// If the rule has not been modified, update it with an existing debt rule.
        /// </summary>
        /// <param name="rule">The debt rule to update from.</param>
        public void Update(DebtRule rule)
        {

            if (!this.modified && this.rowVersion != rule.RowVersion)
            {

                this.DebtRuleId = rule.DebtRuleId;
                this.Name = rule.Name;
                this.PaymentLength = rule.PaymentLength;
                this.PaymentStartDateLength = rule.PaymentStartDateLength;
				this.PaymentStartDateUnitId = rule.PaymentStartDateUnitId;
				this.SettlementValue = rule.SettlementValue;
				this.modified = false;

				this.UpdatePaymentMethod(rule.PaymentMethod.ToList());

            }

            this.rowVersion = rule.RowVersion;

        }

		/// <summary>
		/// Update the payment method list.
		/// </summary>
		/// <param name="methods">The list of payment methods to update from.</param>
		private void UpdatePaymentMethod(List<Guid> methods)
		{

			// Go through PaymentMethods and make sure they're all in methods - if they aren't they need to be removed.
			for (int index = 0; index < this.PaymentMethod.Count; index += 1)
			{

				Guid method = this.PaymentMethod[index];

				if (!methods.Contains(method))
				{

					// A payment method is missing then it's type has been removed, so we can simply discard the payment method itself.
					this.PaymentMethod.Remove(method);
					index -= 1;

				}
				else
				{

					// We found the method in our collection already, so we won't need to add it later.
					methods.Remove(method);

				}

			}
			// We should only need to add new payment methods on initialization or if new payment method types are added.
			foreach (Guid method in methods)
			{

				this.PaymentMethod.Add(method);

			}

		}

        /// <summary>
        /// Compare this object to another DebtRule by the debt rule id.
        /// </summary>
        /// <param name="other">The other debt rule object.</param>
        /// <returns>An indication of the relative order of the two debt rule objects.</returns>
        public int CompareTo(DebtRule other)
        {

			if (this.DebtRuleId == null && other.DebtRuleId == null)
				return 0;
			else if (this.DebtRuleId == null)
				return -1;
			else if (other.DebtRuleId == null)
				return 1;
			else
	            return this.DebtRuleId.Value.CompareTo(other.DebtRuleId.Value);

        }

        /// <summary>
        /// Returns a value indicating whether this is equal to the specified object.
        /// </summary>
        /// <param name="obj">An object to be compared to this one.</param>
        /// <returns>true if the objects are equal.</returns>
        public override Boolean Equals(object obj)
        {

            if (obj is DebtRule)
            {

                DebtRule other = obj as DebtRule;
                return this.DebtRuleId.Equals(other.DebtRuleId);

            }
            return false;

        }

        /// <summary>
        /// Returns the hash code for the value of this instance.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {

            return this.DebtRuleId.GetHashCode();

        }

    }

}
