namespace FluidTrade.Core.TargetClass
{

    using System;
    using System.CodeDom;
    using System.Collections.Generic;

	/// <summary>
	/// Create a representation of the creation of a data relation.
	/// </summary>
	public class CodeDataRelation : CodeObjectCreateExpression
	{

		/// <summary>
		/// Create a representation of the creation of a data relation.
		/// </summary>
		/// <param name="relationSchema">The description of a relationship between two tables.</param>
		public CodeDataRelation(RelationSchema relationSchema)
		{

			// Collect the key fields in the parent table.
			List<CodeExpression> parentFieldList = new List<CodeExpression>();
			foreach (ColumnSchema columnSchema in relationSchema.ParentColumns)
				parentFieldList.Add(new CodePropertyReferenceExpression(new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(relationSchema.ParentTable.DataModel.Name), String.Format("table{0}", relationSchema.ParentTable.Name)), String.Format("{0}Column", columnSchema.Name)));

			// Collect the referenced fields in the child table.
			List<CodeExpression> childFieldList = new List<CodeExpression>();
			foreach (ColumnSchema columnSchema in relationSchema.ChildColumns)
				childFieldList.Add(new CodePropertyReferenceExpression(new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(relationSchema.ChildTable.DataModel.Name), String.Format("table{0}", relationSchema.ChildTable.Name)), String.Format("{0}Column", columnSchema.Name)));

			//            new System.Data.ForeignKeyConstraint("FK_Object_Department", new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableObject.ObjectIdColumn}, new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableDepartment.DepartmentIdColumn});
			this.CreateType = new CodeGlobalTypeReference(typeof(System.Data.DataRelation));
			this.Parameters.Add(new CodePrimitiveExpression(relationSchema.Name));
			this.Parameters.Add(new CodeArrayCreateExpression(new CodeGlobalTypeReference(typeof(System.Data.DataColumn)), parentFieldList.ToArray()));
			this.Parameters.Add(new CodeArrayCreateExpression(new CodeGlobalTypeReference(typeof(System.Data.DataColumn)), childFieldList.ToArray()));
			this.Parameters.Add(new CodePrimitiveExpression(false));

		}

	}

}
