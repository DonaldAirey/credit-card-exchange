namespace FluidTrade.Core
{


    public class WriteRequest : LockRequest
	{

		public WriteRequest(TableSchema tableSchema) : base(tableSchema) { }

	}

}
