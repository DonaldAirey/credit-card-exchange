namespace FluidTrade.Guardian
{

    using System;
    using FluidTrade.Core;
    using FluidTrade.Guardian.Records;

	/// <summary>
	/// Standard Interface for a Dynamic Exchange.
	/// </summary>
	public interface IExchange
	{

        /// <summary>
		/// Starts the exchange.
		/// </summary>
		void Start();

        /// <summary>
        /// Stops an exchange.
        /// </summary>
        void Stop();

	}

}
