namespace FluidTrade.ExpressionEvaluation
{

	using System;
	using System.Globalization;

	/// <include file='Resources/DocComments.xml' path='DocComments/ExpressionParserOptions/Class/*' /> 
	public class ExpressionParserOptions
	{

		private PropertyDictionary MyProperties;
		private ExpressionContext MyOwner;
		private CultureInfo MyParseCulture;
		private const NumberStyles numberStyles = NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent;

		internal ExpressionParserOptions(ExpressionContext owner)
		{
			MyOwner = owner;
			MyProperties = new PropertyDictionary();
			MyParseCulture = CultureInfo.InvariantCulture.Clone() as CultureInfo;

			this.InitializeProperties();
		}

		#region "Methods - Public"

		/// <include file='Resources/DocComments.xml' path='DocComments/ExpressionParserOptions/RecreateParser/*' /> 
		public void RecreateParser()
		{
			MyOwner.RecreateParser();
		}

		#endregion

		#region "Methods - Internal"

		internal ExpressionParserOptions Clone()
		{
			ExpressionParserOptions copy = this.MemberwiseClone() as ExpressionParserOptions;
			copy.MyProperties = MyProperties.Clone();
			return copy;
		}

		internal double ParseDouble(string image)
		{
			return double.Parse(image, numberStyles, MyParseCulture);
		}

		internal float ParseSingle(string image)
		{
			return float.Parse(image, numberStyles, MyParseCulture);
		}
		#endregion

		#region "Methods - Private"

		private void InitializeProperties()
		{
			this.DateTimeFormat = "dd/MM/yyyy";
			this.RequireDigitsBeforeDecimalPoint = false;
			this.DecimalSeparator = '.';
			this.FunctionArgumentSeparator = ',';
		}

		#endregion

		#region "Properties - Public"

		/// <include file='Resources/DocComments.xml' path='DocComments/ExpressionParserOptions/DateTimeFormat/*' /> 
		public string DateTimeFormat
		{
			get { return MyProperties.GetValue<string>("DateTimeFormat"); }
			set { MyProperties.SetValue("DateTimeFormat", value); }
		}

		/// <include file='Resources/DocComments.xml' path='DocComments/ExpressionParserOptions/RequireDigitsBeforeDecimalPoint/*' /> 
		public bool RequireDigitsBeforeDecimalPoint
		{
			get { return MyProperties.GetValue<bool>("RequireDigitsBeforeDecimalPoint"); }
			set { MyProperties.SetValue("RequireDigitsBeforeDecimalPoint", value); }
		}

		/// <include file='Resources/DocComments.xml' path='DocComments/ExpressionParserOptions/DecimalSeparator/*' /> 
		public char DecimalSeparator
		{
			get { return MyProperties.GetValue<char>("DecimalSeparator"); }
			set
			{
				MyProperties.SetValue("DecimalSeparator", value);
				MyParseCulture.NumberFormat.NumberDecimalSeparator = Convert.ToString(value);
			}
		}

		/// <include file='Resources/DocComments.xml' path='DocComments/ExpressionParserOptions/FunctionArgumentSeparator/*' /> 
		public char FunctionArgumentSeparator
		{
			get { return MyProperties.GetValue<char>("FunctionArgumentSeparator"); }
			set { MyProperties.SetValue("FunctionArgumentSeparator", value); }
		}

		#endregion

	}

}