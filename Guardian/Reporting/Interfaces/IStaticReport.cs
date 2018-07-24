namespace FluidTrade.Reporting.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using FluidTrade.Reporting.Controls;

    /// <summary>
    /// interface for all Report. The IStaticReport defines the report document/definition/data but not the UI. The IStaticReport will know about what type of UI control
    /// can be bound to it and can return that through CreateReportControl().
    /// 
    /// The initialization of the IStaticReport  PopulateReportConfiguration() then ConfigureReport();
    /// PopulateReportConfiguration() first fills in required fieldNames and parameters.
    /// next ConfigureReport() will be called. the required fieldNames and parameters from PopulateReportConfiguration() will be available in configureArgs. 
    /// An external source can fill in the values for the parameterNamesToValueMap between PopulateReportConfiguration() and ConfigureReport()
    /// 
    /// </summary>
    public interface IStaticReport
    {
    	/// <summary>
        /// get the report instance data; passed into ConfigureReport
        /// </summary>
        ReportGetDataSourceEventArgs GetDataSourceArgs { get; }

        Dictionary<string, List<ReportParameterValue>> ParameterNamesToValueMap { get; }

        /// <summary>
        /// get the Description of the report to generate; passed into PopulateReportConfiguration() and ConfigureReport()
        /// </summary>
        ReportGenerationItem GenerationItem { get; }

    	/// <summary>
    	/// Gets information about requireFields and parameters of the report
    	/// </summary>
    	/// <param name="generationParameter">Description of all reports</param>
    	/// <param name="generationItem">Description of the report to generate</param>
    	/// <param name="requiredFieldNames">hashset of fieldname that are used in the report</param>
    	/// <param name="parameterNamesToValueMap">dictionary of parameters and values</param>
    	void PopulateReportConfiguration(ReportGenerationParameter generationParameter,
    	                                 ReportGenerationItem generationItem, 
    	                                 HashSet<string> requiredFieldNames,
    	                                 Dictionary<string, List<ReportParameterValue>> parameterNamesToValueMap);
        
    	/// <summary>
    	/// Sets up the report based on the generationItem and configureArgs. may or may not build the report
    	/// </summary>
    	/// <param name="generationItem">Description of the report to generate</param>
    	/// <param name="configureArgs">report instance data</param>
    	void ConfigureReport(ReportGenerationItem generationItem,
    	                     ReportGetDataSourceEventArgs getDataSourceArgs,
    	                     Dictionary<string, List<ReportParameterValue>> parameterNamesToValueMap);

    	/// <summary>
        /// Export the report to a file stream
        /// </summary>
        void Export();

        /// <summary>
        /// build the report for a UI control. This allows for the seperation of the document generation 
        /// to a background thread from the dispaly of the UI which should/could be on the MainUI thread
        /// this will be called before CreateReportControl()
        /// </summary>
        void BuildDocumentForView();

        /// <summary>
        /// Create a new instance of a UIControl that knows how to bind to the IStaticReport
        /// </summary>
        /// <returns></returns>
        ClientReportUserControl CreateReportControl();

        /// <summary>
        /// check all the parameters. Can bring up a dlg to ask user to fill in if missing
        /// </summary>
        /// <param name="parameterNamesToValueMap"></param>
        /// <returns>true if can continue</returns>
        bool ValidateParameters(Dictionary<string, List<ReportParameterValue>> parameterNamesToValueMap);

		/// <summary>
		/// cancel the report generation
		/// </summary>
		void Cancel();
    }

	public interface IStaticReportTranslation
	{
		ReportGetDataSourceEventArgs GetTranslatedObject(ProcessSendGetConfigDataEventArgs rgdsea); 
	}
}
