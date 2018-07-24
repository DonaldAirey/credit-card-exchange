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
using FluidTrade.Reporting.Interfaces;

namespace FluidTrade.Reporting.Controls
{
    /// <summary>
    /// Interaction logic for ClientReportUserControl.xaml
    /// The ClientReportUserControl abstract class is the base for all report controls. 
    /// The base impl contains buttons and wiring for page navigation and printing
    /// 
    /// implementers should call SetReportContent() with their actual UI control once it
    /// is created. 
    /// </summary>
    public abstract partial class ClientReportUserControl : UserControl, FluidTrade.Reporting.Interfaces.IStaticReportControl
	{
    	/// <summary>
        /// The actual report control
        /// </summary>
        private UIElement innerReportControl;

    	/// <summary>
    	/// the iReport that defines the report and its definition/data: passed in through Initialize
    	/// </summary>
    	private IStaticReport report;

    	/// <summary>
        /// base ctor
        /// </summary>
        protected ClientReportUserControl()
        {
            InitializeComponent();
        }

    	#region IStaticReportControl Members

    	/// <summary>
		/// Handles can-execute events based on whether the viewer is displaying the first page or not.
		/// </summary>
		/// <param name="sender">The originator of the request.</param>
		/// <param name="eventArgs">The event arguments.</param>
		public abstract void CanGoBack(object sender, CanExecuteRoutedEventArgs eventArgs);

		/// <summary>
		/// Handles can-execute events based on whether the viewer is displaying the last page or not.
		/// </summary>
		/// <param name="sender">The originator of the request.</param>
		/// <param name="eventArgs">The event arguments.</param>
		public abstract void CanGoForward(object sender, CanExecuteRoutedEventArgs eventArgs);

		/// <summary>
		/// Show the first page of the report.
		/// </summary>
		public abstract void GoToFirstPage();

		/// <summary>
		/// Show the last page of the report.
		/// </summary>
		public abstract void GoToLastPage();

		/// <summary>
		/// Show the next page of the report.
		/// </summary>
		public abstract void GoToNextPage();

		/// <summary>
		/// Show the previous page of the report.
		/// </summary>
		public abstract void GoToPreviousPage();

        /// <summary>
		/// Present the user with a print dialog for the report.
		/// </summary>
		public abstract void PrintReport();


        /// <summary>
        /// Initialize the report. Must be called before operations or report is viewed. Mostlikely called 
        /// right after ctor
        /// </summary>
        /// <param name="report"></param>
        public void Initialize(IStaticReport report)
        {
            this.report = report;
            //call the abstract method so derived class can do their initialization
            this.InitializeInternal(report);
        }

    	/// <summary>
        /// get the IStaticReport that defines the report description and data
        /// </summary>
        public virtual IStaticReport Report
        {
            get
            {
                return report;
            }
        }

    	#endregion

    	/// <summary>
    	/// method for derived class to override. split apart from 
    	/// initialize so derived class does not have to worry about base class logic
    	/// such as setting report
    	/// </summary>
    	/// <param name="report"></param>
    	protected abstract void InitializeInternal(IStaticReport report);

    	/// <summary>
        /// Inserts the derived class UI control into the container
        /// </summary>
        /// <param name="innerReportControl">Derived class UI control that holds the report UI</param>
        protected virtual void SetReportContentControl(UIElement innerReportControl)
        {
            if(this.innerReportControl != null)
            {
                this.dockPanel.Children.Remove(this.innerReportControl);
            }

            this.innerReportControl = innerReportControl;
            this.dockPanel.Children.Add(this.innerReportControl);
        }
    }
}