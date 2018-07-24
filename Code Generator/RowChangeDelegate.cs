namespace FluidTrade.Core
{

	using System;
    using System.CodeDom;
	using System.Reflection;

	/// <summary>
	/// Creates a delegate for notification of a row changing in a strongly typed table.
	/// </summary>
	public class RowChangeDelegate : CodeTypeDelegate
	{

		/// <summary>
		/// Creates a delegate for notification of a row changing in a strongly typed table.
		/// </summary>
		/// <param name="tableSchema">The description of a table.</param>
		public RowChangeDelegate(TableSchema tableSchema)
		{

			//    /// <summary>
			//    /// Delegate for handling changes to the Configuration table.
			//    /// </summary>
			//    /// <param name="sender">The object that originated the event.</param>
			//    /// <param name="e">The event arguments.</param>
			//    public delegate void ConfigurationRowChangeEventHandler(object sender, ConfigurationRowChangeEventArgs e);
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Delegate for handling changes to the {0} table.", tableSchema.Name), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Comments.Add(new CodeCommentStatement("<param name=\"sender\">The object that originated the event.</param>", true));
			this.Comments.Add(new CodeCommentStatement("<param name=\"e\">The event arguments.</param>", true));
			this.Name = String.Format("{0}RowChangeEventHandler", tableSchema.Name);
			this.ReturnType = new CodeGlobalTypeReference(typeof(void));
			this.TypeAttributes = TypeAttributes.Public;
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(Object)), "sender"));
			this.Parameters.Add(new CodeParameterDeclarationExpression(String.Format("{0}RowChangeEventArgs", tableSchema.Name), "e"));

		}

	}

}
