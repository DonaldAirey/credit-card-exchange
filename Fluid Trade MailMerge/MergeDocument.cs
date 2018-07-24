namespace FluidTrade.Office
{

	using Aspose.Words;
	using Aspose.Words.Fields;
	using System;
	using System.Collections.Generic;
	using System.IO;

	/// <summary>
	/// An Aspose.Document capable of mail merging.
	/// </summary>
	/// <copyright>Copyright © 2009 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public class MergeDocument : Document
	{

		// Private Instance Fields
		private Dictionary<String, Object> dictionary;
		private List<WordField> fields;

		/// <summary>
		/// Create a MergeDocument.
		/// </summary>
		public MergeDocument()
			: base()
		{

			// Initialize the object
			this.dictionary = new Dictionary<string, object>();
			this.fields = new List<WordField>();

			// Recursively parse the fields out of the document
			MergeDocument.ParseFields(this, this.fields);

		}

		/// <summary>
		/// Create a MergeDocument.
		/// </summary>
		/// <param name="fileName">The name of a file that supplies the content of the document.</param>
		public MergeDocument(string fileName)
			: base(fileName)
		{

			// Initialize the object
			this.dictionary = new Dictionary<string, object>();
			this.fields = new List<WordField>();

			// Recursively parse the fields out of the document
			MergeDocument.ParseFields(this, this.fields);

		}

		/// <summary>
		/// Create a MergeDocument.
		/// </summary>
		/// <param name="stream">A stream used as a source for this document.</param>
		public MergeDocument(Stream stream)
			: base(stream)
		{

			// Initialize the object
			this.dictionary = new Dictionary<string, object>();
			this.fields = new List<WordField>();

			// Recursively parse the fields out of the document
			MergeDocument.ParseFields(this, this.fields);

		}

		/// <summary>
		/// Gets or sets the dictionary of data elements to be used for the mail merge.
		/// </summary>
		public Dictionary<String, Object> Dictionary
		{
			get
			{
				return this.dictionary;
			}
			set
			{
				this.dictionary = value;
			}
		}

		/// <summary>
		/// Evaluate all the fields in a document.
		/// </summary>
		public void Evaluate()
		{

			// Recursively evaluate each of the MERGEFIELD items.
			foreach (WordField wordField in this.fields)
			{
				wordField.Evaluate();
			}

		}
		/// <summary>
		/// Recursively searches the composite node for fields and constructs a hierarchical organization of those fields.
		/// </summary>
		/// <param name="compositeNode">A node that can be composed of other nodes.</param>
		/// <param name="wordFields">A hierarchical list of the Word fields found in the composite node or it's descendants.</param>
		private static void ParseFields(CompositeNode parentNode, List<WordField> wordFields)
		{

			// This will recursively search the document object model for any fields at any depth and constructs a hierarchical organization of the fields
			// found.
			Node childNode = parentNode.FirstChild;
			while (childNode != null)
			{

				// Composite nodes will be searched recursively for child fields.
				if (childNode.IsComposite)
					MergeDocument.ParseFields(childNode as CompositeNode, wordFields);

				// When a field is identified based on the starting node, a new WordField is generated from the stream of nodes in the document.  These fields
				// classes are easier to manage than an distinguised stream of nodes.
				if (childNode.NodeType == NodeType.FieldStart)
				{
					WordField wordField = WordField.CreateField(childNode as FieldStart);
					wordFields.Add(wordField);
					childNode = wordField.FieldEnd;
				}

				// Test the next node in the stream for the presence of a field.
				childNode = childNode.NextSibling;

			}

		}

	}

}
