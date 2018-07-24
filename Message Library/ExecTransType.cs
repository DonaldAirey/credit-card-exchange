namespace FluidTrade.Core
{

	using System;

	/// <summary>
	/// Summary description for ExecTransType.
	/// </summary>
	[Serializable()]
	public enum ExecTransType
	{
		New = 0,
		Cancel = 1,
		Correct = 2,
		Status = 3
	}

}
