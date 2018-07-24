namespace FluidTrade.Core
{

	using System;
    using System.Xml;
	using System.Xml.Schema;

	/// <summary>
	/// Describes a data model.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public class AppInfoSchema : ObjectSchema
	{

		private String source;
		private XmlNode[] markup;

		/// <summary>
		/// Constructs a schema from the contents of an XML specification.
		/// </summary>
		/// <param name="fileContents">The contents of a file that specifies the schema in XML.</param>
		public AppInfoSchema(DataModelSchema dataModelSchema, XmlSchemaAppInfo xmlSchemaAppInfo)
			: base(dataModelSchema, xmlSchemaAppInfo)
		{

			this.source = xmlSchemaAppInfo.Source;
			this.markup = xmlSchemaAppInfo.Markup;

		}

		public XmlNode[] Markup
		{
			get { return this.markup; }
		}

		public String Source
		{
			get { return this.source; }
		}

	}

}
