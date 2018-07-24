namespace FluidTrade.Guardian.Windows
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Windows.Data;
	using System.Windows;
	using System.Globalization;

	/// <summary>
	/// Convert an integer depth to an thickness indent.
	/// </summary>
	public class IndentConverter : IValueConverter
	{

		/// <summary>
		/// Convert from an integer indent to a thickness.
		/// </summary>
		/// <param name="value">The indent level.</param>
		/// <param name="type">The target type (Thickness).</param>
		/// <param name="parameter">Optional Double parameter specifying indent width.</param>
		/// <param name="culture">The culture (ignored).</param>
		/// <returns>The equivalent thickness.</returns>
		public object Convert(object value, Type type, object parameter, CultureInfo culture)
		{

			return new Thickness((Int32)value * (parameter is Double? (Double)parameter : 19.0), 0, 0, 0);
	
		}

		/// <summary>
		/// Convert back not supported.
		/// </summary>
		/// <param name="o"></param>
		/// <param name="type"></param>
		/// <param name="parameter"></param>
		/// <param name="culture"></param>
		/// <returns></returns>
		public object ConvertBack(object o, Type type, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	
	}

}
