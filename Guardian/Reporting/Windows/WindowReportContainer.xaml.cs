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
using System.Windows.Shapes;
using FluidTrade.Reporting.Controls;
using FluidTrade.Reporting.Interfaces;

namespace FluidTrade.Reporting.Windows
{
    /// <summary>
    /// Interaction logic for ReportWindow.xaml
    /// A window that is aware of an IStaticReport
    /// can create the ClientReportUserControl from the IStaticReport and add it to the Window
    /// </summary>
    public partial class WindowReportContainer : Window
    {
        /// <summary>
        /// ctor
        /// </summary>
        public WindowReportContainer()
            :this(null)
        {
        }
       
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="report">IStaticReport that is used to create the control to be put into the ReportWindow</param>
        public WindowReportContainer(IStaticReport report)
        {
            InitializeComponent();

            if (report != null)
            {
                FluidTrade.Reporting.Controls.ClientReportUserControl reportCtrl = report.CreateReportControl();
                reportCtrl.Initialize(report);
				
				if(string.IsNullOrEmpty(report.GenerationItem.TemplateName) == true)
					this.Title = report.GenerationItem.TemplatePath;
				else
					this.Title = report.GenerationItem.TemplateName;

                this.ReportControl = reportCtrl;
            }
        }

        /// <summary>
        /// Get or Set the report UI control into the window
        /// </summary>
        public ClientReportUserControl ReportControl
        {
            get
            {
                return this.Content as ClientReportUserControl;
            }
            set
            {
                //set width and height to nan(auto)
                value.Height = double.NaN;
                value.Width = double.NaN;

                this.Content = value;
            }
        }
    }
}
