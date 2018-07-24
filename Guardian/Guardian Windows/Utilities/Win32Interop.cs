namespace FluidTrade.Guardian.Utilities
{

	using System;
	using System.Runtime.InteropServices;
	using System.Windows;
	using System.Windows.Interop;

	/// <summary>
	/// Native Win32 methods and wrappers.
	/// </summary>
	public static class Win32Interop
	{
		/// <summary>
		/// Call the native EnableWindow function on a window handle.
		/// </summary>
		/// <param name="hWnd">The window handle.</param>
		/// <param name="enable">True to enable the window, false to disable it.</param>
		/// <returns>If the window was previously disabled, true, otherwise false.</returns>
		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern bool EnableWindow(HandleRef hWnd, bool enable);

		/// <summary>
		/// Enable or disable input to a window.
		/// </summary>
		/// <param name="window">The window to affect.</param>
		/// <param name="enable">True to enable the window, false to disable it.</param>
		/// <returns>If the window was previously disabled, true, otherwise false.</returns>
		public static Boolean EnableWindow(Window window, Boolean enable)
		{

			WindowInteropHelper helper = new WindowInteropHelper(window);
			HandleRef handle = new HandleRef(window, helper.Handle);

			return Win32Interop.EnableWindow(handle, enable);

		}

		/// <summary>
		/// Enable input to a window.
		/// </summary>
		/// <param name="window">The window to affect.</param>
		/// <returns>If the window was previously disabled, true, otherwise false.</returns>
		public static Boolean EnableWindow(Window window)
		{

			return Win32Interop.EnableWindow(window, true);

		}

		/// <summary>
		/// Disable input to a window.
		/// </summary>
		/// <param name="window">The window to affect.</param>
		/// <returns>If the window was previously disabled, true, otherwise false.</returns>
		public static Boolean DisableWindow(Window window)
		{

			return Win32Interop.EnableWindow(window, false);

		}

	}

}
