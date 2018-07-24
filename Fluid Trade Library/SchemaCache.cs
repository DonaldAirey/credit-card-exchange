namespace FluidTrade.Core
{

	using System;
    using System.Data;
	using System.IO;
    using System.Xml.Schema;

	/// <summary>
	/// A collection of schemas.
	/// </summary>
	public class SchemaCache
	{

		// Public Instance Fields
		public System.Data.DataTable DataTable;
		public System.Data.DataColumn UriColumn;
		public System.Data.DataColumn XmlDocumentColumn;

		// Private Instance Fields
		private System.Data.DataView dataView;

		/// <summary>
		/// Create a collection of schemas with a default cache.
		/// </summary>
		public SchemaCache()
		{

			// This provides a data table that can be used to cache the schemas and access them by a URI.
			this.DataTable = new DataTable();

			// This column holds the unique identifier for the schemas in the table.
			this.UriColumn = new DataColumn("Uri", typeof(Uri));
			this.DataTable.Columns.Add(this.UriColumn);
			this.DataTable.Constraints.Add("primaryKey", this.UriColumn, true);

			// This is the source XML for the schema.
			this.XmlDocumentColumn = new DataColumn("XmlDocument", typeof(string));
			this.DataTable.Columns.Add(this.XmlDocumentColumn);

			// Create a view for finding the records based on the URI specification.
			this.dataView = new DataView(this.DataTable);
			this.dataView.Sort = "Uri";
			this.dataView.RowStateFilter = System.Data.DataViewRowState.CurrentRows;

		}

		/// <summary>
		/// Create a collection of schemas with an existing table.
		/// </summary>
		/// <param name="dataTable"></param>
		public SchemaCache(DataTable dataTable)
		{

			// Initialize the object.
			this.DataTable = dataTable;
			this.UriColumn = this.DataTable.Columns["Uri"];
			this.XmlDocumentColumn = this.DataTable.Columns["XmlDocument"];

			// Create a view for finding the records based on the URI specification.
			this.dataView = new DataView(this.DataTable);
			this.dataView.Sort = "Uri";
			this.dataView.RowStateFilter = System.Data.DataViewRowState.CurrentRows;

		}

		/// <summary>
		/// Adds a schema to the cache.
		/// </summary>
		/// <param name="uri"></param>
		/// <param name="document"></param>
		public void Add(Uri uri, string document)
		{

			// Create a new entry in the table of the XML document and use the URI as a unique index to that schema.
			DataRow dataRow = this.DataTable.NewRow();
			dataRow[this.UriColumn] = uri;
			dataRow[this.XmlDocumentColumn] = document;
			this.DataTable.Rows.Add(dataRow);

		}

		public XmlSchemaSet Compile(Uri uri)
		{

			XmlSchemaSet xmlSchemaSet = new XmlSchemaSet();
			xmlSchemaSet.XmlResolver = new DatasetResolver(this.DataTable, this.dataView);
			int index = this.dataView.Find(uri);
			if (index != -1)
			{
				DataRow dataRow = this.dataView[index].Row;
				StringReader stringReader = new StringReader(dataRow[this.XmlDocumentColumn] as string);
				xmlSchemaSet.Add(XmlSchema.Read(stringReader, null));
				xmlSchemaSet.Compile();
			}

			return xmlSchemaSet;

		}

	}

}
