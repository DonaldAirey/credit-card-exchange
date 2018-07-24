namespace FluidTrade.ClientGenerator.DataSetClientClass
{

    using System.CodeDom;
    using System.Collections.Generic;
    using System.Reflection;
    using FluidTrade.Core;

    /// <summary>
	/// Generates the CodeDOM for a strongly typed DataSet from a schema description.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public class DataSetClientClass : CodeTypeDeclaration
	{

		/// <summary>
		/// Creates a strongly typed DataSet from a schema description.
		/// </summary>
		/// <param name="dataSetNamespace">The CodeDOM namespace FluidTrade.ClientGenerator.DataSetClientClass contains this strongly typed DataSet.</param>
		public DataSetClientClass(DataModelSchema dataModelSchema)
		{

			//	[System.Diagnostics.DebuggerStepThroughAttribute()]
			//	public partial class DataModelClient : System.ServiceModel.ClientBase<IDataModel>, IDataModel
			//	{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Client interface to a shared {0}.", dataModelSchema.Name), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.IsPartial = true;
			this.TypeAttributes = TypeAttributes.Public | TypeAttributes.Class;
			this.Name = string.Format("{0}Client", dataModelSchema.Name);
			this.BaseTypes.Add(new CodeTypeReference(string.Format("global::System.ServiceModel.ClientBase<I{0}>", dataModelSchema.Name)));
			this.BaseTypes.Add(new CodeTypeReference(string.Format("I{0}", dataModelSchema.Name)));

			// Constructors
			this.Members.Add(new VoidConstructor(dataModelSchema));
			this.Members.Add(new StringConstructor(dataModelSchema));
	
			// Methods
			List<CodeMemberMethod> methodList = new List<CodeMemberMethod>();
			methodList.Add(new ReadMethod(dataModelSchema));
			methodList.Add(new ReconcileMethod(dataModelSchema));
			foreach (KeyValuePair<string, TableSchema> tablePair in dataModelSchema.Tables)
			{
				methodList.Add(new CreateMethod(tablePair.Value));
				methodList.Add(new UpdateMethod(tablePair.Value));
				methodList.Add(new DestroyMethod(tablePair.Value));
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
