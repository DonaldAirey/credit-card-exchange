namespace FluidTrade.Guardian.Windows
{
	using System;
	using System.ComponentModel;
	using System.IO;
	using System.Windows.Media;
	using System.Windows.Media.Imaging;

	/// <summary>
	/// Represents an item in the StatusList.
	/// </summary>
	public class StatusItem : INotifyPropertyChanged, IComparable<StatusItem>
	{

		// Private Instance Fields
		private System.String description;
		private System.String disabledImage;
		private System.String enabledImage;
		private System.String mnemonic;
		private Status statusCode;
		private Guid statusId;

		/// <summary>
		/// Notifies listeners that a property has changed.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// StatusCode
		/// </summary>
		public Status StatusCode
		{
			get
			{
				return this.statusCode;
			}
			set
			{
				if ((this.statusCode != value))
				{
					this.statusCode = value;
					if ((this.PropertyChanged != null))
						this.PropertyChanged(this, new PropertyChangedEventArgs("StatusCode"));
				}
			}
		}

		/// <summary>
		/// StatusId
		/// </summary>
		public Guid StatusId
		{
			get
			{
				return this.statusId;
			}
			set
			{
				if ((this.statusId != value))
				{
					this.statusId = value;
					if ((this.PropertyChanged != null))
						this.PropertyChanged(this, new PropertyChangedEventArgs("StatusId"));
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
		/// Compares this instance to a specified StatusItem and returns an indication of their relative value.
		/// </summary>
		/// <param name="other">The other StatusItem to compare.</param>
		/// <returns>1 if the current item is greater than the specified item, 0 if they are equal, otherwise -1.</returns>
		public int CompareTo(StatusItem other)
		{

			// The StatusCode property is the unique identifier for these items.
			return this.StatusCode.CompareTo(other.StatusCode);

		}

		/// <summary>
		/// Returns a value indicating whether this is equal to the specified object.
		/// </summary>
		/// <param name="obj">An object to be compared to this one.</param>
		/// <returns>true if the objects are equal.</returns>
		public override Boolean Equals(object obj)
		{
			if (obj is StatusItem)
			{
				StatusItem other = obj as StatusItem;
				return this.StatusCode.Equals(other.StatusCode);
			}
			return false;
		}

		/// <summary>
		/// Returns the hash code for the value of this instance.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return this.StatusCode.GetHashCode();
		}

		/// <summary>
		/// A string represenation of the status.
		/// </summary>
		/// <returns></returns>
		public override String ToString()
		{

			return this.Mnemonic;

		}

	}

}
