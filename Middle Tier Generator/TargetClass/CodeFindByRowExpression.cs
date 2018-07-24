namespace FluidTrade.MiddleTierGenerator.TargetClass
{

    using System.CodeDom;
    using FluidTrade.Core;

	/// <summary>
	/// Creates an expression to find a record based on the primary index of a table.
	/// </summary>
	class CodeFindByRowExpression : CodeCastExpression
	{

		/// <summary>
		/// Creates an expression to find a record based on the primary index of a table.
		/// </summary>
		/// <param name="tableSchema"></param>
		public CodeFindByRowExpression(TableSchema tableSchema, CodeExpression keyExpression)
		{

			//			((DestinationRow)(DataModel.Destination.Rows.Find(destinationRowByFK_Destination_DestinationOrderKey)));
			this.TargetType = new CodeTypeReference(string.Format("{0}Row", tableSchema.Name));
			this.Expression = new CodeMethodInvokeExpression(new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(tableSchema.DataModel.Name), tableSchema.Name), "Rows"), "Find", keyExpression);

		}

	}

}
