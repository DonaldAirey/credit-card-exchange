namespace FluidTrade.Guardian.Windows
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Data;
	using System.Windows.Input;
	using System.Windows.Media;
	using System.Windows.Media.Imaging;
	using System.Windows.Navigation;
	using System.Windows.Shapes;
	using FluidTrade.Guardian;
	using System.Collections.Specialized;
	using System.Collections;

	/// <summary>
	/// Interaction logic for WindowUserManager.xaml
	/// </summary>
	public partial class WindowUserManager : Window
	{

		/// <summary>
		/// Indicates the Organization property.
		/// </summary>
		public static readonly DependencyPropertyKey OrganizationsProperty =
			DependencyProperty.RegisterReadOnly("Organizations", typeof(TenantList), typeof(WindowUserManager), new PropertyMetadata(null));
		/// <summary>
		/// Indicates the Path property.
		/// </summary>
		public static readonly DependencyProperty PathProperty =
			DependencyProperty.Register("Path", typeof(String), typeof(WindowUserManager), new PropertyMetadata(OnSelectedItemChanged));
		/// <summary>
		/// Indicates the SelectedItem property.
		/// </summary>
		public static readonly DependencyProperty SelectedItemProperty =
			DependencyProperty.Register("SelectedItem", typeof(object), typeof(WindowUserManager), new PropertyMetadata(OnSelectedItemChanged));
		/// <summary>
		/// Indicates the ViewType property.
		/// </summary>
		public static readonly DependencyProperty ViewTypeProperty =
			DependencyProperty.Register("ViewType", typeof(ViewType), typeof(WindowUserManager));

		private TenantList organizations;

		/// <summary>
		/// Create a new lusr manager.
		/// </summary>
		public WindowUserManager()
		{
		
			InitializeComponent();

			this.Loaded += (s, e) =>
				this.Organizations = new TenantList();
			this.Unloaded += (s, e) =>
				this.Dispose();
		
		}

		/// <summary>
		/// The list of organizations the user has access to.
		/// </summary>
		public TenantList Organizations
		{
			get { return this.organizations; }
			private set
			{

				this.organizations = value;
				this.SetValue(WindowUserManager.OrganizationsProperty, value);
				this.organizations.CollectionChanged += delegate(object s, NotifyCollectionChangedEventArgs e)
				{
					this.tree.Items.Clear();
					this.Organizations.ForEach((t) =>
						this.tree.Items.Add(
						new TreeViewItem() { Header = t, ItemsSource = new TenantUsersAndGroups() { Tenant = t } }));
				};

			}

		}

		/// <summary>
		/// The current path.
		/// </summary>
		public String Path
		{
			get { return this.GetValue(WindowUserManager.PathProperty) as String; }
			set { this.SetValue(WindowUserManager.PathProperty, value); }
		}

		/// <summary>
		/// The current selected tenant, group list, or user list.
		/// </summary>
		public object SelectedItem
		{
			get { return this.GetValue(WindowUserManager.SelectedItemProperty); }
			set { this.SetValue(WindowUserManager.SelectedItemProperty, value); }
		}

		/// <summary>
		/// The view type.
		/// </summary>
		public ViewType ViewType
		{
			get { return (ViewType)this.GetValue(WindowUserManager.ViewTypeProperty); }
			set { this.SetValue(WindowUserManager.ViewTypeProperty, value); }
		}

		/// <summary>
		/// Handle the properties command being executed.
		/// </summary>
		/// <param name="sender">The object the properties command was executed by.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnProperties(object sender, ExecutedRoutedEventArgs eventArgs)
		{

			(eventArgs.Parameter as Entity).ShowProperties(null);

		}

		/// <summary>
		/// Dispose of the tenant list.
		/// </summary>
		private void Dispose()
		{

			if (this.Organizations != null)
				this.Organizations.Dispose();

		}

		/// <summary>
		/// Handle the selected item changing.
		/// </summary>
		/// <param name="sender">The window itself.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private static void OnSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			WindowUserManager manager = sender as WindowUserManager;

			if (manager.SelectedItem is UserList)
				manager.Path = String.Format(
					@"{0}\Users",
					manager.Organizations.FirstOrDefault(t => t.TenantId == (manager.SelectedItem as UserList).Tenant));
			else if (manager.SelectedItem is GroupList)
				manager.Path = String.Format(
					@"{0}\Groups",
					manager.Organizations.FirstOrDefault(t => t.TenantId == (manager.SelectedItem as GroupList).Tenant));
			else if (manager.SelectedItem is TenantUsersAndGroups)
				manager.Path = String.Format(
					@"{0}",
					(manager.SelectedItem as TenantUsersAndGroups).Tenant);
			else
				manager.Path = null;


		}

		/// <summary>
		/// Handle a change to the selection in the tree.
		/// </summary>
		/// <param name="sender">The tree.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnSelectedTreeItemChanged(object sender, RoutedPropertyChangedEventArgs<object> eventArgs)
		{

			if (eventArgs.NewValue is TreeViewItem)
				this.SelectedItem = (eventArgs.NewValue as TreeViewItem).ItemsSource;
			else
				this.SelectedItem = eventArgs.NewValue;

			if (this.ViewType == ViewType.Detail)
			{

				if (this.SelectedItem is TenantUsersAndGroups)
					this.list.View = this.Resources["tenantGridView"] as GridView;
				else if (this.SelectedItem is UserList)
					this.list.View = this.Resources["userGridView"] as GridView;
				else if (this.SelectedItem is GroupList)
					this.list.View = this.Resources["groupGridView"] as GridView;

			}

		}

		/// <summary>
		/// Handle the view changing.
		/// </summary>
		/// <param name="sender">A context menu.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnView(object sender, ExecutedRoutedEventArgs eventArgs)
		{

			this.ViewType = (ViewType)eventArgs.Parameter;

			if (this.ViewType == ViewType.Detail)
			{

				if (this.SelectedItem is TenantUsersAndGroups)
					this.list.View = this.Resources["tenantGridView"] as GridView;
				else if (this.SelectedItem is UserList)
					this.list.View = this.Resources["userGridView"] as GridView;
				else if (this.SelectedItem is GroupList)
					this.list.View = this.Resources["groupGridView"] as GridView;

			}
			else
			{

				this.list.View = null;

			}

		}

	}

}
