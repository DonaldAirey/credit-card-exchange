namespace FluidTrade.Core
{

	using System;
	using System.IO;

	/// <summary>
	/// Summary description for MessageWriter.
	/// </summary>
	public class MessageWriter : BinaryWriter
	{

		public MessageWriter(FileStream fileStream) : base(fileStream) {}

		public void Write(Message message)
		{

			Write(message.Count);
			foreach (Field field in message)
				Write(field);

		}

		public void Write(Field field)
		{

			Write(Convert.ToInt32(field.Tag));
			switch (field.Tag)
			{

			case Tag.GapFillFlag:
			case Tag.PossDupFlag:
			case Tag.ResetSeqNumFlag:

				// Boolean Values
				Write(Convert.ToBoolean(field.Value));
				break;
			
			case Tag.BeginSeqNo:
			case Tag.BodyLength:
			case Tag.CheckSum:
			case Tag.EncodedTextLen:
			case Tag.EndSeqNo:
			case Tag.HeartBtInt:
			case Tag.InternalRecordId:
			case Tag.MsgSeqNum:
			case Tag.NewSeqNo:
			case Tag.NextExpectedMsgSeqNum:
			case Tag.RawDataLength:
			case Tag.RefSeqNum:

				// Int32 Values
				Write(Convert.ToInt32(field.Value));
				break;

			case Tag.AvgPx:
			case Tag.Commission:
			case Tag.CMSCxlQty:
			case Tag.CMSLeavesQty:
			case Tag.CumQty:
			case Tag.DiscretionOffset:
			case Tag.LastPx:
			case Tag.LastShares:
			case Tag.LeavesQty:
			case Tag.MaxFloor:
			case Tag.MinQty:
			case Tag.OrderQty:
			case Tag.PegDifference:
			case Tag.Price:
			case Tag.StopPx:
			case Tag.StrikePrice:

				// Decimal Values
				Write(Convert.ToDecimal(field.Value));
				break;

			case Tag.ExpireTime:
			case Tag.OrigSendingTime:
			case Tag.SendingTime:
			case Tag.TransactTime:

				// DateTime Values
				DateTime dateTime = (DateTime)field.Value;
				Write(dateTime.Year);
				Write(dateTime.Month);
				Write(dateTime.Day);
				Write(dateTime.Hour);
				Write(dateTime.Minute);
				Write(dateTime.Second);
				break;

			case Tag.MaturityMonthYear:

				DateTime expDateTime = (DateTime)field.Value;
				Write(expDateTime.ToString("yyyyMM"));
				break;

			// Enum Data Types
			case Tag.BusinessRejectReason:
			case Tag.CxlRejReason:
			case Tag.CxlRejResponseTo:
			case Tag.CxlType:
			case Tag.CommType:
			case Tag.DiscretionInst:
			case Tag.DKReason:
			case Tag.EncryptMethod:
			case Tag.ExecTransType:
			case Tag.ExecType:
			case Tag.HandlInst:
			case Tag.LastCapacity:
			case Tag.MsgType:
			case Tag.RefMsgType:
			case Tag.OpenClose:
			case Tag.PutOrCall:
			case Tag.OrdRejReason:
			case Tag.OrdStatus:
			case Tag.OrdType:
			case Tag.SecurityType:
			case Tag.SessionRejectReason:
			case Tag.Side:
			case Tag.TimeInForce:
				
				// Enum Data Types
				Write(Convert.ToInt32(field.Value));
				return;

			default:
				// Write tags that are not specified above as string values
				Write(Convert.ToString(field.Value));
				return;

			}

		}

	}

}
