using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluidTrade.Reporting.Interfaces;

namespace FluidTrade.Reporting
{
    /// <summary>
    /// Description of a single report to generate. 
    /// 
    /// Need to pass in a configHandler which will get called when the report is being generated to
    /// get the configuration. The handler needs to return a ReportConfigureEventArgs. 
    /// This is done through a handler instead of through the ctor
    /// because the reports can be passed as a list that could have different configurations. If at any
    /// time the generation is canceled for the list the the configure does not get called. This configure
    /// could possibly clone data, and not have a sync object at all in this case it is much better to have
    /// a handler rather than clone the data for the ctor.
    /// </summary>
    public class ReportGenerationItem
    {
    	/// <summary>
        /// callback for getting stream for non ui report export
        /// </summary>
        private ReportStreamEventHandler createExportStreamHandler;

    	/// <summary>
    	/// callback for getting parameter values
    	/// </summary>
    	private ReportFillParameterEventHandler fillParameterHandler;

    	/// <summary>
        /// how is the report supposed to be generate export or view
        /// </summary>
        private ReportGenerationType generationType;

    	/// <summary>
    	/// callback for getting dataSource/dataSourceLock
    	/// </summary>
    	private ReportGetDataSourceEventHandler getDataHandler;

    	/// <summary>
    	/// Description text of report. only makes sense if stored in db
    	/// </summary>
    	private string reportDescription;

    	/// <summary>
    	/// the name of the report. only makes sense if stored in the db. if on disk 
    	/// then the fileName is the name
    	/// </summary>
    	private string reportName;

    	/// <summary>
    	/// output path for a report as an export. Could be exported into db
    	/// </summary>
    	private string reportPath;


		/// <summary>
		/// custom report translator
		/// </summary>
		private IStaticReportTranslation reportTranslation;


    	/// <summary>
    	/// Description text of template. only makes sense if stored in db
    	/// </summary>
    	private string templateDescription;

    	/// <summary>
    	/// the name of the template. only makes sense if stored in the db. if on disk 
    	/// then the fileName is the name
    	/// </summary>
    	private string templateName;

    	/// <summary>
    	/// path to the report template file (probably prefixed, and guid if in db)
    	/// </summary>
    	private string templatePath;

    	/// <summary>
        /// 
        /// </summary>
        /// <param name="templatePath"></param>
        /// <param name="templateName"></param>
        /// <param name="templateDescription"></param>
        /// <param name="generationType"></param>
        /// <param name="configureHandler"></param>
        public ReportGenerationItem(string templatePath,
                                    string templateName,
                                    string templateDescription,
									IStaticReportTranslation reportTranslation,
								    ReportGenerationType generationType,
                                    ReportGetDataSourceEventHandler getDataHandler,
                                    ReportFillParameterEventHandler fillParameterHandler)
            : this(templatePath, templateName, templateDescription, null, null, null, reportTranslation, generationType,
                                    getDataHandler, fillParameterHandler, null)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="templatePath"></param>
        /// <param name="templateName"></param>
        /// <param name="templateDescription"></param>
        /// <param name="reportPath"></param>
        /// <param name="reportName"></param>
        /// <param name="reportDescription"></param>
        /// <param name="generationType"></param>
        /// <param name="configureHandler"></param>
        /// <param name="createExportStreamHandler"></param>
        public ReportGenerationItem(string templatePath,
                                    string templateName,
                                    string templateDescription,
                                    string reportPath,
                                    string reportName,
                                    string reportDescription,
									IStaticReportTranslation reportTranslation,
                                    ReportGenerationType generationType,
                                    ReportGetDataSourceEventHandler getDataHandler,
                                    ReportFillParameterEventHandler fillParameterHandler,
                                    ReportStreamEventHandler createExportStreamHandler)
        {
            this.templatePath = templatePath;
            this.templateName = templateName;
            this.templateDescription = templateDescription;
            this.reportPath = reportPath;
            this.reportName = reportName;
			this.reportTranslation = reportTranslation;
            this.reportDescription = reportDescription;
            this.generationType = generationType;
            this.getDataHandler = getDataHandler;
            this.fillParameterHandler = fillParameterHandler;
            this.createExportStreamHandler = createExportStreamHandler;

            if (templatePath == null)
                throw new ArgumentNullException("templatePath", "templatePath cannot be null");

            if (getDataHandler == null)
                throw new ArgumentNullException("configureHandler", "configureHandler cannot be null");

            if (fillParameterHandler == null)
                throw new ArgumentNullException("fillParameterHandler", "fillParameterHandler cannot be null");

            if (generationType != ReportGenerationType.ShowReportInDlg)
            {
                if (reportPath == null)
                    throw new ArgumentNullException("reportPath", "reportPath cannot be null for an export generation type");

                if (createExportStreamHandler == null)
                    throw new ArgumentNullException("createExportStreamHandler", "createExportStreamHandler cannot be null for an export generation type");
            }
        }

        /// <summary>
        /// Get path to the report template file (probably prefixed, and guid if in db)
        /// </summary>
        public string TemplatePath { get { return this.templatePath; }}

        /// <summary>
        /// Get the name of the template. only makes sense if stored in the db. if on disk 
        /// then the fileName is the name
        /// </summary>
        public string TemplateName { get { return this.templateName; } }

        /// <summary>
        /// Get Description text of template. only makes sense if stored in db
        /// </summary>
        public string TemplateDescription { get { return this.templateDescription; } }

        /// <summary>
        /// Get output path for a report as an export. Could be exported into db
        /// </summary>
        public string ReportPath { get { return this.reportPath; } }

        /// <summary>
        /// Get the name of the report. only makes sense if stored in the db. if on disk 
        /// then the fileName is the name
        /// </summary>
        public string ReportName { get { return this.reportName; } }


		/// <summary>
		/// custom report translator
		/// </summary>
		public IStaticReportTranslation ReportTranslation { get { return this.reportTranslation; } }


        /// <summary>
        /// Get Description text of report. only makes sense if stored in db
        /// </summary>
        public string ReportDescription { get { return this.reportDescription; } }

        /// <summary>
        /// Get how is the report supposed to be generated: exported or viewed
        /// </summary>
        public ReportGenerationType GenerationType { get { return this.generationType; } }

        /// <summary>
        /// Get callback for getting dataSource/dataSourceLock
        /// </summary>
        public ReportGetDataSourceEventHandler GetDataHandler { get { return this.getDataHandler; } }

        /// <summary>
        /// Get callback for getting parameter values
        /// </summary>
        public ReportFillParameterEventHandler FillParameterHandler { get { return this.fillParameterHandler; } }

        /// <summary>
        /// Get callback for getting stream for non ui report export
        /// </summary>
        public ReportStreamEventHandler CreateExportStreamHandler
        {
            get
            {
                return this.createExportStreamHandler;
            }
        }        
    }
}
