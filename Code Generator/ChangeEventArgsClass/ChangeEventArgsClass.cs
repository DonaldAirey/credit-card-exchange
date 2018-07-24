namespace FluidTrade.Core.ChangeEventArgsClass
{

    using System.CodeDom;
    using System.Reflection;
    using FluidTrade.Core;

	/// <summary>
	/// Creates a CodeDOM description of a strongly typed changed row event argument.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public class ChangeEventArgsClass : CodeTypeDeclaration
	{

		/// <summary>
		/// Creates a CodeDOM description of a strongly typed changed row event argument.
		/// </summary>
		/// <param name="tableSchema">The table schema that describes the event arguments.</param>
		public ChangeEventArgsClass(TableSchema tableSchema)
		{

			//        /// <summary>
			//        /// Arguments for the event that indicates a change in a Department table row.
			//        /// </summary>
			//        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("FluidTrade.Core.MiddleTier.MiddleTierGenerator", "1.3.0.0")]
			//        public class DepartmentRowChangeEvent : System.EventArgs {
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Arguments for the event that indicates a change in a {0} table row.", tableSchema.Name), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.CustomAttributes.Add(new CodeGeneratedCodeAttribute(tableSchema.DataModel.GeneratorType));
			this.TypeAttributes = TypeAttributes.Public | TypeAttributes.Class;
			this.Name = string.Format("{0}RowChangeEventArgs", tableSchema.Name);
			this.BaseTypes.Add(new CodeGlobalTypeReference(typeof(System.EventArgs)));

			// Private Instance Fields
			this.Members.Add(new RowField(tableSchema));
			this.Members.Add(new DataActionField(tableSchema));

			// Constructors
			this.Members.Add(new Constructor(tableSchema));

			// Properties
			this.Members.Add(new RowProperty(tableSchema));
			this.Members.Add(new DataActionProperty(tableSchema));

			//        }

		}

	}

}
