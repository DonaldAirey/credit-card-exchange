using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.IO;
using Microsoft.Win32;

namespace SetupWatchDog
{
	static class Program
	{
		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

		[DllImport("user32.dll", SetLastError = true)]
		static extern bool BringWindowToTop(IntPtr hWnd);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, SetWindowPosFlags uFlags);

		private const int HWND_TOP = 0;
		private const int HWND_BOTTOM = 1;
		private const int HWND_TOPMOST = -1;
		private const int HWND_NOTOPMOST = -2;

		private enum SetWindowPosFlags : uint
		{
			/// <summary>If the calling thread and the thread that owns the window are attached to different input queues, 
			/// the system posts the request to the thread that owns the window. This prevents the calling thread from 
			/// blocking its execution while other threads process the request.</summary>
			/// <remarks>SWP_ASYNCWINDOWPOS</remarks>
			SynchronousWindowPosition = 0x4000,
			/// <summary>Prevents generation of the WM_SYNCPAINT message.</summary>
			/// <remarks>SWP_DEFERERASE</remarks>
			DeferErase = 0x2000,
			/// <summary>Draws a frame (defined in the window's class description) around the window.</summary>
			/// <remarks>SWP_DRAWFRAME</remarks>
			DrawFrame = 0x0020,
			/// <summary>Applies new frame styles set using the SetWindowLong function. Sends a WM_NCCALCSIZE message to 
			/// the window, even if the window's size is not being changed. If this flag is not specified, WM_NCCALCSIZE 
			/// is sent only when the window's size is being changed.</summary>
			/// <remarks>SWP_FRAMECHANGED</remarks>
			FrameChanged = 0x0020,
			/// <summary>Hides the window.</summary>
			/// <remarks>SWP_HIDEWINDOW</remarks>
			HideWindow = 0x0080,
			/// <summary>Does not activate the window. If this flag is not set, the window is activated and moved to the 
			/// top of either the topmost or non-topmost group (depending on the setting of the hWndInsertAfter 
			/// parameter).</summary>
			/// <remarks>SWP_NOACTIVATE</remarks>
			DoNotActivate = 0x0010,
			/// <summary>Discards the entire contents of the client area. If this flag is not specified, the valid 
			/// contents of the client area are saved and copied back into the client area after the window is sized or 
			/// repositioned.</summary>
			/// <remarks>SWP_NOCOPYBITS</remarks>
			DoNotCopyBits = 0x0100,
			/// <summary>Retains the current position (ignores X and Y parameters).</summary>
			/// <remarks>SWP_NOMOVE</remarks>
			IgnoreMove = 0x0002,
			/// <summary>Does not change the owner window's position in the Z order.</summary>
			/// <remarks>SWP_NOOWNERZORDER</remarks>
			DoNotChangeOwnerZOrder = 0x0200,
			/// <summary>Does not redraw changes. If this flag is set, no repainting of any kind occurs. This applies to 
			/// the client area, the nonclient area (including the title bar and scroll bars), and any part of the parent 
			/// window uncovered as a result of the window being moved. When this flag is set, the application must 
			/// explicitly invalidate or redraw any parts of the window and parent window that need redrawing.</summary>
			/// <remarks>SWP_NOREDRAW</remarks>
			DoNotRedraw = 0x0008,
			/// <summary>Same as the SWP_NOOWNERZORDER flag.</summary>
			/// <remarks>SWP_NOREPOSITION</remarks>
			DoNotReposition = 0x0200,
			/// <summary>Prevents the window from receiving the WM_WINDOWPOSCHANGING message.</summary>
			/// <remarks>SWP_NOSENDCHANGING</remarks>
			DoNotSendChangingEvent = 0x0400,
			/// <summary>Retains the current size (ignores the cx and cy parameters).</summary>
			/// <remarks>SWP_NOSIZE</remarks>
			IgnoreResize = 0x0001,
			/// <summary>Retains the current Z order (ignores the hWndInsertAfter parameter).</summary>
			/// <remarks>SWP_NOZORDER</remarks>
			IgnoreZOrder = 0x0004,
			/// <summary>Displays the window.</summary>
			/// <remarks>SWP_SHOWWINDOW</remarks>
			ShowWindow = 0x0040,
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{			

			//Trace.Assert(false);
			Dictionary<string, string> commandLineMap = new Dictionary<string, string>();
			
			// /Wrapper:True (defualt is false) if true this will just start another process to do the work and exit. This is so 
			//					installer can continue since it waits for process to finish
			// /Window:Name	 Title of window that to pop to top (default =  "CrystalReports Setup")
			// /Class:Name	 Title of window that to pop to top (default = "#32770")
			// /Timeout:20	 number of seconds to watch for window to pop to top (defualt 20)
			// /EVENTSOURCE:event source name of the event source
			// //EVENTLOG:DebTrak Log name
			
			//check for crystal reports
			
			

			string[] args = Environment.GetCommandLineArgs();
			for(int i = 1; i < args.Length; i++)
			{
				string arg = args[i];
				string[] parts = arg.Split(new char[]{'/',':'}, StringSplitOptions.RemoveEmptyEntries);

				string val = "";
				if (parts.Length > 1)
					val = parts[1];

				commandLineMap[parts[0].ToUpperInvariant()] = val;
			}

			string isWrapperStr = "false";
			string windowTitle = "CrystalReports Setup";
			string windowClass = "#32770";
			string timeoutStr = "20";
			string eventSource = String.Empty;
			string eventLog = String.Empty;

			if (commandLineMap.ContainsKey("WRAPPER"))
				isWrapperStr = commandLineMap["WRAPPER"];

			if (commandLineMap.ContainsKey("WINDOW"))
				windowTitle = commandLineMap["WINDOW"];

			if (commandLineMap.ContainsKey("CLASS"))
				windowClass = commandLineMap["CLASS"];

			if (commandLineMap.ContainsKey("TIMEOUT"))
				timeoutStr = commandLineMap["TIMEOUT"];

			if (commandLineMap.ContainsKey("EVENTSOURCE"))
				eventSource = commandLineMap["EVENTSOURCE"];

			if (commandLineMap.ContainsKey("EVENTLOG"))
				eventLog = commandLineMap["EVENTLOG"];

			CreateEventSource(eventSource, eventLog);

			System.Reflection.Assembly testAssembly = null;
			try
			{
				const String crystalAssebly = "CrystalDecisions.CrystalReports.Engine, Version=10.5.3700.0, Culture=neutral, PublicKeyToken=692fbea5521e1304";
				testAssembly = System.Reflection.Assembly.Load(crystalAssebly);
			}
			catch
			{
			}

			if (testAssembly != null)
				return;
			
			bool isWrapper;
			if (bool.TryParse(isWrapperStr, out isWrapper) == false)
				isWrapper = false;

			//start a new watchdog and return from this one so that the
			//installer script can continue
			if (isWrapper == true)
			{
				Process process = new Process();
				StringBuilder newArgs = new StringBuilder();
				foreach (KeyValuePair<string, string> pair in commandLineMap)
				{
					if (pair.Key == "WRAPPER")
						continue;

					newArgs.Append("/");
					newArgs.Append(pair.Key);
					newArgs.Append(":");
					newArgs.Append(pair.Value);
				}
				process.StartInfo = new ProcessStartInfo(args[0], newArgs.ToString());
				process.StartInfo.CreateNoWindow = true;

				process.Start();

				return;
			}

			Thread.Sleep(5000);
			int timeOutSec;
			if (int.TryParse(timeoutStr, out timeOutSec) == false)
			{
				timeOutSec = 20;
			}
			
			int noWindowCount = 0;
			int start = Environment.TickCount;
			while (Environment.TickCount - start < (timeOutSec * 1000))
			{
				// "MsiDialogCloseClass" is the dialog class for the installer
				//not looking for a window title because that could be different

				//need to wait for installers to close
				//#32770
				IntPtr handle = FindWindowEx(IntPtr.Zero, IntPtr.Zero, "MsiDialogCloseClass", null);

				if (handle == IntPtr.Zero)
				{
					//wait for 2x incase inbetween windows
					noWindowCount++;
					if (noWindowCount > 1)
						break;
				}
			}

			Process setupProcess = new Process();

			//get the path to the crystal setup.exe
			string fileName = Path.GetDirectoryName(args[0]);
			fileName = Path.Combine(fileName, "setup.exe");

			setupProcess.StartInfo = new ProcessStartInfo(fileName);
			setupProcess.StartInfo.CreateNoWindow = true;

			//start the setup program
			setupProcess.Start();


			//when the crystal setup opens pop it to the top
			start = Environment.TickCount;

			while (Environment.TickCount - start < (timeOutSec * 1000))
			{
				//#32770
				IntPtr handle = FindWindowEx(IntPtr.Zero, IntPtr.Zero, windowClass, windowTitle);

				if (handle != IntPtr.Zero)
				{
					SetWindowPos(handle, (IntPtr)HWND_TOPMOST, 0, 0, 0, 0,
					SetWindowPosFlags.SynchronousWindowPosition |
					SetWindowPosFlags.IgnoreMove |
					SetWindowPosFlags.IgnoreResize);

					break;
				}

				Thread.Sleep(1000);
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="eventSource"></param>
		/// <param name="eventLog"></param>
		private static void CreateEventSource(string eventSource, string eventLogName)
		{

			if (String.IsNullOrEmpty(eventSource) == false &&
				String.IsNullOrEmpty(eventLogName) == false)
			{
				try
				{
					if (!(EventLog.SourceExists(eventSource)))
					{
						EventLog.CreateEventSource(eventSource, eventLogName);						
						using (EventLog eventLog = new EventLog(eventLogName))
						{
							//Set it to 1 GB
							eventLog.MaximumKilobytes = 1048576;
							eventLog.ModifyOverflowPolicy(OverflowAction.OverwriteAsNeeded, 0);
						}
					}

					EventLog.WriteEntry(eventSource, "Setup Complete", EventLogEntryType.Information);
				}
				catch
				{

				}
			}
		}
	}
}
