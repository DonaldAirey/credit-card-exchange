namespace FluidTrade.MiddleTierGenerator.DataSetClass
{

    using System;
    using System.CodeDom;
    using System.Collections;
	using System.Collections.Generic;
    using System.Data;
    using FluidTrade.Core;

    /// <summary>
	/// Creates a method to handle moving the deleted records from the active data model to the deleted data model.
	/// </summary>
	class ReconcileMethod : CodeMemberMethod
	{

		/// <summary>
		/// Creates a method to handle moving the deleted records from the active data model to the deleted data model.
		/// </summary>
		/// <param name="schema">The data model schema.</param>
		public ReconcileMethod(DataModelSchema dataModelSchema)
		{

			//		/// <summary>
			//		/// Determines whether the given records exist in the current data model.
			//		/// </summary>
			//		/// <param name="keys">An array of record keys and the index of the table to which they belong.</param>
			//		/// <returns>An array of records keys that have been deleted.</returns>
			//		public object[] Reconcile(object[] keys)
			//		{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("Determines whether the given records exist in the current data model.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Comments.Add(new CodeCommentStatement("<param name=\"keys\">An array of record keys and the index of the table to which they belong.</param>", true));
			this.Comments.Add(new CodeCommentStatement("<returns>An array of records keys that have been deleted.</returns>", true));
			this.Name = "Reconcile";
			this.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			this.ReturnType = new CodeGlobalTypeReference(typeof(Object[]));
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(Object[])), "keys"));

			//			global::System.Collections.Generic.List<object> deletedList = new global::System.Collections.Generic.List<object>();
			this.Statements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(List<Object>)),
					"deletedList",
					new CodeObjectCreateExpression(new CodeGlobalTypeReference(typeof(List<Object>)))));

			//			for (int keyIndex = 0; keyIndex < keys.Length; keyIndex = (keyIndex + 1))
			//			{
			CodeIterationStatement forKeyIndex = new CodeIterationStatement(
				new CodeVariableDeclarationStatement(new CodeGlobalTypeReference(typeof(Int32)), "keyIndex", new CodePrimitiveExpression(0)),
				new CodeBinaryOperatorExpression(
					new CodeVariableReferenceExpression("keyIndex"),
					CodeBinaryOperatorType.LessThan,
					new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("keys"), "Length")),
				new CodeAssignStatement(
					new CodeVariableReferenceExpression("keyIndex"),
					new CodeBinaryOperatorExpression(
						new CodeVariableReferenceExpression("keyIndex"),
						CodeBinaryOperatorType.Add,
						new CodePrimitiveExpression(1))));

			//				object[] keyItem = (object[])keys[keyIndex];
			forKeyIndex.Statements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Object[])),
					"keyItem",
					new CodeCastExpression(
						new CodeGlobalTypeReference(typeof(Object[])),
						new CodeIndexerExpression(new CodeVariableReferenceExpression("keys"), new CodeVariableReferenceExpression("keyIndex")))));

			//				System.Data.DataTable dataTable = DataModel.Tables[(int)keyItem[0]];
			forKeyIndex.Statements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(DataTable)),
					"dataTable",
					new CodeIndexerExpression(
						new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "Tables"),
						new CodeCastExpression(
							new CodeGlobalTypeReference(typeof(Int32)),
							new CodeIndexerExpression(new CodeVariableReferenceExpression("keyItem"), new CodePrimitiveExpression(0))))));

			//				try
			//				{
			CodeTryCatchFinallyStatement tryCatch = new CodeTryCatchFinallyStatement();

			//					DataModel.DataLock.EnterReadLock();
			tryCatch.TryStatements.Add(
				new CodeMethodInvokeExpression(
					new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "DataLock"),
					"EnterReadLock"));

			//					if ((dataTable.Rows.Find((object[])keyItem[1]) == null))
			//					{
			CodeConditionStatement ifFound = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodeMethodInvokeExpression(
						new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("dataTable"), "Rows"),
						"Find",
						new CodeCastExpression(
							new CodeGlobalTypeReference(typeof(Object[])),
							new CodeIndexerExpression(new CodeVariableReferenceExpression("keyItem"), new CodePrimitiveExpression(1)))),
					CodeBinaryOperatorType.IdentityEquality,
					new CodePrimitiveExpression(null)));

			//						deletedList.Add(keyItem);
			ifFound.TrueStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeVariableReferenceExpression("deletedList"),
					"Add",
					new CodeVariableReferenceExpression("keyItem")));

			//					}
			tryCatch.TryStatements.Add(ifFound);

			//				}
			//				finally
			//				{
			//					DataModel.DataLock.ExitReadLock();
			tryCatch.FinallyStatements.Add(
				new CodeMethodInvokeExpression(
					new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "DataLock"),
					"ExitReadLock"));

			//				}
			forKeyIndex.Statements.Add(tryCatch);

			//			}
			this.Statements.Add(forKeyIndex);

			//			return deletedList.ToArray();
			this.Statements.Add(new CodeMethodReturnStatement(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("deletedList"), "ToArray")));
	
			//		}

		}

	}

}
