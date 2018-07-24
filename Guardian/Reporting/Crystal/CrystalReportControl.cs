namespace FluidTrade.Reporting.Crystal
{
	using System;
	using System.Collections.Generic;
	using System.Windows;
	using System.Windows.Forms.Integration;
	using System.Windows.Input;
	using System.Windows.Threading;
	using CrystalDecisions.CrystalReports.Engine;
	using CrystalDecisions.Shared;
	using CrystalDecisions.Windows.Forms;
	using FluidTrade.Reporting.Controls;

	/// <summary>
	/// A viewer for Crystal Reports reports.
	/// </summary>
    public class CrystalReportControl : ClientReportUserControl
	{
        /// <summary>
        /// container for the CrystalViewerControl
        /// </summary>
		private WindowsFormsHost host;

        /// <summary>
        /// IStaticReport instance
        /// </summary>
        private CrystalReport report;
        
		/// <summary>
		/// Create a new Crystal Reports client report.
		/// </summary>
        public CrystalReportControl()
		{
			this.host = new WindowsFormsHost();
            this.SetReportContentControl(host);
		}

		/// <summary>
		/// Handles can-execute events based on whether the viewer is displaying the first page or not.
		/// </summary>
		/// <param name="sender">The originator of the request.</param>
		/// <param name="eventArgs">The event arguments.</param>
		public override void CanGoBack(object sender, CanExecuteRoutedEventArgs eventArgs)
		{
			CrystalReportViewer viewer = this.host.Child as CrystalReportViewer;

			eventArgs.CanExecute = viewer != null && viewer.ReportSource != null && viewer.GetCurrentPageNumber() != 1;
		}

		/// <summary>
		/// Handles can-execute events based on whether the viewer is displaying the last page or not.
		/// </summary>
		/// <param name="sender">The originator of the request.</param>
		/// <param name="eventArgs">The event arguments.</param>
		public override void CanGoForward(object sender, CanExecuteRoutedEventArgs eventArgs)
		{

			CrystalReportViewer viewer = this.host.Child as CrystalReportViewer;
			PageView pageViewer = viewer.Controls[0] as PageView;

			eventArgs.CanExecute =
				viewer != null &&
				viewer.ReportSource != null &&
				(pageViewer == null || viewer.GetCurrentPageNumber() != pageViewer.GetLastPageNumber());

		}

		/// <summary>
		/// Show the first page of the report.
		/// </summary>
		public override void GoToFirstPage()
		{

			if (this.host.Child is CrystalReportViewer)
				(this.host.Child as CrystalReportViewer).ShowFirstPage();

		}

		/// <summary>
		/// Show the last page of the report.
		/// </summary>
		public override void GoToLastPage()
		{
			if (this.host.Child is CrystalReportViewer)
				(this.host.Child as CrystalReportViewer).ShowLastPage();
		}

		/// <summary>
		/// Show the next page of the report.
		/// </summary>
		public override void GoToNextPage()
		{
			if (this.host.Child is CrystalReportViewer)
				(this.host.Child as CrystalReportViewer).ShowNextPage();
		}

		/// <summary>
		/// Show the previous page of the report.
		/// </summary>
		public override void GoToPreviousPage()
		{
			if (this.host.Child is CrystalReportViewer)
				(this.host.Child as CrystalReportViewer).ShowPreviousPage();
		}

		/// <summary>
		/// Build a Crystal Reports viewer and display it.
		/// </summary>
		/// <returns>The newly created viewer.</returns>
		private CrystalReportViewer GetCrystalViewer()
		{
			CrystalReportViewer viewer = this.host.Child as CrystalReportViewer;

			if (viewer == null)
			{
				viewer = new CrystalReportViewer();

				viewer.HandleException += OnCrystalException;
				viewer.Error += OnCrystalException;
				viewer.DisplayGroupTree = false;
				viewer.DisplayStatusBar = false;
				viewer.DisplayToolbar = true;
                viewer.ShowPageNavigateButtons = true;
                viewer.ShowGroupTreeButton = false;
                viewer.ShowGotoPageButton = true;
                viewer.ShowRefreshButton = false;
                viewer.ShowTextSearchButton = true;
            
				viewer.DisplayBackgroundEdge = false;
                viewer.EnableDrillDown = false;
				viewer.Dock = System.Windows.Forms.DockStyle.Fill;
				viewer.AutoSize = true;
				viewer.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
				this.host.Child = viewer;
			}

			return viewer;
		}

		/// <summary>
		/// "Handle" exceptions thrown up by Crystal Reports.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="eventArgs"></param>
		private void OnCrystalException(object source, CrystalDecisions.Windows.Forms.ExceptionEventArgs eventArgs)
		{
			eventArgs.Handled = true;
		}

		/// <summary>
		/// Present the user with a print dialog for the report.
		/// </summary>
		public override void PrintReport()
		{
			if (this.host.Child is CrystalReportViewer)
			{
				CrystalReportViewer viewer = this.host.Child as CrystalReportViewer;
				viewer.PrintReport();
			}
		}

        /// <summary>
        /// Initialize the control
        /// </summary>
        /// <param name="report"></param>
        protected override void InitializeInternal(FluidTrade.Reporting.Interfaces.IStaticReport report)
        {
            //the IStaticReport needs to be a CrystalReport
            this.report = (CrystalReport)report;

			CrystalReportViewer viewer = this.GetCrystalViewer();
     
            //subscribe to event to set the view content after the 
            this.Loaded -= new RoutedEventHandler(CrystalReportControl_Loaded); 
            this.Loaded += new RoutedEventHandler(CrystalReportControl_Loaded);
        }

        /// <summary>
        /// handler when the contol is "shown" bind the control to the data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CrystalReportControl_Loaded(object sender, RoutedEventArgs e)
        {
			CrystalReportViewer viewer = this.GetCrystalViewer();
            viewer.ReportSource = this.report.ReportDocument;
        }

        
    }

}
