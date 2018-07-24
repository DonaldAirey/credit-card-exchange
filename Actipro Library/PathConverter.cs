namespace FluidTrade.Actipro 
{
    using System;
    using System.Globalization;
    using System.Text;
    using System.Xml;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Media.Imaging;

	/// <summary>
	/// Converts an <see cref="XmlElement"/> to and from a path.
	/// </summary>
	[ValueConversion(typeof(XmlElement), typeof(string))]
	public class PathConverter : FrameworkElement, IValueConverter {

        /// <summary>
        /// The root of path conversions.
        /// </summary>
        public static readonly DependencyProperty RootProperty;

        static PathConverter()
        {

            RootProperty = DependencyProperty.Register("Root", typeof(FolderNavNode), typeof(PathConverter));

        }

        /// <summary>
        /// The root of path conversions.
        /// </summary>
        public FolderNavNode Root
        {
            get { return this.GetValue(RootProperty) as FolderNavNode; }
            set { this.SetValue(RootProperty, value); }
        }

		/// <summary>
		/// Converts a value.
		/// </summary>
		/// <param name="value">The value produced by the binding source.</param>
		/// <param name="targetType">The type of the binding target property.</param>
		/// <param name="parameter">The converter parameter to use.</param>
		/// <param name="culture">The culture to use in the converter.</param>
		/// <returns>
		/// A converted value. If the method returns null, the valid null value is used.
		/// </returns>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return ConvertItemHelper.GetPath(this.Root, value);
		}

		/// <summary>
		/// This method always returns  <see langword="null"/> and should not be used.
		/// </summary>
		/// <param name="value">Not used.</param>
		/// <param name="targetType">Not used.</param>
		/// <param name="parameter">Not used.</param>
		/// <param name="culture">Not used.</param>
		/// <returns> <see langword="null"/>.</returns>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			return null;
		}

	}

}
