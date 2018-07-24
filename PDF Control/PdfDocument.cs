namespace FluidTrade.Core.Windows
{

	using System;
	using System.Collections.Generic;
	using System.IO;
	using O2S.Components.PDF4NET;
	using O2S.Components.PDF4NET.Forms;
	using O2S.Components.PDF4NET.Annotations;

    /// <summary>
    /// A PDF Document.
    /// </summary>
    public class PdfDocument
    {

        // Private Instance Fields
        private PDFDocument pdfDocument;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public PdfDocument()
        {

			// Initialize the object.
			this.pdfDocument = new PDFDocument();
		
		}

        /// <summary>
        /// Load a PDF document via stream.
        /// </summary>
        /// <param name="pdfStream"></param>
        public void LoadPDF(Stream pdfStream)
        {

            // Load a PDF file from stream.
            this.pdfDocument = new PDFDocument(pdfStream);

        }

        /// <summary>
        /// Fill the PDF form with information from the mail-merge fields.
        /// </summary>
        /// <param name="keyValuePairList">List of key-value pair.  key is name field and value is the string value to be displayed.</param>
		public void MergeFields(Dictionary<string, string> keyValuePairList)
		{

			// Apply each of the mail-merge fields with the document.
			foreach (KeyValuePair<string, string> keyValuePair in keyValuePairList)
			{
				PDFField pdfField = this.pdfDocument.Form.Fields[keyValuePair.Key];
				if (pdfField != null)
					pdfField.Value = keyValuePair.Value;
			}

		}

    }

}
