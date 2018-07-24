namespace FluidTrade.Thirdparty
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Windows.Data;

	/// <summary>
	/// A converter for innverting boolean values.
	/// </summary>
	public class NotConverter : IValueConverter
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

			if (targetType != typeof(Boolean))
				throw new InvalidCastException("NotConverter can only be applied to boolean values");

			return !(Boolean)value;

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

			if (targetType != typeof(Boolean))
				throw new InvalidCastException("NotConverter can only be applied to boolean values");

			return !(Boolean)value;

		}

	}

}
