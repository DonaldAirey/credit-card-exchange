namespace FluidTrade.MiddleTierGenerator.TransactionLogItemClass
{

    using System;
    using System.CodeDom;
    using FluidTrade.Core;

	/// <summary>
	/// Creates a field that holds the data in a transaction log item.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	class DataField : CodeMemberField
	{

		/// <summary>
		/// Creates a field that holds the data in a transaction log item.
		/// </summary>
		public DataField()
		{

			//		internal object[] data;
			this.Attributes = MemberAttributes.Assembly;
			this.Type = new CodeGlobalTypeReference(typeof(Object[]));
			this.Name = "data";

		}

	}

}
