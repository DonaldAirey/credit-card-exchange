namespace FluidTrade.Office
{

	using FluidTrade.Core;
	using Aspose.Words;
	using Aspose.Words.Fields;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Runtime.InteropServices;
	using System.Reflection;

	/// <summary>
	/// Creates a merged Word document from a template and merge fields.
	/// </summary>
	public class MailMerge : IMailMerge
	{

		// Private Constants
		const String licenseResource = "Aspose.Words.lic";

		/// <summary>
		/// Initialize the static resources required for this class.
		/// </summary>
		static MailMerge()
		{

			// This validates the third party library with the OEM license from Asprose.
			License license = new License();
			license.SetLicense(MailMerge.licenseResource);

		}

		/// <summary>
		/// Creates a mail merged document from a template and a collection of merge fields.
		/// </summary>
		/// <param name="sourceDocument">Byte array representation of a docx document.</param>
		/// <param name="dictionary">Dictionary list of mail merge parameters.</param>
		public MemoryStream CreateDocument(Byte[] sourceDocument, Dictionary<String, Object> dictionary)
		{

			// Create a Word Processing Document Object Model from the DOCX source.
			MemoryStream sourceStream = new MemoryStream(sourceDocument);
			MergeDocument mergeDocument = new MergeDocument(sourceStream);
			mergeDocument.Dictionary = dictionary;

			// This will evaluate all the fields in the document using the data dictionary supplied.
			mergeDocument.Evaluate();

			// Now that the mail merge is completed, export the DOCX file to a PDF format.
			MemoryStream destinationStream = new MemoryStream();
			mergeDocument.Save(destinationStream, SaveFormat.Pdf);
			return destinationStream;

		}

	}

}
