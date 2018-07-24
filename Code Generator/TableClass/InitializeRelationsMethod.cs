namespace FluidTrade.Core.TableClass
{

    using System.CodeDom;
	using System.Collections.Generic;

	/// <summary>
	/// Creates a method to initialize all the parent and child relationships.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public class InitializeRelationsMethod : CodeMemberMethod
	{

		/// <summary>
		/// Creates a method to initialize all the parent and child relationships.
		/// </summary>
		/// <param name="tableSchema">A description of the table.</param>
		public InitializeRelationsMethod(TableSchema tableSchema)
		{

			//            /// <summary>
			//            /// Initializes the relation fields with the parent and child relations.
			//            /// </summary>
			//            internal void InitializeRelations() {
			//                // The Relation between the Department and Employee tables
			//                this.relationDepartmentEmployee = this.ParentRelations["FK_Department_Employee"];
			//                // The Relation between the Object and Employee tables
			//                this.relationObjectEmployee = this.ParentRelations["FK_Object_Employee"];
			//                // The Relation between the Race and Employee tables
			//                this.relationRaceEmployee = this.ParentRelations["FK_Race_Employee"];
			//                // The Relation between the Employee and Engineer tables
			//                this.relationEmployeeEngineer = this.ChildRelations["FK_Employee_Engineer"];
			//                // The Relation between the Employee and Manager tables
			//                this.relationEmployeeManager = this.ChildRelations["FK_Employee_Manager"];
			//                // The Relation between the Employee and ProjectMember tables
			//                this.relationEmployeeProjectMember = this.ChildRelations["FK_Employee_ProjectMember"];
			//            }
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("Initializes the relation fields with the parent and child relations.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Attributes = MemberAttributes.Assembly | MemberAttributes.Final;
			this.Name = "InitializeRelations";
			foreach (KeyValuePair<string, RelationSchema> relationPair in tableSchema.ParentRelations)
			{
				// If the foreign keys reference the same parent table, then the names of the methods will need to be decorated with
				// the foreign key name in order to make them unique.
				string relationName = relationPair.Value.IsDistinctPathToParent ?
					string.Format("relation{0}{1}", relationPair.Value.ParentTable.Name, relationPair.Value.ChildTable.Name) :
					string.Format("relation{0}{1}By{2}", relationPair.Value.ParentTable.Name, relationPair.Value.ChildTable.Name, relationPair.Value.Name);
				this.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), relationName),
					new CodeIndexerExpression(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "ParentRelations"), new CodePrimitiveExpression(relationPair.Value.Name))));
			}
			foreach (KeyValuePair<string, RelationSchema> relationPair in tableSchema.ChildRelations)
			{
				// If the foreign keys reference the same parent table, then the names of the methods will need to be decorated with
				// the foreign key name in order to make them unique.
				string relationName = relationPair.Value.IsDistinctPathToChild ?
					string.Format("relation{0}{1}", relationPair.Value.ParentTable.Name, relationPair.Value.ChildTable.Name) :
					string.Format("relation{0}{1}By{2}", relationPair.Value.ParentTable.Name, relationPair.Value.ChildTable.Name, relationPair.Value.Name);
				this.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), relationName),
					new CodeIndexerExpression(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "ChildRelations"), new CodePrimitiveExpression(relationPair.Value.Name))));
			}

		}

	}

}
