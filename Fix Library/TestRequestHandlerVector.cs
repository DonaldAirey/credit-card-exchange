namespace FluidTrade.Core
{

	using System;
	using System.Collections;

	/// <summary>
	/// This object is used to quickly find a message handler based on the message type.
	/// </summary>
	public class TestRequestHandlerVector
	{

		private System.Collections.Hashtable hashtable;

		public TestRequestHandlerVector()
		{

			// Initialize the object.
			this.hashtable = new Hashtable();

		}

		/// <summary>
		/// Indexer to find a message handling method based on the message type.
		/// </summary>
		public EventHandler this[string key]
		{

			get {return (EventHandler)this.hashtable[key];}

			set {if (this.hashtable.ContainsKey(key)) this.hashtable[key] = value; else this.hashtable.Add(key, value);}

		}

		public void Remove(string key)
		{

			this.hashtable.Remove(key);

		}

	}

}
