namespace FluidTrade.Thirdparty
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using ActiproSoftware.Windows.Controls.Editors.Parts.Primitives;
	using System.Windows;
	using ActiproSoftware.Windows.Controls.Editors.Primitives;
	using ActiproSoftware.Windows.Controls.Editors;

	/// <summary>
	/// A type specific part for decimals.
	/// </summary>
	public class DecimalPart : TypeSpecificPartBase<Decimal>, ISpinnable
	{

		/// <summary>
		/// Indicates the Maximum dependency property.
		/// </summary>
		public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register("Maximum", typeof(Decimal), typeof(DecimalPart), new PropertyMetadata(Decimal.MaxValue, OnMaximumChanged));
		/// <summary>
		/// Indicates the Minimum dependency property.
		/// </summary>
		public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register("Minimum", typeof(Decimal), typeof(DecimalPart), new PropertyMetadata(Decimal.MinValue, OnMaximumChanged));
		/// <summary>
		/// Indicates the Step dependency property.
		/// </summary>
		public static readonly DependencyProperty StepProperty = DependencyProperty.Register("Step", typeof(Decimal), typeof(DecimalPart), new PropertyMetadata(1m));

		static DecimalPart()
		{

			DefaultStyleKeyProperty.OverrideMetadata(typeof(DecimalPart), new FrameworkPropertyMetadata(typeof(DecimalPart)));

		}

		/// <summary>
		/// Create a new part for decimals.
		/// </summary>
		public DecimalPart()
		{

			this.Mask = @"((\d{1,3}(,\d\d\d)*)|\d+)(\.\d+)?|\.\d+";
			this.IsFocusMovedOnTerminalMatches = true;
			this.MoveFocusCharacters = " ";
			this.InitialValue = 0m;
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

			get
			{
				return (SpinBehavior.NoWrap != this.SpinBehavior || this.DisplayValue > this.Minimum);
	
			}

		}

		/// <summary>
		/// Gets a value indicating whether a value associated with this instance can be incremented.
		/// </summary>
		/// <value>
		/// <c>true</c> if a value associated with this instance can be incremented; otherwise, <c>false</c>.
		/// </value>
		public bool CanIncrement
		{

			get
			{
			
				return (SpinBehavior.NoWrap != this.SpinBehavior || this.DisplayValue < this.Maximum);
	
			}

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
		/// Get the string representation of a value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The string represenation of the value.</returns>
		protected override string GetString(decimal value)
		{

			return value.ToString();

		}

		/// <summary>
		/// Make sure the value is within our constraints.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		protected override decimal CoerceValue(decimal value)
		{

			if (value > this.Maximum)
				value = this.Maximum;
			else if (value < this.Minimum)
				value = this.Minimum;
			
			return value;

		}

		/// <summary>
		/// Handle the maximum value changing.
		/// </summary>
		/// <param name="sender">The decimal part whose maximum value changed.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private static void OnMaximumChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
		{

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

			DecimalPart part = sender as DecimalPart;

			if (part.Value < part.Minimum)
				part.Value = part.Minimum;

		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="oldValue"></param>
		/// <param name="newValue"></param>
		protected override void OnValueChanged(decimal oldValue, decimal newValue)
		{

			base.OnValueChanged(oldValue, newValue);
			this.DisplayValue = newValue;
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

			return Decimal.TryParse(stringValue, out value);

		}

		/// <summary>
		/// Decrements a value associated with this instance.
		/// </summary>
		public void Decrement()
		{

			if (this.CanDecrement)
				this.DisplayValue = this.DisplayValue -= this.Step;

			this.CommitChangesToValue(PartValueCommitTriggers.SpinnerChange);

		}

		/// <summary>
		/// Increments a value associated with this instance.
		/// </summary>
		public void Increment()
		{

			if (this.CanIncrement)
				this.DisplayValue = this.DisplayValue += this.Step;

			this.CommitChangesToValue(PartValueCommitTriggers.SpinnerChange);

		}

	}

}
