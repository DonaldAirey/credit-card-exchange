namespace FluidTrade.MiddleTierGenerator.TableClass
{

    using System;
    using System.CodeDom;
    using FluidTrade.Core;

    /// <summary>
	/// Creates a field that holds the ordinal (absolute index) of the table in the DataSet.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	class OrdinalField : CodeMemberField
	{

		/// <summary>
		/// Creates a field that holds the ordinal (absolute index) of the table in the DataSet.
		/// </summary>
		public OrdinalField(TableSchema tableSchema)
		{

			//		private int ordinal;
			this.Type = new CodeGlobalTypeReference(typeof(Int32));
			this.Name = "ordinal";

		}

	}

}
