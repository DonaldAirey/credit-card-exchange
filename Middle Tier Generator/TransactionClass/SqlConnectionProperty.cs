namespace FluidTrade.MiddleTierGenerator.TransactionClass
{

    using System.CodeDom;
    using System.Data.SqlClient;
    using FluidTrade.Core;

	/// <summary>
	/// Generates a property that gets the connection to the SQL database.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	class SqlConnectionProperty : CodeMemberProperty
	{

		/// <summary>
		/// Generates a property that gets the lock for the data model.
		/// </summary>
		public SqlConnectionProperty()
		{

			//		/// <summary>
			//		/// Generates a property that gets the connection to the SQL database.
			//		/// </summary>
			//		public global::System.Data.SqlClient.SqlConnection SqlConnection
			//		{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("Gets the SQL Connection used to access the persistent data store.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.CustomAttributes.AddRange(new CodeCustomAttributesForProperties());
			this.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			this.Type = new CodeGlobalTypeReference(typeof(SqlConnection));
			this.Name = "SqlConnection";

			//			get { return this.sqlConnection; }
			this.GetStatements.Add(
				new CodeMethodReturnStatement(
					new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "sqlConnection")));

			//		}

		}

	}

}
