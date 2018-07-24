using System.Windows.Controls;

namespace FluidTrade.Core.Windows.Controls
{
    public class LengthInRangeRule : ValidationRule   
    {
        public LengthInRangeRule()
        {
            Name = "Control";
        }

        public string Name { get; set; }
        public int Max { get; set; }
        public int Min { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="cultureInfo"></param>
        /// <returns></returns>
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            int length = ((string)value).Length;
            if ((length < Min) || (length > Max))
            {
                return new ValidationResult(false,
                  "Length of " + Name + " has to be between " + Min + " and " + Max + " characters.");
            }
            else
            {
                return new ValidationResult(true, null);
            }
        }
    }
}
