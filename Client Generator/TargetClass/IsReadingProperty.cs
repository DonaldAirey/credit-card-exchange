namespace FluidTrade.ClientGenerator.TargetClass
{

    using System.CodeDom;
    using FluidTrade.Core;

    /// <summary>
	/// Creates a method to handle moving the deleted records from the active data model to the deleted data model.
	/// </summary>
	class IsReconcilingProperty : CodeMemberProperty
	{

		/// <summary>
		/// Creates a method to handle moving the deleted records from the active data model to the deleted data model.
		/// </summary>
		/// <param name="schema">The data model schema.</param>
		public IsReconcilingProperty(DataModelSchema dataModelSchema)
		{

			//        /// <summary>
			//        /// Gets or sets an indication of whether the background thread that reconciles the client data model is running or not.
			//        /// </summary>
			//        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
			//        [global::System.ComponentModel.BrowsableAttribute(false)]
			//        [global::System.ComponentModel.DesignerSerializationVisibility(global::System.ComponentModel.DesignerSerializationVisibility.Content)]
			//        public static bool IsReading {
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("Gets or sets an indication of whether the background thread that reconciles the client data model is running or not.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.CustomAttributes.AddRange(new CodeCustomAttributesForMethods());
			this.Name = "IsReading";
			this.Type = new CodeGlobalTypeReference(typeof(System.Boolean));
			this.Attributes = MemberAttributes.Public | MemberAttributes.Static;

			//            get {
			//                try {
			//                    // Prevent other threads from modifying the flag while it is returned to the caller.
			//                    global::System.Threading.Monitor.Enter(DataModel.syncRoot);
			//                    return DataModel.isReading;
			//                }
			//                finally {
			//                    global::System.Threading.Monitor.Exit(DataModel.syncRoot);
			//                }
			//            }
			CodeTryCatchFinallyStatement getTry = new CodeTryCatchFinallyStatement();
			getTry.TryStatements.Add(new CodeCommentStatement("Prevent other threads from modifying the flag while it is returned to the caller."));
			getTry.TryStatements.Add(new CodeMethodInvokeExpression(new CodeGlobalTypeReferenceExpression(typeof(System.Threading.Monitor)), "Enter", new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "syncRoot")));
			getTry.TryStatements.Add(new CodeMethodReturnStatement(new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "isReading")));
			getTry.FinallyStatements.Add(new CodeMethodInvokeExpression(new CodeGlobalTypeReferenceExpression(typeof(System.Threading.Monitor)), "Exit", new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "syncRoot")));
			this.GetStatements.Add(getTry);

			//            set {
			//                try {
			//                    // Prevent other threads from modifying the flag while it is set.
			//                    global::System.Threading.Monitor.Enter(DataModel.syncRoot);
			CodeTryCatchFinallyStatement setTry = new CodeTryCatchFinallyStatement();
			setTry.TryStatements.Add(new CodeCommentStatement("Prevent other threads from modifying the flag while it is set."));
			setTry.TryStatements.Add(new CodeMethodInvokeExpression(new CodeGlobalTypeReferenceExpression(typeof(System.Threading.Monitor)), "Enter", new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "syncRoot")));

			//                    // If the state of the reconciling thread has changed, then either start or stop the background thread
			//                    // depending on the new value.
			//                    if ((DataModel.isReading != value)) {
			setTry.TryStatements.Add(new CodeCommentStatement("If the state of the reconciling thread has changed, then either start or stop the background thread"));
			setTry.TryStatements.Add(new CodeCommentStatement("depending on the new value."));
			CodeConditionStatement isReconcilingChanged = new CodeConditionStatement();
			isReconcilingChanged.Condition = new CodeBinaryOperatorExpression(new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "isReading"), CodeBinaryOperatorType.IdentityInequality, new CodeArgumentReferenceExpression("value"));

			//                        // The background thread that keeps the data model synchronized with the server data model is either
			//                        // started or stopped based on the new value.
			//                        if ((DataModel.isReading = value)) {
			isReconcilingChanged.TrueStatements.Add(new CodeCommentStatement("The background thread that keeps the data model synchronized with the server data model is either"));
			isReconcilingChanged.TrueStatements.Add(new CodeCommentStatement("started or stopped based on the new value."));
			CodeConditionStatement isReconciling = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "isReading"),
					CodeBinaryOperatorType.Assign,
					new CodeArgumentReferenceExpression("value")));

			//							// This will pre-load the data model to save time starting up.
			isReconciling.TrueStatements.Add(new CodeCommentStatement("This will pre-load the data model to save time starting up."));

			//                            // This thread will periodically ask the server for records missing from the client version of the
			//                            // data model.
			//                            DataModel.reconcilerThread = new global::System.Threading.Thread(DataModel.ReadDataModel);
			//                            DataModel.reconcilerThread.Name = "Data Model Reader Thread";
			//                            DataModel.reconcilerThread.Start();
			//                        }
			isReconciling.TrueStatements.Add(new CodeCommentStatement("This thread will periodically ask the server for records missing from the client version of the"));
			isReconciling.TrueStatements.Add(new CodeCommentStatement("data model."));
			isReconciling.TrueStatements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "reconcilerThread"), new CodeObjectCreateExpression(new CodeGlobalTypeReference(typeof(System.Threading.Thread)), new CodeMethodReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "ReadDataModel"))));
			isReconciling.TrueStatements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "reconcilerThread"), "Name"), new CodePrimitiveExpression("Data Model Reader Thread")));
			isReconciling.TrueStatements.Add(new CodeMethodInvokeExpression(new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "reconcilerThread"), "Start"));

			//                        else {
			//                            // This will kill the reconciling thread.  Special consideration is given if the caller is the
			//                            // reconciling thread to make sure that the locking of the monitor is balanced out.
			//                            if ((global::System.Threading.Thread.CurrentThread == DataModel.reconcilerThread)) {
			//                                // If this property accessed by the reconciling thread, then there is no need to join the thread to
			//                                // abort it.
			//                                global::System.Threading.Thread.CurrentThread.Abort();
			//                            }
			isReconciling.FalseStatements.Add(new CodeCommentStatement("This will kill the reconciling thread.  Special consideration is given if the caller is the"));
			isReconciling.FalseStatements.Add(new CodeCommentStatement("reconciling thread to make sure that the locking of the monitor is balanced out."));
			CodeConditionStatement isCurrentThread = new CodeConditionStatement(new CodeBinaryOperatorExpression(new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(System.Threading.Thread)), "CurrentThread"), CodeBinaryOperatorType.IdentityEquality,
				new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "reconcilerThread")));
			isCurrentThread.TrueStatements.Add(new CodeCommentStatement("If this property accessed by the reconciling thread, then there is no need to join the thread to"));
			isCurrentThread.TrueStatements.Add(new CodeCommentStatement("abort it."));
			isCurrentThread.TrueStatements.Add(new CodeMethodInvokeExpression(new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(System.Threading.Thread)), "CurrentThread"), "Abort"));

			//                            else {
			//                                // When joining the thread to shut it down gracefully, the critical code lock must be released in
			//                                // order to allow the Reconciling thread to run.  That same lock must be re acquired in order to
			//                                // leave the books balanced when the property access method exits.
			//                                global::System.Threading.Monitor.Exit(DataModel.syncRoot);
			//                                if ((DataModel.reconcilerThread.Join(DataModel.threadWaitTime) == false)) {
			//                                    DataModel.reconcilerThread.Abort();
			//                                }
			//                                global::System.Threading.Monitor.Enter(DataModel.syncRoot);
			//                            }
			isCurrentThread.FalseStatements.Add(new CodeCommentStatement("When joining the thread to shut it down gracefully, the critical code lock must be released in"));
			isCurrentThread.FalseStatements.Add(new CodeCommentStatement("order to allow the Reconciling thread to run.  That same lock must be re acquired in order to"));
			isCurrentThread.FalseStatements.Add(new CodeCommentStatement("leave the books balanced when the property access method exits."));
			isCurrentThread.FalseStatements.Add(new CodeMethodInvokeExpression(new CodeGlobalTypeReferenceExpression(typeof(System.Threading.Monitor)), "Exit", new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "syncRoot")));
			CodeConditionStatement ifReaderRunning = new CodeConditionStatement(new CodeBinaryOperatorExpression(new CodeMethodInvokeExpression(new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "reconcilerThread"), "Join", new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "threadWaitTime")), CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(false)));
			ifReaderRunning.TrueStatements.Add(new CodeMethodInvokeExpression(new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "reconcilerThread"), "Abort"));
			isCurrentThread.FalseStatements.Add(ifReaderRunning);
			isCurrentThread.FalseStatements.Add(new CodeMethodInvokeExpression(new CodeGlobalTypeReferenceExpression(typeof(System.Threading.Monitor)), "Enter", new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "syncRoot")));

			//                        }
			isReconciling.FalseStatements.Add(isCurrentThread);

			//                    }
			isReconcilingChanged.TrueStatements.Add(isReconciling);

			//                }
			setTry.TryStatements.Add(isReconcilingChanged);

			//                finally {
			//                    global::System.Threading.Monitor.Exit(DataModel.syncRoot);
			//                }
			//            }
			setTry.FinallyStatements.Add(new CodeMethodInvokeExpression(new CodeGlobalTypeReferenceExpression(typeof(System.Threading.Monitor)), "Exit", new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "syncRoot")));
			this.SetStatements.Add(setTry);

			//        }

		}

	}
}
