namespace FluidTrade.Office
{

	using Aspose.Words;
	using Aspose.Words.Fields;
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Globalization;
	using System.Text.RegularExpressions;

	/// <summary>
	/// A field containing information extracted from a data source.
	/// </summary>
	public class MergeField : WordField
	{

		// Private Static Fields
		private static Regex mergeExpression;

		// Private Instance Fields
		private String format;
		private String reference;
		private Object value;

		/// <summary>
		/// Create the static resources required by this class.
		/// </summary>
		static MergeField()
		{

			// This regular expression is used to extract the name of the data element that is referenced by this MergeField.
			MergeField.mergeExpression = new Regex(
				"\\s*MERGEFIELD\\s+(?<reference>\\w+)" +
				"(\\s+\\\\#\\s+(?<numberFormat>\"[^\"]+\"))?" +
				"(\\s+\\\\#\\s+(?<numberFormat>\\S+))?" +
				"(\\s+\\\\@\\s+\"(?<dateFormat>[^\"]+)\")?" +
				"(\\s+\\\\@\\s+(?<dateFormat>\\S+))?" +
				"\\s*");

		}

		/// <summary>
		/// Create a MergeField from the Document Object Model.
		/// </summary>
		/// <param name="fieldStart">The starting node in the DOM from which the MergeField is constructed.</param>
		public MergeField(FieldStart fieldStart)
			: base(fieldStart)
		{

			// Extract from the entire text of the field from the 'Run' nodes in the DOM.  The field seperator divides the text of the merge field from the
			// current contents of that merged field, otherwise the end of the field is used as the delimiter.
			String fieldText = String.Empty;
			for (Node node = this.FieldStart; node.NodeType != NodeType.FieldSeparator && node.NodeType != NodeType.FieldEnd; node = node.NextSibling)
			{
				if (node.NodeType == NodeType.Run)
				{
					fieldText += ((Run)node).Text;
				}
			}

			// A regular expression is used to extract the name of the data element referenced by this MergeField.
			Match match = MergeField.mergeExpression.Match(fieldText);

			// The reference is the name of the field.  This name is cross referenced to the data dictionary when looking up values during a mail merge.
			Group referenceGroup = match.Groups["reference"];
			this.reference = referenceGroup.Success ? referenceGroup.Value : String.Empty;

			// The default formatting is to use the native format for the data type.  This can be overridden with additional codes in the merge field.
			this.format = "{0}";

			// The default formatting for numbers can be overridden with the number format syntax.
			Group numberFormatGroup = match.Groups["numberFormat"];
			if (numberFormatGroup.Success)
			{
				this.format = String.Format("{{0:{0}}}", numberFormatGroup.Value);
			}

			// The default formatting for dates and times can be overridden with the date format syntax.
			Group dateFormatGroup = match.Groups["dateFormat"];
			if (dateFormatGroup.Success)
			{
				this.format = String.Format("{{0:{0}}}", dateFormatGroup.Value);
			}

		}

		/// <summary>
		/// Gets the data item referenced by this MergeField.
		/// </summary>
		public String Reference
		{
			get
			{
				return this.reference;
			}
		}

		/// <summary>
		/// Gets of sets the value of the MergeField.
		/// </summary>
		public Object Value
		{
			get
			{
				return this.value;
			}
			set
			{
				this.value = value;
			}
		}

		/// <summary>
		/// Replaces the merge field with the evaluated text.
		/// </summary>
		public override void Evaluate()
		{

			// This will clear out the literal area of the field.
			this.ClearLiteral();
	
			// Recursively evaluate each of the child fields before the parent is evaluated.
			foreach (WordField wordField in this)
			{
				wordField.Evaluate();
			}

			// Evaluate the MERGEFIELD field using the document's dictionary.
			CompositeNode compositeNode = this.FieldStart.ParentNode as CompositeNode;
			if (compositeNode != null)
			{

				// The documents dictionary is used to provide values for the fields.
				if (this.MergeDocument.Dictionary.TryGetValue(this.Reference, out this.value))
				{

					// Null values are automatically interpreted as empty strings.  The literal area was cleared out at the start of this evaluation.  It is
					// now filled in with the data that matches the fields' tag.
					this.value = value == null ? String.Empty : value;
					Run run = new Run(this.FieldStart.Document, String.Format(this.format, this.value));
					compositeNode.InsertAfter(run, this.FieldSeperator);

				}
				else
				{

					// When a field can't be found, this error text is placed in the document.
					this.value = String.Format("Error! MergeField was not found in header record of data source.");
					Run run = new Run(this.FieldStart.Document, this.value.ToString());
					compositeNode.InsertAfter(run, this.FieldSeperator);

				}

			}

		}

		/// <summary>
		/// Creates a string representation of the object.
		/// </summary>
		/// <returns>A string representing ths object.</returns>
		public override String ToString()
		{
			return String.Format(String.Format("MergeField {0}", this.format), this.reference);
		}

	}

}