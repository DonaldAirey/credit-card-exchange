namespace FluidTrade.Core.TargetInterface
{

    using System.CodeDom;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
	/// Generates the CodeDOM for a strongly typed DataSet from a schema description.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public class TargetInterface : CodeTypeDeclaration
	{

		/// <summary>
		/// Creates a strongly typed DataSet from a schema description.
		/// </summary>
		/// <param name="dataSetNamespace">The CodeDOM namespace FluidTrade.Core.TargetInterface contains this strongly typed DataSet.</param>
		public TargetInterface(DataModelSchema dataModelSchema)
		{

			//	/// <summary>
			//	/// Abstract interface to a thread-safe, multi-tiered DataModel.
			//	/// </summary>
			//	[System.ServiceModel.ServiceContractAttribute()]
			//	public interface IDataModel
			//	{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Abstract interface to a thread-safe, multi-tiered {0}.", dataModelSchema.Name), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeGlobalTypeReference(typeof(System.ServiceModel.ServiceContractAttribute)), new CodeAttributeArgument("ConfigurationName", new CodePrimitiveExpression(string.Format("I{0}", dataModelSchema.Name)))));
			this.Name = string.Format("I{0}", dataModelSchema.Name);
			this.TypeAttributes = TypeAttributes.Public | TypeAttributes.Interface;
			this.IsPartial = true;

			// Methods
			List<CodeMemberMethod> methodList = new List<CodeMemberMethod>();
			methodList.Add(new ReadMethod(dataModelSchema));
			methodList.Add(new ReconcileMethod(dataModelSchema));
			foreach (KeyValuePair<string, TableSchema> tablePair in dataModelSchema.Tables)
			{
				methodList.Add(new CreateMethod(tablePair.Value));
				methodList.Add(new DestroyMethod(tablePair.Value));
				methodList.Add(new UpdateMethod(tablePair.Value));
				if (dataModelSchema.Tables.ContainsKey("Configuration") && tablePair.Value.IsExternal)
				{
					methodList.Add(new CreateExMethod(tablePair.Value));
					methodList.Add(new DestroyExMethod(tablePair.Value));
					methodList.Add(new UpdateExMethod(tablePair.Value));
				}
			}
			methodList.Sort(delegate(CodeMemberMethod firstMethod, CodeMemberMethod secondMethod) { return firstMethod.Name.CompareTo(secondMethod.Name); });
			foreach (CodeMemberMethod codeMemberMethod in methodList)
				this.Members.Add(codeMemberMethod);

		}

	}

}
