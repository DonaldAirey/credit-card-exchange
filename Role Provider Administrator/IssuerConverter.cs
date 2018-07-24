namespace FluidTrade.Guardian
{

	using System;
	using System.Globalization;
    using System.Security.Cryptography.X509Certificates;
    using System.Windows.Data;

	/// <summary>
	/// Converts a X509Certificate2 to a human readable string.
	/// </summary>
	[ValueConversion(typeof(X509Certificate2), typeof(string))]
	public class IssuerConverter : IValueConverter
	{

		/// <summary>
		/// Convert the X509Certificate2 to a string.
		/// </summary>
		/// <param name="value">The source value.</param>
		/// <param name="targetType">The target type.</param>
		/// <param name="parameter">The sour</param>
		/// <param name="culture">The culture for the destination value.</param>
		/// <returns>The IssuerName in a readable form from the X509Certificate2.</returns>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{

			// This will extract the human readable name from the X500 method of encoding tags.
			X509Certificate2 x509Certificate2 = (X509Certificate2)value;
			return x509Certificate2.GetNameInfo(X509NameType.SimpleName, true);

		}

		/// <summary>
		/// Convert the a string back to an X509Certificate2.
		/// </summary>
		/// <param name="value">The source value.</param>
		/// <param name="targetType">The target type.</param>
		/// <param name="parameter">The sour</param>
		/// <param name="culture">The culture for the destination value.</param>
		/// <returns>The IssuerName in a readable form from the X509Certificate2.</returns>
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
	}

}
