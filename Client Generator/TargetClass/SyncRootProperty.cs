namespace FluidTrade.ClientGenerator.TargetClass
{

    using System.CodeDom;
    using FluidTrade.Core;

    /// <summary>
	/// Creates a property that gets the object used to synchrnoize access to the data model.
	/// </summary>
	class SyncRootProperty : CodeMemberProperty
	{

		/// <summary>
		/// Creates a property that gets the object used to synchrnoize access to the data model.
		/// </summary>
		/// <param name="schema">The data model schema.</param>
		public SyncRootProperty(DataModelSchema dataModelSchema)
		{

			//        /// <summary>
			//        /// Gets an object that can be used to synchronize access to the DataModel.
			//        /// </summary>
			//        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
			//        [global::System.ComponentModel.BrowsableAttribute(false)]
			//        [global::System.ComponentModel.DesignerSerializationVisibility(global::System.ComponentModel.DesignerSerializationVisibility.Content)]
			//        public static global::System.Threading.ReaderWriterLock SyncRoot {
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Gets an object that can be used to synchronize access to the {0}.", dataModelSchema.Name), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.CustomAttributes.AddRange(new CodeCustomAttributesForProperties());
			this.Name = "SyncRoot";
			this.Type = new CodeGlobalTypeReference(typeof(System.Object));
			this.Attributes = MemberAttributes.Public | MemberAttributes.Static;

			//            get {
			//                return DataModel.syncRoot;
			//            }
			this.GetStatements.Add(new CodeMethodReturnStatement(new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "syncRoot")));

			//        }

		}

	}
}
