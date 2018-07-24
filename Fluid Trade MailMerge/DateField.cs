namespace FluidTrade.Office
{

	using Aspose.Words;
	using Aspose.Words.Fields;
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Text.RegularExpressions;

	/// <summary>
	/// A field containing information extracted from a data source.
	/// </summary>
	public class DateField : WordField
	{

		// Private Static Fields
		private static Regex dateExpression;

		// Private Instance Fields
		private String format;
		private DateTime value;

		/// <summary>
		/// Create the static resources required by this class.
		/// </summary>
		static DateField()
		{

			// This regular expression is used to extract the name of the data element that is referenced by this DateField.
			DateField.dateExpression = new Regex(
				"\\s*DATE" +
				"(\\s+\\\\@\\s+\"(?<dateFormat>[^\"]+)\")?" +
				"(\\s+\\\\@\\s+(?<dateFormat>\\S+))?" +
				"\\s*");

		}

		/// <summary>
		/// Create a DateField from the Document Object Model.
		/// </summary>
		/// <param name="fieldStart">The starting node in the DOM from which the DateField is constructed.</param>
		public DateField(FieldStart fieldStart)
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

			// A regular expression is used to extract the name of the data element referenced by this DateField.
			Match match = DateField.dateExpression.Match(fieldText);

			// The default formatting is to use the native format for the data type.  This can be overridden with additional codes in the merge field.
			this.format = "{0}";

			// The field can have explicit formatting.  This will pull the format out of the DATE field.
			Group dateFormat = match.Groups["dateFormat"];
			if (dateFormat.Success)
			{
				this.format = String.Format("{{0:{0}}}", dateFormat.Value);
			}

		}

		/// <summary>
		/// Gets of sets the value of the DATE field.
		/// </summary>
		public DateTime Value
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

				// Null values are automatically interpreted as empty strings.  The literal area was cleared out at the start of this evaluation.  It is
				// now filled in with the data that matches the fields' tag.
				this.value = DateTime.Now;
				Run run = new Run(this.FieldStart.Document, String.Format(this.format, this.value));
				compositeNode.InsertAfter(run, this.FieldSeperator);

			}

		}

		/// <summary>
		/// Creates a string representation of the object.
		/// </summary>
		/// <returns>A string representing ths object.</returns>
		public override String ToString()
		{
			return String.Format(String.Format("DateField {0}", this.format), this.value);
		}

	}

}