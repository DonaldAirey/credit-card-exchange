namespace FluidTrade.Core
{

	using System;
	using System.Runtime.Serialization;

	/// <summary>
	/// Contains information about a failure to find a record.
	/// </summary>
	[DataContract]
	public class IndexNotFoundFault: FaultBase
	{

		// Private Instance Fields
		private String indexName;
		private String tableName;

		/// <summary>
		/// Create information about a failure to find a record.
		/// </summary>
		/// <param name="format">The format for the error message.</param>
		/// <param name="args">Variable list of parameters for the failure message.</param>
		public IndexNotFoundFault(String tableName, String indexName)
			: base(GetMessage(tableName, indexName), System.Diagnostics.TraceEventType.Error)
		{

			// Initialize the object.
			this.tableName = tableName;
			this.indexName = indexName;

		}

		/// <summary>
		/// get message text that is use to pass to base class ctor
		/// </summary>
		/// <param name="tableName"></param>
		/// <param name="indexName"></param>
		/// <returns></returns>
		private static String GetMessage(String tableName, String indexName)
		{
			return string.Format("TableName:{0} IndexName:{1}", tableName, indexName);
		}

		/// <summary>
		/// Gets or sets the name of the table where the fault occurred.
		/// </summary>
		[DataMember]
		public String IndexName
		{
			get { return this.indexName; }
			set { this.indexName = value; }
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
