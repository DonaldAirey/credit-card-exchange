namespace FluidTrade.MiddleTierGenerator.DataSetClass
{

    using System;
    using System.CodeDom;
    using FluidTrade.Core;

    /// <summary>
	/// Creates a field that holds a delegate to a method that filters rows from the client data model.
	/// </summary>
	/// <copyright>Copyright � 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	class ReadBufferSizeField : CodeMemberField
	{

		/// <summary>
		/// Creates a field that holds the reader/writer lock for the current data model.
		/// </summary>
		public ReadBufferSizeField()
		{

			//		private long readBufferSize;
			this.Attributes = MemberAttributes.Private;
			this.Type = new CodeGlobalTypeReference(typeof(Int64));
			this.Name = "readBufferSize";

		}

	}

}
