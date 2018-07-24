namespace FluidTrade.Core
{

	using System;
	using System.Collections;
    using System.Runtime.Serialization;


    /// <summary>
	/// This object is used to contain Fields (Fields are Tag-object pairs).
	/// </summary>
	[Serializable()]
	public class Message : Hashtable
	{
		// From the FIX spec:
		// "The tag numbers greater than or equal to 10000 have been reserved for internal use (within a single firm) 
		// and do not need to be registered/reserved via the FIX website."
		// Here, anything prefaced by "Internal" is one of these fields.

		// Public Accessors for fix message fields

		// Boolean Values
		public bool GapFillFlag {get {return (bool)this[Tag.GapFillFlag];} set {this[Tag.GapFillFlag] = value;}}
		public bool IOINaturalFlag {get {return (bool)this[Tag.IOINaturalFlag];} set {this[Tag.IOINaturalFlag] = value;}}
		public bool PossDupFlag
		{
			get {object possDupObject = this[Tag.PossDupFlag]; return possDupObject == null ? false : (bool)possDupObject;}
			set {this[Tag.PossDupFlag] = value;}
		}
		public bool ResetSeqNumFlag
		{
			get {object resetSeqNumObject = this[Tag.ResetSeqNumFlag]; return resetSeqNumObject == null ? false : (bool)resetSeqNumObject;}
			set {this[Tag.ResetSeqNumFlag] = value;}
		}

		// Int32 Values
		public int BeginSeqNo {get {return (int)this[Tag.BeginSeqNo];} set {this[Tag.BeginSeqNo] = value;}}
		public int CheckSum {get {return (int)this[Tag.CheckSum];} set {this[Tag.CheckSum] = value;}}
		public int EndSeqNo {get {return (int)this[Tag.EndSeqNo];} set {this[Tag.EndSeqNo] = value;}}
		public int HeartBtInt {get {return (int)this[Tag.HeartBtInt];} set {this[Tag.HeartBtInt] = value;}}
		public int InternalRecordId {get {return (int)this[Tag.InternalRecordId];} set {this[Tag.InternalRecordId] = value;}}
		public int MsgSeqNum {get {return (int)this[Tag.MsgSeqNum];} set {this[Tag.MsgSeqNum] = value;}}
		public int NewSeqNo {get {return (int)this[Tag.NewSeqNo];} set {this[Tag.NewSeqNo] = value;}}
		public int NextExpectedMsgSeqNum {get {return (int)this[Tag.NextExpectedMsgSeqNum];} set {this[Tag.NextExpectedMsgSeqNum] = value;}}

		// Decimal Values
		public decimal AvgPx {get {return (decimal)this[Tag.AvgPx];} set {this[Tag.AvgPx] = value;}}
		public decimal Commission {get {return (decimal)this[Tag.Commission];} set {this[Tag.Commission] = value;}}
		public decimal CMSCxlQty {get {return (decimal)this[Tag.CMSCxlQty];} set {this[Tag.CMSCxlQty] = value;}}
		public decimal CMSLeavesQty {get {return (decimal)this[Tag.CMSLeavesQty];} set {this[Tag.CMSLeavesQty] = value;}}
		public decimal CumQty {get {return (decimal)this[Tag.CumQty];} set {this[Tag.CumQty] = value;}}
		public decimal DiscretionOffset {get {return (decimal)this[Tag.DiscretionOffset];} set {this[Tag.DiscretionOffset] = value;}}
		public decimal LastPx {get {return (decimal)this[Tag.LastPx];} set {this[Tag.LastPx] = value;}}
		public decimal LastShares {get {return (decimal)this[Tag.LastShares];} set {this[Tag.LastShares] = value;}}
		public decimal LeavesQty {get {return (decimal)this[Tag.LeavesQty];} set {this[Tag.LeavesQty] = value;}}
		public decimal MaxFloor {get {return (decimal)this[Tag.MaxFloor];} set {this[Tag.MaxFloor] = value;}}
		public decimal MinQty {get {return (decimal)this[Tag.MinQty];} set {this[Tag.MinQty] = value;}}
		public decimal OrderQty {get {return (decimal)this[Tag.OrderQty];} set {this[Tag.OrderQty] = value;}}
		public decimal PegDifference {get {return (decimal)this[Tag.PegDifference];} set {this[Tag.PegDifference] = value;}}
		public decimal Price {get {return (decimal)this[Tag.Price];} set {this[Tag.Price] = value;}}
		public decimal StopPx {get {return (decimal)this[Tag.StopPx];} set {this[Tag.StopPx] = value;}}
		public decimal StrikePrice {get {return (decimal)this[Tag.StrikePrice];} set {this[Tag.StrikePrice] = value;}}
		
		// DateTime Values
		public DateTime ExpireTime {get {return (DateTime)this[Tag.ExpireTime];} set {this[Tag.ExpireTime] = value;}}
		public DateTime MaturityMonthYear {get {return (DateTime)this[Tag.MaturityMonthYear];} set {this[Tag.MaturityMonthYear] = value;}}
		public DateTime OrigSendingTime {get {return (DateTime)this[Tag.OrigSendingTime];} set {this[Tag.OrigSendingTime] = value;}}
		public DateTime SendingTime {get {return (DateTime)this[Tag.SendingTime];} set {this[Tag.SendingTime] = value;}}
		public DateTime TransactTime {get {return (DateTime)this[Tag.TransactTime];} set {this[Tag.TransactTime] = value;}}
		public DateTime ValidUntilTime {get {return (DateTime)this[Tag.ValidUntilTime];} set {this[Tag.ValidUntilTime] = value;}}

		// String Values
		public string Account {get {return (string)this[Tag.Account];} set {this[Tag.Account] = value;}}
		public string BeginString {get {return (string)this[Tag.BeginString];} set {this[Tag.BeginString] = value;}}
		public string BusinessRejectRefID {get {return (string)this[Tag.BusinessRejectRefID];} set {this[Tag.BusinessRejectRefID] = value;}}
		public string ClOrdID {get {return (string)this[Tag.ClOrdID];} set {this[Tag.ClOrdID] = value;}}
		public string ClientID {get {return (string)this[Tag.ClientID];} set {this[Tag.ClientID] = value;}}
		public string DeliverToCompID {get {return (string)this[Tag.DeliverToCompID];} set {this[Tag.DeliverToCompID] = value;}}
		public string DeliverToSubID {get {return (string)this[Tag.DeliverToSubID];} set {this[Tag.DeliverToSubID] = value;}}
		public string EncodedText {get {return (string)this[Tag.EncodedText];} set {this[Tag.EncodedText] = value;}}
		public string ExDestination {get {return (string)this[Tag.ExDestination];} set {this[Tag.ExDestination] = value;}}
		public string ExecBroker {get {return (string)this[Tag.ExecBroker];} set {this[Tag.ExecBroker] = value;}}
		public string ExecID {get {return (string)this[Tag.ExecID];} set {this[Tag.ExecID] = value;}}
		public string ExecInst {get {return (string)this[Tag.ExecInst];} set {this[Tag.ExecInst] = value;}}
		public string ExecRefID {get {return (string)this[Tag.ExecRefID];} set {this[Tag.ExecRefID] = value;}}
		public string InternalError {get {return (string)this[Tag.InternalError];} set {this[Tag.InternalError] = value;}}
		public string InternalSourceId {get {return (string)this[Tag.InternalSourceId];} set {this[Tag.InternalSourceId] = value;}}
		public string IOIid {get {return (string)this[Tag.IOIid];} set {this[Tag.IOIid] = value;}}
		public string IOIRefID {get {return (string)this[Tag.IOIRefID];} set {this[Tag.IOIRefID] = value;}}
		public string IOIShares {get {return (string)this[Tag.IOIShares];} set {this[Tag.IOIShares] = value;}}
		public string OnBehalfOfCompID {get {return (string)this[Tag.OnBehalfOfCompID];} set {this[Tag.OnBehalfOfCompID] = value;}}
		public string OnBehalfOfSubID {get {return (string)this[Tag.OnBehalfOfSubID];} set {this[Tag.OnBehalfOfSubID] = value;}}
		public string OrderID {get {return (string)this[Tag.OrderID];} set {this[Tag.OrderID] = value;}}
		public string OrigClOrdID {get {return (string)this[Tag.OrigClOrdID];} set {this[Tag.OrigClOrdID] = value;}}
		public string Password {get {return (string)this[Tag.Password];} set {this[Tag.Password] = value;}}
		public string RawData {get {return (string)this[Tag.RawData];} set {this[Tag.RawData] = value;}}
		public string Rule80A {get {return (string)this[Tag.Rule80A];} set {this[Tag.Rule80A] = value;}}
		public string SecurityID {get {return (string)this[Tag.SecurityID];} set {this[Tag.SecurityID] = value;}}
		public string SenderCompID {get {return (string)this[Tag.SenderCompID];} set {this[Tag.SenderCompID] = value;}}
		public string SenderSubID {get {return (string)this[Tag.SenderSubID];} set {this[Tag.SenderSubID] = value;}}
		public string Symbol {get {return (string)this[Tag.Symbol];} set {this[Tag.Symbol] = value;}}
		public string SymbolSfx {get {return (string)this[Tag.SymbolSfx];} set {this[Tag.SymbolSfx] = value;}}
		public string TargetCompID {get {return (string)this[Tag.TargetCompID];} set {this[Tag.TargetCompID] = value;}}
		public string TargetLocationID {get {return (string)this[Tag.TargetLocationID];} set {this[Tag.TargetLocationID] = value;}}
		public string TargetSubID {get {return (string)this[Tag.TargetSubID];} set {this[Tag.TargetSubID] = value;}}
		public string TestReqID {get {return (string)this[Tag.TestReqID];} set {this[Tag.TestReqID] = value;}}
		public string Text {get {return (string)this[Tag.Text];} set {this[Tag.Text] = value;}}
		public string Username {get {return (string)this[Tag.Username];} set {this[Tag.Username] = value;}}
		
		//enums and groups
		public BusinessRejectReason BusinessRejectReason {get {return (BusinessRejectReason)this[Tag.BusinessRejectReason];} set {this[Tag.BusinessRejectReason] = value;}}
		public CxlRejReason CxlRejReason {get {return (CxlRejReason)this[Tag.CxlRejReason];} set {this[Tag.CxlRejReason] = value;}}
		public CxlRejResponseTo CxlRejResponseTo {get {return (CxlRejResponseTo)this[Tag.CxlRejResponseTo];} set {this[Tag.CxlRejResponseTo] = value;}}
		public CxlType CxlType {get {return (CxlType)this[Tag.CxlType];} set {this[Tag.CxlType] = value;}}
		public CommType CommType {get {return (CommType)this[Tag.CommType];} set {this[Tag.CommType] = value;}}
		public DiscretionInst DiscretionInst {get {return (DiscretionInst)this[Tag.DiscretionInst];} set {this[Tag.DiscretionInst] = value;}}
		public DKReason DKReason {get {return (DKReason)this[Tag.DKReason];} set {this[Tag.DKReason] = value;}}
		public EncryptMethod EncryptMethod {get {return (EncryptMethod)this[Tag.EncryptMethod];} set {this[Tag.EncryptMethod] = value;}}
		public ExecTransType ExecTransType {get {return (ExecTransType)this[Tag.ExecTransType];} set {this[Tag.ExecTransType] = value;}}
		public ExecType ExecType {get {return (ExecType)this[Tag.ExecType];} set {this[Tag.ExecType] = value;}}
		public HandlInst HandlInst {get {return (HandlInst)this[Tag.HandlInst];} set {this[Tag.HandlInst] = value;}}
		public IOIQltyInd IOIQltyInd {get {return (IOIQltyInd)this[Tag.IOIQltyInd];} set {this[Tag.IOIQltyInd] = value;}}
		public IoiQualifierGroup IoiQualifierGroup {get {return (IoiQualifierGroup)this[Tag.IoiQualifierGroup];} set {this[Tag.IoiQualifierGroup] = value;}}
		public IOITransType IOITransType {get {return (IOITransType)this[Tag.IOITransType];} set {this[Tag.IOITransType] = value;}}
		public LastCapacity LastCapacity {get {return (LastCapacity)this[Tag.LastCapacity];} set {this[Tag.LastCapacity] = value;}}
		public MsgType MsgType {get {return (MsgType)this[Tag.MsgType];} set {this[Tag.MsgType] = value;}}
		public MsgType RefMsgType {get {return (MsgType)this[Tag.RefMsgType];} set {this[Tag.RefMsgType] = value;}}
		public OptionPositionType OpenClose {get {return (OptionPositionType)this[Tag.OpenClose];} set {this[Tag.OpenClose] = value;}}
		public OptionType PutOrCall {get {return (OptionType)this[Tag.PutOrCall];} set {this[Tag.PutOrCall] = value;}}
		public OrdRejReason OrdRejReason {get {return (OrdRejReason)this[Tag.OrdRejReason];} set {this[Tag.OrdRejReason] = value;}}
		public OrdStatus OrdStatus {get {return (OrdStatus)this[Tag.OrdStatus];} set {this[Tag.OrdStatus] = value;}}
		public OrderType OrdType {get {return (OrderType)this[Tag.OrdType];} set {this[Tag.OrdType] = value;}}
		public RoutingGroup RoutingGroup {get {return (RoutingGroup)this[Tag.RoutingGroup];} set {this[Tag.RoutingGroup] = value;}}
		public SecurityType SecurityType {get {return (SecurityType)this[Tag.SecurityType];} set {this[Tag.SecurityType] = value;}}
		public SessionRejectReason SessionRejectReason {get {return (SessionRejectReason)this[Tag.SessionRejectReason];} set {this[Tag.SessionRejectReason] = value;}}
		public Side Side {get {return (Side)this[Tag.Side];} set {this[Tag.Side] = value;}}
		public TimeInForce TimeInForce {get {return (TimeInForce)this[Tag.TimeInForce];} set {this[Tag.TimeInForce] = value;}}

		/// <summary>
		/// Indexer to find the object corresponding to the given Tag.
		/// </summary>
		public object this[Tag tag]
		{

			// Get Accessor
			get {return (object)base[tag];}

			// Set Accessor
			set {if (base.ContainsKey(tag)) base[tag] = value; else base.Add(tag, value);}

		}

		public Message() {}

		public Message(Message message) : base(message) {}

		/// <summary>
		/// Serialization requires this constructor in order to access the base serialization constructor
		/// </summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
		public Message(SerializationInfo info, StreamingContext context) : base(info, context) {}

		/// <summary>
		/// Creates an enumerator for iterating through the fields of a FIX message.
		/// </summary>
		/// <returns>An enumerator for iterating through a FIX message.</returns>
		public new IEnumerator GetEnumerator()
		{

			// Use the unerlying hash table to create the enumerator.
			return new FieldEnumerator(base.GetEnumerator());

		}
	
		/// <summary>
		/// Adds a Field to the message.
		/// </summary>
		/// <param name="field"></param>
		public void Add(Field field) {this[field.Tag] = field.Value;}

		/// <summary>
		/// Packs the Message into a raw data packet suitable for transmission.
		/// </summary>
		/// <returns>A stream of bytes representing the FIX Message.</returns>
		public virtual byte[] ToPacket() {return new byte[] {};}

	}

	/// <summary>
	/// Iterates through FixFields in a FixMessage.
	/// </summary>
	internal class FieldEnumerator : IEnumerator
	{

		// Private Members
		private IEnumerator iEnumerator;

		/// <summary>
		/// Creates an Enumerator for iterating through FixFields.
		/// </summary>
		/// <param name="iEnumerator">A hashtable enumerator from a FixMessage.</param>
		public FieldEnumerator(IEnumerator iEnumerator)
		{

			// Initialize the object
			this.iEnumerator = iEnumerator;

		}

		#region IEnumerator Members

		/// <summary>
		/// Resets the enumerator.
		/// </summary>
		public void Reset()
		{

			// Reset the underlying enumerator.
			this.iEnumerator.Reset();

		}

		/// <summary>
		/// The current FixField property.
		/// </summary>
		public object Current
		{
			get
			{

				// Convert the current dictionary entry in the underlying hashtable into a FixField.
				DictionaryEntry dictionaryEntry = (DictionaryEntry)iEnumerator.Current;
				return new Field((Tag)dictionaryEntry.Key, dictionaryEntry.Value);

			}

		}

		/// <summary>
		/// Moves to the next FixField.
		/// </summary>
		/// <returns>True when there are more items in the collection.</returns>
		public bool MoveNext()
		{

			// Move to the next element in the underlying hashtable.
			return this.iEnumerator.MoveNext();

		}

		#endregion

	}

}
