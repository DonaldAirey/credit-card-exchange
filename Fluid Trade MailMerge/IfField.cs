namespace FluidTrade.Office
{

	using Aspose.Words;
	using Aspose.Words.Fields;
	using FluidTrade.ExpressionEvaluation;
	using System;
	using System.Collections.Generic;
	using System.Text.RegularExpressions;

	/// <summary>
	/// Defines a conditional field.
	/// </summary>
	/// <copyright>Copyright © 2009 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public class IfField : WordField
	{

		// Private Static Fields
		private static String ifText;

		// Private Instance Fields
		private List<Node> falseExpression;
		private List<Node> trueExpression;
		private String expression;

		// Private Enums
		private enum State { FieldResult, FalseEndQuote, FalseStatement, FalseStartQuote, TrueEndQuote, TrueStatement, TrueStartQuote, Operator, Expression, Final };

		/// <summary>
		/// Create the resources required for this type.
		/// </summary>
		static IfField()
		{

			// Initialize the Type
			IfField.ifText = "IF ";

		}

		/// <summary>
		/// Create the resources required for this instance.
		/// </summary>
		/// <param name="startNode"></param>
		public IfField(FieldStart startNode)
			: base(startNode)
		{

			// Initialize the object.
			this.falseExpression = new List<Node>();
			this.trueExpression = new List<Node>();
			this.expression = String.Empty;

			// The parser below will pull apart the various parts of the conditional statement.  The biggest issue is that the 'true' clause and the 'false'
			// clause contain a combination of Nodes which must be preserved exactly as they will become part of the document.  It is not enought to just extract
			// the text from these nodes, the fonts, paragraphs and styles and any other fields must be extracted as well.  Parsing nodes is difficult as the
			// various tokens can exist all in a single node, or be spread across several nodes.  These variables are used for parsing.  Note that parsinge starts
			// from the end of the statement and works towards the beginning.  This is because the 'true' and 'false' clauses are a fixed part of the syntax
			// and can be deterministically parsed using the quotes as tokens.  The rest of the statement -- the conditional expression -- is much more
			// difficult and requires a professional strength parser and analyzer.  The reason a RexEx function doesn't work here is because the values
			// extracted are in the form of CodeDOM Nodes and can't be tokenized.
			Int32 lastIndex = default(Int32);
			Node node = this.FieldEnd.PreviousSibling;
			Run run = default(Run);
			Run runClone = default(Run);
			Int32 startIndex = -1;
			State state = default(State);

			// The parsinge is constructed as a simple state machine.  There is no pushback or advanced expression evaluation performed in this section.  The
			// primary goal is to remove the nodes that will become the 'true' and 'false' expressions.  A full-blown expression evaluator will be called with
			// the part of the statement that remains after pulling out the clauses.
			while (state != State.Final)
			{

				switch (node.NodeType)
				{

				case NodeType.Run:

					// Each of the run nodes can have a quote character which delineates the 'true' and 'false' clauses.  The 'startIndex' acts as a kind of
					// cursor as the parser moves through each of the Run nodes.
					run = node as Run;
					if (startIndex == -1)
						startIndex = run.Text.Length - 1;

					// Once a Run node is recognized it is examined for tokens that delinate the clauses.  The state drives how each Run node is examined and
					// determines what is trying to be extracted from each node.
					switch (state)
					{

					case State.FieldResult:

						// Just eat any Run nodes in the Field Result section of the field.  This moves the parser past the garbage -- the last evaluated data 
						// for this field -- and into the useful part of the field.
						startIndex = -1;
						node = node.PreviousSibling;
						break;

					case State.FalseEndQuote:

						// Look for the quote that finishes the 'False' clause.
						lastIndex = run.Text.LastIndexOf('\"', startIndex);
						if (lastIndex == -1)
						{

							// If the delimiter character can't be found then try the previous node.
							startIndex = -1;

						}
						else
						{

							// If the delimiter is found then move the cursor into the body of the clause.
							state = State.FalseStatement;
							startIndex = lastIndex - 1;

						}

						// move to the previous node when there is nothing left to parse in the current node until the delimiter is found.
						if (startIndex == -1)
						{
							node = node.PreviousSibling;
						}

						break;

					case State.FalseStatement:

						// This will extract the body of the 'False' condition.  The next token that will cause a state change is the quote that opened this
						// condition (remember we are parsing backwards).
						lastIndex = run.Text.LastIndexOf('\"', startIndex);
						if (lastIndex == -1)
						{

							// If there are no delimiters in the current run then clone it and copy the text up to the end quote (or the entire Run node if the
							// end quote was in a previously parsed node).  There are times when the false clause is empty and an optimization removes any
							// empty Run nodes.
							String text = run.Text.Substring(0, startIndex + 1);
							if (text != String.Empty)
							{
								runClone = run.Clone(true) as Run;
								runClone.Text = text;
								this.falseExpression.Add(runClone);
							}

							// Since this node doesn't contain a delimiter, the state remains the same.  Continue parsing the next node for the start quote.
							node = node.PreviousSibling;
							startIndex = -1;

						}
						else
						{

							// When an open quote is found we've come to the start of the false statement.  Copy any remaining text in the current Run node 
							// into the resulting clause.  The quotes will be removed but the rest of the text will be a literal copy of the text found in the
							// document complete with all the formatting.
							String text = run.Text.Substring(lastIndex + 1, startIndex - lastIndex);
							if (runClone == null || runClone.Text != String.Empty)
							{
								runClone = run.Clone(true) as Run;
								runClone.Text = text;
								this.falseExpression.Add(runClone);
							}

							// After the opening quote is found the parser will move to a state where it looks for the opening quote.  While this may seem
							// unnecessary because we've already found the quote, it made the logic simpler than having to create a 'push back' concept for
							// the tokens.
							state = State.FalseStartQuote;

						}

						break;

					case State.FalseStartQuote:

						// This will search for the start quote of the 'False' clause.  Note that there is no check here for an empty Run node.  The only way 
						// for the parser to be in this state is if the previous state had found an open quote in the current Run node.
						lastIndex = run.Text.LastIndexOf('\"', startIndex);
						if (lastIndex != -1)
						{

							// If the start quote was the starting character of this Run node then the parsing will continue with the previous sibling when 
							// the next state is entered.  Otherwise, the cursor is moved in front of the opening quote as the parsing continues.
							if (lastIndex == 0)
							{
								node = node.PreviousSibling;
								startIndex = -1;
							}
							else
							{
								startIndex = lastIndex - 1;
							}

							// Transition to a state where the parser searches for the opening quote of the 'True' condition.
							state = State.TrueEndQuote;

						}

						break;

					case State.TrueEndQuote:

						// Look for the quote that finishes the 'True' clause.
						lastIndex = run.Text.LastIndexOf('\"', startIndex);
						if (lastIndex == -1)
						{

							// If the delimiter character can't be found then try the previous node.
							startIndex = -1;

						}
						else
						{

							// If the delimiter is found then move the cursor into the body of the clause.
							state = State.TrueStatement;
							startIndex = lastIndex - 1;

						}

						// move to the previous node when there is nothing left to parse in the current node until the delimiter is found.
						if (startIndex == -1)
						{
							node = node.PreviousSibling;
						}

						break;

					case State.TrueStatement:

						// This will extract the body of the 'True' condition.  The next token that will cause a state change is the quote that opened this
						// condition (remember we are parsing backwards).
						lastIndex = run.Text.LastIndexOf('\"', startIndex);
						if (lastIndex == -1)
						{

							// If there are no delimiters in the current run then clone it and copy the text up to the end quote (or the entire Run node if the
							// end quote was in a previously parsed node).  There are times when the true clause is empty and an optimization removes any
							// empty Run nodes.
							String text = run.Text.Substring(0, startIndex + 1);
							if (text != String.Empty)
							{
								runClone = run.Clone(true) as Run;
								runClone.Text = text;
								this.trueExpression.Add(runClone);
							}

							// Since this node doesn't contain a delimiter, the state remains the same.  Continue parsing the next node for the start quote.
							node = node.PreviousSibling;
							startIndex = -1;

						}
						else
						{

							// When an open quote is found we've come to the start of the true statement.  Copy any remaining text in the current Run node into
							// the resulting clause.  The quotes will be removed but the rest of the text will be a literal copy of the text found in the
							// document complete with all the formatting.
							String text = run.Text.Substring(lastIndex + 1, startIndex - lastIndex);
							if (runClone.Text != String.Empty)
							{
								runClone = run.Clone(true) as Run;
								runClone.Text = text;
								this.trueExpression.Add(runClone);
							}

							// After the opening quote is found the parser will move to a state where it looks for the opening quote.  While this may seem
							// unnecessary because we've already found the quote, it made the logic simpler than having to create a 'push back' concept for
							// the tokens.
							state = State.TrueStartQuote;

						}

						break;

					case State.TrueStartQuote:

						// This will search for the start quote of the 'True' clause.  Note that there is no check here for an empty Run node.  The only way 
						// for the parser to be in this state is if the previous state had found an open quote in the current Run node.
						lastIndex = run.Text.LastIndexOf('\"', startIndex);
						if (lastIndex != -1)
						{

							// If the start quote was the starting character of this Run node then the parsing will continue with the previous sibling when 
							// the next state is entered.  Otherwise, the cursor is moved in front of the opening quote as the parsing continues.
							if (lastIndex == 0)
							{
								node = node.PreviousSibling;
								startIndex = -1;
							}
							else
							{
								startIndex = lastIndex - 1;
							}

							// Transition to a state where the parser searches for condition expression.
							state = State.Expression;

						}

						break;

					case State.Expression:

						// Once the 'True' and 'False' clauses have been parsed, everthing between the start quote and the start of the field can be considered
						// part of the expression.  There is no formatting that is required to evaluate the expression so there is no need to pull apart the
						// Run nodes.
						this.expression = run.Text.Substring(0, startIndex + 1) + expression;

						// This moves the cursor up to the previous run node.  Remember the parsing runs backwards because it is more deterministic to pull out
						// the 'True' and 'False' clauses from the end than to parse forward to determine where the condition ended.
						startIndex = -1;
						node = node.PreviousSibling;
						break;

					}

					break;

				case NodeType.FieldSeparator:

					// The field seperator divides the field into the part you see in the document when working with fields, and the actual text that is 
					// displayed when you are not working with fields.  Anything after the field seperator is previously evaluated data which is of no interest
					// to this parser.
					node = node.PreviousSibling;
					state = State.FalseEndQuote;
					break;

				case NodeType.FieldEnd:

					// Remember that the parser works from the end of the field to the start.  Therefore, the end is just the beginning (of a field).
					WordField wordField = this.FindByEndNode(node);

					// Each field is handled according to the state of the parser.
					switch (state)
					{

					case State.FalseStatement:

						// A field found during the parsing of a false statement is added in its entirety to the literal collection.
						while (node != null && node.NextSibling != wordField.FieldStart)
						{
							this.falseExpression.Insert(0, node.Clone(true));
							node = node.PreviousSibling;
						}

						// Evaluate the previous node in the DOM.
						node = wordField.FieldStart.PreviousSibling;

						break;

					case State.TrueStatement:

						// A field found during the parsing of a true statement is added in its entirety to the literal collection.
						while (node != null && node.NextSibling != wordField.FieldStart)
						{
							this.trueExpression.Insert(0, node.Clone(true));
							node = node.PreviousSibling;
						}

						// Evaluate the previous node in the DOM.
						node = wordField.FieldStart.PreviousSibling;

						break;

					case State.Expression:

						// When a field is found during the evaluation of an expression, the name of the merge field is used as a general purpose parameter to
						// the expression evaluator.
						if (wordField is MergeField)
						{
							MergeField mergeField = wordField as MergeField;
							this.expression = mergeField.Reference + expression;
						}

						// Evaluate the previous node in the DOM.
						node = wordField.FieldStart.PreviousSibling;

						break;

					default:

						// Ignore anything whiel processing the field results.
						node = wordField.FieldStart.PreviousSibling;
						break;

					}

					break;

				case NodeType.FieldStart:

					// The parser works from the end of the field to the front because the expression can have many items in it but the 'True' and 'False' 
					// clauses are relatively easy to delinate.  When the cursor has come to the start of the field, then the parsing of this 'IF' statement is
					// complete.  The last task is to remove the 'IF' keyword from the expression.
					this.expression = expression.Substring(expression.IndexOf(IfField.ifText) + IfField.ifText.Length);
					state = State.Final;
					break;

				}

			}

		}

		/// <summary>
		/// Evaluate the IF field.
		/// </summary>
		public override void Evaluate()
		{

			// This will clear out the literal area of the field.  The main idea of evaluation is to replace the contents of this part of the field with the
			// evaluated data.  The original field -- the stuff before the field seperator -- remains pretty much in tact.
			this.ClearLiteral();

			// Recursively evaluate each of the child fields before the parent is evaluated.
			foreach (WordField wordField in this)
			{
				wordField.Evaluate();
			}

			// This is a general purpose expression evaluator.  It is used to determine the veracity of the expression in the 'IF' statement and depending on
			// the resulting value, will either add the 'True' clause or the 'False' clause to the document.
			ExpressionContext context = new ExpressionContext();

			// The first step to evaluating a conditional statement to provide distinct variables for all the merge fields.
			foreach (WordField wordField in this)
			{
				if (wordField is MergeField)
				{
					MergeField mergeField = wordField as MergeField;
					if (!context.Variables.ContainsKey(mergeField.Reference))
					{
						context.Variables.Add(mergeField.Reference, mergeField.Value);
					}
				}
			}

			try
			{

				// This is the part where the expression that was parsed out of the field is evaluated.  At this point all the variables having been set to
				// values from the merged database and the expression can be compiled and evaluated.  Also note that this will catch any compile time errors
				// and produce a field in the output document that contains the error.
				IDynamicExpression iDynamicExpression = context.CompileDynamic(this.expression);
				List<Node> statement = Convert.ToBoolean(iDynamicExpression.Evaluate()) ? this.trueExpression : this.falseExpression;

				// The literal part of the field has been cleared out and now that the expression has been evaluated it can be filled in with either the data
				// collected from the 'True' or' 'False' statements (depending on the outcome of the evaluation done above).
				foreach (Node replacementNode in statement)
				{
					this.FieldStart.ParentNode.InsertBefore(replacementNode, this.FieldEnd);
				}

				// Adding elements from the 'True' or 'False' statements has the potential to add child fields to this field.  This pass will look generate 
				// any new fields and immediately evaluate them.  Like a real programming language, conditional clauses are not evaluated until the condition 
				// has been resolved.
				foreach (Node replacementNode in statement)
				{
					if (replacementNode.NodeType == NodeType.FieldStart)
					{
						WordField wordField = WordField.CreateField(replacementNode as FieldStart);
						wordField.Evaluate();
					}
				}

			}
			catch (Exception exception)
			{

				// This inserts the error message into the document.
				Run run = new Run(this.FieldStart.Document, String.Format("Error! {0}", exception.Message));
				this.FieldStart.ParentNode.InsertAfter(run, this.FieldEnd);

			}

		}

		/// <summary>
		/// Creates a string representation of the object.
		/// </summary>
		/// <returns>A string representing ths object.</returns>
		public override String ToString()
		{
			return String.Format("IfField {0}", this.expression);
		}

	}

}