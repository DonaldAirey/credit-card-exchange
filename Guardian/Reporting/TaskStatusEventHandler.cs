using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using FluidTrade.Reporting.Interfaces;

namespace FluidTrade.Reporting
{
	/// <summary>
	/// Event Arg to communicate task status changes
	/// </summary>
    public class TaskStatusEventArgs : EventArgs
    {
		/// <summary>
        /// has the task been canceld
        /// </summary>
        private Boolean cancelled;

        /// <summary>
        /// list of error in the task, or possibly subtasks
        /// </summary>
        private List<Exception> errorList;

        /// <summary>
        /// task status
        /// </summary>
        private TaskStatus status;

		/// <summary>
		/// task Id
		/// </summary>
		private Task task;

		/// <summary>
        /// text to show to the user for progress purposes
        /// </summary>
        private string userDescriptionText;


        /// <summary>
        /// ctor
        /// </summary>
		/// <param name="task">task for the event arg</param>
		/// <param name="status">status of the task</param>
		/// <param name="userDescriptionText">text that can be displayed to user about task status</param>
		/// <param name="errorList">list of errors</param>
		/// <param name="cancelled">has the task been canceled</param>
		public TaskStatusEventArgs(Task task, TaskStatus status, string userDescriptionText,
                                    List<Exception> errorList, Boolean cancelled)
        {
            this.task = task;
            this.status = status;
            this.userDescriptionText = userDescriptionText;
            this.errorList = errorList;
            this.cancelled = cancelled;
        }

        /// <summary>
        /// get the taskId of the event
        /// </summary>
        public Task Task { get { return this.task; } }

        /// <summary>
        /// get the status of the task
        /// </summary>
        public TaskStatus Status { get { return this.status; } }

        public string UserDescriptionText { get { return this.userDescriptionText; } }

        // Summary:
        //     Gets a value indicating whether an asynchronous operation has been canceled.
        //
        // Returns:
        //     true if the background operation has been canceled; otherwise false. The
        //     default is false.
        public Boolean Cancelled { get { return this.cancelled; } }
        
        //
        // Summary:
        //     Gets a value indicating which error occurred during an asynchronous operation.
        //
        // Returns:
        //     An System.Exception instance, if an error occurred during an asynchronous
        //     operation; otherwise null.
        public List<Exception> ErrorList { get { return this.errorList; } }
    }

	/// <summary>
	/// callback for notification of task status changes
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="tsea"></param>
    public delegate void TaskStatusEventHandler(object sender, TaskStatusEventArgs tsea);
    

	/// <summary>
	/// enum for different task status
	/// </summary>
    public enum TaskStatus
    {
        None,
        Started,
        Info,
        Completed
    }
}
