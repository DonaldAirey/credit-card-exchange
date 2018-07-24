namespace FluidTrade.Guardian
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.ComponentModel;

	/// <summary>
	/// An in-memory representation of a payment method type.
	/// </summary>
	public class PaymentMethodType : INotifyPropertyChanged, IComparable<PaymentMethodType>
	{

		private string name;
		private Guid paymentMethodTypeId;
		private long rowVersion;

		/// <summary>
		/// Notifies listeners that a property has changed.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Create a new PaymentMethodType from a row in the DataModel.
		/// </summary>
		/// <param name="row">The row to base the new object on.</param>
		public PaymentMethodType(PaymentMethodTypeRow row)
		{

			this.name = row.Name;
			this.paymentMethodTypeId = row.PaymentMethodTypeId;
			this.rowVersion = row.RowVersion;

		}

		/// <summary>
		/// The name of the payment type.
		/// </summary>
		public string Name
		{

			get { return this.name; }

		}

		/// <summary>
		/// The PaymentTypeId of the payment type.
		/// </summary>
		public Guid PaymentMethodTypeId
		{

			get { return this.paymentMethodTypeId; }

		}

		/// <summary>
		/// The version of the row this object was based on.
		/// </summary>
		public long RowVersion
		{

			get { return this.rowVersion; }

		}

		/// <summary>
		/// Compare this object to another PaymentMethodType.
		/// </summary>
		/// <param name="other">The other payment method object.</param>
		/// <returns>An indication of the relative order of the two payment method objects.</returns>
		public int CompareTo(PaymentMethodType other)
		{

			return this.Name.CompareTo(other.Name);

		}

		/// <summary>
		/// Returns a value indicating whether this is equal to the specified object.
		/// </summary>
		/// <param name="obj">An object to be compared to this one.</param>
		/// <returns>true if the objects are equal.</returns>
		public override Boolean Equals(object obj)
		{

			if (obj is PaymentMethodType)
			{

				PaymentMethodType other = obj as PaymentMethodType;
				return this.PaymentMethodTypeId.Equals(other.PaymentMethodTypeId);

			}

			return false;

		}

		/// <summary>
		/// Returns the hash code for the value of this instance.
		/// </summary>
		/// <returns>The hash of the object.</returns>
		public override int GetHashCode()
		{

			return this.paymentMethodTypeId.GetHashCode();

		}

		/// <summary>
		/// Convert the payment method type to a string.
		/// </summary>
		/// <returns>The name of payment method type.</returns>
		public override string ToString()
		{

			return this.Name;

		}

		/// <summary>
		/// Update the payment method type with data from the data model.
		/// </summary>
		/// <param name="row">The row in the data model to update with.</param>
		public void Update(PaymentMethodTypeRow row)
		{

			if (this.paymentMethodTypeId == row.PaymentMethodTypeId && this.rowVersion != row.RowVersion)
			{

				this.name = row.Name;
				this.rowVersion = row.RowVersion;

				if (this.PropertyChanged != null)
					this.PropertyChanged(this, new PropertyChangedEventArgs("Name"));
			}

		}

		/// <summary>
		/// Update the payment method type from another one.
		/// </summary>
		/// <param name="type">The payment method type to update from.</param>
		public void Update(PaymentMethodType type)
		{

			if (this.paymentMethodTypeId == type.PaymentMethodTypeId && this.rowVersion != type.RowVersion)
			{

				this.name = type.Name;
				this.rowVersion = type.RowVersion;

				if (this.PropertyChanged != null)
					this.PropertyChanged(this, new PropertyChangedEventArgs("Name"));

			}

		}

	}

}
