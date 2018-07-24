namespace FluidTrade.Guardian
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DocumentFormat.OpenXml.Packaging;
	using DocumentFormat.OpenXml.Spreadsheet;
	using System.IO;
	using System.Data;
	using System.Reflection;
	using System.Text.RegularExpressions;

	/// <summary>
	/// An Excel import reader.
	/// </summary>
	public class ExcelImportReader : ImportReader
	{

		/// <summary>
		/// The maximum number of spreadsheet rows to search for column headers before giving up.
		/// </summary>
		private static Int32 MaxLeadingRows = 100;

		/// <summary>
		/// Possible values of a boolean field. Includes 'true' and 'false' because we're already doing the check for 'yes' and 'no': might as well
		/// go all the way.
		/// </summary>
		private static readonly Dictionary<String, Boolean> booleanValues = new Dictionary<String, Boolean>
		{
			{ "true", true },
			{ "false", false },
			{ "yes", true },
			{ "no", false},
		};

		private Dictionary<Int32, String> columnLocations = new Dictionary<Int32, String>();
		private String filename;
		private Boolean hasRecords;
		private Dictionary<Int32, String> originalColumnLocations = new Dictionary<Int32, String>();
		private IEnumerator<Row> rows;
		private Int64 size;
		private SharedStringItem[] sharedItemsTable;
		private SpreadsheetDocument spreadsheet;
		private Dictionary<String, String> translation;
		private IEnumerator<WorksheetPart> worksheetParts;

		/// <summary>
		/// Create a new reader.
		/// </summary>
		/// <param name="filename">The excel file to read.</param>
		/// <param name="translation">The translation scheme between the import file and the record, if any.</param>
		/// <param name="schema">The xsd schema for the import.</param>
		/// <param name="schemaVersion">The version of the xsd schema to look for.</param>
		/// <param name="recordType">The type of the records to create.</param>
		/// <param name="parameters">Additional parameters for the record.</param>
		/// <param name="writeFailedRecord">The function to report failed records with.</param>
		public ExcelImportReader(String filename, Dictionary<String, String> translation, Stream schema, Int32 schemaVersion, Type recordType, Dictionary<String, object> parameters, WriteFailedRecordDelegate writeFailedRecord)
			: base(translation, schema, schemaVersion, recordType, parameters, writeFailedRecord)
		{

			WorkbookPart workbook;
			SharedStringTablePart shareStringPart;

			this.filename = filename;
			this.translation = translation;

			this.spreadsheet = SpreadsheetDocument.Open(filename, false);
			workbook = spreadsheet.WorkbookPart;
			shareStringPart = workbook.GetPartsOfType<SharedStringTablePart>().First();
			
			this.sharedItemsTable = shareStringPart.SharedStringTable.Elements<SharedStringItem>().ToArray();
			this.worksheetParts = workbook.WorksheetParts.GetEnumerator();

			foreach (WorksheetPart worksheet in workbook.WorksheetParts)
				this.size += worksheet.Worksheet.Descendants<Row>().Count();

			this.ReadStart();

		}

		/// <summary>
		/// True if there are records left to retrieve.
		/// </summary>
		public override Boolean HasRecords
		{
			get { return this.hasRecords; }
		}

		/// <summary>
		/// The type-specific position in the file.
		/// </summary>
		public override Int64 Position
		{
			get { return this.RecordIndex; }
		}

		/// <summary>
		/// The type-specific size of the file.
		/// </summary>
		public override Int64 Size
		{
			get { return this.size; }
		}

		/// <summary>
		/// Dispose of the streams we're using.
		/// </summary>
		public override void Dispose()
		{

			base.Dispose();

			this.spreadsheet.Close();
			this.spreadsheet.Dispose();

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
			Dictionary<String, object> originalValues = new Dictionary<String, object>();
			List<String> errors = new List<String>();
			Int32 bulkIndex = 0;
			Type columnType;
			Int32 cellIndex;

			bulk = Array.CreateInstance(this.RecordType, batchSize);
			failed = new List<Action>();

			while (this.HasRecords)
			{
				Row row = this.rows.Current;

				this.RecordIndex += 1;
				originalValues.Clear();
				values.Clear();
				errors.Clear();

				foreach (Cell cell in row.Descendants<Cell>())
				{

					object cellValue = this.GetExcelCellValue(sharedItemsTable, cell);

					if (cellValue != null && !(cellValue is String && cellValue.Equals("")))
					{

						cellIndex = this.GetExcelColumnIndex(cell.CellReference);

						if (columnLocations.ContainsKey(cellIndex))
						{

							originalValues[originalColumnLocations[cellIndex]] = cellValue;

						}

					}

				}

				foreach (KeyValuePair<String,object> originalColumn in originalValues)
				{

					try
					{

						object value = originalColumn.Value;

						if (value != null)
						{

							String column = this.translation[originalColumn.Key];

							columnType = this.Table.Columns[column].DataType;

							// If the cell is empty, skip it.
							if (value is String && String.IsNullOrEmpty(value as String))
								continue;
							// If the cell's contents are a string, try to parse it as the type the record is expecting.
							else if (value is String && columnType != typeof(String))
								value = this.ParseString(columnType, value as String);
							// If the cell isn't a string, make sure it's the same type as what the record is expecting.
							else if (value.GetType() != columnType)
								errors.Add(
									String.Format(
										Guardian.Properties.Resources.ExcelTypeValidationFailed,
										columnType,
										value.GetType(),
										this.RecordIndex));

							values[column] = value;

						}

					}
					catch (Exception exception)
					{

						errors.Add(String.Format("{0}: {1}", originalColumn, exception.Message));

					}

				}

				this.MoveNext();

				// If we didn't read anything for this row, just toss it out altogether.
				if (values.Count == 0)
					continue;

				if (originalValues.Count > 0)
				{

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

						failed.Add(this.CreateReportAction(originalValues, errors));

					}

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
		/// Extract the column portion of an excel "cell reference" and convert it to a zero-based index.
		/// </summary>
		/// <param name="cellName">The "cell reference" value of a cell.</param>
		/// <returns>The column index represented by the column portion of the reference.</returns>
		private Int32 GetExcelColumnIndex(String cellName)
		{

			// Excel column names (eg. "A", "B", "AA", "AB") are a kind of base 27 without a "0" value. That makes this conversion a bit weirder than
			// if it were straight base 26.
			//String columnName = Regex.Replace(cellName, "[^A-Z]*", "");
			String columnName = cellName.Substring(0, cellName.IndexOfAny(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' }));
			Int32 ordinalOffset = (Int32)'A' - 1;
			Int32 placeValue = 1;
			Int32 index = 0;

			for (int place = 1; place <= columnName.Length; ++place)
			{

				index +=
					// The multiplicitive value of this 'place'.
					placeValue
					// The current place is calculated right-to-left.
					* ((Int32)columnName[columnName.Length - place]
					// This offset set makes 'A' 1, 'B' 2, and so on.
					   - ordinalOffset);

				placeValue *= 27;

			}

			// Offset by one so that 'A' is index 0.
			return index - columnName.Length;

		}

		/// <summary>
		/// Retrieve the value of an Excell spreadsheet cell, "dereferencing" it if necessary.
		/// </summary>
		/// <param name="items">The shared string table.</param>
		/// <param name="cell">The cell to get the value from.</param>
		/// <returns>The value in the cell, or null if there is none.</returns>
		private object GetExcelCellValue(SharedStringItem[] items, Cell cell)
		{

			object value = null;

			if (cell.CellValue != null && !cell.CellValue.Text.Equals(""))
				if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
					value = items[int.Parse(cell.CellValue.Text)].InnerText.Trim();
				else if (cell.DataType != null &&
						cell.DataType == CellValues.Boolean &&
						(cell.CellValue.Text.Equals("0") || cell.CellValue.Text.Equals("1")))
					value = cell.CellValue.Text.Equals("1");
				else
					value = cell.CellValue.Text.Trim();

			return value;

		}

		/// <summary>
		/// Move to the next available row.
		/// </summary>
		private void MoveNext()
		{

			this.hasRecords = false;

			if (this.rows != null)
				this.hasRecords = this.rows.MoveNext();

			while (!this.HasRecords && this.worksheetParts.MoveNext())
			{

				this.rows = this.worksheetParts.Current.Worksheet.Descendants<Row>().GetEnumerator();
				this.hasRecords = this.rows.MoveNext();

			}

		}

		/// <summary>
		/// Parse a string into a particular type. Note that this requires that the type have a public Parse method that takes a String and returns a
		/// value.
		/// </summary>
		/// <param name="type">The type to parse the string into.</param>
		/// <param name="valueString">The string to parse.</param>
		/// <returns>The parsed value.</returns>
		private object ParseString(Type type, String valueString)
		{

			String lowerCaseValueString = valueString.ToLower();
			object result = null;

			if (type == typeof(DateTime))
			{

				Double oleDate;

				// Dates in Excel can be in either a standard ISO-ish date, or in OLE Automation Date format.
				if (Double.TryParse(valueString, out oleDate))
				{

					result = DateTime.FromOADate(oleDate);

				}
				else
				{

					result = DateTime.Parse(valueString);

				}

			}
			else if (type == typeof(Boolean))
			{

				Boolean booleanResult;

				if (ExcelImportReader.booleanValues.TryGetValue(valueString, out booleanResult))
					result = (object)booleanResult;
				else
					throw new FormatException("Acceptable values for boolean field are: true, false, yes, no");

			}
			else if (type == typeof(Decimal))
			{

				valueString = Regex.Replace(valueString, "[^0-9.,]", "");

				result = Decimal.Parse(valueString);

			}
			else
			{

				MethodInfo parse = type.GetMethod("Parse", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(String) }, null);

				try
				{

					result = parse.Invoke(null, new object[] { valueString });

				}
				catch (TargetInvocationException exception)
				{

					throw exception.InnerException;

				}

			}

			return result;

		}

		/// <summary>
		/// Read in the head of the file, including its size and the column headers.
		/// </summary>
		private void ReadStart()
		{

			Int32 leadingRows = 0;
			String columnValue;
			Int32 cellIndex;

			while (!this.HasRecords && this.worksheetParts.MoveNext())
			{

				this.rows = this.worksheetParts.Current.Worksheet.Descendants<Row>().GetEnumerator();
				this.hasRecords = this.rows.MoveNext();

			}

			while (this.HasRecords && this.columnLocations.Count == 0 && leadingRows < ExcelImportReader.MaxLeadingRows)
			{

				Row row = this.rows.Current;

				this.RecordIndex += 1;

				// Translate the file's organization-specific column names to the canonical names, and figure out where in the column
				// order they are.
				foreach (Cell cell in row.Descendants<Cell>())
				{

					columnValue = this.GetExcelCellValue(sharedItemsTable, cell) as String;
					cellIndex = this.GetExcelColumnIndex(cell.CellReference);

					if (!String.IsNullOrEmpty(columnValue) && this.translation.ContainsKey(columnValue.Trim()))
					{

						columnLocations[cellIndex] = this.translation[columnValue.Trim()];
						originalColumnLocations[cellIndex] = columnValue.Trim();

					}

				}

				leadingRows += 1;

				this.MoveNext();

			}

			// Make sure that they've included all the required columns.
			if (columnLocations.Count == 0)
			{

				throw new ImportHeaderNotFoundException(this.filename + " contains no headers", this.translation.Keys);
			
			}
			else
			{

				List<String> missingHeaders = new List<String>();

				foreach (DataColumn column in this.Table.Columns)
					if (!column.AllowDBNull && !columnLocations.ContainsValue(column.ColumnName))
						missingHeaders.Add(this.Translation.First(c => c.Value == column.ColumnName).Key);

				if (missingHeaders.Count > 0)
					throw new ImportHeaderNotFoundException(
						"Required column missing",
						this.Translation.Keys,
						missingHeaders);

			}

		}

	}

}
