namespace FluidTrade.ClientGenerator.TargetClass
{

    using System.CodeDom;
    using FluidTrade.Core;

    /// <summary>
	/// Creates a method to dispose of the resources used by the data model.
	/// </summary>
	class WaitForUpdateMethod : CodeMemberMethod
	{

		/// <summary>
		/// Creates a method to dispose of the resources used by the data model.
		/// </summary>
		/// <param name="schema">The data model schema.</param>
		public WaitForUpdateMethod(DataModelSchema dataModelSchema)
		{

			//		/// <summary>
			//		/// Waits for an update to the data model.
			//		/// </summary>
			//		public static void WaitForUpdate()
			//		{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Dispose of the managed resources allocated by this object.", dataModelSchema.Name), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Comments.Add(new CodeCommentStatement("<param name=\"disposing\">true to indicate managed resources should be relesaed.</param>", true));
			this.CustomAttributes.AddRange(new CodeCustomAttributesForMethods());
			this.Name = "WaitForUpdate";
			this.Attributes = MemberAttributes.Public | MemberAttributes.Static;

			//			try
			//          {
			//              Monitor.Enter(DataModel.syncUpdate);
			//				Monitor.Wait(DataModel.syncUpdate);
			//          }
			//          finally
			//          {
			//              Monitor.Exit(DataModel.syncUpdate);
			//          }
			CodeTryCatchFinallyStatement tryLockUpdateEvent = new CodeTryCatchFinallyStatement();
			tryLockUpdateEvent.TryStatements.Add(new CodeMethodInvokeExpression(new CodeGlobalTypeReferenceExpression(typeof(System.Threading.Monitor)), "Enter", new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "syncUpdate")));
			tryLockUpdateEvent.TryStatements.Add(new CodeMethodInvokeExpression(new CodeGlobalTypeReferenceExpression(typeof(System.Threading.Monitor)), "Wait", new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "syncUpdate")));
			tryLockUpdateEvent.FinallyStatements.Add(new CodeMethodInvokeExpression(new CodeGlobalTypeReferenceExpression(typeof(System.Threading.Monitor)), "Exit", new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "syncUpdate")));
			this.Statements.Add(tryLockUpdateEvent);

			//		}

		}

	}
}
