namespace FluidTrade.MiddleTierGenerator.TransactionLogItemClass
{

    using System;
    using System.CodeDom;
    using FluidTrade.Core;

	/// <summary>
	/// Creates a field that holds a object that represents the container for another object in the data model.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	class ContainerContextField : CodeMemberField
	{

		/// <summary>
		/// Creates a field that holds a object that represents the container for another object in the data model.
		/// </summary>
		public ContainerContextField()
		{

			//		internal object containerContext;
			this.Attributes = MemberAttributes.Assembly;
			this.Type = new CodeGlobalTypeReference(typeof(Object));
			this.Name = "containerContext";

		}

	}

}
