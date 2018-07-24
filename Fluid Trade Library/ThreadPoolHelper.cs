using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace FluidTrade.Core
{
	// Summary:
	//     ThreadPoolHelper is a wrapper around the system ThreadPool class. 
	//      ThreadPoolHelper will catch all thread exceptions an log them instead of bringing down the application 
	//	   Provides a pool of threads that can be used to post work items, process asynchronous
	//     I/O, wait on behalf of other threads, and process timers.
	// 
	public static class ThreadPoolHelper
	{
		//
		// Summary:
		//     Queues a method for execution. The method executes when a thread pool thread
		//     becomes available.
		//
		// Parameters:
		//   callBack:
		//     A System.Threading.WaitCallback that represents the method to be executed.
		//
		// Returns:
		//     true if the method is successfully queued; System.OutOfMemoryException is
		//     thrown if the work item could not be queued.
		//
		// Exceptions:
		//   System.ApplicationException:
		//     An out-of-memory condition was encountered.
		//
		//   System.OutOfMemoryException:
		//     The work item could not be queued.
		//
		//   System.ArgumentNullException:
		//     callBack is null.
		public static bool QueueUserWorkItem(WaitCallback callBack)
		{
			return ThreadPool.QueueUserWorkItem(CallbackProc, new WorkItemWrapper(callBack, null));
		}

		//
		// Summary:
		//     Queues a method for execution, and specifies an object containing data to
		//     be used by the method. The method executes when a thread pool thread becomes
		//     available.
		//
		// Parameters:
		//   callBack:
		//     A System.Threading.WaitCallback representing the method to execute.
		//
		//   state:
		//     An object containing data to be used by the method.
		//
		// Returns:
		//     true if the method is successfully queued; System.OutOfMemoryException is
		//     thrown if the work item could not be queued.
		//
		// Exceptions:
		//   System.ApplicationException:
		//     An out-of-memory condition was encountered.
		//
		//   System.OutOfMemoryException:
		//     The work item could not be queued.
		//
		//   System.ArgumentNullException:
		//     callBack is null.
		public static bool QueueUserWorkItem(WaitCallback callBack, object state)
		{
			return ThreadPool.QueueUserWorkItem(CallbackProc, new WorkItemWrapper(callBack, state));
		}

		/// <summary>
		/// all threadPool workQ items come thought this method before they the real
		/// callback is invoked
		/// </summary>
		/// <param name="state"></param>
		private static void CallbackProc(object state)
		{
			//since all queue items are from one of the Helper.QueueUserWorkItem()
			//then the state will be a WorkItemWrapper
			((WorkItemWrapper)state).Invoke();
		}

		/// <summary>
		/// class that gets passed as the state for the QueueUserWorkItem.
		/// WorkItemWrapper contains the real callback and the real state for the worker
		/// </summary>
		private class WorkItemWrapper
		{
			/// <summary>
			/// real callback that should be called by the worker thread
			/// </summary>
			private WaitCallback callBack;

			/// <summary>
			/// real state param that should be passed to the thread callback
			/// </summary>
			private object state;


			/// <summary>
			/// ctor
			/// </summary>
			/// <param name="callBack">real callback that should be called by the worker thread</param>
			/// <param name="state">real state param that should be passed to the thread callback</param>
			public WorkItemWrapper(WaitCallback callBack, object state)
			{
				this.callBack = callBack;
				this.state = state;
			}

			/// <summary>
			/// call the worker thread callback with the state
			/// </summary>
			public void Invoke()
			{
				try
				{
					this.callBack(this.state);
				}
				catch(Exception ex)
				{
					System.Diagnostics.Debug.Assert(false, ex.Message, ex.ToString());
					//catch the exception and log
					EventLog.Error(ex);
				}
			}
		}
	}
}
