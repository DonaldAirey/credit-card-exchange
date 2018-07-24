namespace FluidTrade.Core
{

	using System;
	using System.Configuration;
	using System.Reflection;
	using System.Xml;

	/// <summary>
	/// This object is used to parse a section of the configuration file containing information about the
	/// available persistent stores.
	/// </summary>
	public class ToolSectionHandler : IConfigurationSectionHandler
	{

		private const int TypeIndex = 0;
		private const int AssemblyIndex = 1;

		/// <summary>
		/// Parses a section of the configuration file for information about the persistent stores.
		/// </summary>
		/// <param name="parent">The configuration settings in a corresponding parent configuration section.</param>
		/// <param name="configContext">An HttpConfigurationContext when Create is called from the ASP.NET configuration system.
		/// Otherwise, this parameter is reserved and is a null reference.</param>
		/// <param name="section">The XmlNode that contains the configuration information from the configuration file. Provides direct access to the XML contents of the configuration section.</param>
		/// <returns>A ToolSection object created from the data in the configuration file.</returns>
		public object Create(object parent, object configContext, System.Xml.XmlNode section)
		{

			// Create an object to hold the data from the 'tool' section of the application configuration file.
			ToolSection toolSection = new ToolSection();

			try
			{

				// Read each of the nodes that contain information about the persistent stores and place them in the table.
				foreach (XmlNode xmlNode in section.SelectNodes("add"))
				{

					// The 'type' section of the configuration file is modeled after other places in the OS where the type and 
					// assembly information are combined in the same string.  A simpler method might have been to break aprart the
					// type string from the assembly string, but it's also a good idea to use standards where you find them.  In
					// any event, when the 'type' specification is done this way, the padded spaces need to be removed from the
					// values onced they're broken out from the original string.
					XmlAttribute keyAttribute = xmlNode.Attributes["key"];
					if (keyAttribute == null)
						throw new Exception("Syntax error in configuration file section 'tools'.");
					string name = keyAttribute.Value;

					// The text appears in the 'Tools' menu.
					XmlAttribute textAttribute = xmlNode.Attributes["text"];
					if (textAttribute == null)
						throw new Exception("Syntax error in configuration file section 'tools'.");
					string text = textAttribute.Value;

					// Pull apart the tool specification from the attributes.
					XmlAttribute toolSpecificationAttribute = xmlNode.Attributes["tool"];
					if (toolSpecificationAttribute == null)
						throw new Exception("Syntax error in configuration file section 'tools'.");
					string[] toolParts = toolSpecificationAttribute.Value.Split(new char[] {','});
					if (toolParts.Length != 2)
						throw new Exception("Syntax error in configuration file section 'tools'.");

					// Attempt to load the tool into memory and find the type used to instantiate the tool.
					Assembly toolAssembly = Assembly.Load(toolParts[ToolSectionHandler.AssemblyIndex].Trim());
					System.Type toolType = toolAssembly.GetType(toolParts[ToolSectionHandler.TypeIndex]);

					// Add the tool information to the section.  Each of these ToolInfo items describes what kind of object the 
					// tool is associated with and where to find the tool.
					toolSection.Add(new ToolInfo(name, text, toolType));

				}

			}
			catch (Exception exception)
			{

				// Make sure that any errors caught while trying to load the tool info is recorded in the log.  A system
				// administrator can look through these to figure out why the tool information isn't formatted correctly.
				EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);

			}

			// This object can be used to find a persistent store by nane and connect to it.
			return toolSection;

		}

	}

}
