namespace FluidTrade.Core.TableClass
{

    using System.CodeDom;

	/// <summary>
	/// Creates an event that notifies listeners to a successful row change.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public class TableRowDeletedEvent : CodeMemberEvent
	{

		/// <summary>
		/// Creates an event that notifies listeners to a successful row change.
		/// </summary>
		/// <param name="tableSchema">The description of a table.</param>
		public TableRowDeletedEvent(TableSchema tableSchema)
		{

			//            /// <summary>
			//            /// Occurs before a Department row in the table is about to be deleted.
			//            /// </summary>
			//            public event FluidTrade.UnitTest.Server.DataModel.DepartmentRowChangeEventHandler DepartmentRowDeleted;
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Occurs after a {0} row in the table has been deleted.", tableSchema.Name), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Attributes = MemberAttributes.Public;
			this.Name = string.Format("{0}RowDeleted", tableSchema.Name);
			this.Type = new CodeTypeReference(string.Format("{0}RowChangeEventHandler", tableSchema.Name));

		}

	}

}
