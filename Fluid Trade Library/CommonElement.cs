 namespace FluidTrade.Core
{

	using System;
	using System.Xml;

	/// <summary>
	/// A common Xml Element for the XmlDocument.
	/// </summary>
	public class CommonElement : XmlElement
	{

		/// <summary>
		/// Creates a common element for the XmlDocument.
		/// </summary>
		/// <param name="name">The local name of the node.</param>
		/// <param name="xmlDocument">The parent document.</param>
		public CommonElement(string name, XmlDocument xmlDocument) :
			base(string.Empty, name, string.Empty, xmlDocument) {}

		/// <summary>
		/// Creates and adds an attribute to the SectorElement.
		/// </summary>
		/// <param name="name">Name of the attribute.</param>
		/// <param name="attributeValue">The attribute's value.</param>
		public void AddAttribute(string attributeName, bool attributeValue)
		{

			// Create the attribute and append it to the current SectorElement.  Set the value to the text passed into
			// this helper function.
			XmlAttribute xmlAttribute = this.OwnerDocument.CreateAttribute(attributeName);
			this.Attributes.Append(xmlAttribute);
			xmlAttribute.Value = attributeValue.ToString();

		}

		/// <summary>
		/// Creates and adds an attribute to the SectorElement.
		/// </summary>
		/// <param name="name">Name of the attribute.</param>
		/// <param name="attributeValue">The attribute's value.</param>
		public void AddAttribute(string attributeName, int attributeValue)
		{

			// Create the attribute and append it to the current SectorElement.  Set the value to the text passed into
			// this helper function.
			XmlAttribute xmlAttribute = this.OwnerDocument.CreateAttribute(attributeName);
			this.Attributes.Append(xmlAttribute);
			xmlAttribute.Value = attributeValue.ToString();

		}

		/// <summary>
		/// Creates and adds an attribute to the SectorElement.
		/// </summary>
		/// <param name="name">Name of the attribute.</param>
		/// <param name="attributeValue">The attribute's value.</param>
		public void AddAttribute(string attributeName, decimal attributeValue)
		{

			// Create the attribute and append it to the current SectorElement.  Set the value to the text passed into
			// this helper function.
			XmlAttribute xmlAttribute = this.OwnerDocument.CreateAttribute(attributeName);
			this.Attributes.Append(xmlAttribute);
			xmlAttribute.Value = attributeValue.ToString();

		}

		public void AddAttribute(string attributeName, string attributeValue)
		{

			// Create the attribute and append it to the current SectorElement.  Set the value to the text passed into
			// this helper function.
			XmlAttribute xmlAttribute = this.OwnerDocument.CreateAttribute(attributeName);
			this.Attributes.Append(xmlAttribute);
			xmlAttribute.Value = attributeValue;

		}

		/// <summary>
		/// Adds the specified node in order of the 'sortOrder' attribute to the children of this node.
		/// </summary>
		/// <param name="xmlChild">The child element to add to the parent element.</param>
		public void InsertBySortOrder(CommonElement xmlChild)
		{

			// Get the attribute used in the sorting.
			string childSortCode = xmlChild.GetAttribute("SortOrder");

			// This does a quick insert sort based on the value of the 'sortOrder' attribute.
			foreach (XmlElement xmlElement in this)
				if (Convert.ToInt32(xmlElement.GetAttribute("SortOrder")) > Convert.ToInt32(childSortCode))
				{
					this.InsertBefore(xmlChild, xmlElement);
					return;
				}

			// If the 'sortOrder' attribute is larger than any of the siblings, it goes at the end of the list.
			this.AppendChild(xmlChild);

		}

		/// <summary>
		/// Adds the specified node in order of the 'name' attribute to the children of this node.
		/// </summary>
		/// <param name="xmlChild">The child element to add to the parent element.</param>
		public void InsertByName(CommonElement xmlChild)
		{

			// Get the attribute used in the sorting.
			string childSortCode = xmlChild.GetAttribute("Name");

			// This does a quick insert sort based on the value of the 'name' attribute.
			foreach (XmlElement xmlElement in this)
				if (String.Compare(xmlElement.GetAttribute("Name"), childSortCode) > 0)
				{
					this.InsertBefore(xmlChild, xmlElement);
					return;
				}

			// If the 'name' attribute is larger than any of the siblings, it goes at the end of the list.
			this.AppendChild(xmlChild);

		}

	}
	
}
