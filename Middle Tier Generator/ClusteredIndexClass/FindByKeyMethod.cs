namespace FluidTrade.MiddleTierGenerator.ClusteredIndexClass
{

    using System.CodeDom;
    using FluidTrade.Core;

    /// <summary>
	/// Creates a method that finds a row containing the given elements of an index.
	/// </summary>
	class FindByKeyMethod : CodeMemberMethod
	{

		/// <summary>
		/// Creates a method that finds a row containing the given elements of an index.
		/// </summary>
		/// <param name="uniqueConstraintSchema">A description of a unique constraint.</param>
		public FindByKeyMethod(UniqueConstraintSchema uniqueConstraintSchema)
		{

			//        /// <summary>
			//        /// Finds a row in the Configuration table containing the key elements.
			//        /// </summary>
			//        /// <param name="key">An array of key elements.</param>
			//        /// <returns>A ConfigurationKey row that contains the key elements, or null if there is no match.</returns>
			//        public new ConfigurationRow Find(object[] key) {
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Finds a row in the {0} table containing the key elements.", uniqueConstraintSchema.Table.Name), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Comments.Add(new CodeCommentStatement("<param name=\"key\">An array of key elements.</param>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("<returns>A {0} row that contains the key elements, or null if there is no match.</returns>", uniqueConstraintSchema.Name), true));
			this.Attributes = MemberAttributes.Public | MemberAttributes.New | MemberAttributes.Final;
			this.ReturnType = new CodeTypeReference(string.Format("{0}Row", uniqueConstraintSchema.Table.Name));
			this.Name = "Find";
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(System.Object[])), "key"));

			//            try {
			//                DataModel.Configuration.AcquireLock();
			//                return ((ConfigurationRow)(base.Find(key)));
			//            }
			//            catch (global::System.ArgumentException argumentException) {
			//                throw new global::System.ServiceModel.FaultException<ArgumentFault>(new global::FluidTrade.Core.ArgumentFault(argumentException.Message));
			//            }
			//            finally {
			//                DataModel.Configuration.ReleaseLock();
			//            }
			CodeTryCatchFinallyStatement tryCatchFinallyStatement = new CodeTryCatchFinallyStatement();
			CodePropertyReferenceExpression tableExpression = new CodePropertyReferenceExpression(
				new CodeTypeReferenceExpression(string.Format("{0}", uniqueConstraintSchema.Table.DataModel.Name)), uniqueConstraintSchema.Table.Name);
			tryCatchFinallyStatement.TryStatements.Add(
				new CodeMethodInvokeExpression(
					new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(uniqueConstraintSchema.Table.DataModel.Name), "DataLock"),
					"EnterReadLock"));
			tryCatchFinallyStatement.TryStatements.Add(new CodeMethodReturnStatement(new CodeCastExpression(this.ReturnType, new CodeMethodInvokeExpression(new CodeBaseReferenceExpression(), "Find", new CodeArgumentReferenceExpression("key")))));
			CodeCatchClause catchArgumentException = new CodeCatchClause("argumentException", new CodeGlobalTypeReference(typeof(System.ArgumentException)));
			catchArgumentException.Statements.Add(new CodeThrowArgumentExceptionStatement(new CodePropertyReferenceExpression(new CodeArgumentReferenceExpression("argumentException"), "Message")));
			tryCatchFinallyStatement.CatchClauses.Add(catchArgumentException);
			CodeCatchClause catchFormatException = new CodeCatchClause("formatException", new CodeGlobalTypeReference(typeof(System.FormatException)));
			catchFormatException.Statements.Add(new CodeThrowFormatExceptionStatement(new CodePropertyReferenceExpression(new CodeArgumentReferenceExpression("formatException"), "Message")));
			tryCatchFinallyStatement.CatchClauses.Add(catchFormatException);
			tryCatchFinallyStatement.FinallyStatements.Add(
				new CodeMethodInvokeExpression(
					new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(uniqueConstraintSchema.Table.DataModel.Name), "DataLock"),
					"ExitReadLock"));
			this.Statements.Add(tryCatchFinallyStatement);

			//        }

		}
	}
}
