namespace FluidTrade.ClientGenerator.TargetClass
{

    using System.CodeDom;
    using System.Data;
    using FluidTrade.Core;

    /// <summary>
	/// Creates a property to return a collection of the tables in the data model.
	/// </summary>
	public class TablesProperty : CodeMemberProperty
	{

		/// <summary>
		/// Creates a property to return a collection of the tables in the data model.
		/// </summary>
		/// <param name="dataModelSchema">The data model schema.</param>
		public TablesProperty(DataModelSchema dataModelSchema)
		{

			//        /// <summary>
			//        /// Gets the collection of tables contained in the DataModel.
			//        /// </summary>
			//        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
			//        [global::System.ComponentModel.BrowsableAttribute(false)]
			//        [global::System.ComponentModel.DesignerSerializationVisibility(global::System.ComponentModel.DesignerSerializationVisibility.Content)]
			//        public static global::System.Data.DataTableCollection Tables
			//		  {
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Gets the collection of tables contained in the {0}.", dataModelSchema.Name), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.CustomAttributes.AddRange(new CodeCustomAttributesForProperties());
			this.Attributes = MemberAttributes.Public | MemberAttributes.Static;
			this.Type = new CodeGlobalTypeReference(typeof(DataTableCollection));
			this.Name = "Tables";

			//            get {
			//                return FluidTrade.UnitTest.Server.DataModel.dataSet.Tables;
			//            }
			this.GetStatements.Add(
				new CodeMethodReturnStatement(
					new CodePropertyReferenceExpression(
						new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "dataSet"),
						"Tables")));

			//        }

		}

	}
}
