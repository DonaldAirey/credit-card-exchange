namespace FluidTrade.Guardian.Windows
{

	using System;
	using System.ComponentModel;

	/// <summary>
	/// Represents an item in the BlotterList.
	/// </summary>
	public class BlotterItem : INotifyPropertyChanged, IComparable<BlotterItem>
	{

		// Private Instance Fields
		private String description;
		private String name;
		private Guid blotterId;
		private Guid typeId;

		/// <summary>
		/// Notifies listeners that a property has changed.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Gets or sets the blotter identifier.
		/// </summary>
		public Guid BlotterId
		{
			get
			{
				return this.blotterId;
			}
			set
			{
				if ((this.blotterId != value))
				{
					this.blotterId = value;
					if ((this.PropertyChanged != null))
						this.PropertyChanged(this, new PropertyChangedEventArgs("BlotterId"));
				}
			}
		}

		/// <summary>
		/// Gets or sets the description of the object.
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
		/// Gets or sets the name of the object.
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
		/// Gets or sets the type of object in the line item in the blotter.
		/// </summary>
		public Guid TypeId
		{
			get
			{
				return this.typeId;
			}
			set
			{
				if ((this.typeId != value))
				{
					this.typeId = value;
					if ((this.PropertyChanged != null))
						this.PropertyChanged(this, new PropertyChangedEventArgs("TypeId"));
				}
			}
		}

		/// <summary>
		/// Compares this instance to a specified BlotterItem and returns an indication of their relative value.
		/// </summary>
		/// <param name="other">The other BlotterItem to compare.</param>
		/// <returns>1 if the current item is greater than the specified item, 0 if they are equal, otherwise -1.</returns>
		public int CompareTo(BlotterItem other)
		{

			// The BlotterId property is the unique identifier for these items.
			return this.BlotterId.CompareTo(other.BlotterId);

		}

		/// <summary>
		/// Returns a value indicating whether this is equal to the specified object.
		/// </summary>
		/// <param name="obj">An object to be compared to this one.</param>
		/// <returns>true if the objects are equal.</returns>
		public override Boolean Equals(object obj)
		{
			if (obj is BlotterItem)
			{
				BlotterItem other = obj as BlotterItem;
				return this.BlotterId.Equals(other.BlotterId);
			}
			return false;
		}

		/// <summary>
		/// Returns the hash code for the value of this instance.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return this.BlotterId.GetHashCode();
		}

	}

}
