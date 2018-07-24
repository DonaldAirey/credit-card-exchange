namespace FluidTrade.Core.TableClass
{

    using System.CodeDom;

	/// <summary>
	/// Creates an event that notifies listeners to a successful row change.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public class TableRowChangingEvent : CodeMemberEvent
	{

		/// <summary>
		/// Creates an event that notifies listeners to a successful row change.
		/// </summary>
		/// <param name="tableSchema">The description of a table.</param>
		public TableRowChangingEvent(TableSchema tableSchema)
		{

			//            /// <summary>
			//            /// Occurs when a Department row is changing.
			//            /// </summary>
			//            public event FluidTrade.UnitTest.Server.DataModel.DepartmentRowChangeEventHandler DepartmentRowChanging;
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Occurs when a {0} row is changing.", tableSchema.Name), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Attributes = MemberAttributes.Public;
			this.Name = string.Format("{0}RowChanging", tableSchema.Name);
			this.Type = new CodeTypeReference(string.Format("{0}RowChangeEventHandler", tableSchema.Name));

		}

	}

}
