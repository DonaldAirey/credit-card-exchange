namespace FluidTrade.Actipro
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using ActiproSoftware.Windows.Controls.Editors;
	using ActiproSoftware.Windows.Controls.Editors.Parts;
	using ActiproSoftware.Windows.Controls.Editors.Parts.Primitives;
	using ActiproSoftware.Windows.Controls.Editors.Primitives;
	using System.Windows.Data;
	using System.Windows;

	/// <summary>
	/// Part group for decimal parts.
	/// </summary>
	public class DecimalPartGroup : TypeSpecificPartGroupBase<Decimal>
	{

		/// <summary>
		/// The IsReadOnly dependency property.
		/// </summary>
		public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
			"IsReadOnly",
			typeof(Boolean),
			typeof(DecimalPartGroup),
			new PropertyMetadata(false));

		/// <summary>
		/// The Maximum dependency property.
		/// </summary>
		public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
			"Maximum",
			typeof(Decimal),
			typeof(DecimalPartGroup),
			new PropertyMetadata(Decimal.MaxValue, OnMaximumChanged));

		/// <summary>
		/// The Minimum dependency property.
		/// </summary>
		public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
			"Minimum",
			typeof(Decimal),
			typeof(DecimalPartGroup),
			new PropertyMetadata(Decimal.MinValue, OnMinimumChanged));

		/// <summary>
		/// Indicates the Step dependency property.
		/// </summary>
		public static readonly DependencyProperty StepProperty = DependencyProperty.Register(
			"Step",
			typeof(Decimal),
			typeof(DecimalPartGroup),
			new PropertyMetadata(1.00m));

		/// <summary>
		/// The Format dependency property.
		/// </summary>
		public static readonly DependencyProperty StringFormatProperty = DependencyProperty.Register(
			"StringFormat",
			typeof(String),
			typeof(DecimalPartGroup),
			new PropertyMetadata("0"));

		/// <summary>
		/// Create a new decimal part group.
		/// </summary>
		public DecimalPartGroup()
		{

			// Forces the tab to stop on this section of the control.
			this.IsTabStop = true;

		}

		/// <summary>
		/// Gets or sets whether the text box is read only.
		/// </summary>
		public Boolean IsReadOnly
		{
			get { return (Boolean)this.GetValue(DecimalPartGroup.IsReadOnlyProperty); }
			set { this.SetValue(DecimalPartGroup.IsReadOnlyProperty, value); }
		}

		/// <summary>
		/// Gets or sets the maximum value of the text box.
		/// </summary>
		public Decimal Maximum
		{
			get { return (Decimal)this.GetValue(DecimalPartGroup.MaximumProperty); }
			set { this.SetValue(DecimalPartGroup.MaximumProperty, value); }
		}

		/// <summary>
		/// Gets or sets the minimum value of the text box.
		/// </summary>
		public Decimal Minimum
		{
			get { return (Decimal)this.GetValue(DecimalPartGroup.MinimumProperty); }
			set { this.SetValue(DecimalPartGroup.MinimumProperty, value); }
		}

		/// <summary>
		/// Gets or sets the spinner step value of the text box.
		/// </summary>
		public Decimal Step
		{
			get { return (Decimal)this.GetValue(DecimalPartGroup.StepProperty); }
			set { this.SetValue(DecimalPartGroup.StepProperty, value); }
		}

		/// <summary>
		/// Gets or sets the string used to format the value in the control.
		/// </summary>
		public String StringFormat
		{
			get { return (String)this.GetValue(DecimalPartGroup.StringFormatProperty); }
			set { this.SetValue(DecimalPartGroup.StringFormatProperty, value); }
		}

		/// <summary>
		/// Converts the specified string to an instance of Decimal.
		/// </summary>
		/// <param name="text">The text.</param>
		/// <returns>An instance of Decimal.</returns>
		protected override Decimal ConvertFromString(string text)
		{

			// Extract the value from the text of that value.
			return Decimal.Parse(text);

		}

		/// <summary>
		/// Converts the specified value to a string representation.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>A string representation of the specified value.</returns>
		protected override string ConvertToString(Decimal value)
		{

			// Display the value in the requested format.
			return value.ToString(this.StringFormat);

		}

		/// <summary>
		/// Handle the maximum value changing.
		/// </summary>
		/// <param name="sender">The decimal part whose maximum value changed.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private static void OnMaximumChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			// Clip the value at the maxmum.
			DecimalPartGroup decimalPartGroup = sender as DecimalPartGroup;
			if (decimalPartGroup.Value > decimalPartGroup.Maximum)
				decimalPartGroup.Value = decimalPartGroup.Maximum;

		}

		/// <summary>
		/// Handle the minimum value changing.
		/// </summary>
		/// <param name="sender">The decimal part whose minimum value changed.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private static void OnMinimumChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			// Clip the value at the minimum.
			DecimalPartGroup part = sender as DecimalPartGroup;
			if (part.Value < part.Minimum)
				part.Value = part.Minimum;

		}

		/// <summary>
		/// Generate default parts.
		/// </summary>
		/// <param name="defaultItems"></param>
		protected override void GenerateDefaultItems(ActiproSoftware.Windows.Controls.Editors.Primitives.SlottedItemCollection defaultItems)
		{

			// Allow the base class to set its defaults.
			base.GenerateDefaultItems(defaultItems);

			// This group will add a new set of defaults for the decimal part of the edit box.
			DecimalPart decimalPart = new DecimalPart();
			decimalPart.SetBinding(DecimalPart.StringFormatProperty, new Binding("StringFormat") { Source = this });
			decimalPart.SetBinding(DecimalPart.IsReadOnlyProperty, new Binding("IsReadOnly") { Source = this });
			decimalPart.SetBinding(DecimalPart.MaximumProperty, new Binding("Maximum") { Source = this });
			decimalPart.SetBinding(DecimalPart.MinimumProperty, new Binding("Minimum") { Source = this });
			decimalPart.SetBinding(DecimalPart.StepProperty, new Binding("Step") { Source = this, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
			decimalPart.SetBinding(DecimalPart.ValueProperty, new Binding("Value") { Source = this, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
			SyncBinding.CreateBinding(this, DecimalPartGroup.InitialValueProperty, decimalPart, DecimalPart.InitialValueProperty);
			SyncBinding.CreateBinding(
					this,
					DecimalPartGroup.PartValueCommitTriggersProperty,
					decimalPart,
					DecimalPart.PartValueCommitTriggersProperty,
					UpdateSourceTrigger.Default);
			decimalPart.IsFocusMovedOnTerminalMatches = true;
			decimalPart.IsTabStop = true;
			defaultItems.Add(decimalPart);

		}

	}

}
