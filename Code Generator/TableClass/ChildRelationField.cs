namespace FluidTrade.Core.TableClass
{

    using System.CodeDom;

    /// <summary>
	/// Creates a relationship to a child table.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public class ChildRelationField : CodeMemberField
	{

		/// <summary>
		/// Creates a relationship to a child table.
		/// </summary>
		/// <param name="relationSchema">A description of the relationship between two tables.</param>
		public ChildRelationField(RelationSchema relationSchema)
		{

			//            // The Relation between the Employee and Engineer tables
			//            private global::System.Data.DataRelation relationEmployeeEngineer;
			this.Type = new CodeGlobalTypeReference(typeof(System.Data.DataRelation));
			this.Name = relationSchema.IsDistinctPathToChild ?
				string.Format("relation{0}{1}", relationSchema.ParentTable.Name, relationSchema.ChildTable.Name) :
				string.Format("relation{0}{1}By{2}", relationSchema.ParentTable.Name, relationSchema.ChildTable.Name, relationSchema.Name);

		}

	}

}
