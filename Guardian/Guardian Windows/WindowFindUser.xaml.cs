namespace FluidTrade.Guardian.Windows
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Data;
	using System.Windows.Documents;
	using System.Windows.Input;
	using System.Windows.Media;
	using System.Windows.Media.Imaging;
	using System.Windows.Navigation;
	using System.Windows.Shapes;
	using System.Windows.Threading;
	using FluidTrade.Core;
	using FluidTrade.Guardian;

    /// <summary>
    /// Interaction logic for WindowFindUser.xaml
    /// </summary>
    public partial class WindowFindUser
    {

		/// <summary>
		/// Indicates the IncludeUsers dependency property.
		/// </summary>
		public static readonly DependencyProperty IncludeUsersProperty =
			DependencyProperty.Register("IncludeUsers", typeof(Boolean), typeof(WindowFindUser), new PropertyMetadata(true));
		/// <summary>
		/// Indicates the IncludeGroups dependency property.
		/// </summary>
		public static readonly DependencyProperty IncludeGroupsProperty =
			DependencyProperty.Register("IncludeGroups", typeof(Boolean), typeof(WindowFindUser), new PropertyMetadata(true));

		private DataBoundList<RightsHolder> allRightsHolders;
        private RightsHolder selectedUser = null;

        private delegate void populate(List<RightsHolder> users);
        
        /// <summary>
        /// Create a new find-user window.
        /// </summary>
        public WindowFindUser()
        {

            InitializeComponent();
			this.Cursor = Cursors.AppStarting;

            this.Loaded += delegate(object sender, RoutedEventArgs eventArgs)
            {
				this.allRightsHolders = !this.IncludeGroups ? new UserList() : !this.IncludeUsers ? new GroupList() : new RightsHolderList(false);
				this.allRightsHolders.Initialized += delegate(object s, EventArgs e)
				{
					this.Cursor = Cursors.Arrow;
					this.OnSearchBoxTextChanged(s, e);
				};

				this.usersFound.ItemsSource = this.allRightsHolders;

				this.Cursor = Cursors.AppStarting;
				this.searchBox.Focus();
            };

			this.Closed += (s, e) => this.allRightsHolders.Dispose();

        }

		/// <summary>
		/// Whether to include groups in the search list.
		/// </summary>
		public Boolean IncludeGroups
		{

			get { return (Boolean)this.GetValue(WindowFindUser.IncludeGroupsProperty); }
			set { this.SetValue(WindowFindUser.IncludeGroupsProperty, value); }

		}

		/// <summary>
		/// Whether to include users in the search list.
		/// </summary>
		public Boolean IncludeUsers
		{

			get { return (Boolean)this.GetValue(WindowFindUser.IncludeUsersProperty); }
			set { this.SetValue(WindowFindUser.IncludeUsersProperty, value); }

		}

        /// <summary>
        /// Abandon the search and close the window.
        /// </summary>
        /// <param name="sender">The close button.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void OnCancel(object sender, RoutedEventArgs eventArgs)
        {

            this.Close();

        }

        /// <summary>
        /// Save the selected user and close the window. If there was no selection and only a single matched user, save that user instead.
        /// </summary>
        /// <param name="sender">The okay button.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void OnOkay(object sender, RoutedEventArgs eventArgs)
        {

            if (usersFound.Items.Count == 1)
                this.selectedUser = this.usersFound.Items[0] as RightsHolder;
            else if (usersFound.Items.Count > 0)
                this.selectedUser = this.usersFound.SelectedItem as RightsHolder;
            this.Close();

        }

		/// <summary>
		/// Update the results list when the search text changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="eventArgs"></param>
        private void OnSearchBoxTextChanged(object sender, EventArgs eventArgs)
        {

			if (this.usersFound.MultiTenant)
				this.usersFound.ItemsSource = this.allRightsHolders.Where(u => u.QualifiedName.ToLower().Contains(this.searchBox.Text.ToLower()));
			else
				this.usersFound.ItemsSource = this.allRightsHolders.Where(u => u.Name.ToLower().Contains(this.searchBox.Text.ToLower()));

        }

        /// <summary>
        /// The user that was selected from the list of matching users. If no user was selected, or the cancel was clicked, SelectedUser is null.
        /// </summary>
        public RightsHolder SelectedUser
        {

            get { return selectedUser; }

        }

    }

}
