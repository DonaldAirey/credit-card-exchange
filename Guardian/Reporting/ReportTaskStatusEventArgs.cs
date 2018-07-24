using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluidTrade.Reporting.Interfaces;

namespace FluidTrade.Reporting
{
	/// <summary>
	/// Subclass of TaskStatusEventArgs that adds the report specific property Report
	/// </summary>
    public class ReportTaskStatusEventArgs : TaskStatusEventArgs
    {
		/// <summary>
		/// the report that is associated with the task
		/// </summary>
        private IStaticReport report;

		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="report">Report for the eventArg</param>
		/// <param name="task">task for the event arg</param>
		/// <param name="status">status of the task</param>
		/// <param name="userDescriptionText">text that can be displayed to user about task status</param>
		/// <param name="errorList">list of errors</param>
		/// <param name="cancelled">has the task been canceled</param>
        public ReportTaskStatusEventArgs(IStaticReport report, Task task, TaskStatus status, string userDescriptionText,
                                    List<Exception> errorList, Boolean cancelled)
            : base(task, status, userDescriptionText, errorList, cancelled)
        {
            this.report = report;
        }

		/// <summary>
		/// get the Report associated with the task
		/// </summary>
        public IStaticReport Report { get { return this.report; }}
    }
}
