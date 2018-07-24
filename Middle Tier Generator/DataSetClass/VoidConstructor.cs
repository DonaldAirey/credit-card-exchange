namespace FluidTrade.MiddleTierGenerator.DataSetClass
{

    using System;
    using System.CodeDom;
    using System.Threading;
    using FluidTrade.Core;

	/// <summary>
	/// Represents a declaration of a property that gets the parent row.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	class VoidConstructor : CodeConstructor
	{

		/// <summary>
		/// Generates a property to get a parent row.
		/// </summary>
		/// <param name="foreignKeyConstraintSchema">The foreign key that references the parent table.</param>
		public VoidConstructor(DataModelSchema dataModelSchema)
		{

			//		/// <summary>
			//		/// Creates the System.DataSet used to hold the data for the DataModel.
			//		/// </summary>
			//		internal DataModelDataSet()
			//		{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Creates the System.DataSet used to hold the data for the {0}.", dataModelSchema.Name), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Attributes = MemberAttributes.Assembly;

			//			this.dataLock = new global::System.Threading.ReaderWriterLockSlim();
			this.Statements.Add(
				new CodeAssignStatement(
					new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "dataLock"),
					new CodeObjectCreateExpression(new CodeGlobalTypeReference(typeof(ReaderWriterLockSlim)))));

			//			this.getFilterContextHandler = this.GetFilterContext;
			this.Statements.Add(new CodeAssignStatement(
				new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "getFilterContextHandler"),
				new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "GetFilterContext")));

			//			this.identifier = FluidTrade.Guardian.Properties.Settings.Default.DataModelInstanceId;
			this.Statements.Add(
				new CodeAssignStatement(
					new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "identifier"),
					new CodePropertyReferenceExpression(
						new CodePropertyReferenceExpression(
							new CodePropertyReferenceExpression(
								new CodeTypeReferenceExpression(string.Format("{0}.{1}", dataModelSchema.TargetNamespace, "Properties")),
								"Settings"),
							"Default"),
						"DataModelInstanceId")));

			//			this.readBufferSize = FluidTrade.Guardian.Properties.Settings.Default.ReadBufferSize;
			this.Statements.Add(
				new CodeAssignStatement(
					new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "readBufferSize"),
					new CodePropertyReferenceExpression(
						new CodePropertyReferenceExpression(
							new CodePropertyReferenceExpression(
								new CodeTypeReferenceExpression(string.Format("{0}.{1}", dataModelSchema.TargetNamespace, "Properties")),
								"Settings"),
							"Default"),
						"ReadBufferSize")));

			//			this.rowVersion = 0;
			this.Statements.Add(
				new CodeAssignStatement(
					new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "rowVersion"),
					new CodePrimitiveExpression(0L)));

			//			this.transactionLog = new global::System.Collections.Generic.LinkedList<TransactionLogItem>();
			this.Statements.Add(
				new CodeAssignStatement(
					new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "transactionLog"),
					new CodeObjectCreateExpression(new CodeTypeReference("global::System.Collections.Generic.LinkedList<TransactionLogItem>"))));

			//			this.transactionLogAge = FluidTrade.Guardian.Properties.Settings.Default.TransactionLogAge;
			this.Statements.Add(
				new CodeAssignStatement(
					new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "transactionLogAge"),
					new CodePropertyReferenceExpression(
						new CodePropertyReferenceExpression(
							new CodePropertyReferenceExpression(
								new CodeTypeReferenceExpression(string.Format("{0}.{1}", dataModelSchema.TargetNamespace, "Properties")),
								"Settings"),
							"Default"),
						"TransactionLogAge")));

			//			this.transactionLogCompressionSize = FluidTrade.Guardian.Properties.Settings.Default.TransactionLogCompressionSize;
			this.Statements.Add(
				new CodeAssignStatement(
					new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "transactionLogCompressionSize"),
					new CodePropertyReferenceExpression(
						new CodePropertyReferenceExpression(
							new CodePropertyReferenceExpression(
								new CodeTypeReferenceExpression(string.Format("{0}.{1}", dataModelSchema.TargetNamespace, "Properties")),
								"Settings"),
							"Default"),
						"TransactionLogCompressionSize")));

			//			this.transactionLogCompressionTime = FluidTrade.Guardian.Properties.Settings.Default.TransactionLogCompressionTime;
			this.Statements.Add(
				new CodeAssignStatement(
					new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "transactionLogCompressionTime"),
					new CodePropertyReferenceExpression(
						new CodePropertyReferenceExpression(
							new CodePropertyReferenceExpression(
								new CodeTypeReferenceExpression(string.Format("{0}.{1}", dataModelSchema.TargetNamespace, "Properties")),
								"Settings"),
							"Default"),
						"TransactionLogCompressionTime")));

			//			this.transactionLogLock = new global::System.Threading.ReaderWriterLockSlim();
			this.Statements.Add(
				new CodeAssignStatement(
					new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "transactionLogLock"),
					new CodeObjectCreateExpression(new CodeGlobalTypeReference(typeof(ReaderWriterLockSlim)))));

			//			this.compressorThread = new global::System.Threading.Thread(this.CompressLog);
			this.Statements.Add(
				new CodeAssignStatement(
					new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "compressorThread"),
					new CodeObjectCreateExpression(
						new CodeGlobalTypeReference(typeof(System.Threading.Thread)),
						new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "CompressLog"))));

			//			this.compressorThread.IsBackground = true;
			this.Statements.Add(
				new CodeAssignStatement(
					new CodePropertyReferenceExpression(
						new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "compressorThread"),
						"IsBackground"),
					new CodePrimitiveExpression(true)));

			//			this.compressorThread.Name = "Transaction Log Compressor";
			this.Statements.Add(
				new CodeAssignStatement(
					new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "compressorThread"), "Name"),
					new CodePrimitiveExpression("Transaction Log Compressor")));

			//			this.compressorThread.Priority = global::System.Threading.ThreadPriority.Lowest;
			this.Statements.Add(
				new CodeAssignStatement(
					new CodePropertyReferenceExpression(
						new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "compressorThread"),
						"Priority"),
					new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(ThreadPriority)), "Lowest")));

			//			this.compressorThread.Start();
			this.Statements.Add(
				new CodeMethodInvokeExpression(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "compressorThread"), "Start"));

			//		}

		}

	}

}
