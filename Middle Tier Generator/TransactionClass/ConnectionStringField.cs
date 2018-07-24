namespace FluidTrade.MiddleTierGenerator.TransactionClass
{

    using System;
    using System.CodeDom;
    using FluidTrade.Core;

	/// <summary>
	/// Creates a field that holds a delegate to a method that filters rows from the client data model.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	class ConnectionStringField : CodeMemberField
	{

		/// <summary>
		/// Creates a field that holds the reader/writer lock for the current data model.
		/// </summary>
		public ConnectionStringField()
		{

			//        private static string connectionString;
			this.Attributes = MemberAttributes.Private | MemberAttributes.Static;
			this.Type = new CodeGlobalTypeReference(typeof(String));
			this.Name = "connectionString";

		}

	}

}
