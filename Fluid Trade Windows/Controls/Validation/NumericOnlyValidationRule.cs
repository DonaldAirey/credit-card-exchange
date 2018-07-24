using System;
using System.Windows.Controls;

namespace FluidTrade.Core.Windows.Controls
{
	/// <summary>
	/// Numeric  validation rule
	/// </summary>
    class NumericOnlyValidationRule : System.Windows.Controls.ValidationRule
    {
		/// <summary>
		/// Validae 
		/// </summary>
		/// <param name="value"></param>
		/// <param name="cultureInfo"></param>
		/// <returns></returns>
        public override System.Windows.Controls.ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            string _strInt = value.ToString();
            int _int = -1;
            if (!Int32.TryParse(_strInt, out _int))
                return new ValidationResult(false, "Value must be an integer");            
            return new ValidationResult(true, null);
        }
    }
}
