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
using System.Windows.Shapes;
using System.Xml;
using System.Threading;
using FluidTrade.Guardian;

namespace FluidTrade.Reporting.Windows
{
	/// <summary>
	/// Interaction logic for WindowImportProperties.xaml
	/// </summary>
	public partial class WindowImportProperties : Window
	{
		/// <summary>
		/// xml of imported file
		/// </summary>
		private string importedXml;

		/// <summary>
		/// path to report to import
		/// </summary>
		private string reportImportPath;

		/// <summary>
		/// path to translation to import
		/// </summary>
		private string translationImportPath;

		/// <summary>
		/// report description
		/// </summary>
		private string reportDescription;

		/// <summary>
		/// report name
		/// </summary>
		private string reportName;
		
		/// <summary>
		/// protected ctor, used for designer only
		/// </summary>
		protected WindowImportProperties()
		{
			InitializeComponent();
		}

		/// <summary>
		/// ctor for window in property only mode
		/// import and ok button will not be available
		/// </summary>
		/// <param name="name"></param>
		/// <param name="description"></param>
		public WindowImportProperties(string name, string description)
			:this(false, false)
		{
			InitializeComponent();
			this.Title = string.Format("{0} Properties", name); 
			this.nameTextBox.Text = name;
			this.descriptionTextBox.Text = description;
		}

		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="enableChanges">show ok/cancel button or just the close button and disable textboxes</param>
		/// <param name="showImport">show the import button</param>
		public WindowImportProperties(bool enableChanges, bool showImport)
		{
			InitializeComponent();
			if (enableChanges == false)
			{
				//make the txtBoxes readonly
				this.nameTextBox.IsReadOnly = true;
				this.descriptionTextBox.IsReadOnly = true;
				this.importFromTextBox.IsReadOnly = true;
				//hide the ok button
				this.okBtn.Visibility = Visibility.Hidden;
				
				//change cancel to close
				//TODO:!!!RM LOCALIZE
				this.cancelBtn.Content = "Close";
			}

			if (showImport == false)
			{
				//hide all the import controls
				this.importBtn.Visibility = Visibility.Hidden;
				this.importFromLabel.Visibility = Visibility.Hidden;
				this.importFromTextBox.Visibility = Visibility.Hidden;

				this.translationTextBox.Visibility = Visibility.Hidden;
				this.translationLabel.Visibility = Visibility.Hidden;
				this.translationBtn.Visibility = Visibility.Hidden;
				this.translationRow.Height = new GridLength(0);
				this.templateBufferRow.Height = new GridLength(0);
				this.templateRow.Height = new GridLength(0);
			}
			else
			{
				this.Title = "Import Report";
			}
		}

		/// <summary>
		/// handler for import button click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void importBtn_Click(object sender, RoutedEventArgs e)
		{
			//show file picker dialog
			string fileName = FluidTrade.Reporting.Windows.WindowPickReportFromDisk.PickReport();
			if(string.IsNullOrEmpty(fileName))
				return;

			this.importFromTextBox.Text = fileName;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void translationBtn_Click(object sender, RoutedEventArgs e)
		{
			//show file picker dialog
			string fileName = FluidTrade.Reporting.Windows.WindowPickReportFromDisk.PickTranslation();
			if (string.IsNullOrEmpty(fileName))
				return;

			this.translationTextBox.Text = fileName;
		}

		/// <summary>
		/// handler for cancel button click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void cancelBtn_Click(object sender, RoutedEventArgs e)
		{
			//set the result to false
			this.DialogResult = false;

			//close the window
			this.Close();
		}

		/// <summary>
		/// handler for ok button click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void okBtn_Click(object sender, RoutedEventArgs e)
		{
			this.reportImportPath = this.importFromTextBox.Text;
			this.translationImportPath = this.translationTextBox.Text;
			this.reportName = this.nameTextBox.Text;
			this.reportDescription = this.descriptionTextBox.Text;

			if (!string.IsNullOrEmpty(this.reportImportPath))
			{
				//get the xml from the file to be imported
				this.importedXml = ReportXmlHelper.FilesToXml(this.reportName, this.reportDescription, this.reportImportPath, this.translationImportPath);

				//crete the reportObject to use in the webservice
				FluidTrade.Guardian.TradingSupportReference.Report report = new FluidTrade.Guardian.TradingSupportReference.Report();
				report.Name = this.reportName;
				report.ReportTypeId = DataModel.ReportType.ReportTypeKeyReportTypeCode.Find(ReportType.StaticReport).ReportTypeId;
				report.Xaml = this.importedXml;

				//make the webservice call to create the new report record on a worker thread
				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(new WaitCallback(this.CreateReportCallback), report);

				//disable the window and do not close until the webservice is complete
				this.IsEnabled = false;
			}
			else
			{
				//!!!RM show an error?
				this.DialogResult = false;
				this.Close();
			}
		}

		/// <summary>
		/// worker thread callback to make webservice call to create a new report record that
		/// contains the new report xml
		/// </summary>
		/// <param name="state"></param>
		private void CreateReportCallback(object state)
		{
			FluidTrade.Guardian.TradingSupportReference.Report report = (FluidTrade.Guardian.TradingSupportReference.Report)state;
			TradingSupportWebService.CreateReport(report);

			//notify back to the UI thread that the webservice has completed
			this.Dispatcher.BeginInvoke(new System.Windows.Forms.MethodInvoker(CreateReportCallbackDone), null);
		}

		/// <summary>
		/// callback on ui thread from worker thread to notify that
		/// the report record has been saved
		/// </summary>
		private void CreateReportCallbackDone()
		{
			//save has completed set result to true
			this.DialogResult = true; 

			//close this window
			this.Close();
		}

		/// <summary>
		/// get xml of imported file
		/// </summary>
		public string ImportedXml { get { return this.importedXml; } }

		/// <summary>
		/// get report description
		/// </summary>
		public string ReportDescription { get { return this.reportDescription; } }

		/// <summary>
		/// get path to report to import
		/// </summary>
		public string ReportImportPath { get { return this.reportImportPath; } }

		/// <summary>
		/// get report name
		/// </summary>
		public string ReportName { get { return this.reportName; } }

	
	}
}
