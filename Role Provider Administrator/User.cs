namespace FluidTrade.Guardian
{

    using System.ComponentModel;

    public class User : INotifyPropertyChanged
	{

		private System.String name;
		private Role role;

		public User(Role role, string name)
		{

			// Initialize the object
			this.role = role;
			this.name = name;

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

		public Role Role
		{
			get { return this.role; }
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
