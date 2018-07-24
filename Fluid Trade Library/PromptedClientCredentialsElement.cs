namespace FluidTrade.Core
{

	using System;
	using System.ServiceModel.Configuration;

	/// <summary>
	/// Represents a configuration element that configures a specialized set of client credentials having a user prompt.
	/// </summary>
	public class PromptedClientCredentialsElement : ClientCredentialsElement
	{

		/// <summary>
		/// Gets the type of this Behavior element.
		/// </summary>
		public override Type BehaviorType
		{
			get { return typeof(PromptedClientCredentials); }
		}

		/// <summary>
		/// Creates a custom behavior based on the settings of this configuration element.
		/// </summary>
		/// <returns></returns>
		protected override object CreateBehavior()
		{

			// Create and apply the prompted client credentials as part of the behavior of the endpoint.  Once this is applied, the
			// user will be prompted with a custom user interface when the channel is initialized.
			PromptedClientCredentials customClientCredentials = new PromptedClientCredentials();
			base.ApplyConfiguration(customClientCredentials);
			return customClientCredentials;

		}

	}

}
