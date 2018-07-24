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
using FluidTrade.Guardian;
using System.IO;

namespace FluidTrade.Reporting.Windows
{
    /// <summary>
    /// Window that displays list of known reports
    /// </summary>
    public partial class WindowPickReport : Window
    {
		/// <summary>
		/// path to file that was selected
		/// </summary>
		private String filePath;

		/// <summary>
		/// report custom translator
		/// </summary>
		private FluidTrade.Reporting.Interfaces.IStaticReportTranslation translator;

		/// <summary>
		/// name of the report that was selected
		/// </summary>
		private String reportName;
		
		/// <summary>
		/// Static helper method to show PickReport window
		/// returns true if user clicked OK, 
		///	
		/// </summary>
		/// <param name="name">Name of the report the user picked</param>
		/// <param name="path">path to the report file on disk</param>
		/// <param name="translator"></param>
		/// <returns>true if your clicked OK</returns>
		public static bool PickReport(out string name, out string path, out FluidTrade.Reporting.Interfaces.IStaticReportTranslation translator)
		{
			name = null;
			path = null;
			translator = null;
			//try to load a crystal report dll. if not there don't let user continue
			System.Reflection.Assembly testAssembly = null;
			try
			{
				const String crystalAssebly = "CrystalDecisions.CrystalReports.Engine, Version=10.5.3700.0, Culture=neutral, PublicKeyToken=692fbea5521e1304";
				testAssembly = System.Reflection.Assembly.Load(crystalAssebly);
			}
			catch
			{
			}
			if (testAssembly == null)
			{
				//TODO: LOCALIZE
				MessageBox.Show("Crystal reports are needed for reporting feature. Crystal Reports install can be found at: install directory\\Dependancy\\CrystalReports\\setup.exe");
				return false;
			}
			//create the PickReport window
			WindowPickReport window = new WindowPickReport();
			if (true.Equals(window.ShowDialog()))
			{
				//if the user clicked ok set the return values
				path = window.FilePath;
				name = window.ReportName;
				translator = window.translator;
				return true;
			}

			//nothing seleceted
			return false;
		}

        /// <summary>
        /// Ctor for WindowPickReport. Window that displays list of known reports
        /// </summary>
        public WindowPickReport()
        {
			//initialize the filePath
			this.filePath = null;

            InitializeComponent();

			//get the staticType guid from the dataModel
			Guid staticTypeId = DataModel.ReportType.ReportTypeKeyReportTypeCode.Find(ReportType.StaticReport).ReportTypeId;

			//fill in the listbox with any reports from DataSet that are the StaticReport type
			this.listBox1.BeginInit();
			foreach (ReportRow row in DataModel.Report)
			{
				if (row.IsReportTypeIdNull() == false &&
					row.ReportTypeId == staticTypeId)
					this.listBox1.Items.Add(new ReportListBoxItem(row));
			}
			this.listBox1.EndInit();

			//disable ok and propertyies until there is a selection
			this.propBtn.IsEnabled = false;
			this.okBtn.IsEnabled = false;

			this.listBox1.SelectionChanged += new SelectionChangedEventHandler(listBox1_SelectionChanged);

			if (this.listBox1.Items.Count > 0)
				this.listBox1.SelectedIndex = 0;
        }

		/// <summary>
		/// handler for the listbox selection change
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void listBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			//if there is a selection enable the okButton and the properties button
			this.propBtn.IsEnabled = this.okBtn.IsEnabled = this.listBox1.SelectedItem != null;
		}

		/// <summary>
		/// get the values Name/Description/Xml from the ListBox item
		/// </summary>
		/// <param name="item">usually the selected item in the list box as ReportListBoxItem</param>
		/// <param name="createTmpFile">true if should create the file in the xml on disk</param>
		/// <param name="reportName">ref to the report Name</param>
		/// <param name="reportDesc">ref to the report Description</param>
		/// <param name="filePath">ref to the reportFilePath</param>
		private void GetItemValues(ReportListBoxItem item, bool createTmpFile, ref string reportName,
											ref string reportDesc, ref string filePath, ref FluidTrade.Reporting.Interfaces.IStaticReportTranslation translator)
		{
			if (item.Row != null)
			{
				//the item is pointing to a row in the dataModel
				ReportXmlHelper.XmlToTempFile(item.Row.Xaml, createTmpFile, out reportName, out reportDesc, out filePath, out translator);
			}
			else if (string.IsNullOrEmpty(item.LocalFilePath) == false)
			{
				//item is pointing at a local file
				reportName = filePath = item.LocalFilePath;
				reportDesc = string.Empty;
			}
			else if (string.IsNullOrEmpty(item.Xml) == false)
			{
				//item is pointing at a newly imported report
				ReportXmlHelper.XmlToTempFile(item.Xml, createTmpFile, out reportName, out reportDesc, out filePath, out translator);
			}
			else
			{
				//error
				return;
			}
		}

		/// <summary>
		/// handler for the import button click, Shows Import Window to user
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void importBtn_Click(object sender, ExecutedRoutedEventArgs e)
		{
			WindowImportProperties win = new WindowImportProperties(true, true);
			if (true.Equals(win.ShowDialog()))
			{
				//closed with ok so the dlg will have sent the reprt to the 
				//db but need to update the pick list and select the new item
				ReportListBoxItem newItem = new ReportListBoxItem(win.ReportName, win.ImportedXml);
				this.listBox1.Items.Add(newItem);
				this.listBox1.SelectedItem = newItem;
			}
		}

		/// <summary>
		/// handler for the property button click. Will show the properties window if
		/// the listbox has a selected item
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void propBtn_Click(object sender, ExecutedRoutedEventArgs e)
		{
			//get the selected item
			ReportListBoxItem item = this.listBox1.SelectedItem as ReportListBoxItem;
			if (item == null)
				return; //no selected item so return

			string reportName = null;
			string reportDesc = null;
			string filePath = null;
			FluidTrade.Reporting.Interfaces.IStaticReportTranslation translator = null;
			//get the values from the item
			this.GetItemValues(item, false, ref reportName, ref reportDesc, ref filePath, ref translator);

			//show the properties window
			WindowImportProperties win = new WindowImportProperties(reportName, reportDesc);
			win.ShowDialog();
		}

		/// <summary>
		/// handler for the local file button click, Shows select local file Window to user
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void localFileBtn_Click(object sender, ExecutedRoutedEventArgs e)
		{
			string localFile = WindowPickReportFromDisk.PickReport();
			if (string.IsNullOrEmpty(localFile))
				return;
			
			//have a new item add it to the list and select it
			ReportListBoxItem newItem = new ReportListBoxItem(localFile);
			this.listBox1.Items.Add(newItem);
			this.listBox1.SelectedItem = newItem;
		}

		/// <summary>
		/// handler for the OK button click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void okBtn_Click(object sender, ExecutedRoutedEventArgs e)
		{
			ReportListBoxItem item = this.listBox1.SelectedItem as ReportListBoxItem;
			if (item != null)
			{
				string reportDesc = null;
				this.GetItemValues(item, true, ref this.reportName, ref reportDesc, ref this.filePath, ref this.translator);

				if(string.IsNullOrEmpty(this.reportName))
				{
					MessageBox.Show("Report Name needed");
					return;
				}

				if (string.IsNullOrEmpty(this.reportName))
				{
					MessageBox.Show("Report Path needed");
					return;
				}

				this.DialogResult = true;
			
			}


			this.Close();
		}

		/// <summary>
		/// handler for the cancel button click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void cancelBtn_Click(object sender, ExecutedRoutedEventArgs e)
		{
			this.DialogResult = false;
			this.Close();
		}

		/// <summary>
		/// get path to file that was selected
		/// </summary>
		public String FilePath { get { return this.filePath; } }

		/// <summary>
		/// get name of the report that was selected
		/// </summary>
		public String ReportName { get { return this.reportName; } }


		/// <summary>
		/// nested class that is used for the listbox items. Overrides the 
		/// ToString() to define what is displayed while internally item 
		/// knows about localFiles, ReportRows, and newly imported files
		/// </summary>
		protected class ReportListBoxItem
		{
			/// <summary>
			/// report row for the list box item
			/// </summary>
			private ReportRow row;

			/// <summary>
			/// name for the list box item
			/// </summary>
			private string name;

			/// <summary>
			/// xml for the list box item
			/// </summary>
			private string xml;

			/// <summary>
			/// local file path for the list box item
			/// </summary>
			private string localFilePath;

			/// <summary>
			/// ctor
			/// </summary>
			/// <param name="row">ReportRow that the item is pointing to</param>
			public ReportListBoxItem(ReportRow row)
			{
				this.row = row;
			}

			/// <summary>
			/// ctor used for newly imported files
			/// </summary>
			/// <param name="name"></param>
			/// <param name="xml"></param>
			public ReportListBoxItem(string name, string xml)
			{
				this.name = name;
				this.xml = xml;
			}

			/// <summary>
			/// ctor for local files
			/// </summary>
			/// <param name="localFilePath"></param>
			public ReportListBoxItem(string localFilePath)
			{
				this.localFilePath = localFilePath;
				this.name = localFilePath;
			}

			/// <summary>
			/// get report row for the list box item
			/// </summary>
			public ReportRow Row { get { return this.row; } }

			/// <summary>
			/// get local file path for the list box item
			/// </summary>
			public string LocalFilePath { get { return this.localFilePath; } }

			/// <summary>
			/// get name for the list box item
			/// </summary>
			public string Name { get { return this.name; } }
			
			/// <summary>
			/// get xml for the list box item
			/// </summary>
			public string Xml { get { return this.xml; } }

			/// <summary>
			/// Override of ToString for listBox display
			/// </summary>
			/// <returns></returns>
			public override string ToString()
			{
				if (row == null)
					return name;

				return row.Name;
			}
		}
	}
}

namespace FluidTrade.Reporting.Windows.WindowPickReportCommands
{
	/// <summary>
	/// this could be a nested class, but cannot figure out how to get the xaml to pick it up
	/// so for now it is in its own namespace 
	/// 
	/// for the Button Commands
	/// </summary>
	public class ButtonCommands
	{
		private static RoutedUICommand import;
		private static RoutedUICommand properties;
		private static RoutedUICommand localFile;
		private static RoutedUICommand ok;
		private static RoutedUICommand cancel;
		static ButtonCommands()
		{
			// Initialize the commands.
			import = new RoutedUICommand("Import...", "Import", typeof(ButtonCommands));
			properties = new RoutedUICommand("Properties...", "Properties", typeof(ButtonCommands));
			localFile = new RoutedUICommand("Local File...", "LocalFile", typeof(ButtonCommands));
			ok = new RoutedUICommand("OK", "OK", typeof(ButtonCommands));
			cancel = new RoutedUICommand("Cancel", "Cancel", typeof(ButtonCommands));
		}

		/// <summary>
		/// Show the import window.
		/// </summary>
		public static RoutedUICommand Import
		{
			get { return import; }
		}

		/// <summary>
		/// Show the properties window.
		/// </summary>
		public static RoutedUICommand Properties
		{
			get { return properties; }
		}

		/// <summary>
		/// Show the LocalFile window.
		/// </summary>
		public static RoutedUICommand LocalFile
		{
			get { return localFile; }
		}

		/// <summary>
		/// Select the report.
		/// </summary>
		public static RoutedUICommand OK
		{
			get { return ok; }
		}

		/// <summary>
		/// cancel report selection.
		/// </summary>
		public static RoutedUICommand Cancel
		{
			get { return cancel; }
		}
	}
}
