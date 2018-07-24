namespace FluidTrade.Guardian.Windows
{

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;
    using System.Transactions;
    using FluidTrade.Guardian.Windows;
    using FluidTrade.Guardian;
	using System.Threading;
	using System.Windows.Threading;

    /// <summary>
    /// Interaction logic for WindowDeleteSingle.xaml
    /// </summary>
    public partial class WindowDeleteSingle : Window
    {

        // Private Instance Fields
        private GuardianObject entity;

        /// <summary>
        /// Create the delete-confirmation dialog box.
        /// </summary>
        public WindowDeleteSingle()
        {

            InitializeComponent();
			this.Owner = Application.Current.MainWindow;

        }

        /// <summary>
        /// The entity we're attempting to delete. Set this before opening the
        /// dialog box, as it is used to fill out information in the window.
        /// </summary>
        public GuardianObject Entity
        {

            get { return entity; }
            set
            {

                entity = value;

				this.DataContext = entity;

            }

        }

        /// <summary>
        /// Handles clicking of the 'No' button. No further action is required - just close the window.
        /// </summary>
        /// <param name="sender">The 'No' button.</param>
        /// <param name="arguments">Parameters for the event.</param>
        private void OnCancel(object sender, RoutedEventArgs arguments)
        {

            this.Close();

        }

        /// <summary>
        /// Handles clicking of the 'Yes' button. Delete the target entity and related entities etc. and close the window.
        /// </summary>
        /// <param name="sender">The 'Yes' button.</param>
        /// <param name="arguments">Parameters for the event.</param>
        private void OnOkay(object sender, RoutedEventArgs arguments)
        {

			WindowDeleteProgress deleteEntity = new WindowDeleteProgress();

			deleteEntity.DeleteList = new List<GuardianObject>();
			deleteEntity.DeleteList.Add(this.Entity);
			this.Close();
			deleteEntity.Show();

        }

    }

}
