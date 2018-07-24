namespace FluidTrade.Guardian.Windows.Controls
{

	using System;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Data;
	using System.Windows.Input;
	using FluidTrade.Core;
	using System.ComponentModel;

	/// <summary>
	/// A control for editing lengths of time in days, weeks, or months.
	/// </summary>
	public class TermLength : TextBox
	{

		private static readonly DependencyProperty InternalTextProperty = DependencyProperty.Register(
			"InternalText",
			typeof(string),
			typeof(TermLength),
			new PropertyMetadata("") { CoerceValueCallback = TermLength.CoerceText });

		/// <summary>
		/// Indicates the Length dependency property.
		/// </summary>
		public static readonly DependencyProperty LengthProperty = DependencyProperty.Register(
			"Length",
			typeof(decimal?),
			typeof(TermLength),
			new PropertyMetadata(TermLength.OnLengthChanged));
		/// <summary>
		/// Indicates the Units dependency property.
		/// </summary>
		public static readonly DependencyProperty UnitsProperty = DependencyProperty.Register(
			"Units",
			typeof(Guid?),
			typeof(TermLength),
			new PropertyMetadata(TermLength.OnUnitsChanged));
		/// <summary>
		/// Indicates the UnitsChanged routed event.
		/// </summary>
		public static readonly RoutedEvent UnitsChangedEvent =
			EventManager.RegisterRoutedEvent("UnitsChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TermLength));
		/// <summary>
		/// Indicates the LengthChanged routed event.
		/// </summary>
		public static readonly RoutedEvent LengthChangedEvent =
			EventManager.RegisterRoutedEvent("LengthChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TermLength));

		/// <summary>
		/// The event fired when the length of the term length changes.
		/// </summary>
		public event RoutedEventHandler LengthChanged
		{
			add { AddHandler(LengthChangedEvent, value); }
			remove { RemoveHandler(LengthChangedEvent, value); }
		}
		/// <summary>
        /// The event fired when the units of the term length changes.
        /// </summary>
        public event RoutedEventHandler UnitsChanged
        {
            add { AddHandler(UnitsChangedEvent, value); }
            remove { RemoveHandler(UnitsChangedEvent, value); }
        }

		private decimal lastValue;
		private TimeUnit lastUnits;

		private class TermLengthConverter : IValueConverter
		{

			private TermLength termLength;

			/// <summary>
			/// Create a converter and bind it to a TermLength.
			/// </summary>
			/// <param name="termLength">The TermLength object this converter is associated with.</param>
			public TermLengthConverter(TermLength termLength)
			{

				this.termLength = termLength;

			}

			/// <summary>
			/// Convert between decimal&amp;TimeUnit and a string.
			/// </summary>
			/// <param name="value">The new value just set.</param>
			/// <param name="targetType">The type of object we should return.</param>
			/// <param name="parameter">Paremeter to the converter (ignored).</param>
			/// <param name="culture">The culture to do the conversion in.</param>
			/// <returns>The converted value.</returns>
			public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
			{

				try
				{

					if (targetType == typeof(Guid?))
						return this.ConvertToTimeUnitId(value as String);
					else if (targetType == typeof(Decimal?))
						return this.ConvertToDecimal(value as String);
					else if (targetType == typeof(String))
						return this.ConvertToString(value);

				}
				catch(Exception exception)
				{

					EventLog.Warning("TermLengthConverter could not convert object of type '{0}' to a '{1}'. {2}", value.GetType(), targetType,
							exception.Message); 

				}

				return null;

			}

			/// <summary>
			/// Convert between decimal&amp;TimeUnit and a string.
			/// </summary>
			/// <param name="value">The new value just set.</param>
			/// <param name="targetType">The type of object we should return.</param>
			/// <param name="parameter">Paremeter to the converter (ignored).</param>
			/// <param name="culture">The culture to do the conversion in.</param>
			/// <returns>The converted value.</returns>
			public object ConvertBack(object value, Type targetType, object parameter,
				System.Globalization.CultureInfo culture)
			{

				return Convert(value, targetType, parameter, culture);

			}

			/// <summary>
			/// Convert the decimal part of a string to a Decimal?.
			/// </summary>
			/// <param name="value">The source value.</param>
			/// <returns>The Decimal? representation of the left half of the source, or null if there isn't one.</returns>
			private object ConvertToDecimal(String value)
			{

				if (Regex.Match(value, @"^[-+]?\d*\.?\d").Success)
				{

					String[] valueAndUnits = (value as string).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
					Boolean parsed = false;
					Decimal parsedValue = 0m;

					parsed = Decimal.TryParse(valueAndUnits[0], out parsedValue);

					return parsed? (Decimal?)Math.Abs(parsedValue) : null;

				}
				else
				{

					return null;

				}

			}

			/// <summary>
			/// Convert the the right half a string into a TimeUnitId (a Guid?).
			/// </summary>
			/// <param name="value">The source value.</param>
			/// <returns>The Guid? representation of the right half of the source, or null if there isn't one.</returns>
			private object ConvertToTimeUnitId(String value)
			{

				if (Regex.Match(value as string, @"^[-+]?\d*\.?\d+ +(Days|Weeks|Months)$").Success)
				{

					string[] valueAndUnits = (value as string).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

					return TimeUnitList.Default.Find((TimeUnit)Enum.Parse(typeof(TimeUnit), valueAndUnits[1], true));

				}
				else
				{

					return null;

				}

			}

			/// <summary>
			/// Combine the Length and Unit pieces of a term into a String.
			/// </summary>
			/// <param name="value">The new value of either the length or unit part.</param>
			/// <returns>A string based on </returns>
			private object ConvertToString(Object value)
			{

				if (value is Decimal?)
					return String.Format("{0:0.###} {1}", value, termLength.Units == null? "" : (Object)(TimeUnitList.Default.Find(termLength.Units.Value).TimeUnitCode));
				else
					return String.Format("{0:0.###} {1}", this.termLength.Length, value == null ? "" : (Object)(TimeUnitList.Default.Find((Guid)value).TimeUnitCode));

			}

		}

		static TermLength()
		{

			//TextBox.TextProperty.AddOwner(typeof(TermLength), new PropertyMetadata() { CoerceValueCallback = CoerceText });
			//DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(TextBox.TextProperty, typeof(TermLength));
			//dpd.DesignerCoerceValueCallback = CoerceText;

		}

		/// <summary>
		/// Create a new TermLength control.
		/// </summary>
		public TermLength() : base()
		{

			lastValue = 0m;
			lastUnits = TimeUnit.Days;

		}

		/// <summary>
		/// The scalar portion of the term length.
		/// </summary>
		public decimal? Length
		{

			get { return (decimal?)this.GetValue(TermLength.LengthProperty); }
			set { this.SetValue(TermLength.LengthProperty, value); }

		}

		/// <summary>
		/// The time units the term length is in.
		/// </summary>
		public Guid? Units
		{

			get { return (Guid?)this.GetValue(TermLength.UnitsProperty); }
			set { this.SetValue(TermLength.UnitsProperty, value); }

		}

		/// <summary>
		/// Discover which (if any) TimeUnit member best matches what's already been entered for the units portion of the control and return a
		/// string containing what of the unit's name missing from the already entered text.
		/// </summary>
		/// <param name="text">The already-entered text of the unit.</param>
		/// <returns>The 'rest' of the unit's name.</returns>
		private static string CompleteUnit(string text)
		{

			// Do our best to find a best matching time unit and return what's been typed of it so far.
			if (!String.IsNullOrEmpty(text))
				foreach (string unit in Enum.GetNames(typeof(TimeUnit)))
					if (unit.StartsWith(text, true, System.Globalization.CultureInfo.CurrentUICulture))
						return unit;

			return "";

		}

		/// <summary>
		/// Handle the GotFocus event. If possible, store the current valid value for use later.
		/// </summary>
		/// <param name="eventArgs">The event arguments.</param>
		protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs eventArgs)
		{

			if (this.Length != null)
				this.lastValue = this.Length.Value;
			if (this.Units != null)
				this.lastUnits = TimeUnitList.Default.Find(this.Units.Value).TimeUnitCode;

			base.OnGotKeyboardFocus(eventArgs);

		}

		/// <summary>
		/// Handle the LostFocus event. If necessary, swap in the last valid value.
		/// </summary>
		/// <param name="eventArgs">The event arguments.</param>
		protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs eventArgs)
		{

			// When the user is done editing, making sure we have valid values for Length and Units.
			if (!this.IsReadOnly)
			{

				string[] valueAndUnits = this.Text.Split(new char[] { ' ' });

				if (valueAndUnits.Length > 1)
				{

					string unitText = TermLength.CompleteUnit(valueAndUnits[1]);

					if (!String.IsNullOrEmpty(unitText))
						try
						{

							this.Units = TimeUnitList.Default.Find((TimeUnit)Enum.Parse(typeof(TimeUnit), unitText, true)).TimeUnitId;

						}
						catch
						{

							this.Units = null;

						}
				}

				if (Regex.Match(valueAndUnits[0], @"^[-+]?\d*\.?\d").Success)
				{

					Boolean parsed = false;
					Decimal parsedValue = 0m;

					parsed = Decimal.TryParse(valueAndUnits[0], out parsedValue);

					this.Length = parsed ? (Decimal?)Math.Abs(parsedValue) : null;

				}

				if (this.Length == null)
					this.Length = this.lastValue;
				if (this.Units == null)
					this.Units = TimeUnitList.Default.Find(this.lastUnits).TimeUnitId;

				this.Text = String.Format("{0:0.###} {1}", this.Length, this.Units == null ? "" : (Object)(TimeUnitList.Default.Find(this.Units.Value).TimeUnitCode));

			}

			base.OnLostKeyboardFocus(eventArgs);

		}

		/// <summary>
		/// Handle the text of the control changing. Maintain the number-space-unit formatting as much as is possible. If the user is typing in the
		/// units, try to complete it for them.
		/// </summary>
		/// <param name="sender">The TermLength.</param>
		/// <param name="newText">The event arguments.</param>
		private static object CoerceText(DependencyObject sender, object newText)
		{

#if false
			TermLength termLength = sender as TermLength;
			string text = newText as string;
			string[] valueAndUnits = text.Split(new char[] { ' ' }, StringSplitOptions.None);

			if (!String.IsNullOrEmpty(text) && !Regex.Match(text, @"^[-+]?\d*\.?\d+ +(Days|Weeks|Months)$").Success)
			{

				string value = "";
				string units = "";
				int caret = termLength.SelectionStart;

				if (valueAndUnits.Length >= 2)
				{

					value = TermLength.VerifyDecimal(valueAndUnits[0]);
					units = TermLength.VerifyUnit(valueAndUnits[1]);

					text = value + " " + units;

				}
				else if (valueAndUnits.Length == 1)
				{

					if (Regex.Match(valueAndUnits[0], @"\d|\.").Success)
					{

						value = TermLength.VerifyDecimal(valueAndUnits[0]);
						text = value;

					}
					else
					{

						units = TermLength.VerifyUnit(valueAndUnits[0]);
						text = units;

					}

				}

				if (caret < text.Length && caret >= 0)
					termLength.SelectionStart = caret;
				else
					termLength.SelectionStart = text.Length;

			}
#endif

			return newText;

		}

		/// <summary>
		/// Handles a change in the text. Updates the bindings with Length and Units.
		/// </summary>
		/// <param name="eventArgs">The event arguments</param>
		protected override void OnTextChanged(TextChangedEventArgs eventArgs)
		{

			string[] valueAndUnits = this.Text.Split(new char[] { ' ' }, StringSplitOptions.None);
//			BindingExpression binding;

			base.OnTextChanged(eventArgs);

#if false
			binding = this.GetBindingExpression(TermLength.LengthProperty);
			if (binding != null)
				binding.UpdateTarget();

			binding = this.GetBindingExpression(TermLength.UnitsProperty);
			if (binding != null)
				binding.UpdateTarget();

			if (this.SelectionStart == this.Text.Length && this.Units == null && valueAndUnits.Length > 1 && valueAndUnits[1] != "" && !eventArgs.Changes.Any(c => c.RemovedLength > 0))
			{

				string rest = TermLength.CompleteUnit(valueAndUnits[1]);
				this.SelectionLength = rest.Length;
				this.Text += rest;

			}
#endif

		}

		/// <summary>
		/// Raise the ValueChanged event when the value change.
		/// </summary>
		/// <param name="sender">The TermLength</param>
		/// <param name="eventArgs">The event arguments.</param>
		private static void OnLengthChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			TermLength termLength = sender as TermLength;

			termLength.Text = String.Format("{0:0.###} {1}", termLength.Length, termLength.Units == null ? "" : (Object)(TimeUnitList.Default.Find(termLength.Units.Value).TimeUnitCode));
			(sender as TermLength).RaiseEvent(new RoutedEventArgs(TermLength.LengthChangedEvent));

		}

		/// <summary>
		/// Raise the UnitsChanged event when the units change.
		/// </summary>
		/// <param name="sender">The TermLength</param>
		/// <param name="eventArgs">The event arguments.</param>
		private static void OnUnitsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			TermLength termLength = sender as TermLength;

			termLength.Text = String.Format("{0:0.###} {1}", termLength.Length, termLength.Units == null ? "" : (Object)(TimeUnitList.Default.Find(termLength.Units.Value).TimeUnitCode));
			(sender as TermLength).RaiseEvent(new RoutedEventArgs(TermLength.UnitsChangedEvent));

		}

		/// <summary>
		/// Ensure that a string is a valid decimal value.
		/// </summary>
		/// <param name="text">The text to clean up.</param>
		/// <returns>The cleaned-up text.</returns>
		private static string VerifyDecimal(string text)
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

		/// <summary>
		/// Ensure that a string is either a valid TimeUnit, or could be should the user finish typing it.
		/// </summary>
		/// <param name="text">The text to clean-up.</param>
		/// <returns>The cleaned-up text.</returns>
		private static string VerifyUnit(string text)
		{

			// Do our best to find a best matching time unit and return what's been typed of it so far.
			foreach (string unit in Enum.GetNames(typeof(TimeUnit)))
				if (unit.StartsWith(text, true, System.Globalization.CultureInfo.CurrentUICulture))
					return unit.Substring(0, text.Length);
				else if (text.StartsWith(unit, true, System.Globalization.CultureInfo.CurrentUICulture))
					return unit;

			return "";

		}

	}

}
