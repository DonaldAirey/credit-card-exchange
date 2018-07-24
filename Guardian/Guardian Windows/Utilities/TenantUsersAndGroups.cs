namespace FluidTrade.Guardian
{

	using System.Collections;
	using FluidTrade.Guardian.Windows;
	using System.ComponentModel;
	using System.Windows;
	using System.Collections.Specialized;

	/// <summary>
	/// A list containing the lists of users and groups in a tenant.
	/// </summary>
	public class TenantUsersAndGroups : DependencyObject,IEnumerable,INotifyCollectionChanged
	{

		/// <summary>
		/// Indicates the Tenant dependency property.
		/// </summary>
		public static readonly DependencyProperty TenantProperty =
			DependencyProperty.Register("Tenant", typeof(Tenant), typeof(TenantUsersAndGroups), new PropertyMetadata(OnTenantChanged));

		private ArrayList usersAndGroups = new ArrayList();

		/// <summary>
		/// Raised when the contents of the collection changes.
		/// </summary>
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		/// <summary>
		/// Sets the tenant and stores its users and groups.
		/// </summary>
		public Tenant Tenant
		{
			get { return this.GetValue(TenantUsersAndGroups.TenantProperty) as Tenant; }
			set { this.SetValue(TenantUsersAndGroups.TenantProperty, value); }
		}

		/// <summary>
		/// Handle the tenant changing.
		/// </summary>
		/// <param name="sender">The user and groups pair.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private static void OnTenantChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			TenantUsersAndGroups usersAndGroups = sender as TenantUsersAndGroups;

			usersAndGroups.usersAndGroups.Clear();
			usersAndGroups.usersAndGroups.Add(usersAndGroups.Tenant.Users);
			usersAndGroups.usersAndGroups.Add(usersAndGroups.Tenant.Groups);
			if (usersAndGroups.CollectionChanged != null)
				usersAndGroups.CollectionChanged(usersAndGroups, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

		}

		/// <summary>
		/// Retrieve the enumerator over the users and groups lists.
		/// </summary>
		/// <returns>The enumerator.</returns>
		public IEnumerator GetEnumerator()
		{

			return this.usersAndGroups.GetEnumerator();

		}

	}

}