namespace FluidTrade.ClientGenerator.TargetClass
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
		/// <param name="dataSetNamespace">The CodeDOM namespace FluidTrade.ClientGenerator.TargetClass
		public TargetClass(DataModelSchema dataModelSchema)
		{

			//    /// <summary>
			//    /// A thread-safe, multi-tiered DataModel.
			//    /// </summary>
			//    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("FluidTrade.Core.Client.MiddleTierGenerator", "1.3.0.0")]
			//    [global::System.ComponentModel.DesignerCategoryAttribute("code")]
			//    [global::System.ComponentModel.ToolboxItemAttribute(true)]
			//    public class DataModel : FluidTrade.UnitTest.Server.IDataModel {
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("A thread-safe, multi-tiered {0}.", dataModelSchema.Name), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.CustomAttributes.Add(new CodeGeneratedCodeAttribute(typeof(ClientGenerator)));
			this.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeGlobalTypeReference(typeof(System.ComponentModel.DesignerCategoryAttribute)), new CodeAttributeArgument(new CodePrimitiveExpression("code"))));
			this.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeGlobalTypeReference(typeof(System.ComponentModel.ToolboxItemAttribute)), new CodeAttributeArgument(new CodePrimitiveExpression(true))));
			this.Name = dataModelSchema.Name;
			this.TypeAttributes = TypeAttributes.Public | TypeAttributes.Class;

			// Private Constants
			List<CodeMemberField> constantList = new List<CodeMemberField>();
			constantList.Add(new DeletedTimeColumnField());
			constantList.Add(new PrimaryKeyOffsetField());
			constantList.Add(new RefreshIntervalField());
			constantList.Add(new RowDataIndexField());
			constantList.Add(new RowStateIndexField());
			constantList.Add(new RowVersionColumnField());
			constantList.Add(new TableRowsIndexField());
			constantList.Add(new TableTableNameIndexField());
			constantList.Add(new ThreadWaitTimeField());
			constantList.Sort(delegate(CodeMemberField firstField, CodeMemberField secondField) { return firstField.Name.CompareTo(secondField.Name); });
			foreach (CodeMemberField codeMemberField in constantList)
				this.Members.Add(codeMemberField);

			// Private Instance Fields
			List<CodeMemberField> privateInstanceFieldList = new List<CodeMemberField>();
			privateInstanceFieldList.Add(new DataSetField());
			privateInstanceFieldList.Add(new DataSetIdField());
			privateInstanceFieldList.Add(new IsReadingField());
			privateInstanceFieldList.Add(new PurgeBufferSizeField());
			privateInstanceFieldList.Add(new ReadDataModelThreadField());
			privateInstanceFieldList.Add(new SequenceField());
			privateInstanceFieldList.Add(new SyncRootField());
			privateInstanceFieldList.Add(new UpdateBufferMutexField());
			privateInstanceFieldList.Add(new SyncUpdateField());
			foreach (KeyValuePair<string, TableSchema> keyValuePair in dataModelSchema.Tables)
				privateInstanceFieldList.Add(new TableField(keyValuePair.Value));
			foreach (KeyValuePair<string, RelationSchema> relationPair in dataModelSchema.Relations)
				privateInstanceFieldList.Add(new RelationField(relationPair.Value));
			privateInstanceFieldList.Sort(delegate(CodeMemberField firstField, CodeMemberField secondField) { return firstField.Name.CompareTo(secondField.Name); });
			foreach (CodeMemberField codeMemberField in privateInstanceFieldList)
				this.Members.Add(codeMemberField);

			// Public Fields (Work-around for a bug that prevents the generation of static events).
			List<CodeMemberField> publicFieldList = new List<CodeMemberField>();
			publicFieldList.Add(new BeginMergeField());
			publicFieldList.Add(new CommunicationExceptionField());
			publicFieldList.Add(new EndMergeField());
			publicFieldList.Add(new ProgressField());
			publicFieldList.Sort(delegate(CodeMemberField firstField, CodeMemberField secondField) { return firstField.Name.CompareTo(secondField.Name); });
			foreach (CodeMemberField codeMemberField in publicFieldList)
				this.Members.Add(codeMemberField);

			// Constructors
			this.Members.Add(new StaticConstructor(dataModelSchema));

			// Properties
			List<CodeMemberProperty> propertyList = new List<CodeMemberProperty>();
			propertyList.Add(new IsReconcilingProperty(dataModelSchema));
			propertyList.Add(new SyncRootProperty(dataModelSchema));
			propertyList.Add(new RelationsProperty(dataModelSchema));
			propertyList.Add(new TablesProperty(dataModelSchema));
			foreach (KeyValuePair<string, TableSchema> keyValuePair in dataModelSchema.Tables)
				propertyList.Add(new TableProperty(keyValuePair.Value));
			propertyList.Sort(delegate(CodeMemberProperty firstProperty, CodeMemberProperty secondProperty) { return firstProperty.Name.CompareTo(secondProperty.Name); });
			foreach (CodeMemberProperty codeMemberProperty in propertyList)
				this.Members.Add(codeMemberProperty);

			// Methods
			List<CodeMemberMethod> methodList = new List<CodeMemberMethod>();
			methodList.Add(new OnBeginMergeMethod(dataModelSchema));
			methodList.Add(new OnEndMergeMethod(dataModelSchema));
			methodList.Add(new PurgeDataModelMethod(dataModelSchema));
			methodList.Add(new ReadDataModelMethod(dataModelSchema));
			methodList.Add(new ReadMethod(dataModelSchema));
			methodList.Add(new ResetMethod(dataModelSchema));
			methodList.Add(new WaitForUpdateMethod(dataModelSchema));
			methodList.Add(new WriteMethod(dataModelSchema));
			methodList.Sort(delegate(CodeMemberMethod firstMethod, CodeMemberMethod secondMethod) { return firstMethod.Name.CompareTo(secondMethod.Name); });
			foreach (CodeMemberMethod codeMemberMethod in methodList)
				this.Members.Add(codeMemberMethod);
	
		}

	}

}
