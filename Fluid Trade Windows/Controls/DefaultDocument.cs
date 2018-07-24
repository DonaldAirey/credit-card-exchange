namespace FluidTrade.Core.Windows.Controls
{

	using System;
	using System.Windows;
	using System.Windows.Media;

	/// <summary>Default values for the Report and spreadsheet.</summary>
	/// <copyright>Copyright © 2006 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public class DefaultDocument
	{

		public static System.Boolean IsProtected = false;
		public static System.Int32 Steps = 10;
		public static System.Object Value = DBNull.Value;
		public static System.Double RowHeight = 17.0;
		public static System.Double ColumnWidth = 64.0;
		public static System.Double HeaderHeight = 20.0;
		public static System.Double ScaleFactor = 1.0;
		public static System.Double BorderWidth = 1.0;
		public static System.Double FontSize = 8.25;
		public static System.String Format = "{0:}";
		public static System.String DataType = "string";
		public static System.Windows.FontStyle FontStyle = FontStyles.Normal;
		public static System.Windows.FontWeight FontWeight = FontWeights.Regular;
		public static System.Windows.FontStretch FontStretch = FontStretches.Normal;
		public static System.Windows.HorizontalAlignment HorizontalAlignment = HorizontalAlignment.Left;
		public static System.Windows.Media.Color ForeColor = Colors.Black;
		public static System.Windows.Media.Color BackColor = Colors.White;
		public static System.Windows.Media.Color BorderColor = Colors.DarkGray;
		public static System.Windows.Media.Color StartColor = Colors.Black;
		public static System.Windows.Media.FontFamily FontFamily = new FontFamily("Segoe UI");
		public static System.Windows.TextAlignment TextAlignment = TextAlignment.Left;
		public static System.Windows.TextDecorationCollection TextDecorations = new TextDecorationCollection();
		public static System.Windows.VerticalAlignment VerticalAlignment = VerticalAlignment.Center;

	}
}
