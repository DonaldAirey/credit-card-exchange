namespace FluidTrade.ClientGenerator.DataSetClientClass
{

    using System;
    using System.CodeDom;
    using FluidTrade.Core;

    class StringConstructor : CodeConstructor
	{

		public StringConstructor(DataModelSchema dataModelSchema)
		{

			//		public DataModelClass()
			//		{
			this.Attributes = MemberAttributes.Public;
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(String)), "endpointConfigurationName"));
			this.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression("endpointConfigurationName"));

			//		}
			
		}

	}

}
