namespace FluidTrade.Core.Windows
{

	using O2S.Components.PDFView4NET;
	using O2S.Components.PDFView4NET.Forms;
	using System;
	using System.IO;
	using System.Collections.Generic;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Forms.Integration;

	/// <summary>
	/// A PDF Viewer with mail-merge capabilities.
	/// </summary>
	public class PdfViewer : UserControl
	{

		// Private Instance Fields
		private WindowsFormsHost windowsFormHost;
		private PDFPageView pdfPageView;
		private static PdfViewer pdfViewer;

		/// <summary>
		/// Getter for PdfPageView 
		/// </summary>
		public PDFPageView PdfPageView
		{
			get { return pdfPageView;}
		}

		/// <summary>
		/// Identifies the Source dependency property.
		/// </summary>
		public static readonly DependencyProperty SourceProperty;

        /// <summary>
        /// Create the static resources required for a FluidTrade.FluidTradeClient.PdfViewer.
        /// </summary>
        static PdfViewer()
        {

            // Source Property
            PdfViewer.SourceProperty = DependencyProperty.Register(
                "Source",
                typeof(Stream),
                typeof(PdfViewer),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnSourceChanged)));

        }

		/// <summary>
		/// Create a PDF Viewer.
		/// </summary>
		public PdfViewer()
		{

			// Initialize the object.
			this.windowsFormHost = new WindowsFormsHost();
			this.pdfPageView = new PDFPageView();

			// Configure the control.  The PDF Viewer from O2 Solutions is a Forms based control, but the desired usage is WPF.  The generic UserControl is
			// given a special control that can host this Windows Forms control.  This window hierarchy allows the consumer of this library to be ignorant of the
			// WindowsFormsIntegration assembly.
			this.Content = this.windowsFormHost;
			this.windowsFormHost.Child = this.pdfPageView;

			// This serial number prevents the 'Evaluation Copy' message from appearin in the control.  It validates that we've got a developer license to
			// build using this control.
			this.pdfPageView.Document = new PDFDocument();
			this.pdfPageView.Document.SerialNumber = "PDFVW-6ATTA-DK6XD-A2XTO-AIQUM-3ECYE";

		}

		public void SetPageDisplayLayout(PDFPageDisplayLayout pageDisplayLayout)
		{
			pdfViewer.pdfPageView.PageDisplayLayout = pageDisplayLayout;
		}

		public void GoToPreviousPage()
		{
			if (this.pdfPageView.PageNumber > 0)
			{
				this.pdfPageView.PageNumber--;
				this.UpdateLayout();
			}
		}

		public void GoToNextPage()
		{
			if (this.pdfPageView.PageNumber < this.pdfPageView.Document.PageCount - 1)
			{
				this.pdfPageView.PageNumber++;
				this.UpdateLayout();
			}
		}

		/// <summary>
		/// Gets or sets the Source.
		/// </summary>
		public Stream Source
		{
			get { return (Stream)this.GetValue(PdfViewer.SourceProperty); }
			set { this.SetValue(PdfViewer.SourceProperty, value); }
		}

		/// <summary>
		/// Handles a change to the Source property.
		/// </summary>
		/// <param name="dependencyObject">The object that owns the property.</param>
		/// <param name="dependencyPropertyChangedEventArgs">A description of the changed property.</param>
		private static void OnSourceChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{

			// Extract the strongly typed variables from the generic parameters.
			pdfViewer = dependencyObject as PdfViewer;
			Stream stream = (Stream)dependencyPropertyChangedEventArgs.NewValue;

			// Provide the ability to view more than one page.
			pdfViewer.pdfPageView.PageDisplayLayout = PDFPageDisplayLayout.OneColumn;

			try
			{
				if (stream != null)
				{
					// Load the new stream into the document in the viewer.
					pdfViewer.pdfPageView.Document.Load(stream);
				}
				else
				{
					pdfViewer.pdfPageView.Document.Dispose();
				}
			}
			catch (ArgumentOutOfRangeException)
			{
				// Swallow exception, doing nothing there this a third party bug that we are ignoring intentionally.
				// as there was no apparent way to clear out the document.
			}

		}

		public void MergeFields(Dictionary<string, string> keyValuePairList)
		{

			// Apply each of the mail-merge fields with the document.
			foreach (KeyValuePair<string, string> keyValuePair in keyValuePairList)
			{
				PDFField pdfField = this.pdfPageView.Document.Form.Fields[keyValuePair.Key];
				if (pdfField != null)
					pdfField.Value = keyValuePair.Value;
			}

		}

	}

}
