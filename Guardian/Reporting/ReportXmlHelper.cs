using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace FluidTrade.Reporting
{
	/// <summary>
	/// Static helper class to serialize a file on disk to base64Encoded xml document
	/// and to deserialize an xml document with the same format to a temp file on disk
	/// </summary>
	public static class ReportXmlHelper
	{
		/// <summary>
		/// content xml tag
		/// </summary>
		private const string Content = "Content";

		/// <summary>
		/// description xml tag
		/// </summary>
		private const string Description = "Description";

		/// <summary>
		/// name xml tag
		/// </summary>
		private const string Name = "Name";

		/// <summary>
		/// reportDocument xml tag
		/// </summary>
		private const string ReportDocument = "ReportDocument";

		/// <summary>
		/// TranslationAssembly xml tag
		/// </summary>
		private const string TranslationAssembly = "TranslationAssembly";

		/// <summary>
		/// prefix used for all temporary files that are saved to disk
		/// </summary>
		public const string TmpPrefix = "_FTTMP";

		/// <summary>
		/// serialize a stream to xml document as base64Encoded content
		/// </summary>
		/// <param name="reportName">name of report will be stored in xml</param>
		/// <param name="reportDescription">description of report to be stored in xml</param>
		/// <param name="filePath">path of file on disk</param>
		/// <returns></returns>
		public static string FilesToXml(string reportName, string reportDescription, string reportFilePath, string translationFilePath)
		{
			string encodedReportFile = null;
			string encodedTranslationFile = null;
			Stream reportFileStream = null;
			reportFileStream = File.OpenRead(reportFilePath);
			if (reportFileStream != null)
			{
				using (BinaryReader binReader = new BinaryReader(reportFileStream))
				{
					Byte[] bytes = new Byte[10000000];//10mb is probabably overkill
					int index = 0;
					int count = 0;
					//loop while there is more data
					while ((count = binReader.Read(bytes, index, 10000)) > 0)
					{
						index += count;
					}

					//change the bytes to a base64 string
					encodedReportFile = Convert.ToBase64String(bytes, 0, index, Base64FormattingOptions.None);
				}

				reportFileStream.Close();
			}

			if (string.IsNullOrEmpty(translationFilePath) == false)
			{
				Stream translationFileStream = null;
				translationFileStream = File.OpenRead(translationFilePath);
				if (translationFileStream != null)
				{
					using (BinaryReader binReader = new BinaryReader(translationFileStream))
					{
						Byte[] bytes = new Byte[10000000];//10mb is probabably overkill
						int index = 0;
						int count = 0;
						//loop while there is more data
						while ((count = binReader.Read(bytes, index, 10000)) > 0)
						{
							index += count;
						}

						//change the bytes to a base64 string
						encodedTranslationFile = Convert.ToBase64String(bytes, 0, index, Base64FormattingOptions.None);
					}

					translationFileStream.Close();
				}
			}

			//write out the xml
			StringBuilder xmlSb = new StringBuilder();
			using (StringWriter stringWriter = new StringWriter(xmlSb))
			{
				using (System.Xml.XmlTextWriter xmlWriter = new System.Xml.XmlTextWriter(stringWriter))
				{
					xmlWriter.WriteStartElement(ReportDocument);
					xmlWriter.WriteAttributeString(Name, reportName);
					xmlWriter.WriteAttributeString(Description, reportDescription);
					if (string.IsNullOrEmpty(encodedReportFile) == false)
					{
						xmlWriter.WriteStartElement(Content);
						xmlWriter.WriteValue(encodedReportFile);
						xmlWriter.WriteEndElement();
					}

					if (string.IsNullOrEmpty(encodedTranslationFile) == false)
					{
						xmlWriter.WriteStartElement(TranslationAssembly);
						xmlWriter.WriteValue(encodedTranslationFile);
						xmlWriter.WriteEndElement();
					}

					xmlWriter.WriteEndElement();
				}
			}
			//return the xml
			return xmlSb.ToString();
		}

		/// <summary>
		/// deserialize xml document to name and description and optionally create a temp file with the content
		/// </summary>
		/// <param name="xml">xml containing data</param>
		/// <param name="createTmpFile">true to create a temp file with the contents of the xml</param>
		/// <param name="name">Name attribute in xml</param>
		/// <param name="description">description attribute</param>
		/// <param name="tmpFilePath">path of temp file create on disk</param>
		public static void XmlToTempFile(string xml, bool createTmpFile, out string name, out string description, out string tmpFilePath, out Interfaces.IStaticReportTranslation translator)
		{
			name = null;
			description = null;
			tmpFilePath = null;
			translator = null;
			if(createTmpFile == true)
			{
				//figure out where to create the temp file
				tmpFilePath = Environment.GetEnvironmentVariable("TMP", EnvironmentVariableTarget.User);

				if (string.IsNullOrEmpty(tmpFilePath))
					tmpFilePath = Environment.GetEnvironmentVariable("TEMP", EnvironmentVariableTarget.User);
				if (string.IsNullOrEmpty(tmpFilePath))
					tmpFilePath = Environment.GetEnvironmentVariable("TMP", EnvironmentVariableTarget.Process);
				if (string.IsNullOrEmpty(tmpFilePath))
					tmpFilePath = Environment.GetEnvironmentVariable("TEMP", EnvironmentVariableTarget.Process);
				if (string.IsNullOrEmpty(tmpFilePath))
					tmpFilePath = Environment.GetEnvironmentVariable("TMP", EnvironmentVariableTarget.Machine);
				if (string.IsNullOrEmpty(tmpFilePath))
					tmpFilePath = Environment.GetEnvironmentVariable("TEMP", EnvironmentVariableTarget.Machine);

				if (string.IsNullOrEmpty(tmpFilePath))
					return;
				tmpFilePath = string.Format(@"{0}\{1}{2}{3}.rpt", tmpFilePath, TmpPrefix, name, Environment.TickCount);

			}
		
			XmlToTempFile(xml, tmpFilePath, out name, out description, out translator);
		}

		/// <summary>
		/// deserialize xml document to name and description and optionally fill stream with content
		/// </summary>
		/// <param name="xml">xml containing data</param>
		/// <param name="name">Name attribute in xml</param>
		/// <param name="description">description attribute</param>
		/// <param name="outputStream">null if no content is not required</param>
		public static void XmlToTempFile(string xml, string tmpFilePath, out string name, out string description, out FluidTrade.Reporting.Interfaces.IStaticReportTranslation translator)
		{
			Stream outputStream = null;
			if(string.IsNullOrEmpty(tmpFilePath) == false)
				outputStream = File.Open(tmpFilePath, FileMode.CreateNew);
			name = null;
			description = null;
			string base64File = null;
			string base64TranslationContent = null;
			translator = null;
			//create the xmlReader to walk the xml doc
			using (StringReader stringReader = new StringReader(xml))
			{
				using (System.Xml.XmlTextReader xmlReader = new System.Xml.XmlTextReader(stringReader))
				{
					xmlReader.WhitespaceHandling = System.Xml.WhitespaceHandling.Significant;
					//read to get to the first node
					xmlReader.Read();

					//get the name and description attributes
					name = xmlReader.GetAttribute(Name);
					description = xmlReader.GetAttribute(Description);


					if (outputStream == null)
						return;//dont need to do anymore

					//get to the content
					xmlReader.Read();
					base64File = xmlReader.ReadElementContentAsString();

					//get to the translator
					if (xmlReader.NodeType != System.Xml.XmlNodeType.EndElement)
					{
						xmlReader.Read();
						base64TranslationContent = xmlReader.ReadContentAsString();
					}
					xmlReader.ReadEndElement();
				}
			}

			//if no outputStream or no content in xml then nothing left to do
			if (outputStream == null || string.IsNullOrEmpty(base64File))
				return;

			//convert the content to bytes
			Byte[] bytes = Convert.FromBase64String(base64File);

			//write the bytes to the stream
			using (BinaryWriter binWriter = new BinaryWriter(outputStream))
			{
				binWriter.Write(bytes);
				binWriter.Flush();
			}


			if (string.IsNullOrEmpty(base64TranslationContent) == false)
			{
				//convert the content to bytes
				Byte[] translationBytes = Convert.FromBase64String(base64TranslationContent);

				System.Reflection.Assembly translationAssembly = AppDomain.CurrentDomain.Load(translationBytes);
				Type[] types = translationAssembly.GetTypes();
				foreach (Type t in types)
				{
					if (t.GetInterface("FluidTrade.Reporting.Interfaces.IStaticReportTranslation") != null)
					{
						translator = Activator.CreateInstance(t) as FluidTrade.Reporting.Interfaces.IStaticReportTranslation;
						break;
					}
				}
			}
		}
	}
}
