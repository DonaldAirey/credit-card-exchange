namespace FluidTrade.Guardian.Windows.Controls
{

	using System;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Input;

	/// <summary>
	/// A TextBox for editing amounts of money. The value of the MoneyBox is displayed as a money amount in the current locale, but is edited a
	/// simple decimal value.
	/// </summary>
	public class MoneyBox : TextBox
	{

		/// <summary>
		/// Indicates the DisplayText dependency property.
		/// </summary>
		public static readonly DependencyProperty DisplayTextProperty = DependencyProperty.Register("DisplayText", typeof(string), typeof(MoneyBox));
		/// <summary>
		/// Indicates the IsEditing dependency property.
		/// </summary>
		public static readonly DependencyProperty IsEditingProperty = DependencyProperty.Register("IsEditing", typeof(Boolean), typeof(MoneyBox), new PropertyMetadata(false));
		/// <summary>
		/// Indicates the IsNullable dependency property.
		/// </summary>
		public static readonly DependencyProperty IsNullableProperty =
			DependencyProperty.Register("IsNullable", typeof(Boolean), typeof(MoneyBox), new PropertyMetadata(false, MoneyBox.OnIsNullableChanged));
		/// <summary>
		/// Indicates the Value dependency property.
		/// </summary>
		public static readonly DependencyProperty ValueProperty =
			DependencyProperty.Register("Value", typeof(decimal), typeof(MoneyBox), new PropertyMetadata(0.0m, MoneyBox.OnValueChanged));
		/// <summary>
		/// Indicates the ValueChanged routed event.
		/// </summary>
		public static readonly RoutedEvent ValueChangedEvent =
			EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MoneyBox));

		/// <summary>
		/// The event fired when the value of the money box changes.
		/// </summary>
		public event RoutedEventHandler ValueChanged
		{
			add { AddHandler(ValueChangedEvent, value); }
			remove { RemoveHandler(ValueChangedEvent, value); }
		}

		private decimal oldValue = 0m;

		static MoneyBox()
		{

			DefaultStyleKeyProperty.OverrideMetadata(typeof(MoneyBox), new FrameworkPropertyMetadata(typeof(MoneyBox)));

		}

		/// <summary>
		/// Create a new MoneyBox.
		/// </summary>
		public MoneyBox()
		{

			this.Text = String.Format("{0:0.00}", this.Value);
			this.DisplayText = String.Format("{0:C}", this.Value);
			this.IsKeyboardFocusWithinChanged += OnIsKeyboardFocusWithinChanged;

		}

		/// <summary>
		/// The text displayed when the MoneyBox does not have focus.
		/// </summary>
		public string DisplayText
		{

			get { return this.GetValue(MoneyBox.DisplayTextProperty) as string; }
			set { this.SetValue(MoneyBox.DisplayTextProperty, value); }

		}

		/// <summary>
		/// True when the user is editing the value in the MoneyBox, false otherwise.
		/// </summary>
		public Boolean IsEditing
		{

			get { return (Boolean)this.GetValue(MoneyBox.IsEditingProperty); }
			set { this.SetValue(MoneyBox.IsEditingProperty, value); }

		}

		/// <summary>
		/// Get or set whether Value can be null.
		/// </summary>
		public Boolean IsNullable
		{

			get { return (Boolean)this.GetValue(MoneyBox.IsNullableProperty); }
			set { this.SetValue(MoneyBox.IsNullableProperty, value); }

		}

		/// <summary>
		/// Get or set the current integer value of the MoneyBox.
		/// </summary>
		public decimal? Value
		{

			get { return (decimal?)this.GetValue(MoneyBox.ValueProperty); }
			set { this.SetValue(MoneyBox.ValueProperty, value); }

		}

		/// <summary>
		/// Handing the changing of IsKeyboardFocusWithin. Set IsEditing appropriately.
		/// </summary>
		/// <param name="sender">The MoneyBox.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnIsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			this.IsEditing = (Boolean)eventArgs.NewValue && !this.IsReadOnly;

		}

		/// <summary>
		/// Handle the value of IsNullable changing. Try to set a reasonable value for Value if it doesn't have one.
		/// </summary>
		/// <param name="sender">The MoneyBox.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private static void OnIsNullableChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			MoneyBox box = sender as MoneyBox;

			if (!box.IsNullable && box.Value == null)
				box.Value = box.oldValue;

		}

		/// <summary>
		/// Handle losing keyboard focus. Set the value of Value to match the value of Text, if possible, and the old value of Value if not.
		/// </summary>
		/// <param name="eventArgs">The event arguments.</param>
		protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs eventArgs)
		{

			if (String.IsNullOrEmpty(this.Text))
			{

				if (this.IsNullable)
				{

					this.Value = null;

				}
				else
				{

					this.Value = this.oldValue;

				}

			}
			else
			{

				this.Value = Decimal.Parse(this.Text);

			}

			base.OnLostKeyboardFocus(eventArgs);

		}

        /// <summary>
        /// Make sure the text in the text box is always a decimal number.
        /// </summary>
        /// <param name="eventArgs">The event arguments.</param>
		protected override void OnTextChanged(TextChangedEventArgs eventArgs)
        {

			int caret = this.SelectionStart - 1;
			string text;

			text = this.VerifyDecimal(this.Text);

			if (text != this.Text)
			{

				this.Text = text;

				if (caret < text.Length && caret > 0)
					this.SelectionStart = caret;
				else
					this.SelectionStart = this.Text.Length;

			}

		}
		
        /// <summary>
        /// Handle the Value of MoneyBox changing. Change the value of Text to match.
        /// </summary>
        /// <param name="sender">The MoneyBox.</param>
        /// <param name="eventArgs">The event arguments.</param>
		static private void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			MoneyBox box = sender as MoneyBox;

			box.Text = String.Format("{0:0.00}", box.Value);
			box.DisplayText = String.Format("{0:C}", box.Value);

			box.RaiseEvent(new RoutedEventArgs(MoneyBox.ValueChangedEvent));

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
					text = text.Insert(dot, ".");

				}
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
