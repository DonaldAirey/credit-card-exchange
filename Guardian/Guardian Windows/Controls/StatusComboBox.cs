namespace FluidTrade.Guardian.Windows.Controls
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Windows;

	/// <summary>
	/// A drop-down with statuses in it.
	/// </summary>
	public class StatusComboBox : PersistentComboBox
	{

		static StatusComboBox()
		{

			StatusComboBox.SelectedValuePathProperty.OverrideMetadata(typeof(StatusComboBox), new FrameworkPropertyMetadata("StatusId"));

		}

		/// <summary>
		/// Create a new Status combo box.
		/// </summary>
		public StatusComboBox()
		{

			this.DisplayMemberPath = "Mnemonic";
			this.ItemsSource = StatusList.Default;

		}

	}

}
