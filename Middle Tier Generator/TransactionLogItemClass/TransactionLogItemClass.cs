namespace FluidTrade.MiddleTierGenerator.TransactionLogItemClass
{

    using System.CodeDom;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;
    using FluidTrade.Core;

    /// <summary>
	/// Creates a CodeDOM description of a class used to collect fields during the transaction log compression.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	class TransactionLogItemClass : CodeTypeDeclaration
	{

		/// <summary>
		/// Creates a CodeDOM description of a class used to collect fields during the transaction log compression.
		/// </summary>
		/// <param name="tableSchema">The schema that describes the table.</param>
		public TransactionLogItemClass(DataModelSchema dataModelSchema)
		{

			//	/// <summary>
			//	/// An item in the transaction log.
			//	/// </summary>
			//	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("FluidTrade.Core.MiddleTier.MiddleTierGenerator", "1.3.0.0")]
			//	[global::System.ComponentModel.DesignerCategoryAttribute("code")]
			//	public class TransactionLogItem
			//	{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("An item in the transaction log.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.CustomAttributes.Add(new CodeGeneratedCodeAttribute(typeof(MiddleTierGenerator)));
			this.CustomAttributes.Add(
				new CodeAttributeDeclaration(
					new CodeGlobalTypeReference(typeof(DesignerCategoryAttribute)),
					new CodeAttributeArgument(new CodePrimitiveExpression("code"))));
			this.Name = "TransactionLogItem";
			this.TypeAttributes = TypeAttributes.Public | TypeAttributes.Class;

			// Private Instance Fields
			List<CodeMemberField> fieldList = new List<CodeMemberField>();
			fieldList.Add(new ContainerContextField());
			fieldList.Add(new DataField());
			fieldList.Add(new KeyLengthField());
			fieldList.Add(new SequenceField());
			fieldList.Add(new TimeStampField());
			fieldList.Sort(delegate(CodeMemberField firstField, CodeMemberField secondField) { return firstField.Name.CompareTo(secondField.Name); });
			foreach (CodeMemberField codeMemberField in fieldList)
				this.Members.Add(codeMemberField);

			// Constructors
			this.Members.Add(new Constructor(dataModelSchema));

			// Methods
			List<CodeMemberMethod> methodList = new List<CodeMemberMethod>();
			methodList.Add(new EqualsMethod(dataModelSchema));
			methodList.Add(new GetHashCodeMethod(dataModelSchema));
			methodList.Sort(delegate(CodeMemberMethod firstMethod, CodeMemberMethod secondMethod) { return firstMethod.Name.CompareTo(secondMethod.Name); });
			foreach (CodeMemberMethod codeMemberMethod in methodList)
				this.Members.Add(codeMemberMethod);

			//        }

		}

	}

}
