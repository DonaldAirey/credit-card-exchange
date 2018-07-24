namespace FluidTrade.MiddleTierGenerator.DataSetClass
{

    using System.CodeDom;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Reflection;
    using FluidTrade.Core;

    /// <summary>
	/// Creates a CodeDOM description of a strongly typed Table.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	class DataSetClass : CodeTypeDeclaration
	{

		/// <summary>
		/// Create a CodeDOM description of a strongly typed Table.
		/// </summary>
		/// <param name="tableSchema">The schema that describes the table.</param>
		public DataSetClass(DataModelSchema dataModelSchema)
		{

			//	/// <summary>
			//	/// A thread-safe DataSet able to handle transactions.
			//	/// </summary>
			//	public class DataModelDataSet : global::System.Data.DataSet
			//	{
			//
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("A thread-safe DataSet able to handle transactions.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.CustomAttributes.Add(new CodeGeneratedCodeAttribute(typeof(MiddleTierGenerator)));
			this.CustomAttributes.Add(
				new CodeAttributeDeclaration(
					new CodeGlobalTypeReference(typeof(DesignerCategoryAttribute)),
					new CodeAttributeArgument(new CodePrimitiveExpression("code"))));
			this.Name = string.Format("{0}DataSet", dataModelSchema.Name);
			this.TypeAttributes = TypeAttributes.NestedAssembly | TypeAttributes.Class;
			this.BaseTypes.Add(new CodeGlobalTypeReference(typeof(DataSet)));

			// Private Instance Fields
			List<CodeMemberField> privateInstanceFieldList = new List<CodeMemberField>();
			privateInstanceFieldList.Add(new CompressorThreadField());
			privateInstanceFieldList.Add(new DataLockField());
			privateInstanceFieldList.Add(new GetFilterContextHandlerField());
			privateInstanceFieldList.Add(new IdentifierField());
			privateInstanceFieldList.Add(new MasterRowVersionField());
			privateInstanceFieldList.Add(new ReadBufferSizeField());
			privateInstanceFieldList.Add(new SequenceField());
			privateInstanceFieldList.Add(new TransactionLogField());
			privateInstanceFieldList.Add(new TransactionLogCompressionSizeField());
			privateInstanceFieldList.Add(new TransactionLogCompressionTimeField());
			privateInstanceFieldList.Add(new TransactionLogAgeField());
			privateInstanceFieldList.Add(new TransactionLogLockField());
			privateInstanceFieldList.Sort(delegate(CodeMemberField firstField, CodeMemberField secondField) { return firstField.Name.CompareTo(secondField.Name); });
			foreach (CodeMemberField codeMemberField in privateInstanceFieldList)
				this.Members.Add(codeMemberField);

			// Constructors
			this.Members.Add(new VoidConstructor(dataModelSchema));

			// Properties
			List<CodeMemberProperty> propertyList = new List<CodeMemberProperty>();
			propertyList.Add(new DataLockProperty());
			propertyList.Add(new GetFilterContextHandlerProperty());
			propertyList.Add(new TransactionLogLockProperty());
			propertyList.Sort(delegate(CodeMemberProperty firstProperty, CodeMemberProperty secondProperty) { return firstProperty.Name.CompareTo(secondProperty.Name); });
			foreach (CodeMemberProperty codeMemberProperty in propertyList)
				this.Members.Add(codeMemberProperty);

			// Methods
			List<CodeMemberMethod> methodList = new List<CodeMemberMethod>();
			methodList.Add(new AddTransactionMethod(dataModelSchema));
			methodList.Add(new CompressLogMethod(dataModelSchema));
			methodList.Add(new GetFilterContextMethod(dataModelSchema));
			methodList.Add(new IncrementRowVersionMethod(dataModelSchema));
			methodList.Add(new LoadDataMethod(dataModelSchema));
			methodList.Add(new ReadMethod(dataModelSchema));
			methodList.Add(new ReconcileMethod(dataModelSchema));
			methodList.Add(new SequenceRecordMethod(dataModelSchema));
			methodList.Sort(delegate(CodeMemberMethod firstMethod, CodeMemberMethod secondMethod) { return firstMethod.Name.CompareTo(secondMethod.Name); });
			foreach (CodeMemberMethod codeMemberMethod in methodList)
				this.Members.Add(codeMemberMethod);

			//        }

			this.IsPartial = true;
		}

	}

}
