namespace FluidTrade.Core
{

	using System;
    using System.Collections.Generic;
    using System.Xml.Schema;

	/// <summary>
	/// Describes a data model.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public class AnnotationSchema : ObjectSchema
	{

		private List<Object> itemList;

		/// <summary>
		/// Constructs a schema from the contents of an XML specification.
		/// </summary>
		/// <param name="fileContents">The contents of a file that specifies the schema in XML.</param>
		public AnnotationSchema(DataModelSchema dataModelSchema, XmlSchemaAnnotation xmlSchemaAnnotation)
			: base(dataModelSchema, xmlSchemaAnnotation)
		{

			this.itemList = new List<Object>();

			foreach (XmlSchemaObject xmlSchemaObject in xmlSchemaAnnotation.Items)
			{
				if (xmlSchemaObject is XmlSchemaAppInfo)
					this.itemList.Add(new AppInfoSchema(dataModelSchema, xmlSchemaObject as XmlSchemaAppInfo));
			}

		}

		public List<Object> Items
		{
			get { return this.itemList; }
		}

	}

}
