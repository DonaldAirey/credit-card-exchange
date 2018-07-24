namespace FluidTrade.Guardian
{

	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.ServiceModel;
	using System.Threading;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Controls.Primitives;
	using System.Windows.Data;
	using System.Windows.Input;
	using System.Windows.Threading;
	using FluidTrade.Core;
	using FluidTrade.Core.Windows;
	using FluidTrade.Core.Windows.Controls;
	using FluidTrade.Guardian.Windows;
	using System.Windows.Media;  

	/// <summary>
	/// Wraps a FluidTrade.Windows.Controls.Report in a System.Windows.Controls.Page.
	/// </summary>
	public partial class ViewerDebtNegotiatorBlotter : Viewer
	{

		// Private Static Fields
		private static ReportDebtNegotiatorWorkingOrder reportDebtNegotiatorWorkingOrder;
		private static ReportDebtNegotiatorMatch reportMatch;
		private static DebtNegotiatorNegotiationConsole debtNegotiatorNegotiationConsole;
		private static ReportDebtNegotiatorSettlement settlementReport;
		private static ReportCreditCard reportCreditCardDetail;
		private static ReportDebtNegotiatorPaymentSummary paymentSummaryReport;
		//private static Frame letterFrame = new Frame();
		private static PDFReport pdfReportControl;
		private static TabItem staticMatchTab;
		private static TabControl staticTabControl;
		public DynamicReport currentSelectedReport;
		public DynamicReport previouslySelectedReport;
		public bool hasCurrentReportChanged;
		private static Guid currentConsumerSettlementId;
		private Byte[] settlementLetter = null;
		private FluidTrade.Guardian.WindowPdfViewer windowPdfViewer = null;
		private delegate void SettlementLetterHandler(Byte[] settlementLetter);

		// Constants
		private const int stressTests = 500;

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
		private Grid workingOrderGrid;
		private Grid workingOrderRow1Grid;
		private Grid matchGrid;
		private Grid matchRow1Grid;
		private Grid settlementGrid;
		private Grid settlementRow1Grid;


		private GridSplitter gridSplitterSettlement;
		private GridSplitter gridSplitterWorkingOder;

		/// <summary>
		/// Identifies the FluidTrade.FluidTradeClient.ViewerDebtNegotiatorBlotter.AnimationSpeed dependency property.
		/// </summary>
		public static readonly DependencyProperty AnimationSpeedProperty;

		/// <summary>
		/// Identifies the FluidTrade.FluidTradeClient.ViewerDebtNegotiatorBlotter.IsHeaderFrozen dependency property.
		/// </summary>
		public static readonly DependencyProperty IsHeaderFrozenProperty;

		/// <summary>
		/// Identifies the FluidTrade.FluidTradeClient.ViewerDebtNegotiatorBlotter.IsLayoutFrozen dependency property.
		/// </summary>
		public static readonly DependencyProperty IsLayoutFrozenProperty;

		/// <summary>
		/// Identifies the FluidTrade.FluidTradeClient.ViewerDebtNegotiatorBlotter.IsNavigationPaneVisible dependency property.
		/// </summary>
		public static readonly DependencyProperty IsNavigationPaneVisibleProperty;

		/// <summary>
		/// Identifies the FluidTrade.FluidTradeClient.ViewerDebtNegotiatorBlotter.Scale dependency property.
		/// </summary>
		public static readonly DependencyProperty ScaleProperty;


		/// <summary>
		/// Create the static resources required for a FluidTrade.FluidTradeClient.ViewerDebtNegotiatorBlotter.
		/// </summary>
		static ViewerDebtNegotiatorBlotter()
		{

			// AnimationSpeed Property
			ViewerDebtNegotiatorBlotter.AnimationSpeedProperty = DependencyProperty.Register(
				"AnimationSpeed",
				typeof(AnimationSpeed),
				typeof(ViewerDebtNegotiatorBlotter),
				new FrameworkPropertyMetadata(AnimationSpeed.Off, new PropertyChangedCallback(OnAnimationSpeedChanged)));

			// IsHeaderFrozen Property
			ViewerDebtNegotiatorBlotter.IsHeaderFrozenProperty = DependencyProperty.Register(
				"IsHeaderFrozen",
				typeof(Boolean),
				typeof(ViewerDebtNegotiatorBlotter),
				new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnIsHeaderFrozenChanged)));

			// IsLayoutFrozen Property
			ViewerDebtNegotiatorBlotter.IsLayoutFrozenProperty = DependencyProperty.Register(
				"IsLayoutFrozen",
				typeof(Boolean),
				typeof(ViewerDebtNegotiatorBlotter),
				new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnIsLayoutFrozenChanged)));

			// IsNavigationPaneVisible Property
			ViewerDebtNegotiatorBlotter.IsNavigationPaneVisibleProperty = DependencyProperty.Register(
				"IsNavigationPaneVisible",
				typeof(Boolean),
				typeof(ViewerDebtNegotiatorBlotter),
				new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnIsNavigationPaneVisibleChanged)));

			// Scale Property
			ViewerDebtNegotiatorBlotter.ScaleProperty = DependencyProperty.Register(
				"Scale",
				typeof(Double),
				typeof(ViewerDebtNegotiatorBlotter),
				new FrameworkPropertyMetadata(1.0, new PropertyChangedCallback(OnScaleChanged)));

			// For performance purposes, a single, shared report is used to display all content.  Since the greatest delays are incurred when elements are
			// removed and added to the visual tree, recycling the same report makes things appear quicker because the visual elements remain in place but the
			// data bindings to those elements points to the new content.
			ViewerDebtNegotiatorBlotter.reportDebtNegotiatorWorkingOrder = new ReportDebtNegotiatorWorkingOrder();
			ViewerDebtNegotiatorBlotter.reportMatch = new ReportDebtNegotiatorMatch();
			ViewerDebtNegotiatorBlotter.settlementReport = new ReportDebtNegotiatorSettlement();
			ViewerDebtNegotiatorBlotter.reportCreditCardDetail = new ReportCreditCard();
			ViewerDebtNegotiatorBlotter.paymentSummaryReport = new ReportDebtNegotiatorPaymentSummary();
			ViewerDebtNegotiatorBlotter.debtNegotiatorNegotiationConsole = new DebtNegotiatorNegotiationConsole();
			ViewerDebtNegotiatorBlotter.pdfReportControl = new PDFReport();

		}

		private void ReportCreditCardDetail_LostFocus(object sender, RoutedEventArgs e)
		{
			SetCreditCardReportCurrentSelectReport(false);
		}

		private void ReportCreditCardDetail_GotFocus(object sender, RoutedEventArgs e)
		{
			SetCreditCardReportCurrentSelectReport(true);
		}

		private void ReportDebtNegotiatorWorkingOrder_GotFocus(object sender, RoutedEventArgs e)
		{
			SetCreditCardReportCurrentSelectReport(false);
		}

		private void SetCreditCardReportCurrentSelectReport(bool setCurrentReport)
		{
			this.previouslySelectedReport = this.currentSelectedReport;

			if (setCurrentReport)
				this.currentSelectedReport = ViewerDebtNegotiatorBlotter.reportCreditCardDetail;
			else
				this.currentSelectedReport = ViewerDebtNegotiatorBlotter.reportDebtNegotiatorWorkingOrder;

			hasCurrentReportChanged = true;

			UpdateMenu();
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
				this.currentSelectedReport = ViewerDebtNegotiatorBlotter.paymentSummaryReport;
			}
			else
			{
				this.currentSelectedReport = ViewerDebtNegotiatorBlotter.settlementReport;
			}

			hasCurrentReportChanged = true;
			UpdateMenu();
		}

		private void ReportPDFControl_LostFocus(object sender, RoutedEventArgs e)
		{
			this.previouslySelectedReport = this.currentSelectedReport; 
			this.currentSelectedReport = ViewerDebtNegotiatorBlotter.settlementReport;
			hasCurrentReportChanged = true;
			UpdateMenu();
		}

		private void ReportPDFControl_GotFocus(object sender, RoutedEventArgs e)
		{
			OnSettlementReportSelectionChanged(sender, e);
			UpdateMenu();
		}

		void ReportDebtNegotiatorWorkingOrderSelectionChanged(object sender, EventArgs e)
		{
			ReportCell reportCell = ViewerDebtNegotiatorBlotter.reportDebtNegotiatorWorkingOrder.FocusedCell;
			if (reportCell != null)
				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(InitializeCreditCardDetailReport, reportCell);
		}

		void InitializeCreditCardDetailReport(object state)
		{
			ReportCell reportCell = state as ReportCell;
			if (reportCell != null)
			{
				try
				{
					lock (DataModel.SyncRoot)
					{
						WorkingOrderRow workingOrderRow = reportCell.ReportRow.IContent.Key as WorkingOrderRow;
						if (workingOrderRow != null)
						{
							ConsumerTrustRow consumerTrustRow = DataModel.ConsumerTrust.ConsumerTrustKey.Find(workingOrderRow.SecurityId);
							this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate()
							{
								ViewerDebtNegotiatorBlotter.reportCreditCardDetail.ConsumerId = consumerTrustRow.ConsumerId;
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
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void OnSettlementReportSelectionChanged(object sender, EventArgs e)
		{
			ReportCell reportCell = ViewerDebtNegotiatorBlotter.settlementReport.FocusedCell;
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
						ConsumerTrustSettlementRow consumerTrustSettlementRow = reportCell.ReportRow.IContent.Key as ConsumerTrustSettlementRow;
						if (consumerTrustSettlementRow != null)
						{
							this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate()
							{
								ViewerDebtNegotiatorBlotter.paymentSummaryReport.ConsumerTrustSettlementId = consumerTrustSettlementRow.ConsumerTrustSettlementId;
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
						ConsumerTrustSettlementRow consumerTrustSettlementRow = reportCell.ReportRow.IContent.Key as ConsumerTrustSettlementRow;
						if (consumerTrustSettlementRow != null)
						{
							this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate()
							{
								currentConsumerSettlementId = consumerTrustSettlementRow.ConsumerTrustSettlementId;
								ViewerDebtNegotiatorBlotter.pdfReportControl.ConsumerSettlementId = currentConsumerSettlementId;
								this.EnableApproveSettlementActions(true);


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

		private void OnMatchReportSelectionChanged(object sender, EventArgs e)
		{
			// HACK: See OnMouseUp() method which handles row selection for match reports in a hacked fashion.
			// This can be uncommented once properly architected row selection is completed.
			//if ((ViewerDebtNegotiatorBlotter.reportMatch.reportGrid.SelectedRowHeaderBlocks.Count > 1) || (ViewerDebtNegotiatorBlotter.reportMatch.reportGrid.SelectedRowHeaderBlocks[0].Count > 1))
			//{
			//    ViewerDebtNegotiatorBlotter.debtNegotiatorNegotiationConsole.IsEnabled = false;
			//}
			//else
			//{
				ReportCell reportCell = ViewerDebtNegotiatorBlotter.reportMatch.FocusedCell;
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
								ViewerDebtNegotiatorBlotter.debtNegotiatorNegotiationConsole.MatchId = matchRow.MatchId;
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

			if (this.currentSelectedReport is ReportDebtNegotiatorMatch && ViewerDebtNegotiatorBlotter.reportMatch.FocusedCell != null)
			{
				if (ViewerDebtNegotiatorBlotter.reportMatch.FocusedCell.Content is FluidTrade.Guardian.Schema.DebtNegotiatorMatch.SelectRow)
				{
					if (ViewerDebtNegotiatorBlotter.reportMatch.multipleSelectedRows)
					{
						ViewerDebtNegotiatorBlotter.debtNegotiatorNegotiationConsole.IsEnabled = false;
					}
					else
					{
						ReportCell reportCell = ViewerDebtNegotiatorBlotter.reportMatch.FocusedCell;
						if (reportCell != null)
						{
							ThreadPoolHelper.QueueUserWorkItem(InitializeMatchReport, reportCell);
						}
					}
				}
			}

			if ((ViewerDebtNegotiatorBlotter.reportDebtNegotiatorWorkingOrder.FocusedCell != null) && (this.currentSelectedReport is ReportDebtNegotiatorWorkingOrder))
			{
				if (!ViewerDebtNegotiatorBlotter.reportDebtNegotiatorWorkingOrder.multipleSelectedRows)
				{
					ReportCell reportCell = ViewerDebtNegotiatorBlotter.reportDebtNegotiatorWorkingOrder.FocusedCell;
					if (reportCell != null)
					{
						ReportDebtNegotiatorWorkingOrderSelectionChanged(this, e);
					}
				}
			}

		}

		

		/// <summary>
		/// Creates an object that wraps a Markthree.Controls.Report in a Page.
		/// </summary>
		public ViewerDebtNegotiatorBlotter(params Object[] arguments)
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


			// The delegates will handle the loading and unloading of the viewer into the visual tree.
			this.Loaded += new RoutedEventHandler(OnLoaded);
			this.Unloaded += new RoutedEventHandler(OnUnloaded);

			GuardianCommands.Import.CanExecute(null, this);

			//this.blotterList = new BlotterList() { Filter = new Func<BlotterItem, bool>
			//    (t => t.TypeId == this.blotter.TypeId && t.BlotterId != this.blotter.BlotterId) };

			this.currentSelectedReport = ViewerDebtNegotiatorBlotter.reportDebtNegotiatorWorkingOrder;
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

			// Register for these events (do not forget to unregister them later).
			ViewerDebtNegotiatorBlotter.reportDebtNegotiatorWorkingOrder.SelectionChanged += new EventHandler(ReportDebtNegotiatorWorkingOrderSelectionChanged);
			ViewerDebtNegotiatorBlotter.settlementReport.SelectionChanged += new EventHandler(OnSettlementReportSelectionChanged);
			ViewerDebtNegotiatorBlotter.reportMatch.SelectionChanged += new EventHandler(OnMatchReportSelectionChanged);

			// Added eventhandler to be able to set the current selected report to the credit card report and back to the working order report.
			ViewerDebtNegotiatorBlotter.reportCreditCardDetail.GotFocus += new RoutedEventHandler(ReportCreditCardDetail_GotFocus);
			ViewerDebtNegotiatorBlotter.reportDebtNegotiatorWorkingOrder.GotFocus += new RoutedEventHandler(ReportDebtNegotiatorWorkingOrder_GotFocus);
			ViewerDebtNegotiatorBlotter.paymentSummaryReport.GotFocus += new RoutedEventHandler(ReportPaymentSummary_GotFocus);
			ViewerDebtNegotiatorBlotter.paymentSummaryReport.LostFocus += new RoutedEventHandler(ReportPaymentSummary_LostFocus);
			ViewerDebtNegotiatorBlotter.pdfReportControl.GotFocus += new RoutedEventHandler(ReportPDFControl_GotFocus);
			ViewerDebtNegotiatorBlotter.pdfReportControl.LostFocus += new RoutedEventHandler(ReportPDFControl_LostFocus);

			this.UpdateMenu();
		}



		/// <summary>
		/// Gets or sets the speed of animation.
		/// </summary>
		public AnimationSpeed AnimationSpeed
		{
			get { return (AnimationSpeed)this.GetValue(ViewerDebtNegotiatorBlotter.AnimationSpeedProperty); }
			set { this.SetValue(ViewerDebtNegotiatorBlotter.AnimationSpeedProperty, value); }
		}

		/// <summary>
		/// Gets or sets an indication of whether the headers can be modified or not.
		/// </summary>
		private Boolean IsHeaderFrozen
		{
			get { return (Boolean)this.GetValue(ViewerDebtNegotiatorBlotter.IsHeaderFrozenProperty); }
			set { this.SetValue(ViewerDebtNegotiatorBlotter.IsHeaderFrozenProperty, value); }
		}

		/// <summary>
		/// Gets or sets an indication of whether the panel layout can be changed or not.
		/// </summary>
		private Boolean IsLayoutFrozen
		{
			get { return (Boolean)this.GetValue(ViewerDebtNegotiatorBlotter.IsLayoutFrozenProperty); }
			set { this.SetValue(ViewerDebtNegotiatorBlotter.IsLayoutFrozenProperty, value); }
		}

		/// <summary>
		/// Gets or sets an indication of whether the navigation pane is visible or not.
		/// </summary>
		private Boolean IsNavigationPaneVisible
		{
			get { return (Boolean)this.GetValue(ViewerDebtNegotiatorBlotter.IsNavigationPaneVisibleProperty); }
			set { this.SetValue(ViewerDebtNegotiatorBlotter.IsNavigationPaneVisibleProperty, value); }
		}

		/// <summary>
		/// Gets or sets whether the scale factor used for magnifying the content.
		/// </summary>
		private Double Scale
		{
			get { return (Double)this.GetValue(ViewerDebtNegotiatorBlotter.ScaleProperty); }
			set { this.SetValue(ViewerDebtNegotiatorBlotter.ScaleProperty, value); }
		}

		

		/// <summary>
		/// Handles a change to the AnimationSpeed property.
		/// </summary>
		/// <param name="dependencyObject">The object that owns the property.</param>
		/// <param name="dependencyPropertyChangedEventArgs">A description of the changed property.</param>
		private static void OnAnimationSpeedChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{

			// Extract the strongly typed variables from the generic parameters.
			ViewerDebtNegotiatorBlotter viewerPrototype = dependencyObject as ViewerDebtNegotiatorBlotter;
			AnimationSpeed animationSpeed = (AnimationSpeed)dependencyPropertyChangedEventArgs.NewValue;

			// Set the animation speed on the report.
			ViewerDebtNegotiatorBlotter.reportDebtNegotiatorWorkingOrder.AnimationSpeed = animationSpeed;

			// Adjust the menu items for the new animation setting.
			viewerPrototype.menuItemSetAnimationFast.IsChecked = animationSpeed == AnimationSpeed.Fast;
			viewerPrototype.menuItemSetAnimationMedium.IsChecked = animationSpeed == AnimationSpeed.Medium;
			viewerPrototype.menuItemSetAnimationOff.IsChecked = animationSpeed == AnimationSpeed.Off;
			viewerPrototype.menuItemSetAnimationSlow.IsChecked = animationSpeed == AnimationSpeed.Slow;

		}

		private void OnCreateSlice(object sender, RoutedEventArgs e)
		{

			if (ViewerDebtNegotiatorBlotter.reportDebtNegotiatorWorkingOrder.Rows != null)
			{

				List<WorkingOrderRow> selectedRows = new List<WorkingOrderRow>();

				foreach (FluidTrade.Core.Windows.Controls.ReportRow reportRow in ViewerDebtNegotiatorBlotter.reportDebtNegotiatorWorkingOrder.Rows)
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

			DebtNegotiator entity = this.Content as DebtNegotiator;

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

		/// <summary>
		/// Handler for sendTo Blotter.
		/// </summary>
		/// <param name="sender">The "Move to..." menu item.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnSendTo(object sender, ExecutedRoutedEventArgs eventArgs)
		{

			ViewerDebtNegotiatorBlotter.reportDebtNegotiatorWorkingOrder.MoveRows();

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
			this.tabControl.SelectionChanged += new SelectionChangedEventHandler(OnTabControlSelectionChanged);
			ViewerDebtNegotiatorBlotter.staticTabControl = this.tabControl;
			this.SetValue(Page.ContentProperty, tabControl);

			// Working Order Tab
			this.tabWorkingOrder = new TabItem();
			this.tabWorkingOrder.Header = "Inventory Manager";

			// Match Tab
			this.tabMatch = new TabItem();
			ViewerDebtNegotiatorBlotter.staticMatchTab = this.tabMatch;
			this.tabMatch.Header = "Negotiation Manager";
			this.matchGrid = new Grid();
			RowDefinition matchRow0 = new RowDefinition();
			matchRow0.Height = new GridLength(1.0, GridUnitType.Star);
			matchGrid.RowDefinitions.Add(matchRow0);

			RowDefinition matchRow1 = new RowDefinition();
			matchRow1.Height = GridLength.Auto;
			matchGrid.RowDefinitions.Add(matchRow1);

			this.matchRow1Grid = new Grid();
			Grid.SetColumn(this.matchRow1Grid, 0);
			Grid.SetRow(this.matchRow1Grid, 0);

			// Create a column for the MatchedWorkingOrder report.
			ColumnDefinition matchColumn10 = new ColumnDefinition();
			matchColumn10.Width = new GridLength(1.0, GridUnitType.Star);
			this.matchRow1Grid.ColumnDefinitions.Add(matchColumn10);

			// Create a column for the NegotiationConsole.
			ColumnDefinition matchColumn11 = new ColumnDefinition();
			matchColumn11.Width = new GridLength(1.0, GridUnitType.Auto);
			this.matchRow1Grid.ColumnDefinitions.Add(matchColumn11);
			this.matchGrid.Children.Add(this.matchRow1Grid);

			// The lion's share of time to load a report is adding the child user interface elements to the report.  Recycling the cells from one report to
			// another makes things move much faster.  The viewer creates a single, static Prototype Report for its content and reuses it from one viewer to
			// the next.  The downside to this architecture is that the binding must be done in code because the XAML doesn't have access to the static report.			
			OnLoadTabWorkingOrder();

			this.matchRow1Grid.Children.Add(ViewerDebtNegotiatorBlotter.reportMatch);
			this.matchRow1Grid.Children.Add(ViewerDebtNegotiatorBlotter.debtNegotiatorNegotiationConsole);

			Grid.SetColumn(ViewerDebtNegotiatorBlotter.reportMatch, 0);
			Grid.SetRow(ViewerDebtNegotiatorBlotter.reportMatch, 0);

			Grid.SetColumn(ViewerDebtNegotiatorBlotter.debtNegotiatorNegotiationConsole, 2);
			Grid.SetRow(ViewerDebtNegotiatorBlotter.debtNegotiatorNegotiationConsole, 0);
			ViewerDebtNegotiatorBlotter.debtNegotiatorNegotiationConsole.HorizontalAlignment = HorizontalAlignment.Left;


			this.tabMatch.Content = this.matchGrid;

			InitializeSettlementTab();

			// Add the tabs to the page.
			tabControl.Items.Add(this.tabWorkingOrder);
			tabControl.Items.Add(this.tabMatch);
			tabControl.Items.Add(this.tabSettlement);


			// This selects which orders are displayed in the viewer.
			//if (DynamicReport.openReports.ContainsKey(this.blotter.BlotterId) == false)
			//{
			//    DataModel.BlotterId = this.blotter.BlotterId;
			//    DataModel.sequence = -1;
			//    DynamicReport.openReports.Add(this.blotter.BlotterId, DateTime.Now);
			//}

			ViewerDebtNegotiatorBlotter.reportDebtNegotiatorWorkingOrder.BlotterId = this.blotter.BlotterId;
			ViewerDebtNegotiatorBlotter.reportMatch.BlotterId = this.blotter.BlotterId;
			ViewerDebtNegotiatorBlotter.settlementReport.BlotterId = this.blotter.BlotterId;
			ViewerDebtNegotiatorBlotter.reportCreditCardDetail.BlotterId = this.blotter.BlotterId;
			ViewerDebtNegotiatorBlotter.paymentSummaryReport.BlotterId = this.blotter.BlotterId;
			ViewerDebtNegotiatorBlotter.paymentSummaryReport.ConsumerTrustSettlementId = Guid.Empty;
			ViewerDebtNegotiatorBlotter.reportCreditCardDetail.ConsumerId = Guid.Empty;

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
			BindingOperations.SetBinding(this, ViewerDebtNegotiatorBlotter.AnimationSpeedProperty, bindingAnimationSpeed);

			// Bind the "IsHeaderFrozen" property to the setting.
			Binding bindingApplyHeaderFrozen = new Binding("IsHeaderFrozen");
			bindingApplyHeaderFrozen.Source = FluidTrade.Guardian.Properties.Settings.Default;
			bindingApplyHeaderFrozen.Mode = BindingMode.TwoWay;
			BindingOperations.SetBinding(this, ViewerDebtNegotiatorBlotter.IsHeaderFrozenProperty, bindingApplyHeaderFrozen);

			// Bind the "IsLayoutFrozen" property to the setting.
			Binding bindingApplyLayoutFrozen = new Binding("IsLayoutFrozen");
			bindingApplyLayoutFrozen.Source = FluidTrade.Guardian.Properties.Settings.Default;
			bindingApplyLayoutFrozen.Mode = BindingMode.TwoWay;
			BindingOperations.SetBinding(this, ViewerDebtNegotiatorBlotter.IsLayoutFrozenProperty, bindingApplyLayoutFrozen);

			// Bind the "IsNavigationPaneVisible" property to the settings.
			Binding bindingIsNavigationPaneVisible = new Binding("IsNavigationPaneVisible");
			bindingIsNavigationPaneVisible.Source = FluidTrade.Guardian.Properties.Settings.Default;
			bindingIsNavigationPaneVisible.Mode = BindingMode.TwoWay;
			BindingOperations.SetBinding(this, ViewerDebtNegotiatorBlotter.IsNavigationPaneVisibleProperty, bindingIsNavigationPaneVisible);

			// Bind the "Scale" property to the setting.
			Binding bindingSliderScale = new Binding("Scale");
			bindingSliderScale.Source = FluidTrade.Guardian.Properties.Settings.Default;
			bindingSliderScale.Mode = BindingMode.TwoWay;
			BindingOperations.SetBinding(this, ViewerDebtNegotiatorBlotter.ScaleProperty, bindingSliderScale);

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
				ConsumerTrustSettlementRow consumerTrustSettlementRow = DataModel.ConsumerTrustSettlement.ConsumerTrustSettlementKey.Find(currentConsumerSettlementId);
				if (!consumerTrustSettlementRow.IsSettlementLetterNull())
					this.settlementLetter = Convert.FromBase64String(consumerTrustSettlementRow.SettlementLetter);
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

			Grid.SetColumn(ViewerDebtNegotiatorBlotter.settlementReport, 0);
			Grid.SetRow(ViewerDebtNegotiatorBlotter.settlementReport, 0);

			Grid.SetColumn(gridSplitterSettlement, 0);
			Grid.SetRow(gridSplitterSettlement, 1);

			this.settlemntTabControl = new TabControl();

			this.tabSettlementLetter = new TabItem();
			this.tabSettlementLetter.Header = "Settlement Letter";
			this.tabSettlementLetter.Content = ViewerDebtNegotiatorBlotter.pdfReportControl;

			// Clear out any cached settlement letter.
			ViewerDebtNegotiatorBlotter.pdfReportControl.ConsumerSettlementId = Guid.Empty;
			EnableApproveSettlementActions(false);

			//ViewerDebtNegotiatorBlotter.pdfReportControl.ViewBtn.Click += new RoutedEventHandler(OnViewbutton);
			this.tabSettlementPaymentSummary = new TabItem();
			this.tabSettlementPaymentSummary.Header = "Payment Summary";
			this.tabSettlementPaymentSummary.Content = ViewerDebtNegotiatorBlotter.paymentSummaryReport;

			Grid.SetColumn(settlemntTabControl, 0);
			Grid.SetRow(settlemntTabControl, 2);

			settlemntTabControl.Items.Add(this.tabSettlementLetter);
			settlemntTabControl.Items.Add(this.tabSettlementPaymentSummary);

			this.settlementRow1Grid.Children.Add(ViewerDebtNegotiatorBlotter.settlementReport);
			this.settlementRow1Grid.Children.Add(gridSplitterSettlement);
			this.settlementRow1Grid.Children.Add(settlemntTabControl);
			this.settlementGrid.Children.Add(this.settlementRow1Grid);
			this.tabSettlement.Content = settlementGrid;
		}

		/// <summary>
		/// Initialize tab working order.
		/// </summary>
		private void OnLoadTabWorkingOrder()
		{
			this.workingOrderGrid = new Grid();
			this.workingOrderRow1Grid = new Grid();

			RowDefinition workingColumn01 = new RowDefinition();
			workingColumn01.Height = new GridLength(4.0, GridUnitType.Star);

			RowDefinition workingColumn11 = new RowDefinition();
			// Sets the Height of the splitter in the working order report.
			workingColumn11.Height = new GridLength(7.0, GridUnitType.Pixel);

			RowDefinition workingColumn21 = new RowDefinition();
			workingColumn21.Height = new GridLength(1.0, GridUnitType.Star);


			this.workingOrderRow1Grid.RowDefinitions.Add(workingColumn01);
			this.workingOrderRow1Grid.RowDefinitions.Add(workingColumn11);
			this.workingOrderRow1Grid.RowDefinitions.Add(workingColumn21);


			Grid.SetColumn(ViewerDebtNegotiatorBlotter.reportDebtNegotiatorWorkingOrder, 0);
			Grid.SetRow(ViewerDebtNegotiatorBlotter.reportDebtNegotiatorWorkingOrder, 0);

			// Create the settlement report horizontal splitter.
			gridSplitterWorkingOder = new GridSplitter();
			gridSplitterWorkingOder.HorizontalAlignment = HorizontalAlignment.Stretch;
			gridSplitterWorkingOder.ShowsPreview = true;
			gridSplitterWorkingOder.Background = new System.Windows.Media.SolidColorBrush(this.splitColor);
			gridSplitterWorkingOder.ResizeDirection = GridResizeDirection.Rows;

			Grid.SetColumn(ViewerDebtNegotiatorBlotter.settlementReport, 0);
			Grid.SetRow(ViewerDebtNegotiatorBlotter.settlementReport, 0);

			Grid.SetColumn(gridSplitterWorkingOder, 0);
			Grid.SetRow(gridSplitterWorkingOder, 1);

			Grid.SetColumn(ViewerDebtNegotiatorBlotter.reportCreditCardDetail, 0);
			Grid.SetRow(ViewerDebtNegotiatorBlotter.reportCreditCardDetail, 2);


			this.workingOrderRow1Grid.Children.Add(ViewerDebtNegotiatorBlotter.reportDebtNegotiatorWorkingOrder);
			this.workingOrderRow1Grid.Children.Add(gridSplitterWorkingOder);
			this.workingOrderRow1Grid.Children.Add(ViewerDebtNegotiatorBlotter.reportCreditCardDetail);
			this.workingOrderGrid.Children.Add(this.workingOrderRow1Grid);
			this.tabWorkingOrder.Content = workingOrderGrid;

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
				EventLog.Error("Debt Negotiator Report Unloading before loading");
				return;
			}
			////End hack.

			// Unregister for the events when the reports are changed otherwise repeated calls to the methods occur and will appear as memory leak.
			ViewerDebtNegotiatorBlotter.reportDebtNegotiatorWorkingOrder.SelectionChanged -= new EventHandler(ReportDebtNegotiatorWorkingOrderSelectionChanged);
			ViewerDebtNegotiatorBlotter.settlementReport.SelectionChanged -= new EventHandler(OnSettlementReportSelectionChanged);
			ViewerDebtNegotiatorBlotter.reportMatch.SelectionChanged -= new EventHandler(OnMatchReportSelectionChanged);

			// Added eventhandler to be able to set the current selected report to the credit card report and back to the working order report.
			ViewerDebtNegotiatorBlotter.reportCreditCardDetail.GotFocus -= new RoutedEventHandler(ReportCreditCardDetail_GotFocus);
			ViewerDebtNegotiatorBlotter.reportDebtNegotiatorWorkingOrder.GotFocus -= new RoutedEventHandler(ReportDebtNegotiatorWorkingOrder_GotFocus);
			ViewerDebtNegotiatorBlotter.paymentSummaryReport.GotFocus -= new RoutedEventHandler(ReportPaymentSummary_GotFocus);
			ViewerDebtNegotiatorBlotter.paymentSummaryReport.LostFocus -= new RoutedEventHandler(ReportPaymentSummary_LostFocus);
			ViewerDebtNegotiatorBlotter.pdfReportControl.GotFocus -= new RoutedEventHandler(ReportPDFControl_GotFocus);
			ViewerDebtNegotiatorBlotter.pdfReportControl.LostFocus -= new RoutedEventHandler(ReportPDFControl_LostFocus);

			this.tabWorkingOrder.Content = null;
			this.workingOrderRow1Grid.Children.Remove(ViewerDebtNegotiatorBlotter.reportDebtNegotiatorWorkingOrder);
			this.workingOrderRow1Grid.Children.Remove(ViewerDebtNegotiatorBlotter.reportCreditCardDetail);

			this.matchRow1Grid.Children.Remove(ViewerDebtNegotiatorBlotter.reportMatch);
			this.matchRow1Grid.Children.Remove(ViewerDebtNegotiatorBlotter.debtNegotiatorNegotiationConsole);

			this.tabSettlement.Content = null;
			this.settlementRow1Grid.Children.Remove(ViewerDebtNegotiatorBlotter.settlementReport);
			this.settlementRow1Grid.Children.Remove(tabSettlement);

			this.tabSettlement.Content = null;
			this.tabSettlementLetter.Content = null;
			this.tabSettlementPaymentSummary.Content = null;

			// When the object is removed from the visual tree, there is no need to hold on to the bindings to the settings.
			// Another viewer may be installed and there's no reason why this viewer still needs to be a load on the settings data
			// structure.
			BindingOperations.ClearBinding(this, ViewerDebtNegotiatorBlotter.AnimationSpeedProperty);
			BindingOperations.ClearBinding(this, ViewerDebtNegotiatorBlotter.IsHeaderFrozenProperty);
			BindingOperations.ClearBinding(this, ViewerDebtNegotiatorBlotter.IsLayoutFrozenProperty);
			BindingOperations.ClearBinding(this, ViewerDebtNegotiatorBlotter.IsNavigationPaneVisibleProperty);
			BindingOperations.ClearBinding(this, ViewerDebtNegotiatorBlotter.ScaleProperty);

			// Removing the shared content from the viewer allows this viewer to be garbage collected while the core report will 
			// stay around until the current application domain is unloaded.
			this.SetValue(Page.ContentProperty, null);

			Viewer.lastSelectedtabItem.Upsert(this.blotter.BlotterId, this.tabControl.SelectedIndex);
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
			ViewerDebtNegotiatorBlotter viewerPrototype = dependencyObject as ViewerDebtNegotiatorBlotter;
			Boolean isHeaderFrozen = (Boolean)dependencyPropertyChangedEventArgs.NewValue;

			// This takes care of the actual work of setting the filter to display only the filled orders (or removing the filter).
			ViewerDebtNegotiatorBlotter.reportDebtNegotiatorWorkingOrder.IsHeaderFrozen = isHeaderFrozen;
			ViewerDebtNegotiatorBlotter.reportMatch.IsHeaderFrozen = isHeaderFrozen;
			ViewerDebtNegotiatorBlotter.settlementReport.IsHeaderFrozen = isHeaderFrozen;
			ViewerDebtNegotiatorBlotter.reportCreditCardDetail.IsHeaderFrozen = isHeaderFrozen;
			ViewerDebtNegotiatorBlotter.paymentSummaryReport.IsHeaderFrozen = isHeaderFrozen;

			// The user interface is modified here to reflect the change to the property.
			if (viewerPrototype.toggleButtonIsHeaderFrozen.IsChecked != isHeaderFrozen)
				viewerPrototype.toggleButtonIsHeaderFrozen.IsChecked = isHeaderFrozen;
			if (viewerPrototype.toggleColumnMode.IsChecked != isHeaderFrozen)
				viewerPrototype.toggleColumnMode.IsChecked = isHeaderFrozen;


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
			ViewerDebtNegotiatorBlotter viewerPrototype = dependencyObject as ViewerDebtNegotiatorBlotter;
			Boolean isLayoutFrozen = (Boolean)dependencyPropertyChangedEventArgs.NewValue;

			// This takes care of the actual work of setting the filter to display only the filled orders (or removing the filter).
			//ViewerDebtNegotiatorBlotter.reportDebtNegotiatorWorkingOrder.IsLayoutFrozen = isLayoutFrozen;

			//TODO: ************ Remove this next line otherwise it will permanantly set the IsLayoutFrozen ****************
			ViewerDebtNegotiatorBlotter.reportDebtNegotiatorWorkingOrder.IsLayoutFrozen = true;

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
			ViewerDebtNegotiatorBlotter viewerPrototype = dependencyObject as ViewerDebtNegotiatorBlotter;
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
			ViewerDebtNegotiatorBlotter viewerPrototype = dependencyObject as ViewerDebtNegotiatorBlotter;
			Double scale = (Double)dependencyPropertyChangedEventArgs.NewValue;

			// Set the animation speed on the report.
			ViewerDebtNegotiatorBlotter.reportDebtNegotiatorWorkingOrder.Scale = scale;
			ViewerDebtNegotiatorBlotter.reportMatch.Scale = scale;
			ViewerDebtNegotiatorBlotter.settlementReport.Scale = scale;
			ViewerDebtNegotiatorBlotter.reportCreditCardDetail.Scale = scale;
			ViewerDebtNegotiatorBlotter.paymentSummaryReport.Scale = scale;
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
					currentSelectedReport = ViewerDebtNegotiatorBlotter.reportMatch;
				else if (tabControl.SelectedItem == tabSettlement)
					currentSelectedReport = ViewerDebtNegotiatorBlotter.settlementReport;
				else
					currentSelectedReport = ViewerDebtNegotiatorBlotter.reportDebtNegotiatorWorkingOrder;

				if (this.previouslySelectedReport != this.currentSelectedReport)
					hasCurrentReportChanged = true;

				UpdateMenu();

				e.Handled = true;

			}
		}


		/// <summary>
		/// Filter orders with account balance greater than this
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>    
		private void OnBalanceChanged(object sender, TextChangedEventArgs e)
		{
			// This will remove any orders from the report that are less than the given text.
			Decimal filterValue;
			Decimal.TryParse(this.textBoxBalanceFilter.Text, out filterValue);
			ViewerDebtNegotiatorBlotter.reportMatch.BalanceFilter = filterValue;
			ViewerDebtNegotiatorBlotter.reportDebtNegotiatorWorkingOrder.BalanceFilter = filterValue;

		}

		private void OnIsEmployedChecked(object sender, RoutedEventArgs routedEventArgs)
		{
			ToggleButton toggle = this.toggleButtonApplyIsEmployedFilter;
			ViewerDebtNegotiatorBlotter.reportMatch.IsEmployedFilter = toggle.IsChecked.GetValueOrDefault(false);
			ViewerDebtNegotiatorBlotter.reportDebtNegotiatorWorkingOrder.IsEmpoyedFilter = toggle.IsChecked.GetValueOrDefault(false);

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
		/// Manually sets the current subset of blotters for this report.
		/// </summary>
		/// <param name="sender">Sender object</param>
		/// <param name="DependencyPropertyChangedEventArgs">Property changed event</param>
		private void OnMoveToIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{

			if (currentSelectedReport == ViewerDebtNegotiatorBlotter.reportDebtNegotiatorWorkingOrder)
			{
				this.menuItemSendTo.Visibility = Visibility.Visible;
			}
			else
				this.menuItemSendTo.Visibility = Visibility.Collapsed;
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
			if (currentSelectedReport == ViewerDebtNegotiatorBlotter.reportDebtNegotiatorWorkingOrder)
			{
				this.menuItemMarkAsRead.Visibility = Visibility.Collapsed;
				this.menuItemMarkAsUnread.Visibility = Visibility.Collapsed;
				this.contextMenuItemApplicationCommandDelete.Visibility = Visibility.Visible;
				if (isHeader)
				{

					this.menuItemSendTo.Visibility = Visibility.Collapsed;
					this.contextMenuItemSendTo.Visibility = Visibility.Collapsed;

				}
				else
				{

					this.menuItemSendTo.Visibility = Visibility.Visible;
					this.contextMenuItemSendTo.Visibility = Visibility.Visible;

				}
			}
			else
			{
				this.menuItemSendTo.Visibility = Visibility.Collapsed;
				this.contextMenuItemSendTo.Visibility = Visibility.Collapsed;
				this.menuItemApplicationCommandDelete.Visibility = Visibility.Collapsed;
				this.contextMenuItemApplicationCommandDelete.Visibility = Visibility.Collapsed;
			}
			if (currentSelectedReport == ViewerDebtNegotiatorBlotter.reportMatch)
			{
				this.menuItemMarkAsRead.Visibility = Visibility.Visible;
				this.menuItemMarkAsUnread.Visibility = Visibility.Visible;
				this.contextMenuItemMarkAsRead.Visibility = Visibility.Visible;
				this.contextMenuItemMarkAsUnread.Visibility = Visibility.Visible;
				this.menuItemApplicationCommandDelete.Visibility = Visibility.Collapsed;
				this.contextMenuItemApplicationCommandDelete.Visibility = Visibility.Collapsed;
			}
			else
			{
				this.menuItemMarkAsRead.Visibility = Visibility.Collapsed;
				this.menuItemMarkAsUnread.Visibility = Visibility.Collapsed;
				this.contextMenuItemMarkAsRead.Visibility = Visibility.Collapsed;
				this.contextMenuItemMarkAsUnread.Visibility = Visibility.Collapsed;
			}
			if (currentSelectedReport == ViewerDebtNegotiatorBlotter.reportCreditCardDetail)
			{
				this.menuItemMarkAsRead.Visibility = Visibility.Collapsed;
				this.menuItemMarkAsUnread.Visibility = Visibility.Collapsed;
				this.contextMenuItemMarkAsRead.Visibility = Visibility.Collapsed;
				this.contextMenuItemMarkAsUnread.Visibility = Visibility.Collapsed;
				this.menuItemApplicationCommandDelete.Visibility = Visibility.Visible;
				this.contextMenuItemApplicationCommandDelete.Visibility = Visibility.Visible;
				this.menuItemSendTo.Visibility = Visibility.Collapsed;
				this.contextMenuItemSendTo.Visibility = Visibility.Collapsed;
			}

			if (currentSelectedReport == ViewerDebtNegotiatorBlotter.settlementReport)
			{
				this.menuItemMarkAsRead.Visibility = Visibility.Collapsed;
				this.menuItemMarkAsUnread.Visibility = Visibility.Collapsed;
				this.contextMenuItemMarkAsRead.Visibility = Visibility.Collapsed;
				this.contextMenuItemMarkAsUnread.Visibility = Visibility.Collapsed;
				this.menuItemApplicationCommandDelete.Visibility = Visibility.Collapsed;
				this.contextMenuItemApplicationCommandDelete.Visibility = Visibility.Collapsed;
				if (currentSelectedReport.reportGrid != null && currentSelectedReport.reportGrid.CurrentReportCell != null && ViewerDebtNegotiatorBlotter.pdfReportControl.IsLetterLoaded && ViewerDebtNegotiatorBlotter.pdfReportControl.settlementLetter != null)
					EnableApproveSettlementActions(true);
				else
					EnableApproveSettlementActions(false);
			}
		}

		private void EnableApproveSettlementActions(bool IsEnabled)
		{
			if (IsEnabled)
			{

				this.menuItemPrintPreview.Visibility = Visibility.Visible;
				this.buttonPrintPreview.Visibility = Visibility.Visible;
				this.printPreviewSeparator.Visibility = Visibility.Visible;
			}
			else
			{

				this.menuItemPrintPreview.Visibility = Visibility.Collapsed;
				this.buttonPrintPreview.Visibility = Visibility.Collapsed;
				this.printPreviewSeparator.Visibility = Visibility.Collapsed;

			}
		}

		private void OnApproveSettlement(object sender, ExecutedRoutedEventArgs e)
		{
			// Commented out because the Debt Negotiator can not approve settlements.
			//ViewerDebtNegotiatorBlotter.settlementReport.ApproveSettlement();
		}

		private void OnMarkAsRead(object sender, ExecutedRoutedEventArgs e)
		{
			ViewerDebtNegotiatorBlotter.reportMatch.MarkMatchRowsAsRead();
		}

		private void OnMarkAsUnread(object sender, ExecutedRoutedEventArgs e)
		{
			ViewerDebtNegotiatorBlotter.reportMatch.MarkMatchRowsAsUnread();
		}

		private void OnApplicationCommandDelete(object sender, ExecutedRoutedEventArgs e)
		{
			if (currentSelectedReport == ViewerDebtNegotiatorBlotter.reportDebtNegotiatorWorkingOrder)
			{
				ViewerDebtNegotiatorBlotter.reportDebtNegotiatorWorkingOrder.DeleteRows();
			}

			if (currentSelectedReport == ViewerDebtNegotiatorBlotter.reportCreditCardDetail)
			{
				MessageBoxResult result;

				string message = "Are you sure you want to perform the delete operation?";
				result = MessageBox.Show(message, Application.Current.MainWindow.Title, MessageBoxButton.OKCancel);

				if (result == MessageBoxResult.OK)
				{
					ViewerDebtNegotiatorBlotter.reportCreditCardDetail.DeleteRows();
				}
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

