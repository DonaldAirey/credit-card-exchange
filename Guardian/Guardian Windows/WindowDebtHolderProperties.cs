namespace FluidTrade.Guardian.Windows
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Windows;

	/// <summary>
	/// Properties for a debt holder.
	/// </summary>
	public class WindowDebtHolderProperties : WindowDebtClassProperties
	{

		/// <summary>
		/// Create a new properties window.
		/// </summary>
		public WindowDebtHolderProperties()
			: base()
		{

			this.Customize.BankAccountVisibility = Visibility.Visible;

		}

	}
}
