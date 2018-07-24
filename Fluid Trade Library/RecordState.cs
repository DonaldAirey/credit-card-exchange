namespace FluidTrade.Core
{


    /// <summary>
	/// The state of rows found in a reconcilliation.
	/// </summary>
	public class RecordState
	{
		public const int Added = 0;
		public const int Deleted = 1;
		public const int Modified = 2;
	}

}
