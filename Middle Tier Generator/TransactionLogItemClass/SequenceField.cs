namespace FluidTrade.MiddleTierGenerator.TransactionLogItemClass
{

    using System;
    using System.CodeDom;
    using FluidTrade.Core;

	/// <summary>
	/// Creates a field that holds sequence of the item in the transaction log.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	class SequenceField : CodeMemberField
	{

		/// <summary>
		/// Creates a field that holds sequence of the item in the transaction log.
		/// </summary>
		public SequenceField()
		{

			//		internal long sequence;
			this.Attributes = MemberAttributes.Assembly;
			this.Type = new CodeGlobalTypeReference(typeof(Int64));
			this.Name = "sequence";

		}

	}

}
