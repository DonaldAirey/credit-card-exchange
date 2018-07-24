namespace FluidTrade.MiddleTierGenerator.TransactionClass
{

    using System;
    using System.CodeDom;
    using FluidTrade.Core;

	/// <summary>
	/// Creates a field that holds the records used in a transaction.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	class SyncRootField : CodeMemberField
	{

		/// <summary>
		/// Creates a field that holds the records used in a transaction.
		/// </summary>
		public SyncRootField()
		{

			//		private static object syncRoot;
			this.Attributes = MemberAttributes.Private | MemberAttributes.Static;
			this.Type = new CodeGlobalTypeReference(typeof(Object));
			this.Name = "syncRoot";

		}

	}

}
