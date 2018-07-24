namespace FluidTrade.Guardian.Windows.Controls
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Windows.Controls;
	using System.Windows;

	/// <summary>
	/// A control that displays a TimeSpan with a particular format.
	/// </summary>
	public class TimeSpanBlock : TextBlock
	{

		/// <summary>
		/// Indicates the Format dependency property.
		/// </summary>
		public static readonly DependencyProperty FormatProperty = DependencyProperty.Register("Format", typeof(string), typeof(TimeSpanBlock), new PropertyMetadata(OnFormatChanged));
		/// <summary>
		/// Indicates the Value dependency property.
		/// </summary>
		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(TimeSpan), typeof(TimeSpanBlock), new PropertyMetadata(OnValueChanged));

		private string cachedFormat = "{0}d{1}:{2}:{3}.{3}";

		/// <summary>
		/// Gets or sets the format used to display the Value.
		/// </summary>
		public string Format
		{

			get { return this.GetValue(TimeSpanBlock.FormatProperty) as string; }
			set { this.SetValue(TimeSpanBlock.FormatProperty, value); }

		}

		/// <summary>
		/// Gets or sets the TimeSpan to display.
		/// </summary>
		public TimeSpan Value
		{

			get { return (TimeSpan)this.GetValue(TimeSpanBlock.ValueProperty); }
			set { this.SetValue(TimeSpanBlock.ValueProperty, value); }

		}

		/// <summary>
		/// Generate the format used to display the Value.
		/// </summary>
		/// <returns></returns>
		private string GetFormat()
		{

			Dictionary<char, char> formatChars = new Dictionary<char, char>();
			StringBuilder format = new StringBuilder();
			int offset = 0;
			
			formatChars['d'] = '0';
			formatChars['h'] = '1';
			formatChars['m'] = '2';
			formatChars['s'] = '3';
			formatChars['f'] = '4';

			while (offset < this.Format.Length - 1)
			{

				char formatChar = this.Format[offset];

				if (formatChars.ContainsKey(formatChar))
				{

					format.Append('{');
					format.Append(formatChars[formatChar]);
					format.Append(':');

					do
					{

						format.Append('0');

					} while (offset+1 < this.Format.Length && this.Format[++offset] == formatChar);

					format.Append('}');

				}
				else
				{

					format.Append(this.Format[offset++]);

				}

			}

			return format.ToString();

		}

		/// <summary>
		/// Handle the Format changing.
		/// </summary>
		/// <param name="sender">The TimeSpanBlock object.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private static void OnFormatChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			TimeSpanBlock block = sender as TimeSpanBlock;

			if (block != null)
			{

				block.cachedFormat = block.GetFormat();
				block.Text = String.Format(block.cachedFormat, block.Value.Days, block.Value.Hours, block.Value.Minutes, block.Value.Seconds, block.Value.Ticks);

			}

		}

		/// <summary>
		/// Handle the Value changing.
		/// </summary>
		/// <param name="sender">The TimeSpanBlock object.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private static void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			TimeSpanBlock block = sender as TimeSpanBlock;

			if (block != null)
			{

				block.Text = String.Format(block.cachedFormat, block.Value.Days, block.Value.Hours, block.Value.Minutes, block.Value.Seconds, block.Value.Ticks);

			}

		}

	}

}
