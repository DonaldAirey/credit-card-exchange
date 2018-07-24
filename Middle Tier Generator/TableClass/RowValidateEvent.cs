namespace FluidTrade.MiddleTierGenerator.TableClass
{

    using System.CodeDom;
    using FluidTrade.Core;

	/// <summary>
	/// Creates an event that notifies listeners to a successful row change.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public class TableRowValidateEvent : CodeMemberEvent
	{

		/// <summary>
		/// Creates an event that notifies listeners when a row needs to be validated.
		/// </summary>
		/// <param name="tableSchema">The description of a table.</param>
		public TableRowValidateEvent(TableSchema tableSchema)
		{

			//            /// <summary>
			//            /// Occurs when a Department row needs to be validated.
			//            /// </summary>
			//            public event FluidTrade.UnitTest.Server.DataModel.DepartmentRowChangeEventHandler DepartmentRowValidate;
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Occurs when a {0} row needs to be validated.", tableSchema.Name), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Attributes = MemberAttributes.Public;
			this.Type = new CodeTypeReference(string.Format("{0}RowChangeEventHandler", tableSchema.Name));
			this.Name = string.Format("{0}RowValidate", tableSchema.Name);

		}

	}

}
