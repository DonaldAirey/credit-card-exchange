namespace FluidTrade.Reporting.Crystal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using FluidTrade.Reporting.Interfaces;
    using CrystalDecisions.CrystalReports.Engine;
    using CrystalDecisions.Shared;
    using FluidTrade.Reporting.Controls;
    using CrystalDecisions.Windows.Forms;

    /// <summary>
    /// Implementation of IStaticReport for Crystal Reporting engine
    /// </summary>
    public class CrystalReport: IStaticReport
    {
    	/// <summary>
    	/// description of the report to generate report to generate. passed into configure report
    	/// </summary>
    	private ReportGenerationItem generationItem;

    	/// <summary>
        /// Report configuration information that is returned by ConfigureReport()
        /// </summary>
        private ReportGetDataSourceEventArgs getDataSourceArgs;

        /// <summary>
        /// Report configuration information that is returned by ConfigureReport()
        /// </summary>
        private Dictionary<string, List<ReportParameterValue>> parameterNamesToValueMap;

    	/// <summary>
        /// list of all documents (including sub docs in the report);
        /// </summary>
        private List<ReportDocument> reportDocList = new List<ReportDocument>();

    	/// <summary>
    	/// Inner Crystal object that contains the report data/defn
    	/// </summary>
    	private ReportDocument reportDocument;

    	/// <summary>
    	/// Get Inner Crystal object that contains the report data/defn
    	/// </summary>
    	public ReportDocument ReportDocument
    	{
    		get
    		{
    			return this.reportDocument;
    		}
    	}

    	#region IStaticReport Members

    	/// <summary>
        /// Gets information about requireFields and parameters of the report
        /// </summary>
        /// <param name="generationParameter">Description of all reports</param>
        /// <param name="generationItem">Description of the report to generate</param>
        /// <param name="requiredFieldNames">hashset of fieldname that are used in the report</param>
        /// <param name="parameterNamesToValueMap">dictionary of parameters and values</param>
        public void PopulateReportConfiguration(ReportGenerationParameter generationParameter,
            ReportGenerationItem generationItem, HashSet<string> requiredFieldNames, Dictionary<string, List<ReportParameterValue>> parameterNamesToValueMap)
        {
            if (String.IsNullOrEmpty(generationItem.TemplatePath) || !generationItem.TemplatePath.EndsWith(".rpt"))
                return;

            this.generationItem = generationItem;

            this.reportDocument = new ReportDocument();

            this.reportDocument.Load(generationItem.TemplatePath);
            this.reportDocList = new List<ReportDocument>();
            reportDocList.Add(reportDocument);
            
            //find fieldnames for this document
            FindReportFields(reportDocument, requiredFieldNames);

            //find documens and fieldNames in subReports
            WalkSubReports(reportDocument.ReportDefinition.Sections, reportDocList, requiredFieldNames);

            //!!! RM need to deal with lists of items .for defaults until then not filling in report's 
            //default values
            foreach (ParameterField paramField in reportDocument.ParameterFields)
            {
                List<ReportParameterValue> valuesList = new List<ReportParameterValue>();
                foreach (ParameterValue curVal in paramField.CurrentValues)
                {
                    //TODO: !!!RM need to change paramter value to a system type and not use the crystal type
                    valuesList.Add(new ReportParameterValue(curVal, null));
                }

                parameterNamesToValueMap[paramField.Name] = valuesList;
            }
        }

		/// <summary>
		/// cancel the generation of the report
		/// </summary>
		public void Cancel()
		{
			try
			{
				this.reportDocument.Close();
				this.reportDocument.Dispose();
			}
			catch
			{
				//dont care
			}
		}
        /// <summary>
        /// Sets up the report based on the generationItem and configureArgs. may or may not build the report
        /// </summary>
        /// <param name="generationItem">Description of the report to generate</param>
        /// <param name="configureArgs">report instance data</param>
        public void ConfigureReport(ReportGenerationItem generationItem, 
            ReportGetDataSourceEventArgs getDataSourceArgs,
            Dictionary<string, List<ReportParameterValue>> parameterNamesToValueMap)
        {
            //get the configuration args 
            this.getDataSourceArgs = getDataSourceArgs;
            this.parameterNamesToValueMap = parameterNamesToValueMap;

            //need to set source before seting params
            foreach (ReportDocument doc in this.reportDocList)
            {
                doc.SetDataSource(this.getDataSourceArgs.DataSource);
            }

            //loop thru parameters that are set and set them into the crystal documnet
            foreach (ParameterField paramField in reportDocument.ParameterFields)
            {
                ParameterValues currentParameterValues = new ParameterValues();

                List<ReportParameterValue> objList;
                this.parameterNamesToValueMap.TryGetValue(paramField.Name, out objList);

                //convert all the data to a string and let crystal covert it to its known type
                for(int i =0; i < objList.Count; i++)
                {
                    object val = objList[i].Value;
                    if (val == null)
                        continue;
                    object convertedObject = null;
                    convertedObject = val.ToString();
                    objList[i].Value = convertedObject;

                    if (objList[i].EndValue != null)
                        objList[i].EndValue = objList[i].EndValue.ToString();
                }

                //create single or range crystal values
                foreach(ReportParameterValue paramVal in objList)
                {
                   if(paramVal.IsRange == false)
                   {
                        ParameterDiscreteValue pv = new ParameterDiscreteValue();
                        pv.Value = paramVal.Value;
                        currentParameterValues.Add(pv);
                   }
                   else
                   {
                        ParameterRangeValue pv = new ParameterRangeValue();
                        pv.StartValue = paramVal.Value;
                        pv.EndValue = paramVal.EndValue;
                        currentParameterValues.Add(pv);
                    }
                }

                //apply the newly set parameter values to the document so they will be used in the generation
                ParameterFieldDefinitions parameterFieldDefinitions = reportDocument.DataDefinition.ParameterFields;
                ParameterFieldDefinition parameterFieldDefinition = parameterFieldDefinitions[paramField.Name];
                parameterFieldDefinition.ApplyCurrentValues(currentParameterValues);
            }
        }


        /// <summary>
        /// Get Report configuration information that is returned by ConfigureReport()
        /// </summary>
        public ReportGetDataSourceEventArgs GetDataSourceArgs
        {
            get
            {
                return this.getDataSourceArgs;
            }
        }

        /// <summary>
        /// Get map of names of parameters to a list of the parameter values. 
        /// 
        /// </summary>
        public Dictionary<string, List<ReportParameterValue>> ParameterNamesToValueMap
        {
            get
            {
                return this.parameterNamesToValueMap;
            }
        }

        /// <summary>
        /// Get description of the report to generate report to generate. passed into configure report
        /// </summary>
        public ReportGenerationItem GenerationItem
        {
            get
            {
                return this.generationItem;
            }
        }

        /// <summary>
        /// build the report/inner reportDocument based on configureArgs passed into ConfigureReport() without any ui control. UI control to be bound to document at a later time.
        /// </summary>
        public void BuildDocumentForView()
        {    
            //accessing a page in crystal causes the report to be generated
            this.reportDocument.FormatEngine.GetPage(new PageRequestContext());            
        }

        /// <summary>
        /// export the report/inner reportDocument based on configureArgs passed into ConfigureReport() 
        /// </summary>
        public void Export()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// create the ui control that is associated with the report. in this case CrystalReportControl
        /// </summary>
        /// <returns>a new CrystalReportControl that can be bound to this IStaticReport data</returns>
        public ClientReportUserControl CreateReportControl()
        {
            return new CrystalReportControl();
        }

    	/// check parameters prompting user for missing ones
    	/// </summary>
    	/// <param name="parameterNamesToValueMap"></param>
    	/// <returns>true if can continue</returns>
    	public bool ValidateParameters(Dictionary<string, List<ReportParameterValue>> parameterNamesToValueMap)
    	{
    		List<ParameterField> missingParamList = new List<ParameterField>();
            
    		//loop thru parameters in crystal doc looking
    		//for them in the paramNameValueMap
    		foreach (ParameterField paramField in this.reportDocument.ParameterFields)
    		{
    			List<ReportParameterValue> objList;
    			parameterNamesToValueMap.TryGetValue(paramField.Name, out objList);
    			if (objList == null || objList.Count == 0)
    			{
    				missingParamList.Add((ParameterField)paramField.Clone());
    			}
    		}

    		//nithing is missing
    		if (missingParamList.Count == 0)
    			return true;

    		//prepare crystal prompting dialog to prompt user
    		//for values. !!!RM their prompting dlg is messy probably need to do something else
    		ParameterFields parameterFieldInfo = new ParameterFields();
    		foreach (ParameterField missingField in missingParamList)
    		{
    			parameterFieldInfo.Add(missingField);
    		}

    		PromptingDialog dialog = new PromptingDialog();
    		if (dialog.DoModal(null, parameterFieldInfo) == -1)
    		{
    			//user canceled the dialog so cannot continue
    			return false;
    		}

    		//fill the missing fields entered in the dialog into
    		//the crystal document
    		foreach (ParameterField missingField in missingParamList)
    		{
    			ParameterRangeValue rangeValue = missingField.CurrentValues[0] as ParameterRangeValue;
    			ParameterDiscreteValue discreteValue = missingField.CurrentValues[0] as ParameterDiscreteValue;
    			if (rangeValue != null)
    			{
    				List<ReportParameterValue> objList = new List<ReportParameterValue>();
    				ReportParameterValue paramVal = new ReportParameterValue(rangeValue.StartValue, null);
    				paramVal.IsRange = true;
    				paramVal.EndValue = rangeValue.EndValue;
    				objList.Add(paramVal);

    				parameterNamesToValueMap[missingField.Name] = objList;
    			}
    			else
    			{
    				List<ReportParameterValue> objList = new List<ReportParameterValue>();
    				objList.Add(new ReportParameterValue(discreteValue.Value, null));

    				parameterNamesToValueMap[missingField.Name] = objList;
    			}
    		}

    		//ok to continue with report generation since all fields are found
    		return true;
    	}

    	#endregion

    	/// <summary>
        /// iterate though subReports to populate list of documents and fieldnames
        /// </summary>
        /// <param name="sections">Sections of document to walk</param>
        /// <param name="reportDocList">list of inner subreport docs to populate</param>
        /// <param name="fieldNames">hashset of fieldNames to populate</param>
        private void WalkSubReports(Sections sections, List<ReportDocument> reportDocList, HashSet<string> fieldNames)
        {
            foreach (CrystalDecisions.CrystalReports.Engine.Section section in sections)
            {
                // In each section we need to loop through all the reporting objects
                foreach (ReportObject reportObject in section.ReportObjects)
                {
                    if (reportObject is CrystalDecisions.CrystalReports.Engine.FieldObject)
                    {
                        DatabaseFieldDefinition fieldDefn = ((FieldObject)reportObject).DataSource as DatabaseFieldDefinition;

                        if (fieldDefn != null)
                            fieldNames.Add(string.Format("{0}.{1}", fieldDefn.TableName, fieldDefn.Name));
                    }

                    if (reportObject.Kind == ReportObjectKind.SubreportObject)
                    {
                        SubreportObject subReport = (SubreportObject)reportObject;
                        ReportDocument subDocument = subReport.OpenSubreport(subReport.SubreportName);
                        reportDocList.Add(subDocument);
                    }
                }
            }
        }
        /// <summary>
        /// get all the fields that the report uses to pass back via the configureArgs returned by ConfigureReport()
        /// </summary>
        /// <param name="report">crystal document to look for fields</param>
        /// <param name="fieldNames">hashset that contain all field names</param>
        private void FindReportFields(ReportDocument report, HashSet<string> fieldNames)
        {
            //loop though the group fields
            foreach (GroupNameFieldDefinition field in report.DataDefinition.GroupNameFields)
            {
                this.FindFieldNames(fieldNames, field.Name);
            }

            //loop through the sortfields
            foreach (SortField field in report.DataDefinition.SortFields)
            {
                DatabaseFieldDefinition dbFieldDefn = field.Field as DatabaseFieldDefinition;

                if (dbFieldDefn != null)
                    fieldNames.Add(string.Format("{0}.{1}", dbFieldDefn.TableName, dbFieldDefn.Name));
                else
                    this.FindFieldNames(fieldNames, field.Field.Name);
            }

            //loop through the summary fields
            foreach (SummaryFieldDefinition field in report.DataDefinition.SummaryFields)
            {
                this.FindFieldNames(fieldNames, field.Name);
            }

            //loop through alll the formulas
            foreach (FormulaFieldDefinition field in report.DataDefinition.FormulaFields)
            {
                this.FindFieldNames(fieldNames, field.Text);
            }

            //loop through all the links (data keys)
            foreach (TableLink tl in report.Database.Links)
            {
                foreach (CrystalDecisions.CrystalReports.Engine.DatabaseFieldDefinition o in tl.DestinationFields)
                {
                    fieldNames.Add(string.Format("{0}.{1}", o.TableName, o.Name));
                }
                foreach (CrystalDecisions.CrystalReports.Engine.DatabaseFieldDefinition o in tl.SourceFields)
                {
                    fieldNames.Add(string.Format("{0}.{1}", o.TableName, o.Name));
                }
            }

            //?might need to loop through sub reports.
        }

        /// <summary>
        /// find field names in a string denoted by "{fieldName}"
        /// </summary>
        /// <param name="hashSet">hashset to put fieldName into so do not duplicate</param>
        /// <param name="text">text that may or may not contain field names</param>
        private void FindFieldNames(HashSet<string> hashSet, string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            //since this is simple wont use regex
            Int32 startIndex = 0;
            Int32 endIndex = 0;
            while (true)
            {
                startIndex = text.IndexOf('{', startIndex);
                if (startIndex == -1)
                    break;

                endIndex = text.IndexOf('}', startIndex);
                if (startIndex == -1)
                    break;

                //skip the {
                startIndex++;
                hashSet.Add(text.Substring(startIndex, endIndex - startIndex));

                startIndex = endIndex;
            }
        }

        // <summary>
    }
}
