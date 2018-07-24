namespace FluidTrade.Core
{


    /// <summary>
	/// This object is used to quickly find a message handler based on the message type.
	/// </summary>
	public class FixMessageHandlerVector : System.Collections.Hashtable
	{

		private FixMessageHandler defaultFixMessageHandler;

		/// <summary>The Default Message Handler</summary>
		public FixMessageHandler DefaultMessageHandler
		{
			get {return this.defaultFixMessageHandler;}
			set {this.defaultFixMessageHandler = value;}
		}

		/// <summary>
		/// Adds a message type and message handler into the table.
		/// </summary>
		/// <param name="key">The message type.</param>
		/// <param name="messageHandler">A vector for handling messages.</param>
		public void Add(MsgType key, FixMessageHandler fixMessageHandler)
		{

			// Call the base class to add the pair.
			base.Add(key, fixMessageHandler);

		}

		/// <summary>
		/// Indexer to find a message handling method based on the message type.
		/// </summary>
		public FixMessageHandler this[MsgType key]
		{

			get
			{
				FixMessageHandler fixMessageHandler = (FixMessageHandler)base[key];
				return fixMessageHandler == null ? this.defaultFixMessageHandler : fixMessageHandler;
			}

			set {if (this.ContainsKey(key)) base[key] = value; else Add(key, value);}

		}
		
		/// <summary>
		/// Hides the generic version of 'Add'.
		/// </summary>
		private new void Add(object key, object value) {}

	}

}
