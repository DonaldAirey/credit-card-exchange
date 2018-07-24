namespace FluidTrade.Core.TableClass
{

    using System.CodeDom;

	/// <summary>
	/// Creates a field that holds an index to a table.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public class DataIndexField : CodeMemberField
	{

		/// <summary>
		/// Creates a field that holds an index to a table.
		/// </summary>
		/// <param name="uniqueConstraintSchema">The description of a unique constraint.</param>
		public DataIndexField(UniqueConstraintSchema uniqueConstraintSchema)
		{

			//            // The DepartmentKey Index
			//            private UnitTest.Server.DataModel.DepartmentKeyIndex indexDepartmentKey;
			this.Type = new CodeGlobalTypeReference(typeof(DataIndex));
			this.Name = string.Format("index{0}", uniqueConstraintSchema.Name);

		}

	}

}
