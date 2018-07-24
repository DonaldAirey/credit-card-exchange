namespace FluidTrade.MiddleTierGenerator.TargetClass
{

    using System.CodeDom;
    using System.Collections.Generic;
    using System.Reflection;
    using FluidTrade.Core;
    using FluidTrade.Core.TargetClass;

    /// <summary>
	/// Generates the CodeDOM for a strongly typed DataSet from a schema description.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public class TargetClass : CodeTypeDeclaration
	{

		/// <summary>
		/// Creates a strongly typed DataSet from a schema description.
		/// </summary>
		/// <param name="dataSetNamespace">The CodeDOM namespace FluidTrade.MiddleTierGenerator.TargetClass
		public TargetClass(DataModelSchema dataModelSchema)
		{

			//    /// <summary>
			//    /// A thread-safe, multi-tiered DataModel.
			//    /// </summary>
			//    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("FluidTrade.Core.MiddleTier.MiddleTierGenerator", "1.3.0.0")]
			//    [global::System.ComponentModel.DesignerCategoryAttribute("code")]
			//    [global::System.ComponentModel.ToolboxItemAttribute(true)]
			//    public class DataModel : FluidTrade.UnitTest.Server.IDataModel {
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("A thread-safe, multi-tiered {0}.", dataModelSchema.Name), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.CustomAttributes.Add(new CodeGeneratedCodeAttribute(typeof(MiddleTierGenerator)));
			this.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeGlobalTypeReference(typeof(System.ComponentModel.DesignerCategoryAttribute)), new CodeAttributeArgument(new CodePrimitiveExpression("code"))));
			this.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeGlobalTypeReference(typeof(System.ComponentModel.ToolboxItemAttribute)), new CodeAttributeArgument(new CodePrimitiveExpression(true))));
			this.Name = dataModelSchema.Name;
			this.TypeAttributes = TypeAttributes.Public | TypeAttributes.Class;
			this.BaseTypes.Add(new CodeTypeReference(string.Format("I{0}", dataModelSchema.Name)));
			this.IsPartial = true;
			//	Private Instance Fields
			List<CodeMemberField> privateInstanceFieldList = new List<CodeMemberField>();
			privateInstanceFieldList.Add(new CompactTimeField());
			privateInstanceFieldList.Add(new DataSetField(dataModelSchema));
			privateInstanceFieldList.Add(new LockTimeoutField());
			privateInstanceFieldList.Add(new ThreadWaitTimeField());
			foreach (KeyValuePair<string, TableSchema> keyValuePair in dataModelSchema.Tables)
				privateInstanceFieldList.Add(new TableField(keyValuePair.Value));
			foreach (KeyValuePair<string, RelationSchema> relationPair in dataModelSchema.Relations)
				privateInstanceFieldList.Add(new RelationField(relationPair.Value));
			privateInstanceFieldList.Sort((first, second) => { return first.Name.CompareTo(second.Name); });
			foreach (CodeMemberField codeMemberField in privateInstanceFieldList)
				this.Members.Add(codeMemberField);

			// Constructors
			this.Members.Add(new StaticConstructor(dataModelSchema));

			// Properties
			List<CodeMemberProperty> propertyList = new List<CodeMemberProperty>();
			propertyList.Add(new GetFilterContextHandlerProperty(dataModelSchema));
			propertyList.Add(new LockTimeoutProperty(dataModelSchema));
			propertyList.Add(new DataLockProperty(dataModelSchema));
			propertyList.Add(new RelationsProperty(dataModelSchema));
			propertyList.Add(new TransactionLogLockProperty(dataModelSchema));
			foreach (KeyValuePair<string, TableSchema> keyValuePair in dataModelSchema.Tables)
				propertyList.Add(new TableProperty(keyValuePair.Value));
			propertyList.Add(new TablesProperty(dataModelSchema));
			propertyList.Sort((first, second) => { return first.Name.CompareTo(second.Name); });
			foreach (CodeMemberProperty codeMemberProperty in propertyList)
				this.Members.Add(codeMemberProperty);

			// Methods
			List<CodeMemberMethod> methodList = new List<CodeMemberMethod>();
			methodList.Add(new AddTransactionMethod(dataModelSchema));
			methodList.Add(new ReadMethod(dataModelSchema));
			methodList.Add(new ReadXmlMethod(dataModelSchema));
			methodList.Add(new ReconcileMethod(dataModelSchema));
			foreach (KeyValuePair<string, TableSchema> tablePair in dataModelSchema.Tables)
			{
				methodList.Add(new CreateMethod(tablePair.Value));
				methodList.Add(new DestroyMethod(tablePair.Value));
				methodList.Add(new UpdateMethod(tablePair.Value));
				if (dataModelSchema.Tables.ContainsKey("Configuration") && tablePair.Value.IsExternal)
				{
					methodList.Add(new CreateExMethod(tablePair.Value));
					methodList.Add(new UpdateExMethod(tablePair.Value));
					methodList.Add(new DestroyExMethod(tablePair.Value));
				}
			}
			methodList.Sort((first, second) => { return first.Name.CompareTo(second.Name); });
			foreach (CodeMemberMethod codeMemberMethod in methodList)
				this.Members.Add(codeMemberMethod);

		}

	}

}
