namespace FluidTrade.Actipro
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using ActiproSoftware.Windows.Controls.Editors.Parts.Primitives;
	using System.Windows;
	using ActiproSoftware.Windows.Controls.Editors.Primitives;
	using ActiproSoftware.Windows.Controls.Editors;
	using System.Windows.Data;

	/// <summary>
	/// A type specific part for decimals.
	/// </summary>
	public class DecimalPart : TypeSpecificPartBase<Decimal>, ISpinnable
	{

		/// <summary>
		/// The Maximum dependency property.
		/// </summary>
		public static readonly DependencyProperty MaximumProperty;

		/// <summary>
		/// The Minimum dependency property.
		/// </summary>
		public static readonly DependencyProperty MinimumProperty;

		/// <summary>
		/// The IsReadOnly dependency property.
		/// </summary>
		public static readonly DependencyProperty IsReadOnlyProperty;

		/// <summary>
		/// The Step dependency property.
		/// </summary>
		public static readonly DependencyProperty StepProperty;

		/// <summary>
		/// The Format dependency property.
		/// </summary>
		public static readonly DependencyProperty StringFormatProperty;

		/// <summary>
		/// Create the static resources required for this class.
		/// </summary>
		static DecimalPart()
		{

			// This control will have a distinct style from the base class.
			DefaultStyleKeyProperty.OverrideMetadata(typeof(DecimalPart), new FrameworkPropertyMetadata(typeof(DecimalPart)));

			// The Format dependency property.
			DecimalPart.StringFormatProperty = DependencyProperty.Register(
				"StringFormat",
				typeof(String),
				typeof(DecimalPart),
				new PropertyMetadata("0"));

			DecimalPart.IsReadOnlyProperty = DependencyProperty.Register(
				"IsReadOnly",
				typeof(Boolean),
				typeof(DecimalPart),
				new PropertyMetadata(false));

			// The Maximum dependency property.
			DecimalPart.MaximumProperty = DependencyProperty.Register(
				"Maximum",
				typeof(Decimal),
				typeof(DecimalPart),
				new PropertyMetadata(Decimal.MaxValue, OnMaximumChanged));

			// The Minimum dependency property.
			DecimalPart.MinimumProperty = DependencyProperty.Register(
				"Minimum",
				typeof(Decimal),
				typeof(DecimalPart),
				new PropertyMetadata(Decimal.MinValue, OnMaximumChanged));

			// The Step dependency property.
			DecimalPart.StepProperty = DependencyProperty.Register(
				"Step",
				typeof(Decimal),
				typeof(DecimalPart),
				new PropertyMetadata(1.00m));

		}

		/// <summary>
		/// Create a new part for decimals.
		/// </summary>
		public DecimalPart()
		{

			// Initialize the object.
			this.Mask = @"((\d{1,3}(,\d\d\d)*)|\d+)(\.\d+)?|\.\d+";
			this.IsFocusMovedOnTerminalMatches = true;
			this.MoveFocusCharacters = " ";
			this.InitialValue = 0.00m;
			this.IsNullAllowed = false;

		}

		/// <summary>
		/// Gets a value indicating whether a value associated with this instance can be decremented.
		/// </summary>
		/// <value>
		/// <c>true</c> if a value associated with this instance can be decremented; otherwise, <c>false</c>.
		/// </value>
		public bool CanDecrement
		{
			get { return (SpinBehavior.NoWrap != this.SpinBehavior || this.DisplayValue > this.Minimum); }
		}

		/// <summary>
		/// Gets a value indicating whether a value associated with this instance can be incremented.
		/// </summary>
		/// <value>
		/// <c>true</c> if a value associated with this instance can be incremented; otherwise, <c>false</c>.
		/// </value>
		public bool CanIncrement
		{
			get { return (SpinBehavior.NoWrap != this.SpinBehavior || this.DisplayValue < this.Maximum); }
		}

		/// <summary>
		/// Gets or sets whether the text box is read only.
		/// </summary>
		public Boolean IsReadOnly
		{
			get { return (Boolean)this.GetValue(DecimalPart.IsReadOnlyProperty); }
			set { this.SetValue(DecimalPart.IsReadOnlyProperty, value); }
		}

		/// <summary>
		/// Gets or sets the maximum value of the text box.
		/// </summary>
		public Decimal Maximum
		{
			get { return (Decimal)this.GetValue(DecimalPart.MaximumProperty); }
			set { this.SetValue(DecimalPart.MaximumProperty, value); }
		}

		/// <summary>
		/// Gets or sets the minimum value of the text box.
		/// </summary>
		public Decimal Minimum
		{
			get { return (Decimal)this.GetValue(DecimalPart.MinimumProperty); }
			set { this.SetValue(DecimalPart.MinimumProperty, value); }
		}

		/// <summary>
		/// Gets or sets the spinner step value of the text box.
		/// </summary>
		public Decimal Step
		{
			get { return (Decimal)this.GetValue(DecimalPart.StepProperty); }
			set { this.SetValue(DecimalPart.StepProperty, value); }
		}

		/// <summary>
		/// Gets or sets string format for the value displayed in the text box.
		/// </summary>
		public String StringFormat
		{
			get { return (String)this.GetValue(DecimalPart.StringFormatProperty); }
			set { this.SetValue(DecimalPart.StringFormatProperty, value); }
		}

		/// <summary>
		/// Get the string representation of a value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The string represenation of the value.</returns>
		protected override string GetString(decimal value)
		{

			// Present the value according to the 'Format' property.
			return value.ToString(this.StringFormat);

		}

		/// <summary>
		/// Make sure the value is within our constraints.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		protected override decimal CoerceValue(decimal value)
		{

			// This insures that the value never exeeds the maximum or minimum.
			return value > this.Maximum ? this.Maximum : value < this.Minimum ? this.Minimum : value;

		}

		/// <summary>
		/// Hook up to parts of the template once it's been applied.
		/// </summary>
		public override void OnApplyTemplate()
		{

			base.OnApplyTemplate();

			if (this.MaskedTextBox != null)
				this.MaskedTextBox.SetBinding(MaskedTextBox.IsReadOnlyProperty, new Binding("IsReadOnly") { Source = this });


		}

		/// <summary>
		/// Handle the maximum value changing.
		/// </summary>
		/// <param name="sender">The decimal part whose maximum value changed.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private static void OnMaximumChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			// Clip the value at the new maximum.
			DecimalPart part = sender as DecimalPart;
			if (part.Value > part.Maximum)
				part.Value = part.Maximum;

		}

		/// <summary>
		/// Handle the minimum value changing.
		/// </summary>
		/// <param name="sender">The decimal part whose minimum value changed.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private static void OnMinimumChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			// Clip the value at the new minimum.
			DecimalPart part = sender as DecimalPart;
			if (part.Value < part.Minimum)
				part.Value = part.Minimum;

		}

		/// <summary>
		/// Handles a change made to the value of the control.
		/// </summary>
		/// <param name="oldValue"></param>
		/// <param name="newValue"></param>
		protected override void OnValueChanged(decimal oldValue, decimal newValue)
		{

			// Allow the base class to process the changed value.
			base.OnValueChanged(oldValue, newValue);

			// Adjust the value displayed in the control.
			this.DisplayValue = newValue;

			// Commit the changes to the display.
			this.CommitChangesToValue(PartValueCommitTriggers.StringValueChange);

		}

		/// <summary>
		/// Attempt to parse a decimal value.
		/// </summary>
		/// <param name="stringValue">The string name.</param>
		/// <param name="value">The decimal value.</param>
		/// <returns>True if the value was successfully parsed.</returns>
		protected override bool TryGetEffectiveValue(string stringValue, out decimal value)
		{

			// Parse the text into a decimal value.
			return Decimal.TryParse(stringValue, out value);

		}

		/// <summary>
		/// Decrements the Value by the Step property.
		/// </summary>
		public void Decrement()
		{

			// Decrease the Value property by the value of the Step property.
			if (this.CanDecrement)
				this.DisplayValue = this.DisplayValue -= this.Step;

			// Commit the changes to the value.
			this.CommitChangesToValue(PartValueCommitTriggers.SpinnerChange);

		}

		/// <summary>
		/// Increments a value associated with this instance.
		/// </summary>
		public void Increment()
		{

			// Increase the Value property by the value of the Step property.
			if (this.CanIncrement)
				this.DisplayValue = this.DisplayValue += this.Step;

			// Commit the changes to the value.
			this.CommitChangesToValue(PartValueCommitTriggers.SpinnerChange);

		}

	}

}
