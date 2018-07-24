namespace FluidTrade.Guardian.Windows
{

	using System;
	using System.ComponentModel;
    using FluidTrade.Core;

	/// <summary>
	/// Represents an item in the TimeInForceList.
	/// </summary>
	public class TimeInForceItem : INotifyPropertyChanged, IComparable<TimeInForceItem>
	{

		// Private Instance Fields
		private System.String description;
		private System.String mnemonic;
		private System.Int32 sortOrder;
		private TimeInForce timeInForceCode;

		/// <summary>
		/// Notifies listeners that a property has changed.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// TimeInForceCode
		/// </summary>
		public TimeInForce TimeInForceCode
		{
			get
			{
				return this.timeInForceCode;
			}
			set
			{
				if ((this.timeInForceCode != value))
				{
					this.timeInForceCode = value;
					if ((this.PropertyChanged != null))
						this.PropertyChanged(this, new PropertyChangedEventArgs("TimeInForceCode"));
				}
			}
		}

		/// <summary>
		/// Mnemonic
		/// </summary>
		public string Mnemonic
		{
			get
			{
				return this.mnemonic;
			}
			set
			{
				if ((this.mnemonic != value))
				{
					this.mnemonic = value;
					if ((this.PropertyChanged != null))
						this.PropertyChanged(this, new PropertyChangedEventArgs("Mnemonic"));
				}
			}
		}

		/// <summary>
		/// Description
		/// </summary>
		public string Description
		{
			get
			{
				return this.description;
			}
			set
			{
				if ((this.description != value))
				{
					this.description = value;
					if ((this.PropertyChanged != null))
						this.PropertyChanged(this, new PropertyChangedEventArgs("Description"));
				}
			}
		}

		/// <summary>
		/// SortOrder
		/// </summary>
		public int SortOrder
		{
			get
			{
				return this.sortOrder;
			}
			set
			{
				if ((this.sortOrder != value))
				{
					this.sortOrder = value;
					if ((this.PropertyChanged != null))
						this.PropertyChanged(this, new PropertyChangedEventArgs("SortOrder"));
				}
			}
		}

		/// <summary>
		/// Compares this instance to a specified TimeInForceItem and returns an indication of their relative value.
		/// </summary>
		/// <param name="other">The other TimeInForceItem to compare.</param>
		/// <returns>1 if the current item is greater than the specified item, 0 if they are equal, otherwise -1.</returns>
		public int CompareTo(TimeInForceItem other)
		{

			// The TimeInForceCode property is the unique identifier for these items.
			return this.TimeInForceCode.CompareTo(other.TimeInForceCode);

		}

		/// <summary>
		/// Returns a value indicating whether this is equal to the specified object.
		/// </summary>
		/// <param name="obj">An object to be compared to this one.</param>
		/// <returns>true if the objects are equal.</returns>
		public override Boolean Equals(object obj)
		{
			if (obj is TimeInForceItem)
			{
				TimeInForceItem other = obj as TimeInForceItem;
				return this.TimeInForceCode.Equals(other.TimeInForceCode);
			}
			return false;
		}

		/// <summary>
		/// Returns the hash code for the value of this instance.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return this.TimeInForceCode.GetHashCode();
		}

	}

}
