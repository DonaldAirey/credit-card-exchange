namespace FluidTrade.Core.Windows.Controls
{

	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
    using System.Windows.Markup;

	[ContentProperty("Children")]
	public class RowTemplate : INotifyPropertyChanged
	{

		// Private Instance Variables
		private System.Collections.Generic.List<RowTemplate> children;
		private System.Double height;
		private System.String path;
		private System.String rowId;
		private System.Type targetType;

		// Public Events
		public event EventHandler CollectionChanged;
		public event PropertyChangedEventHandler PropertyChanged;

		public RowTemplate()
		{

			this.children = new List<RowTemplate>();

		}

		public Type TargetType
		{
			get { return this.targetType; }
			set
			{
				if (this.targetType != value)
				{
					this.targetType = value;
					if (this.CollectionChanged != null)
						this.CollectionChanged(this, EventArgs.Empty);
				}
			}
		}

		public string Path
		{
			get { return this.path; }
			set { this.path = value; }
		}

		public List<RowTemplate> Children
		{
			get { return this.children; }
			set { this.children = value; }
		}

		public string RowId
		{
			get { return this.rowId; }
			set { this.rowId = value; }
		}

		public double Height
		{
			get { return this.height; }
			set
			{
				if (this.height != value)
				{
					this.height = value;
					if (this.CollectionChanged != null)
						this.CollectionChanged(this, EventArgs.Empty);
					if (this.PropertyChanged != null)
						this.PropertyChanged(this, new PropertyChangedEventArgs("Height"));
				}
			}
		}

	}
}
