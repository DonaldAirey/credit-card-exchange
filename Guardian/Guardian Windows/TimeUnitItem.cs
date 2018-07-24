namespace FluidTrade.Guardian
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using FluidTrade.Core;
	using System.ComponentModel;

	/// <summary>
	/// Represents an item in the TimeUnitList.
	/// </summary>
	public class TimeUnitItem : INotifyPropertyChanged, IComparable<TimeUnitItem>
	{

		// Private Instance Fields
		private System.String name;
		private TimeUnit timeUnitCode;
		private Guid timeUnitId;

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
		/// The TimeUnit value.
		/// </summary>
		public TimeUnit TimeUnitCode
		{
			get
			{
				return this.timeUnitCode;
			}
			set
			{
				if ((this.timeUnitCode != value))
				{
					this.timeUnitCode = value;
					if ((this.PropertyChanged != null))
						this.PropertyChanged(this, new PropertyChangedEventArgs("TimeUnitCode"));
				}
			}
		}

		/// <summary>
		/// The TimeUnitId.
		/// </summary>
		public Guid TimeUnitId
		{
			get
			{
				return this.timeUnitId;
			}
			set
			{
				if ((this.timeUnitId != value))
				{
					this.timeUnitId = value;
					if ((this.PropertyChanged != null))
						this.PropertyChanged(this, new PropertyChangedEventArgs("TimeUnitId"));
				}
			}
		}

		/// <summary>
		/// Compares this instance to a specified TimeInForceItem and returns an indication of their relative value.
		/// </summary>
		/// <param name="other">The other TimeInForceItem to compare.</param>
		/// <returns>1 if the current item is greater than the specified item, 0 if they are equal, otherwise -1.</returns>
		public int CompareTo(TimeUnitItem other)
		{

			// The TimeInForceCode property is the unique identifier for these items.
			return this.TimeUnitCode.CompareTo(other.TimeUnitCode);

		}

		/// <summary>
		/// Returns a value indicating whether this is equal to the specified object.
		/// </summary>
		/// <param name="obj">An object to be compared to this one.</param>
		/// <returns>true if the objects are equal.</returns>
		public override Boolean Equals(object obj)
		{
			if (obj is TimeUnitItem)
			{
				TimeUnitItem other = obj as TimeUnitItem;
				return this.TimeUnitCode.Equals(other.TimeUnitCode);
			}
			return false;
		}

		/// <summary>
		/// Returns the hash code for the value of this instance.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return this.TimeUnitCode.GetHashCode();
		}

	}
}
