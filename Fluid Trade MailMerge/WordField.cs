namespace FluidTrade.Office
{

	using Aspose.Words;
	using Aspose.Words.Fields;
	using System;
	using System.Collections;
	using System.Collections.Generic;

	/// <summary>
	/// Defines a field as a hierarchical group of nodes.
	/// </summary>
	/// <copyright>Copyright © 2009 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public abstract class WordField : IEnumerable
	{

		// Private Instance Fields
		private Dictionary<Node, WordField> endDictionary;
		private FieldStart fieldStart;
		private FieldEnd fieldEnd;
		private FieldSeparator fieldSeperator;
		private Dictionary<Node, WordField> startDictionary;

		/// <summary>
		/// Create a hierarchical group of nodes from the start of the field.
		/// </summary>
		/// <param name="fieldStart">The node which represents the start of a field.</param>
		public WordField(FieldStart fieldStart)
		{

			// Create the object.
			this.endDictionary = new Dictionary<Node, WordField>();
			this.startDictionary = new Dictionary<Node, WordField>();
			this.fieldStart = fieldStart;

			// This will find the end of the field and acts as a kind of delimiter.  Since the DOM structure makes heavy use of linked lists, it can also act
			// to join the end of a field to the start and back again when navigating through the document.
			Boolean parsing = true;
			Node node = this.fieldStart.NextSibling;
			while (parsing)
			{

				// This is the field that is going to be parsed out of the stream.
				WordField wordField = default(WordField);

				// Each node is parsed to determine what kind of an object has been discovered here.
				switch (node.NodeType)
				{

				case NodeType.FieldStart:

					// This will recurse into any of the embedded fields creating a hierarchical order of fields that can be evaluated from the inside out.
					wordField = CreateField(node as FieldStart);

					// Any field found in the field field result area is ignored.  All other fields are added to a collection that can be indexed sequentially
					// using an enumerator or directly using the dictionary.
					this.startDictionary.Add(wordField.FieldStart, wordField);
					this.endDictionary.Add(wordField.FieldEnd, wordField);

					// This moves the parser to the position after the field no matter how many levels of embedded fields were included.
					node = wordField.FieldEnd;

					break;

				case NodeType.FieldSeparator:

					// This indicates where the literal portion of a field starts.
					this.fieldSeperator = node as FieldSeparator;

					break;

				case NodeType.FieldEnd:

					// The end of the field is part of this obect and used when navigating forward or backward through the nodes of a paragraph.
					this.fieldEnd = node as FieldEnd;

					// When the final field is found the parsing of this field is completed.
					parsing = false;

					break;

				}

				// The parsing continues until the last field node is discovered.
				node = node.NextSibling;

			}

		}

		/// <summary>
		/// Gets the node that ends the field.
		/// </summary>
		public FieldEnd FieldEnd
		{
			get
			{ 
				return this.fieldEnd;
			}
		}

		/// <summary>
		/// Gets the node that starts the field.
		/// </summary>
		public FieldStart FieldStart
		{
			get
			{
				return this.fieldStart;
			}
		}

		/// <summary>
		/// Gets the node that starts the field.
		/// </summary>
		public FieldSeparator FieldSeperator
		{
			get
			{
				return this.fieldSeperator;
			}
		}

		/// <summary>
		/// Gets the type of this field.
		/// </summary>
		public FieldType FieldType
		{
			get
			{
				return this.fieldStart.FieldType;
			}
		}

		/// <summary>
		/// Gets the parent document which contains the data required for evaluating fields.
		/// </summary>
		public MergeDocument MergeDocument
		{
			get
			{
				return this.FieldStart.Document as MergeDocument;
			}
		}

		/// <summary>
		/// Creates a strongly-typed field from the tokens in the WordProcessing DOM.
		/// </summary>
		/// <param name="fieldStart">The starting point for the field.</param>
		/// <returns>A strongly typed field based on the tokens found in the stream.</returns>
		public static WordField CreateField(FieldStart fieldStart)
		{

			// This is what is created if the type isn't recognized.
			WordField wordField = default(WordField);

			// The field type indicates what parser is used to evaluate the field.
			switch (fieldStart.FieldType)
			{

			case FieldType.FieldDate:

				// Create a new 'DATE' field.
				wordField = new DateField(fieldStart);
				break;

			case FieldType.FieldIf:

				// Create a new 'IF' field.
				wordField = new IfField(fieldStart);
				break;

			case FieldType.FieldMergeField:

				// Create a new 'MERGEFIELD' field.
				wordField = new MergeField(fieldStart);
				break;

			default:

				// All other fields are unhandled but parsed.
				wordField = new UnhandledField(fieldStart);
				break;

			}

			// This is a generic field that can be evaluated.
			return wordField;

		}

		/// <summary>
		/// Replaces the field with the evaluated text.
		/// </summary>
		public virtual void Evaluate()
		{

			// Recursively evaluate each of the child fields before the parent is evaluated.
			foreach (WordField wordField in this)
			{
				wordField.Evaluate();
			}

		}

		/// <summary>
		/// Randomly access the child fields of a given field.
		/// </summary>
		/// <param name="index">The end node of a WordField.</param>
		/// <returns>The WordField to which this end node belongs.</returns>
		public WordField FindByEndNode(Node index)
		{
			return this.endDictionary[index];
		}

		/// <summary>
		/// Randomly access the child fields of a given field.
		/// </summary>
		/// <param name="index">The start node of a WordField.</param>
		/// <returns>The WordField to which this end node belongs.</returns>
		public WordField FindByStartNode(Node node)
		{
			return this.startDictionary[node];
		}

		/// <summary>
		/// Returns an enumerator that iterates through the child fields.
		/// </summary>
		/// <returns>An enumerator that can be used to iterate through the child fields.</returns>
		public IEnumerator GetEnumerator()
		{

			// The dictionary of child values provides an enumerator that can be used to iterate through the collection.
			return this.endDictionary.Values.GetEnumerator();

		}

		/// <summary>
		/// Clears the literal data from a field.
		/// </summary>
		public void ClearLiteral()
		{

			// This will remove the literal portion of a field.
			Node literalNode = this.fieldSeperator.NextSibling;
			while (literalNode != this.fieldEnd)
			{

				// When parsed, the fields that appear in the literal area are added to the list of child fields that need to be evaluated. However, this can
				// lead to errors, especially if the field doesn't exist any more.  Removing the field from the list of children inhibits the evaluation of
				// anything that appears after the field seperator.
				if (literalNode.NodeType == NodeType.FieldStart)
				{
					WordField wordField = this.FindByStartNode(literalNode);
					this.startDictionary.Remove(wordField.FieldStart);
					this.endDictionary.Remove(wordField.FieldEnd);
				}

				// This will remove all the nodes in the literal part of the field.
				Node nextNode = literalNode.NextSibling;
				literalNode.Remove();
				literalNode = nextNode;

			}

		}

		/// <summary>
		/// Returns a string that represents the WordField.
		/// </summary>
		/// <returns>A string that represents the WordField.</returns>
		public override String ToString()
		{

			// The field is decorated with the type name.
			return String.Format("Field {0}, Children {1}", Enum.GetName(typeof(FieldType), this.FieldStart.FieldType), this.endDictionary.Count);

		}

	}

}