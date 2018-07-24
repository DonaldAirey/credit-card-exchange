namespace FluidTrade.Actipro
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
		/// Indicates the IsDisplayOnly dependency property.
		/// </summary>
		public static readonly DependencyProperty IsDisplayOnlyProperty =
			DependencyProperty.Register("IsDisplayOnly", typeof(Boolean), typeof(MoneyTextBox), new PropertyMetadata(false));
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
		/// Gets or sets whether the text box is for display only.
		/// </summary>
		public Boolean IsDisplayOnly
		{
			get { return (Boolean)this.GetValue(MoneyTextBox.IsDisplayOnlyProperty); }
			set { this.SetValue(MoneyTextBox.IsDisplayOnlyProperty, value); }
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
		/// Update the text when focus is lost.
		/// </summary>
		/// <param name="e">The event arguments.</param>
		protected override void OnLostKeyboardFocus(System.Windows.Input.KeyboardFocusChangedEventArgs e)
		{

			BindingExpression bindingExpression = this.GetBindingExpression(MaskedTextBox.TextProperty);

			base.OnLostKeyboardFocus(e);

			// For some reason, the selection isn't getting cleared when we lose keyboard focus, so we force it here.
			this.SelectionLength = 0;

			// Make sure the display looks reflects the underlying values.
			bindingExpression.UpdateTarget();

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
