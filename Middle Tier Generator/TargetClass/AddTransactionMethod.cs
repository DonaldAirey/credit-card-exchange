namespace FluidTrade.MiddleTierGenerator.TargetClass
{

    using System;
    using System.CodeDom;
    using FluidTrade.Core;

    /// <summary>
	/// Creates a method to handle moving the deleted records from the active data model to the deleted data model.
	/// </summary>
	class AddTransactionMethod : CodeMemberMethod
	{

		/// <summary>
		/// Creates a method to handle moving the deleted records from the active data model to the deleted data model.
		/// </summary>
		/// <param name="schema">The data model schema.</param>
		public AddTransactionMethod(DataModelSchema dataModelSchema)
		{

			//		/// <summary>
			//		/// Adds a transaction item to the log.
			//		/// </summary>
			//		/// <param name="iRow">The record to be added to the transaction log.</param>
			//		/// <param name="data">An array of updated fields.</param>
			//		[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
			//		[global::System.ComponentModel.BrowsableAttribute(false)]
			//		internal static void AddTransaction(IRow iRow, object[] data)
			//		{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("Adds a transaction item to the log.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Comments.Add(new CodeCommentStatement("<param name=\"iRow\">The record to be added to the transaction log.</param>", true));
			this.Comments.Add(new CodeCommentStatement("<param name=\"data\">An array of updated fields.</param>", true));
			this.CustomAttributes.AddRange(new CodeCustomAttributesForProperties());
			this.Name = "AddTransaction";
			this.Attributes = MemberAttributes.Assembly | MemberAttributes.Static;
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(FluidTrade.Core.IRow)), "iRow"));
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(Object[])), "data"));

			//			DataModel.dataModelDataSet.AddTransaction(iRow, data);
			this.Statements.Add(
				new CodeMethodInvokeExpression(
					new CodeFieldReferenceExpression(
						new CodeTypeReferenceExpression(dataModelSchema.Name),
						String.Format("{0}DataSet", CommonConversion.ToCamelCase(dataModelSchema.Name))),
					"AddTransaction",
					new CodeArgumentReferenceExpression("iRow"),
					new CodeArgumentReferenceExpression("data")));

			//		}

		}

	}

}
