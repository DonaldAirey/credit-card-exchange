namespace FluidTrade.Guardian
{

    using System.Collections.ObjectModel;
    using System.ComponentModel;

    public class Role : INotifyPropertyChanged
	{

		// Private Instance Fields
		private System.String name;
		private System.Collections.ObjectModel.ObservableCollection<User> users;

		public Role(string name)
		{

			this.name = name;
			this.users = new ObservableCollection<User>();

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

		public ObservableCollection<User> Users
		{
			get { return this.users; }
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
