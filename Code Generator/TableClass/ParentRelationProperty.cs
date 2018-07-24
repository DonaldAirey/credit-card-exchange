namespace FluidTrade.Core.TableClass
{

    using System.CodeDom;

	/// <summary>
	/// Represents a declaration of a property that gets the parent row.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public class ParentRelationProperty : CodeMemberProperty
	{

		/// <summary>
		/// Generates a property to get a parent row.
		/// </summary>
		/// <param name="foreignKeyConstraintSchema">The foreign key that references the parent table.</param>
		public ParentRelationProperty(RelationSchema relationSchema)
		{

			//			/// <summary>
			//			/// Gets the parent relation between the Department and Employee tables.
			//			/// </summary>
			//			public System.Data.DataRelation DepartmentEmployeeRelation
			//			{
			//				get
			//				{
			//					return this.relationDepartmentEmployee;
			//				}
			//			}
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Gets the parent relation between the {0} and {1} tables.", relationSchema.ParentTable.Name, relationSchema.ChildTable.Name), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			this.Type = new CodeGlobalTypeReference(typeof(System.Data.DataRelation));
			this.Name = relationSchema.IsDistinctPathToParent ?
				string.Format("{0}{1}Relation", relationSchema.ParentTable.Name, relationSchema.ChildTable.Name) :
				string.Format("{0}{1}By{2}Relation", relationSchema.ParentTable.Name, relationSchema.ChildTable.Name, relationSchema.Name);
			this.GetStatements.Add(new CodeMethodReturnStatement(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(),
				relationSchema.IsDistinctPathToParent ?
				string.Format("relation{0}{1}", relationSchema.ParentTable.Name, relationSchema.ChildTable.Name) :
				string.Format("relation{0}{1}By{2}", relationSchema.ParentTable.Name, relationSchema.ChildTable.Name, relationSchema.Name))));

		}

	}

}
