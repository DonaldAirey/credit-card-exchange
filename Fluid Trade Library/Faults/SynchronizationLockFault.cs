namespace FluidTrade.Core
{

	using System;
	using System.Runtime.Serialization;

	/// <summary>
	/// Create information about a failure to have a record locked when data is accessed.
	/// </summary>
	[DataContract]
	public class SynchronizationLockFault : FaultBase
	{

		// Private Instance Fields
		private String tableName;

		/// <summary>
		/// Create information about a failure to have a record locked when data is accessed.
		/// </summary>
		/// <param name="tableName">The name of the table where the fault occurred..</param>
		public SynchronizationLockFault(String tableName)
			: base(string.Concat("TableName: ", tableName), System.Diagnostics.TraceEventType.Error)
		{

			// Initialize the object
			this.tableName = tableName;

		}

		/// <summary>
		/// Gets or sets the message describing the fault.
		/// </summary>
		[DataMember]
		public String TableName
		{
			get { return this.tableName; }
			set { this.tableName = value; }
		}

	}

}
