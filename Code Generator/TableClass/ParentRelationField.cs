namespace FluidTrade.Core.TableClass
{

    using System.CodeDom;

    /// <summary>
	/// Creates a field that holds the relation to a parent table.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public class ParentRelationField : CodeMemberField
	{

		/// <summary>
		/// Creates a field that holds the relation to a parent table.
		/// </summary>
		/// <param name="relationSchema">The description of a relationship between two tables.</param>
		public ParentRelationField(RelationSchema relationSchema)
		{

			//            // The Relation between the Department and Employee tables
			//            private global::System.Data.DataRelation relationDepartmentEmployee;
			this.Type = new CodeGlobalTypeReference(typeof(System.Data.DataRelation));
			this.Name = relationSchema.IsDistinctPathToParent ?
				string.Format("relation{0}{1}", relationSchema.ParentTable.Name, relationSchema.ChildTable.Name) :
				string.Format("relation{0}{1}By{2}", relationSchema.ParentTable.Name, relationSchema.ChildTable.Name, relationSchema.Name);

		}

	}

}
