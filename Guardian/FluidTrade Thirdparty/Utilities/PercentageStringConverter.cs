namespace FluidTrade.Thirdparty
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Data;

	class PercentageStringConverter : IValueConverter 
    {
        #region IValueConverter Members

		/// <summary>
		/// Convert to percentage
		/// </summary>
		/// <param name="value"></param>
		/// <param name="targetType"></param>
		/// <param name="parameter"></param>
		/// <param name="culture"></param>
		/// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Decimal)
            {
				Decimal valueToConvert = (Decimal)value;
				return String.Format("{0:0%}", value);
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null; 
        }

        #endregion
    }
}
