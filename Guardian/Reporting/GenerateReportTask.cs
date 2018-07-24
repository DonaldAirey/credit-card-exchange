using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using FluidTrade.Reporting.Interfaces;
using System.ComponentModel;

namespace FluidTrade.Reporting
{
    /// <summary>
    /// The GenerateReportTask is an encapsulated unit of work for generating reports. Each
    /// GerneratedReport task will spawn a new worker thread when Start() is called
    /// 
    /// the ReportGenTask takes the follwing steps in order to generate the report:
    /// 
    /// Ctor():
    ///         initialized with a list of reports and how to generate them
    ///         records the currentSync context; This will be used to send or post
    ///                 calls back to the thread that created the task.
    /// Start()
    ///         starts the worker Thread 0> WorkerProc()
    /// 
    /// WorkerProc()
    ///         loops through ReportGenItems (reportGenItems define the report and how to generate)
    ///         Creates IStaticReport Object (at this point only crystal)
    ///         calls report.PopulateReportConfiguration to build report definition
    ///         sends (blocking) to caller thread a request for GetConfigData the config data is the dataSource for the 
    ///                     report, the syncObject for locker (if any), and any parameter values
    ///         builds the report (back on the worker thread)
    ///         either exports the report to a file via a stream (!!!RM NOT IMPL YET)
    ///             or if ReportGenerationType.ShowReportInDlg 
    ///             the will display a window with the report contained. The window will be shown on 
    ///             the thread that created the ReportGenTask
    ///         
    /// 
    /// </summary>
    public class GenerateReportTask : Task
    {
    	private const string PATH_TAG = "_PATH_";

    	/// <summary>
    	/// the error exception (if any) will be up to caller to check this and do something
    	/// the worker thread should never throw an ex
    	/// </summary>
    	private Exception error;

    	/// <summary>
        /// generation information about the reports
        /// </summary>
        private ReportGenerationParameter generationParameter;

        /// <summary>
        /// has the task been canceld
        /// </summary>
        private Boolean isCanceled;

    	/// <summary>
    	/// worker thread the will generate the report
    	/// </summary>
    	private Thread workerThread;

		/// <summary>
		/// current report being worked on
		/// </summary>
		private IStaticReport currentReport;

		/// <summary>
		/// lock object for current report
		/// </summary>
		private object currentReportSync = new object();


    	/// <summary>
        /// ctor for report task
        /// </summary>
        /// <param name="generationParameter">Object that defines what the generation task will do</param>
        public GenerateReportTask(ReportGenerationParameter generationParameter)
            : base(null, SynchronizationContext.Current)
        {
            this.generationParameter = generationParameter;
        }

    	/// <summary>
    	/// has the task been canceld
    	/// </summary>
    	public Boolean IsCanceled { get { return this.isCanceled; } }

    	/// <summary>
    	/// the error exception (if any) will be up to caller to check this and do something
    	/// the worker thread should never throw an ex
    	/// </summary>
    	public Exception Error { get { return this.error; } }

    	/// <summary>
        /// start the task.  This will spawn a new thread to do the work of creating the report
        /// </summary>
        public override void Start()
        {
            this.workerThread = new Thread(new ThreadStart(this.WorkerProc));
            this.workerThread.IsBackground = true;

            this.workerThread.Name = string.Concat("ReportGen:", this.generationParameter.Name);
            this.workerThread.Start();
        }

        /// <summary>
        /// cancel the task. Since most of the work is done inside of crystal the only way to 
        /// cancel it to abort the thread. Hopefully this does not happen too much
        /// </summary>
        public override void Cancel()
        {
			if (this.IsCompleted == false)
			{
				this.workerThread.Abort();
				lock (currentReportSync)
				{
					if (this.currentReport != null)
					{
						try
						{
							this.currentReport.Cancel();
						}
						catch
						{
							//dont care
						}
					}
				}

			}
        }

        /// <summary>
        /// Worker thread metod that does the work of generating the report(s)
        /// </summary>
        private void WorkerProc()
        {
            try
            {
                //loop thru all the reports to generate
                foreach (ReportGenerationItem curItem in this.generationParameter.GenerationItems)
                {
                   
                    ////NOTE: at somepoint could have a factory to create
                    ////the correct IStaticReport Object but for now since it is
                    ////all crystal Just create a crystal report Object
                    IStaticReport report = new Crystal.CrystalReport();
					lock (currentReportSync)
						this.currentReport = report;

                    HashSet<string> requiredFieldNames = new HashSet<string>();
                    Dictionary<string, List<ReportParameterValue>> parameterNamesToValueMap = new Dictionary<string, List<ReportParameterValue>>();

                    //get the pre-configuration data from the IStaticReport Object
                    //the pre-configuration data is infomation about the report that 
                    //is needed to configure the report. 
                    //in most cases this would load the report template get a list of needed parameters
                    //and a list of needed fieldNames
                    report.PopulateReportConfiguration(this.generationParameter, curItem, requiredFieldNames, parameterNamesToValueMap);

                   
                    ReportGetDataSourceEventArgs getDataArgs = new ReportGetDataSourceEventArgs(report, requiredFieldNames);
					ReportGetDataSourceEventArgs originalGetDataArgs = getDataArgs;
                    //build fillParamsArgs tree
                    List<ReportFillParameterEventArgs> fillParamArgsList = this.CreateParameterArgsList(report, parameterNamesToValueMap);
                   
                    ProcessSendGetConfigDataEventArgs sendArgs = new ProcessSendGetConfigDataEventArgs();
                    sendArgs.curItem = curItem;
                    sendArgs.report = report;
                    sendArgs.getDataSourceArgs = getDataArgs;
                    sendArgs.fillParameterArgsList = fillParamArgsList;
                    sendArgs.parameterNamesToValueMap = parameterNamesToValueMap;

                    //sendArgs.parameterNamesToValueMap = parameterNamesToValueMap;

                    //is there is a syncContext then can send request back to caller thread
                    if (this.SyncContext != null)
                    {
                        //want to get this back to the caller thread to get the Config Data
                        //especially the parameterValues. Might want to split the call up
                        
                        //make blocking send call back to caller thread to get the configuration data
                        this.PostOrSend(false, new SendOrPostCallback(this.ProcessSendGetConfigData),
                                                                                        new Object[] { sendArgs });

                    }
                    else
                    {
                        //no syncContext so call on this thread
                        this.ProcessSendGetConfigData(sendArgs);
                    }

                    if (sendArgs.Validated == false)
                    {
                        //TODO: !!!RM LOCALIZE
                        //notify any subscribers of progress
                        this.Notify(new ReportTaskStatusEventArgs(report, this, TaskStatus.Info, string.Format("completed generation: {0}", curItem.TemplatePath), null, false));
                        continue;
                    }

					if (curItem.ReportTranslation != null)
					{
						getDataArgs = curItem.ReportTranslation.GetTranslatedObject(sendArgs);
						if (getDataArgs == null)
							continue;
					}
                    Object curItemDatsSyncObject = getDataArgs.DataSourceSyncObject; // get the sync Object as soon as the GetConfig() returns.

                    //Call IReportObject to configure the report. This will probably not build the report, but it could
                    //it would all depend on the impl of IStaticReport
                    // if it did then the BuildDocumentForView or export calls below would be faster
                    report.ConfigureReport(curItem, getDataArgs, parameterNamesToValueMap);

                    //TODO: !!!RM LOCALIZE
                    //notify any subscribers of progress
                    this.Notify(new ReportTaskStatusEventArgs(report, this, TaskStatus.Started, string.Format("starting generation: {0}", curItem.TemplatePath), null, false));

                    //if lock Object not null lock it 
                    if (curItemDatsSyncObject != null)
                    {
                        lock (curItemDatsSyncObject)
                        {
                            if (curItem.GenerationType == ReportGenerationType.ShowReportInDlg)
                                report.BuildDocumentForView();
                            else
                                report.Export();
                        }
                    }
                    else
                    {
                        if (curItem.GenerationType == ReportGenerationType.ShowReportInDlg)
                            report.BuildDocumentForView();
                        else
                            report.Export();
                    }

                    //TODO: !!!RM LOCALIZE
                    //notify any subscribers of progress
                    this.Notify(new ReportTaskStatusEventArgs(report, this, TaskStatus.Info, string.Format("completed generation: {0}", curItem.TemplatePath), null, false));

                    //if gen type is show then need to post a message back to 
                    //the caller thread to show the window. doning this after
                    //sending completed message
                    if (curItem.GenerationType == ReportGenerationType.ShowReportInDlg)
                    {
                        this.PostOrSend(false, new SendOrPostCallback(this.ShowReport),
                                                                new Object[] { report });
                    }                    
                }
            }
            catch (ThreadAbortException)
            {
                //catch the abort, and cancel the task
                Thread.ResetAbort();
               
                this.isCanceled = true;
            }
            catch (Exception ex)
            {
				System.Windows.MessageBox.Show(ex.ToString());
                //catch any other ex and set the property.
                //the task should never throw will be up to caller to determine what to do with
                //the exception
                this.error = ex;
            }
            finally
            {
                //all done
                this.SetTaskCompleted();
            }
        }


    	/// <summary>
        /// Creates and fills a list of ReportFillParameterEventArgs with all the parameters defined.
        /// 
        /// </summary>
        /// <param name="parameterNamesToValueMap"></param>
        /// <returns></returns>
        private List<ReportFillParameterEventArgs> CreateParameterArgsList(IStaticReport report,
                                    Dictionary<string, List<ReportParameterValue>> parameterNamesToValueMap)
        {
            List<ReportFillParameterEventArgs> fillParamArgsList = new List<ReportFillParameterEventArgs>();
            foreach (KeyValuePair<string, List<ReportParameterValue>> pair in parameterNamesToValueMap)
            {
                string paramName = pair.Key;
                string upperParamName = paramName.ToUpperInvariant();

                //if not a path just make a non path ReportFillParameterEventArgs
                if (!upperParamName.StartsWith(PATH_TAG))
                {
                    fillParamArgsList.Add(new ReportFillParameterEventArgs(report, paramName, pair.Value, PathType.None, null));
                    continue;
                }

                ReportFillParameterEventArgs parentPathArgs = new ReportFillParameterEventArgs(report, paramName, pair.Value,
                                                                                            PathType.Path, null);

                fillParamArgsList.Add(parentPathArgs);
                //split the path and create the args for the calls back to the provider
                string[] pathArgsAr = paramName.Substring(PATH_TAG.Length).Split('_');

                for (int i = 0; i < pathArgsAr.Length; i++)
                {
                    PathType pathType = (PathType)Enum.Parse(typeof(PathType), pathArgsAr[i], true);

                    ReportFillParameterEventArgs newPathArgs = new ReportFillParameterEventArgs(report, pathArgsAr[i], null,
                                                        pathType, parentPathArgs);

                    switch (pathType)
                    {
                        case PathType.SelectedItem:
                        case PathType.SelectedFolder:
                        case PathType.FolderParent:
                        case PathType.FolderRoot:
                        {
                            //positioning args
                            parentPathArgs = newPathArgs;
                            break;
                        }
                    }
                }
            }
                
            return fillParamArgsList;
        }

        /// <summary>
        /// Handler that does the show of the report window. This should alway be 
        /// on the Caller's thread
        /// </summary>
        /// <param name="args"></param>
        private void ShowReport(Object args)
        {
            IStaticReport report = (IStaticReport)args;
            FluidTrade.Reporting.Windows.WindowReportContainer window = new FluidTrade.Reporting.Windows.WindowReportContainer(report);
            window.Show();
        }

        /// <summary>
        /// Handler that calls the GetConfigData for the GenerationItem. This should alway be 
        /// on the Caller's thread
        /// </summary>
        /// <param name="args"></param>
        private void ProcessSendGetConfigData(Object args)
        {
            ProcessSendGetConfigDataEventArgs sendArgs = (ProcessSendGetConfigDataEventArgs)args;

            sendArgs.curItem.GetDataHandler(this, sendArgs.getDataSourceArgs);

            if (sendArgs.fillParameterArgsList.Count != 0)
            {
                //List<ReportFillParameterEventArgs>
                //sendArgs.fillParameterArgsList
                //fist postion the "data pointer" to the correct item/folder
                

                foreach (ReportFillParameterEventArgs curFillArgs in sendArgs.fillParameterArgsList)
                {
                    //not a path so easy.. 
                    //subscriber probably not doing anything with it
                    if (curFillArgs.PathType == PathType.None)
                    {
                        sendArgs.curItem.FillParameterHandler(this, curFillArgs);
                        continue;
                    }

                    //so this is a path.. need to call for all the items.
                    ReportFillParameterEventArgs curPathFillArgs = curFillArgs.FirstChild;

                    bool atDataPosition = false;

                    ReportFillParameterEventArgs lastPathFillArgs = null;
                    //First get to the correct data position
                    while(curPathFillArgs != null && atDataPosition == false)
                    {
                        switch (curPathFillArgs.PathType)
                        {
                            case PathType.SelectedItem:
                            case PathType.SelectedFolder:
                            case PathType.FolderParent:
                            case PathType.FolderRoot:
                                {
                                    lastPathFillArgs = curPathFillArgs;
                                    sendArgs.curItem.FillParameterHandler(this, curPathFillArgs);
                                    curPathFillArgs = curPathFillArgs.FirstChild;
                                    break;
                                }
                            default:
                                atDataPosition = true;
                                break;
                        }
                    }

                    //the data to get is not defined so the dataPosition defines the data
                    if (atDataPosition == false)
                    {
                        if (lastPathFillArgs.Values != null)
                            curFillArgs.Values.AddRange(lastPathFillArgs.Values);
                        
                        continue;
                    }

                    //now try to get the data
                    foreach(ReportFillParameterEventArgs dataPathFillArgs in lastPathFillArgs.Children)
                    {
                        switch (dataPathFillArgs.PathType)
                        {
                            case PathType.CursorFolder:
                            case PathType.CursorChildFolders:
                            case PathType.CursorItems:
                                {
                                    sendArgs.curItem.FillParameterHandler(this, dataPathFillArgs);
                                    if (dataPathFillArgs.Values != null)
                                        curFillArgs.Values.AddRange(dataPathFillArgs.Values);

                                    break;
                                }
                            case PathType.CursorDescendantFolders:
                            case PathType.CursorDescendantItems:
                                {
                                    //RM could do both DescendantFolders and DescendantItems at the same
                                    //time, but guessing that only one will be used in any path 

                                    break;
                                }
                        }
                    }
                }//end foreach(fillParameterArgsList)
            }


            sendArgs.Validated = sendArgs.report.ValidateParameters(sendArgs.parameterNamesToValueMap);
                
        }

        /// <summary>
        /// mark the task as complete and notify any subscribers
        /// </summary>
        private void SetTaskCompleted()
        {
            //if it the task is already completed return
            if (this.IsCompleted == true)
                return;

            this.IsCompleted = true;

			try
			{
				//notify subscribers of status
				this.Notify(new ReportTaskStatusEventArgs(null, this, TaskStatus.Completed, "completed child task", null, this.isCanceled));
			}
			finally
			{
				//try to cleanup any temp files
				foreach (ReportGenerationItem item in this.generationParameter.GenerationItems)
				{
					try
					{
						if(string.IsNullOrEmpty(item.TemplatePath) == false)
						{
							string fileName = System.IO.Path.GetFileName(item.TemplatePath);
							if (fileName.StartsWith(ReportXmlHelper.TmpPrefix))
								System.IO.File.Delete(fileName);
						}
					}
					catch
					{
					}
				}
			}
        }
    }

	/// <summary>
	/// nested helper class that is used to store all the information that is used as the args parameter in the
	/// SendOrPostCallback for the GetConfigData()
	/// </summary>
	public class ProcessSendGetConfigDataEventArgs : System.EventArgs
	{
		public ReportGenerationItem curItem;
		public List<ReportFillParameterEventArgs> fillParameterArgsList;
		public ReportGetDataSourceEventArgs getDataSourceArgs;
		public Dictionary<string, List<ReportParameterValue>> parameterNamesToValueMap;
		public IStaticReport report;
		public bool Validated;
	}
}