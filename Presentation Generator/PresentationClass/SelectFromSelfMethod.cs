namespace FluidTrade.PresentationGenerator.PresentationClass
{

    using System.CodeDom;
    using FluidTrade.Core;

	/// <summary>
	/// Generates the method that is used in queries where no transformation is to take place.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	class SelectFromSelfMethod : CodeMemberMethod
	{

		/// <summary>
		/// Generates the method that is used in queries where no transformation is to take place.
		/// </summary>
		public SelectFromSelfMethod(ClassSchema classSchema)
		{

			//		/// <summary>
			//		/// Selects a record from itself.
			//		/// </summary>
			//		private static FluidTrade.Sandbox.WorkingOrderHeader.WorkingOrderHeader SelectFromSelf(FluidTrade.Sandbox.WorkingOrderHeader.WorkingOrderHeader workingOrderHeader)
			//		{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("Selects a record from itself.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Attributes = MemberAttributes.Private | MemberAttributes.Static;
			this.ReturnType = new CodeTypeReference(classSchema.Type);
			this.Name = "SelectFromSelf";
			this.Parameters.Add(new CodeParameterDeclarationExpression(this.ReturnType, CommonConversion.ToCamelCase(classSchema.Name)));

			//			return workingOrderHeader;
			this.Statements.Add(new CodeMethodReturnStatement(new CodeArgumentReferenceExpression(CommonConversion.ToCamelCase(classSchema.Name))));

			//		}

		}

	}

}
