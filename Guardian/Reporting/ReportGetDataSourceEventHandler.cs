using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrystalDecisions.CrystalReports.Engine;
using FluidTrade.Reporting.Interfaces;

namespace FluidTrade.Reporting
{
    /// <summary>
    /// EventArg used in ReportGenerationItem.GetConfigData callback. The ReportConfigureEventArgs is initailized
    /// with the IStaticReport, requiredColumnNames, 
    /// 
    /// The handler of ReportGenerationItem.GetConfigData. Must set the DataSource property and optionaly set the 
    /// DataSourceSyncObject if the dataSource should be locked when the report is being generated.
    /// </summary>
    public class ReportGetDataSourceEventArgs : EventArgs
    {
    	/// <summary>
        /// data source for the report to be generated from
        /// </summary>
        private System.Data.DataSet dataSource;

        /// <summary>
        /// object to lock when generating the report (can be null)
        /// </summary>
        private object dataSourceSyncObject;

    	/// <summary>
    	/// the IStaticReport instance for that is to be configured. 
    	/// </summary>
    	private IStaticReport report;

    	/// <summary>
    	/// the columns that are needed by the report in the dataSource
    	/// </summary>
    	private HashSet<string> requiredColumnNames;

    	/// <summary>
        /// ctor
        /// </summary>
        /// <param name="report"></param>
        /// <param name="requiredColumnNames">columns that are used in the report</param>
        public ReportGetDataSourceEventArgs(IStaticReport report,
            HashSet<string> requiredColumnNames)
        {
            this.report = report;
            this.requiredColumnNames = requiredColumnNames;
        }

		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="reportGetDataSourceEventArgs"></param>
		public ReportGetDataSourceEventArgs(ReportGetDataSourceEventArgs reportGetDataSourceEventArgs)
		{
			this.report = reportGetDataSourceEventArgs.report;
			this.requiredColumnNames = reportGetDataSourceEventArgs.requiredColumnNames;
			this.dataSource = reportGetDataSourceEventArgs.dataSource;
			this.dataSourceSyncObject = reportGetDataSourceEventArgs.dataSourceSyncObject;
		}

        /// <summary>
        /// get the columns that are needed by the report in the dataSource
        /// </summary>
        public HashSet<string> RequiredColumnNames { get { return this.requiredColumnNames; } }

		/// <summary>
		/// the IStaticReport instance for that is to be configured. 
		/// </summary>
		public IStaticReport Report { get { return this.report; } }

        /// <summary>
        /// get or set data source for the report to be generated from
        /// </summary>
        public System.Data.DataSet DataSource { get { return this.dataSource; } set { this.dataSource = value; } }

        /// <summary>
        /// get or set object to lock when generating the report (can be null)
        /// </summary>
        public object DataSourceSyncObject { get { return this.dataSourceSyncObject; } set { this.dataSourceSyncObject = value; } }
    }

    public delegate void ReportGetDataSourceEventHandler(object sender, ReportGetDataSourceEventArgs reportGetDataSourceEventArgs);

    
}
