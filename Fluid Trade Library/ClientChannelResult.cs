namespace FluidTrade.Core
{

	using System;

    /// <summary>
	/// Data for an asynchronous operation involving a WCF channel initialization.
	/// </summary>
	public class ClientChannelResult : IAsyncResult
	{

		// Public Instance Fields
		public System.ServiceModel.IClientChannel IClientChannel;
	
		#region IAsyncResult Members

		public object AsyncState
		{
			get { throw new NotImplementedException(); }
		}

		public System.Threading.WaitHandle AsyncWaitHandle
		{
			get { throw new NotImplementedException(); }
		}

		public bool CompletedSynchronously
		{
			get { return true; }
		}

		public bool IsCompleted
		{
			get { throw new NotImplementedException(); }
		}

		#endregion

	}
}
