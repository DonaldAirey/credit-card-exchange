namespace FluidTrade.Thirdparty
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using ActiproSoftware.Windows.Controls.Editors.Primitives;
	using ActiproSoftware.Windows;
	using System.Windows;
	using ActiproSoftware.Windows.Controls.Editors;
	using ActiproSoftware.Windows.Controls.Editors.Parts;
	using System.Globalization;
	using System.Windows.Data;
	using System.Windows.Input;

	/// <summary>
	/// A text box for editing currency amounts.
	/// </summary>
	public class MoneyTextBox : MaskedTextBox
	{

		/// <summary>
		/// Indicates the Value dependency property.
		/// </summary>
		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
			"Value",
			typeof(Decimal),
			typeof(MoneyTextBox),
			new FrameworkPropertyMetadata(0m, MoneyTextBox.OnValueChanged));

		/// <summary>
		/// The event raised when Value changes.
		/// </summary>
		public event DependencyPropertyChangedEventHandler ValueChanged;

		static MoneyTextBox()
		{

			DefaultStyleKeyProperty.OverrideMetadata(typeof(MoneyTextBox), new FrameworkPropertyMetadata(typeof(MoneyTextBox)));

		}

		/// <summary>
		/// Create a new MoneyTextBox.
		/// </summary>
		public MoneyTextBox()
		{

			this.MaskType = MaskType.Regex;
			this.Mask = CultureInfo.CurrentUICulture.NumberFormat.CurrencySymbol + @"((\d{1,3}(,\d\d\d)*)|\d*)\.\d\d";
			this.PromptIndicatorType = IndicatorType.Geometry;
			this.PromptIndicatorVisibility = IndicatorVisibility.Always;
			this.HorizontalContentAlignment = HorizontalAlignment.Right;

			this.SetBinding(
				MaskedTextBox.TextProperty,
				new Binding("Value") { Source = this, StringFormat = "{0:C}" });

		}

		/// <summary>
		/// The decimal value of the text box.
		/// </summary>
		public Decimal Value
		{
			get { return (Decimal)this.GetValue(MoneyTextBox.ValueProperty); }
			set { this.SetValue(MoneyTextBox.ValueProperty, value); }
		}

		/// <summary>
		/// Raise the ValueChanged event.
		/// </summary>
		/// <param name="sender">The MoneyTextBox whose Value changed.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private static void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			MoneyTextBox box = sender as MoneyTextBox;

			if (box.ValueChanged != null)
				box.ValueChanged(sender, eventArgs);

		}

	}

}
