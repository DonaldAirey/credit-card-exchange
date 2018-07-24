namespace FluidTrade.Core
{

	using System;

	/// <summary>
	///  SessionRejectReason.
	/// </summary>
	[Serializable()]
	public enum SessionRejectReason
	{
		 InvalidTagNumber = 0 ,
		 RequiredTagMissing = 1 ,
		 TagNotDefinedForMessageType = 2 ,
		 UndefinedTag = 3 ,
		 TagSpecifiedWithoutValue = 4 ,
		 ValueOutOfRange = 5 ,
		 IncorrectDataFormat = 6 ,
		 DecryptionProblem = 7 ,
		 SignatureProblem = 8 ,
		 CompIdProblem = 9 ,
		 SendingTimeAccuracyProblem = 10 ,
		 InvalidMsgType  = 11
	}

}
