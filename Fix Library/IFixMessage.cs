namespace FluidTrade.Core.Server
{

    using System;
    using FluidTrade.Core;

    /// <summary>
	/// This delegate is used to raise events when FIX messages have been transmitted.
	/// </summary>
	public delegate bool FixMessageDelegate(FixMessage fixMessage);

	/// <summary>
	/// </summary>
	public class FixMessageEventWrapper : MarshalByRefObject
	{

		public event FixMessageDelegate fixMessageDelegate;

		public bool FixMessageEventHandler(FixMessage fixMessage)
		{
			return (bool)this.fixMessageDelegate(fixMessage);
		}

	}

	/// <summary>
	/// Common interface used for remoting to a Fix Gateway.
	/// </summary>
	public interface IFixMessage
	{

		void Send(FixMessage fixMessage);
		void Register(string instanceId, FixMessageEventWrapper eventWrapper);
		void Unregister(string instanceId);

	}

}
