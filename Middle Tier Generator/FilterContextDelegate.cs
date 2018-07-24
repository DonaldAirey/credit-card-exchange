namespace FluidTrade.Core
{

	using System;
    using System.CodeDom;

	/// <summary>
	/// Generates a delegate for filtering records from the stream of data sent to the client.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public class FilterContextDelegate : CodeTypeDelegate
	{

		/// <summary>
		/// Generates a delegate for filtering records from the stream of data sent to the client.
		/// </summary>
		/// <param name="dataSetNamespace">The CodeDOM namespace FluidTrade.Core.FilterContextDelegate contains this strongly typed DataSet.</param>
		public FilterContextDelegate(DataModelSchema dataModelSchema)
		{

			//	/// <summary>
			//	/// A general purpose delegate to find a context for filtering results that are returned to the client.
			//	/// </summary>
			//	/// <param name="dataModelTransaction"></param>
			//	/// <returns>An object that can be used, often a user identifier, to determine whether a given row can be returned to the client.</returns>
			//	public delegate Object FilterContextDelegate(DataModelTransaction dataModelTransaction);
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("Handles filters that remove data from the stream returned to the client.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Comments.Add(new CodeCommentStatement("<param name=\"dataModelTransaction\">The current transaction.</param>", true));
			this.Comments.Add(new CodeCommentStatement("<returns>An object that can be used, often a user identifier, to determine whether a given row can be returned to the client.</returns>", true));
			this.ReturnType = new CodeGlobalTypeReference(typeof(Object));
			this.Name = "FilterContextDelegate";
			this.Parameters.Add(
				new CodeParameterDeclarationExpression(
					new CodeTypeReference(String.Format("{0}Transaction", dataModelSchema.Name)),
					String.Format("{0}Transaction", CommonConversion.ToCamelCase(dataModelSchema.Name))));

		}

	}

}
