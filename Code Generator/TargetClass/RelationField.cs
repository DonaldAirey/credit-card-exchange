namespace FluidTrade.Core.TargetClass
{

    using System.CodeDom;

	/// <summary>
	/// Creates a field that holds the relationship between two tables.
	/// </summary>
	public class RelationField : CodeMemberField
	{

		/// <summary>
		/// Creates a field that holds the relationship between two tables.
		/// </summary>
		public RelationField(RelationSchema relationSchema)
		{

			//        // Relates the Employee table to the ProjectMember table.
			//        private static global::System.Data.DataRelation relationEmployeeProjectMember;
			this.Attributes = MemberAttributes.Private | MemberAttributes.Static;
			this.Type = new CodeGlobalTypeReference(typeof(System.Data.DataRelation));
			this.Name = relationSchema.IsDistinctPathToParent ?
				string.Format("relation{0}{1}", relationSchema.ParentTable.Name, relationSchema.ChildTable.Name) :
				string.Format("relation{0}{1}By{2}", relationSchema.ParentTable.Name, relationSchema.ChildTable.Name, relationSchema.Name);

		}

	}

}
