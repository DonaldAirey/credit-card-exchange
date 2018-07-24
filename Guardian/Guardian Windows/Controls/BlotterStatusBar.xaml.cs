namespace FluidTrade.Guardian.Windows.Controls
{

    using System;
    using System.Collections.Generic;
    using System.IO;
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

    /// <summary>
    /// Interaction logic for BlotterStatusBar.xaml
    /// </summary>
    public partial class BlotterStatusBar : EntityStatusBar
    {

        // Private Delegates
        private delegate void SetStatusBarDelegate(String workingOrders, String matches);

        // Private Fields
		/// <summary>
		/// The Blotter this status bar is for - for use by worker threads.
		/// </summary>
        protected Guid entityId;

		/// <summary>
        /// The zoom scale dependency property.
        /// </summary>
        public static DependencyProperty ZoomProperty;

        private TextBlock workingOrdersCount;
        private TextBlock matchesCount;

        /// <summary>
        /// Register the dependency properties.
        /// </summary>
        static BlotterStatusBar()
        {

            ZoomProperty = DependencyProperty.Register("Zoom", typeof(Double), typeof(BlotterStatusBar));

        }

        /// <summary>
        /// Create a new status bar for a blotter.
        /// </summary>
        public BlotterStatusBar()
        {

            TextBlock workingOrders = new TextBlock()
                {
                    Text = "Working Accounts: ",
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Colors.Gray),
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                    VerticalAlignment = System.Windows.VerticalAlignment.Center,
                    Margin = new Thickness(5, 0, 10, 0)
                };
            TextBlock matches = new TextBlock()
                {
                    Text = "Matches: ",
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Colors.Gray),
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                    VerticalAlignment = System.Windows.VerticalAlignment.Center,
                    Margin = new Thickness(5, 0, 10, 0)
                };

            this.workingOrdersCount = new TextBlock()
                {
                    FontSize = 12,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                    VerticalAlignment = System.Windows.VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 10, 2)
                };
            this.matchesCount = new TextBlock()
                {
                    FontSize = 12,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                    VerticalAlignment = System.Windows.VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 10, 2)
                };

            InitializeComponent();
            base.Children.Add(new FlowGridRow() { workingOrders, this.workingOrdersCount });
            base.Children.Add(new FlowGridRow() { matches, this.matchesCount });

            // The delegates will handle the loading and unloading of the viewer into the visual tree.
            this.Loaded += new RoutedEventHandler(OnLoaded);
            this.Unloaded += new RoutedEventHandler(OnUnloaded);

        }

        /// <summary>
        /// Count all of the working orders and matches in blotters below the parent entity. The caller should lock the DataModel.
        /// </summary>
        /// <param name="parent">The parent entity.</param>
        /// <param name="orders">The total number of working orders. This should initially be 0.</param>
        /// <param name="matches">The total number of matches. This should initially be 0.</param>
        private void CountChildOrders(EntityRow parent, ref int orders, ref int matches)
        {

            BlotterRow blotter = DataModel.Blotter.BlotterKey.Find(parent.EntityId);

            if (blotter != null)
            {

                orders += blotter.GetWorkingOrderRows().Count();
                matches += blotter.GetMatchRows().Count();

            }

            foreach (EntityTreeRow tree in DataModel.EntityTree)
                if (tree.ParentId == parent.EntityId)
                    this.CountChildOrders(tree.EntityRowByFK_Entity_EntityTree_ChildId, ref orders, ref matches);

        }

		/// <summary>
		/// Handles a change to the blotter property.
		/// </summary>
		/// <param name="eventArgs">The unused routed event arguments.</param>
		protected override void OnEntityChanged(DependencyPropertyChangedEventArgs eventArgs)
		{

			Blotter blotter = eventArgs.NewValue as Blotter;
			FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(this.SetBlotter, blotter.EntityId);

		}

		/// <summary>
        /// Tests to see if the slider can decrease the scale factor.
        /// </summary>
        /// <param name="sender">The object that originated the event.</param>
        /// <param name="canExecuteRoutedEventArgs">The unused routed event arguments.</param>
        private void OnCanDecreaseZoom(object sender, CanExecuteRoutedEventArgs canExecuteRoutedEventArgs)
        {

            // This tests to see if the maginification slider exists in the visual tree and whether its value is still greater than
            // the minimum scale factor.
            canExecuteRoutedEventArgs.CanExecute = this.sliderScale == null ? false :
                Math.Round(this.Zoom, 2) > Math.Round(this.sliderScale.Minimum, 2);

        }

        /// <summary>
        /// Tests to see if the slider can increase the scale factor.
        /// </summary>
        /// <param name="sender">The object that originated the event.</param>
        /// <param name="canExecuteRoutedEventArgs">The unused routed event arguments.</param>
        private void OnCanIncreaseZoom(object sender, CanExecuteRoutedEventArgs canExecuteRoutedEventArgs)
        {

            // This tests to see if the maginification slider exists in the visual tree and whether its value is still less than
            // the maximum scale factor.
            canExecuteRoutedEventArgs.CanExecute = this.sliderScale == null ? false :
                Math.Round(this.Zoom, 2) < Math.Round(this.sliderScale.Maximum, 2);

        }

        /// <summary>
        /// Decrease the scale factor.
        /// </summary>
        /// <param name="sender">The object that originated the event.</param>
        /// <param name="routedEventArgs">The unused routed event arguments.</param>
        private void OnDecreaseZoom(object sender, RoutedEventArgs routedEventArgs)
        {

            // This will decrease the scale factor until it reflects the minimum magnification allowable.
            Double target = this.sliderScale.Value - this.sliderScale.SmallChange;
            this.Zoom = target < this.sliderScale.Minimum ? this.sliderScale.Minimum : target;

        }

        /// <summary>
        /// Increases the scale factor.
        /// </summary>
        /// <param name="sender">The object that originated the event.</param>
        /// <param name="routedEventArgs">The unused routed event arguments.</param>
        private void OnIncreaseZoom(object sender, RoutedEventArgs routedEventArgs)
        {

            // This will increase the scale factor until it reflects the maximum magnification allowable.
            Double target = this.sliderScale.Value + this.sliderScale.SmallChange;
            this.Zoom = target > this.sliderScale.Maximum ? this.sliderScale.Maximum : target;

        }

		/// <summary>
		/// Set the statusbar to update when the blotter (the DataModel) updates.
		/// </summary>
		/// <param name="sender">The status bar.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnLoaded(Object sender, RoutedEventArgs eventArgs)
		{

			// The EndMerge will keep the status bar synchronized with the data model.
			DataModel.EndMerge += new EventHandler(this.PopulateHandler);

			// This will initialize the status bar when the object is first loaded.
			this.PopulateHandler(this, EventArgs.Empty);

		}

		/// <summary>
		/// Make sure the statusbar is no longer updated after the blotter is no longer in view.
		/// </summary>
		/// <param name="sender">The status bar.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnUnloaded(Object sender, RoutedEventArgs eventArgs)
		{

			// When this control is no longer visible it is removed from the event handlers called at the end of a merge.
			DataModel.EndMerge -= this.PopulateHandler;

		}

		/// <summary>
        /// Initializes the status bar.
        /// </summary>
        /// <param name="workingOrders">The number of working orders.</param>
        /// <param name="matches">The number of matches.</param>
        private void Populate(String workingOrders, String matches)
        {

            // Initialize the status bar elements
            this.workingOrdersCount.Text = workingOrders;
            this.matchesCount.Text = matches;

        }

        /// <summary>
        /// Populates the status bar with information about the current blotter.
        /// </summary>
        protected virtual void Populate()
        {

            try
            {

                // This will pass on the useful bits of information from the data model to the foreground where they can be displayed in the status bar.
				Boolean entityDeleted = false;
                int workingOrders = 0;
                int matches = 0;

                // Collect data from the data model to be sent to the foreground.
                lock (DataModel.SyncRoot)
                {

                    EntityRow entityRow = DataModel.Entity.EntityKey.Find(this.entityId);

					if (entityRow != null)
					{

						CountChildOrders(entityRow, ref workingOrders, ref matches);

					}
					else
					{

						entityDeleted = true;

					}

                }

                // Call the foreground thread to display the information collected from the background.
				if (!entityDeleted)
					this.Dispatcher.BeginInvoke(
						DispatcherPriority.Normal,
						new SetStatusBarDelegate(Populate),
						workingOrders.ToString(),
						matches.ToString());

            }
            catch
            {

                // If we fail, we'll just keep up the old status bar.

            }

        }

        /// <summary>
        /// Initializes the status bar.
        /// </summary>
        /// <param name="state">The unused starting parameters for the thread.</param>
        private void Populate(object state)
        {

            // The background thread will call the virtual method to populate the status bar.
            this.Populate();

        }

        /// <summary>
        /// Queue the Populate method in a background thread.
        /// </summary>
        /// <param name="sender">The status bar.</param>
        /// <param name="eventArgs">The event arguments.</param>
		private void PopulateHandler(object sender, EventArgs eventArgs)
		{

			// Information from the shared data model is required to initialize the frame of the viewer.
			FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(this.Populate);

		}

		/// <summary>
		/// Sets the Blotter field used by the background processes to identify this control.
		/// </summary>
		/// <param name="state">The thread initialization parameter.</param>
		private void SetBlotter(object state)
		{

			// This value is used by the background to give an identity to the control.  The Windows Properties can't be used because they are created on a
			// different thread.
			this.entityId = (Guid)state;

		}

		/// <summary>
        /// The slider zoom control.
        /// </summary>
        public Slider Slider
        {

            get { return this.sliderScale; }

        }

        /// <summary>
        /// The current zoom of the slider control.
        /// </summary>
        public double Zoom
        {

            get { return (double)this.GetValue(BlotterStatusBar.ZoomProperty); }
            set { this.SetValue(BlotterStatusBar.ZoomProperty, value); }

        }

    }

}
