namespace FluidTrade.MiddleTierGenerator.TableClass
{

    using System;
    using System.CodeDom;
    using FluidTrade.Core;

    /// <summary>
	/// Creates a propert that gets the abolute index of the table in the DataSet.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	class OrdinalProperty : CodeMemberProperty
	{

		/// <summary>
		/// Creates a propert that gets the abolute index of the table in the DataSet.
		/// </summary>
		/// <param name="tableSchema">A description of the table.</param>
		public OrdinalProperty(TableSchema tableSchema)
		{

			//		/// <summary>
			//		/// Gets the absolute index of the Account table in the DataSet.
			//		/// </summary>
			//		[global::System.ComponentModel.BrowsableAttribute(false)]
			//		public int Ordinal
			//		{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Gets the absolute index of the {0} table in the DataSet.", tableSchema.Name), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.CustomAttributes.AddRange(new CodeCustomAttributesForProperties());
			this.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			this.Type = new CodeGlobalTypeReference(typeof(Int32));
			this.Name = "Ordinal";

			//			get
			//			{
			//				return this.ordinal;
			//			}
			this.GetStatements.Add(new CodeMethodReturnStatement(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "ordinal")));

			//			set
			//			{
			//				this.ordinal = value;
			//			}
			this.SetStatements.Add(
				new CodeAssignStatement(
					new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "ordinal"),
					new CodeArgumentReferenceExpression("value")));

			//		}

		}

	}

}
