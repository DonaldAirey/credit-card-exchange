namespace FluidTrade.Guardian
{
    using System;
	using System.Linq;
    using System.Collections.Generic;
	using System.IO;
	using System.Reflection;
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
    using FluidTrade.Guardian.Windows.Controls;
	using Microsoft.Win32;
	using System.Windows.Media;
    
	/// <summary>
	/// Wraps a FluidTrade.Windows.Controls.Report in a System.Windows.Controls.Page.
	/// </summary>
	public partial class ViewerDebtHolderBlotter : Viewer
	{

		// Private Static Fields
		private static ReportDebtHolderWorkingOrder reportWorkingOrder;
        private static ReportDebtHolderMatch reportMatch;
		private static DebtHolderNegotiationConsole debtHolderNegotiationConsole;
        private static PDFReport pdfReportControl;
		private static ReportDebtHolderPaymentSummary paymentSummaryReport;
		private static TabItem staticMatchTab;
        private static ReportDebtHolderSettlement settlementReport;	
		private static TabControl staticTabControl;
		public DynamicReport currentSelectedReport;
		public DynamicReport previouslySelectedReport;
		public bool hasCurrentReportChanged;
		private static Guid currentConsumerSettlementId;
		private Byte[] settlementLetter = null;
		private FluidTrade.Guardian.WindowPdfViewer windowPdfViewer = null;
		private delegate void SettlementLetterHandler(Byte[] settlementLetter);




		// Private Instance Fields
		private Blotter blotter;
		private TabControl tabControl;
		private TabItem tabWorkingOrder;
		private TabItem tabMatch;
		private TabItem tabSettlement;
		private TabControl settlemntTabControl;
		private TabItem tabSettlementLetter;
		private TabItem tabSettlementPaymentSummary;
		private Object[] arguments; 

		private Grid matchGrid;
		private Grid row1Grid;
		private Grid settlementGrid;	
		private Grid settlementRow1Grid;
		private RowDefinition matchRow0;
		private RowDefinition matchRow1;
        private GridSplitter gridSplitterSettlement;

		
		/// <summary>
		/// Identifies the FluidTrade.FluidTradeClient.ViewerDebtHolderBlotter.AnimationSpeed dependency property.
		/// </summary>
		public static readonly DependencyProperty AnimationSpeedProperty;

		/// <summary>
		/// Identifies the FluidTrade.FluidTradeClient.ViewerDebtHolderBlotter.IsHeaderFrozen dependency property.
		/// </summary>
		public static readonly DependencyProperty IsHeaderFrozenProperty;

		/// <summary>
		/// Identifies the FluidTrade.FluidTradeClient.ViewerDebtHolderBlotter.IsLayoutFrozen dependency property.
		/// </summary>
		public static readonly DependencyProperty IsLayoutFrozenProperty;

		/// <summary>
		/// Identifies the FluidTrade.FluidTradeClient.ViewerDebtHolderBlotter.IsNavigationPaneVisible dependency property.
		/// </summary>
		public static readonly DependencyProperty IsNavigationPaneVisibleProperty;

		/// <summary>
		/// Identifies the FluidTrade.FluidTradeClient.ViewerDebtHolderBlotter.Scale dependency property.
		/// </summary>
		public static readonly DependencyProperty ScaleProperty;

		/// <summary>
		/// Create the static resources required for a FluidTrade.FluidTradeClient.ViewerDebtHolderBlotter.
		/// </summary>
		static ViewerDebtHolderBlotter()
		{

			// AnimationSpeed Property
			ViewerDebtHolderBlotter.AnimationSpeedProperty = DependencyProperty.Register(
				"AnimationSpeed",
				typeof(AnimationSpeed),
				typeof(ViewerDebtHolderBlotter),
				new FrameworkPropertyMetadata(AnimationSpeed.Off, new PropertyChangedCallback(OnAnimationSpeedChanged)));

			// IsHeaderFrozen Property
			ViewerDebtHolderBlotter.IsHeaderFrozenProperty = DependencyProperty.Register(
				"IsHeaderFrozen",
				typeof(Boolean),
				typeof(ViewerDebtHolderBlotter),
				new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnIsHeaderFrozenChanged)));

			// IsLayoutFrozen Property
			ViewerDebtHolderBlotter.IsLayoutFrozenProperty = DependencyProperty.Register(
				"IsLayoutFrozen",
				typeof(Boolean),
				typeof(ViewerDebtHolderBlotter),
				new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnIsLayoutFrozenChanged)));

			// IsNavigationPaneVisible Property
			ViewerDebtHolderBlotter.IsNavigationPaneVisibleProperty = DependencyProperty.Register(
				"IsNavigationPaneVisible",
				typeof(Boolean),
				typeof(ViewerDebtHolderBlotter),
				new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnIsNavigationPaneVisibleChanged)));

			// Scale Property
			ViewerDebtHolderBlotter.ScaleProperty = DependencyProperty.Register(
				"Scale",
				typeof(Double),
				typeof(ViewerDebtHolderBlotter),
				new FrameworkPropertyMetadata(1.0, new PropertyChangedCallback(OnScaleChanged)));


			// For performance purposes, a single, shared report is used to display all content.  Since the greatest delays are incurred when elements are
			// removed and added to the visual tree, recycling the same report makes things appear quicker because the visual elements remain in place but the
			// data bindings to those elements points to the new content.
			ViewerDebtHolderBlotter.reportWorkingOrder = new ReportDebtHolderWorkingOrder();
            ViewerDebtHolderBlotter.reportMatch = new ReportDebtHolderMatch();
            ViewerDebtHolderBlotter.settlementReport = new ReportDebtHolderSettlement();
			ViewerDebtHolderBlotter.paymentSummaryReport = new ReportDebtHolderPaymentSummary();
			ViewerDebtHolderBlotter.debtHolderNegotiationConsole = new DebtHolderNegotiationConsole();
            ViewerDebtHolderBlotter.pdfReportControl = new PDFReport();

		}

		private void ReportPaymentSummary_LostFocus(object sender, RoutedEventArgs e)
		{
			SetPaymentSummaryReportCurrentSelectReport(false);
		}

		private void ReportPaymentSummary_GotFocus(object sender, RoutedEventArgs e)
		{
			SetPaymentSummaryReportCurrentSelectReport(true);
		}

		private void SetPaymentSummaryReportCurrentSelectReport(bool setCurrentReport)
		{
			this.previouslySelectedReport = this.currentSelectedReport;

			if (setCurrentReport)
			{
				this.currentSelectedReport = ViewerDebtHolderBlotter.paymentSummaryReport;
			}
			else
			{
				this.currentSelectedReport = ViewerDebtHolderBlotter.settlementReport;
			}

			hasCurrentReportChanged = true;

			UpdateMenu();
		}

		private void ReportPDFControl_LostFocus(object sender, RoutedEventArgs e)
		{
			this.previouslySelectedReport = this.currentSelectedReport;
			this.currentSelectedReport = ViewerDebtHolderBlotter.settlementReport;
			hasCurrentReportChanged = true;
			UpdateMenu();
		}

		private void ReportPDFControl_GotFocus(object sender, RoutedEventArgs e)
		{
			OnSettlementReportSelectionChanged(sender, e);
			UpdateMenu();
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void OnSettlementReportSelectionChanged(object sender, EventArgs e)
		{
			ReportCell reportCell = ViewerDebtHolderBlotter.settlementReport.FocusedCell;
			if (reportCell != null)
			{
				ThreadPoolHelper.QueueUserWorkItem(InitializePDFReport, reportCell);
				ThreadPoolHelper.QueueUserWorkItem(InitializePaymentSummaryReport, reportCell);
			}
		}

		/// <summary>
		/// Reset the payment summary report
		/// </summary>
		/// <param name="state"></param>
		private void InitializePaymentSummaryReport(object state)
		{
			ReportCell reportCell = state as ReportCell;
			if (reportCell != null)
			{
				try
				{
					lock (DataModel.SyncRoot)
					{
						ConsumerDebtSettlementRow consumerDebtSettlementRow = reportCell.ReportRow.IContent.Key as ConsumerDebtSettlementRow;
						if (consumerDebtSettlementRow != null)
						{
							this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate()
							{
								ViewerDebtHolderBlotter.paymentSummaryReport.ConsumerTrustSettlementId = consumerDebtSettlementRow.ConsumerDebtSettlementId;
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

		private void InitializePDFReport(object state)
		{
			ReportCell reportCell = state as ReportCell;
			if (reportCell != null)
			{
				try
				{
					lock (DataModel.SyncRoot)
					{
						ConsumerDebtSettlementRow consumerDebtSettlementRow = reportCell.ReportRow.IContent.Key as ConsumerDebtSettlementRow;
						if (consumerDebtSettlementRow != null)
						{
							// Fire this on the foreground.
							this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate()
							{
								currentConsumerSettlementId = consumerDebtSettlementRow.ConsumerDebtSettlementId;
								ViewerDebtHolderBlotter.pdfReportControl.ConsumerSettlementId = currentConsumerSettlementId;
								this.EnableApproveSettlementActions(true);
							}));

							// Continue to refresh the letter in the background thread.
							ViewerDebtHolderBlotter.pdfReportControl.RefreshLetter(currentConsumerSettlementId);

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
		private void OnMatchReportSelectionChanged(object sender, EventArgs e)
		{
			// HACK: See OnMouseUp() method which handles row selection for match reports in a hacked fashion.
			// This can be uncommented once properly architected row selection is completed.
			//if ((ViewerDebtHolderBlotter.reportMatch.reportGrid.SelectedRowHeaderBlocks.Count > 1) || (ViewerDebtHolderBlotter.reportMatch.reportGrid.SelectedRowHeaderBlocks[0].Count > 1))
			//{
			//    ViewerDebtHolderBlotter.debtHolderNegotiationConsole.IsEnabled = false;
			//}
			//else
			//{
				ReportCell reportCell = ViewerDebtHolderBlotter.reportMatch.FocusedCell;
				if (reportCell != null)
				{
					ThreadPoolHelper.QueueUserWorkItem(InitializeMatchReport, reportCell);
				}
			//}
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
								ViewerDebtHolderBlotter.debtHolderNegotiationConsole.MatchId = matchRow.MatchId;
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
		/// HACK: This method was added for specifically handling selection of rows in the Match Report. 
		/// As we do not correct support selecting of a row in BodyCanvas.cs or ReportGrid.
		/// Note the Border aka RowHeaderCanvas is responsible for rows being selected because 
		/// for this disfunctional system, we have implemented this Mouse up operation.
		/// Do NOT remove this method until we have architected something that correct handles row selection.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnMouseUp(MouseButtonEventArgs e)
		{
			base.OnMouseUp(e);

			if (this.currentSelectedReport is ReportDebtHolderMatch && ViewerDebtHolderBlotter.reportMatch.FocusedCell != null)
			{
				if (ViewerDebtHolderBlotter.reportMatch.FocusedCell.Content is FluidTrade.Guardian.Schema.DebtHolderMatch.SelectRow)
				{
					if (ViewerDebtHolderBlotter.reportMatch.multipleSelectedRows)
					{
						ViewerDebtHolderBlotter.debtHolderNegotiationConsole.IsEnabled = false;
					}
					else
					{
						ReportCell reportCell = ViewerDebtHolderBlotter.reportMatch.FocusedCell;
						if (reportCell != null)
						{
							ThreadPoolHelper.QueueUserWorkItem(InitializeMatchReport, reportCell);
						}
					}
				}
			}
		}


		/// <summary>
		/// Creates an object that wraps a Markthree.Controls.Report in a Page.
		/// </summary>
		public ViewerDebtHolderBlotter(params Object[] arguments)
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

			this.menuItemManageUsers.IsEnabled = true;

			// The delegates will handle the loading and unloading of the viewer into the visual tree.
			this.Loaded += new RoutedEventHandler(OnLoaded);
			this.Unloaded += new RoutedEventHandler(OnUnloaded);

			GuardianCommands.Import.CanExecute(null, this);
			//{
			//    Filter = new Func<BlotterItem, bool>
			//        (t => t.TypeId == this.blotter.TypeId && t.BlotterId != this.blotter.BlotterId)
			//};

			this.currentSelectedReport = ViewerDebtHolderBlotter.reportWorkingOrder;
			hasCurrentReportChanged = false;

            // Fixed Bug - 20090320 (AR/NP)
            // Trouble happened when starting a ContextMenu on a control that does not get focus.
            // ERROR: System.Windows.Data Error: 4 : Cannot find source for binding with reference 'ElementName=viewerDebtNegotiatorBlotter'. 
            //      BindingExpression:(no path); DataItem=null; target element is 'MenuItem' (Name='contextMenuItemSelectColumns'); target property is 'CommandTarget' (type 'IInputElement')
            // This is added to fix a Microsoft bug with the bindings see
            // http://www.wiredprairie.us/journal/2007/04/commandtarget_menuitem_context.html
            // Setting the CommandTarget explicitly resolves this issue.
            this.contextMenuItemSelectColumns.CommandTarget = this;
			this.contextMenuItemSelectAll.CommandTarget = this;
            this.contextMenuItemApplicationCommandDelete.CommandTarget = this;
            this.menuItemReportShowWindow.CommandTarget = this;
			this.menuItemApproveSettlement.CommandTarget = this;
			this.menuItemRegenerateSettlement.CommandTarget = this;

			// Register for these events (do not forget to unregister them later).
			ViewerDebtHolderBlotter.settlementReport.SelectionChanged += new EventHandler(OnSettlementReportSelectionChanged);
			ViewerDebtHolderBlotter.reportMatch.SelectionChanged += new EventHandler(OnMatchReportSelectionChanged);
			ViewerDebtHolderBlotter.paymentSummaryReport.GotFocus += new RoutedEventHandler(ReportPaymentSummary_GotFocus);
			ViewerDebtHolderBlotter.paymentSummaryReport.LostFocus += new RoutedEventHandler(ReportPaymentSummary_LostFocus);
			ViewerDebtHolderBlotter.pdfReportControl.GotFocus += new RoutedEventHandler(ReportPDFControl_GotFocus);
			ViewerDebtHolderBlotter.pdfReportControl.LostFocus += new RoutedEventHandler(ReportPDFControl_LostFocus);

			this.UpdateMenu();
		}

		/// <summary>
		/// Gets or sets the speed of animation.
		/// </summary>
		public AnimationSpeed AnimationSpeed
		{
			get { return (AnimationSpeed)this.GetValue(ViewerDebtHolderBlotter.AnimationSpeedProperty); }
			set { this.SetValue(ViewerDebtHolderBlotter.AnimationSpeedProperty, value); }
		}

		/// <summary>
		/// Gets or sets an indication of whether the headers can be modified or not.
		/// </summary>
		private Boolean IsHeaderFrozen
		{
			get { return (Boolean)this.GetValue(ViewerDebtHolderBlotter.IsHeaderFrozenProperty); }
			set { this.SetValue(ViewerDebtHolderBlotter.IsHeaderFrozenProperty, value); }
		}

		/// <summary>
		/// Gets or sets an indication of whether the panel layout can be changed or not.
		/// </summary>
		private Boolean IsLayoutFrozen
		{
			get { return (Boolean)this.GetValue(ViewerDebtHolderBlotter.IsLayoutFrozenProperty); }
			set { this.SetValue(ViewerDebtHolderBlotter.IsLayoutFrozenProperty, value); }
		}

		/// <summary>
		/// Gets or sets an indication of whether the navigation pane is visible or not.
		/// </summary>
		private Boolean IsNavigationPaneVisible
		{
			get { return (Boolean)this.GetValue(ViewerDebtHolderBlotter.IsNavigationPaneVisibleProperty); }
			set { this.SetValue(ViewerDebtHolderBlotter.IsNavigationPaneVisibleProperty, value); }
		}

		/// <summary>
		/// Gets or sets whether the scale factor used for magnifying the content.
		/// </summary>
		private Double Scale
		{
			get { return (Double)this.GetValue(ViewerDebtHolderBlotter.ScaleProperty); }
			set { this.SetValue(ViewerDebtHolderBlotter.ScaleProperty, value); }
		}

        /// <summary>
        /// 
        /// </summary>
		public static void ActivateMatch()
		{

			ViewerDebtHolderBlotter.staticTabControl.SelectedItem = ViewerDebtHolderBlotter.staticMatchTab;

		}

		/// <summary>
		/// Handler for sendTo Blotter.
		/// </summary>
		/// <param name="sender">The "Move to..." menu item.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnSendTo(object sender, ExecutedRoutedEventArgs eventArgs)
		{

			ViewerDebtHolderBlotter.reportWorkingOrder.MoveRows();

		}

		/// <summary>
		/// Handles a change to the AnimationSpeed property.
		/// </summary>
		/// <param name="dependencyObject">The object that owns the property.</param>
		/// <param name="dependencyPropertyChangedEventArgs">A description of the changed property.</param>
		private static void OnAnimationSpeedChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{

			// Extract the strongly typed variables from the generic parameters.
			ViewerDebtHolderBlotter viewerPrototype = dependencyObject as ViewerDebtHolderBlotter;
			AnimationSpeed animationSpeed = (AnimationSpeed)dependencyPropertyChangedEventArgs.NewValue;

			// Set the animation speed on the report.
			ViewerDebtHolderBlotter.reportWorkingOrder.AnimationSpeed = animationSpeed;

			// Adjust the menu items for the new animation setting.
			viewerPrototype.menuItemSetAnimationFast.IsChecked = animationSpeed == AnimationSpeed.Fast;
			viewerPrototype.menuItemSetAnimationMedium.IsChecked = animationSpeed == AnimationSpeed.Medium;
			viewerPrototype.menuItemSetAnimationOff.IsChecked = animationSpeed == AnimationSpeed.Off;
			viewerPrototype.menuItemSetAnimationSlow.IsChecked = animationSpeed == AnimationSpeed.Slow;

		}

		private void OnCreateSlice(object sender, RoutedEventArgs e)
		{

			if (ViewerDebtHolderBlotter.reportWorkingOrder.Rows != null)
			{

				List<WorkingOrderRow> selectedRows = new List<WorkingOrderRow>();

				foreach (FluidTrade.Core.Windows.Controls.ReportRow reportRow in ViewerDebtHolderBlotter.reportWorkingOrder.Rows)
					if (reportRow.IContent.Key is WorkingOrderRow)
						selectedRows.Add(reportRow.IContent.Key as WorkingOrderRow);

				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(ExecuteSlice, selectedRows);

			}

		}

		/// <summary>
		/// Handle the import command. Query the user for xml script and import it.
		/// </summary>
		/// <param name="sender">The import menu item.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnImport(object sender, RoutedEventArgs eventArgs)
		{

			DebtHolder entity = this.Content as DebtHolder;

			entity.ImportAccounts();

		}

		/// <summary>
		/// Handle the manage users command. Open the user manager.
		/// </summary>
		/// <param name="sender">The import menu item.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnManageUsers(object sender, RoutedEventArgs eventArgs)
		{

			try
			{

				Entity entity = this.Content as Entity;

				if (entity != null)
				{

					WindowUserAccounts manager = new WindowUserAccounts();

					manager.Owner = Application.Current.MainWindow;
					manager.ShowDialog();

				}

			}
			catch (Exception exception)
			{

				EventLog.Error("{0}: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);

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
					String.Format( FluidTrade.Core.Properties.Resources.RecordNotFoundError,
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
			this.tabControl.SelectionChanged += new SelectionChangedEventHandler(OnTabControlSelectionChanged);
            this.tabControl.SizeChanged += new SizeChangedEventHandler(tabControl_SizeChanged);
			ViewerDebtHolderBlotter.staticTabControl = this.tabControl;
			this.SetValue(Page.ContentProperty, tabControl);

			// Working Order Tab
			this.tabWorkingOrder = new TabItem();
			this.tabWorkingOrder.Header = "Inventory Manager";

			// Match Tab
			this.tabMatch = new TabItem();
			ViewerDebtHolderBlotter.staticMatchTab = this.tabMatch;
			this.tabMatch.Header = "Negotiation Manager";
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

            // Create a column for the MatchedWorkingOrder report.
            ColumnDefinition matchColumn10 = new ColumnDefinition();
            matchColumn10.Width = new GridLength(1.0, GridUnitType.Star);
            this.row1Grid.ColumnDefinitions.Add(matchColumn10);

            // Create a column for the NegotiationConsole.
            ColumnDefinition matchColumn11 = new ColumnDefinition();
            matchColumn11.Width = new GridLength(1.0, GridUnitType.Auto);
            this.row1Grid.ColumnDefinitions.Add(matchColumn11);
            this.matchGrid.Children.Add(this.row1Grid);

			InitializeSettlementTab();
					

			// Add the tabs to the page.
			tabControl.Items.Add(this.tabWorkingOrder);
			tabControl.Items.Add(this.tabMatch);
			tabControl.Items.Add(this.tabSettlement);

			// The lion's share of time to load a report is adding the child user interface elements to the report.  Recycling the cells from one report to
			// another makes things move much faster.  The viewer creates a single, static Prototype Report for its content and reuses it from one viewer to
			// the next.  The downside to this architecture is that the binding must be done in code because the XAML doesn't have access to the static report.
			this.tabWorkingOrder.Content = ViewerDebtHolderBlotter.reportWorkingOrder;

			this.row1Grid.Children.Add(ViewerDebtHolderBlotter.reportMatch);
			this.row1Grid.Children.Add(ViewerDebtHolderBlotter.debtHolderNegotiationConsole);

			Grid.SetColumn(ViewerDebtHolderBlotter.reportMatch, 0);
			Grid.SetRow(ViewerDebtHolderBlotter.reportMatch, 0);
			
            Grid.SetColumn(ViewerDebtHolderBlotter.debtHolderNegotiationConsole, 2);
			Grid.SetRow(ViewerDebtHolderBlotter.debtHolderNegotiationConsole, 0);
            ViewerDebtHolderBlotter.debtHolderNegotiationConsole.HorizontalAlignment = HorizontalAlignment.Left;

			this.tabMatch.Content = this.matchGrid;

			// This selects which orders are displayed in the viewer.
			ViewerDebtHolderBlotter.reportWorkingOrder.BlotterId = this.blotter.BlotterId;
			ViewerDebtHolderBlotter.reportMatch.BlotterId = this.blotter.BlotterId;
			ViewerDebtHolderBlotter.settlementReport.BlotterId = this.blotter.BlotterId;
			ViewerDebtHolderBlotter.paymentSummaryReport.ConsumerTrustSettlementId = Guid.Empty;
			ViewerDebtHolderBlotter.paymentSummaryReport.BlotterId = this.blotter.BlotterId;

			this.tabControl.SelectedIndex = Viewer.lastSelectedtabItem.Get(this.blotter.BlotterId).GetValueOrDefault();

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
			BindingOperations.SetBinding(this, ViewerDebtHolderBlotter.AnimationSpeedProperty, bindingAnimationSpeed);

			// Bind the "IsHeaderFrozen" property to the setting.
			Binding bindingApplyHeaderFrozen = new Binding("IsHeaderFrozen");
            bindingApplyHeaderFrozen.Source = FluidTrade.Guardian.Properties.Settings.Default;
			bindingApplyHeaderFrozen.Mode = BindingMode.TwoWay;
			BindingOperations.SetBinding(this, ViewerDebtHolderBlotter.IsHeaderFrozenProperty, bindingApplyHeaderFrozen);

			// Bind the "IsLayoutFrozen" property to the setting.
			Binding bindingApplyLayoutFrozen = new Binding("IsLayoutFrozen");
            bindingApplyLayoutFrozen.Source = FluidTrade.Guardian.Properties.Settings.Default;
			bindingApplyLayoutFrozen.Mode = BindingMode.TwoWay;
			BindingOperations.SetBinding(this, ViewerDebtHolderBlotter.IsLayoutFrozenProperty, bindingApplyLayoutFrozen);

			// Bind the "IsNavigationPaneVisible" property to the settings.
			Binding bindingIsNavigationPaneVisible = new Binding("IsNavigationPaneVisible");
            bindingIsNavigationPaneVisible.Source = FluidTrade.Guardian.Properties.Settings.Default;
			bindingIsNavigationPaneVisible.Mode = BindingMode.TwoWay;
			BindingOperations.SetBinding(this, ViewerDebtHolderBlotter.IsNavigationPaneVisibleProperty, bindingIsNavigationPaneVisible);

			// Bind the "Scale" property to the setting.
			Binding bindingSliderScale = new Binding("Scale");
            bindingSliderScale.Source = FluidTrade.Guardian.Properties.Settings.Default;
			bindingSliderScale.Mode = BindingMode.TwoWay;
			BindingOperations.SetBinding(this, ViewerDebtHolderBlotter.ScaleProperty, bindingSliderScale);

		}

		//Initialize the settlement tab
		private void InitializeSettlementTab()
		{
			// Settlement Tab
			this.tabSettlement = new TabItem();
			this.tabSettlement.Header = "Settlement Manager";
			this.settlementGrid = new Grid();

			this.settlementRow1Grid = new Grid();
			Grid.SetColumn(this.settlementRow1Grid, 0);
			Grid.SetRow(this.settlementRow1Grid, 0);

			RowDefinition settlementColumn01 = new RowDefinition();
			settlementColumn01.Height = new GridLength(2.0, GridUnitType.Star);

			RowDefinition settlementColumn11 = new RowDefinition();
			// Sets the Height of the splitter in the settlement report.
			settlementColumn11.Height = new GridLength(7.0, GridUnitType.Pixel);

			RowDefinition settlementColumn21 = new RowDefinition();
			settlementColumn21.Height = new GridLength(4.0, GridUnitType.Star);

			this.settlementRow1Grid.RowDefinitions.Add(settlementColumn01);
			this.settlementRow1Grid.RowDefinitions.Add(settlementColumn11);
			this.settlementRow1Grid.RowDefinitions.Add(settlementColumn21);

			// Create the settlement report horizontal splitter.
			gridSplitterSettlement = new GridSplitter();
			gridSplitterSettlement.HorizontalAlignment = HorizontalAlignment.Stretch;
			gridSplitterSettlement.ShowsPreview = true;
			gridSplitterSettlement.Background = new System.Windows.Media.SolidColorBrush(this.splitColor);

			// Fill in the Grid Rows
			Grid.SetColumn(ViewerDebtHolderBlotter.settlementReport, 0);
			Grid.SetRow(ViewerDebtHolderBlotter.settlementReport, 0);

			Grid.SetColumn(gridSplitterSettlement, 0);
			Grid.SetRow(gridSplitterSettlement, 1);

			this.settlemntTabControl = new TabControl();
			this.tabSettlementLetter = new TabItem();
			this.tabSettlementLetter.Header = "Settlement Letter";
			this.tabSettlementLetter.Content = ViewerDebtHolderBlotter.pdfReportControl;
			
			this.tabSettlementPaymentSummary = new TabItem();
			this.tabSettlementPaymentSummary.Header = "Payment Summary";
			this.tabSettlementPaymentSummary.Content = ViewerDebtHolderBlotter.paymentSummaryReport;

			Grid.SetColumn(settlemntTabControl, 0);
			Grid.SetRow(settlemntTabControl, 2);

			settlemntTabControl.Items.Add(this.tabSettlementLetter);
			settlemntTabControl.Items.Add(this.tabSettlementPaymentSummary);

			this.settlementRow1Grid.Children.Add(ViewerDebtHolderBlotter.settlementReport);
			this.settlementRow1Grid.Children.Add(gridSplitterSettlement);
			this.settlementRow1Grid.Children.Add(settlemntTabControl);
			this.settlementGrid.Children.Add(this.settlementRow1Grid);

			// Clear out any cached settlement letter.
			ViewerDebtHolderBlotter.pdfReportControl.ConsumerSettlementId = Guid.Empty;
			EnableApproveSettlementActions(false);

			this.tabSettlement.Content = settlementGrid;
		}


		/// <summary>
		/// Get the Settlement Letter from the data model.
		/// </summary>
		/// <param name="state"></param>
		private void GetSettlementLetter(object state)
		{
			Guid settlementId = (Guid)state;

			// Lock the data model to extract information from the settlementRow.
			lock (DataModel.SyncRoot)
			{
				ConsumerDebtSettlementRow consumerDebtSettlementRow = DataModel.ConsumerDebtSettlement.ConsumerDebtSettlementKey.Find(currentConsumerSettlementId);
				if (!consumerDebtSettlementRow.IsSettlementLetterNull())
					this.settlementLetter = Convert.FromBase64String(consumerDebtSettlementRow.SettlementLetter);
			}

			// Dispatch to foreground thread for updating the GUI.
			if (settlementLetter != null)
				this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SettlementLetterHandler(UpdateSettlementPDFViewer), settlementLetter);

		}

		/// <summary>
		/// Update the GUI with the Settlement Letter.
		/// </summary>
		/// <param name="settlementInfo">SettlementInfo object.</param>
		private void UpdateSettlementPDFViewer(Byte[] settlementLetter)
		{
			// Load up the PDF from a memory stream and dsplay the settlement letter.
			windowPdfViewer.setPdfViewerSource(new MemoryStream(settlementLetter));
			windowPdfViewer.Show();
		}


        /// <summary>
        /// Handle any changes to the size of the TabControl.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void tabControl_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            // Detect any changes to the width of the tabcontrol and we are only interested in increasing tabcontrol size.
            if (e.WidthChanged && e.NewSize.Width > e.PreviousSize.Width)
            {
                // Detect the increase in tabcontrol width, and resize automatically move the splitter to dock.
                double newSplitterLocation = e.NewSize.Width - ViewerDebtHolderBlotter.debtHolderNegotiationConsole.ActualWidth;
            }
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
				EventLog.Error("Debt Holder Report Unloading before loading");
				return;
			}
			////End hack.

			// Unregister for the events when the reports are changed otherwise repeated calls to the methods occur and will appear as memory leak.
			ViewerDebtHolderBlotter.settlementReport.SelectionChanged -= new EventHandler(OnSettlementReportSelectionChanged);
			ViewerDebtHolderBlotter.reportMatch.SelectionChanged -= new EventHandler(OnMatchReportSelectionChanged);
			ViewerDebtHolderBlotter.paymentSummaryReport.GotFocus -= new RoutedEventHandler(ReportPaymentSummary_GotFocus);
			ViewerDebtHolderBlotter.paymentSummaryReport.LostFocus -= new RoutedEventHandler(ReportPaymentSummary_LostFocus);
			ViewerDebtHolderBlotter.pdfReportControl.GotFocus -= new RoutedEventHandler(ReportPDFControl_GotFocus);
			ViewerDebtHolderBlotter.pdfReportControl.LostFocus -= new RoutedEventHandler(ReportPDFControl_LostFocus);

			this.tabControl.SelectionChanged -= new SelectionChangedEventHandler(OnTabControlSelectionChanged);
			this.tabControl.SizeChanged -= new SizeChangedEventHandler(tabControl_SizeChanged);

			this.tabWorkingOrder.Content = null;
			this.row1Grid.Children.Remove(ViewerDebtHolderBlotter.reportMatch);
			this.row1Grid.Children.Remove(ViewerDebtHolderBlotter.debtHolderNegotiationConsole);
			this.settlementRow1Grid.Children.Remove(tabSettlement);

			this.tabSettlement.Content = null;
			this.tabSettlementLetter.Content = null;
			this.tabSettlementPaymentSummary.Content = null;

			this.settlementRow1Grid.Children.Remove(ViewerDebtHolderBlotter.settlementReport);
			this.settlementGrid.Children.Remove(gridSplitterSettlement);
			this.settlementRow1Grid.Children.Remove(ViewerDebtHolderBlotter.pdfReportControl);
									

			// When the object is removed from the visual tree, there is no need to hold on to the bindings to the settings.
			// Another viewer may be installed and there's no reason why this viewer still needs to be a load on the settings data
			// structure.
			BindingOperations.ClearBinding(this, ViewerDebtHolderBlotter.AnimationSpeedProperty);
			BindingOperations.ClearBinding(this, ViewerDebtHolderBlotter.IsHeaderFrozenProperty);
			BindingOperations.ClearBinding(this, ViewerDebtHolderBlotter.IsLayoutFrozenProperty);
			BindingOperations.ClearBinding(this, ViewerDebtHolderBlotter.IsNavigationPaneVisibleProperty);
			BindingOperations.ClearBinding(this, ViewerDebtHolderBlotter.ScaleProperty);

			// Removing the shared content from the viewer allows this viewer to be garbage collected while the core report will 
			// stay around until the current application domain is unloaded.
			this.SetValue(Page.ContentProperty, null);

			Viewer.lastSelectedtabItem.Upsert(this.blotter.BlotterId, this.tabControl.SelectedIndex);
			this.EnableApproveSettlementActions(false);
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
			ViewerDebtHolderBlotter viewerPrototype = dependencyObject as ViewerDebtHolderBlotter;
			Boolean isHeaderFrozen = (Boolean)dependencyPropertyChangedEventArgs.NewValue;

			// This takes care of the actual work of setting the filter to display only the filled orders (or removing the filter).
			ViewerDebtHolderBlotter.reportWorkingOrder.IsHeaderFrozen = isHeaderFrozen;
			ViewerDebtHolderBlotter.reportMatch.IsHeaderFrozen = isHeaderFrozen;
			ViewerDebtHolderBlotter.settlementReport.IsHeaderFrozen = isHeaderFrozen;
			ViewerDebtHolderBlotter.paymentSummaryReport.IsHeaderFrozen = isHeaderFrozen;

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
			ViewerDebtHolderBlotter viewerPrototype = dependencyObject as ViewerDebtHolderBlotter;
			Boolean isLayoutFrozen = (Boolean)dependencyPropertyChangedEventArgs.NewValue;

			// This takes care of the actual work of setting the filter to display only the filled orders (or removing the filter).
			//ViewerDebtHolderBlotter.reportWorkingOrder.IsLayoutFrozen = isLayoutFrozen;

            //TODO: ************ Remove this next line otherwise it will permanantly set the IsLayoutFrozen ****************
            ViewerDebtHolderBlotter.reportWorkingOrder.IsLayoutFrozen = true;

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
			ViewerDebtHolderBlotter viewerPrototype = dependencyObject as ViewerDebtHolderBlotter;
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
			ViewerDebtHolderBlotter viewerPrototype = dependencyObject as ViewerDebtHolderBlotter;
			Double scale = (Double)dependencyPropertyChangedEventArgs.NewValue;

			// Set the animation speed on the report.
			ViewerDebtHolderBlotter.reportWorkingOrder.Scale = scale;
			ViewerDebtHolderBlotter.reportMatch.Scale = scale;
			ViewerDebtHolderBlotter.settlementReport.Scale = scale;
			ViewerDebtHolderBlotter.paymentSummaryReport.Scale = scale;

		}



		/// <summary>
		/// Search allows for getting the string that was enter to be searched and then locating each cell in the report grid.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public override void OnSearchHandler(Object sender, RoutedEventArgs e, string searchToString, Action completeAction)
		{
			base.OnSearchHandler(sender, e, searchToString, completeAction);

			bool success = false;
			string searchString = searchToString;
			ReportGrid reportGrid = null;

			if (currentSelectedReport != null)
			{
				reportGrid = currentSelectedReport.reportGrid;

				success = reportGrid.FindSearchedCell(searchString, completeAction);

				

				if (!success)
					return;

			}
		}


		public override void OnSearchRequestCancel()
		{
			ReportGrid reportGrid = null;

			if (currentSelectedReport != null)
			{
				reportGrid = currentSelectedReport.reportGrid;

				reportGrid.CancelSearch();
			}

			
		}

		/// <summary>
		/// Selects the visible columns of the report.
		/// </summary>
		/// <param name="sender">The object that originated this event.</param>
		/// <param name="e">The unused event argments.</param>
		private void OnSelectColumns(object sender, ExecutedRoutedEventArgs e)
		{
			
			// Allow the user to select the columns for the report.
			this.currentSelectedReport.SelectColumns();			

		}

        /// <summary>
        /// Selects all cells in the report.
        /// </summary>
        /// <param name="sender">The object that originated this event.</param>
        /// <param name="e">The unused event argments.</param>
        private void OnSelectAll(object sender, ExecutedRoutedEventArgs e)
        {
            // Allow the user to select the columns for the report.
            this.currentSelectedReport.SelectAll();

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
		/// Called when changing the tabs
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void OnTabControlSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			this.previouslySelectedReport = this.currentSelectedReport;

			///Both TabControl and Combobox is derived from Selector object. Everytime any combobox
			// in this report changes this handler will be called. So we need to do some basic
			//sanity checks here.
			if (e.OriginalSource is TabControl)
			{
				TabControl senderControl = sender as TabControl;
				if (tabControl.SelectedItem == tabMatch)
					currentSelectedReport = ViewerDebtHolderBlotter.reportMatch;
				else if (tabControl.SelectedItem == tabSettlement)
					currentSelectedReport = ViewerDebtHolderBlotter.settlementReport;
				else
					currentSelectedReport = ViewerDebtHolderBlotter.reportWorkingOrder;

				if (this.previouslySelectedReport != this.currentSelectedReport)
					hasCurrentReportChanged = true;

                UpdateMenu();
                
				e.Handled = true;

			}
		}

	
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBalanceChanged(object sender, TextChangedEventArgs e)
		{

			// This will remove any orders from the report that are less than the given text.			
			Decimal filterValue;
			Decimal.TryParse(this.textBoxBalanceFilter.Text, out filterValue);
			ViewerDebtHolderBlotter.reportMatch.CreditCardBalanceFilter = filterValue;
			ViewerDebtHolderBlotter.reportWorkingOrder.BalanceFilter = filterValue;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDaysLateChanged(object sender, TextChangedEventArgs e)
        {			
			Int32 filterValue;
			Int32.TryParse(this.textBoxDaysLateFilter.Text, out filterValue);	
			//ViewerDebtHolderBlotter.reportMatch.DaysLateFilter = filterValue;
            //ViewerDebtHolderBlotter.reportWorkingOrder.DaysLateFilter = filterValue;
        }

		/// <summary>
		/// Changes the text used to filter symbols from the list of orders.
		/// </summary>
		/// <param name="sender">The object that originated this event.</param>
		/// <param name="e">The unused event argments.</param>
		private void OnTextBoxPostalCodeTextChanged(object sender, TextChangedEventArgs e)
		{
			// This will remove any orders from the report that don't start with the given text.			
			string filterValue;
			filterValue = this.textBoxPostalCodeCodeFilter.Text;
			ViewerDebtHolderBlotter.reportMatch.PostalCodeFilter = filterValue;
			ViewerDebtHolderBlotter.reportWorkingOrder.PostalCodeFilter = filterValue;

		}
		
		/// <summary>
        /// Called just before showing the context menu
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="routedEventArgs"></param>
		private void OnContextMenuOpen(object sender, RoutedEventArgs routedEventArgs)
		{

			this.UpdateMenu();

		}

        /// <summary>
        /// Updates the items that are displayed in the Menu.
        /// </summary>
        private void UpdateMenu()
        {
			Point mouse = Mouse.PrimaryDevice.GetPosition(this);
			PointHitTestParameters param = new PointHitTestParameters(mouse);
			Boolean isHeader = false;

			VisualTreeHelper.HitTest(
				this,
				delegate(DependencyObject d)
				{
					if (d is ColumnHeaderCanvas)
					{

						isHeader = true;
						return HitTestFilterBehavior.Stop;
					}
					else
					{
						return HitTestFilterBehavior.ContinueSkipSelf;
					}
				},
				d => HitTestResultBehavior.Continue,
				param);

			//TODO: Correctly set the appropriate report and then remove the need for the IsVisibleChanged methods as they do not work.
            // Also need to call this on start up to set the menu correct from the start 
            // Should default the current report to working order report.
			if (currentSelectedReport == ViewerDebtHolderBlotter.reportWorkingOrder)
			{
				this.menuItemMarkAsRead.Visibility = Visibility.Collapsed;
				this.menuItemMarkAsUnread.Visibility = Visibility.Collapsed;
				this.menuItemApproveSettlement.Visibility = Visibility.Collapsed;
				this.menuItemRegenerateSettlement.Visibility = Visibility.Collapsed;
				this.contextMenuItemApproveSettlement.Visibility = Visibility.Collapsed;
				this.contextMenuItemRegenerateSettlement.Visibility = Visibility.Collapsed;
				this.contextMenuItemApplicationCommandDelete.Visibility = Visibility.Visible;
				if (isHeader)
				{

					this.menuItemMoveTo.Visibility = Visibility.Collapsed;
					this.contextMenuItemSendTo.Visibility = Visibility.Collapsed;

				}
				else
				{

					this.menuItemMoveTo.Visibility = Visibility.Visible;
					this.contextMenuItemSendTo.Visibility = Visibility.Visible;

				}
			}
			else
			{
				this.menuItemMoveTo.Visibility = Visibility.Collapsed;
				this.contextMenuItemSendTo.Visibility = Visibility.Collapsed;
				this.contextMenuItemApplicationCommandDelete.Visibility = Visibility.Collapsed;
			}
            if (currentSelectedReport == ViewerDebtHolderBlotter.reportMatch)
            {
                this.menuItemMarkAsRead.Visibility = Visibility.Visible;
                this.menuItemMarkAsUnread.Visibility = Visibility.Visible;
                this.contextMenuItemMarkAsRead.Visibility = Visibility.Visible;
                this.contextMenuItemMarkAsUnread.Visibility = Visibility.Visible;
				this.menuItemApproveSettlement.Visibility = Visibility.Collapsed;
				this.menuItemRegenerateSettlement.Visibility = Visibility.Collapsed;
				this.contextMenuItemApproveSettlement.Visibility = Visibility.Collapsed;
				this.contextMenuItemRegenerateSettlement.Visibility = Visibility.Collapsed;
				this.contextMenuItemApplicationCommandDelete.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.menuItemMarkAsRead.Visibility = Visibility.Collapsed;
                this.menuItemMarkAsUnread.Visibility = Visibility.Collapsed;
                this.contextMenuItemMarkAsRead.Visibility = Visibility.Collapsed;
                this.contextMenuItemMarkAsUnread.Visibility = Visibility.Collapsed;
            }
			if (currentSelectedReport == ViewerDebtHolderBlotter.settlementReport)
			{
				this.menuItemMarkAsRead.Visibility = Visibility.Collapsed;
				this.menuItemMarkAsUnread.Visibility = Visibility.Collapsed;
				this.contextMenuItemMarkAsRead.Visibility = Visibility.Collapsed;
				this.contextMenuItemMarkAsUnread.Visibility = Visibility.Collapsed;
				this.contextMenuItemApplicationCommandDelete.Visibility = Visibility.Collapsed;

				if (currentSelectedReport.reportGrid != null && currentSelectedReport.reportGrid.CurrentReportCell != null && ViewerDebtHolderBlotter.pdfReportControl.IsLetterLoaded && ViewerDebtHolderBlotter.pdfReportControl.settlementLetter != null)
					EnableApproveSettlementActions(true);
				else
					EnableApproveSettlementActions(false);
			}

        }

		private void EnableApproveSettlementActions(bool IsEnabled)
		{
			if (IsEnabled)
			{

				this.menuItemApproveSettlement.Visibility = Visibility.Visible;
				this.contextMenuItemApproveSettlement.Visibility = Visibility.Visible;
				this.menuItemRegenerateSettlement.Visibility = Visibility.Visible;
				this.contextMenuItemRegenerateSettlement.Visibility = Visibility.Visible;
				this.buttonApproveSettlement.Visibility = Visibility.Visible;
				this.menuItemPrintPreview.Visibility = Visibility.Visible;
				this.buttonPrintPreview.Visibility = Visibility.Visible;
				this.printPreviewSeparator.Visibility = Visibility.Visible;

			}
			else
			{

				this.menuItemApproveSettlement.Visibility = Visibility.Collapsed;
				this.contextMenuItemApproveSettlement.Visibility = Visibility.Collapsed;
				this.menuItemRegenerateSettlement.Visibility = Visibility.Collapsed;
				this.contextMenuItemRegenerateSettlement.Visibility = Visibility.Collapsed;
				this.buttonApproveSettlement.Visibility = Visibility.Collapsed;
				this.menuItemPrintPreview.Visibility = Visibility.Collapsed;
				this.buttonPrintPreview.Visibility = Visibility.Collapsed;
				this.printPreviewSeparator.Visibility = Visibility.Collapsed;
			}
		}
		
		private void OnApproveSettlement(object sender, RoutedEventArgs e)
		{
			if (ViewerDebtHolderBlotter.pdfReportControl.IsLetterLoaded)
			{
				ViewerDebtHolderBlotter.settlementReport.ApproveSettlement();
			}
			else
			{
				this.Dispatcher.BeginInvoke(new Action(() =>
					MessageBox.Show(Application.Current.MainWindow, "Unable to Approve Settlement Letter until the letter has been loaded. Please try again.", Application.Current.MainWindow.Title)));
				
				ViewerDebtHolderBlotter.pdfReportControl.RefreshLetter(currentConsumerSettlementId);

			}
		}

		private void OnRegenerateSettlement(object sender, RoutedEventArgs e)
		{
			ViewerDebtHolderBlotter.settlementReport.RegenerateSettlement();
		}
		
		private void OnMarkAsRead(object sender, ExecutedRoutedEventArgs e)
        {
            ViewerDebtHolderBlotter.reportMatch.MarkMatchRowsAsRead();
        }

        private void OnMarkAsUnread(object sender, ExecutedRoutedEventArgs e)
        {
            ViewerDebtHolderBlotter.reportMatch.MarkMatchRowsAsUnread();
        }

        private void OnApplicationCommandDelete(object sender, ExecutedRoutedEventArgs e)
        {
            if (currentSelectedReport == ViewerDebtHolderBlotter.reportWorkingOrder)
            {
				ViewerDebtHolderBlotter.reportWorkingOrder.DeleteRows();
            }
        }

		private void OnPrintPreview(object sender, ExecutedRoutedEventArgs e)
		{
			PerformPrintPreview();
		}

		private void OnPrintPreviewButton(object sender, RoutedEventArgs e)
		{
			PerformPrintPreview();
		}

		private void OnApproveSettlementButton(object sender, RoutedEventArgs e)
		{
			//if (ViewerDebtHolderBlotter.pdfReportControl.IsLetterLoaded)
			//{
			//    ViewerDebtHolderBlotter.settlementReport.ApproveSettlement();
			//}
			//else
			//{
			//    this.Dispatcher.BeginInvoke(new Action(() =>
			//        MessageBox.Show(Application.Current.MainWindow, "Unable to Approve Settlement Letter until the letter has been loaded. Please try again.", Application.Current.MainWindow.Title)));

			//    ViewerDebtHolderBlotter.pdfReportControl.RefreshLetter(currentConsumerSettlementId);

			//}
			this.OnApproveSettlement(sender, e);
		}

		private void PerformPrintPreview()
		{
			try
			{
				// Create pdf viewer for the settlement letter.
				windowPdfViewer = new WindowPdfViewer();

				if (currentConsumerSettlementId == Guid.Empty)
					return; 
				
				if (currentConsumerSettlementId == null)
				{
					throw new Exception("Settlement Id is null.");
				}

				// Update this control with information associated with the settlementId.  Do this on a background thread because the DataModel needs to be used.
				ThreadPoolHelper.QueueUserWorkItem(new WaitCallback(GetSettlementLetter), currentConsumerSettlementId);

			}
			catch (Exception ex)
			{

				this.Dispatcher.BeginInvoke(new Action(() =>
					MessageBox.Show(Application.Current.MainWindow, "Error Unable to View Settlement Letter - " + ex.Message, Application.Current.MainWindow.Title)));

			}

		}
		        
	}

}
