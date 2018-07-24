namespace FluidTrade.MiddleTierGenerator.TargetClass
{

    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using FluidTrade.Core;
    using FluidTrade.Core.TargetClass;

	/// <summary>
	/// Creates a static constructor for the data model.
	/// </summary>
	class StaticConstructor : CodeTypeConstructor
	{

		/// <summary>
		/// Create a static constructor for the data model.
		/// </summary>
		/// <param name="dataModelSchema">A description of the data model.</param>
		public StaticConstructor(DataModelSchema dataModelSchema)
		{

			/// <summary>
			//        static DataModel() {
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Static Constructor for the {0}.", dataModelSchema.Name), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));

			//			DataModel.lockTimeout = global::FluidTrade.Core.Properties.Settings.Default.LockTimeout;
			this.Statements.Add(
				new CodeAssignStatement(
					new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "lockTimeout"),
					new CodePropertyReferenceExpression(
						new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(FluidTrade.Core.Properties.Settings)), "Default"),
					"LockTimeout")));

			//			DataModel.dataSet = new global::System.Data.DataSet();
			this.Statements.Add(
				new CodeAssignStatement(
					new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), String.Format("{0}DataSet", CommonConversion.ToCamelCase(dataModelSchema.Name))),
					new CodeObjectCreateExpression(new CodeTypeReference(String.Format("{0}DataSet", dataModelSchema.Name)))));

			//			DataModel.dataSet.DataSetName = "DataModel";
			this.Statements.Add(
				new CodeAssignStatement(
					new CodePropertyReferenceExpression(
						new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), String.Format("{0}DataSet", CommonConversion.ToCamelCase(dataModelSchema.Name))),
						"DataSetName"),
					new CodePrimitiveExpression(dataModelSchema.Name)));

			//			DataModel.dataSet.CaseSensitive = true;
			this.Statements.Add(
				new CodeAssignStatement(
					new CodePropertyReferenceExpression(
						new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), String.Format("{0}DataSet", CommonConversion.ToCamelCase(dataModelSchema.Name))), "CaseSensitive"),
						new CodePrimitiveExpression(true)));

			//			DataModel.dataSet.EnforceConstraints = true;
			this.Statements.Add(
				new CodeAssignStatement(
					new CodePropertyReferenceExpression(
						new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), String.Format("{0}DataSet", CommonConversion.ToCamelCase(dataModelSchema.Name))), "EnforceConstraints"),
						new CodePrimitiveExpression(true)));

			// Create each of the tables and add them to the data set.
			for (int tableIndex = 0; tableIndex < dataModelSchema.Tables.Count; tableIndex++)
			{

				KeyValuePair<string, TableSchema> keyValuePair = Enumerable.ElementAt(dataModelSchema.Tables, tableIndex);

				//            FluidTrade.UnitTest.Server.DataModel.tableConfiguration = new ConfigurationDataTable();
				this.Statements.Add(
					new CodeAssignStatement(
						new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), String.Format("table{0}", keyValuePair.Value.Name)),
						new CodeObjectCreateExpression(string.Format("{0}DataTable", keyValuePair.Value.Name))));

				//            DataModel.tableConfiguration.Ordinal = 0;
				this.Statements.Add(
					new CodeAssignStatement(
						new CodePropertyReferenceExpression(
							new CodePropertyReferenceExpression(
								new CodeTypeReferenceExpression(dataModelSchema.Name),
								String.Format("table{0}", keyValuePair.Value.Name)),
							"Ordinal"),
						new CodePrimitiveExpression(tableIndex)));

				//            FluidTrade.UnitTest.Server.DataModel.dataSet.Tables.Add(FluidTrade.UnitTest.Server.DataModel.tableConfiguration);
				this.Statements.Add(
					new CodeMethodInvokeExpression(
						new CodePropertyReferenceExpression(
							new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), String.Format("{0}DataSet", CommonConversion.ToCamelCase(dataModelSchema.Name))), "Tables"),
						"Add",
						new CodePropertyReferenceExpression(
							new CodeTypeReferenceExpression(dataModelSchema.Name),
							String.Format("table{0}", keyValuePair.Value.Name))));

			}

			//            global::System.Data.ForeignKeyConstraint foreignKeyConstraint0 = new System.Data.ForeignKeyConstraint("FK_Object_Department", new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableObject.ObjectIdColumn}, new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableDepartment.DepartmentIdColumn});
			//            FluidTrade.UnitTest.Server.DataModel.tableDepartment.Constraints.Add(foreignKeyConstraint0);
			//            global::System.Data.ForeignKeyConstraint foreignKeyConstraint1 = new System.Data.ForeignKeyConstraint("FK_Department_Employee", new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableDepartment.DepartmentIdColumn}, new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableEmployee.DepartmentIdColumn});
			//            FluidTrade.UnitTest.Server.DataModel.tableEmployee.Constraints.Add(foreignKeyConstraint1);
			//            global::System.Data.ForeignKeyConstraint foreignKeyConstraint2 = new System.Data.ForeignKeyConstraint("FK_Object_Employee", new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableObject.ObjectIdColumn}, new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableEmployee.EmployeeIdColumn});
			//            FluidTrade.UnitTest.Server.DataModel.tableEmployee.Constraints.Add(foreignKeyConstraint2);
			//            global::System.Data.ForeignKeyConstraint foreignKeyConstraint3 = new System.Data.ForeignKeyConstraint("FK_Race_Employee", new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableRace.RaceCodeColumn}, new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableEmployee.RaceCodeColumn});
			//            FluidTrade.UnitTest.Server.DataModel.tableEmployee.Constraints.Add(foreignKeyConstraint3);
			//            global::System.Data.ForeignKeyConstraint foreignKeyConstraint4 = new System.Data.ForeignKeyConstraint("FK_Employee_Engineer", new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableEmployee.EmployeeIdColumn}, new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableEngineer.EngineerIdColumn});
			//            FluidTrade.UnitTest.Server.DataModel.tableEngineer.Constraints.Add(foreignKeyConstraint4);
			//            global::System.Data.ForeignKeyConstraint foreignKeyConstraint5 = new System.Data.ForeignKeyConstraint("FK_Manager_Engineer", new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableManager.ManagerIdColumn}, new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableEngineer.ManagerIdColumn});
			//            FluidTrade.UnitTest.Server.DataModel.tableEngineer.Constraints.Add(foreignKeyConstraint5);
			//            global::System.Data.ForeignKeyConstraint foreignKeyConstraint6 = new System.Data.ForeignKeyConstraint("FK_Employee_Manager", new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableEmployee.EmployeeIdColumn}, new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableManager.ManagerIdColumn});
			//            FluidTrade.UnitTest.Server.DataModel.tableManager.Constraints.Add(foreignKeyConstraint6);
			//            global::System.Data.ForeignKeyConstraint foreignKeyConstraint7 = new System.Data.ForeignKeyConstraint("FK_Object_Project", new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableObject.ObjectIdColumn}, new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableProject.ProjectIdColumn});
			//            FluidTrade.UnitTest.Server.DataModel.tableProject.Constraints.Add(foreignKeyConstraint7);
			//            global::System.Data.ForeignKeyConstraint foreignKeyConstraint8 = new System.Data.ForeignKeyConstraint("FK_Employee_ProjectMember", new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableEmployee.EmployeeIdColumn}, new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableProjectMember.EmployeeIdColumn});
			//            FluidTrade.UnitTest.Server.DataModel.tableProjectMember.Constraints.Add(foreignKeyConstraint8);
			//            global::System.Data.ForeignKeyConstraint foreignKeyConstraint9 = new System.Data.ForeignKeyConstraint("FK_Project_ProjectMember", new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableProject.ProjectIdColumn}, new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableProjectMember.ProjectIdColumn});
			//            FluidTrade.UnitTest.Server.DataModel.tableProjectMember.Constraints.Add(foreignKeyConstraint9);
			int foreignKeyCount = 0;
			foreach (KeyValuePair<string, TableSchema> tablePair in dataModelSchema.Tables)
				foreach (KeyValuePair<string, ConstraintSchema> constraintPair in tablePair.Value.Constraints)
					if (constraintPair.Value is ForeignKeyConstraintSchema)
					{

						// Construct a foreign key constraint described by this schema.
						ForeignKeyConstraintSchema foreignKeyConstraintSchema = constraintPair.Value as ForeignKeyConstraintSchema;

						//			FluidTrade.UnitTest.Client.DataModel.relationDepartmentEmployee = new System.Data.DataRelation("FK_Department_Employee", new global::System.Data.DataColumn[] {
						//						FluidTrade.UnitTest.Client.DataModel.tableDepartment.DepartmentIdColumn}, new global::System.Data.DataColumn[] {
						//						FluidTrade.UnitTest.Client.DataModel.tableEmployee.DepartmentIdColumn}, false);
						//			FluidTrade.UnitTest.Client.DataModel.dataSet.Relations.Add(FluidTrade.UnitTest.Client.DataModel.relationDepartmentEmployee);
						CodeVariableReferenceExpression foreignKeyConstraintExpression = new CodeVariableReferenceExpression(string.Format("foreignKeyConstraint{0}", foreignKeyCount++));
						this.Statements.Add(new CodeVariableDeclarationStatement(new CodeGlobalTypeReference(typeof(System.Data.ForeignKeyConstraint)), foreignKeyConstraintExpression.VariableName, new CodeForeignKeyConstraint(foreignKeyConstraintSchema)));
						this.Statements.Add(new CodeMethodInvokeExpression(new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), string.Format("table{0}", tablePair.Value.Name)), "Constraints"), "Add", foreignKeyConstraintExpression));

					}

			//            FluidTrade.UnitTest.Server.DataModel.relationDepartmentEmployee = new System.Data.DataRelation("FK_Department_Employee", new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableDepartment.DepartmentIdColumn}, new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableEmployee.DepartmentIdColumn}, false);
			//            FluidTrade.UnitTest.Server.DataModel.dataSet.Relations.Add(FluidTrade.UnitTest.Server.DataModel.relationDepartmentEmployee);
			//            FluidTrade.UnitTest.Server.DataModel.relationEmployeeEngineer = new System.Data.DataRelation("FK_Employee_Engineer", new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableEmployee.EmployeeIdColumn}, new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableEngineer.EngineerIdColumn}, false);
			//            FluidTrade.UnitTest.Server.DataModel.dataSet.Relations.Add(FluidTrade.UnitTest.Server.DataModel.relationEmployeeEngineer);
			//            FluidTrade.UnitTest.Server.DataModel.relationEmployeeManager = new System.Data.DataRelation("FK_Employee_Manager", new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableEmployee.EmployeeIdColumn}, new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableManager.ManagerIdColumn}, false);
			//            FluidTrade.UnitTest.Server.DataModel.dataSet.Relations.Add(FluidTrade.UnitTest.Server.DataModel.relationEmployeeManager);
			//            FluidTrade.UnitTest.Server.DataModel.relationEmployeeProjectMember = new System.Data.DataRelation("FK_Employee_ProjectMember", new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableEmployee.EmployeeIdColumn}, new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableProjectMember.EmployeeIdColumn}, false);
			//            FluidTrade.UnitTest.Server.DataModel.dataSet.Relations.Add(FluidTrade.UnitTest.Server.DataModel.relationEmployeeProjectMember);
			//            FluidTrade.UnitTest.Server.DataModel.relationManagerEngineer = new System.Data.DataRelation("FK_Manager_Engineer", new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableManager.ManagerIdColumn}, new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableEngineer.ManagerIdColumn}, false);
			//            FluidTrade.UnitTest.Server.DataModel.dataSet.Relations.Add(FluidTrade.UnitTest.Server.DataModel.relationManagerEngineer);
			//            FluidTrade.UnitTest.Server.DataModel.relationObjectDepartment = new System.Data.DataRelation("FK_Object_Department", new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableObject.ObjectIdColumn}, new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableDepartment.DepartmentIdColumn}, false);
			//            FluidTrade.UnitTest.Server.DataModel.dataSet.Relations.Add(FluidTrade.UnitTest.Server.DataModel.relationObjectDepartment);
			//            FluidTrade.UnitTest.Server.DataModel.relationObjectEmployee = new System.Data.DataRelation("FK_Object_Employee", new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableObject.ObjectIdColumn}, new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableEmployee.EmployeeIdColumn}, false);
			//            FluidTrade.UnitTest.Server.DataModel.dataSet.Relations.Add(FluidTrade.UnitTest.Server.DataModel.relationObjectEmployee);
			//            FluidTrade.UnitTest.Server.DataModel.relationObjectProject = new System.Data.DataRelation("FK_Object_Project", new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableObject.ObjectIdColumn}, new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableProject.ProjectIdColumn}, false);
			//            FluidTrade.UnitTest.Server.DataModel.dataSet.Relations.Add(FluidTrade.UnitTest.Server.DataModel.relationObjectProject);
			//            FluidTrade.UnitTest.Server.DataModel.relationProjectProjectMember = new System.Data.DataRelation("FK_Project_ProjectMember", new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableProject.ProjectIdColumn}, new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableProjectMember.ProjectIdColumn}, false);
			//            FluidTrade.UnitTest.Server.DataModel.dataSet.Relations.Add(FluidTrade.UnitTest.Server.DataModel.relationProjectProjectMember);
			//            FluidTrade.UnitTest.Server.DataModel.relationRaceEmployee = new System.Data.DataRelation("FK_Race_Employee", new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableRace.RaceCodeColumn}, new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableEmployee.RaceCodeColumn}, false);
			//            FluidTrade.UnitTest.Server.DataModel.dataSet.Relations.Add(FluidTrade.UnitTest.Server.DataModel.relationRaceEmployee);
			foreach (KeyValuePair<string, RelationSchema> relationPair in dataModelSchema.Relations)
			{

				// The name of the relation is decorated with the relation name when the relation between the child and the parent
				// isn't unique.
				string relationName = relationPair.Value.IsDistinctPathToParent ?
					string.Format("relation{0}{1}", relationPair.Value.ParentTable.Name, relationPair.Value.ChildTable.Name) :
					string.Format("relation{0}{1}By{2}", relationPair.Value.ParentTable.Name, relationPair.Value.ChildTable.Name,
					relationPair.Value.Name);

				//            FluidTrade.UnitTest.Server.DataModel.relationRaceEmployee = new System.Data.DataRelation("FK_Race_Employee", new FluidTrade.Core.Column[] {
				//                        FluidTrade.UnitTest.Server.DataModel.tableRace.RaceCodeColumn}, new FluidTrade.Core.Column[] {
				//                        FluidTrade.UnitTest.Server.DataModel.tableEmployee.RaceCodeColumn}, false);
				//            FluidTrade.UnitTest.Server.DataModel.dataSet.Relations.Add(FluidTrade.UnitTest.Server.DataModel.relationRaceEmployee);
				CodeExpression relationFieldExpression = new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), relationName);
				this.Statements.Add(new CodeAssignStatement(relationFieldExpression, new CodeDataRelation(relationPair.Value)));
				this.Statements.Add(new CodeMethodInvokeExpression(new CodePropertyReferenceExpression(new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), String.Format("{0}DataSet", CommonConversion.ToCamelCase(dataModelSchema.Name))), "Relations"), "Add", relationFieldExpression));

			}

			//            FluidTrade.UnitTest.Server.DataModel.Configuration.InitializeRelations();
			//            FluidTrade.UnitTest.Server.DataModel.Department.InitializeRelations();
			//            FluidTrade.UnitTest.Server.DataModel.Employee.InitializeRelations();
			//            FluidTrade.UnitTest.Server.DataModel.Engineer.InitializeRelations();
			//            FluidTrade.UnitTest.Server.DataModel.Manager.InitializeRelations();
			//            FluidTrade.UnitTest.Server.DataModel.Object.InitializeRelations();
			//            FluidTrade.UnitTest.Server.DataModel.Project.InitializeRelations();
			//            FluidTrade.UnitTest.Server.DataModel.ProjectMember.InitializeRelations();
			//            FluidTrade.UnitTest.Server.DataModel.Race.InitializeRelations();
			foreach (KeyValuePair<string, TableSchema> keyValuePair in dataModelSchema.Tables)
			{
				this.Statements.Add(new CodeMethodInvokeExpression(new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), keyValuePair.Value.Name), "InitializeRelations"));
			}

			//            global::System.Threading.ThreadPool.QueueUserWorkItem(LoadData);
			this.Statements.Add(
				new CodeMethodInvokeExpression(
					new CodeGlobalTypeReferenceExpression(typeof(ThreadPool)),
					"QueueUserWorkItem",
					new CodeMethodReferenceExpression(
						new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "dataModelDataSet"),
						"LoadData")));

			//        }

		}

	}

}
