namespace FluidTrade.Guardian.Windows
{

	using FluidTrade.Core.Windows;
	using FluidTrade.Guardian.TradingSupportReference;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Windows;
	using System.IO;
	using System.Threading;
	using System.Windows.Threading;
	using FluidTrade.Core;
	using System.Xml.Schema;
	using System.Xml;
	using System.Windows.Data;
	using System.Reflection;

	/// <summary>
	/// A window that imports records from a specified file and tracks its progress.
	/// </summary>
	public class WindowImport : WindowProgress
	{

		/// <summary>
		/// Indicates the BulkCount dependency property.
		/// </summary>
		public static readonly DependencyProperty BulkCountProperty =
			DependencyProperty.Register("BulkCount", typeof(int), typeof(WindowImport), new PropertyMetadata(10));
		/// <summary>
		/// Indicates the ImportFile dependency property.
		/// </summary>
		public static readonly DependencyProperty ImportFileProperty =
			DependencyProperty.Register("ImportFile", typeof(string), typeof(WindowImport));
		/// <summary>
		/// Indicates the Importer dependency property.
		/// </summary>
		public static readonly DependencyProperty ImporterProperty =
			DependencyProperty.Register("Importer", typeof(DataImporter.RecordImporter), typeof(WindowImport));
		/// <summary>
		/// Indicates the ImportPulse dependency property.
		/// </summary>
		public static readonly DependencyProperty ImportPulseProperty =
			DependencyProperty.Register("ImportPulse", typeof(DataImporter.ImportPulseEventHandler), typeof(WindowImport));
		/// <summary>
		/// Indicates the Parameters dependency property.
		/// </summary>
		public static readonly DependencyProperty ParametersProperty =
			DependencyProperty.Register("Parameters", typeof(Dictionary<string,object>), typeof(WindowImport));
		/// <summary>
		/// Indicates the RecordType dependency property.
		/// </summary>
		public static readonly DependencyProperty RecordTypeProperty =
			DependencyProperty.Register("RecordType", typeof(Type), typeof(WindowImport));
		/// <summary>
		/// Indicates the Schema dependency property.
		/// </summary>
		public static readonly DependencyProperty SchemaProperty =
			DependencyProperty.Register("Schema", typeof(Stream), typeof(WindowImport));
		/// <summary>
		/// Indicates the SchemaVersion dependency property.
		/// </summary>
		public static readonly DependencyProperty SchemaVersionProperty =
			DependencyProperty.Register("SchemaVersion", typeof(Int32), typeof(WindowImport));
		/// <summary>
		/// Indicates the Succeeded dependency property.
		/// </summary>
		public static readonly DependencyProperty SucceededProperty =
			DependencyProperty.Register("Succeeded", typeof(Int64), typeof(WindowImport), new PropertyMetadata(0L));
		/// <summary>
		/// Indicates the Translation dependency property.
		/// </summary>
		public static readonly DependencyProperty TranslationProperty =
			DependencyProperty.Register("Translation", typeof(Dictionary<String,String>), typeof(WindowImport));

		private DataImporter importer = new DataImporter();
		private System.Timers.Timer timer;
		private DateTime startTime;
		private Boolean loaded;
		private String failureFile;
		private Boolean succeeded;

		/// <summary>
		/// Create a new import window.
		/// </summary>
		public WindowImport() : base()
		{

			this.Maximum = 0;
			this.Loaded += this.OnLoaded;
			this.Message = "Initializing....";
			this.Title = "Importing Records";
			this.SetBinding(WindowImport.HeaderProperty, new Binding("Succeeded") { Source = this, StringFormat = "{0:Imported 0 records so far;Initializing;Initializing}" });
			this.Parameters = importer.Parameters;
			this.Owner = Application.Current.MainWindow;

		}

		/// <summary>
		/// The the number of records to import at once.
		/// </summary>
		public int BulkCount
		{

			get { return (int)this.GetValue(WindowImport.BulkCountProperty); }
			set { this.SetValue(WindowImport.BulkCountProperty, value); }

		}

		/// <summary>
		/// The delegate called to import a record.
		/// </summary>
		public DataImporter.RecordImporter Importer
		{

			get { return this.GetValue(WindowImport.ImporterProperty) as DataImporter.RecordImporter; }
			set { this.SetValue(WindowImport.ImporterProperty, value); }

		}

		/// <summary>
		/// The the file to import data from.
		/// </summary>
		public string ImportFile
		{

			get { return this.GetValue(WindowImport.ImportFileProperty) as string; }
			set { this.SetValue(WindowImport.ImportFileProperty, value); }

		}

		/// <summary>
		/// The delegate called when an import pulse is fired.
		/// </summary>
		public DataImporter.ImportPulseEventHandler ImportPulse
		{

			get { return this.GetValue(WindowImport.ImportPulseProperty) as DataImporter.ImportPulseEventHandler; }
			set { this.SetValue(WindowImport.ImportPulseProperty, value); }

		}

		/// <summary>
		/// The parameters for the import.
		/// </summary>
		public Dictionary<string, object> Parameters
		{

			get { return this.GetValue(WindowImport.ParametersProperty) as Dictionary<string, object>; }
			set { this.SetValue(WindowImport.ParametersProperty, value); }

		}

		/// <summary>
		/// Type of the TradingSupport record to use.
		/// </summary>
		public Type RecordType
		{

			get { return this.GetValue(WindowImport.RecordTypeProperty) as Type; }
			set { this.SetValue(WindowImport.RecordTypeProperty, value); }

		}

		/// <summary>
		/// The XML schema used to validate the data file.
		/// </summary>
		public Stream Schema
		{

			get { return this.GetValue(WindowImport.SchemaProperty) as Stream; }
			set { this.SetValue(WindowImport.SchemaProperty, value); }

		}

		/// <summary>
		/// The version of the XML schema used to validate the data file.
		/// </summary>
		public Int32 SchemaVersion
		{

			get { return (Int32)this.GetValue(WindowImport.SchemaVersionProperty); }
			set { this.SetValue(WindowImport.SchemaVersionProperty, value); }

		}

		/// <summary>
		/// The the number of records to succeed so far.
		/// </summary>
		public Int64 Succeeded
		{

			get { return (Int64)this.GetValue(WindowImport.SucceededProperty); }
			set { this.SetValue(WindowImport.SucceededProperty, value); }

		}

		/// <summary>
		/// The translation scheme between the import file and the record, if any.
		/// </summary>
		public Dictionary<String, String> Translation
		{

			get { return this.GetValue(WindowImport.TranslationProperty) as Dictionary<String, String>; }
			set { this.SetValue(WindowImport.TranslationProperty, value); }

		}

		/// <summary>
		/// Handled the Loaded event. Kick off the import.
		/// </summary>
		/// <param name="sender">The DataImporter.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnLoaded(object sender, EventArgs eventArgs)
		{

			this.Time = TimeSpan.Zero;
			this.TimeLeftVisibility = Visibility.Hidden;
			this.TimeLeft = TimeSpan.Zero;

			this.timer = new System.Timers.Timer(1000);

			this.timer.Elapsed += this.UpdateTime;

			this.importer.BulkCount = this.BulkCount;
			this.importer.EndLoading += this.OnEndLoading;
			this.importer.Failed += this.OnFailed;
			this.importer.Importer = this.Importer;
			this.importer.ImportPulse += this.OnPulse;
			this.importer.ImportPulse += this.ImportPulse;
			this.importer.RecordType = this.RecordType;
			this.importer.Schema = this.Schema;
			this.importer.SchemaVersion = this.SchemaVersion;
			this.importer.Translation = this.Translation;
			this.importer.Success += this.OnSuccess;

			this.importer.ImportFromFile(this.ImportFile);

		}

		/// <summary>
		/// Handle the Cancel event.
		/// </summary>
		/// <param name="sender">The Cancel button.</param>
		/// <param name="eventArgs">The event arguments.</param>
		protected override void OnCancel(object sender, EventArgs eventArgs)
		{

			if (this.succeeded)
				this.Close();
			else
				this.importer.Cancel();

		}

		/// <summary>
		/// Handle the EndLoading event. Switch to a determinate progress bar.
		/// </summary>
		/// <param name="sender">The DataImporter.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnEndLoading(object sender, DataImporter.InfoEventArgs eventArgs)
		{

			this.Dispatcher.BeginInvoke(
				DispatcherPriority.Normal,
				new Action(delegate ()
					{

						this.failureFile = eventArgs.FailureFile;
						this.loaded = true;
						this.Maximum = eventArgs.Size;
						this.IsIndeterminate = false;
						this.Message = String.Format("Importing records from {0}....", Path.GetFileName(this.ImportFile));
						this.startTime = DateTime.Now;
						this.timer.Start();

					}));

		}

		/// <summary>
		/// Handle the failed event.
		/// </summary>
		/// <param name="sender">The DataImporter.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnFailed(object sender, DataImporter.ImportEventArgs eventArgs)
		{

			this.Dispatcher.BeginInvoke(
				DispatcherPriority.Normal,
				new Action(delegate()
				{

					Boolean threadAborted = eventArgs.Exception is ThreadAbortException || eventArgs.Exception.InnerException is ThreadAbortException;

					this.timer.Stop();
					if (!(threadAborted && !this.loaded))
					{

						WindowImportReport report = new WindowImportReport();

						if (eventArgs.FailedCount != 0)
							report.FailureFile = this.failureFile;
						else
							report.FailureFile = null;
						report.ImportEventArgs = eventArgs;
						report.SchemaVersion = this.SchemaVersion;
						report.Owner = this;
						report.ShowDialog();
						this.Close();

					}

				}));

		}

		/// <summary>
		/// Handle the ReadPulse and ImportPulse events. Tick the progress bar forward once.
		/// </summary>
		/// <param name="sender">The DataImporter.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnPulse(object sender, DataImporter.ImportEventArgs eventArgs)
		{

			this.Dispatcher.BeginInvoke(
				DispatcherPriority.Normal,
				new Action(delegate()
					{

						this.Succeeded = eventArgs.SucceededCount;
						this.TimeLeftVisibility = Visibility.Visible;
						this.Value = eventArgs.Position;
						TimeSpan left = new TimeSpan((long)((this.Maximum - this.Value) * ((DateTime.Now - this.startTime).Ticks / this.Value)));
						this.TimeLeft = left < TimeSpan.Zero ? TimeSpan.Zero : left;
					
					}));

		}

		/// <summary>
		/// Handle the success event.
		/// </summary>
		/// <param name="sender">The DataImporter.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnSuccess(object sender, DataImporter.ImportEventArgs eventArgs)
		{

			this.Dispatcher.BeginInvoke(
				DispatcherPriority.Normal,
				new Action(delegate()
					{

						this.timer.Stop();
						this.TimeLeft = TimeSpan.Zero;

						if (eventArgs.FailedCount == 0)
						{

							BindingOperations.ClearBinding(this, WindowImport.HeaderProperty);
							this.Header = String.Format(Properties.Resources.ImportEndedSuccessHeader, eventArgs.SucceededCount);
							this.Message = String.Format(Properties.Resources.ImportEndedSuccessMessage, Path.GetFileName(this.ImportFile));
							this.cancelBtn.Content = Properties.Resources.Close;
							this.succeeded = true;
							this.Activate();

						}
						else
						{

							WindowImportReport report = new WindowImportReport();

							report.ImportEventArgs = eventArgs;
							report.FailureFile = this.failureFile;
							report.SchemaVersion = this.SchemaVersion;
							report.Owner = this;
							report.ShowDialog();
							this.Close();

						}

					}));

		}

		/// <summary>
		/// Update our timer and countdown.
		/// </summary>
		/// <param name="sender">The Timer.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void UpdateTime(object sender, EventArgs eventArgs)
		{

			this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate()
				{

					TimeSpan left = this.TimeLeft - new TimeSpan(0, 0, 1);
					this.TimeLeft = left < TimeSpan.Zero? TimeSpan.Zero : left;
					this.Time = DateTime.Now - this.startTime;
				
				}));

		}

	}

}
