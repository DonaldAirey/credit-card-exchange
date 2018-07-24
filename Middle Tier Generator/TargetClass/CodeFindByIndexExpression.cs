namespace FluidTrade.MiddleTierGenerator.TargetClass
{

    using System.CodeDom;
    using System.Collections.Generic;
    using FluidTrade.Core;

	/// <summary>
	/// Creates an expression to find a record based on the primary index of a table.
	/// </summary>
	class CodeFindByIndexExpression : CodeMethodInvokeExpression
	{

		/// <summary>
		/// Creates an expression to find a record based on the primary index of a table.
		/// </summary>
		/// <param name="tableSchema"></param>
		public CodeFindByIndexExpression(TableSchema tableSchema, CodeExpression keyExpression)
		{

			//            FluidTrade.UnitTest.Server.DataModel.Employee.EmployeeKey.Find(new object[] {
			//                        employeeId});
			foreach (KeyValuePair<string, ConstraintSchema> constraintPair in tableSchema.Constraints)
				if (constraintPair.Value is UniqueConstraintSchema)
				{
					UniqueConstraintSchema uniqueConstraintSchema = constraintPair.Value as UniqueConstraintSchema;
					if (uniqueConstraintSchema.IsPrimaryKey)
					{
						this.Method = new CodeMethodReferenceExpression(new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(tableSchema.DataModel.Name), tableSchema.Name), uniqueConstraintSchema.Name), "Find");
						this.Parameters.Add(keyExpression);
					}
				}

		}

	}

}
