namespace FluidTrade.Thirdparty
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Data;

    class PercentageConverter : IValueConverter 
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Decimal)
            {
                Decimal valueToConvert = (Decimal)value;
                return valueToConvert * 100m;
            }
            return 0m;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
			if (value is Decimal)
			{
				Decimal valueToConvert = (Decimal)value;
				return valueToConvert / 100m;
			}
			return 0m;
		}
    }
}
