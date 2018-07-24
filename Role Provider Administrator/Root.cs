namespace FluidTrade.Guardian
{

    using System.Collections.ObjectModel;
    using System.ComponentModel;

    public class Root : INotifyPropertyChanged
	{

		// Private Instance Fields
		private System.String name;
		private System.Collections.ObjectModel.ObservableCollection<Role> roles;

		public Root(string name)
		{

			this.name = name;
			this.roles = new ObservableCollection<Role>();

		}

		public string Name
		{

			get { return this.name; }
			set
			{
				if (this.name != value)
				{
					this.name = value;
					if (this.PropertyChanged != null)
						this.PropertyChanged(this, new PropertyChangedEventArgs("Name"));
				}
			}

		}

		public ObservableCollection<Role> Roles
		{
			get { return this.roles; }
		}

		public override string ToString()
		{
			return name;
		}

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

	}

}
