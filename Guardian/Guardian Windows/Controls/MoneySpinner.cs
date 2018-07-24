namespace FluidTrade.Guardian.Windows.Controls
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Windows.Controls;
	using System.Windows.Data;
	using System.Threading;
	using System.Text.RegularExpressions;

	/// <summary>
	/// A spinner whose value is currency amounts displayed in a localized format.
	/// </summary>
	public class MoneySpinner : Spinner
	{

		/// <summary>
		/// Create a new percentage spinner.
		/// </summary>
		public MoneySpinner()
		{

			this.DisplayFormat = "$#0.00";
			this.IsInteger = false;
			this.Loaded += this.OnLoaded;

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

				int caret = textBox.SelectionStart;
				BindingExpression textBinding = this.textBox.GetBindingExpression(TextBox.TextProperty);
				string text;
				int dollar = textBox.Text.IndexOf(Thread.CurrentThread.CurrentUICulture.NumberFormat.CurrencySymbol);

				if (dollar != 0 && dollar != 1)
					caret += Thread.CurrentThread.CurrentUICulture.NumberFormat.CurrencySymbol.Length;

				text = this.VerifyMoney(textBox.Text);

				if (text != this.textBox.Text)
				{

					this.textBox.Text = text;

				}

				if (caret < text.Length + 1 && caret > 0)
					this.textBox.SelectionStart = caret;
				else if (textBox.Text.Length > 0)
					this.textBox.SelectionStart = textBox.Text.Length - 1;
				else
					this.textBox.SelectionStart = 0;

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
		protected string VerifyMoney(string text)
		{

			string dollar = Thread.CurrentThread.CurrentUICulture.NumberFormat.CurrencySymbol;

			if (text != "" && !Regex.Match(text, @"^[-+]?" + Regex.Escape(dollar) + @"\d*(\.\d{1,2})?$").Success)
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
				
				// Make sure there's only one "dollar sign"
/*				if (text.IndexOfAny(new char[] { '+', '-' }) == 0)
					text = text.Insert(1, dollar);
				else
					text = text.Insert(0, dollar);*/
				text = String.Format("{0:" + this.DisplayFormat + "}", Decimal.Parse(text));

			}

			return text;

		}

	}

}
