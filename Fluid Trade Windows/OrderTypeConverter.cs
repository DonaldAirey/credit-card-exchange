namespace FluidTrade.Core.Windows
{

	using System;
	using System.Globalization;
    using System.Windows.Data;

	/// <summary>
	/// Converts an Order Type into a human readable form.
	/// </summary>
	[ValueConversion(typeof(object), typeof(string))]
	public class OrderTypeConverter : IValueConverter
	{

		/// <summary>
		/// Converts a value into human readable text using the C# conversion formats.
		/// </summary>
		/// <param name="value">The value to be converted.</param>
		/// <param name="targetType">The target data type.</param>
		/// <param name="parameter">The formatting string.</param>
		/// <param name="culture">The culture.</param>
		/// <returns>A text version of the input value formatted using specified parameter.</returns>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			string formatString = parameter as string;
			return formatString == null ? value.ToString() : string.Format(culture, formatString, value);
		}

		/// <summary>
		/// Converts the object back to the original value.
		/// </summary>
		/// <param name="value">The value to be converted.</param>
		/// <param name="targetType">The target data type.</param>
		/// <param name="parameter">The formatting string.</param>
		/// <param name="culture">The culture.</param>
		/// <returns>The original value.</returns>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			// This method isn't required for string formatting.
			throw new NotImplementedException();
		}

	}

}
