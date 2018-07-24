namespace FluidTrade.Core
{


    public class ReadRequest : LockRequest
	{

		public ReadRequest(TableSchema tableSchema) : base(tableSchema) { }

	}

}
