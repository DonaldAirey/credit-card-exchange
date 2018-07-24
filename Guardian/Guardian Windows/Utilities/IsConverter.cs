namespace FluidTrade.Guardian.Windows
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Windows.Data;

	/// <summary>
	/// A converter from an object to a Boolean by the result of an 'is' on a passed in type.
	/// </summary>
	public class IsConverter : IValueConverter
	{

		/// <summary>
		/// Determine whether an object is of a particular type and return the result.
		/// </summary>
		/// <param name="value">The object to check.</param>
		/// <param name="targetType">The target type (Boolean).</param>
		/// <param name="parameter">The type to check against.</param>
		/// <param name="culture">Ignored.</param>
		/// <returns></returns>
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{

			if (value != null)
			{

				Type type = parameter as Type;
				Type valueType = value.GetType();

				return type == null ? false : valueType == type || valueType.IsSubclassOf(type);

			}
			else
			{

				return false;

			}

		}

		/// <summary>
		/// Convert back not supported.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="targetType"></param>
		/// <param name="parameter"></param>
		/// <param name="culture"></param>
		/// <returns></returns>
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}

	}
}
