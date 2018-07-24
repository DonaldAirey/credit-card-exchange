namespace FluidTrade.Guardian.Windows.Controls
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using FluidTrade.Guardian.Windows.Controls;
	using System.Windows.Controls;
	using System.Windows.Data;
	using System.Text.RegularExpressions;
	using System.Windows;

	/// <summary>
	/// A spinner whose value is a ratio (eg. .25, .89) displayed as a percentage.
	/// </summary>
	public class PercentageSpinner : Spinner
	{

		/// <summary>
		/// A custom converter to keep track of the relationship between the value of a Spinner and the text in the Spinner's text box.
		/// </summary>
		private class PercentStringConverter : IValueConverter
		{

			private Spinner spinner;

			/// <summary>
			/// Create a converter and bind it to a spinner.
			/// </summary>
			/// <param name="spinner"></param>
			public PercentStringConverter(Spinner spinner)
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

						string newValue = Regex.Replace(value as string, "[^0-9.]", "");

						if (newValue != "")
							return Decimal.Parse(Regex.Replace(value as string, "[^0-9.]", "")) / 100m;
						else
							return null;

					}

				}
				else
				{

					decimal? decimalValue = (decimal?)value;

					if (decimalValue == null)
						return "";
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
		/// Create a new percentage spinner.
		/// </summary>
		public PercentageSpinner()
		{

			this.Step = 0.01m;
			this.IsInteger = false;
			this.DisplayFormat = "0.#%";
			this.Loaded += this.OnLoaded;
			this.textBox.SetBinding(TextBox.TextProperty, new Binding("Value") { Converter = new PercentStringConverter(this), Source = this });

		}

		/// <summary>
		/// Handle the value of IsInteger changing. Set a reasonable default for DisplayFormat.
		/// </summary>
		/// <param name="eventArgs">The event arguments.</param>
		protected override void OnIsInitegerChanged(DependencyPropertyChangedEventArgs eventArgs)
		{

				spinner.DisplayFormat = "#0.#####%";

		}

		/// <summary>
		/// Handle the loaded event. Change some of the defaults to 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="eventArgs"></param>
		public void OnLoaded(object sender, EventArgs eventArgs)
		{


		}

		/// <summary>
		/// Make the text in the text box is always digits and that the current value tracks the text box's text.
		/// </summary>
		/// <param name="sender">The text box.</param>
		/// <param name="eventArgs">The event arguments.</param>
		override protected void OnTextChanged(object sender, TextChangedEventArgs eventArgs)
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
						this.textBox.SelectionStart = textBox.Text.Length - 1;

				}

				// The binding with Value only updates when we lose focus, so we have to update it ourselves here.
				if (textBinding != null)
					textBinding.UpdateSource();

			}

		}

		/// <summary>
		/// Clean up a string so that it is a decimal percentage.
		/// </summary>
		/// <param name="text">The string to process.</param>
		/// <returns>The cleaned string.</returns>
		protected override string VerifyDecimal(string text)
		{

			if (text != "" && !Regex.Match(text, @"^[-+]?\d*\.?\d+%$").Success)
				return base.VerifyDecimal(text) + "%";
			else
				return text;

		}

		/// <summary>
		/// Clean up a string so that it is an integer percentage.
		/// </summary>
		/// <param name="text">The string to process.</param>
		/// <returns>The cleaned string.</returns>
		protected override string VerifyInteger(string text)
		{

			if (text != "" && !Regex.Match(text, @"^[-+]?\d+%$").Success)
				return base.VerifyInteger(text);
			else
				return text;

		}

	}

}
