namespace FluidTrade.Guardian
{
	using System;
	using System.Collections.Generic;
	using System.ServiceModel;
	using System.Threading;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Data;
	using System.Windows.Input;
	using System.Windows.Threading;
	using FluidTrade.Core;
	using FluidTrade.Core.Windows;
	using FluidTrade.Core.Windows.Controls;
	using FluidTrade.Guardian.Windows;

    /// <summary>
    /// Wraps a MarkThree.Windows.Controls.DynamicReport in a System.Windows.Controls.Page.
    /// </summary>
    public partial class ViewerBlotter : Viewer
    {

        // Private Static Fields
        private static ReportWorkingOrder reportWorkingOrder;
        private static ReportMatch reportMatch;
		private static ExecutionReport reportExecution;
        private static NegotiationConsole negotiationConsole;
        private static TabItem staticMatchTab;
        private static TabControl staticTabControl;

        // Constants
        private const int stressTests = 500;

        // Private Instance Fields
        private Blotter blotter;
        private TabControl tabControl;
        private TabItem tabWorkingOrder;
        private TabItem tabMatch;
		private TabItem tabExecution;
        private Object[] arguments;

        private Grid matchGrid;
        private Grid row1Grid;
			
        private RowDefinition matchRow0;
        private RowDefinition matchRow1;

        /// <summary>
        /// Identifies the FluidTrade.FluidTradeClient.ViewerBlotter.AnimationSpeed dependency property.
        /// </summary>
        public static readonly DependencyProperty AnimationSpeedProperty;

        /// <summary>
        /// Identifies the FluidTrade.FluidTradeClient.ViewerBlotter.IsHeaderFrozen dependency property.
        /// </summary>
        public static readonly DependencyProperty IsHeaderFrozenProperty;

        /// <summary>
        /// Identifies the FluidTrade.FluidTradeClient.ViewerBlotter.IsLayoutFrozen dependency property.
        /// </summary>
        public static readonly DependencyProperty IsLayoutFrozenProperty;

        /// <summary>
        /// Identifies the FluidTrade.FluidTradeClient.ViewerBlotter.IsNavigationPaneVisible dependency property.
        /// </summary>
        public static readonly DependencyProperty IsNavigationPaneVisibleProperty;

        /// <summary>
        /// Identifies the FluidTrade.FluidTradeClient.ViewerBlotter.Scale dependency property.
        /// </summary>
        public static readonly DependencyProperty ScaleProperty;

        /// <summary>
        /// Create the static resources required for a FluidTrade.FluidTradeClient.ViewerBlotter.
        /// </summary>
        static ViewerBlotter()
        {

            // AnimationSpeed Property
            ViewerBlotter.AnimationSpeedProperty = DependencyProperty.Register(
                "AnimationSpeed",
                typeof(AnimationSpeed),
                typeof(ViewerBlotter),
                new FrameworkPropertyMetadata(AnimationSpeed.Off, new PropertyChangedCallback(OnAnimationSpeedChanged)));

            // IsHeaderFrozen Property
            ViewerBlotter.IsHeaderFrozenProperty = DependencyProperty.Register(
                "IsHeaderFrozen",
                typeof(Boolean),
                typeof(ViewerBlotter),
                new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnIsHeaderFrozenChanged)));

            // IsLayoutFrozen Property
            ViewerBlotter.IsLayoutFrozenProperty = DependencyProperty.Register(
                "IsLayoutFrozen",
                typeof(Boolean),
                typeof(ViewerBlotter),
                new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnIsLayoutFrozenChanged)));

            // IsNavigationPaneVisible Property
            ViewerBlotter.IsNavigationPaneVisibleProperty = DependencyProperty.Register(
                "IsNavigationPaneVisible",
                typeof(Boolean),
                typeof(ViewerBlotter),
                new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnIsNavigationPaneVisibleChanged)));

            // Scale Property
            ViewerBlotter.ScaleProperty = DependencyProperty.Register(
                "Scale",
                typeof(Double),
                typeof(ViewerBlotter),
                new FrameworkPropertyMetadata(1.0, new PropertyChangedCallback(OnScaleChanged)));

            // For performance purposes, a single, shared report is used to display all content.  Since the greatest delays are incurred when elements are
            // removed and added to the visual tree, recycling the same report makes things appear quicker because the visual elements remain in place but the
            // data bindings to those elements points to the new content.
            ViewerBlotter.reportWorkingOrder = new ReportWorkingOrder();
            ViewerBlotter.reportMatch = new ReportMatch();
			ViewerBlotter.reportExecution = new ExecutionReport();
            ViewerBlotter.negotiationConsole = new NegotiationConsole();

        }

        private void reportMatch_SelectionChanged(object sender, EventArgs e)
        {
            ReportCell reportCell = ViewerBlotter.reportMatch.FocusedCell;
            if (reportCell != null)
            {
				ThreadPoolHelper.QueueUserWorkItem(InitializeMatchReport, reportCell);
            }
        }

		private void InitializeMatchReport(object state)
		{
			ReportCell reportCell = state as ReportCell;
			if (reportCell != null)
			{
				try
				{
					lock (DataModel.SyncRoot)
					{
						MatchRow matchRow = reportCell.ReportRow.IContent.Key as MatchRow;
						if (matchRow != null)
						{
							this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate()
							{
								ViewerBlotter.negotiationConsole.MatchId = matchRow.MatchId;
							}));
						}
					}
				}
				catch (Exception exception)
				{

					// Log any errors trying to select the detail, but don't let these errors kill the application.
					EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);
				}
			}
		}
        /// <summary>
		/// Creates an object that wraps a Markthree.Controls.DynamicReport in a Page.
        /// </summary>
        public ViewerBlotter(params Object[] arguments)
        {

            // This will display the given object in the viewer.
            this.Content = arguments[0];
            this.arguments = new Object[arguments.Length - 1];
            Array.Copy(arguments, 1, this.arguments, 0, this.arguments.Length);

            // This describes the content of the report.
            if (this.Content is Blotter)
                this.blotter = this.Content as Blotter;

            // The IDE managed components are initialized here.
            InitializeComponent();

			// Register for these events (do not forget to unregister them later).
			ViewerBlotter.reportMatch.SelectionChanged += new EventHandler(reportMatch_SelectionChanged);

            // The delegates will handle the loading and unloading of the viewer into the visual tree.
            this.Loaded += new RoutedEventHandler(OnLoaded);
            this.Unloaded += new RoutedEventHandler(OnUnloaded);

        }

        /// <summary>
        /// Gets or sets the speed of animation.
        /// </summary>
        public AnimationSpeed AnimationSpeed
        {
            get { return (AnimationSpeed)this.GetValue(ViewerBlotter.AnimationSpeedProperty); }
            set { this.SetValue(ViewerBlotter.AnimationSpeedProperty, value); }
        }

        /// <summary>
        /// Gets or sets an indication of whether the headers can be modified or not.
        /// </summary>
        private Boolean IsHeaderFrozen
        {
            get { return (Boolean)this.GetValue(ViewerBlotter.IsHeaderFrozenProperty); }
            set { this.SetValue(ViewerBlotter.IsHeaderFrozenProperty, value); }
        }

        /// <summary>
        /// Gets or sets an indication of whether the panel layout can be changed or not.
        /// </summary>
        private Boolean IsLayoutFrozen
        {
            get { return (Boolean)this.GetValue(ViewerBlotter.IsLayoutFrozenProperty); }
            set { this.SetValue(ViewerBlotter.IsLayoutFrozenProperty, value); }
        }

        /// <summary>
        /// Gets or sets an indication of whether the navigation pane is visible or not.
        /// </summary>
        private Boolean IsNavigationPaneVisible
        {
            get { return (Boolean)this.GetValue(ViewerBlotter.IsNavigationPaneVisibleProperty); }
            set { this.SetValue(ViewerBlotter.IsNavigationPaneVisibleProperty, value); }
        }

        /// <summary>
        /// Gets or sets whether the scale factor used for magnifying the content.
        /// </summary>
        private Double Scale
        {
            get { return (Double)this.GetValue(ViewerBlotter.ScaleProperty); }
            set { this.SetValue(ViewerBlotter.ScaleProperty, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static void ActivateMatch()
        {

            ViewerBlotter.staticTabControl.SelectedItem = ViewerBlotter.staticMatchTab;

        }

        /// <summary>
        /// Handles a change to the AnimationSpeed property.
        /// </summary>
        /// <param name="dependencyObject">The object that owns the property.</param>
        /// <param name="dependencyPropertyChangedEventArgs">A description of the changed property.</param>
        private static void OnAnimationSpeedChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {

            // Extract the strongly typed variables from the generic parameters.
            ViewerBlotter viewerPrototype = dependencyObject as ViewerBlotter;
            AnimationSpeed animationSpeed = (AnimationSpeed)dependencyPropertyChangedEventArgs.NewValue;

            // Set the animation speed on the report.
            ViewerBlotter.reportWorkingOrder.AnimationSpeed = animationSpeed;

            // Adjust the menu items for the new animation setting.
            viewerPrototype.menuItemSetAnimationFast.IsChecked = animationSpeed == AnimationSpeed.Fast;
            viewerPrototype.menuItemSetAnimationMedium.IsChecked = animationSpeed == AnimationSpeed.Medium;
            viewerPrototype.menuItemSetAnimationOff.IsChecked = animationSpeed == AnimationSpeed.Off;
            viewerPrototype.menuItemSetAnimationSlow.IsChecked = animationSpeed == AnimationSpeed.Slow;

        }

        private void OnCreateSlice(object sender, RoutedEventArgs e)
        {

			if (ViewerBlotter.reportWorkingOrder.Rows != null)
			{

				List<WorkingOrderRow> selectedRows = new List<WorkingOrderRow>();

				foreach (FluidTrade.Core.Windows.Controls.ReportRow reportRow in ViewerBlotter.reportWorkingOrder.Rows)
					if (reportRow.IContent.Key is WorkingOrderRow)
						selectedRows.Add(reportRow.IContent.Key as WorkingOrderRow);

				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(ExecuteSlice, selectedRows);

			}

        }

        private void ExecuteSlice(object state)
        {

            List<WorkingOrderRow> workingOrders = state as List<WorkingOrderRow>;

			// Create a channel to the middle tier.
			TradingSupportReference.TradingSupportClient tradingSupportClient = new TradingSupportReference.TradingSupportClient(Guardian.Properties.Settings.Default.TradingSupportEndpoint);

            try
            {

				List<TradingSupportReference.DestinationOrderInfo> destinationOrders = new List<TradingSupportReference.DestinationOrderInfo>();

                lock (DataModel.SyncRoot)
                {

                    DestinationRow destinationRow = DataModel.Destination.DestinationKeyExternalId0.Find("GUARDIAN ECN");

                    foreach (WorkingOrderRow workingOrderRow in workingOrders)
                    {

                        decimal sourceOrderQuantity = 0.0M;
                        foreach (SourceOrderRow sourceOrderRow in workingOrderRow.GetSourceOrderRows())
                            sourceOrderQuantity += sourceOrderRow.OrderedQuantity;

                        Decimal destinationOrderQuantity = 0.0M;
                        foreach (DestinationOrderRow destinationOrderRow in workingOrderRow.GetDestinationOrderRows())
                            destinationOrderQuantity += destinationOrderRow.OrderedQuantity;

                        if (sourceOrderQuantity > destinationOrderQuantity)
                        {

							TradingSupportReference.DestinationOrderInfo destinationOrderInfo = new TradingSupportReference.DestinationOrderInfo();
                            destinationOrderInfo.BlotterId = workingOrderRow.BlotterId;
                            destinationOrderInfo.DestinationId = destinationRow.DestinationId;
                            destinationOrderInfo.OrderedQuantity = sourceOrderQuantity - destinationOrderQuantity;
                            destinationOrderInfo.OrderTypeId = workingOrderRow.OrderTypeId;
                            destinationOrderInfo.SecurityId = workingOrderRow.SecurityId;
                            destinationOrderInfo.SettlementId = workingOrderRow.SettlementId;
                            destinationOrderInfo.SideCodeId = workingOrderRow.SideId;
                            destinationOrderInfo.TimeInForceCodeId = workingOrderRow.TimeInForceId;
                            destinationOrderInfo.WorkingOrderId = workingOrderRow.WorkingOrderId;

                            destinationOrders.Add(destinationOrderInfo);

                        }

                    }

                }
				
                tradingSupportClient.CreateDestinationOrders(destinationOrders.ToArray());

            }
            catch (FaultException<OptimisticConcurrencyFault> optimisticConcurrencyException)
            {

                // The record is busy.
                this.Dispatcher.Invoke(DispatcherPriority.Normal,
                    (MessageDelegate)((string message) => { MessageBox.Show(message, Application.Current.MainWindow.Title); }),
                    String.Format(FluidTrade.Core.Properties.Resources.OptimisticConcurrencyError,                    
                    optimisticConcurrencyException.Detail.TableName));

            }
            catch (FaultException<RecordNotFoundFault> recordNotFoundException)
            {

                // The record is busy.
                this.Dispatcher.Invoke(DispatcherPriority.Normal,
                    (MessageDelegate)((string message) => { MessageBox.Show(message, Application.Current.MainWindow.Title); }),
                    String.Format(FluidTrade.Core.Properties.Resources.RecordNotFoundError,
                    CommonConversion.FromArray(recordNotFoundException.Detail.Key),
                    recordNotFoundException.Detail.TableName));

            }
            catch (CommunicationException communicationException)
            {

                // Log communication problems.
                this.Dispatcher.Invoke(DispatcherPriority.Normal,
                    (MessageDelegate)((string message) => { MessageBox.Show(message, Application.Current.MainWindow.Title); }),
                    communicationException.Message);

            }
            catch (Exception exception)
            {

                // Log communication problems.
                EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);

            }
			finally
			{

				if (tradingSupportClient != null && tradingSupportClient.State == CommunicationState.Opened)
					tradingSupportClient.Close();
			}
        }

        /// <summary>
        /// Handles the loading of this control into the visual tree.
        /// </summary>
        /// <param name="sender">The object that originated the event.</param>
        /// <param name="e">The unused routed event arguments.</param>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {

            this.tabControl = new TabControl();
            ViewerBlotter.staticTabControl = this.tabControl;
            this.SetValue(Page.ContentProperty, tabControl);

            // Working Order Tab
            this.tabWorkingOrder = new TabItem();
            this.tabWorkingOrder.Header = "Working Orders";

            // Match Tab
            this.tabMatch = new TabItem();
            ViewerBlotter.staticMatchTab = this.tabMatch;
            this.tabMatch.Header = "Matches";
            this.matchGrid = new Grid();
            this.matchRow0 = new RowDefinition();
            this.matchRow0.Height = new GridLength(1.0, GridUnitType.Star);
            this.matchGrid.RowDefinitions.Add(matchRow0);

            this.matchRow1 = new RowDefinition();
            this.matchRow1.Height = GridLength.Auto;
            this.matchGrid.RowDefinitions.Add(matchRow1);

            this.row1Grid = new Grid();
            Grid.SetColumn(this.row1Grid, 0);
            Grid.SetRow(this.row1Grid, 0);
            ColumnDefinition matchColumn10 = new ColumnDefinition();
            matchColumn10.Width = new GridLength(1.0, GridUnitType.Star);
            this.row1Grid.ColumnDefinitions.Add(matchColumn10);
            ColumnDefinition matchColumn11 = new ColumnDefinition();
            matchColumn11.Width = GridLength.Auto;
            this.row1Grid.ColumnDefinitions.Add(matchColumn11);
            this.matchGrid.Children.Add(this.row1Grid);

			this.tabExecution = new TabItem();
			this.tabExecution.Header = "Execution";
			this.tabExecution.Content = ViewerBlotter.reportExecution;

            // Add the tabs to the page.
            tabControl.Items.Add(this.tabWorkingOrder);
            tabControl.Items.Add(this.tabMatch);
			tabControl.Items.Add(this.tabExecution);

            // The lion's share of time to load a report is adding the child user interface elements to the report.  Recycling the cells from one report to
            // another makes things move much faster.  The viewer creates a single, static Prototype Report for its content and reuses it from one viewer to
            // the next.  The downside to this architecture is that the binding must be done in code because the XAML doesn't have access to the static report.
            this.tabWorkingOrder.Content = ViewerBlotter.reportWorkingOrder;

            this.row1Grid.Children.Add(ViewerBlotter.reportMatch);
            this.row1Grid.Children.Add(ViewerBlotter.negotiationConsole);
            Grid.SetColumn(ViewerBlotter.reportMatch, 0);
            Grid.SetRow(ViewerBlotter.reportMatch, 0);
            Grid.SetColumn(ViewerBlotter.negotiationConsole, 1);
            Grid.SetRow(ViewerBlotter.negotiationConsole, 0);
            this.tabMatch.Content = this.matchGrid;

            // This selects which orders are displayed in the viewer.
            ViewerBlotter.reportWorkingOrder.BlotterId = this.blotter.BlotterId;
            ViewerBlotter.reportMatch.BlotterId = this.blotter.BlotterId;
			ViewerBlotter.reportExecution.BlotterId = this.blotter.BlotterId;

            if (this.arguments.Length > 0)
            {
                if (this.arguments[0] is Match)
                {
                    this.tabControl.SelectedItem = this.tabMatch;
                }
            }

            // Bind the "AnimationSpeed" property to the setting.
            Binding bindingAnimationSpeed = new Binding("AnimationSpeed");
            bindingAnimationSpeed.Source = FluidTrade.Guardian.Properties.Settings.Default;
            bindingAnimationSpeed.Mode = BindingMode.TwoWay;
            BindingOperations.SetBinding(this, ViewerBlotter.AnimationSpeedProperty, bindingAnimationSpeed);

            // Bind the "IsHeaderFrozen" property to the setting.
            Binding bindingApplyHeaderFrozen = new Binding("IsHeaderFrozen");
            bindingApplyHeaderFrozen.Source = FluidTrade.Guardian.Properties.Settings.Default;
            bindingApplyHeaderFrozen.Mode = BindingMode.TwoWay;
            BindingOperations.SetBinding(this, ViewerBlotter.IsHeaderFrozenProperty, bindingApplyHeaderFrozen);

            // Bind the "IsLayoutFrozen" property to the setting.
            Binding bindingApplyLayoutFrozen = new Binding("IsLayoutFrozen");
            bindingApplyLayoutFrozen.Source = FluidTrade.Guardian.Properties.Settings.Default;
            bindingApplyLayoutFrozen.Mode = BindingMode.TwoWay;
            BindingOperations.SetBinding(this, ViewerBlotter.IsLayoutFrozenProperty, bindingApplyLayoutFrozen);

            // Bind the "IsNavigationPaneVisible" property to the settings.
            Binding bindingIsNavigationPaneVisible = new Binding("IsNavigationPaneVisible");
            bindingIsNavigationPaneVisible.Source = FluidTrade.Guardian.Properties.Settings.Default;
            bindingIsNavigationPaneVisible.Mode = BindingMode.TwoWay;
            BindingOperations.SetBinding(this, ViewerBlotter.IsNavigationPaneVisibleProperty, bindingIsNavigationPaneVisible);

            // Bind the "Scale" property to the setting.
            Binding bindingSliderScale = new Binding("Scale");
            bindingSliderScale.Source = FluidTrade.Guardian.Properties.Settings.Default;
            bindingSliderScale.Mode = BindingMode.TwoWay;
            BindingOperations.SetBinding(this, ViewerBlotter.ScaleProperty, bindingSliderScale);

        }

        /// <summary>
        /// Handles the unloading of this control from the visual tree.
        /// </summary>
        /// <param name="sender">The object that originated the event.</param>
        /// <param name="e">The unused routed event arguments.</param>
        void OnUnloaded(object sender, RoutedEventArgs e)
        {

			///////HACK  -  Unloaded is being calle before loaded.  To replicate, try to switch blotters really fast using navigation
			///tree view.
			if (this.tabWorkingOrder == null)
			{
				EventLog.Error("Report Unloading before loading");
				return;
			}
			////End hack.

			// Unregister for the events when the reports are changed otherwise repeated calls to the methods occur and will appear as memory leak.
			ViewerBlotter.reportMatch.SelectionChanged -= new EventHandler(reportMatch_SelectionChanged);

            this.tabWorkingOrder.Content = null;
            this.row1Grid.Children.Remove(ViewerBlotter.reportMatch);
            this.row1Grid.Children.Remove(ViewerBlotter.negotiationConsole);
			this.tabExecution.Content = null;

            // When the object is removed from the visual tree, there is no need to hold on to the bindings to the settings.
            // Another viewer may be installed and there's no reason why this viewer still needs to be a load on the settings data
            // structure.
            BindingOperations.ClearBinding(this, ViewerBlotter.AnimationSpeedProperty);
            BindingOperations.ClearBinding(this, ViewerBlotter.IsHeaderFrozenProperty);
            BindingOperations.ClearBinding(this, ViewerBlotter.IsLayoutFrozenProperty);
            BindingOperations.ClearBinding(this, ViewerBlotter.IsNavigationPaneVisibleProperty);
            BindingOperations.ClearBinding(this, ViewerBlotter.ScaleProperty);

            // Removing the shared content from the viewer allows this viewer to be garbage collected while the core report will 
            // stay around until the current application domain is unloaded.
            this.SetValue(Page.ContentProperty, null);

        }

        /// <summary>
        /// Handles a change to the IsFilledFilter property.
        /// </summary>
        /// <param name="dependencyObject">The object that owns the property.</param>
        /// <param name="dependencyPropertyChangedEventArgs">A description of the changed property.</param>
        private static void OnIsFilledFilterChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {

            // Extract the strongly typed variables from the generic parameters.
            ViewerBlotter viewerPrototype = dependencyObject as ViewerBlotter;
            Boolean isFilledFilter = (Boolean)dependencyPropertyChangedEventArgs.NewValue;

            // This takes care of the actual work of setting the filter to display only the filled orders (or removing the filter).
            ViewerBlotter.reportWorkingOrder.IsFilledFilter = isFilledFilter;

        }

        /// <summary>
        /// Handles a change to the IsHeaderFrozen property.
        /// </summary>
        /// <param name="dependencyObject">The object that owns the property.</param>
        /// <param name="dependencyPropertyChangedEventArgs">A description of the changed property.</param>
        private static void OnIsHeaderFrozenChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {

            // Extract the strongly typed variables from the generic parameters.
            ViewerBlotter viewerPrototype = dependencyObject as ViewerBlotter;
            Boolean isHeaderFrozen = (Boolean)dependencyPropertyChangedEventArgs.NewValue;

            // This takes care of the actual work of setting the filter to display only the filled orders (or removing the filter).
            ViewerBlotter.reportWorkingOrder.IsHeaderFrozen = isHeaderFrozen;
            ViewerBlotter.reportMatch.IsHeaderFrozen = isHeaderFrozen;
			ViewerBlotter.reportExecution.IsHeaderFrozen = isHeaderFrozen;

            // The user interface is modified here to reflect the change to the property.
            if (viewerPrototype.toggleButtonIsHeaderFrozen.IsChecked != isHeaderFrozen)
                viewerPrototype.toggleButtonIsHeaderFrozen.IsChecked = isHeaderFrozen;

        }

        /// <summary>
        /// Handles a change to the IsLayoutFrozen property.
        /// </summary>
        /// <param name="dependencyObject">The object that owns the property.</param>
        /// <param name="dependencyPropertyChangedEventArgs">A description of the changed property.</param>
        private static void OnIsLayoutFrozenChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {

            // Extract the strongly typed variables from the generic parameters.
            ViewerBlotter viewerPrototype = dependencyObject as ViewerBlotter;
            Boolean isLayoutFrozen = (Boolean)dependencyPropertyChangedEventArgs.NewValue;

            // This takes care of the actual work of setting the filter to display only the filled orders (or removing the filter).
            ViewerBlotter.reportWorkingOrder.IsLayoutFrozen = isLayoutFrozen;

        }

        /// <summary>
        /// Handles a change to the IsNavigationPaneVisible property.
        /// </summary>
        /// <param name="dependencyObject">The object that owns the property.</param>
        /// <param name="dependencyPropertyChangedEventArgs">A description of the changed property.</param>
        private static void OnIsNavigationPaneVisibleChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {

            // Extract the strongly typed variables from the generic parameters.
            ViewerBlotter viewerPrototype = dependencyObject as ViewerBlotter;
            Boolean isNavigationPaneVisible = (Boolean)dependencyPropertyChangedEventArgs.NewValue;

            // The real logic is handled by the application window.  This handler is only got to update the menu user interface to
            // reflect the state of the navigation pane.
            if (viewerPrototype.menuItemIsNavigationPaneVisible.IsChecked != isNavigationPaneVisible)
                viewerPrototype.menuItemIsNavigationPaneVisible.IsChecked = isNavigationPaneVisible;

        }

        /// <summary>
        /// Handles a change to the Scale property.
        /// </summary>
        /// <param name="dependencyObject">The object that owns the property.</param>
        /// <param name="dependencyPropertyChangedEventArgs">A description of the changed property.</param>
        private static void OnScaleChanged(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {

            // Extract the strongly typed variables from the generic parameters.
            ViewerBlotter viewerPrototype = dependencyObject as ViewerBlotter;
            Double scale = (Double)dependencyPropertyChangedEventArgs.NewValue;

            // Set the animation speed on the report.
            ViewerBlotter.reportWorkingOrder.Scale = scale;
            ViewerBlotter.reportMatch.Scale = scale;
			ViewerBlotter.reportExecution.Scale = scale;

        }

        /// <summary>
        /// Selects the visible columns of the report.
        /// </summary>
        /// <param name="sender">The object that originated this event.</param>
        /// <param name="e">The unused event argments.</param>
        private void OnSelectColumns(object sender, ExecutedRoutedEventArgs e)
        {

            // Allow the user to select the columns for the report.
            ViewerBlotter.reportWorkingOrder.SelectColumns();

        }

        /// <summary>
        /// Selects a slow animation speed.
        /// </summary>
        /// <param name="sender">The object that originated this event.</param>
        /// <param name="e">The unused event argments.</param>
        private void OnSetAnimationSlow(object sender, ExecutedRoutedEventArgs e)
        {

            // Set the animation speed.
            this.AnimationSpeed = AnimationSpeed.Slow;

        }

        /// <summary>
        /// Selects a medium animation speed.
        /// </summary>
        /// <param name="sender">The object that originated this event.</param>
        /// <param name="e">The unused event argments.</param>
        private void OnSetAnimationMedium(object sender, ExecutedRoutedEventArgs e)
        {

            // Set the animation speed.
            this.AnimationSpeed = AnimationSpeed.Medium;

        }

        /// <summary>
        /// Selects a fast animation speed.
        /// </summary>
        /// <param name="sender">The object that originated this event.</param>
        /// <param name="e">The unused event argments.</param>
        private void OnSetAnimationFast(object sender, ExecutedRoutedEventArgs e)
        {

            // Set the animation speed.
            this.AnimationSpeed = AnimationSpeed.Fast;

        }

        /// <summary>
        /// Selects a fast animation speed.
        /// </summary>
        /// <param name="sender">The object that originated this event.</param>
        /// <param name="e">The unused event argments.</param>
        private void OnSetAnimationOff(object sender, ExecutedRoutedEventArgs e)
        {

            // Set the animation speed.
            this.AnimationSpeed = AnimationSpeed.Off;

        }

        /// <summary>
        /// Prevents or allows changes to the column and row headers.
        /// </summary>
        /// <param name="sender">The object that originated this event.</param>
        /// <param name="routedEventArgs">The unused event argments.</param>
        private void OnSetIsHeaderFrozen(object sender, RoutedEventArgs routedEventArgs)
        {

            // Freeze or thaw the column and row headings.
            this.IsHeaderFrozen = !this.IsHeaderFrozen;

        }

        /// <summary>
        /// Prevents or allows changes to the layout of the headers.
        /// </summary>
        /// <param name="sender">The object that originated this event.</param>
        /// <param name="routedEventArgs">The unused event argments.</param>
        private void OnSetIsLayoutFrozen(object sender, RoutedEventArgs routedEventArgs)
        {

            // Freeze or thaw the the ability to change the layout of the headings.
            this.IsLayoutFrozen = !this.IsLayoutFrozen;

        }

        /// <summary>
        /// Checks or clears the 'IsNavigationPaneVisible' menu item.
        /// </summary>
        /// <param name="sender">The object that originated this event.</param>
        /// <param name="routedEventArgs">The unused event argments.</param>
        private void OnSetIsNavigationPaneVisible(object sender, RoutedEventArgs routedEventArgs)
        {

            // Freeze or thaw the the ability to change the layout of the headings.
            this.IsNavigationPaneVisible = !this.IsNavigationPaneVisible;

        }

        /// <summary>
        /// Changes the text used to filter symbols from the list of orders.
        /// </summary>
        /// <param name="sender">The object that originated this event.</param>
        /// <param name="e">The unused event argments.</param>
        private void OnTextBoxSymbolTextChanged(object sender, TextChangedEventArgs e)
        {

            // This will remove any orders from the report that don't start with the given text.
            TextBox textBox = sender as TextBox;
            ViewerBlotter.reportWorkingOrder.SymbolFilter = textBox.Text;

        }
    }

}
