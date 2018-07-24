namespace FluidTrade.ClientGenerator.ClusteredIndexClass
{

    using System;
    using System.CodeDom;
    using FluidTrade.Core;

    /// <summary>
	/// Creates a CodeDOM description of a strongly typed index.
	/// </summary>
	public class Constructor : CodeConstructor
	{

		/// <summary>
		/// Creates a CodeDOM description of a strongly typed index.
		/// </summary>
		/// <param name="constraintSchema">The description of a unique constraint.</param>
		public Constructor(UniqueConstraintSchema uniqueConstraintSchema)
		{

			//		/// <summary>
			//		/// Create a primary, unique index on the AccessControl table.
			//		/// </summary>
			//		/// <param name="indexName">The name of the index.</param>
			//		/// <param name="columns">The columns that describe a unique key.</param>
			//      public GenderKeyIndex(string indexName, System.Data.DataColumn[] columns) : 
			//                base(indexName, columns) {
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Create a primary, unique index on the {0} table.", uniqueConstraintSchema.Table.Name), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Comments.Add(new CodeCommentStatement("<param name=\"indexName\">The name of the index.</param>", true));
			this.Comments.Add(new CodeCommentStatement("<param name=\"columns\">The columns that describe a unique key.</param>", true));
			this.Attributes = MemberAttributes.Public;
			this.Name = string.Format("{0}Index", uniqueConstraintSchema.Name);
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(String)), "indexName"));
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(System.Data.DataColumn[])), "columns"));
			this.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression("indexName"));
			this.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression("columns"));

			//        }

		}

	}

}
