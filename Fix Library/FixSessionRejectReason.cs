namespace FluidTrade.Core
{


    /// <summary>
	/// Summary description for FixSessionRejectReason.
	/// </summary>
	public class FixSessionRejectReason
	{

		public const string InvalidTagNumber = "0";
		public const string RequiredTagMissing = "1";
		public const string TagNotDefinedForThisMessageType = "2";
		public const string UndefinedTag = "3";
		public const string TagSpecifiedWithoutValue = "4";
		public const string ValueIsIncorrect = "5";
		public const string IncorrectDataFormat = "6";
		public const string DecryptionProblem = "7";
		public const string SignatureProblem = "8";
		public const string CompIDProblem = "9";
		public const string SendingTimeAccuracyProblem = "10";
		public const string InvalidMsgType = "11";
		public const string XMLValidationError = "12";
		public const string TagAppearsMoreThanOnce = "13";
		public const string TagSpecifiedOutOfRequiredOrder = "14";
		public const string RepeatingGroupFieldsOutOfOrder = "15";
		public const string IncorrectNumInGroupCountForRepeatingGroup = "16";
		public const string NonDataValueIncludesFieldDelimiter = "17";
		public const string Other = "99";

	}

}
