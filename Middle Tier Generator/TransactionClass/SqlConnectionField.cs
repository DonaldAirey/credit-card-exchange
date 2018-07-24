namespace FluidTrade.MiddleTierGenerator.TransactionClass
{

    using System.CodeDom;
    using System.Data.SqlClient;
    using FluidTrade.Core;

	/// <summary>
	/// Creates a field that holds the connection to the SQL database.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	class SqlConnectionField : CodeMemberField
	{

		/// <summary>
		/// Creates a field that holds the connection to the SQL database.
		/// </summary>
		public SqlConnectionField()
		{

			//		private global::System.Data.SqlClient.SqlConnection sqlConnection;
			this.Attributes = MemberAttributes.Private;
			this.Type = new CodeGlobalTypeReference(typeof(SqlConnection));
			this.Name = "sqlConnection";

		}

	}

}
