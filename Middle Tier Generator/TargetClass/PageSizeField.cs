namespace FluidTrade.MiddleTierGenerator.TargetClass
{

    using System;
    using System.CodeDom;
    using FluidTrade.Core;

    /// <summary>
	/// Creates a field that holds a delegate to a method that filters rows from the client data model.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	class PageSizeField : CodeMemberField
	{

		/// <summary>
		/// Creates a field that holds the reader/writer lock for the current data model.
		/// </summary>
		public PageSizeField()
		{

			//		internal static global::System.Threading.ReaderWriterLockSlim dataLock;
			this.Attributes = MemberAttributes.Assembly | MemberAttributes.Static;
			this.Type = new CodeGlobalTypeReference(typeof(Int64));
			this.Name = "pageSize";
			this.InitExpression = new CodePrimitiveExpression(1000);

		}

	}

}
