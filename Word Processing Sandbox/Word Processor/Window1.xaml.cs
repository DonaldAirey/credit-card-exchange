namespace FluidTrade
{

	using Aspose.Words;
	using Aspose.Words.Fields;
	using FluidTrade.Office;
	using System;
	using System.IO;
	using System.Collections.Generic;
	using System.Windows;

	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class Window1 : Window
	{
		public Window1()
		{
			InitializeComponent();
			Window1.ReadDocument(@"C:\Users\Donald Roy Airey\Documents\Visual Studio 2008\Projects\Word Processing Sandbox\Word Processor\National Holdings Settlement.docx");
		}

		/// <summary>
		/// 1. Create a new package as a Word document
		/// 2. Add a style.xml part
		/// 3. Add an embedded image part
		/// 4. Create the document.xml part content 
		/// </summary>
		/// <returns>File path location or error message</returns>
		public static void ReadDocument(String fileName)
		{
			try
			{

				// Read the file document into the DOM.
				Byte[] sourceDocument = default(Byte[]);
				using (FileStream inputStream = new FileStream(fileName, FileMode.Open))
				{
					sourceDocument = new Byte[inputStream.Length];
					inputStream.Read(sourceDocument, 0, sourceDocument.Length);
				}

				// This dictionary is used for the mail merge operation.
				Dictionary<String, Object> dictionary = new Dictionary<string, object>();
				dictionary.Add("PayeeCompanyName", "Central Banking");
				dictionary.Add("PayeeAddress1", "6 Bay Place");
				dictionary.Add("PayeeAddress2", null);
				dictionary.Add("PayeeCity", "Huntington");
				dictionary.Add("PayeeProvinceAbbreviation", "NY");
				dictionary.Add("PayeePostalCode", "11743");
				dictionary.Add("DebtorLastName", "Franken");
				dictionary.Add("DebtorFirstName", "Al");
				dictionary.Add("DebtorAddress1", "203 Huntington Avenue");
				dictionary.Add("DebtorAddress2", "Apt. 2");
				dictionary.Add("DebtorCity", "Boston");
				dictionary.Add("DebtorProvinceAbbreviation", "MA");
				dictionary.Add("DebtorPostalCode", "02109");
				dictionary.Add("DebtorSalutation", "Mr.");
				dictionary.Add("DebtorOriginalAccountNumber", "2033 2838 8281 3822");
				dictionary.Add("AccountBalance", 3402.22M);
				dictionary.Add("SettlementAmount", 1500.00M);
				dictionary.Add("PaymentStartDate", DateTime.Parse("7/31/2009"));

				// Merge the dictionary with the contents of the loaded document and evaluate all the fields.
				MailMerge mailMerge = new MailMerge();
				MemoryStream memoryStream = mailMerge.CreateDocument(sourceDocument, dictionary);

				// Write the generated data to the output PDF file.
				using (FileStream outputStream = new FileStream("../../document.pdf", FileMode.OpenOrCreate))
				{
					Byte[] outputBuffer = memoryStream.ToArray();
					outputStream.Write(outputBuffer, 0, outputBuffer.Length);
				}
	
			}
			catch (Exception exception)
			{
				Console.WriteLine("{0}, {1}", exception.Message, exception.StackTrace);
			}
		}

	}

}
