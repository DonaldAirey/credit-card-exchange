namespace FluidTrade.Core.TableClass
{

    using System.CodeDom;

	/// <summary>
	/// Creates an event that notifies listeners to a successful row change.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public class TableRowDeletingEvent : CodeMemberEvent
	{

		/// <summary>
		/// Creates an event that notifies listeners to a successful row change.
		/// </summary>
		/// <param name="tableSchema">The description of a table.</param>
		public TableRowDeletingEvent(TableSchema tableSchema)
		{

			//            /// <summary>
			//            /// Occurs after a Department row in the table has been deleted.
			//            /// </summary>
			//            public event FluidTrade.UnitTest.Server.DataModel.DepartmentRowChangeEventHandler DepartmentRowDeleting;
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Occurs before a {0} row in the table is about to be deleted.", tableSchema.Name), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Attributes = MemberAttributes.Public;
			this.Name = string.Format("{0}RowDeleting", tableSchema.Name);
			this.Type = new CodeTypeReference(string.Format("{0}RowChangeEventHandler", tableSchema.Name));

		}

	}

}
