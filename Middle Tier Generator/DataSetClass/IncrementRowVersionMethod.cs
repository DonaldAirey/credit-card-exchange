namespace FluidTrade.MiddleTierGenerator.DataSetClass
{

    using System;
    using System.CodeDom;
    using System.Threading;
    using FluidTrade.Core;

	/// <summary>
	/// Creates a default method for getting a context for a filtered read operation.
	/// </summary>
	class IncrementRowVersionMethod : CodeMemberMethod
	{

		/// <summary>
		/// Provides a default handler for to get a Reader Context.
		/// </summary>
		/// <param name="schema">The data model schema.</param>
		public IncrementRowVersionMethod(DataModelSchema dataModelSchema)
		{

			//		/// <summary>
			//		/// Increments and returns the master row version.
			//		/// </summary>
			//		private long IncrementRowVersion()
			//		{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("Provides a default handler for to get a Reader Context.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			this.ReturnType = new CodeGlobalTypeReference(typeof(Int64));
			this.Name = "IncrementRowVersion";

			//			return global::System.Threading.Interlocked.Increment(ref this.masterRowVersion);
			this.Statements.Add(
				new CodeMethodReturnStatement(
					new CodeMethodInvokeExpression(
						new CodeGlobalTypeReferenceExpression(typeof(Interlocked)),
						"Increment",
						new CodeDirectionExpression(
							FieldDirection.Ref,
							new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "rowVersion")))));

			//		}

		}

	}
}
