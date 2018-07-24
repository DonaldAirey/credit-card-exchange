namespace FluidTrade.Guardian
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Data;
	using System.IO;
	using System.Reflection;

	/// <summary>
	/// Interface to streaming import readers.
	/// </summary>
	public abstract class ImportReader : IDisposable
	{

		/// <summary>
		/// Delegate used to report errors.
		/// </summary>
		/// <param name="recordIndex">The index into the file where the record resides.</param>
		/// <param name="record">The list of values representing the record.</param>
		/// <param name="errors">The list of errors that occurred.</param>
		public delegate void WriteFailedRecordDelegate(Int64 recordIndex, Dictionary<String,object> record, List<string> errors);

		/// <summary>
		/// The index into the DataSet where the table representing the root element is.
		/// </summary>
		private static Int32 RootElementTableIndex = 0;
		/// <summary>
		/// The index into the DataSet where the table of import records (should) reside.
		/// </summary>
		private static Int32 RecordTableIndex = 1;

		/// <summary>
		/// Create a new reader.
		/// </summary>
		/// <param name="translation">The translation scheme between the import file and the record, if any.</param>
		/// <param name="schema">The xsd schema for the import.</param>
		/// <param name="schemaVersion">The version of the xsd schema to look for.</param>
		/// <param name="recordType">The type of the records to create.</param>
		/// <param name="parameters">Additional parameters for the record.</param>
		/// <param name="writeFailedRecord">The function to report failed records with.</param>
		public ImportReader(Dictionary<String, String> translation, Stream schema, Int32 schemaVersion, Type recordType, Dictionary<String, object> parameters, WriteFailedRecordDelegate writeFailedRecord)
		{

			DataTable table;
			String rootElementName;

			this.Translation = translation;
			this.SchemaVersion = schemaVersion;
			this.Parameters = parameters;
			this.Properties = new Dictionary<String, PropertyInfo>();
			this.RecordIndex = 0;
			this.RecordType = recordType;
			this.WriteFailedRecord = writeFailedRecord;

			this.LoadSchema(schema, out table, out rootElementName);

			this.Table = table;
			this.RootElementName = rootElementName;

		}

		/// <summary>
		/// True if there are records left to retrieve.
		/// </summary>
		public abstract Boolean HasRecords { get; }

		/// <summary>
		/// The parameters to add to each object. The names and types of these parameters must match their counterparts in the RecordType. If the field in
		/// the RecordType is nullable, the type of the parameter must also be nullable.
		/// </summary>
		public Dictionary<string, object> Parameters { get; private set; }

		/// <summary>
		/// The type-specific position in the file.
		/// </summary>
		public abstract Int64 Position { get; }

		/// <summary>
		/// The properties that can be set in a record.
		/// </summary>
		protected Dictionary<String, PropertyInfo> Properties { get; private set; }

		/// <summary>
		/// The index of the next record.
		/// </summary>
		protected Int64 RecordIndex { get; set; }

		/// <summary>
		/// The type of the record to import with.
		/// </summary>
		protected Type RecordType { get; private set; }

		/// <summary>
		/// The name of the root element of the xsd.
		/// </summary>
		protected String RootElementName { get; set; }

		/// <summary>
		/// The current version of the schema the import is validated against.
		/// </summary>
		protected Int32 SchemaVersion { get; private set; }

		/// <summary>
		/// The type-specific size of the file.
		/// </summary>
		public abstract Int64 Size { get; }

		/// <summary>
		/// The table built from the xsd.
		/// </summary>
		protected DataTable Table { get; set; }

		/// <summary>
		/// The translation scheme between the import file and the record, if any.
		/// </summary>
		protected Dictionary<String, String> Translation { get; set; }

		/// <summary>
		/// The function to report errors with.
		/// </summary>
		protected WriteFailedRecordDelegate WriteFailedRecord { get; private set; }

		/// <summary>
		/// Create a new record and fill it with data.
		/// </summary>
		/// <param name="data">The data to fill the record with.</param>
		/// <returns>The populated record.</returns>
		protected object BuildRecord(Dictionary<String, object> data)
		{

			try
			{

				ConstructorInfo recordConstructor = this.RecordType.GetConstructor(new Type[0]);
				object record = recordConstructor.Invoke(new object[] { });

				// Fill in properties with parameters. We do these first so that they can contain defaults that are overwritten in the import file.
				foreach (string parameter in this.Parameters.Keys)
					this.Properties[parameter].SetValue(record, this.Parameters[parameter], null);
				// Fill in properties with fields in the row.
				foreach (string column in data.Keys)
					if (data[column] != null)
						if (data[column] is String)
							this.Properties[column].SetValue(record, (data[column] as String).Trim(), null);
						else
							this.Properties[column].SetValue(record, data[column], null);

				return record;

			}
			catch (TargetInvocationException exception)
			{

				throw exception.InnerException;

			}

		}

		/// <summary>
		/// Create an appropriate reader.
		/// </summary>
		/// <param name="filename">The filename of the import file.</param>
		/// <param name="translation">The translation scheme between the import file and the record, if any.</param>
		/// <param name="schema">The xsd schema for the import.</param>
		/// <param name="schemaVersion">The version of the xsd schema to look for.</param>
		/// <param name="recordType">The type of the records to create.</param>
		/// <param name="parameters">Additional parameters for the record.</param>
		/// <param name="writeFailedRecord">The function to report failed records with.</param>
		/// <returns>The reader for the file.</returns>
		public static ImportReader Create(String filename, Dictionary<String, String> translation, Stream schema, Int32 schemaVersion, Type recordType, Dictionary<String, object> parameters, WriteFailedRecordDelegate writeFailedRecord)
		{

			String extension = Path.GetExtension(filename);
			ImportReader reader = null;

			if (extension.Equals(".xml"))
				reader = new XmlImportReader(filename, translation, schema, schemaVersion, recordType, parameters, writeFailedRecord);
			else if (extension.Equals(".xlsx"))
				reader = new ExcelImportReader(filename, translation, schema, schemaVersion, recordType, parameters, writeFailedRecord);

			return reader;

		}

		/// <summary>
		/// Dispose of any resources.
		/// </summary>
		public virtual void Dispose()
		{

		}

		/// <summary>
		/// Add a report action for a failed record to a list.
		/// </summary>
		/// <param name="values">The record.</param>
		/// <param name="errors">The errors.</param>
		protected Action CreateReportAction(Dictionary<String, object> values, List<String> errors)
		{

			Int64 i = this.RecordIndex;
			Dictionary<String, object> v = new Dictionary<String, object>();
			List<String> e = errors.ToList();

			foreach (KeyValuePair<String, object> kvp in values)
				v.Add(kvp.Key, kvp.Value);

			// Make sure the dictionary we're writing out contains all of the columns we're looking for.
			foreach (KeyValuePair<String, String> translation in this.Translation)
				if (!v.ContainsKey(translation.Key))
					v[translation.Key] = "";

			return () => this.WriteFailedRecord(i, v, e);

		}

		/// <summary>
		/// Retrieve a batch of records from the file.
		/// </summary>
		/// <param name="batchSize">The maximum size of the batch.</param>
		/// <param name="succeeded">The records that were successfully parsed.</param>
		/// <param name="failed">Functions to report records that could not be parsed.</param>
		/// <returns>An array containing the records.</returns>
		public abstract void GetBatch(Int32 batchSize, out Array succeeded, out List<Action> failed);

		/// <summary>
		/// Retrieve the properties used to fill the record.
		/// </summary>
		/// <param name="table">The table that contains the schema for the data.</param>
		/// <returns>The properties.</returns>
		private Dictionary<String, PropertyInfo> GetRecordProperties(DataTable table)
		{

			Dictionary<string, PropertyInfo> properties = new Dictionary<string, PropertyInfo>();

			// Retrieve the properties that will be filled by parameters.
			foreach (string parameter in this.Parameters.Keys)
			{

				properties[parameter] = this.RecordType.GetProperty(parameter, this.Parameters[parameter].GetType());

				if (properties[parameter] == null)
					throw new Exception("Unable to find the property named " + parameter);

			}
			// Retrieve the properties that will be filled by columns in the table.
			foreach (DataColumn column in table.Columns)
			{
				Type type = column.DataType;

				if (column.AllowDBNull && type.IsValueType)
					type = typeof(Nullable<>).MakeGenericType(type);

				properties[column.ColumnName] = this.RecordType.GetProperty(column.ColumnName, type);

				if (properties[column.ColumnName] == null)
					//throw new Exception("Unable to find the property named " + column.ColumnName);
					properties.Remove(column.ColumnName);

			}

			return properties;

		}

		/// <summary>
		/// Load the XSD into a dataset, get the datatable that contains the record schema, and get the name of the root element.
		/// </summary>
		/// <param name="schema">A stream containg the xsd to build the dataset from.</param>
		/// <param name="table">The datatable containing the record schema.</param>
		/// <param name="rootElement">The name of the root element in the xml file.</param>
		private void LoadSchema(Stream schema, out DataTable table, out String rootElement)
		{

			try
			{

				DataSet data = new DataSet();

				schema.Seek(0, SeekOrigin.Begin);
				data.ReadXmlSchema(schema);

				table = data.Tables[ImportReader.RecordTableIndex];

				rootElement = data.Tables[ImportReader.RootElementTableIndex].TableName;

				// Adding attributes to the base element added a table (that would contain those elements).
				this.Properties = this.GetRecordProperties(table);

			}
			finally
			{


				schema.Dispose();

			}

		}

		/// <summary>
		/// Convert a record into a dictionary of columns and values using the column names in the translation table.
		/// </summary>
		/// <param name="record">The record to detranslate.</param>
		/// <returns>The detranslated record.</returns>
		public Dictionary<String, object> DetranslateRecord(object record)
		{

			Dictionary<String, object> values = new Dictionary<String, object>();

			foreach (String column in this.Translation.Keys)
			{

				PropertyInfo property = record.GetType().GetProperty(this.Translation[column]);

				values[column] = property.GetValue(record, new object[0]);

			}

			return values;

		}

	}

}
