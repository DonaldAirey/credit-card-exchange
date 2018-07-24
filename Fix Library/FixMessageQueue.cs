namespace FluidTrade.Core
{


    /// <summary>
	/// A serialized pipeline for orders.
	/// </summary>
	public class FixMessageQueue : WaitQueue
	{

		public void Enqueue(FixMessage fixMessage) {base.Enqueue(fixMessage);}

		public new FixMessage Dequeue() {return (FixMessage)base.Dequeue();}

		private new void Enqueue(object objectType) {}
	
	}

}
