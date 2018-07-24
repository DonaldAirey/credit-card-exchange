namespace FluidTrade.PresentationGenerator
{

	using System;
	using System.Xml;

    /// <summary>
	/// Creates the namespace FluidTrade.Core.Windows.Controls
	/// </summary>
	public class QualifiedName
	{

		// Public Static Fields
		public static XmlQualifiedName AnyType;
		public static XmlQualifiedName Arguments;
		public static XmlQualifiedName Argument;
		public static XmlQualifiedName Comparer;
		public static XmlQualifiedName Constructor;
		public static XmlQualifiedName DataType;
		public static XmlQualifiedName DestinationNamespace;
		public static XmlQualifiedName GeneratedCode;
		public static XmlQualifiedName KeySelector;
		public static XmlQualifiedName KeyType;
		public static XmlQualifiedName Namespace;
		public static XmlQualifiedName OrderBy;
		public static XmlQualifiedName Predicate;
		public static XmlQualifiedName Property;
		public static String ReportUri = "urn:schemas-fluidtrade-com:report";
		public static XmlQualifiedName ResultType;
		public static String SchemaUri = "http://www.w3.org/2001/XMLSchema";
		public static XmlQualifiedName Selector;
		public static XmlQualifiedName Select;
		public static XmlQualifiedName Setter;
		public static XmlQualifiedName Snippet;
		public static XmlQualifiedName Source;
		public static XmlQualifiedName SourceType;
		public static XmlQualifiedName Using;
		public static XmlQualifiedName Value;
		public static XmlQualifiedName VariableName;
		public static XmlQualifiedName VariableType;
		public static XmlQualifiedName Where;

		/// <summary>
		/// Create the static resources required by this class.
		/// </summary>
		static QualifiedName()
		{

			// Initialize the qualified names.
			QualifiedName.AnyType = new XmlQualifiedName("anyType", SchemaUri);
			QualifiedName.Argument = new XmlQualifiedName("argument", ReportUri);
			QualifiedName.Arguments = new XmlQualifiedName("arguments", ReportUri);
			QualifiedName.Comparer = new XmlQualifiedName("comparer");
			QualifiedName.Constructor = new XmlQualifiedName("constructor", ReportUri);
			QualifiedName.DataType = new XmlQualifiedName("datatype", ReportUri);
			QualifiedName.DestinationNamespace = new XmlQualifiedName("destinationNamespace");
			QualifiedName.KeySelector = new XmlQualifiedName("keySelector");
			QualifiedName.KeyType = new XmlQualifiedName("keyType");
			QualifiedName.GeneratedCode = new XmlQualifiedName("generatedCode", ReportUri);
			QualifiedName.Namespace = new XmlQualifiedName("namespace");
			QualifiedName.OrderBy = new XmlQualifiedName("orderBy", ReportUri);
			QualifiedName.Predicate = new XmlQualifiedName("predicate");
			QualifiedName.Property = new XmlQualifiedName("property");
			QualifiedName.ResultType = new XmlQualifiedName("resultType");
			QualifiedName.Select = new XmlQualifiedName("select", ReportUri);
			QualifiedName.Selector = new XmlQualifiedName("selector");
			QualifiedName.Setter = new XmlQualifiedName("setter", ReportUri);
			QualifiedName.Snippet = new XmlQualifiedName("snippet", ReportUri);
			QualifiedName.Source = new XmlQualifiedName("source");
			QualifiedName.SourceType = new XmlQualifiedName("sourceType");
			QualifiedName.Using = new XmlQualifiedName("using", ReportUri);
			QualifiedName.Value = new XmlQualifiedName("value");
			QualifiedName.VariableName = new XmlQualifiedName("name");
			QualifiedName.VariableType = new XmlQualifiedName("type");
			QualifiedName.Where = new XmlQualifiedName("where", ReportUri);

		}

	}

}
