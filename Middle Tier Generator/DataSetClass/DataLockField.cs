namespace FluidTrade.MiddleTierGenerator.DataSetClass
{

    using System.CodeDom;
    using System.Threading;
    using FluidTrade.Core;

	/// <summary>
	/// Creates a field that holds a delegate to a method that filters rows from the client data model.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	class DataLockField : CodeMemberField
	{

		/// <summary>
		/// Creates a field that holds the reader/writer lock for the current data model.
		/// </summary>
		public DataLockField()
		{

			//		internal static global::System.Threading.ReaderWriterLockSlim dataLock;
			this.Attributes = MemberAttributes.Private;
			this.Type = new CodeGlobalTypeReference(typeof(ReaderWriterLockSlim));
			this.Name = "dataLock";

		}

	}

}
