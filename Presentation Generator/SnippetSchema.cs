namespace FluidTrade.PresentationGenerator
{

	using System;
    using System.Xml;

    /// <summary>
	/// A description of a property setter.
	/// </summary>
	public class SnippetSchema
	{

		// Private Instance Fields
		protected String value;

		/// <summary>
		/// Creates a loose set of free form code.
		/// </summary>
		/// <param name="xmlNode">The XML Schema extensions that describe the query.</param>
		public SnippetSchema(XmlNode xmlNode)
		{

			// Extract the loose code from the attributes.
			foreach (XmlNode childNode in xmlNode.ChildNodes)
				if (childNode is XmlText)
				{
					XmlText xmlText = childNode as XmlText;
					this.value = xmlText.Data;
				}

		}

		/// <summary>
		/// Gets the value of the property to be set.
		/// </summary>
		public String Value
		{
			get { return this.value; }
		}

	}

}
