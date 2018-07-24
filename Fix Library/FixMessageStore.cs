namespace FluidTrade.Core
{


    /// <summary>
	/// Summary description for FixMessageStore.
	/// </summary>
	public class FixMessageStore : MessageStore
	{

		public new FixMessage this[int recordIndex]
		{
			
			get {Message message = base[recordIndex]; return message == null ? null : new FixMessage(message);}
			set {base[recordIndex] = value;}}

	}

}
