namespace FluidTrade.Reporting.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Input;

    public interface IStaticReportControl
    {
    	/// <summary>
    	/// the report object passed through Initialize that contains the report defn/data/document
    	/// </summary>
    	IStaticReport Report { get; }

    	/// <summary>
        /// Handles can-execute events based on whether the viewer is displaying the first page or not.
        /// </summary>
        /// <param name="sender">The originator of the request.</param>
        /// <param name="eventArgs">The event arguments.</param>
        void CanGoBack(object sender, CanExecuteRoutedEventArgs eventArgs);

        /// <summary>
        /// Handles can-execute events based on whether the viewer is displaying the last page or not.
        /// </summary>
        /// <param name="sender">The originator of the request.</param>
        /// <param name="eventArgs">The event arguments.</param>
        void CanGoForward(object sender, CanExecuteRoutedEventArgs eventArgs);

        /// <summary>
        /// Show the first page of the report.
        /// </summary>
        void GoToFirstPage();

        /// <summary>
        /// Show the last page of the report.
        /// </summary>
        void GoToLastPage();

        /// <summary>
        /// Show the next page of the report.
        /// </summary>
        void GoToNextPage();

        /// <summary>
        /// Show the previous page of the report.
        /// </summary>
        void GoToPreviousPage();

        /// <summary>
        /// Present the user with a print dialog for the report.
        /// </summary>
        void PrintReport();

        /// <summary>
        /// Initialize the Control with the report object that contain the report defn/data/document
        /// </summary>
        /// <param name="report"></param>
        void Initialize(IStaticReport report);
    }
}
