namespace FluidTrade.Core
{

    using System.Collections.Generic;

	public class ForeignKeyConstraintParameterItem : ExternalParameterItem
	{

		public ForeignKeyConstraintSchema ForeignKeyConstraintSchema;
		public System.CodeDom.CodeVariableReferenceExpression CodeVariableReferenceExpression;
		public System.Collections.Generic.List<ForeignKeyVariableItem> ForeignKeyVariables;

		public ForeignKeyConstraintParameterItem()
		{

			// Initialize the object
			this.ForeignKeyVariables = new List<ForeignKeyVariableItem>();

		}

	}

}
