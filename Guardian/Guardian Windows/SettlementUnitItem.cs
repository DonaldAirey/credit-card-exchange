namespace FluidTrade.Guardian
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using FluidTrade.Core;
	using System.ComponentModel;

	/// <summary>
	/// Represents an item in the SettlementUnitList.
	/// </summary>
	public class SettlementUnitItem : INotifyPropertyChanged, IComparable<SettlementUnitItem>
	{

		// Private Instance Fields
		private System.String name;
		private SettlementUnit settlementUnitCode;
		private Guid settlementUnitId;

		/// <summary>
		/// Notifies listeners that a property has changed.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// The name.
		/// </summary>
		public string Name
		{
			get
			{
				return this.name;
			}
			set
			{
				if ((this.name != value))
				{
					this.name = value;
					if ((this.PropertyChanged != null))
						this.PropertyChanged(this, new PropertyChangedEventArgs("Name"));
				}
			}
		}

		/// <summary>
		/// The SettlementUnit value.
		/// </summary>
		public SettlementUnit SettlementUnitCode
		{
			get
			{
				return this.settlementUnitCode;
			}
			set
			{
				if ((this.settlementUnitCode != value))
				{
					this.settlementUnitCode = value;
					if ((this.PropertyChanged != null))
						this.PropertyChanged(this, new PropertyChangedEventArgs("SettlementUnitCode"));
				}
			}
		}

		/// <summary>
		/// The SettlementUnitId.
		/// </summary>
		public Guid SettlementUnitId
		{
			get
			{
				return this.settlementUnitId;
			}
			set
			{
				if ((this.settlementUnitId != value))
				{
					this.settlementUnitId = value;
					if ((this.PropertyChanged != null))
						this.PropertyChanged(this, new PropertyChangedEventArgs("SettlementUnitId"));
				}
			}
		}

		/// <summary>
		/// Compares this instance to a specified TimeInForceItem and returns an indication of their relative value.
		/// </summary>
		/// <param name="other">The other TimeInForceItem to compare.</param>
		/// <returns>1 if the current item is greater than the specified item, 0 if they are equal, otherwise -1.</returns>
		public int CompareTo(SettlementUnitItem other)
		{

			// The TimeInForceCode property is the unique identifier for these items.
			return this.SettlementUnitCode.CompareTo(other.SettlementUnitCode);

		}

		/// <summary>
		/// Returns a value indicating whether this is equal to the specified object.
		/// </summary>
		/// <param name="obj">An object to be compared to this one.</param>
		/// <returns>true if the objects are equal.</returns>
		public override Boolean Equals(object obj)
		{
			if (obj is SettlementUnitItem)
			{
				SettlementUnitItem other = obj as SettlementUnitItem;
				return this.SettlementUnitCode.Equals(other.SettlementUnitCode);
			}
			return false;
		}

		/// <summary>
		/// Returns the hash code for the value of this instance.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return this.SettlementUnitCode.GetHashCode();
		}

	}
}
