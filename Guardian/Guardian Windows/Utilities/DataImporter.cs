namespace FluidTrade.Guardian
{

	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Text.RegularExpressions;
	using System.Windows;
	using System.Xml;
	using System.Xml.Schema;
	using DocumentFormat.OpenXml.Packaging;
	using DocumentFormat.OpenXml.Spreadsheet;
	using FluidTrade.Core;
	using FluidTrade.Guardian.TradingSupportReference;
	using System.Security;
	using System.ServiceModel;
	using System.Threading;

	using ImportPair = System.Collections.Generic.KeyValuePair<System.Array, System.Collections.Generic.List<System.Action>>;

	/// <summary>
	/// Import external data files.
	/// </summary>
	public class DataImporter
	{

		private static readonly Int32 CacheSize;

		/// <summary>
		/// Event arguments for the EndLoading event.
		/// </summary>
		public class InfoEventArgs : EventArgs
		{

			private String failureFile;
			private Int64 size;

			/// <summary>
			/// Create a new event arguments object.
			/// </summary>
			/// <param name="size">The records read from disk.</param>
			/// <param name="failureFile">The name of the file where failed records are written.</param>
			public InfoEventArgs(Int64 size, String failureFile)
				: base()
			{

				this.size = size;
				this.failureFile = failureFile;

			}

			/// <summary>
			/// The name of the file where errror records are written.
			/// </summary>
			public String FailureFile
			{

				get { return this.failureFile; }

			}
			
			/// <summary>
			/// The total records read from disk.
			/// </summary>
			public Int64 Size
			{

				get { return this.size; }

			}

		}

		/// <summary>
		/// Event arguments for the ImportPulse event.
		/// </summary>
		public class ImportEventArgs : EventArgs
		{

			private Exception exception;
			private Int64 position;
			private Int64 succeededCount;
			private Int64 failedCount;

			/// <summary>
			/// Create a new event arguments object.
			/// </summary>
			/// <param name="position">The current position in the import file.</param>
			/// <param name="succeeded">The number of records that succeeded.</param>
			/// <param name="failed">The number of records that failed.</param>
			public ImportEventArgs(long position, long succeeded, long failed)
				: base()
			{

				this.position = position;
				this.succeededCount = succeeded;
				this.failedCount = failed;
				this.exception = null;

			}

			/// <summary>
			/// Create a new event arguments object.
			/// </summary>
			/// <param name="position">The current position in the import file.</param>
			/// <param name="succeeded">The number of records that succeeded.</param>
			/// <param name="failed">The number of records that failed.</param>
			/// <param name="exception">The exception that caused the import to fail, if any.</param>
			public ImportEventArgs(long position, long succeeded, long failed, Exception exception)
				: base()
			{

				this.position = position;
				this.succeededCount = succeeded;
				this.failedCount = failed;
				this.exception = exception;

			}

			/// <summary>
			/// The exception that caused the import to fail, if any.
			/// </summary>
			public Exception Exception
			{

				get { return this.exception; }

			}

			/// <summary>
			/// The number of accounts that could not be imported.
			/// </summary>
			public long FailedCount
			{

				get { return this.failedCount; }

			}

			/// <summary>
			/// The current position in the import file.
			/// </summary>
			public long Position
			{

				get { return this.position; }

			}

			/// <summary>
			/// The number of accounts successfully imported.
			/// </summary>
			public long SucceededCount
			{

				get { return this.succeededCount; }

			}

			/// <summary>
			/// The total number of accounts processed.
			/// </summary>
			public long TotalCount
			{

				get { return this.failedCount + this.succeededCount; }

			}

		}

		/// <summary>
		/// Event handler for the EndLoading event.
		/// </summary>
		/// <param name="sender">The DataImporter that sent the event.</param>
		/// <param name="eventArgs">The event arguments.</param>
		public delegate void EndLoadingEventHandler(object sender, InfoEventArgs eventArgs);
		/// <summary>
		/// Event handler for the ImportPulse event.
		/// </summary>
		/// <param name="sender">The DataImporter that sent the event.</param>
		/// <param name="eventArgs">The event arguments.</param>
		public delegate void ImportPulseEventHandler(object sender, ImportEventArgs eventArgs);
		/// <summary>
		/// Event handler for the Failed event.
		/// </summary>
		/// <param name="sender">The DataImporter that sent the event.</param>
		/// <param name="eventArgs">The event arguments.</param>
		public delegate void FailedEventHandler(object sender, ImportEventArgs eventArgs);
		/// <summary>
		/// The method called to import records.
		/// </summary>
		/// <param name="record">The records to import.</param>
		/// <param name="sentSize">The actual bulk size used.</param>
		/// <returns>A dictionary of failed objects to their error messages.</returns>
		public delegate Dictionary<object, string> RecordImporter(Array record, out Int32 sentSize);
		/// <summary>
		/// Event handler for the Success event.
		/// </summary>
		/// <param name="sender">The DataImporter that sent the event.</param>
		/// <param name="eventArgs">The event arguments.</param>
		public delegate void SuccessEventHandler(object sender, ImportEventArgs eventArgs);

		/// <summary>
		/// Parameters passed from the caller to the record.
		/// </summary>
		private Dictionary<String, object> parameters = new Dictionary<String, object>();
		/// <summary>
		/// The file where failed records are put.
		/// </summary>
		private String failureFile;
		/// <summary>
		/// Maximum number of errors per record encountered so far.
		/// </summary>
		private Int32 maximumErrorCount;
		/// <summary>
		/// The threads being used to do the import. To cancel the import, all of these must be killed.
		/// </summary>
		private List<Thread> threads;

		/// <summary>
		/// The event fired when the importer has finished loading the data from disk.
		/// </summary>
		public event EndLoadingEventHandler EndLoading;
		/// <summary>
		/// The event fired periodically while pushing data up to the server.
		/// </summary>
		public event ImportPulseEventHandler ImportPulse;
		/// <summary>
		/// The event fired if the import fails.
		/// </summary>
		public event FailedEventHandler Failed;
		/// <summary>
		/// The event fired if the import succeeds.
		/// </summary>
		public event SuccessEventHandler Success;

		static DataImporter()
		{

			DataImporter.CacheSize = Properties.Settings.Default.ImportCacheSize;

		}

		/// <summary>
		/// Construct a new importer.
		/// </summary>
		public DataImporter()
		{

			this.threads = new List<Thread>();
			this.maximumErrorCount = 0;

		}

		/// <summary>
		/// How many records to import at once.
		/// </summary>
		public int BulkCount { get; set; }

		/// <summary>
		/// Get the name of the file that contains the failed record report.
		/// </summary>
		public String FailureFile
		{
			get { return this.failureFile; }
		}

		/// <summary>
		/// The delegate called to actually import a record.
		/// </summary>
		public RecordImporter Importer
		{

			get;
			set;

		}

		/// <summary>
		/// The parameters to add to each object. The names and types of these parameters must match their counterparts in the RecordType. If the field in
		/// the RecordType is nullable, the type of the parameter must also be nullable.
		/// </summary>
		public Dictionary<string, object> Parameters
		{

			get { return this.parameters; }

		}

		/// <summary>
		/// The type of the record to import with.
		/// </summary>
		public Type RecordType { get; set; }

		/// <summary>
		/// The schema to validate xml files against. This schema should have exactly one element type in it, and should define a subset of the fields
		/// available in the RecordType. The fields in the schema must match their counterparts in the RecordType both in name and in type. If the
		/// field is optional in the RecordType (ie. is nullable), it must be optional in the schema (ie. is minOccurs="0").
		/// </summary>
		public Stream Schema { get; set; }

		/// <summary>
		/// The translation scheme between the import file and the record, if any.
		/// </summary>
		public Dictionary<String, String> Translation { get; set; }

		/// <summary>
		/// The current version of the schema the import is validated against.
		/// </summary>
		public Int32 SchemaVersion { get; set; }

		/// <summary>
		/// Start a function in a seperate thread, store the thread so we can kill it later.
		/// </summary>
		/// <param name="start">The function to start.</param>
		private void Begin(ThreadStart start)
		{

			Thread thread = new Thread(delegate()
				{
					try
					{
						start();
					}
					catch (Exception exception)
					{
						EventLog.Warning(String.Format("Import thread aborted unexectedly: {0}\n{1}", exception, exception.StackTrace));
					}
					finally
					{
						System.Diagnostics.Debug.WriteLine("DataImport thread exiting.");
					}
				});

			thread.Name = string.Concat("DoImport_", DateTime.Now.ToString("hh_mm"));
			thread.IsBackground = true;
			this.threads.Add(thread);
			thread.Start();

		}

		/// <summary>
		/// Cancel the import. Threads may lag if they are in system calls outside of the .Net runtime (like network calls).
		/// </summary>
		public void Cancel()
		{

			foreach (Thread thread in this.threads)
			{

				thread.Abort();

				// Shake the thread out of blocking calls.
				if (thread.IsAlive)
					thread.Interrupt();

				thread.Join();

			}

		}

		/// <summary>
		/// Write the schema used for the failure file.
		/// </summary>
		/// <param name="failure">The xml stream to write to.</param>
		private void WriteFailureSchema(XmlWriter failure)
		{

			String xsd = "http://www.w3.org/2001/XMLSchema";

			failure.WriteStartElement("schema", xsd);

			failure.WriteStartElement("element", xsd);
			failure.WriteAttributeString("name", "FailedRecords");

			failure.WriteStartElement("complexType", xsd);
			failure.WriteStartElement("choice", xsd);
			failure.WriteAttributeString("minOccurs", "0");
			failure.WriteAttributeString("maxOccurs", "unbounded");

			failure.WriteStartElement("element", xsd);
			failure.WriteAttributeString("name", "Record");
			
			failure.WriteStartElement("complexType", xsd);

			failure.WriteStartElement("attribute", xsd);
			failure.WriteAttributeString("name", "Index");
			failure.WriteAttributeString("type", "xsd:int");
			failure.WriteEndElement();

			for (Int32 error = 0; error < this.maximumErrorCount; ++error)
			{

				failure.WriteStartElement("attribute", xsd);
				failure.WriteAttributeString("name", String.Format("Error{0}", error));
				failure.WriteAttributeString("type", "xsd:string");
				failure.WriteEndElement();

			}

			failure.WriteStartElement("all", xsd);

			foreach (KeyValuePair<String, String> translation in this.Translation)
			{

				failure.WriteStartElement("element", xsd);
				failure.WriteAttributeString("name", translation.Key);
				failure.WriteAttributeString("type", "xsd:string");
				failure.WriteEndElement();

			}

			failure.WriteEndElement();

			failure.WriteEndElement();
			failure.WriteEndElement();
			failure.WriteEndElement();

			failure.WriteEndElement();
			failure.WriteEndElement();
			failure.WriteEndElement();

		}

		/// <summary>
		/// Import a file.
		/// </summary>
		/// <param name="filename">The file to import from.</param>
		public void ImportFromFile(String filename)
		{

			this.Begin(() =>
				this.Start(filename));

		}

		/// <summary>
		/// Open a new file to write failed records to.
		/// </summary>
		/// <param name="filename">The original import filename.</param>
		/// <returns>The opened stream.</returns>
		private Stream OpenFailureFile(String filename)
		{

			String folder = Path.GetDirectoryName(filename);
			Stream file = null;
			UInt32 number = 0;

			while (file == null)
			{

				try
				{

					this.failureFile = String.Format("{0}{1: (0)\\.failures\\.xml;;\\.failures\\.xml}", filename, number);
					file = new FileStream(this.failureFile, FileMode.CreateNew, FileAccess.Write);

				}
				catch (SecurityException)
				{

					file = null;
					this.failureFile = null;

					if (MessageBox.Show(
							String.Format(Guardian.Properties.Resources.ImportFailureReadonly, folder),
							Guardian.Properties.Resources.ImportFailureRolloverTitle,
							MessageBoxButton.YesNo)
						== MessageBoxResult.Yes)
						file = Stream.Null;
					else
						throw new Exception("User doesn't have write permission to " + folder);

				}
				catch (UnauthorizedAccessException)
				{

					file = null;
					this.failureFile = null;

					if (MessageBox.Show(
							String.Format(Guardian.Properties.Resources.ImportFailureReadonly, folder),
							Guardian.Properties.Resources.ImportFailureRolloverTitle,
							MessageBoxButton.YesNo)
						== MessageBoxResult.Yes)
						file = Stream.Null;
					else
						throw new Exception("User doesn't have write permission to " + folder);

				}
				catch
				{

					file = null;
					this.failureFile = null;

					if (number < UInt32.MaxValue)
						number += 1;
					else if (MessageBox.Show(
								Guardian.Properties.Resources.ImportFailureRollover,
								Guardian.Properties.Resources.ImportFailureRolloverTitle,
								MessageBoxButton.YesNo)
							== MessageBoxResult.Yes)
						file = Stream.Null;
					else
						throw new Exception("Ran out of numbers for import failure files");

				}

			}

			return file;

		}

		/// <summary>
		/// Read records in from the import file.
		/// </summary>
		/// <param name="reader">The reader to use.</param>
		/// <param name="queue">The queue to write to.</param>
		private void Read(ImportReader reader, SynchronizedQueue<ImportPair> queue)
		{

			List<Action> failed;
			Array succeeded;

			try
			{

				while (reader.HasRecords && !queue.Empty)
				{

					Int32 bulkCount;

					lock (queue.SyncRoot)
						bulkCount = this.BulkCount;

					reader.GetBatch(bulkCount, out succeeded, out failed);
					queue.Enqueue(new ImportPair(succeeded, failed));

				}

			}
			catch (SynchronizedQueue<ImportPair>.QueueEmptyException)
			{

				// We've cleared the queue - finish up.

			}
			catch (Exception exception)
			{

				queue.Enqueue(new ImportPair(
					Array.CreateInstance(this.RecordType, 0),
					new List<Action> {
						delegate { throw exception; } }));

			}
			finally
			{

				reader.Dispose();
				queue.Empty = true;

			}

		}

		/// <summary>
		/// Import a file.
		/// </summary>
		/// <param name="filename">The file to import from.</param>
		private void Start(string filename)
		{

			Int64 index = 1;
			Int64 failed = 0;
			Int64 succeeded = 0;
			XmlWriterSettings settings = new XmlWriterSettings()
			{
				CloseOutput = true,
				ConformanceLevel = ConformanceLevel.Document,
				Indent = true,
				IndentChars = "\t",
				NewLineChars = "\r\n",
				NewLineHandling = NewLineHandling.Replace,
				NewLineOnAttributes = true,
			};
			XmlWriter failure = XmlWriter.Create(this.OpenFailureFile(filename), settings);
			SynchronizedQueue<ImportPair> queue = new SynchronizedQueue<ImportPair>(DataImporter.CacheSize);
			
				//DataModel.IsReading = false;
			try
			{

				ImportReader reader = ImportReader.Create(
					filename,
					this.Translation,
					this.Schema,
					this.SchemaVersion,
					this.RecordType,
					this.Parameters,
					(i, r, e) => this.WriteFailedRecord(failure, i, r, e));
				ImportPair records = new ImportPair();

				failure.WriteStartDocument();
				failure.WriteStartElement("FailedRecords");
				failure.WriteAttributeString("xmlns", "xsd", null, "http://www.w3.org/2001/XMLSchema");

				this.maximumErrorCount = this.Translation.Count;
				//this.WriteFailureSchema(failure);

				this.Begin(() =>
					this.Read(reader, queue));

				// Let our listeners know that we're starting the actual upload.
				if (this.EndLoading != null)
					this.EndLoading(this, new InfoEventArgs(reader.Size, this.failureFile));

				while (!queue.Empty || queue.Count != 0)
				{

					Int32 subIndex = 0;
					Dictionary<object, string> errors = null;
					Int32 sentSize;

					records = queue.Dequeue();
					failed += records.Value.Count;
					index += records.Value.Count;

					foreach (Action failReport in records.Value)
						failReport();

					errors = this.Importer(records.Key, out sentSize);

					if (sentSize < records.Key.Length)
						lock (queue.SyncRoot)
							this.BulkCount = sentSize;

					foreach (object record in errors.Keys)
					{

						List<String> messages = new List<String>() { errors[record] };
						failed += 1;
						this.WriteFailedRecord(failure, index + subIndex, reader.DetranslateRecord(record), messages);
						subIndex += 1;

					}

					succeeded += records.Key.Length - errors.Count;

					if (this.ImportPulse != null)
						this.ImportPulse(this, new ImportEventArgs(reader.Position, succeeded, failed));

					index += records.Key.Length;

				}

				if (this.Success != null)
					this.Success(this, new ImportEventArgs(0, succeeded, failed));

			}
			catch (ThreadAbortException exception)
			{

				if (this.Failed != null)
					this.Failed(this, new ImportEventArgs(0, succeeded, failed, exception));
				EventLog.Information("Import of {0} aborted by user", filename);

			}
			catch (Exception exception)
			{

				if (this.Failed != null)
					this.Failed(this, new ImportEventArgs(0, succeeded, failed, exception));
				EventLog.Information("Error importing {3}\n{0}: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace, filename);

			}
			finally
			{

				try
				{

					failure.WriteEndDocument();
					failure.Close();

				}
				catch (Exception exception)
				{

					EventLog.Information("Error closing failure file for import {3}\n{0}: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace, filename);

				}

				// If there are no errors, try to remove the failure file (since it's empty anyway).
				try
				{

					if (this.failureFile != null && failed == 0)
						File.Delete(this.failureFile);

				}
				catch (Exception exception)
				{

					EventLog.Information("Error removing failure file for import {3}\n{0}: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace, filename);

				}

				queue.Empty = true;

			}

			//DataModel.IsReading = true;

		}

		/// <summary>
		/// Write a failed record to disk.
		/// </summary>
		/// <param name="failure">The xml writer to write to.</param>
		/// <param name="recordIndex">Index of the record.</param>
		/// <param name="record">The record that failed.</param>
		/// <param name="errors">The error messages.</param>
		private void WriteFailedRecord(XmlWriter failure, Int64 recordIndex, Dictionary<String,object> record, List<string> errors)
		{

			Int32 errorIndex = 1;

			// Log broken records (haha) here.
			failure.WriteStartElement("Record");

			failure.WriteAttributeString("Index", String.Format("{0}", recordIndex));

			//if (this.maximumErrorCount < errors.Count)
			//	this.maximumErrorCount = errors.Count;

			foreach (String error in errors)
			{

				failure.WriteAttributeString(String.Format("Error{0}", errorIndex), error);
				errorIndex += 1;
				if (errorIndex >= this.maximumErrorCount)
					break;

			}

			foreach (String property in record.Keys)
			{

				object value = record[property];

				if (value != null)
				{

					failure.WriteElementString(property, String.Format("{0}", value));

				}

			}

			failure.WriteEndElement();
			failure.Flush();

		}

	}

}
