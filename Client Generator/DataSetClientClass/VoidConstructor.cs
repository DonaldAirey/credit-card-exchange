namespace FluidTrade.ClientGenerator.DataSetClientClass
{

    using System.CodeDom;
    using FluidTrade.Core;

    class VoidConstructor : CodeConstructor
	{

		public VoidConstructor(DataModelSchema dataModelSchema)
		{

			//		public DataModelClass()
			//		{
			this.Attributes = MemberAttributes.Public;

			//		}
			
		}

	}

}
