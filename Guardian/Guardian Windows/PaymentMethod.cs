namespace FluidTrade.Guardian
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.ComponentModel;

	/// <summary>
	/// An in-memory representation of a payment method.
	/// </summary>
	public class PaymentMethod : IComparable<PaymentMethod>, INotifyPropertyChanged
	{

		private Guid paymentMethodId;
		private Guid paymentMethodTypeId;
		private string typeName;
		private Guid debtRuleId;
		private Int64 rowVersion;
		private Boolean dirty;
		private Boolean delete;

		/// <summary>
		/// The event raised when one of the properties changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Create an empty payment method.
		/// </summary>
		public PaymentMethod()
		{

			this.paymentMethodId = Guid.Empty;
			this.paymentMethodTypeId = Guid.Empty;
			this.typeName = "";
			this.debtRuleId = Guid.Empty;
			this.dirty = true;
			this.delete = false;

		}

		/// <summary>
		/// Create a payment method from a payment method row.
		/// </summary>
		/// <param name="debtRulePaymentMethodRow">The payment method row in the DataModel.</param>
		public PaymentMethod(DebtRulePaymentMethodRow debtRulePaymentMethodRow)
		{

			PaymentMethodTypeRow type = debtRulePaymentMethodRow.PaymentMethodTypeRow;

			this.paymentMethodId = debtRulePaymentMethodRow.DebtRulePaymentMethodId;
			this.paymentMethodTypeId = debtRulePaymentMethodRow.PaymentMethodTypeId;
			this.typeName = type.Name;
			this.debtRuleId = debtRulePaymentMethodRow.DebtRuleId;
			this.rowVersion = debtRulePaymentMethodRow.RowVersion;
			this.dirty = false;
			this.delete = false;

		}

		/// <summary>
		/// Commit this payment method to the server.
		/// </summary>
		public void Commit()
		{

			DataModelClient dataModelClient = new DataModelClient(FluidTrade.Guardian.Properties.Settings.Default.DataModelEndpoint);

			if (this.Delete)
			{

				// Could be the user added and then deleted a user to the list, so makes sure the row actually exists.
				if (!this.New)
					dataModelClient.DestroyDebtRulePaymentMethod(new object[] { this.PaymentMethodId }, this.RowVersion);

			}
			else if (this.Dirty)
			{

				if (!this.New)
				{

					dataModelClient.UpdateDebtRulePaymentMethod(this.DebtRuleId, this.PaymentMethodId, new object[] { this.PaymentMethodId },
						this.PaymentMethodTypeId, this.RowVersion);

				}
				else
				{

					// If the list item doesn't exist, that means the user added it, and we need to Create a new AccessControl row for it.
					this.paymentMethodId = Guid.NewGuid();
					dataModelClient.CreateDebtRulePaymentMethod(this.DebtRuleId, this.PaymentMethodId, this.PaymentMethodTypeId);

				}

				this.dirty = false;

			}

			dataModelClient.Close();

		}

		/// <summary>
		/// Gets or sets the DebtRuleId of the debt rule this payment method belongs to.
		/// </summary>
		public Guid DebtRuleId
		{

			get { return this.debtRuleId; }
			set
			{

				this.debtRuleId = value;
				this.dirty = true;
				this.RaisePropertyChanged("DebtRuleId");
		
			}

		}

		/// <summary>
		/// If true, this payment method should be removed from the data model (if it exists there).
		/// </summary>
		public Boolean Delete
		{

			get { return this.delete; }
			set {
				
				this.delete = value;
				this.RaisePropertyChanged("Delete");
			
			}

		}

		/// <summary>
		/// If true, this payment method contains changes not in the data model. If this is a newly created payment method, it is Dirty by default.
		/// </summary>
		public Boolean Dirty
		{

			get { return this.dirty; }

		}

		/// <summary>
		/// True if this payment method is newly created and not yet commited.
		/// </summary>
		public Boolean New
		{

			get { return this.paymentMethodId == Guid.Empty; }

		}

		/// <summary>
		/// The id of the payment method. If the payment method is new and not in the data model, PaymentMethodId is Guid.Empty.
		/// </summary>
		public Guid PaymentMethodId
		{

			get { return this.paymentMethodId; }
			set { this.paymentMethodId = value; }

		}

		/// <summary>
		/// Gets or sets the PaymentMethodTypeId of the type of this payment method.
		/// </summary>
		public Guid PaymentMethodTypeId
		{

			get { return this.paymentMethodTypeId; }
			set
			{

				this.paymentMethodTypeId = value;
				this.dirty = true;
				this.RaisePropertyChanged("PaymentMethodId");

			}

		}

		/// <summary>
		/// Raise the property changed event.
		/// </summary>
		/// <param name="property">The name of the property that changed.</param>
		protected void RaisePropertyChanged(string property)
		{

			if (this.PropertyChanged != null)
				this.PropertyChanged(this, new PropertyChangedEventArgs(property));

		}

		/// <summary>
		/// The row version this in-memory payment method corresponds to.
		/// </summary>
		public long RowVersion
		{

			get { return this.rowVersion; }

		}

		/// <summary>
		/// The name of this payment method's type.
		/// </summary>
		public string TypeName
		{

			get { return this.typeName; }
			set {
				
				this.typeName = value;
				this.RaisePropertyChanged("TypeName");
			
			}

		}

		/// <summary>
		/// Get a string representation of this payment method.
		/// </summary>
		/// <returns>The name and description of the payment method's type.</returns>
		public override string ToString()
		{

			return this.typeName;

		}

		/// <summary>
		/// If the payment method has not been modified, update it with an existing debt rule.
		/// </summary>
		/// <param name="debtRulePaymentMethodRow">The payment method row to update from.</param>
		public void Update(DebtRulePaymentMethodRow debtRulePaymentMethodRow)
		{

			if (!this.Dirty && debtRulePaymentMethodRow.PaymentMethodTypeId == this.PaymentMethodTypeId && debtRulePaymentMethodRow.RowVersion != this.RowVersion)
			{

				this.paymentMethodId = debtRulePaymentMethodRow.DebtRulePaymentMethodId;
				this.paymentMethodTypeId = debtRulePaymentMethodRow.PaymentMethodTypeId;
				this.debtRuleId = debtRulePaymentMethodRow.DebtRuleId;

			}

			this.rowVersion = debtRulePaymentMethodRow.RowVersion;

		}

		/// <summary>
		/// If the payment method has not been modified, update it with an existing debt rule.
		/// </summary>
		/// <param name="method">The payment method to update from.</param>
		public void Update(PaymentMethod method)
		{

			if (!this.Dirty && method.PaymentMethodTypeId == this.PaymentMethodTypeId && method.RowVersion != this.RowVersion)
			{

				this.paymentMethodId = method.PaymentMethodId;
				this.paymentMethodTypeId = method.PaymentMethodTypeId;
				this.debtRuleId = method.DebtRuleId;

			}

			this.rowVersion = method.RowVersion;

		}

		#region Equality Members

		/// <summary>
		/// Compare this object to another PaymentMethod by the debt rule id.
		/// </summary>
		/// <param name="other">The other payment method object.</param>
		/// <returns>An indication of the relative order of the two payment method objects.</returns>
		public int CompareTo(PaymentMethod other)
		{

			return this.TypeName.CompareTo(other.TypeName);

		}

		/// <summary>
		/// Returns a value indicating whether this is equal to the specified object.
		/// </summary>
		/// <param name="obj">An object to be compared to this one.</param>
		/// <returns>true if the objects are equal.</returns>
		public override Boolean Equals(object obj)
		{

			if (obj is PaymentMethod)
			{

				PaymentMethod other = obj as PaymentMethod;
				return this.PaymentMethodTypeId.Equals(other.PaymentMethodTypeId)
					&& this.DebtRuleId.Equals(other.DebtRuleId);

			}
			return false;

		}

		/// <summary>
		/// Returns the hash code for the value of this instance.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{

			return this.PaymentMethodId.GetHashCode();

		}

		#endregion

	}

}
