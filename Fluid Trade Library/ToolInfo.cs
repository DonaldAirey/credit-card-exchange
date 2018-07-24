namespace FluidTrade.Core
{


    /// <summary>
	/// Information about a tool from the configuration file.  This contains information about what class of objects
	/// the tool is associated with, the name of the assembly where the tool is found, and the Type of the tool.
	/// </summary>
	public class ToolInfo
	{

		// Private Members
		public readonly string Name;
		public readonly string Text;
		public readonly System.Type ToolType;

		/// <summary>
		/// Initializes information about a tool.
		/// </summary>
		/// <param name="name">The object with which this tool is associated.</param>
		/// <param name="assemblyName">The name of the assembly where the tool is found.</param>
		/// <param name="TypeName">The Type of the tool.</param>
		public ToolInfo(string name, string text, System.Type toolType)
		{

			// Initialize the object.
			this.Name = name;
			this.Text = text;
			this.ToolType = toolType;

		}

	}
	
}
