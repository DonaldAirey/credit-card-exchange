namespace FluidTrade.Core
{

    using System;
    using System.Collections.Specialized;
    using System.Runtime.Serialization;
    using System.Text;

	/// <summary>
	/// This object contains information for trading securities over electronic connections.
	/// </summary>
	[Serializable()]
	public class FixMessage : Message
	{

		public const char FixSeparator = '\x0001';

		/// <summary>
		/// Default constructor for the FixMessage.
		/// </summary>
		public FixMessage() {}

		public FixMessage(Message message) : base(message) {}

		/// <summary>
		/// Serialization requires this constructor in order to access the base serialization constructor
		/// </summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
		public FixMessage(SerializationInfo info, StreamingContext context) : base(info, context) {}

		/// <summary>
		/// Creates a FIX message from a FIX string.
		/// </summary>
		/// <param name="fixString">A FIX string.</param>
		public FixMessage(string fixString)
		{

			// Initialize the values from the string.
			AddFixString(fixString, FixSeparator);

		}
		
		/// <summary>
		/// Creates a FIX message from a raw packet of data.
		/// </summary>
		/// <param name="packet">A raw packet of data.</param>
		public FixMessage(byte[] packet)
		{

			// Initialize the values from the packet.
			AddFixString(Encoding.ASCII.GetString(packet), FixSeparator);

		}
		
		/// <summary>
		/// Creates a FIX message from an array of fields.
		/// </summary>
		/// <param name="fields">An array of fields.</param>
		public FixMessage(string[] fields)
		{

			// Initialize the values from an array of fields.
			AddFieldArray(fields);

		}


		/// <summary>
		/// Converts the tag number string to a Tag, uses 
		/// the Parse method of the FixMessage class to convert
		/// the text value to the proper type, and adds the field.
		/// </summary>
		/// <param name="tagNumber"></param>
		/// <param name="stringValue"></param>
		public void AddField(string tagNumber, string stringValue) 
		{

			Tag tag = TagConverter.ConvertFrom(tagNumber);
			this[tag] = FixMessage.Parse(tag, stringValue);

		}

		/// <summary>
		/// Adds a piece of a FIX string to the FixMessage. Used for custom tags.
		/// Specify the separator between FIX tag=value fields.
		/// </summary>
		/// <param name="fixString"></param>
		/// <param name="separator"></param>
		public void AddFixString(string fixSubString, char separator)
		{

			// Split the string into an array of fields and add it.
			AddFieldArray(fixSubString.Split(separator));

		}

		/// <summary>
		/// Adds an array of fields to the FixMessage.
		/// </summary>
		/// <param name="fields">An array of fields.</param>
		private void AddFieldArray(string[] fields)
		{
			// This local is used for repeating groups (see below.)
			StringCollection groupFields	 = new StringCollection();

			// Add each field after parse the values into a "Tag=Value" pair.
			foreach (string field in fields)
			{

				// The 'Split' method has a bothersome habit of leaving a null entry when a token delimiter ends up a the end
				// of the string.  This will quickly filter out those byproducts of the 'Split'.
				if (field == string.Empty)
					continue;

				// Break the field up into the 'Tag' and 'Value' fields, parse them and add them to the hash table.
				string[] values = field.Split('=');
				if (values.Length > 1)
				{
					try
					{
						//tagNumber
						Tag tag = TagConverter.ConvertFrom(values[0]);

						string stringValue = values[1];

						// Handle repeating group fields here.
						// Per the FIX spec:
						//  "Fields within repeating data groups must be specified in the order that the fields are
						//  specified in the message definition within the FIX specification document. The NoXXX field,
						//  where XXX is the field being counted, specifies the number of repeating group instances and
						//  must immediately precede the repeating group contents."
						// Generally: 
						//   1. record the number of repeating group instances for a group when the NoXXX field occurs.
						//   2. when each field in the group is read, save it in the groupFields array, unless it the last field.
						//   3. when the last field of the group occurs, add a group entry to the group object. 
						// Order is important (See note from FIX spec above.) groupFields index 0 is the first field of the group, 1 is the second, etc.
						// The xxxxGroup object (derived from class RepeatingGroup) holds the all of the repeating groups relating to "xxxx".
						// Each repeating group in xxxxGroup is called an Entry.
						// The groupFields array is temp space to hold the group fields as they are read from the FIX string.
						switch (tag)
						{
								// IOIQualifiers. 
								// There is only one field (IOIQualifier) in each group, so groupFields is not used.
							case Tag.NoIOIQualifiers:
								// This field indicates the start of a set of repeating groups.
								// But for FIX 4.0, only one IOIQualifier is allowed and this field is not used.
								// So don't create the group object here, create it when the first qualifier is encountered.
								// See IOIQualifier below.
								break;
							case Tag.IOIQualifier:
								if (!this.ContainsKey(Tag.IoiQualifierGroup) )
								{
									// if there is not yet an IoiQualifierGroup, create the group object to hold the repeating groups.
									this[Tag.IoiQualifierGroup] = new IoiQualifierGroup();
								}
								// get the group object created previously.
								IoiQualifierGroup ioiQualifierGroup = (IoiQualifierGroup)this[Tag.IoiQualifierGroup];
								// IOIQualifier is the only field in this group, so add the group to the IoiQualifierGroup object.
								// IOIQualifier is not a string in FixMessage, so it needs to be parsed into its correct type.
								ioiQualifierGroup.Add( (IOIQualifier)FixMessage.Parse(tag, stringValue) );
								break;

								// RoutingIDs. (Valid in FIX 4.2 only.)
								// There are 2 fields in each group here, so save the first one in the groupFields array.
								// (It could just be saved in a string variable, but the array is more general. A group can have more than 2 fields.)
								// For example:
								// The following FIX string describes three repeating groups for Routing (215=3), with the first group having
								// two fields: RoutingType=3 (216=3), and RoutingID=RTEID1 (217=RTEID1). Similarly for groups 2 and 3.
								// 215=3|216=3|217=RTEID1|216=3|217=RTEID2|216=3|217=RTEID3|
							case Tag.NoRoutingIDs:
								// This field indicates the start of a set of repeating groups.
								// Create the group object to hold the repeating groups.
								this[Tag.RoutingGroup] = new RoutingGroup();
								break;
							case Tag.RoutingType:
								// Save the RoutingType string in the groupFields collection.
								groupFields.Add(stringValue);
								break;
							case Tag.RoutingID:
								// RoutingID is the last field in this group, so add the group to the RoutingGroup object.
								if ( this.Contains(Tag.RoutingGroup) )
								{
									// get the group object created above.
									RoutingGroup routingGroup = (RoutingGroup)this[Tag.RoutingGroup];
									if ( groupFields.Count > 0 )
									{
										// Add the current group fields to the group object.
										// The first (and in this case only) item in the groupFields array is RoutingType.
										// RoutingType combined with the RoutingID is a complete group.
										// RoutingID is a string in FixMessage, so it doesn't need to be parsed.
										// RoutingType is not a string in FixMessage, so it needs to be parsed into its correct type.
										RoutingType routingType = (RoutingType)FixMessage.Parse(Tag.RoutingType, groupFields[0]);
										routingGroup.Add( routingType,  stringValue );

										// once the group has been added, clear out the temp array for the next group.
										groupFields.Clear();
									}
								}
								break;

								// Regular processing of (non-repeating) FIX fields.
							default:
								this[tag] = FixMessage.Parse(tag, stringValue);
								break;

						}
					}
					catch
					{
						string SenderComp = this.Contains(Tag.SenderCompID) ? this.SenderCompID : string.Empty;
						string TargetComp = this.Contains(Tag.TargetCompID) ? this.TargetCompID : string.Empty;
						FluidTrade.Core.EventLog.Error("MsgSeqNum={0}, SenderCompID={1}, TargetCompID={2}: Can't parse FIX field {3}={4}. Field ignored.", this.MsgSeqNum, SenderComp, TargetComp, values[0], values[1]);
					}
				}

			}

		}

		/// <summary>
		/// Parses a string to a properly typed value based on the FIX tag.
		/// </summary>
		/// <param name="tag">Determines the datatype of the conversion.</param>
		/// <param name="value">A string representing a value.</param>
		public static object Parse(Tag tag, string value)
		{

			// Use the FIX tag to determine the parsing method and target data type.
			switch (tag)
			{
				case Tag.GapFillFlag:
				case Tag.IOINaturalFlag:
				case Tag.PossDupFlag:
				case Tag.ResetSeqNumFlag:

					// Boolean Values
					return value == "Y" ? true : false;
			
			
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
				case Tag.NoIOIQualifiers:
				case Tag.NoRoutingIDs:
				case Tag.RawDataLength:
				case Tag.RefSeqNum:

					// Int32 Values
					return Int32.Parse(value);

				case Tag.AvgPx:
				case Tag.Commission:
				case Tag.CxlQty:
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
					return Decimal.Parse(value);

				case Tag.ExpireTime:
				case Tag.OrigSendingTime:
				case Tag.SendingTime:
				case Tag.TransactTime:
				case Tag.ValidUntilTime:

					// DateTime Values
					int year = Int32.Parse(value.Substring(0, 4));
					int month = Int32.Parse(value.Substring(4, 2));
					int day = Int32.Parse(value.Substring(6, 2));
					int hour = Int32.Parse(value.Substring(9, 2));
					int minute = Int32.Parse(value.Substring(12, 2));
					int second = Int32.Parse(value.Substring(15, 2));
					return new DateTime(year, month, day, hour, minute, second);
				
				case Tag.MaturityMonthYear:

					int expYear = Int32.Parse(value.Substring(0, 4));
					int expMonth = Int32.Parse(value.Substring(4, 2));
					return new DateTime(expYear, expMonth, 1);

				//enums
				case Tag.BusinessRejectReason: return BusinessRejectReasonConverter.ConvertFrom(value);
				case Tag.CxlRejReason: return CxlRejReasonConverter.ConvertFrom(value);
				case Tag.CxlRejResponseTo: return CxlRejResponseToConverter.ConvertFrom(value);
				case Tag.CxlType: return CxlTypeConverter.ConvertFrom(value);
				case Tag.CommType: return CommTypeConverter.ConvertFrom(value);
				case Tag.DiscretionInst: return DiscretionInstConverter.ConvertFrom(value);
				case Tag.DKReason: return DKReasonConverter.ConvertFrom(value);
				case Tag.EncryptMethod: return EncryptMethodConverter.ConvertFrom(value);
				case Tag.ExecTransType: return ExecTransTypeConverter.ConvertFrom(value);
				case Tag.ExecType: return ExecTypeConverter.ConvertFrom(value);
				case Tag.HandlInst: return HandlInstConverter.ConvertFrom(value);
				case Tag.IOIQualifier: return IoiQualifierConverter.ConvertFrom(value);
				case Tag.IOIQltyInd: return IOIQltyIndConverter.ConvertFrom(value);
				case Tag.IOITransType: return IOITransTypeConverter.ConvertFrom(value);
				case Tag.LastCapacity: return LastCapacityConverter.ConvertFrom(value);
				case Tag.MsgType: return MsgTypeConverter.ConvertFrom(value);
				case Tag.RefMsgType: return MsgTypeConverter.ConvertFrom(value);
				case Tag.OpenClose: return OptionPositionTypeConverter.ConvertFrom(value);
				case Tag.PutOrCall: return OptionTypeConverter.ConvertFrom(value);
				case Tag.OrdRejReason: return OrdRejReasonConverter.ConvertFrom(value);
				case Tag.OrdStatus: return OrdStatusConverter.ConvertFrom(value);
				case Tag.OrdType: return OrdTypeConverter.ConvertFrom(value);
				case Tag.RoutingType: return RoutingTypeConverter.ConvertFrom(value);
				case Tag.SecurityType: return SecurityTypeConverter.ConvertFrom(value);
				case Tag.SessionRejectReason: return SessionRejectReasonConverter.ConvertFrom(value);
				case Tag.Side: return SideConverter.ConvertFrom(value);
				case Tag.TimeInForce: return TimeInForceConverter.ConvertFrom(value);


				default:
					// String Values
					return value;

			}

		}

		/// <summary>
		/// Packs the FIX Message into FIX string.
		/// </summary>
		/// <returns>the FIX Message string.</returns>
		public override string ToString()
		{

			string beginStringField = string.Empty;
			string msgTypeField = string.Empty;
			string headerFields = string.Empty;
			string bodyFields = string.Empty;
			string fieldValue = string.Empty;

			if ( !this.Contains(Tag.BeginString) ) throw new MissingFieldException("FIX BeginString field is missing");
			if ( !this.Contains(Tag.MsgType) ) throw new MissingFieldException("FIX MsgType field is missing");

			// Create a stream of bytes from the fields in the FIX message.  Remember that a hashtable -- the underlying data store
			// for the message -- iterates through the fields in a random order.  This is fine for most FIX fields, but some in the
			// header and the trailer must be in a specified order in the packet, so these fields are pulled out of the message
			// during this loop and assembled later into the final message.
			foreach (Field field in this)
			{

				switch (field.Tag)
				{

					case Tag.BeginString:

						// The 'BeginString' must always be the first field in the message.
						beginStringField = TagConverter.ConvertTo(Tag.BeginString) + "=" + field.Value.ToString() + FixSeparator.ToString();
						break;

					case Tag.BodyLength:
					case Tag.CheckSum:
						// these are calculated below once the FIX string is assembled.
						break;

					case Tag.MsgType:

						// The 'MsgType' field is always the third field in the message.
						string msgTypeValue = MsgTypeConverter.ConvertTo((MsgType)field.Value);
						msgTypeField = TagConverter.ConvertTo(Tag.MsgType) + "=" + msgTypeValue + FixSeparator.ToString();
						break;

						//////////////////
						// Header Fields
						//////////////////
					case Tag.PossDupFlag:
					//case Tag.PossResend:

						// bool Header Fields
						headerFields += TagConverter.ConvertTo(field.Tag) + "=" + ((bool)field.Value ? "Y" : "N") + FixSeparator.ToString();
						break;
				
					case Tag.SendingTime:
					//case Tag.OnBehalfOfSendingTime:
					case Tag.OrigSendingTime:

						// DateTime Header Fields.
						headerFields += TagConverter.ConvertTo(field.Tag) + "=" + ((DateTime)field.Value).ToString("yyyyMMdd-HH:mm:ss") + FixSeparator.ToString();
						break;

					case Tag.MsgSeqNum:

						// MsgSeqNum Header Field.
						headerFields += TagConverter.ConvertTo(field.Tag) + "=" + field.Value.ToString() + FixSeparator.ToString();
						break;

					case Tag.DeliverToCompID:
					//case Tag.DeliverToLocationID:
					case Tag.DeliverToSubID:
					case Tag.OnBehalfOfCompID:
					//case Tag.OnBehalfOfLocationID:
					case Tag.OnBehalfOfSubID:
					case Tag.SenderCompID:
					//case Tag.SenderLocationID:
					case Tag.SenderSubID:
					case Tag.TargetCompID:
					case Tag.TargetLocationID:
					case Tag.TargetSubID:

						// ID Header Fields.
						headerFields += TagConverter.ConvertTo(field.Tag) + "=" + field.Value.ToString() + FixSeparator.ToString();
						break;

						//////////////////
						// Body Fields
						//////////////////
						// These are the Body Fields ( in alpha order) that require some conversion or business logic. 
						// The 'default' case takes care of the rest.
					case Tag.Account:

						// the Account Body Field is not valid in a cancel message before FIX 4.2
						if ( !this.MsgType.Equals(MsgType.OrderCancelRequest) ||
							( this.MsgType.Equals(MsgType.OrderCancelRequest) && !this.BeginString.Equals("FIX.4.0") &&  !this.BeginString.Equals("FIX.4.1") ) )
						{
							if (field.Value != null)
								bodyFields += TagConverter.ConvertTo(field.Tag) + "=" + field.Value.ToString() + FixSeparator.ToString();
						}
						break;

					case Tag.BusinessRejectReason:

						// BusinessRejectReason Body Field
						fieldValue = BusinessRejectReasonConverter.ConvertTo((BusinessRejectReason)field.Value);
						bodyFields += TagConverter.ConvertTo(field.Tag) + "=" + fieldValue + FixSeparator.ToString();
						break;

					case Tag.CommType:

						// CommType Body Field
						fieldValue = CommTypeConverter.ConvertTo((CommType)field.Value);
						bodyFields += TagConverter.ConvertTo(field.Tag) + "=" + fieldValue + FixSeparator.ToString();
						break;

					case Tag.CxlRejReason:

						// CxlRejReason Body Field. 
						// Only FIX 4.2 and above allow CxlRejReason > 1. (1=CxlRejReason.UnknownOrder).
						if ( this.BeginString.Equals("FIX.4.0") || this.BeginString.Equals("FIX.4.1") )
						{
							if ( (CxlRejReason)field.Value > CxlRejReason.UnknownOrder )
								// default to TooLateToCancel
								field.Value = CxlRejReason.TooLateToCancel;
						}
						fieldValue = CxlRejReasonConverter.ConvertTo((CxlRejReason)field.Value);
						bodyFields += TagConverter.ConvertTo(field.Tag) + "=" + fieldValue + FixSeparator.ToString();
						break;

					case Tag.CxlRejResponseTo:

						// the CxlRejResponseTo Body Field is not valid in a cancel-reject message before FIX 4.2
						if ( !this.MsgType.Equals(MsgType.OrderCancelReject) ||
							( this.MsgType.Equals(MsgType.OrderCancelReject) && !this.BeginString.Equals("FIX.4.0") &&  !this.BeginString.Equals("FIX.4.1") ) )
						{
							fieldValue = CxlRejResponseToConverter.ConvertTo((CxlRejResponseTo)field.Value);
							bodyFields += TagConverter.ConvertTo(field.Tag) + "=" + fieldValue + FixSeparator.ToString();
						}
						break;

					case Tag.CxlType:

						// CxlType Body Field Only for FIX 4.0
						if ( this.BeginString.Equals("FIX.4.0") )
						{
							fieldValue = CxlTypeConverter.ConvertTo((CxlType)field.Value);
							bodyFields += TagConverter.ConvertTo(field.Tag) + "=" + fieldValue + FixSeparator.ToString();
						}
						break;

					case Tag.DiscretionInst:

						// DiscretionInst Body Field
						fieldValue = DiscretionInstConverter.ConvertTo((DiscretionInst)field.Value);
						bodyFields += TagConverter.ConvertTo(field.Tag) + "=" + fieldValue + FixSeparator.ToString();
						break;

					case Tag.DKReason:

						// DKReason Body Field
						fieldValue = DKReasonConverter.ConvertTo((DKReason)field.Value);
						bodyFields += TagConverter.ConvertTo(field.Tag) + "=" + fieldValue + FixSeparator.ToString();
						break;

					case Tag.EncryptMethod:

						// EncryptMethod Body Field
						fieldValue = EncryptMethodConverter.ConvertTo((EncryptMethod)field.Value);
						bodyFields += TagConverter.ConvertTo(field.Tag) + "=" + fieldValue + FixSeparator.ToString();
						break;

					case Tag.ExecTransType:

						// ExecTransType Body Field
						fieldValue = ExecTransTypeConverter.ConvertTo((ExecTransType)field.Value);
						bodyFields += TagConverter.ConvertTo(field.Tag) + "=" + fieldValue + FixSeparator.ToString();
						break;

					case Tag.ExecType:

						// ExecType Body Field - Not valid in FIX 4.0
						if ( !this.BeginString.Equals("FIX.4.0") )
						{
							fieldValue = ExecTypeConverter.ConvertTo((ExecType)field.Value);
							bodyFields += TagConverter.ConvertTo(field.Tag) + "=" + fieldValue + FixSeparator.ToString();
						}
						break;

					case Tag.ExpireTime:

						// ExpireTime Body Field
						bodyFields += TagConverter.ConvertTo(field.Tag) + "=" + ((DateTime)field.Value).ToString("yyyyMMdd-HH:mm:ss") + FixSeparator.ToString();
						break;

					case Tag.GapFillFlag:

						// GapFillFlag Body Field
						bodyFields += TagConverter.ConvertTo(field.Tag) + "=" + ((bool)field.Value ? "Y" : "N") + FixSeparator.ToString();
						break;

					case Tag.HandlInst:

						// HandlInst Body Field
						fieldValue = HandlInstConverter.ConvertTo((HandlInst)field.Value);
						bodyFields += TagConverter.ConvertTo(field.Tag) + "=" + fieldValue + FixSeparator.ToString();
						break;

					case Tag.IOINaturalFlag:

						// IOINaturalFlag Body Field
						bodyFields += TagConverter.ConvertTo(field.Tag) + "=" + ((bool)field.Value ? "Y" : "N") + FixSeparator.ToString();
						break;

					case Tag.IOIQltyInd:

						// IOIQltyInd Body Field
						fieldValue = IOIQltyIndConverter.ConvertTo((IOIQltyInd)field.Value);
						bodyFields += TagConverter.ConvertTo(field.Tag) + "=" + fieldValue + FixSeparator.ToString();
						break;

					case Tag.IoiQualifierGroup:

						// Expand the repeating group fields.
						IoiQualifierGroup ioiQualifierGroup = (IoiQualifierGroup)field.Value;
						if ((ioiQualifierGroup != null) && (ioiQualifierGroup.Count > 0))
						{
							// The 'NoIOIQualifiers' field is only valid for FIX 4.1 and above.
							if ( !this.BeginString.Equals("FIX.4.0") )
							{
								// the number of repeating instances goes in the 'NoIOIQualifiers' field.
								bodyFields += TagConverter.ConvertTo(Tag.NoIOIQualifiers) + "=" + ioiQualifierGroup.Count.ToString() + FixSeparator.ToString();
							}
							// Add the qualifiers.
							foreach (IoiQualifierGroup.Entry entry in ioiQualifierGroup)
							{
								fieldValue = IoiQualifierConverter.ConvertTo(entry.IoiQualifier);			
								bodyFields += TagConverter.ConvertTo(Tag.IOIQualifier) + "=" + fieldValue + FixSeparator.ToString();

								// FIX 4.0 only supports 1 qualifier.
								if ( this.BeginString.Equals("FIX.4.0") )
									break;
							}
						}
						break;
						// These fields should be part of the repeating group "IoiQualifierGroup",
						// and should NOT be used individually.
					case Tag.NoIOIQualifiers:
					case Tag.IOIQualifier:
						break;

					case Tag.IOITransType:

						// IOITransType Body Field
						fieldValue = IOITransTypeConverter.ConvertTo((IOITransType)field.Value);
						bodyFields += TagConverter.ConvertTo(field.Tag) + "=" + fieldValue + FixSeparator.ToString();
						break;

					case Tag.LastCapacity:

						// LastCapacity Body Field
						fieldValue = LastCapacityConverter.ConvertTo((LastCapacity)field.Value);
						bodyFields += TagConverter.ConvertTo(field.Tag) + "=" + fieldValue + FixSeparator.ToString();
						break;

					case Tag.LeavesQty:

						// LeavesQty Body Field - Not valid in FIX 4.0
						if ( !this.BeginString.Equals("FIX.4.0") )
						{
							fieldValue = field.Value.ToString();
							bodyFields += TagConverter.ConvertTo(field.Tag) + "=" + fieldValue + FixSeparator.ToString();
						}
						break;

					case Tag.MaturityMonthYear:

						// MaturityMonthYear Body Field
						bodyFields += TagConverter.ConvertTo(field.Tag) + "=" + ((DateTime)field.Value).ToString("yyyyMM") + FixSeparator.ToString();
						break;

					case Tag.OpenClose:

						// OpenClose Body Field
						fieldValue = OptionPositionTypeConverter.ConvertTo((OptionPositionType)field.Value);
						bodyFields += TagConverter.ConvertTo(field.Tag) + "=" + fieldValue + FixSeparator.ToString();
						break;

					case Tag.OrdRejReason:

						// OrdRejReason Body Field
						fieldValue = OrdRejReasonConverter.ConvertTo((OrdRejReason)field.Value);
						bodyFields += TagConverter.ConvertTo(field.Tag) + "=" + fieldValue + FixSeparator.ToString();
						break;

					case Tag.OrdStatus:

						// the OrdStatus Body Field is not valid in a cancel-reject message before FIX 4.1
						if ( !this.MsgType.Equals(MsgType.OrderCancelReject) ||
							( this.MsgType.Equals(MsgType.OrderCancelReject) && !this.BeginString.Equals("FIX.4.0") ) )
						{
							fieldValue = OrdStatusConverter.ConvertTo((OrdStatus)field.Value);
							bodyFields += TagConverter.ConvertTo(field.Tag) + "=" + fieldValue + FixSeparator.ToString();
						}
						break;

					case Tag.OrdType:

						// OrdType Body Field
						fieldValue = OrdTypeConverter.ConvertTo((OrderType)field.Value);
						bodyFields += TagConverter.ConvertTo(field.Tag) + "=" + fieldValue + FixSeparator.ToString();
						break;

					case Tag.OrigClOrdID:

						// the OrigClOrdID Body Field is not valid in cancel-reject or execution report messages before FIX 4.1
						if ( (!this.MsgType.Equals(MsgType.OrderCancelReject) && !this.MsgType.Equals(MsgType.ExecutionReport)) ||
							 ( this.MsgType.Equals(MsgType.OrderCancelReject) && !this.BeginString.Equals("FIX.4.0") ) ||
							 ( this.MsgType.Equals(MsgType.ExecutionReport) && !this.BeginString.Equals("FIX.4.0") ) )
						{
							if (field.Value != null)
								bodyFields += TagConverter.ConvertTo(field.Tag) + "=" + field.Value.ToString() + FixSeparator.ToString();
						}
						break;

					case Tag.PutOrCall:

						// PutOrCall Body Field
						fieldValue = OptionTypeConverter.ConvertTo((OptionType)field.Value);
						bodyFields += TagConverter.ConvertTo(field.Tag) + "=" + fieldValue + FixSeparator.ToString();
						break;

					case Tag.RefMsgType:

						// RefMsgType Body Field
						fieldValue = MsgTypeConverter.ConvertTo((MsgType)field.Value);
						bodyFields += TagConverter.ConvertTo(field.Tag) + "=" + fieldValue + FixSeparator.ToString();
						break;

					case Tag.ResetSeqNumFlag:

						// ResetSeqNumFlag Body Field
						bodyFields += TagConverter.ConvertTo(field.Tag) + "=" + ((bool)field.Value ? "Y" : "N") + FixSeparator.ToString();
						break;

					case Tag.RoutingGroup:

						// Expand the Routing repeating group fields.
						RoutingGroup routingGroup = (RoutingGroup)field.Value;
						if ((routingGroup != null) && (routingGroup.Count > 0))
						{
							// the number of repeating instances goes in the 'NoRoutingIDs' field.
							bodyFields += TagConverter.ConvertTo(Tag.NoRoutingIDs) + "=" + routingGroup.Count.ToString() + FixSeparator.ToString();
							foreach (RoutingGroup.Entry entry in routingGroup)
							{
								// Add the routing type.
								fieldValue = RoutingTypeConverter.ConvertTo(entry.RoutingType);			
								bodyFields += TagConverter.ConvertTo(Tag.RoutingType) + "=" + fieldValue + FixSeparator.ToString();
								
								// Add the routing ID.
								bodyFields += TagConverter.ConvertTo(Tag.RoutingID) + "=" + entry.RoutingId + FixSeparator.ToString();
							}
						}
						break;
						// These fields should be part of the repeating group "RoutingGroup",
						// and should NOT be used individually.
					case Tag.NoRoutingIDs:
					case Tag.RoutingID:
					case Tag.RoutingType:
						break;

					case Tag.SecurityType:

						// SecurityType Body Field
						fieldValue = SecurityTypeConverter.ConvertTo((SecurityType)field.Value);
						bodyFields += TagConverter.ConvertTo(field.Tag) + "=" + fieldValue + FixSeparator.ToString();
						break;

					case Tag.SessionRejectReason:

						// SessionRejectReason Body Field
						fieldValue = SessionRejectReasonConverter.ConvertTo((SessionRejectReason)field.Value);
						bodyFields += TagConverter.ConvertTo(field.Tag) + "=" + fieldValue + FixSeparator.ToString();
						break;

					case Tag.Side:

						// Side Body Field
						fieldValue = SideConverter.ConvertTo((Side)field.Value);
						bodyFields += TagConverter.ConvertTo(field.Tag) + "=" + fieldValue + FixSeparator.ToString();
						break;

					case Tag.TimeInForce:

						// TimeInForce Body Field
						fieldValue = TimeInForceConverter.ConvertTo((TimeInForce)field.Value);
						bodyFields += TagConverter.ConvertTo(field.Tag) + "=" + fieldValue + FixSeparator.ToString();
						break;

					case Tag.TransactTime:

						// the TransactTime Body Field is not valid before FIX 4.2
						if ( !this.BeginString.Equals("FIX.4.0") && !this.BeginString.Equals("FIX.4.1") )	
						{
							bodyFields += TagConverter.ConvertTo(field.Tag) + "=" + ((DateTime)field.Value).ToString("yyyyMMdd-HH:mm:ss") + FixSeparator.ToString();
						}
						break;


					case Tag.ValidUntilTime:

						// ValidUntilTime Body Field
						bodyFields += TagConverter.ConvertTo(field.Tag) + "=" + ((DateTime)field.Value).ToString("yyyyMMdd-HH:mm:ss") + FixSeparator.ToString();
						break;


					default:
						// Default processing of these types for Tags not in the above list.
						// Int32 Body Fields
						// Decimal Body Fields
						// String Body Fields
						if (field.Value != null)
							bodyFields += TagConverter.ConvertTo(field.Tag) + "=" + field.Value.ToString() + FixSeparator.ToString();
						break;

				}

			}

			if ( headerFields.Equals(string.Empty) ) throw new MissingFieldException("FIX Header fields are missing");

			// The 'BodyLength' fields must be calculated after the body and part of the header is assembled.
			int bodyPacketLength = msgTypeField.Length + headerFields.Length + bodyFields.Length;
			string bodyLengthField = TagConverter.ConvertTo(Tag.BodyLength) + "=" + bodyPacketLength.ToString() + FixSeparator.ToString();

			// Assemble the packet from the component fields.
			string packet = beginStringField + bodyLengthField + msgTypeField + headerFields + bodyFields;

			// The final step in assembling a packet is the calculation of the checksum.  This is the least significant byte of 
			// the sum of all the byte values in the packet.
			int checksum = 0;
			foreach (byte messageByte in packet) checksum += messageByte;
			string checksumField = TagConverter.ConvertTo(Tag.CheckSum) + "=" + (checksum & 0xFF).ToString("000") + FixSeparator.ToString();

			return (packet + checksumField);
		}

		/// <summary>
		/// Packs the FIX Message into a raw data packet suitable for transmission.
		/// </summary>
		/// <returns>A stream of bytes representing the FIX Message.</returns>
		public override byte[] ToPacket()
		{
			// Convert the packet and checksum to an array of bytes which is suitable for transmission.
			return Encoding.ASCII.GetBytes(this.ToString());
		}

	}

}
