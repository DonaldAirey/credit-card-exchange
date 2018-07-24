namespace FluidTrade.MiddleTierGenerator.TransactionLogItemClass
{

    using System;
    using System.CodeDom;
    using FluidTrade.Core;

	/// <summary>
	/// Creates a field that holds the time the log item was created.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	class TimeStampField : CodeMemberField
	{

		/// <summary>
		/// Creates a field that holds the time the log item was created.
		/// </summary>
		public TimeStampField()
		{

			//		internal int keyLength;
			this.Attributes = MemberAttributes.Assembly;
			this.Type = new CodeGlobalTypeReference(typeof(DateTime));
			this.Name = "timeStamp";

		}

	}

}
