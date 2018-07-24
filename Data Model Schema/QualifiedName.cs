namespace FluidTrade.Core
{

	using System;
    using System.Xml;

	public class QualifiedName
	{

		// Private Constants
		private const String xmlSchema = "http://www.w3.org/2001/XMLSchema";
		private const String msData = "urn:schemas-microsoft-com:xml-msdata";
		private const String msProp = "urn:schemas-microsoft-com:xml-msprop";
		private const String mtsData = "urn:schemas-fluidtrade-com:xml-mtsdata";

		// Public Static Fields
		public static XmlQualifiedName IsDataSet;
		public static XmlQualifiedName IsEncrypted;
		public static XmlQualifiedName IsPersistent;
		public static XmlQualifiedName AnyType;
		public static XmlQualifiedName PrimaryKey;
		public static XmlQualifiedName DataType;
		public static XmlQualifiedName AutoIncrement;
		public static XmlQualifiedName AutoIncrementSeed;
		public static XmlQualifiedName AutoIncrementStep;
		public static XmlQualifiedName UpdateRule;
		public static XmlQualifiedName DeleteRule;
		public static XmlQualifiedName ConstraintOnly;

		/// <summary>
		/// Create the static resources required by this class.
		/// </summary>
		static QualifiedName()
		{

			// Initialize the qualified names.
			QualifiedName.AnyType = new XmlQualifiedName("anyType", xmlSchema);
			QualifiedName.IsDataSet = new XmlQualifiedName("IsDataSet", msData);
			QualifiedName.PrimaryKey = new XmlQualifiedName("PrimaryKey", msData);
			QualifiedName.IsEncrypted = new XmlQualifiedName("IsEncrypted", mtsData);
			QualifiedName.IsPersistent = new XmlQualifiedName("IsPersistent", mtsData);
			QualifiedName.DataType = new XmlQualifiedName("DataType", msData);
			QualifiedName.AutoIncrement = new XmlQualifiedName("AutoIncrement", msData);
			QualifiedName.AutoIncrementSeed = new XmlQualifiedName("AutoIncrementSeed", msData);
			QualifiedName.AutoIncrementSeed = new XmlQualifiedName("AutoIncrementStep", msData);
			QualifiedName.UpdateRule = new XmlQualifiedName("UpdateRule", msData);
			QualifiedName.DeleteRule = new XmlQualifiedName("DeleteRule", msData);
			QualifiedName.ConstraintOnly = new XmlQualifiedName("ConstraintOnly", msData);

		}

	}

}
