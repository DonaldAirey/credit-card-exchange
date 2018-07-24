namespace FluidTrade.Core
{
    using System;

    /// <summary>
	/// Validator for FixMessages
	/// </summary>
	public class Validator
	{
		static Validator() {}

		/// <summary>
		/// Validates a FixMessage. Returns empty string if valid, otherwise a message indicating the validation errors.
		/// </summary>
		/// <param name="fixMessage"></param>
		/// <returns>string</returns>
		public static string ValidateFixMessage(FixMessage fixMessage)
		{
			if (fixMessage == null)
				return "Fix Message Invalid.";

			string validationMsg = string.Empty;
			try
			{
				if ( !fixMessage.Contains(Tag.MsgType) ) validationMsg += "MsgType";
				if ( !fixMessage.Contains(Tag.BeginString) ) validationMsg += ", BeginString";
				if ( !fixMessage.Contains(Tag.SenderCompID) ) validationMsg += ", SenderCompID";
				if ( !fixMessage.Contains(Tag.TargetCompID) ) validationMsg += ", TargetCompID";

				if (validationMsg != string.Empty)
				{
					validationMsg = string.Format("Missing fields: {0}.", validationMsg);
				}
			}
			catch (Exception ex)
			{
				validationMsg = string.Format("ValidateFixMessage: Error: {0}", ex.Message);
			}

			return validationMsg;
		}

	}
}
