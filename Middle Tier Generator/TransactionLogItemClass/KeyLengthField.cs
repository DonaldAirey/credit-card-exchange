namespace FluidTrade.MiddleTierGenerator.TransactionLogItemClass
{

    using System;
    using System.CodeDom;
    using FluidTrade.Core;

	/// <summary>
	/// Creates a field that holds the length of the key found in the data.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	class KeyLengthField : CodeMemberField
	{

		/// <summary>
		/// Creates a field that holds the length of the key found in the data.
		/// </summary>
		public KeyLengthField()
		{

			//		internal int keyLength;
			this.Attributes = MemberAttributes.Assembly;
			this.Type = new CodeGlobalTypeReference(typeof(Int32));
			this.Name = "keyLength";

		}

	}

}
