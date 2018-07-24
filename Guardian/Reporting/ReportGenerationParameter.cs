using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluidTrade.Reporting.Interfaces;
using System.ComponentModel;
using System.Threading;

namespace FluidTrade.Reporting
{
	/// <summary>
	/// Container for multiple ReportGenerationItems. 
	/// This is the parameter that is passed to the ReportTask.  It allows 
	/// for the naming of the set of reports to generate, and to define the 
	/// set of reports
	/// </summary>
    public class ReportGenerationParameter
    {
		/// <summary>
		/// list of generation item
		/// </summary>
        private List<ReportGenerationItem> generationItems;

		/// <summary>
		/// name of the set of reportGeneration Items
		/// </summary>
		private string name;


		/// <summary>
		/// ctor list of reportGenerationItems to be built with the GenReportTask
		/// </summary>
		/// <param name="generationItems"></param>
        public ReportGenerationParameter(List<ReportGenerationItem> generationItems)
        {
            this.generationItems = generationItems;
        }

       
		/// <summary>
		/// Get the list of ReportGenerationItems
		/// </summary>
        public List<ReportGenerationItem> GenerationItems { get { return this.generationItems; } }
        
		/// <summary>
		/// Get or set the name of the GenerationItem set
		/// </summary>
		public string Name { get { return this.name; } set { this.name = value; } }
    }
}