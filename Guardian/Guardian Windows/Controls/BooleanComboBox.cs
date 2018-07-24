namespace FluidTrade.Guardian.Windows.Controls
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Windows;
	using System.Windows.Controls;

	/// <summary>
	/// A drop down with values of the Status enum.
	/// </summary>
	public class BooleanComboBox : ComboBox
	{

		private class BooleanItem
		{

			public Boolean Value { get; set; }

			public override string ToString()
			{

				return this.Value ? "True" : "False";

			}

		}

		static BooleanComboBox()
		{

			BooleanComboBox.SelectedValuePathProperty.OverrideMetadata(typeof(BooleanComboBox), new FrameworkPropertyMetadata("Value"));

		}

		/// <summary>
		/// Create a new Status combo box.
		/// </summary>
		public BooleanComboBox()
		{

			this.ItemsSource = new List<BooleanItem>() { new BooleanItem() { Value = true }, new BooleanItem() { Value = false } };

		}

	}

}
