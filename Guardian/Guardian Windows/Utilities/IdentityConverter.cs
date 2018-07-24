namespace FluidTrade.Guardian.Windows
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Windows.Data;

	/// <summary>
	/// This no-frills converter is intended for "converting" between different classes in a class hierarchy.
	/// </summary>
	public class IdentityConverter : IValueConverter
	{

		/// <summary>
		/// Convert from one kind of object to another.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <param name="targetType">The target type.</param>
		/// <param name="parameter">The optional parameter.</param>
		/// <param name="culture">The culture to do the conversion in.</param>
		/// <returns>The value itself.</returns>
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{

			return value;

		}

		/// <summary>
		/// Convert from one kind of object to another.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <param name="targetType">The target type.</param>
		/// <param name="parameter">The optional parameter.</param>
		/// <param name="culture">The culture to do the conversion in.</param>
		/// <returns>The value itself.</returns>
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{

			return value;

		}

	}

}
