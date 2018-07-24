namespace FluidTrade.Guardian.Windows
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Windows.Data;

	/// <summary>
	/// Convert between a nullable value and its underlying type.
	/// </summary>
	public class NullableConverter : IValueConverter
	{

		private static readonly Dictionary<Type, object> Defaults = new Dictionary<Type, object>
			{
				{ typeof(Guid), Guid.Empty },
				{ typeof(DateTime), DateTime.MinValue },
			};

		private object ToNullable(Type nullableType, object value, object def)
		{

			if (value.Equals(def))
				return null;
			else
				return nullableType.GetConstructor(new Type[] { Nullable.GetUnderlyingType(nullableType) }).Invoke(new object[] { value });

		}

		private object FromNullable(Type nullableType, object value, object def)
		{

			if (value == null)
				return def;
			else
				return nullableType.GetProperty("Value").GetGetMethod().Invoke(value, new object[0]);

		}

		/// <summary>
		/// Convert between a nullable type and its underlying type.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <param name="targetType">The target type - either a nullable type or its undylying type.</param>
		/// <param name="parameter">Ignored.</param>
		/// <param name="culture">Ignored.</param>
		/// <returns>The converted value.</returns>
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{

			if (targetType == typeof(Nullable<>) || targetType == typeof(object) && value.GetType().IsValueType)
				return ToNullable(typeof(Nullable<>).MakeGenericType(value.GetType()), value, Defaults[value.GetType()]);
			else
				return FromNullable(typeof(Nullable<>).MakeGenericType(targetType), value, Defaults[targetType]);

		}

		/// <summary>
		/// Convert between a nullable type and its underlying type.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <param name="targetType">The target type - either a nullable type or its undylying type.</param>
		/// <param name="parameter">Ignored.</param>
		/// <param name="culture">Ignored.</param>
		/// <returns>The converted value.</returns>
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{

			return this.Convert(value, targetType, parameter, culture);

		}

	}

}
