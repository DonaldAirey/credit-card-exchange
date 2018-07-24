namespace FluidTrade.MiddleTierGenerator.TargetClass
{

    using System;
    using System.CodeDom;
    using FluidTrade.Core;

    /// <summary>
	/// Generates a property that gets or sets a delegate for getting a context for a filtered read operation.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	class GetFilterContextHandlerProperty : CodeMemberProperty
	{

		/// <summary>
		/// Generates a property that gets or sets a delegate for getting a context for a filtered read.
		/// </summary>
		/// <param name="tableSchema">A description of the data model.</param>
		public GetFilterContextHandlerProperty(DataModelSchema dataModelSchema)
		{

			//            /// <summary>
			//            /// Gets or sets a delegate used to get a context for a filtered read operation.
			//            /// </summary>
			//            public ObjectDelegate GetFilterContextHandler {
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("Gets or sets a delegate used to get a context for a filtered read operation.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.CustomAttributes.AddRange(new CodeCustomAttributesForProperties());
			this.Attributes = MemberAttributes.Public | MemberAttributes.Static;
			this.Type = new CodeTypeReference("FilterContextDelegate");
			this.Name = "GetFilterContextHandler";

			//                get {
			//                    return this.getFilterContextHandler;
			//                }
			//                set {
			//                    this.getFilterContextHandler = value;
			//                }
			this.GetStatements.Add(
				new CodeMethodReturnStatement(
					new CodePropertyReferenceExpression(
						new CodeFieldReferenceExpression(
							new CodeTypeReferenceExpression(dataModelSchema.Name),
							String.Format("{0}DataSet", CommonConversion.ToCamelCase(dataModelSchema.Name))),
						"GetFilterContextHandler")));
			this.SetStatements.Add(
				new CodeAssignStatement(
					new CodePropertyReferenceExpression(
						new CodeFieldReferenceExpression(
							new CodeTypeReferenceExpression(dataModelSchema.Name),
							String.Format("{0}DataSet", CommonConversion.ToCamelCase(dataModelSchema.Name))),
						"GetFilterContextHandler"),
					new CodeVariableReferenceExpression("value")));

			//            }

		}

	}

}
