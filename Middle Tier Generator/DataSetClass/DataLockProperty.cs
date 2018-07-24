namespace FluidTrade.MiddleTierGenerator.DataSetClass
{

    using System.CodeDom;
    using System.Threading;
    using FluidTrade.Core;

	/// <summary>
	/// Generates a property that gets the lock for the data model.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	class DataLockProperty : CodeMemberProperty
	{

		/// <summary>
		/// Generates a property that gets the lock for the data model.
		/// </summary>
		public DataLockProperty()
		{

			//		/// <summary>
			//		/// Gets the lock for the data model.
			//		/// </summary>
			//		public global::System.Threading.ReaderWriterLockSlim DataLock
			//		{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("Gets the lock for the data model.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.CustomAttributes.AddRange(new CodeCustomAttributesForProperties());
			this.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			this.Type = new CodeGlobalTypeReference(typeof(ReaderWriterLockSlim));
			this.Name = "DataLock";

			//			get
			//			{
			//				return this.dataLock;
			//			}
			this.GetStatements.Add(
				new CodeMethodReturnStatement(
					new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "dataLock")));

			//		}

		}

	}

}
