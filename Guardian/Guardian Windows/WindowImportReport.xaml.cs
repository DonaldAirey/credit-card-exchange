namespace FluidTrade.Guardian.Windows
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Data;
	using System.Windows.Documents;
	using System.Windows.Input;
	using System.Windows.Media;
	using System.Windows.Media.Imaging;
	using System.Windows.Navigation;
	using System.Windows.Shapes;
	using System.Diagnostics;
	using System.IO;
	using System.Threading;
	using System.Xml.Schema;
	using System.Xml;

	/// <summary>
	/// Interaction logic for WindowImportReport.xaml
	/// </summary>
	public partial class WindowImportReport : Window
	{

		/// <summary>
		/// Indicates the Error property.
		/// </summary>
		public static readonly DependencyProperty ErrorProperty =
			DependencyProperty.Register("Error", typeof(String), typeof(WindowImportReport), new PropertyMetadata(""));
		/// <summary>
		/// Indicates the Failed dependency property.
		/// </summary>
		public static readonly DependencyPropertyKey FailedProperty =
			DependencyProperty.RegisterReadOnly("Failed", typeof(Boolean), typeof(WindowImportReport), new PropertyMetadata(false));
		/// <summary>
		/// Indicates the FailedCount dependency property.
		/// </summary>
		public static readonly DependencyPropertyKey FailedCountProperty =
			DependencyProperty.RegisterReadOnly("FailedCount", typeof(Int64), typeof(WindowImportReport), new PropertyMetadata(0L));
		/// <summary>
		/// Indicates the FailureFile property.
		/// </summary>
		public static readonly DependencyProperty FailureFileProperty =
			DependencyProperty.Register("FailureFile", typeof(String), typeof(WindowImportReport), new PropertyMetadata("", WindowImportReport.OnFailureFileChanged));
		/// <summary>
		/// Indicates the Failed dependency property.
		/// </summary>
		public static readonly DependencyPropertyKey FailureFileBasenameProperty =
			DependencyProperty.RegisterReadOnly("FailureFileBasename", typeof(String), typeof(WindowImportReport), new PropertyMetadata(""));
		/// <summary>
		/// Indicates the ImportEventArgs property.
		/// </summary>
		public static readonly DependencyProperty ImportEventArgsProperty =
			DependencyProperty.Register(
				"ImportEventArgs",
				typeof(DataImporter.ImportEventArgs),
				typeof(WindowImportReport),
				new PropertyMetadata(WindowImportReport.OnImportEventArgsChanged));
		/// <summary>
		/// Indicates the SchemaVersion dependency property.
		/// </summary>
		public static readonly DependencyProperty SchemaVersionProperty =
			DependencyProperty.Register("SchemaVersion", typeof(Int32), typeof(WindowImportReport), new PropertyMetadata(0));
		/// <summary>
		/// Indicates the SucceededCount dependency property.
		/// </summary>
		public static readonly DependencyPropertyKey SucceededCountProperty =
			DependencyProperty.RegisterReadOnly("SucceededCount", typeof(Int64), typeof(WindowImportReport), new PropertyMetadata(0L));

		/// <summary>
		/// Create a new import report window.
		/// </summary>
		public WindowImportReport()
		{

			InitializeComponent();
			this.Loaded += OnLoaded;

		}

		/// <summary>
		/// In the case of failure, the error message reported in the exception.
		/// </summary>
		public String Error
		{

			get { return this.GetValue(WindowImportReport.ErrorProperty) as String; }
			set { this.SetValue(WindowImportReport.ErrorProperty, value); }

		}

		/// <summary>
		/// True if this report is for a failed import.
		/// </summary>
		public Boolean Failed
		{

			get { return this.ImportEventArgs != null && this.ImportEventArgs.Exception == null; }

		}

		/// <summary>
		/// The number of records that failed to be imported.
		/// </summary>
		public Int64 FailedCount
		{

			get { return this.ImportEventArgs == null ? 0 : this.ImportEventArgs.FailedCount; }

		}

		/// <summary>
		/// The name of the file where failed records are written.
		/// </summary>
		public String FailureFile
		{

			get { return this.GetValue(WindowImportReport.FailureFileProperty) as String; }
			set { this.SetValue(WindowImportReport.FailureFileProperty, value); }

		}

		/// <summary>
		/// The name of the file where failed records are written.
		/// </summary>
		public String FailureFileBasename
		{

			get { return System.IO.Path.GetFileName(this.FailureFile); }

		}

		/// <summary>
		/// The event arguments that the window is reporting on. This property must be set before the window is shown.
		/// </summary>
		public DataImporter.ImportEventArgs ImportEventArgs
		{

			get { return this.GetValue(WindowImportReport.ImportEventArgsProperty) as DataImporter.ImportEventArgs; }
			set { this.SetValue(WindowImportReport.ImportEventArgsProperty, value); }

		}

		/// <summary>
		/// The current version of the import schema.
		/// </summary>
		public Int32 SchemaVersion
		{

			get { return (Int32)this.GetValue(WindowImportReport.SchemaVersionProperty); }
			set { this.SetValue(WindowImportReport.SchemaVersionProperty, value); }

		}

		/// <summary>
		/// The number of records successfully imported.
		/// </summary>
		public Int64 SucceededCount
		{

			get { return this.ImportEventArgs == null ? 0 : this.ImportEventArgs.SucceededCount; }

		}

		/// <summary>
		/// Handle the import event arguments changing.
		/// </summary>
		/// <param name="sender">The import report whose ImportEventArgs changed.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private static void OnImportEventArgsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			DataImporter.ImportEventArgs importEventArgs = eventArgs.NewValue as DataImporter.ImportEventArgs;
			String fullText = "";
			String error = "";

			sender.SetValue(WindowImportReport.FailedProperty, importEventArgs.Exception != null);
			sender.SetValue(WindowImportReport.FailedCountProperty, importEventArgs.FailedCount);
			sender.SetValue(WindowImportReport.SucceededCountProperty, importEventArgs.SucceededCount);

			if (importEventArgs.Exception == null)
			{

				fullText = String.Format(Properties.Resources.ImportSucceededWithErrors, importEventArgs.SucceededCount, importEventArgs.FailedCount);

			}
			else
			{

				if (importEventArgs.Exception is ThreadAbortException)
				{

					error = String.Format(Properties.Resources.ImportAborted, importEventArgs.SucceededCount);

				}
				else if (importEventArgs.Exception is ImportHeaderNotFoundException)
				{

					StringBuilder headerText = new StringBuilder();
					ICollection<String> headers = (importEventArgs.Exception as ImportHeaderNotFoundException).Headers;
					ICollection<String> missingHeaders = (importEventArgs.Exception as ImportHeaderNotFoundException).MissingHeaders;
					Int32 index = 0;

					if (missingHeaders != null)
					{

						foreach (String header in missingHeaders)
						{

							index += 1;
							headerText.Append(header);
							if (index < missingHeaders.Count)
								headerText.Append(", ");

						}

						error = String.Format(Properties.Resources.ImportFailedRequiredColumnMissing, headerText);

					}
					else
					{

						foreach (String header in headers)
						{

							index += 1;
							headerText.Append(header);
							if (index < headers.Count)
								headerText.Append(", ");

						}

						error = String.Format(Properties.Resources.ImportFailedNoHeaders, headerText);

					}

				}
				else if (importEventArgs.Exception is XmlSchemaValidationException)
				{

					XmlSchemaValidationException exception = importEventArgs.Exception as XmlSchemaValidationException;
					error = String.Format(Properties.Resources.XmlValidationFailed, exception.LineNumber, exception.LinePosition, exception.Message);

				}
				else if (importEventArgs.Exception is XmlException)
				{

					XmlException exception = importEventArgs.Exception as XmlException;
					error = String.Format(Properties.Resources.XmlValidationFailed, exception.LineNumber, exception.LinePosition, exception.Message);

				}
				else if (importEventArgs.Exception is FormatException)
				{

					XmlException exception = importEventArgs.Exception as XmlException;
					error = String.Format(Properties.Resources.ImportVersionMismatch, sender.GetValue(WindowImportReport.SchemaVersionProperty));

				}
				else
				{

					error = importEventArgs.Exception.Message;
					// This is an unexpected error. Log it.
					FluidTrade.Core.EventLog.Error("Exception while importing: {0}\nStack trace: {1}", importEventArgs.Exception.Message, importEventArgs.Exception.StackTrace);

				}

				fullText = String.Format(Properties.Resources.ImportFailed, error);

			}

			sender.SetValue(WindowImportReport.ErrorProperty, fullText);

		}

		/// <summary>
		/// Handle the failure file name changing.
		/// </summary>
		/// <param name="sender">The import report whose ImportEventArgs changed.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private static void OnFailureFileChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			String failureFile = eventArgs.NewValue as String;

			if (failureFile != null)
			{

				sender.SetValue(WindowImportReport.FailureFileBasenameProperty, System.IO.Path.GetFileName(failureFile));

				try
				{

					(sender as WindowImportReport).failuresDisplay.Text = File.ReadAllText(failureFile);

				}
				catch
				{

					(sender as WindowImportReport).failuresDisplay.Text = "Failure records unavailable.";

				}

			}

		}

		/// <summary>
		/// Handle the Okay command.
		/// </summary>
		/// <param name="sender">The OK button.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnOkay(object sender, RoutedEventArgs eventArgs)
		{

			this.Close();

		}

		/// <summary>
		/// Handle the user clicking the failure file link.
		/// </summary>
		/// <param name="sender">The failure file HyperLink.</param>
		/// <param name="e">The event arguments.</param>
		private void OnGoToPage(object sender, ExecutedRoutedEventArgs e)
		{

			Process explorer = new Process();

			explorer.StartInfo.Arguments = "/select," + this.FailureFile;
			explorer.StartInfo.FileName = "explorer";

			explorer.Start();

		}

		/// <summary>
		/// Handle the loaded event.
		/// </summary>
		/// <param name="sender">The import window.</param>
		/// <param name="e">The event arguments.</param>
		private void OnLoaded(object sender, RoutedEventArgs e)
		{

			if (this.ImportEventArgs == null)
				throw new ArgumentNullException("ImportEventArgs");

		}

		/// <summary>
		/// Handle the expansion of the error list.
		/// </summary>
		/// <param name="sender">The expand button.</param>
		/// <param name="e">The event arguments.</param>
		private void OnZoom(object sender, ExecutedRoutedEventArgs e)
		{

			Boolean? expandValue = (Boolean?)(e.Parameter);

			if (expandValue == null || expandValue.Value == false)
			{

				this.failuresBox.BorderThickness = new Thickness(0,1,0,0);
				this.failuresDisplay.Visibility = Visibility.Collapsed;

			}
			else
			{

				this.failuresBox.BorderThickness = new Thickness(1);
				this.failuresDisplay.Visibility = Visibility.Visible;
			
			}

		}

	}

}
