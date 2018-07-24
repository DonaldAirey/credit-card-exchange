namespace FluidTrade.Core
{

	using System;
	using System.Runtime.Serialization;
	using System.Text;

	/// <summary>
	/// Contains information about a failure to have the proper row version when accessing a record.
	/// </summary>
	[DataContract]
	public class OptimisticConcurrencyFault : FaultBase
	{

		// Private Instance Fields
		private System.Object[] key;
		private String tableName;

		/// <summary>
		/// Create information about a failure to have the proper row version when accessing a record.
		/// </summary>
		/// <param name="format">The format for the error message.</param>
		/// <param name="args">Variable list of parameters for the failure message.</param>
		public OptimisticConcurrencyFault(String tableName, Object[] key)
			: base(GetMessage(tableName, key), System.Diagnostics.TraceEventType.Error)
		{

			// Initialize the object
			this.tableName = tableName;
			this.key = key;

		}

		/// <summary>
		/// get message text that is use to pass to base class ctor
		/// </summary>
		/// <param name="tableName"></param>
		/// <param name="indexName"></param>
		/// <returns></returns>
		private static String GetMessage(String tableName, Object[] key)
		{
			StringBuilder keyStringBuilder = new StringBuilder();
			if(key != null)
				foreach(object keyVal in key)
				{
					if(keyVal == null)
						continue;
					if(keyStringBuilder.Length != 0)
						keyStringBuilder.Append(";");

					keyStringBuilder.Append(keyVal);
				}

			return string.Format("TableName:{0} Key:{1}", tableName, keyStringBuilder.ToString());
		}


		/// <summary>
		/// Gets or sets the key that caused the fault.
		/// </summary>
		[DataMember]
		public Object[] Key
		{
			get { return this.key; }
			set { this.key = value; }
		}

		/// <summary>
		/// Gets or sets the name of the table where the fault occurred.
		/// </summary>
		[DataMember]
		public String TableName
		{
			get { return this.tableName; }
			set { this.tableName = value; }
		}

	}

}
