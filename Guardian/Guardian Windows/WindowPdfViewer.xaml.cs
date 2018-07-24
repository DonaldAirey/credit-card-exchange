using O2S.Components.PDFRender4NET;
using O2S.Components.PDFRender4NET.Printing;
using System.Drawing.Printing;
using Microsoft.Win32;
using System.Printing;

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
using System.Windows.Xps.Packaging;
using System.IO;


namespace FluidTrade.Guardian
{

	/// <summary>
	/// Interaction logic for WindowPdfViewer.xaml
	/// </summary>
	public partial class WindowPdfViewer : Window
	{

		/// <summary>
		/// Windodw to display the Settlement letter.
		/// </summary>
		public WindowPdfViewer()
		{
			InitializeComponent();
		}


		/// <summary>
		/// Set the source on the pdfViewer.
		/// </summary>
		/// <param name="Source"></param>
		public void setPdfViewerSource(Stream Source)
		{
			this.pdfViewer.Source = Source;
			this.pdfViewer.SetPageDisplayLayout(O2S.Components.PDFView4NET.PDFPageDisplayLayout.SinglePage);
		}
		
		private void PrintButton_Click(object sender, RoutedEventArgs e)
		{
			// Load the PDF file.
			PDFFile file = PDFFile.Open(this.pdfViewer.Source);
			file.SerialNumber = "PDFVW-6ATTA-DK6XD-A2XTO-AIQUM-3ECYE";
			// Create a default printer settings to print on the default printer.
			PrinterSettings settings = new PrinterSettings();
			PDFPrintSettings pdfPrintSettings = new PDFPrintSettings(settings);
			pdfPrintSettings.PageScaling = PageScaling.FitToPrinterMargins;

			// Create the print dialog object and set options
			PrintDialog pDialog = new PrintDialog();
			pDialog.PageRangeSelection = PageRangeSelection.AllPages;
			pDialog.UserPageRangeEnabled = true;

			// Display the dialog. This returns true if the user presses the Print button.
			Nullable<Boolean> print = pDialog.ShowDialog();
			if (print == true)
			{
				// Get the settings from the PrintDialog and manually set each one.
				// If there are more option that need to be set the we need to identify them and set them manually.

				pdfPrintSettings.PrinterSettings.PrinterName = pDialog.PrintQueue.FullName;

				pdfPrintSettings.PrinterSettings.Copies = (short) pDialog.PrintTicket.CopyCount;
				pdfPrintSettings.PrinterSettings.FromPage = pDialog.PageRange.PageFrom;
				pdfPrintSettings.PrinterSettings.ToPage= pDialog.PageRange.PageTo;

				// Print the PDF file.
				file.Print(pdfPrintSettings);
			}
			
			file.Dispose();

		}

		private void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			// Displays a SaveFileDialog so the user can save the settlement letter.
			SaveFileDialog saveFileDialog1 = new SaveFileDialog();
			// This showing the "All Files" filter cause problem in the UI //TODO: Figure out why this is happen later could be a MS bug.
			//saveFileDialog1.Filter = "PDF File|*.pdf|All Files|*.*";
			saveFileDialog1.Filter = "PDF File|*.pdf";
			saveFileDialog1.Title = "Save Settlement Letter";
			saveFileDialog1.RestoreDirectory = true;
			saveFileDialog1.FileName = "SettlementLetter"; // Default file name 
			saveFileDialog1.DefaultExt = ".pdf"; // Default file extension 

			// If ok button is pressed and the file name is not an empty string open it for saving.
			if ((saveFileDialog1.ShowDialog() == true) && (saveFileDialog1.FileName != ""))
			{

				O2S.Components.PDFView4NET.PDFPageView pdfPageView = this.pdfViewer.PdfPageView;
				pdfPageView.Document.Save(saveFileDialog1.FileName);

			}
		}

		private void PreviousPageButton_Click(object sender, RoutedEventArgs e)
		{
			if (this.pdfViewer.PdfPageView.PageNumber > 0)
			{
				this.pdfViewer.SetPageDisplayLayout(O2S.Components.PDFView4NET.PDFPageDisplayLayout.SinglePage);
				this.pdfViewer.GoToPreviousPage();
				this.NextButton.IsEnabled = true;
			}

			if (this.pdfViewer.PdfPageView.PageNumber <= 0)
			{
				this.PreviousButton.IsEnabled = false;
			}

		}

		private void NextPageButton_Click(object sender, RoutedEventArgs e)
		{
			if (this.pdfViewer.PdfPageView.PageNumber < this.pdfViewer.PdfPageView.Document.PageCount - 1)
			{
				this.pdfViewer.SetPageDisplayLayout(O2S.Components.PDFView4NET.PDFPageDisplayLayout.SinglePage);
				this.pdfViewer.GoToNextPage();
				this.PreviousButton.IsEnabled = true;
			}

			if (this.pdfViewer.PdfPageView.PageNumber >= this.pdfViewer.PdfPageView.Document.PageCount - 1)
			{
				this.NextButton.IsEnabled = false;
			}

		}

	}
}
