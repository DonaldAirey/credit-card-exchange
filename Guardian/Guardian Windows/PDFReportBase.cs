namespace FluidTrade.Guardian.Windows
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Threading;

    /// <summary>
    /// Abstract base class for PDFReports.
    /// </summary>
    abstract public class PDFReportBase : UserControl
    {

        // Dependency properties used by the foreground thread only.
        private static DependencyProperty ConsumerSettlementIdProperty = DependencyProperty.Register(
            "ConsumerSettlementId",
            typeof(Guid),
            typeof(PDFReportBase),
            new PropertyMetadata(PDFReportBase.OnConsumerSettlementId));


        /// <summary>
        /// Handle changes of the DependencyProperty that is registered with this event handler.
        /// </summary>
        /// <param name="sender">Originator of this event.</param>
        /// <param name="eventArgs">Argument containing data of this event.</param>
        private static void OnConsumerSettlementId(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
        {

            // Extract the target of this property change from the parameters.
            PDFReportBase pdfReport = sender as PDFReportBase;

            // Update this control with information associated with the settlementId.  Do this on a background thread because the DataModel needs to be used.
            FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(new WaitCallback(pdfReport.InitializeData), eventArgs.NewValue);

        }

        // Public properties

        /// <summary>
        /// The Id associated with this Settlement.
        /// </summary>
        public Guid ConsumerSettlementId
        {
            get { return (Guid)this.GetValue(PDFReportBase.ConsumerSettlementIdProperty); }
            set { this.SetValue(PDFReportBase.ConsumerSettlementIdProperty, value); }
        }

        // Abstract methods.

        /// <summary>
        /// Method to initialize the data after the SettlementID has been updated.
        /// </summary>
        /// <param name="parameter"></param>
        abstract protected void InitializeData(object parameter);

    }
}
