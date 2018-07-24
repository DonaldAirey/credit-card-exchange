namespace FluidTrade.Guardian.Windows.Controls
{

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;

    /// <summary>
    /// Interaction logic for Spinner control.
    /// </summary>
    public partial class Spinner : UserControl
    {

        /// <summary>
        /// A custom converter to keep track of the relationship between the value of a Spinner and the text in the Spinner's text box.
        /// </summary>
        private class NumberStringConverter : IValueConverter
        {

            private Spinner spinner;

            /// <summary>
            /// Create a converter and bind it to a spinner.
            /// </summary>
            /// <param name="spinner"></param>
            public NumberStringConverter(Spinner spinner)
            {

                this.spinner = spinner;

            }

            /// <summary>
            /// Convert between the text of the spinner and its value. When the spinner's text has been set directly with the Text property
            /// (ie. UserText is true) then no conversion should take place (since the text box probably contains a non-integer). Otherwise,
            /// normal string to integer (and back) conversion takes place, provided the source of the conversion isn't empty/null, which case
            /// the target of the conversion will also be empty.
            /// </summary>
            /// <param name="value">The source of the conversion</param>
            /// <param name="targetType">The type to convert the value to.</param>
            /// <param name="parameter">Ignored.</param>
            /// <param name="culture">The current culture of the spinner.</param>
            /// <returns>The appropriate value for the target type.</returns>
            public object Convert(object value, Type targetType, object parameter,
                System.Globalization.CultureInfo culture)
            {

				if (targetType == typeof(decimal?))
				{

					if (String.IsNullOrEmpty(value as string))
					{

						return null;

					}
					else
					{

						String newValue = Regex.Replace(value as string, "[^0-9.]", "");

						Decimal result;
						Boolean parsed = Decimal.TryParse(newValue, out result);

						return parsed? (Decimal?)result : null;

					}

				}
				else
				{

					decimal? decimalValue = (decimal?)value;

					if (decimalValue == null)
						return "";
					else
						return String.Format("{0:"+this.spinner.DisplayFormat+"}", decimalValue);

				}

            }

            /// <summary>
            /// Convert between the text of the spinner and its value.
            /// </summary>
            /// <param name="value">The source of the conversion</param>
            /// <param name="targetType">The type to convert the value to.</param>
            /// <param name="parameter">Ignored.</param>
            /// <param name="culture">The current culture of the spinner.</param>
            /// <returns>The appropriate value for the target type.</returns>
            public object ConvertBack(object value, Type targetType, object parameter,
                System.Globalization.CultureInfo culture)
            {

                return Convert(value, targetType, parameter, culture);

            }

        }

		/// <summary>
		/// Indicates the DisplayFormat dependency property.
		/// </summary>
		public static readonly DependencyProperty DisplayFormatProperty;
		/// <summary>
		/// Indicates the IsInteger dependency property.
		/// </summary>
		public static readonly DependencyProperty IsIntegerProperty;
		/// <summary>
		/// Indicates the is MaxValue dependency property.
		/// </summary>
		public static readonly DependencyProperty IsMaxValueProperty;
		/// <summary>
		/// Indicates the IsMinValue dependency property.
		/// </summary>
		public static readonly DependencyProperty IsMinValueProperty;
		/// <summary>
		/// Indicates the IsNullable dependency property.
		/// </summary>
		public static readonly DependencyProperty IsNullableProperty;
		/// <summary>
		/// Indicates the IsReadonly dependency property.
		/// </summary>
		public static readonly DependencyProperty IsReadOnlyProperty;
		/// <summary>
		/// Indicates the MaxValue dependency property.
		/// </summary>
		public static readonly DependencyProperty MaxValueProperty;
		/// <summary>
		/// Indicates the MinValue dependency property.
		/// </summary>
		public static readonly DependencyProperty MinValueProperty;
		/// <summary>
		/// Indicates the Step dependency property.
		/// </summary>
		public static readonly DependencyProperty StepProperty;
		/// <summary>
		/// Indicates the TextAlignment dependency property.
		/// </summary>
		public static readonly DependencyProperty TextAlignmentProperty;
		/// <summary>
		/// Indicates the Value dependency property.
		/// </summary>
		public static readonly DependencyProperty ValueProperty;
		/// <summary>
		/// Indicates the ValueChanged routed event.
		/// </summary>
		public static readonly RoutedEvent ValueChangedEvent;

		/// <summary>
		/// The command sent to step the spinner's value up.
		/// </summary>
		public static readonly RoutedCommand IncreaseCommand = new RoutedCommand("Increase", typeof(Spinner));
		/// <summary>
		/// The command sent to step the spinner's value up.
		/// </summary>
		public static readonly RoutedCommand DecreaseCommand = new RoutedCommand("Decrease", typeof(Spinner));
		
		/// <summary>
        /// The event fired when the value of the spinner changes.
        /// </summary>
        public event RoutedEventHandler ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }

		private decimal? lastValue;

        /// <summary>
        /// Setup the dependancy properties.
        /// </summary>
        static Spinner()
        {

			Spinner.DisplayFormatProperty = DependencyProperty.Register("DisplayFormat", typeof(string), typeof(Spinner), new PropertyMetadata("0"));
			Spinner.MaxValueProperty = DependencyProperty.Register("MaxValue", typeof(decimal?), typeof(Spinner), new PropertyMetadata(null));
			Spinner.MinValueProperty = DependencyProperty.Register("MinValue", typeof(decimal?), typeof(Spinner), new PropertyMetadata(null));
			Spinner.IsIntegerProperty = DependencyProperty.Register("IsInteger", typeof(Boolean), typeof(Spinner), new PropertyMetadata(true, Spinner.OnIsIntegerChanged));
			Spinner.IsMaxValueProperty = DependencyProperty.Register("IsMaxValue", typeof(Boolean), typeof(Spinner));
			Spinner.IsMinValueProperty = DependencyProperty.Register("IsMinValue", typeof(Boolean), typeof(Spinner));
			Spinner.IsNullableProperty = DependencyProperty.Register("IsNullable", typeof(Boolean), typeof(Spinner), new PropertyMetadata(false, Spinner.OnIsNullableChanged));
			Spinner.IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(Boolean), typeof(Spinner));
			Spinner.StepProperty = DependencyProperty.Register("Step", typeof(decimal), typeof(Spinner), new PropertyMetadata(1.0m));
			Spinner.TextAlignmentProperty = DependencyProperty.Register("TextAlignment", typeof(TextAlignment), typeof(Spinner), new PropertyMetadata(TextAlignment.Left));
			Spinner.ValueProperty = DependencyProperty.Register("Value", typeof(decimal?), typeof(Spinner), new PropertyMetadata(null, Spinner.OnValueChanged, Spinner.CoerceValue));

			Spinner.ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Spinner));

        }

        /// <summary>
        /// Create a new spin control.
        /// </summary>
        public Spinner()
        {
 
            InitializeComponent();
			this.lastValue = null;
            this.textBox.SetBinding(TextBox.TextProperty, new Binding("Value") { Converter = new NumberStringConverter(this), Source = this });

        }

		/// <summary>
		/// The format with which to display the value in the text box. The default is "#0".
		/// </summary>
		public string DisplayFormat
		{

			get { return this.GetValue(Spinner.DisplayFormatProperty) as string; }
			set { this.SetValue(Spinner.DisplayFormatProperty, value); }

		}

		/// <summary>
		/// Get or set whether the content of the spinner is considered an integer. If true, content of the spinner is constrained to integer values.
		/// The default is true.
		/// </summary>
		public Boolean IsInteger
		{

			get { return (Boolean)this.GetValue(Spinner.IsIntegerProperty); }
            set { this.SetValue(Spinner.IsIntegerProperty, value); }
			
		}

        /// <summary>
		/// Get (or set) whether Value is also the MaxValue.
		/// </summary>
        public Boolean IsMaxValue
        {

            get { return (Boolean)this.GetValue(Spinner.IsMaxValueProperty); }
            set { this.SetValue(Spinner.IsMaxValueProperty, value); }

        }

        /// <summary>
        /// Get (or set) whether Value is also the MinValue.
        /// </summary>
        public Boolean IsMinValue
        {

            get { return (Boolean)this.GetValue(Spinner.IsMinValueProperty); }
            set { this.SetValue(Spinner.IsMinValueProperty, value); }

        }

		/// <summary>
		/// Get or set whether Value can be null.
		/// </summary>
		public Boolean IsNullable
		{

			get { return (Boolean)this.GetValue(Spinner.IsNullableProperty); }
			set { this.SetValue(Spinner.IsNullableProperty, value); }

		}

		/// <summary>
		/// Get or set whether spinner is read-only.
		/// </summary>
		public Boolean IsReadOnly
		{

			get { return (Boolean)this.GetValue(Spinner.IsReadOnlyProperty); }
			set { this.SetValue(Spinner.IsReadOnlyProperty, value); }

		}

        /// <summary>
        /// Get or set the maximum value the spinner can have. If MaxValue is null, then the spinner has no maximum value.
        /// </summary>
		public decimal? MaxValue
        {

			get { return (decimal?)this.GetValue(Spinner.MaxValueProperty); }
            set { this.SetValue(Spinner.MaxValueProperty, value); }

        }

        /// <summary>
        /// Get or set the minimum value the spinner can have. If MinValue is null, then the spinner has no minimum value.
        /// </summary>
		public decimal? MinValue
        {

			get { return (decimal?)this.GetValue(Spinner.MinValueProperty); }
            set { this.SetValue(Spinner.MinValueProperty, value); }

        }

		/// <summary>
		/// The amount that increment or decrement step by.
		/// </summary>
		public decimal? Step
		{

			get { return (decimal?)this.GetValue(Spinner.StepProperty); }
			set { this.SetValue(Spinner.StepProperty, value); }

		}

		/// <summary>
		/// The alignment of the text in the text box.
		/// </summary>
		public TextAlignment TextAlignment
		{

			get { return (TextAlignment)this.GetValue(Spinner.TextAlignmentProperty); }
			set { this.SetValue(Spinner.TextAlignmentProperty, value); }

		}

        /// <summary>
        /// Get or set the current integer value of the spinner.
        /// </summary>
		public decimal? Value
        {

			get { return (decimal?)this.GetValue(Spinner.ValueProperty); }
            set { this.SetValue(Spinner.ValueProperty, value); }
            
        }

		/// <summary>
		/// Adjust the new value of Value so it is within the min/max value.
		/// </summary>
		/// <param name="dependencyObject">The spinner.</param>
		/// <param name="newValueObject">The proposed value of Value</param>
		/// <returns>The new value to use for Value.</returns>
		public static object CoerceValue(DependencyObject dependencyObject, object newValueObject)
		{

			Spinner spinner = dependencyObject as Spinner;
			decimal? newValue = (decimal?)newValueObject;

			if (spinner.MaxValue != null && newValue > spinner.MaxValue)
				newValue = spinner.MaxValue;
			if (spinner.MinValue != null && newValue < spinner.MinValue)
				newValue = spinner.MinValue;

			return (object)newValue;

		}

		/// <summary>
		/// Handle the LostFocus event. If necessary, swap in the last valid value.
		/// </summary>
		/// <param name="eventArgs">The event arguments.</param>
		protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs eventArgs)
		{

			this.lastValue = this.Value;

		}

		/// <summary>
		/// Handle the LostFocus event. If necessary, swap in the last valid value.
		/// </summary>
		/// <param name="eventArgs">The event arguments.</param>
		protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs eventArgs)
		{

			if (!this.IsNullable && this.Value == null)
				this.Value = this.lastValue;

			BindingExpression textBinding = this.textBox.GetBindingExpression(TextBox.TextProperty);

			if (textBinding != null)
				textBinding.UpdateTarget();

		}

		/// <summary>
		/// Handle the value of IsInteger changing. Set a reasonable default for DisplayFormat.
		/// </summary>
		/// <param name="sender">The spinner whose IsInteger changed</param>
		/// <param name="eventArgs">The event arguments.</param>
		private static void OnIsIntegerChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			Spinner spinner = sender as Spinner;

			spinner.OnIsInitegerChanged(eventArgs);

		}

		/// <summary>
		/// Handle the value of IsInteger changing. Set a reasonable default for DisplayFormat.
		/// </summary>
		/// <param name="eventArgs">The event arguments.</param>
		protected virtual void OnIsInitegerChanged(DependencyPropertyChangedEventArgs eventArgs)
		{

			if (spinner.IsInteger)
				spinner.DisplayFormat = "#0";
			else
				spinner.DisplayFormat = "#0.0#####";

		}

		/// <summary>
		/// Handle the value of IsNullable changing. Try to set a reasonable value for Value if it doesn't have one.
		/// </summary>
		/// <param name="sender">The spinner whose IsNullable changed</param>
		/// <param name="eventArgs">The event arguments.</param>
		private static void OnIsNullableChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			Spinner spinner = sender as Spinner;

			if (!spinner.IsNullable && spinner.Value == null)
				if (spinner.MinValue != null)
					spinner.Value = spinner.MinValue;
				else if (spinner.MaxValue != null)
					spinner.Value = spinner.MaxValue;
				else
					spinner.Value = 0m;

		}

        /// <summary>
        /// Reject paste events that contain characters other than integer characters.
        /// </summary>
        /// <param name="sender">The text box.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void OnPaste(object sender, DataObjectPastingEventArgs eventArgs)
        {

            if (eventArgs.FormatToApply == System.Windows.DataFormats.UnicodeText)
            {

                String text = eventArgs.DataObject.GetData(eventArgs.FormatToApply) as String;

				if (this.IsInteger)
				{

					if (!Regex.Match(text, @"^[-+]?\d*\.?\d+$").Success)
						eventArgs.CancelCommand();

				}
				else
				{

					if (!Regex.Match(text, @"^[-+]?\d+$").Success)
						eventArgs.CancelCommand();

				}

            }

        }

        /// <summary>
        /// Decrease the spinner's current value by one (if possible).
        /// </summary>
        /// <param name="sender">The down button.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void OnDecrease(object sender, RoutedEventArgs eventArgs)
        {

            // If there is no value yet, try to find a reasonable starting point.
            if (this.Value == null)
                if (this.MaxValue != null)
                    this.Value = this.MaxValue;
                else if (this.MinValue != null)
                    this.Value = this.MinValue;
                else
                    this.Value = 0;

            this.Value -= this.Step;

        }

        /// <summary>
        /// Increase the spinner's current value by one (if possible);
        /// </summary>
        /// <param name="sender">The up button.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void OnIncrease(object sender, RoutedEventArgs eventArgs)
        {
 
            // If there is no value yet, try to find a reasonable starting point.
            if (this.Value == null)
                if (this.MinValue != null)
                    this.Value = this.MinValue;
                else if (this.MaxValue != null)
                    this.Value = this.MaxValue;
                else
                    this.Value = 0;

            this.Value += this.Step;

        }

        /// <summary>
        /// Make the text in the text box is always digits and that the current value tracks the text box's text.
        /// </summary>
        /// <param name="sender">The text box.</param>
        /// <param name="eventArgs">The event arguments.</param>
		protected virtual void OnTextChanged(object sender, TextChangedEventArgs eventArgs)
        {

            if (textBox != null)
            {

				int caret = textBox.SelectionStart - 1;
				BindingExpression textBinding = this.textBox.GetBindingExpression(TextBox.TextProperty);
				string text;

				if (this.IsInteger)
					text = this.VerifyInteger(textBox.Text);
				else
					text = this.VerifyDecimal(textBox.Text);

				if (text != this.textBox.Text)
				{

					this.textBox.Text = text;

					if (caret < text.Length && caret > 0)
						this.textBox.SelectionStart = caret;
					else
						this.textBox.SelectionStart = textBox.Text.Length;

				}

				// The binding with Value only updates when we lose focus, so we have to update it ourselves here.
				if (textBinding != null)
                    textBinding.UpdateSource();

			}

		}

        /// <summary>
        /// Update the appearance of the spinner buttons (if necessary) and fire a ValueChanged event.
        /// </summary>
        /// <param name="sender">The spinner control that owns the value.</param>
        /// <param name="eventArgs">The event arguments.</param>
		static private void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			Spinner spinner = sender as Spinner;

			spinner.OnValueChanged(eventArgs);

		}

        /// <summary>
        /// Update the appearance of the spinner buttons (if necessary) and fire a ValueChanged event.
        /// </summary>
        /// <param name="eventArgs">The event arguments.</param>
        protected void OnValueChanged(DependencyPropertyChangedEventArgs eventArgs)
        {


            if (this.Value == null)
            {

				this.SetValue(Spinner.IsMaxValueProperty, false);
				this.SetValue(Spinner.IsMinValueProperty, false);

            }
            else
            {

				this.SetValue(Spinner.IsMaxValueProperty, spinner.Value == spinner.MaxValue);
				this.SetValue(Spinner.IsMinValueProperty, spinner.Value == spinner.MinValue);

            }

            spinner.RaiseEvent(new RoutedEventArgs(Spinner.ValueChangedEvent));

        }

		/// <summary>
		/// Ensure that a string is a decimal value.
		/// </summary>
		/// <param name="text">The string.</param>
		/// <returns>The new, possibly cleaned string.</returns>
		virtual protected string VerifyDecimal(string text)
		{

			// If there's non-digits in the text box, filter them out. Be careful not to move the caret.
			if (text != "" && !Regex.Match(text, @"^[-+]?\d*\.?\d+$").Success)
			{

				text = Regex.Replace(text, @"[^0-9.+-]", "");

				// Make sure there's only one dot.
				if (text.Contains('.'))
				{

					int dot = text.IndexOf('.');

					text = Regex.Replace(text, @"\.", "");

					if (dot > text.Length)
						text = text.Insert(dot, ".0");
					else
						text = text.Insert(dot, ".");

				}
				// Make sure there are no errant signs.
				if (text.LastIndexOfAny(new char[] { '+', '-' }) > 0)
				{

					string sign = text.Substring(0, 1);

					text = Regex.Replace(text, @"[+-]", "");
					if (sign == "+" || sign == "-")
						text = sign + text;

				}

			}

			return text;

        }

		/// <summary>
		/// Ensure that a string is an integer value.
		/// </summary>
		/// <param name="text">The string.</param>
		/// <returns>The new, possibly cleaned string.</returns>
		virtual protected string VerifyInteger(string text)
		{

			// If there's non-digits in the text box, filter them out. Be careful not to move the caret.
			if (text != "" && !Regex.Match(text, @"^[-+]?\d+$").Success)
			{

				text = Regex.Replace(text, @"[^0-9+-]", "");

				// Make sure there are no errant signs.
				if (text.LastIndexOfAny(new char[] { '+', '-' }) > 0)
				{

					string sign = text.Substring(0, 1);

					text = Regex.Replace(text, @"[+-]", "");
					text = sign + text;

				}

			}

			return text;

        }

    }

}
