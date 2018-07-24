namespace FluidTrade.ClientGenerator.TargetClass
{

    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using FluidTrade.Core;
	using FluidTrade.Core.Properties;
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
			//        /// Static Constructor for the DataModel.
			//        /// </summary>
			//        static DataModel() {
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Static Constructor for the {0}.", dataModelSchema.Name), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));

			//			DataModel.purgeBufferSize = FluidTrade.Guardian.Properties.Settings.Default.PurgeBufferSize;
			this.Statements.Add(
				new CodeAssignStatement(
					new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "purgeBufferSize"),
					new CodePropertyReferenceExpression(
						new CodePropertyReferenceExpression(
							new CodePropertyReferenceExpression(
								new CodeTypeReferenceExpression(string.Format("{0}.{1}", dataModelSchema.TargetNamespace, "Properties")),
								"Settings"),
							"Default"),
						"PurgeBufferSize")));

			//            FluidTrade.UnitTest.Server.DataModel.dataSet = new global::System.Data.DataSet();
			//            FluidTrade.UnitTest.Server.DataModel.dataSet.DataSetName = "DataModel";
			//            FluidTrade.UnitTest.Server.DataModel.dataSet.CaseSensitive = true;
			//            FluidTrade.UnitTest.Server.DataModel.dataSet.EnforceConstraints = true;
			this.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "dataSet"), new CodeObjectCreateExpression(new CodeGlobalTypeReference(typeof(System.Data.DataSet)))));
			this.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "dataSet"), "DataSetName"), new CodePrimitiveExpression(dataModelSchema.Name)));
			this.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "dataSet"), "CaseSensitive"), new CodePrimitiveExpression(true)));
			this.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "dataSet"), "EnforceConstraints"), new CodePrimitiveExpression(true)));

			//            FluidTrade.UnitTest.Server.DataModel.tableConfiguration = new ConfigurationDataTable();
			//            FluidTrade.UnitTest.Server.DataModel.dataSet.Tables.Add(FluidTrade.UnitTest.Server.DataModel.tableConfiguration);
			//            FluidTrade.UnitTest.Server.DataModel.tableDepartment = new DepartmentDataTable();
			//            FluidTrade.UnitTest.Server.DataModel.dataSet.Tables.Add(FluidTrade.UnitTest.Server.DataModel.tableDepartment);
			//            FluidTrade.UnitTest.Server.DataModel.tableEmployee = new EmployeeDataTable();
			//            FluidTrade.UnitTest.Server.DataModel.dataSet.Tables.Add(FluidTrade.UnitTest.Server.DataModel.tableEmployee);
			//            FluidTrade.UnitTest.Server.DataModel.tableEngineer = new EngineerDataTable();
			//            FluidTrade.UnitTest.Server.DataModel.dataSet.Tables.Add(FluidTrade.UnitTest.Server.DataModel.tableEngineer);
			//            FluidTrade.UnitTest.Server.DataModel.tableManager = new ManagerDataTable();
			//            FluidTrade.UnitTest.Server.DataModel.dataSet.Tables.Add(FluidTrade.UnitTest.Server.DataModel.tableManager);
			//            FluidTrade.UnitTest.Server.DataModel.tableObject = new ObjectDataTable();
			//            FluidTrade.UnitTest.Server.DataModel.dataSet.Tables.Add(FluidTrade.UnitTest.Server.DataModel.tableObject);
			//            FluidTrade.UnitTest.Server.DataModel.tableProject = new ProjectDataTable();
			//            FluidTrade.UnitTest.Server.DataModel.dataSet.Tables.Add(FluidTrade.UnitTest.Server.DataModel.tableProject);
			//            FluidTrade.UnitTest.Server.DataModel.tableProjectMember = new ProjectMemberDataTable();
			//            FluidTrade.UnitTest.Server.DataModel.dataSet.Tables.Add(FluidTrade.UnitTest.Server.DataModel.tableProjectMember);
			//            FluidTrade.UnitTest.Server.DataModel.tableRace = new RaceDataTable();
			//            FluidTrade.UnitTest.Server.DataModel.dataSet.Tables.Add(FluidTrade.UnitTest.Server.DataModel.tableRace);
			foreach (KeyValuePair<string, TableSchema> keyValuePair in dataModelSchema.Tables)
			{
				this.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), String.Format("table{0}", keyValuePair.Value.Name)), new CodeObjectCreateExpression(string.Format("{0}DataTable", keyValuePair.Value.Name))));
				this.Statements.Add(new CodeMethodInvokeExpression(new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "dataSet"), "Tables"), "Add", new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), String.Format("table{0}", keyValuePair.Value.Name))));
			}

			//            // Enforce the foreign key constraint between Object and Department tables.
			//            global::System.Data.ForeignKeyConstraint foreignKeyConstraint0 = new System.Data.ForeignKeyConstraint("FK_Object_Department", new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableObject.ObjectIdColumn}, new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableDepartment.DepartmentIdColumn});
			//            FluidTrade.UnitTest.Server.DataModel.tableDepartment.Constraints.Add(foreignKeyConstraint0);
			//            // Enforce the foreign key constraint between Department and Employee tables.
			//            global::System.Data.ForeignKeyConstraint foreignKeyConstraint1 = new System.Data.ForeignKeyConstraint("FK_Department_Employee", new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableDepartment.DepartmentIdColumn}, new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableEmployee.DepartmentIdColumn});
			//            FluidTrade.UnitTest.Server.DataModel.tableEmployee.Constraints.Add(foreignKeyConstraint1);
			//            // Enforce the foreign key constraint between Object and Employee tables.
			//            global::System.Data.ForeignKeyConstraint foreignKeyConstraint2 = new System.Data.ForeignKeyConstraint("FK_Object_Employee", new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableObject.ObjectIdColumn}, new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableEmployee.EmployeeIdColumn});
			//            FluidTrade.UnitTest.Server.DataModel.tableEmployee.Constraints.Add(foreignKeyConstraint2);
			//            // Enforce the foreign key constraint between Race and Employee tables.
			//            global::System.Data.ForeignKeyConstraint foreignKeyConstraint3 = new System.Data.ForeignKeyConstraint("FK_Race_Employee", new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableRace.RaceCodeColumn}, new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableEmployee.RaceCodeColumn});
			//            FluidTrade.UnitTest.Server.DataModel.tableEmployee.Constraints.Add(foreignKeyConstraint3);
			//            // Enforce the foreign key constraint between Employee and Engineer tables.
			//            global::System.Data.ForeignKeyConstraint foreignKeyConstraint4 = new System.Data.ForeignKeyConstraint("FK_Employee_Engineer", new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableEmployee.EmployeeIdColumn}, new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableEngineer.EngineerIdColumn});
			//            FluidTrade.UnitTest.Server.DataModel.tableEngineer.Constraints.Add(foreignKeyConstraint4);
			//            // Enforce the foreign key constraint between Manager and Engineer tables.
			//            global::System.Data.ForeignKeyConstraint foreignKeyConstraint5 = new System.Data.ForeignKeyConstraint("FK_Manager_Engineer", new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableManager.ManagerIdColumn}, new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableEngineer.ManagerIdColumn});
			//            FluidTrade.UnitTest.Server.DataModel.tableEngineer.Constraints.Add(foreignKeyConstraint5);
			//            // Enforce the foreign key constraint between Employee and Manager tables.
			//            global::System.Data.ForeignKeyConstraint foreignKeyConstraint6 = new System.Data.ForeignKeyConstraint("FK_Employee_Manager", new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableEmployee.EmployeeIdColumn}, new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableManager.ManagerIdColumn});
			//            FluidTrade.UnitTest.Server.DataModel.tableManager.Constraints.Add(foreignKeyConstraint6);
			//            // Enforce the foreign key constraint between Object and Project tables.
			//            global::System.Data.ForeignKeyConstraint foreignKeyConstraint7 = new System.Data.ForeignKeyConstraint("FK_Object_Project", new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableObject.ObjectIdColumn}, new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableProject.ProjectIdColumn});
			//            FluidTrade.UnitTest.Server.DataModel.tableProject.Constraints.Add(foreignKeyConstraint7);
			//            // Enforce the foreign key constraint between Employee and ProjectMember tables.
			//            global::System.Data.ForeignKeyConstraint foreignKeyConstraint8 = new System.Data.ForeignKeyConstraint("FK_Employee_ProjectMember", new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableEmployee.EmployeeIdColumn}, new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableProjectMember.EmployeeIdColumn});
			//            FluidTrade.UnitTest.Server.DataModel.tableProjectMember.Constraints.Add(foreignKeyConstraint8);
			//            // Enforce the foreign key constraint between Project and ProjectMember tables.
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
						this.Statements.Add(
							new CodeAssignStatement(
									new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(foreignKeyConstraintExpression.VariableName), "AcceptRejectRule"),
									new CodeFieldReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(System.Data.AcceptRejectRule)), "Cascade")));
						this.Statements.Add(new CodeMethodInvokeExpression(new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), string.Format("table{0}", tablePair.Value.Name)), "Constraints"), "Add", foreignKeyConstraintExpression));

					}

			//            // Create a relation between the Department and Employee tables.
			//            FluidTrade.UnitTest.Server.DataModel.relationDepartmentEmployee = new System.Data.DataRelation("FK_Department_Employee", new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableDepartment.DepartmentIdColumn}, new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableEmployee.DepartmentIdColumn}, false);
			//            FluidTrade.UnitTest.Server.DataModel.dataSet.Relations.Add(FluidTrade.UnitTest.Server.DataModel.relationDepartmentEmployee);
			//            // Create a relation between the Employee and Engineer tables.
			//            FluidTrade.UnitTest.Server.DataModel.relationEmployeeEngineer = new System.Data.DataRelation("FK_Employee_Engineer", new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableEmployee.EmployeeIdColumn}, new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableEngineer.EngineerIdColumn}, false);
			//            FluidTrade.UnitTest.Server.DataModel.dataSet.Relations.Add(FluidTrade.UnitTest.Server.DataModel.relationEmployeeEngineer);
			//            // Create a relation between the Employee and Manager tables.
			//            FluidTrade.UnitTest.Server.DataModel.relationEmployeeManager = new System.Data.DataRelation("FK_Employee_Manager", new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableEmployee.EmployeeIdColumn}, new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableManager.ManagerIdColumn}, false);
			//            FluidTrade.UnitTest.Server.DataModel.dataSet.Relations.Add(FluidTrade.UnitTest.Server.DataModel.relationEmployeeManager);
			//            // Create a relation between the Employee and ProjectMember tables.
			//            FluidTrade.UnitTest.Server.DataModel.relationEmployeeProjectMember = new System.Data.DataRelation("FK_Employee_ProjectMember", new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableEmployee.EmployeeIdColumn}, new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableProjectMember.EmployeeIdColumn}, false);
			//            FluidTrade.UnitTest.Server.DataModel.dataSet.Relations.Add(FluidTrade.UnitTest.Server.DataModel.relationEmployeeProjectMember);
			//            // Create a relation between the Manager and Engineer tables.
			//            FluidTrade.UnitTest.Server.DataModel.relationManagerEngineer = new System.Data.DataRelation("FK_Manager_Engineer", new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableManager.ManagerIdColumn}, new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableEngineer.ManagerIdColumn}, false);
			//            FluidTrade.UnitTest.Server.DataModel.dataSet.Relations.Add(FluidTrade.UnitTest.Server.DataModel.relationManagerEngineer);
			//            // Create a relation between the Object and Department tables.
			//            FluidTrade.UnitTest.Server.DataModel.relationObjectDepartment = new System.Data.DataRelation("FK_Object_Department", new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableObject.ObjectIdColumn}, new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableDepartment.DepartmentIdColumn}, false);
			//            FluidTrade.UnitTest.Server.DataModel.dataSet.Relations.Add(FluidTrade.UnitTest.Server.DataModel.relationObjectDepartment);
			//            // Create a relation between the Object and Employee tables.
			//            FluidTrade.UnitTest.Server.DataModel.relationObjectEmployee = new System.Data.DataRelation("FK_Object_Employee", new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableObject.ObjectIdColumn}, new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableEmployee.EmployeeIdColumn}, false);
			//            FluidTrade.UnitTest.Server.DataModel.dataSet.Relations.Add(FluidTrade.UnitTest.Server.DataModel.relationObjectEmployee);
			//            // Create a relation between the Object and Project tables.
			//            FluidTrade.UnitTest.Server.DataModel.relationObjectProject = new System.Data.DataRelation("FK_Object_Project", new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableObject.ObjectIdColumn}, new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableProject.ProjectIdColumn}, false);
			//            FluidTrade.UnitTest.Server.DataModel.dataSet.Relations.Add(FluidTrade.UnitTest.Server.DataModel.relationObjectProject);
			//            // Create a relation between the Project and ProjectMember tables.
			//            FluidTrade.UnitTest.Server.DataModel.relationProjectProjectMember = new System.Data.DataRelation("FK_Project_ProjectMember", new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableProject.ProjectIdColumn}, new FluidTrade.Core.Column[] {
			//                        FluidTrade.UnitTest.Server.DataModel.tableProjectMember.ProjectIdColumn}, false);
			//            FluidTrade.UnitTest.Server.DataModel.dataSet.Relations.Add(FluidTrade.UnitTest.Server.DataModel.relationProjectProjectMember);
			//            // Create a relation between the Race and Employee tables.
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
				this.Statements.Add(new CodeMethodInvokeExpression(new CodePropertyReferenceExpression(new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "dataSet"), "Relations"), "Add", relationFieldExpression));

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
				this.Statements.Add(new CodeMethodInvokeExpression(new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), string.Format("table{0}", keyValuePair.Value.Name)), "InitializeRelations"));

			//			DataModel.syncUpdate = new global::System.Object();
			this.Statements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "syncUpdate"), new CodeObjectCreateExpression(new CodeGlobalTypeReference(typeof(System.Object)))));

			//			DataModel.syncRoot = new System.Object();
			this.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "syncRoot"), new CodeObjectCreateExpression(new CodeGlobalTypeReference(typeof(System.Object)))));

			//			DataModel.dataSetId = Guid.Empty;
			this.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "dataSetId"), new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(Guid)), "Empty")));
	
			//			DataModel.sequence = int.MinValue;
			this.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "sequence"), new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(Int64)), "MinValue")));

			//			DataModel.updateBufferMutex = new Mutex(false);
			this.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "updateBufferMutex"), new CodeObjectCreateExpression(new CodeGlobalTypeReference(typeof(System.Threading.Mutex)), new CodePrimitiveExpression(false))));

			//        }

		}

	}

}
