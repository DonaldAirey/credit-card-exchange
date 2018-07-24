namespace FluidTrade.MiddleTierGenerator.RowClass
{

    using System.CodeDom;
    using System.Collections.Generic;
    using System.Reflection;
    using FluidTrade.Core;
    using FluidTrade.Core.RowClass;

    /// <summary>
	/// Creates a CodeDOM description of a strongly typed Row.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	class RowClass : CodeTypeDeclaration
	{

		/// <summary>
		/// Creates the CodeDOM for a strongly typed row in a strongly typed table.
		/// </summary>
		/// <param name="tableSchema">The table schema that describes this row.</param>
		public RowClass(TableSchema tableSchema)
		{

			//        /// <summary>
			//        /// Represents a row of data in the Employee table.
			//        /// </summary>
			//        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("FluidTrade.Core.MiddleTier.MiddleTierGenerator", "1.3.0.0")]
			//        public class EmployeeRow : global::System.Data.DataRow, global::FluidTrade.Core.IRow {
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Represents a row of data in the {0} table.", tableSchema.Name), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.CustomAttributes.Add(new CodeGeneratedCodeAttribute(typeof(MiddleTierGenerator)));
			this.Name = string.Format("{0}Row", tableSchema.Name);
			this.TypeAttributes = TypeAttributes.Public | TypeAttributes.Class;
			this.BaseTypes.Add(new CodeGlobalTypeReference(typeof(FluidTrade.Core.DataRowBase)));

			this.IsPartial = true;

			// Private Instance Fields
			List<CodeMemberField> privateInstanceFieldList = new List<CodeMemberField>();
			privateInstanceFieldList.Add(new TableField(tableSchema));
			privateInstanceFieldList.Sort(delegate(CodeMemberField firstField, CodeMemberField secondField) { return firstField.Name.CompareTo(secondField.Name); });
			foreach (CodeMemberField codeMemberField in privateInstanceFieldList)
				this.Members.Add(codeMemberField);

			// Constructors
			this.Members.Add(new RowBuilderConstructor(tableSchema));

			// Properties
			List<CodeMemberProperty> propertyList = new List<CodeMemberProperty>();
			foreach (KeyValuePair<string, RelationSchema> relationPair in tableSchema.ParentRelations)
				propertyList.Add(new ParentRowProperty(relationPair.Value));
			foreach (KeyValuePair<string, ColumnSchema> columnPair in tableSchema.Columns)
				propertyList.Add(new ColumnProperty(tableSchema, columnPair.Value));
			propertyList.Sort(delegate(CodeMemberProperty firstProperty, CodeMemberProperty secondProperty) { return firstProperty.Name.CompareTo(secondProperty.Name); });
			foreach (CodeMemberProperty codeMemberProperty in propertyList)
				this.Members.Add(codeMemberProperty);


			this.Members.Add(new LockTimeoutProperty(tableSchema));

			// Methods
			List<CodeMemberMethod> methodList = new List<CodeMemberMethod>();
			foreach (KeyValuePair<string, RelationSchema> relationPair in tableSchema.ChildRelations)
				methodList.Add(new GetChildRowsMethod(relationPair.Value));
			foreach (KeyValuePair<string, ColumnSchema> columnPair in tableSchema.Columns)
				if (columnPair.Value.IsNullable)
				{
					methodList.Add(new IsNullMethod(tableSchema, columnPair.Value));
					methodList.Add(new SetNullMethod(tableSchema, columnPair.Value));
				}
			methodList.Sort(delegate(CodeMemberMethod firstMethod, CodeMemberMethod secondMethod) { return firstMethod.Name.CompareTo(secondMethod.Name); });
			foreach (CodeMemberMethod codeMemberMethod in methodList)
				this.Members.Add(codeMemberMethod);

			//        }

		}

	}

}
