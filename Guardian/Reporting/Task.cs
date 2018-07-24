using System;
using System.Threading;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
namespace FluidTrade.Reporting.Interfaces
{
    /// <summary>
    /// basic task default impl the defines a unit of work
    /// </summary>
    public abstract class Task 
    {
    	/// <summary>
        /// delegate for async events always points to AsyncHandlerProc()
        /// </summary>
        private SendOrPostCallback asyncHandler;

        /// <summary>
        /// parent task
        /// </summary>
        private Task parentTask;

    	/// <summary>
    	/// SynchronizationContext for async events
    	/// </summary>
    	private SynchronizationContext syncContext;

    	/// <summary>
        /// default ctor
        /// </summary>
        protected Task()
            :this(null, null)
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="parent">parent of task could be null</param>
        protected Task(Task parent)
            : this(parent, (parent == null) ? null : parent.SyncContext)
        {
        }

        /// <summary>
        /// ctor taking parent task (can be null) and syncContext
        /// </summary>
        /// <param name="parent">parent task</param>
        /// <param name="syncContext">syncContext used to send/post back to the correct thread</param>
        protected Task(Task parent, SynchronizationContext syncContext)
        {
            this.parentTask = parent;
            this.syncContext = syncContext;
            this.asyncHandler = new SendOrPostCallback(this.AsyncHandlerProc);        }


    	/// <summary>
        /// get if the task has completed (protected setter)
        /// </summary>
        public Boolean IsCompleted { get; protected set; }


        /// <summary>
        /// get the task at the root of the tree
        /// </summary>
        public Task RootTask 
        {
            get
            {
                //if the parent is null this is the root
                if (this.parentTask == null)
                    return this;

                //ask the parent for its root to walk the tree
                return parentTask.RootTask;
            }
        }

        /// <summary>
        /// get task's parent
        /// </summary>
        public Task ParentTask
        {
            get
            {
               return parentTask;
            }
        }

        /// <summary>
        /// Sync Context used for async events
        /// </summary>
        public SynchronizationContext SyncContext
        {
            get
            {
                return this.syncContext;
            }
        }

    	/// <summary>
    	/// private event handler for events that are supposed to be raised on the caller thread (defined by the thread
    	/// ctor syncContext parameter)
    	/// </summary>
    	private event TaskStatusEventHandler asyncNotify;

    	/// <summary>
    	/// cancel the task
    	/// </summary>
    	public abstract void Cancel();

    	/// <summary>
    	/// start the task
    	/// </summary>
    	public abstract void Start();

    	/// <summary>
    	/// callback to subscriber on task thread may or may not be same as subscriber thread
    	/// </summary>
    	public event TaskStatusEventHandler SyncNotify;

    	/// <summary>
        /// raises the SyncNotify and then the AsyncNotify evnts in that order
        /// </summary>
        /// <param name="args"></param>
        protected virtual void Notify(TaskStatusEventArgs args)
        {
            //raise the syncNotify on the same thread as the caller of the Notify()
            if (this.SyncNotify != null)
                this.SyncNotify(this, args);

            //use the PostOrSend to raise the asyncNotify event on the thread that
            //is defined as using the synContext that is passed into the Task ctor
            if(this.asyncNotify != null)
                this.PostOrSend(true, asyncNotify, new object[]{this, args});
        }

        /// <summary>
        /// Post or send a delegate to the thread that
        /// is defined as using the synContext that is passed into the Task ctor
        /// in order for that the delegate event can be raised on the correct thread
        /// </summary>
        /// <param name="isPost">is the call a non-blocking post(true) or a blocking send(false)</param>
        /// <param name="handler">event handler that will be called on the thread defined by synContext</param>
        /// <param name="args">arguments for delegate call</param>
        protected void PostOrSend(Boolean isPost, Delegate handler, object[] args)
        {
            //return if null
            if (this.syncContext == null)
                return;

            //create eventArgs that hold information about the call
            ProcessAsyncEventArgs paea = new ProcessAsyncEventArgs(this, handler, args);

            if (isPost == false)
            {
                //throw here if trying to send to same thread
                if (this.syncContext == SynchronizationContext.Current)
                    throw new InvalidOperationException("Cannot use send to same thread, use post or SyncNotify event");
                this.syncContext.Send(this.asyncHandler, paea);
            }
            else
            {
                this.syncContext.Post(this.asyncHandler, paea);
            }
        }

        /// <summary>
        /// handler for the syncContext Post or Send Calls. This should be 
        /// on the thread that is defined by synContext
        /// </summary>
        /// <param name="state"></param>
        private void AsyncHandlerProc(object state)
        {
            //raise the event 
            RaiseEvent((ProcessAsyncEventArgs)state);
        }

        /// <summary>
        /// raise an event defined by a ProcessAsyncEventArgs. 
        /// The event is raised on the thread that is making the RaiseEvent() call
        /// </summary>
        /// <param name="paea">args that contain all the infomation about event to be raised</param>
        private static void RaiseEvent(ProcessAsyncEventArgs paea)
        {
            //try to cast to known types of handler since those are faster than a DynamicInvoke
            //in most cases the handler is either a TaskStatusEventHandler or a SendOrPostCallback
            //but throw in a few extras that we know about
            if (paea.Handler is FluidTrade.Reporting.TaskStatusEventHandler)
            {
                ((TaskStatusEventHandler)paea.Handler)(paea.Sender, (TaskStatusEventArgs)paea.Args[1]);
            }
            else if (paea.Handler is System.Threading.SendOrPostCallback)
            {
                ((System.Threading.SendOrPostCallback)paea.Handler)(paea.Args[0]);
            }
            else if (paea.Handler is EventHandler)
            {
                //event handler is 
                if ((paea.Args == null) || (paea.Args.Length < 1))
                {
                    ((EventHandler)paea.Handler)(paea.Sender, EventArgs.Empty);
                }
                else
                {
                    ((EventHandler)paea.Handler)(paea.Sender, (EventArgs)paea.Args[0]);
                }
            }
            else if (paea.Handler is MethodInvoker)
            {
                ((MethodInvoker)paea.Handler)();
            }
            else if (paea.Handler is WaitCallback)
            {
                ((WaitCallback)paea.Handler)(paea.Args[0]);
            }
            else
            {
                paea.RetVal = paea.Handler.DynamicInvoke(paea.Args);
            }
        }

        /// <summary>
        /// post callback to subscriber on the thread that the subscriber subscribed on
        /// </summary>
        public event TaskStatusEventHandler AsyncNotify
        {
            add
            {
                System.Diagnostics.Debug.Assert(syncContext != null, "SyncContext needs to be set first for Async events");
                
                this.asyncNotify += value;
            }
            remove
            {
                this.asyncNotify -= value;
            }
        }

    	#region Nested type: ProcessAsyncEventArgs

    	/// <summary>
        /// event args that contains information for the asyc event to be raised
        /// </summary>
        protected class ProcessAsyncEventArgs : EventArgs
        {
    		/// <summary>
            /// the parameters for the event. many times
            /// the sender is included in the array at pos 0
            /// </summary>
            private object[] args;
            
            /// <summary>
            /// delegate for event to be raised
            /// </summary>
            private Delegate handler;

            /// <summary>
            /// the return value of the call. 
            /// </summary>private object sender;
            private object retVal;

    		/// <summary>
    		/// the sender of the event
    		/// </summary>
    		private object sender;


    		/// <summary>
            /// ctor
            /// </summary>
            /// <param name="sender">the sender of the event</param>
            /// <param name="handler">delegate for event to be raised</param>
            /// <param name="args">the parameters for the event. many times the sender is included in the array at pos 0</param>
            public ProcessAsyncEventArgs(object sender, Delegate handler, object[] args)
            {
                this.sender = sender;
                this.handler = handler;
                this.args = args;

                Debug.Assert(handler != null, "Handler cannot be null");
            }

            /// <summary>
            /// get the sender of the event
            /// </summary>
            public object Sender { get { return this.sender; } }

            /// <summary>
            /// get the parameters for the event. many times
            /// the sender is included in the array at pos 0
            /// </summary>
            public object[] Args { get { return this.args; } }

            /// <summary>
            /// get delegate for event to be raised
            /// </summary>
            public Delegate Handler { get { return this.handler; } }
            
            /// <summary>
            /// get or set the return value of the call. 
            /// </summary>
            public object RetVal { get { return this.retVal; } set { this.retVal = value; } }
        }

    	#endregion

    	#region Nested type: ProcessAsyncHandler

    	protected delegate void ProcessAsyncHandler(ProcessAsyncEventArgs a);

    	#endregion
    }
}
