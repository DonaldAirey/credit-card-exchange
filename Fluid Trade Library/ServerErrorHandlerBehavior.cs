namespace FluidTrade.Core
{
	using System;
	using System.ServiceModel.Configuration;

	public class ServerErrorHandlerBehavior : BehaviorExtensionElement
	{

		public override Type BehaviorType
		{
			get
			{
				return typeof(ServerErrorHandler);
			}

		}

		protected override object CreateBehavior()
		{
			return new ServerErrorHandler();
		}
	}
}
