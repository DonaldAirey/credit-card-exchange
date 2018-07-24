namespace FluidTrade.PresentationGenerator.PresentationClass
{

    using System;
    using System.CodeDom;
    using FluidTrade.Core;

    /// <summary>
	/// Generates the constructor for the generated class.
	/// </summary>
	public class SchemaSelect : System.CodeDom.CodeMemberMethod
	{

		/// <summary>
		/// Generates the constructor for the generated class.
		/// </summary>
		/// <param name="classSchema">A description of a Type.</param>
		public SchemaSelect(ClassSchema classSchema)
		{

			//		/// <summary>
			//		/// Creates a FluidTrade.Sandbox.WorkingOrder.WorkingOrder.
			//		/// </summary>
			//		public Sandbox.WorkingOrderRow Select(FluidTrade.Sandbox.WorkingOrderRow workingOrderRow)
			//		{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Creates a {0}.", classSchema.Type), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			this.ReturnType = new CodeTypeReference(classSchema.Type);
			this.Name = "Select";
			foreach (ArgumentSchema argumentSchema in classSchema.Constructor.Arguments)
				this.Parameters.Add(new CodeParameterDeclarationExpression(argumentSchema.Type, argumentSchema.Name));

			//			this.key = workingOrderRow;
			foreach (SnippetSchema snippetSchema in classSchema.Constructor.Snippets)
			{
				if (snippetSchema is SetterSchema)
				{
					SetterSchema setterSchema = snippetSchema as SetterSchema;
					string[] statementList = setterSchema.Value.Split(';');
					for (int index = 0; index < statementList.Length - 1; index++)
						this.Statements.Add(new CodeSnippetStatement(statementList[index] + ';'));
					this.Statements.Add(new CodeAssignStatement(
						new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), setterSchema.Property),
						new CodeSnippetExpression(statementList[statementList.Length - 1])));
				}
				else
				{
					string[] statementList = snippetSchema.Value.Split(';');
					for (int index = 0; index < statementList.Length; index++)
						this.Statements.Add(new CodeSnippetStatement(statementList[index] + ';'));
				}

			}

			// This will run through each of the members of the class an initialize the complex data types.  If the element allows 
			// only one occurance of an element, then a single object is created.  If the element allows many, then a list is
			// created.  Basic data types are not constructred here.
			foreach (PropertySchema propertySchema in classSchema.Properties)
			{

				// For scalar properties, the property is created and then populated using the setters.
				if (propertySchema.MaxOccurs == 1)
				{

					//			this.askPrice = new FluidTrade.Sandbox.WorkingOrder.AskPrice();
					CodeFieldReferenceExpression fieldExpression = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(),
						CommonConversion.ToCamelCase(propertySchema.Name));

					//			this.askPrice.isEnabled = workingOrderRow.StatusCode != FluidTrade.Sandbox.Status.Filled;
					//			this.askPrice.price = workingOrderRow.PriceRow.AskPrice;
					foreach (SnippetSchema snippetSchema in propertySchema.Snippets)
					{
						if (snippetSchema is SetterSchema)
						{
							SetterSchema setterSchema = snippetSchema as SetterSchema;
							string[] statementList = setterSchema.Value.Split(';');
							for (int index = 0; index < statementList.Length - 1; index++)
								this.Statements.Add(new CodeSnippetStatement(statementList[index] + ';'));
							this.Statements.Add(new CodeAssignStatement(
								new CodeFieldReferenceExpression(fieldExpression, CommonConversion.ToCamelCase(setterSchema.Property)),
								new CodeSnippetExpression(statementList[statementList.Length - 1])));
						}
						else
						{
							string[] statementList = snippetSchema.Value.Split(';');
							for (int index = 0; index < statementList.Length; index++)
								this.Statements.Add(new CodeSnippetStatement(statementList[index] + ';'));
						}

					}

				}

				// Vectors are populated using the LINQ libraries to select, filter, sort and transform the source data.
				if (propertySchema.MaxOccurs == Decimal.MaxValue)
				{

					// The index is used as an indicator to set the destination property instead of creating an intermediate 
					// step.  The general idea is to set up a series of queries that will select, sort, filter and otherwise
					// transform the source data into a format that relects the users' preferences.
					for (int queryIndex = 0; queryIndex < propertySchema.Queries.Count; queryIndex++)
					{

						// This schema will generate a step that transforms the source data.
						QuerySchema querySchema = propertySchema.Queries[queryIndex];

						// The results will either be stored in an intermediate variable or the property that the ultimate
						// destination for this data.  The source of the data is computed independently of the destination and
						// stored here until a decision about where the results are held is made.
						CodeExpression initializationExpression = null;

						// Transform the 'Where' particle into a CodeDOM expression.
						if (querySchema is WhereSchema)
						{
							//			System.Linq.Enumerable.Where<Sandbox.WorkingOrderRow>(DataModel.WorkingOrder, FluidTrade.Sandbox.WorkingOrderHeader.WorkingOrderHeader.prefilter.Filter);
							WhereSchema whereSchema = querySchema as WhereSchema;
							initializationExpression = new CodeMethodInvokeExpression(
								new CodeTypeReferenceExpression("System.Linq.Enumerable"),
								string.Format("Where<{0}>", whereSchema.SourceType),
								new CodeVariableReferenceExpression(whereSchema.Source),
								new CodePropertyReferenceExpression(
									new CodeFieldReferenceExpression(
										new CodeThisReferenceExpression(),
										CommonConversion.ToCamelCase(whereSchema.Predicate)),
									"Filter"));
						}

						// Transform the 'Select' particle into a CodeDOM expression.
						if (querySchema is SelectSchema)
						{
							//			System.Linq.Enumerable.Select<Sandbox.WorkingOrderRow, FluidTrade.Sandbox.WorkingOrder.WorkingOrder>(a811c4058ea3a4d79af050b1d46fcda0d, FluidTrade.Sandbox.WorkingOrder.WorkingOrder.Select);
							SelectSchema selectSchema = querySchema as SelectSchema;
							initializationExpression = new CodeMethodInvokeExpression(
								new CodeTypeReferenceExpression("System.Linq.Enumerable"),
								string.Format("Select<{0}, {1}>", selectSchema.SourceType, selectSchema.ResultType),
								new CodeVariableReferenceExpression(selectSchema.Source),
								new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), CommonConversion.ToCamelCase(selectSchema.Selector)));
						}

						// Transform the 'OrderBy' particle into a CodeDOM expression.
						if (querySchema is OrderBySchema)
						{
							//			System.Linq.Enumerable.OrderBy<Sandbox.WorkingOrder.WorkingOrder, FluidTrade.Sandbox.WorkingOrder.WorkingOrder>(n65cf68c24b3e423da4e8bb4ade1c20cc, FluidTrade.Sandbox.WorkingOrder.WorkingOrder.SelectSelf, FluidTrade.Sandbox.WorkingOrderHeader.WorkingOrderHeader.comparer);
							OrderBySchema orderBySchema = querySchema as OrderBySchema;
							initializationExpression = new CodeMethodInvokeExpression(
								new CodeTypeReferenceExpression("System.Linq.Enumerable"),
								string.Format("OrderBy<{0}, {1}>", orderBySchema.SourceType, orderBySchema.KeyType),
								new CodeVariableReferenceExpression(orderBySchema.Source),
								new CodeVariableReferenceExpression(orderBySchema.KeySelector),
								new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), CommonConversion.ToCamelCase(orderBySchema.Comparer)));
						}


						//			System.Collections.Generic.IEnumerable<Sandbox.WorkingOrder.WorkingOrder> list0 = System.Linq.Enumerable.Where<Sandbox.WorkingOrder.WorkingOrder>(y22c1b202d41f47a19511dce0cbc80e65, FluidTrade.Sandbox.WorkingOrderHeader.WorkingOrderHeader.filter.Filter);
						this.Statements.Add(new CodeVariableDeclarationStatement(
							new CodeTypeReference(string.Format("System.Collections.Generic.IEnumerable<{0}>", querySchema.ResultType)),
							querySchema.Name,
							initializationExpression));
		
						// The last query particle will populate the property.  All the others are stored in intermediate variables.
						if (queryIndex == propertySchema.Queries.Count - 1)
						{
							//						this.workingOrderList = System.Linq.Enumerable.ToArray<Sandbox.WorkingOrder.WorkingOrder>(foobar);
							this.Statements.Add(
								new CodeAssignStatement(
									new CodeFieldReferenceExpression(new CodeThisReferenceExpression(),
										CommonConversion.ToCamelCase(propertySchema.Name)),
										new CodeMethodInvokeExpression(
											new CodeTypeReferenceExpression(typeof(System.Linq.Enumerable)),
											"ToArray",
											new CodeVariableReferenceExpression(querySchema.Name))));
						}

					}

				}

			}

			//      return this;
			this.Statements.Add(new CodeMethodReturnStatement(new CodeThisReferenceExpression()));

		}

	}

}
