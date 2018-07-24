namespace FluidTrade.Thirdparty
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Windows.Data;
	using ActiproSoftware.Windows.Controls.Editors.Primitives;
	using ActiproSoftware.Windows.Controls.Editors.Parts;
	using ActiproSoftware.Windows.Controls.Editors.Parts.Primitives;
	using System.Windows;

	/// <summary>
	/// A text box for editing percentages.
	/// </summary>
	public class PercentageTextBox : TypeSpecificEditBoxBase<Decimal>
	{

		private class PercentageStringConverter : IValueConverter
		{

			/// <summary>
			/// Convert to percentage
			/// </summary>
			/// <param name="value"></param>
			/// <param name="targetType"></param>
			/// <param name="parameter"></param>
			/// <param name="culture"></param>
			/// <returns></returns>
			public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
			{

				Decimal valueToConvert = (Decimal)value;
				return valueToConvert / 100m;
	
			}

			/// <summary>
			/// Convert to percentage
			/// </summary>
			/// <param name="value"></param>
			/// <param name="targetType"></param>
			/// <param name="parameter"></param>
			/// <param name="culture"></param>
			/// <returns></returns>
			public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
			{

				Decimal valueToConvert = (Decimal)value;
				return valueToConvert * 100m;

			}

		}

		/// <summary>
		/// Indicates the Maximum dependency property.
		/// </summary>
		public static readonly DependencyProperty MaximumProperty =
			DependencyProperty.Register("Maximum", typeof(Decimal), typeof(PercentageTextBox), new PropertyMetadata(1m));
		/// <summary>
		/// Indicates the Minimum dependency property.
		/// </summary>
		public static readonly DependencyProperty MinimumProperty =
			DependencyProperty.Register("Minimum", typeof(Decimal), typeof(PercentageTextBox), new PropertyMetadata(0m));
		/// <summary>
		/// Indicates the IsReadOnly dependency property.
		/// </summary>
		public static readonly DependencyProperty IsReadOnlyProperty =
			DependencyProperty.Register("IsReadOnly", typeof(Boolean), typeof(PercentageTextBox), new PropertyMetadata(false));
		/// <summary>
		/// Indicates the Step dependency property.
		/// </summary>
		public static readonly DependencyProperty StepProperty = DependencyProperty.Register("Step", typeof(Decimal), typeof(PercentageTextBox), new PropertyMetadata(.01m));

		static PercentageTextBox()
		{

			DefaultStyleKeyProperty.OverrideMetadata(typeof(PercentageTextBox), new FrameworkPropertyMetadata(typeof(PercentageTextBox)));
		}

		/// <summary>
		/// Gets or sets whether the text box is read only.
		/// </summary>
		public Boolean IsReadOnly
		{
			get { return (Boolean)this.GetValue(PercentageTextBox.IsReadOnlyProperty); }
			set { this.SetValue(DecimalPartGroup.IsReadOnlyProperty, value); }
		}

		/// <summary>
		/// Gets or sets the maximum value of the text box.
		/// </summary>
		public Decimal Maximum
		{
			get { return (Decimal)this.GetValue(PercentageTextBox.MaximumProperty); }
			set { this.SetValue(PercentageTextBox.MaximumProperty, value); }
		}

		/// <summary>
		/// Gets or sets the minimum value of the text box.
		/// </summary>
		public Decimal Minimum
		{
			get { return (Decimal)this.GetValue(PercentageTextBox.MinimumProperty); }
			set { this.SetValue(PercentageTextBox.MinimumProperty, value); }
		}

		/// <summary>
		/// Gets or sets the spinner step value of the text box.
		/// </summary>
		public Decimal Step
		{
			get { return (Decimal)this.GetValue(PercentageTextBox.StepProperty); }
			set { this.SetValue(PercentageTextBox.StepProperty, value); }
		}

		/// <summary>
		/// Converts the specified string to an instance of Decimal.
		/// </summary>
		/// <param name="text">The text.</param>
		/// <returns>An instance of Decimal.</returns>
		protected override Decimal ConvertFromString(string text)
		{

			return Decimal.Parse(text);

		}

		/// <summary>
		/// Handle the value changing.
		/// </summary>
		/// <param name="e">The event arguments.</param>
		protected override void OnValueChanged(ActiproSoftware.Windows.PropertyChangedRoutedEventArgs<decimal> e)
		{
			base.OnValueChanged(e);
		}

		/// <summary>
		/// For reasons that I can't figure out just yet, we need to coerce the value at every level or it doesn't always get coerced.
		/// </summary>
		/// <param name="value">The new value.</param>
		/// <returns>The corrected new value.</returns>
		protected override decimal CoerceValue(decimal value)
		{

			if (value > this.Maximum)
				value = this.Maximum;
			else if (value < this.Minimum)
				value = this.Minimum;

			return value;

		}

		/// <summary>
		/// Converts the specified value to a string representation.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>A string representation of the specified value.</returns>
		protected override string ConvertToString(Decimal value)
		{

			return value.ToString();

		}

		/// <summary>
		/// Generates a list of objects to be used in the DefaultItems collection.
		/// </summary>
		/// <param name="defaultItems">The collection that should be updated.</param>
		protected override void GenerateDefaultItems(SlottedItemCollection defaultItems)
		{

			base.GenerateDefaultItems(defaultItems);
			DecimalPartGroup decimalGroup = new DecimalPartGroup();

			decimalGroup.Style = this.TryFindResource("percentagePartGroup") as Style;
			decimalGroup.SetBinding(DecimalPartGroup.IsReadOnlyProperty, new Binding("IsReadOnly") { Source = this });
			decimalGroup.SetBinding(
					DecimalPartGroup.MaximumProperty,
					new Binding("Maximum") { Source = this, Converter = new PercentageConverter(), UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
			decimalGroup.SetBinding(
					DecimalPartGroup.MinimumProperty,
					new Binding("Minimum") { Source = this, Converter = new PercentageConverter(), UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
			decimalGroup.SetBinding(
					DecimalPartGroup.StepProperty,
					new Binding("Step") { Source = this, Converter = new PercentageConverter(), UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
			decimalGroup.SetBinding(
					DecimalPartGroup.ValueProperty,
					new Binding("Value") { Source = this, Converter = new PercentageConverter(), UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
			SyncBinding.CreateBinding(
					this,
					PercentageTextBox.PartValueCommitTriggersProperty,
					decimalGroup,
					DecimalPartGroup.PartValueCommitTriggersProperty,
					UpdateSourceTrigger.Default);

			defaultItems.Add(decimalGroup);
			defaultItems.Add(new TextBlockPartGroup() { Text = "%" });

		}

	}

}
