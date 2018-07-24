namespace FluidTrade.Reporting.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// interface to define methods that need to be implmented to handle events from 
    /// the ReportGenerationItem. 
    /// </summary>
    public interface IStaticReportGenerationItemHandler
    {
        /// <summary>
        /// Event handler to get the data source information for the report. could be a global object
        /// with a lock or a copy. The reporting engine will lock on the lock object if
        /// it is supplied
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="reportGetDataSourceEventArgs"></param>
        void ReportGetData(object sender, ReportGetDataSourceEventArgs reportGetDataSourceEventArgs);
        
        /// <summary>
        /// Event handler to fill in neeeded parameters for the report. The user will be
        /// promoted for any missing parameters.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="reportFillParamsArgs"></param>
        void ReportFillParams(object sender, ReportFillParameterEventArgs reportFillParamsArgs);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="reportConfigureEventArgs"></param>
        //!!!RM for export once implemented
        //void ReportStreamEventHandler(object sender, ReportStreamEventArgs reportConfigureEventArgs);
    }
}
