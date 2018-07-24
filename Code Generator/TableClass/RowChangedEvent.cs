namespace FluidTrade.Core.TableClass
{

    using System.CodeDom;

	/// <summary>
	/// Creates an event that notifies listeners to a successful row change.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public class TableRowChangedEvent : CodeMemberEvent
	{

		/// <summary>
		/// Creates an event that notifies listeners to a successful row change.
		/// </summary>
		/// <param name="tableSchema">The description of a table.</param>
		public TableRowChangedEvent(TableSchema tableSchema)
		{

			//            /// <summary>
			//            /// Occurs after a Department row has been changed successfully.
			//            /// </summary>
			//            public event FluidTrade.UnitTest.Server.DataModel.DepartmentRowChangeEventHandler DepartmentRowChanged;
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Occurs after a {0} row has been changed successfully.", tableSchema.Name), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Attributes = MemberAttributes.Public;
			this.Type = new CodeTypeReference(string.Format("{0}RowChangeEventHandler", tableSchema.Name));
			this.Name = string.Format("{0}RowChanged", tableSchema.Name);

		}

	}

}
