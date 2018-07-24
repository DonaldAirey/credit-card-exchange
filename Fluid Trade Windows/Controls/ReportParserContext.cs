namespace FluidTrade.Core.Windows.Controls
{
    using System.Linq;
    using System.Reflection;
	using System.Windows.Markup;
	using System.Xml.Linq;

	/// <summary>
	/// A context used for parsing XAML source code.
	/// </summary>
	public class ReportParserContext : ParserContext
	{

		/// <summary>
		/// Creates a context that can be used for parsing XAML source code.
		/// </summary>
		/// <param name="xDocument">The XAML document that is to be parsed.</param>
		/// <param name="assemblyList">A List of assemblies that provide namespaces used in the XAML document.</param>
		public ReportParserContext(XDocument xDocument)
		{

			// This object provides an association between the namespaces declard in the XAML source and the CLR namespaces and 
			// the assemblies where those CLR namespaces can be found.
			this.XamlTypeMapper = new XamlTypeMapper(new string[] { });

			// The main idea here is to map the XML prefix character in the XAML source code to the assemlby containing the type
			// information.  For standard URI namespace FluidTrade.Core.Windows.Controls, the prefix is mapped to the URI namespace FluidTrade.Core.Windows.Controls then the URI
			// namespace FluidTrade.Core.Windows.Controls mapped to an assembly.  XAML has a special URI specification that can be used to direct the parser to a
			// specific CLR namespace FluidTrade.Core.Windows.Controls a specific assembly also.
			foreach (XAttribute xAttribute in xDocument.Root.Attributes().Where(attribute => attribute.IsNamespaceDeclaration))
			{

				// The prefix and the URI are extracted from each of the attributes containing the namespace FluidTrade.Core.Windows.Controls
				string prefix = xAttribute.Name.LocalName == "xmlns" ? string.Empty : xAttribute.Name.LocalName;
				string namespaceUri = xAttribute.Value;

				// A namespace FluidTrade.Core.Windows.Controls with the text 'clr-namespace' is a special URI which instructs the parser to explicitly
				// look for type information in the given CLR namespace FluidTrade.Core.Windows.Controls the optionally specified assembly.  Note that the XAML
				// mapper appears to be able to find the XAML namespaces in assemblies that have an explicit declaration.  This
				// processing here is for the ad-hoc declarations.
				if (namespaceUri.StartsWith("clr-namespace"))
				{

					// The URI is ripped apart to extract the CLR namespace FluidTrade.Core.Windows.Controls the optional assembly information.
					string assemblyName = Assembly.GetEntryAssembly().FullName;
					string targetNamespace = string.Empty;
					string[] tokens = namespaceUri.Split(';');
					string[] clrTokens = tokens[0].Split(':');
					if (clrTokens.Length == 2)
						targetNamespace = clrTokens[1];
					if (tokens.Length == 2)
					{
						string[] assemblyTokens = tokens[1].Split('=');
						assemblyName = assemblyTokens.Length == 2 && assemblyTokens[0] == "assembly" ?
							assemblyTokens[1] : Assembly.GetCallingAssembly().FullName;
					}

					// This creates a relationship between the XAML namespace FluidTrade.Core.Windows.Controls the CLR namespace FluidTrade.Core.Windows.Controls the assembly where that CLR
					// namespace FluidTrade.Core.Windows.Controls be found.  Note that a single XAML namespace FluidTrade.Core.Windows.Controls be mapped to several CLR namespaces across
					// several assemblies.
					this.XamlTypeMapper.AddMappingProcessingInstruction(namespaceUri, targetNamespace, assemblyName);

				}

				// This provides a mapping between the prefix used in the XAML to the URI that is the real namespace.  Note that 
				// the URI is further mapped to a CLR namespace FluidTrade.Core.Windows.Controls an assembly either explicitly or implicitly.
				this.XmlnsDictionary.Add(prefix, namespaceUri);

			}

		}

	}

}
