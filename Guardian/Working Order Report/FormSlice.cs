namespace FluidTrade.Guardian
{

    using System;
	using System.Collections.Generic;
    using System.Windows.Forms;
    using FluidTrade.Guardian;

	public enum Unit { Shares, Percent }

	/// <summary>
	/// Used to acquire parameters for slicing working orders into destination orders.
	/// </summary>
	public partial class FormSlice : Form
	{

		/// <summary>
		/// The units of the desired slicing method (shares, percent, etc.)
		/// </summary>
		public Unit Unit;

		/// <summary>
		/// The value used to slice an order.
		/// </summary>
		public System.Decimal Value;

		/// <summary>
		/// Used to manage the 'units' combo box which allows the user to select shares or quantity.
		/// </summary>
		private class UnitItem
		{

			// Private Fields
			private Unit unit;
			private System.String name;

			/// <summary>
			/// Used to manage the 'units' combo box which allows the user to select shares or quantity.
			/// </summary>
			/// <param name="unit">The units (shares, percent, etc).</param>
			/// <param name="text">The name of the unit.</param>
			public UnitItem(Unit unit, string text) { this.unit = unit; this.name = text; }

			/// <summary>
			/// The units (percent, shares, etc.).
			/// </summary>
			public Unit Unit { get { return this.unit; } }

			/// <summary>
			/// The name of the unit.
			/// </summary>
			public string Name { get { return this.name; } }

		}

		/// <summary>
		/// Used to manage the 'units' combo box which allows the user to select shares or quantity.
		/// </summary>
		private class DestinationItem
		{

			// Public Fields
			public System.Guid destintationId;
			public System.String name;
			public System.String shortName;

			/// <summary>
			/// Used to manage the 'units' combo box which allows the user to select shares or quantity.
			/// </summary>
			/// <param name="unit"></param>
			/// <param name="text"></param>
			public DestinationItem(Guid destinationId, string name, string shortName)
			{

				// Initialize the object.
				this.destintationId = destinationId;
				this.name = name;
				this.shortName = shortName;
			
			}

			public Guid DestinationId { get { return this.destintationId; } }
			
			public string Name { get { return this.name; } }

			public string ShortName { get { return this.shortName; } }

		}

		/// <summary>
		/// Create a new form to qcquire parameters from the user for slicing orders into destination orders.
		/// </summary>
		public FormSlice()
		{

			// The IDE supported resources are initialized here.
			InitializeComponent();

			// Initialize the units combo box.
			List<UnitItem> unitItems = new List<UnitItem>();
			unitItems.Add(new UnitItem(Unit.Shares, Resource.SharesString));
			unitItems.Add(new UnitItem(Unit.Percent, Resource.PercentString));
			this.comboBoxUnit.DataSource = unitItems;
			this.comboBoxUnit.DisplayMember = "Name";
			this.comboBoxUnit.ValueMember = "Unit";
			this.comboBoxUnit.SelectedValue = Unit.Shares;

		}

		/// <summary>
		/// Accepts the contents of the form applies it to the document form.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
		private void buttonOK_Click(object sender, EventArgs e)
		{

			// Extract the units used to slice orders from the dialog box.
			UnitItem unitItem = this.comboBoxUnit.SelectedItem as UnitItem;
			this.Unit = unitItem.Unit;

			// Extract the value which will either be a percent of the order or a fixed number of shares depending on the units
			// selected
			switch (this.Unit)
			{
			case Unit.Percent:

				this.Value = Convert.ToDecimal(textBoxValue.Text) / 100.0m;
				break;

			case Unit.Shares:

				this.Value = Convert.ToDecimal(textBoxValue.Text);
				break;

			}

			// Set the return code for the dialog and exit.
			this.DialogResult = DialogResult.OK;
			this.Close();

		}

	}

}