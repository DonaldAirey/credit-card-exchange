namespace FluidTrade.Guardian.Windows
{

	using System;
	using System.ComponentModel;
	using System.IO;
	using System.Windows.Media;
	using System.Windows.Media.Imaging;
    using FluidTrade.Core;

	/// <summary>
	/// Represents an item in the SideList.
	/// </summary>
	public class SideItem : INotifyPropertyChanged, IComparable<SideItem>
	{

		// Private Instance Fields
		private System.String description;
		private System.String disabledImage;
		private System.String enabledImage;
		private System.String mnemonic;
		private System.Int32 sortOrder;
		private Side sideCode;

		/// <summary>
		/// Notifies listeners that a property has changed.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// SideCode
		/// </summary>
		public Side SideCode
		{
			get
			{
				return this.sideCode;
			}
			set
			{
				if ((this.sideCode != value))
				{
					this.sideCode = value;
					if ((this.PropertyChanged != null))
						this.PropertyChanged(this, new PropertyChangedEventArgs("SideCode"));
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
		/// DisabledImage
		/// </summary>
		public string DisabledImage
		{
			get
			{
				return this.disabledImage;
			}
			set
			{
				if ((this.disabledImage != value))
				{
					this.disabledImage = value;
					if ((this.PropertyChanged != null))
					{
						this.PropertyChanged(this, new PropertyChangedEventArgs("DisabledImage"));
						this.PropertyChanged(this, new PropertyChangedEventArgs("DisabledImageSource"));
					}
				}
			}
		}

		/// <summary>
		/// DisabledImageSource
		/// </summary>
		public ImageSource DisabledImageSource
		{
			get
			{
				// The images are created on demand.
				BitmapImage bitmapImage = new BitmapImage();
				bitmapImage.BeginInit();
				bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
				bitmapImage.StreamSource = new MemoryStream(Convert.FromBase64String(this.disabledImage));
				bitmapImage.EndInit();
				return bitmapImage;
			}
		}

		/// <summary>
		/// EnabledImage
		/// </summary>
		public string EnabledImage
		{
			get
			{
				return this.enabledImage;
			}
			set
			{
				if ((this.enabledImage != value))
				{
					this.enabledImage = value;
					if ((this.PropertyChanged != null))
					{
						this.PropertyChanged(this, new PropertyChangedEventArgs("EnabledImage"));
						this.PropertyChanged(this, new PropertyChangedEventArgs("EnabledImageSource"));
					}
				}
			}
		}

		/// <summary>
		/// EnabledImageSource
		/// </summary>
		public ImageSource EnabledImageSource
		{
			get
			{
				// The images are created on demand.
				BitmapImage bitmapImage = new BitmapImage();
				bitmapImage.BeginInit();
				bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
				bitmapImage.StreamSource = new MemoryStream(Convert.FromBase64String(this.enabledImage));
				bitmapImage.EndInit();
				return bitmapImage;
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
		/// Compares this instance to a specified SideItem and returns an indication of their relative value.
		/// </summary>
		/// <param name="other">The other SideItem to compare.</param>
		/// <returns>1 if the current item is greater than the specified item, 0 if they are equal, otherwise -1.</returns>
		public int CompareTo(SideItem other)
		{

			// The SideCode property is the unique identifier for these items.
			return this.SideCode.CompareTo(other.SideCode);

		}

		/// <summary>
		/// Returns a value indicating whether this is equal to the specified object.
		/// </summary>
		/// <param name="obj">An object to be compared to this one.</param>
		/// <returns>true if the objects are equal.</returns>
		public override Boolean Equals(object obj)
		{
			if (obj is SideItem)
			{
				SideItem other = obj as SideItem;
				return this.SideCode.Equals(other.SideCode);
			}
			return false;
		}

		/// <summary>
		/// Returns the hash code for the value of this instance.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return this.SideCode.GetHashCode();
		}

	}

}
