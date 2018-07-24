namespace FluidTrade.Guardian
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Xml;
	using System.Xml.Schema;
	using System.IO;
	using System.Data;

	/// <summary>
	/// An xml import reader.
	/// </summary>
	public class XmlImportReader : ImportReader
	{

		private FileStream file;
		private XmlReader xml;

		/// <summary>
		/// Create a new reader.
		/// </summary>
		/// <param name="filename">The xml file to read.</param>
		/// <param name="translation">The translation scheme between the import file and the record, if any.</param>
		/// <param name="schema">The xsd schema for the import.</param>
		/// <param name="schemaVersion">The version of the xsd schema to look for.</param>
		/// <param name="recordType">The type of the records to create.</param>
		/// <param name="parameters">Additional parameters for the record.</param>
		/// <param name="writeFailedRecord">The function to report failed records with.</param>
		public XmlImportReader(String filename, Dictionary<String, String> translation, Stream schema, Int32 schemaVersion, Type recordType, Dictionary<String, object> parameters, WriteFailedRecordDelegate writeFailedRecord)
			: base(translation, schema, schemaVersion, recordType, parameters, writeFailedRecord)
		{

			XmlReaderSettings settings = new XmlReaderSettings();

			settings.ValidationFlags = XmlSchemaValidationFlags.ReportValidationWarnings;
			settings.ValidationType = ValidationType.None;
			settings.ValidationEventHandler += this.OnValidationError;
			settings.IgnoreWhitespace = true;

			this.file = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
			this.xml = XmlReader.Create(this.file, settings);

			this.ReadStart();

		}
		/// <summary>
		/// True if there are records left to retrieve.
		/// </summary>
		public override Boolean HasRecords
		{
			get { return this.xml.IsStartElement(); }
		}

		/// <summary>
		/// The type-specific position in the file.
		/// </summary>
		public override Int64 Position
		{
			get { return this.file.Position; }
		}

		/// <summary>
		/// The type-specific size of the file.
		/// </summary>
		public override Int64 Size
		{
			get { return this.file.Length; }
		}

		/// <summary>
		/// Dispose of the streams we're using.
		/// </summary>
		public override void Dispose()
		{

			base.Dispose();

			// Read the closing tag of the base element.
			xml.ReadEndElement();

			xml.Close();

		}

		/// <summary>
		/// Retrieve a batch of records from the file.
		/// </summary>
		/// <param name="batchSize">The maximum size of the batch.</param>
		/// <param name="bulk">The records that were successfully parsed.</param>
		/// <param name="failed">Functions to report records that could not be parsed.</param>
		/// <returns>An array containing the records.</returns>
		public override void GetBatch(Int32 batchSize, out Array bulk, out List<Action> failed)
		{

			Dictionary<String, object> values = new Dictionary<String, object>();
			int bulkIndex = 0;

			bulk = Array.CreateInstance(this.RecordType, batchSize);
			failed = new List<Action>();

			// Read throught the records.
			while (this.xml.IsStartElement())
			{

				List<String> errors = new List<String>();

				this.RecordIndex += 1;
				values.Clear();

				// Read the opening tag of a record element.
				this.xml.ReadStartElement();

				while (this.xml.IsStartElement())
				{

					String columnName = this.xml.LocalName;
					String text = null;

					try
					{

						DataColumn column = this.Table.Columns[columnName];

						// If we got the column type, then the element name is in the schema.
						if (column != null)
						{

							Type columnType = column.DataType;
							object value;

							text = this.xml.Value;
							value = this.xml.ReadElementContentAs(this.Table.Columns[columnName].DataType, null);
							values[columnName] = value;

							if (values[columnName] is String)
								values[columnName] = (values[columnName] as String).Trim();

						}
						// Otherwise we need to skip this element and complain about it.
						else
						{

							this.xml.ReadElementContentAsString();
							throw new XmlSchemaValidationException(String.Format("The record field '{0}' is not in the schema", columnName));

						}

					}
					catch (Exception exception)
					{

						try
						{

							while (!this.xml.IsStartElement())
								this.xml.Read();

						}
						catch { }

						values[columnName] = text;
						errors.Add(String.Format("{0}: {1}", columnName, exception.Message));

					}

				}

				// Read the closing tag of a record element.
				xml.ReadEndElement();

				foreach (DataColumn column in this.Table.Columns)
					if (!column.AllowDBNull && (!values.ContainsKey(column.ColumnName) || values[column.ColumnName] == null))
						errors.Add(String.Format(Guardian.Properties.Resources.ValidateNonNullColumnFailed, column.ColumnName));

				if (errors.Count == 0)
				{

					// Add the new record to the this bulk of records.
					bulk.SetValue(this.BuildRecord(values), bulkIndex);
					bulkIndex += 1;

				}
				else
				{

					failed.Add(this.CreateReportAction(values, errors));

				}

				// If we've filled up our 
				if (bulkIndex >= batchSize)
					break;

			}

			if (bulkIndex < batchSize)
			{

				Array smallBulk = Array.CreateInstance(this.RecordType, bulkIndex);
				Array.Copy(bulk, smallBulk, bulkIndex);
				bulk = smallBulk;

			}

		}

		/// <summary>
		/// Throw validation errors so the reader can catch them.
		/// </summary>
		/// <param name="sender">The originator of the error.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnValidationError(object sender, ValidationEventArgs eventArgs)
		{

			throw eventArgs.Exception;

		}

		/// <summary>
		/// Read the start of the xml file.
		/// </summary>
		private void ReadStart()
		{

			// Move to the opening tag of base element.
			if (!xml.ReadToFollowing(this.RootElementName) || xml.EOF)
				throw new XmlSchemaValidationException(
					String.Format("Expected root element, '{0}', not found", this.RootElementName),
					null,
					(xml as IXmlLineInfo).LineNumber,
					0);

			// Find out what version of the schema this file was built against.
			if (xml.MoveToFirstAttribute())
				do
				{

					if (xml.LocalName == "SchemaVersion")
						if (Int32.Parse(xml.Value) != this.SchemaVersion)
							throw new FormatException("Import in wrong format version");

				} while (xml.MoveToNextAttribute());
			else
				throw new FormatException("Import file is not versioned");

			// Eat the opening tag of the base element.
			xml.ReadStartElement();

		}

	}

}
