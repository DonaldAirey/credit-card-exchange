namespace FluidTrade.Core
{

    using System;
    using System.ComponentModel;
	using System.Configuration;
	using System.Diagnostics;
    using System.Net;
	using System.Net.Sockets;
	using System.Text;
	using System.Threading;

    /// <summary>FixMessageHandler</summary>
	public delegate void FixMessageHandler(FixMessage fixMessage);

	/// <summary>FixMessageEventHandler</summary>
	public delegate void FixMessageEventHandler(object sender, FixMessageEventArgs fixMessageEventArgs);

	/// <summary>FixSessionEventHandler</summary>
	public delegate void FixSessionEventHandler(object sender, FixSessionEventArgs fixSessionEventArgs);

	/// <summary>ResendRequest Delegate</summary>
	public delegate void MessageGapEventHandler(object sender, MessageGapEventArgs resendRequestEventArgs);

	/// <summary>
	/// A Communication FixSession to the Appia Server.
	/// </summary>
	public class FixSession : Component
	{

		// Default Constants
		public const int DefaultBufferSize = 0x200;
		public const int DefaultReasonableTransmissionTime = 5;
		public const int DefaultHeartbeatInterval = 0;
		public const int MessageDelay = 100;

		// Private Members
		private bool isReceiverThreadRunning;
		private bool isSenderThreadRunning;
		private bool isHeartbeatThreadRunning;
		private bool isConnected;
		private bool isLoggedIn;
		private bool isStillBeating;
		private int heartbeatInterval;
		private int reasonableTransmissionTime;
		private int testRequestCounter;
		private int inboundSequenceNumber;
		private int outboundSequenceNumber;
		private string sessionId;
		private string serverName;
		private int port;
		private string beginString;
		private string senderCompID;
		private string targetCompID;
		private FixMessageQueue fixMessageQueue;
		private System.Threading.Thread receiverThread;
		private System.Threading.Thread senderThread;
		private System.Threading.Thread heartbeatThread;
		private System.Threading.ManualResetEvent outgoingQueue;
		private System.Net.Sockets.TcpClient tcpClient;
		private System.Net.Sockets.NetworkStream networkStream;
		private static System.Diagnostics.EventLog eventLog;
		private FixMessageStore outboundMessageStore;
		private FixMessageStore inboundMessageStore;
		private FixMessageHandlerVector fixMessageHandlerVector;
		private TestRequestHandlerVector testRequestHandlerVector;

		// Public Members
		public string SessionId {get {return this.sessionId;} set {this.sessionId = value;}}

		/// <summary>Multicaster for Unrecognized Message Events.</summary>
		public FixMessageEventHandler UnrecognizedMessageEvent;

		/// <summary>Multicaster for Connect Events.</summary>
		public EventHandler ConnectEvent;

		/// <summary>Multicaster for Disconnect Events.</summary>
		public FixSessionEventHandler DisconnectEvent;

		/// <summary>Multicaster for Disconnect Events.</summary>
		public MessageGapEventHandler MessageGapEvent;

		/// <summary>Multicaster for Reject Events.</summary>
		public FixMessageEventHandler RejectEvent;

		/// <summary>Multicaster for Login Events.</summary>
		public FixMessageEventHandler LoginEvent;

		/// <summary>Multicaster for Logout Events.</summary>
		public FixMessageEventHandler LogoutEvent;

		/// <summary>Multicaster for Heartbeat Events.</summary>
		public FixMessageEventHandler HeartbeatEvent;

		/// <summary>Multicaster for TestRequest Events.</summary>
		public FixMessageEventHandler TestRequestEvent;

		/// <summary>Multicaster for ResendRequest Events.</summary>
		public FixMessageEventHandler ResendRequestEvent;

		/// <summary>Multicaster for SequenceReset Events.</summary>
		public FixMessageEventHandler SequenceResetEvent;

		/// <summary>Multicaster for OrderEvents.</summary>
		public FixMessageEventHandler OrderEvent;

		/// <summary>Multicaster for CancelEvents.</summary>
		public FixMessageEventHandler CancelEvent;

		/// <summary>Multicaster for CancelReplaceEvents.</summary>
		public FixMessageEventHandler CancelReplaceEvent;

		/// <summary>Multicaster for ExecutionReportEvents.</summary>
		public FixMessageEventHandler ExecutionReportEvent;

		/// <summary>Multicaster for SendMessageEvents.</summary>
		public FixMessageEventHandler SendMessageEvent;

		/// <summary>
		/// Static Constructor for a FixSession.
		/// </summary>
		static FixSession()
		{

			// Write critical errors to the event log.
			FixSession.eventLog = new System.Diagnostics.EventLog("Application", ".", "Appia FixSession");

		}

		/// <summary>
		/// Constructor for a FixSession.
		/// </summary>
		public FixSession(string sessionId, string beginString, string senderCompID, string targetCompID, string serverName, int port)
		{

			// The connection is not opened until a communication session has been established with the alternate party.
			this.IsConnected = false;
			this.IsLoggedIn = false;

			// This value is used to give each TestReq message a unique ID.
			this.testRequestCounter = 0;

			// Initialize the object.
			this.sessionId = sessionId;

			// This queue is used to pass outgoing messages to a central thread that handles outgoing TCP/IP communication with the
			// counterparty.
			this.fixMessageQueue = new FixMessageQueue();

			// These two data stores contain the persistent messages for a session.
			this.outboundMessageStore = new FixMessageStore();
			this.inboundMessageStore = new FixMessageStore();

			// These threads control the session level protocol.
			this.receiverThread = null;
			this.senderThread = null;
			this.heartbeatThread = null;

			// Set up the counterparties for this session.
			this.serverName = serverName;
			this.port = port;
			this.beginString = beginString;
			this.senderCompID = senderCompID;
			this.targetCompID = targetCompID;

			// Set the heartbeat interval from the application settings.  This value determines the period between the 'keep alive' messages
			// that are sent between the counterparties.
			string heartbeatIntervalString = ConfigurationManager.AppSettings["heartbeat"];
			this.heartbeatInterval = heartbeatIntervalString != null ? Convert.ToInt32(heartbeatIntervalString) :
				DefaultHeartbeatInterval;

			// The 'reasonable transmission time' is a term from the FIX specification.  It is a fudge factor for the heartbeat 
			// interval to account for the fact that the heartbeat messages may take some amount of time to reach the destination
			// after the negociated heartbeat interval has expired.
			string reasonableTransmissionTimeString = ConfigurationManager.AppSettings["reasonableTransmissionTime"];
			this.reasonableTransmissionTime = reasonableTransmissionTimeString != null ?
				Convert.ToInt32(reasonableTransmissionTimeString) :
				FixSession.DefaultReasonableTransmissionTime;

			// This event will suspend the outgoing messages until a login response has been retrieved from the counterparty during
			// the session initialization.
			this.outgoingQueue = new ManualResetEvent(false);

			// The 'TestRequest' message will generate a heartbeat from the counterparty with a 'TestReqID' as a tag.  This 
			// 'TestReqID' can be used to confirm a specific round trip event.  This is a hash table of 'TestReqID's paired with
			// handlers for those round trip events.  For example, on login, a 'TestRequest' message is sent to the counterparty
			// with a 'TestReqID'. When a heartbeat is returned with the specified 'TestReqID', the login procedure knows that a
			// round trip was completed and all messages in each buffer -- incoming and outgoing -- have been flushed.
			this.testRequestHandlerVector = new TestRequestHandlerVector();

			// Create a table of vectors for quickly parsing the message types out of the receiving stream and calling the methods
			// that handle each message type.
			this.fixMessageHandlerVector = new FixMessageHandlerVector();
			this.fixMessageHandlerVector.DefaultMessageHandler = new FixMessageHandler(UnrecognizedFixMessageHandler);
			this.fixMessageHandlerVector[MsgType.Reject] = new FixMessageHandler(RejectFixMessageHandler);
			this.fixMessageHandlerVector[MsgType.Login] = new FixMessageHandler(LoginFixMessageHandler);
			this.fixMessageHandlerVector[MsgType.Logout] = new FixMessageHandler(LogoutFixMessageHandler);
			this.fixMessageHandlerVector[MsgType.Heartbeat] = new FixMessageHandler(HeartbeatFixMessageHandler);
			this.fixMessageHandlerVector[MsgType.TestRequest] = new FixMessageHandler(TestRequestFixMessageHandler);
			this.fixMessageHandlerVector[MsgType.ResendRequest] = new FixMessageHandler(ResendRequestFixMessageHandler);
			this.fixMessageHandlerVector[MsgType.SequenceReset] = new FixMessageHandler(SequenceResetFixMessageHandler);
			this.fixMessageHandlerVector[MsgType.ExecutionReport] = new FixMessageHandler(ExecutionReportFixMessageHandler);
			this.fixMessageHandlerVector[MsgType.Order] = new FixMessageHandler(OrderFixMessageHandler);
			this.fixMessageHandlerVector[MsgType.OrderCancelRequest] = new FixMessageHandler(CancelFixMessageHandler);
			this.fixMessageHandlerVector[MsgType.OrderCancelReplaceRequest] = new FixMessageHandler(CancelReplaceFixMessageHandler);

		}

		/// <summary>
		/// Indicates whether the communication session is open.
		/// </summary>
		public bool IsConnected
		{
			get {lock (this) return this.isConnected;}
			set {lock (this) this.isConnected = value;}
		}

		/// <summary>
		/// Indicates whether the session is logged in or not.
		/// </summary>
		public bool IsLoggedIn
		{
			get {lock (this) return this.isLoggedIn;}
			set {lock (this) this.isLoggedIn = value;}
		}

		/// <summary>
		/// Indicates whether the session is logged in or not.
		/// </summary>
		public bool IsStillBeating
		{
			get {lock (this) return this.isStillBeating;}
			set {lock (this) this.isStillBeating = value;}
		}

		/// <summary>
		/// Thread-safe access to the flag that controls the receiver thread.
		/// </summary>
		public bool IsReceiverThreadRunning
		{
			get {lock (this) return this.isReceiverThreadRunning;}
			set {lock (this) this.isReceiverThreadRunning = value;}
		}

		/// <summary>
		/// Thread-safe access to the flag that controls the sender thread.
		/// </summary>
		public bool IsSenderThreadRunning
		{
			get {lock (this) return this.isSenderThreadRunning;}
			set {lock (this) this.isSenderThreadRunning = value;}
		}

		/// <summary>
		/// Thread-safe access to the flag that controls the receiver thread.
		/// </summary>
		public bool IsHeartbeatThreadRunning
		{
			get {lock (this) return this.isHeartbeatThreadRunning;}
			set {lock (this) this.isHeartbeatThreadRunning = value;}
		}

		/// <summary>
		/// Open a bi-directional communication stream to the server.
		/// </summary>
		/// <param name="serverName">The IP Address of the target computer.</param>
		/// <param name="port">The port for the Appia Server.</param>
		public void Open()
		{

			// Open up the message stores for this session.
			// Initialize the sequence numbers from the message stores.
			if ( !this.inboundMessageStore.IsOpen )
			{
				this.inboundMessageStore.Open(string.Format("{0}_inbound", this.sessionId));
				this.ResetIncomingSequence();
			}
			if ( !this.outboundMessageStore.IsOpen )
			{
				this.outboundMessageStore.Open(string.Format("{0}_outbound", this.sessionId));
				this.ResetOutgoingSequence();
			}

			// Us the DNS services to translate the server name into an IP address.
			IPHostEntry hostEntry = Dns.GetHostEntry(this.serverName);
			if (hostEntry.AddressList.Length == 0)
				throw new Exception(string.Format("The address of the FIX Server '{0}' can't be found.", this.serverName));

			// Create a new TCP client for the fixSession.
			this.tcpClient = new TcpClient();
			this.tcpClient.Connect(hostEntry.AddressList[0], this.port);

			// This fixSession is now open for business.
			this.IsConnected = true;

			// Create a network stream for reading and writing to the socket.
			this.networkStream = this.tcpClient.GetStream();

			// This thread will listen for incoming messages from the server.
			this.receiverThread = new Thread(new ThreadStart(ReceiverThread));
			this.receiverThread.Start();

			// This thread will listen for incoming messages from the server.
			this.senderThread = new Thread(new ThreadStart(SenderThread));
			this.senderThread.Start();

			// Send the login message.
			this.SendLogin();

		}

		/// <summary>
		/// Sends a login message.
		/// </summary>
		private void SendLogin()
		{

			FixMessage loginMessage = new FixMessage();
			loginMessage.MsgType = MsgType.Login;
			loginMessage.EncryptMethod = EncryptMethod.None;
			loginMessage.HeartBtInt = this.heartbeatInterval;
			SendMessage(loginMessage);

		}

		/// <summary>
		/// Closes a fixSession to the FIX Server.
		/// </summary>
		public void Close()
		{

			// Don't attempt to close the stream and terminate the receiver thread if the fixSession has already been closed.
			if (this.IsConnected)
			{

				FixSession.eventLog.WriteEntry(string.Format("Closing Session '{0}' at {1}.", this.sessionId, DateTime.Now));

				// Don't attempt the logout sequence if the session is already logged out.
				if (this.IsLoggedIn)
				{
					
					// This will flush the line and insure that we've received and processed any resequencing messages before the
					// session is terminated. The vector 'LogoutTestRequestHandler' will finish the logout procedure after any message
					// gaps have been filled and the output queue has been emptied.
					string testReqID = string.Format("TestRequest{0}", this.IncrementTestRequestCounter());
					this.testRequestHandlerVector[testReqID] = new EventHandler(LogoutTestRequestHandler);
				
					// Transmit a TestReq message.
					FixMessage testRequestMessage = new FixMessage();
					testRequestMessage.MsgType = MsgType.TestRequest;
					testRequestMessage.TestReqID = testReqID;
					PostMessage(testRequestMessage);

				}

			}

		}

		/// <summary>
		/// Threadsafe method to update the counter used by the TestRequest messages.
		/// </summary>
		/// <returns>A unique counter.</returns>
		private int IncrementTestRequestCounter()
		{

			// Lock the thread while the counter is incremented.
			lock (this) {return ++this.testRequestCounter;}

		}

		/// <summary>
		/// Handles incoming packets from the FIX counterparty.
		/// </summary>
		private void ReceiverThread()
		{

			// This flag controls the execution of this thread.
			this.IsReceiverThreadRunning = true;

			// Buffer for reading data
			Byte[] buffer = new Byte[DefaultBufferSize];

			// This flag is used to detect a garbled message, which should be ignored after it's been read.
			bool isGarbled = false;

			// This is used to insure that the tags come in the right order.
			int fieldCounter = 0;

			// The body of the message is between the first two tags and the checksum tag.
			int bodyStart = 0;
			int bodyLength = 0;

			// As the incoming message is parsed, these values keep track of the location of the current tag.
			int fieldStart = 0;
			int fieldLength = 0;

			// There may be one or more messages in a buffer read from the counterparty.  These variables keep track
			// of a message as it is parsed out of the buffer.
			int packageStart = 0;
			int packageLength = 0;

			// This string is used to collect the messages as they're read in from the buffer.  As the incoming buffer is read, the
			// bytes are placed in this string and it is parsed.  When a complete message is read, it is removed from the beginning
			// of this string and the parsing continues.
			string package = string.Empty;

			// Tags are read and validated.  When the checksum tag is read, the message is used to create a FixMessage.
			FixMessage fixMessage;

			// Enter the listening loop.
			while(this.IsReceiverThreadRunning)
			{

				// This will insure that the thread survives any errors while processing the buffer.
				try
				{

					// This will block here until a complete buffer of data is read from the socket stream.
					int bytesRead = this.networkStream.Read(buffer, 0, buffer.Length);

					// This will reset the heartbeat timer.  If no messages are received after a defined interval, a 'TestReq'
					// message will be generated to see if the counterparty is still alive.
					this.IsStillBeating = true;

					// Pull apart multiple messages that may be in the stream and process each message.
					package += Encoding.ASCII.GetString(buffer, 0, bytesRead);

					// Parse all the tags in the current buffer of data.  When a checksum tag is read, the message is considered to
					// be complete and will generate an event.
					while (fieldStart + fieldLength < package.Length)
					{

						// Keep scanning the characters in the current buffer looking for a tagged delimiter termination character.
						if (package[fieldStart + fieldLength] != FixMessage.FixSeparator)
							fieldLength++;
						else
						{

							// When a complete tagged delimiter is circumscribed, read it out of the buffer.
							string field = package.Substring(fieldStart, fieldLength);

							// After a tagged delimiter is read, prepare to read the next one.
							fieldStart += fieldLength + 1;
							fieldLength = 0;

							// The package length keeps track of the total bytes in the package of tagged delimiters.
							packageLength += field.Length + 1;

							// The tagged delimiter is constructed from a simple value and key combination.  The message takes the
							// form of '<tag>=<value>' in ASCII characters.  This will pull apart the tag into the key and value
							// parts.
							string[] values = field.Split(new char[] {'='});

							// Convert the text to a Tag.
							Tag tag = TagConverter.ConvertFrom(values[0]);

							// Validation Check #1: Begin String
							if (tag == Tag.BeginString && fieldCounter != 0)
								isGarbled = true;

							// Validation Check #2: BodyLength
							if (tag == Tag.BodyLength && fieldCounter != 1)
								isGarbled = true;

							// If the field is a checksum, process the message and reset the buffer pointers.
							if (tag == Tag.CheckSum)
							{
								int passedInCheckSum = (int)FixMessage.Parse(tag, values[1]);

								// The message checksum is the least 16 bits of the sum of the characters in the body of the
								// message.  Note that the begining and length of the body are computed from the 'BodyLength' 
								// field.
								int checkSum = 0;
								int checkSumIndex = bodyStart + bodyLength;
								for (int index = 0; index < checkSumIndex; index++)
									checkSum += package[index];
								checkSum &= 0xFF;

								// Validation Check #3: Checksum
								if (checkSum != passedInCheckSum)
									isGarbled = true;

								// If the computed checksum is the same as the one passed in the FIX message, then the message is
								// valid and it can be used to generate an event.
								if (!isGarbled)
								{
									// load the message into the FixMessage object.
									fixMessage = new FixMessage(package.Substring(packageStart, packageLength));
									Debug.WriteLine(string.Format("Received FIX Message {0}", fixMessage.ToString()));


									bool possDupFlag = fixMessage.PossDupFlag;
									int expectedSequenceNumber = this.inboundSequenceNumber + 1;

									if (fixMessage.PossDupFlag)
										FixSession.eventLog.WriteEntry(string.Format("Session '{0}' - Received Poss Dup Message #{1}, Expecting #{2}", this.sessionId, fixMessage.MsgSeqNum, expectedSequenceNumber), EventLogEntryType.Warning);
									else
										// Debug message
										Debug.WriteLine(string.Format("Received Message #{0}, Expecting #{1}", fixMessage.MsgSeqNum, expectedSequenceNumber));

									if (fixMessage.MsgSeqNum == expectedSequenceNumber)
									{
										// If its a possDup and we already have it, skip it. Otherwise, process it.
										if (!possDupFlag || this.inboundMessageStore[this.inboundSequenceNumber] == null)
										{

											// The message is committed to the persistent data store.
											this.inboundMessageStore[fixMessage.MsgSeqNum-1] = fixMessage;

											// The FIX message type is used to jump to the right method for handling this message.
											this.fixMessageHandlerVector[fixMessage.MsgType](fixMessage);

										}

										this.inboundSequenceNumber = expectedSequenceNumber;

									}
									else
									{

										//  This is where the logic to check for message gaps should be placed.
										if (fixMessage.MsgSeqNum < expectedSequenceNumber && !fixMessage.PossDupFlag)
											CriticalSequenceError(fixMessage.MsgSeqNum, expectedSequenceNumber);
										else
											OnMessageGap(new MessageGapEventArgs(this.sessionId, expectedSequenceNumber, fixMessage.MsgSeqNum));

									}

								}
								else
									FixSession.eventLog.WriteEntry(string.Format("Ignored a garbled message for Session '{0}'.", this.sessionId), EventLogEntryType.Error);


								// At this point, a complete message was read.  If it was valid, it was also passed along to a
								// message handler.  Now is the time to reset the buffer values to parse another message.
								fixMessage = null;

								// Any partial packets are used as the start of the next message.
								package = package.Substring(packageStart + packageLength);

								// The previous message has been removed from the buffer, so reset the buffer pointers.
								isGarbled = false;
								fieldCounter = 0;
								fieldStart = 0;
								fieldLength = 0;
								packageStart = 0;
								packageLength = 0;

							}
							else
							{

								// Process the next field in the message.
								fieldCounter++;

								// The body length field is used to check the consistency of the message.  It counts up the number
								// of message bytes from the second field to the checksum field.  That is, the checksum field can
								// be found exactly this many bytes from the 'BodyLength' field.
								if (tag == Tag.BodyLength)
								{
									bodyStart = packageLength;
									bodyLength = Int32.Parse(values[1]);
								}

							}

						}

					}

				}
				catch(ThreadAbortException)
				{
					Thread.ResetAbort();
				}
				catch (Exception exception)
				{

					// Ignore the exception if the session is disconnected.  It most likely came from the termination of the 
					// thread.
					if (this.IsConnected)
					{
						// Write unexpected errors to the log.
						FixSession.eventLog.WriteEntry(string.Format("Error in session '{0}': {1}", this.sessionId, exception.ToString()), EventLogEntryType.Error);

						// Since termination of the communication was unexpected, close out the session.
						string msg = (exception.InnerException != null) ? exception.Message + ": " + exception.InnerException.Message: exception.Message ;
						OnDisconnect(new FixSessionEventArgs(this.sessionId, msg));

					}

				}

			}

		}

		private void CriticalSequenceError(int receivedMsgSeqNum, int expectedMsgSeqNum)
		{
			string msg = string.Format("CriticalSequenceError:  Message sequence number: {0} is less than expected sequence number: {1}", receivedMsgSeqNum, expectedMsgSeqNum);

			if (this.IsLoggedIn)
			{
				// If logged in, logout.
				FixMessage logoutMessage = new FixMessage();
				logoutMessage.MsgType = MsgType.Logout;
				logoutMessage.Text = msg;
					
				PostMessage(logoutMessage);
			}

			// Clean up the session.
			OnDisconnect(new FixSessionEventArgs(this.sessionId, msg));

		}

		/// <summary>
		/// Sends FIX messages to the counterpary.
		/// </summary>
		private void SenderThread()
		{

			// This flag can be used to terminate the execution of this thread.
			this.IsSenderThreadRunning = true;

			// Keep on running until this flag is cleared from an outside thread.
			while (this.IsSenderThreadRunning)
			{

				// Wait for the login message to be returned before sending the rest of the messages
				this.outgoingQueue.WaitOne();

				// Wait for a message to be available on the queue and remove it.
				SendMessage(this.fixMessageQueue.Dequeue());

			}

		}

		/// <summary>
		/// Watchdog thread for keeping the communication session open.
		/// </summary>
		private void HeartbeatThread()
		{

			int previousInboundSequenceNumber = this.inboundSequenceNumber;

			// This flag controls the execution of this thread.
			this.IsHeartbeatThreadRunning = true;

			while (this.IsHeartbeatThreadRunning)
			{
				Thread.Sleep((this.heartbeatInterval + this.reasonableTransmissionTime) * 1000);

				if (!this.IsStillBeating)
					Close();

				this.IsStillBeating = false;

				// check to see if the sequence number has changed.
				if (previousInboundSequenceNumber == this.inboundSequenceNumber)
				{

					// If there has been no change, transmit a Heartbeat message.
					FixMessage heartbeatMessage = new FixMessage();
					heartbeatMessage.MsgType = MsgType.Heartbeat;
					PostMessage(heartbeatMessage);

				}

				// save the sequence number for comparison after the sleep.
				previousInboundSequenceNumber = this.inboundSequenceNumber;

			}

		}

		/// <summary>
		/// Completes the Login sequence.  Called after the buffers have been flushed.
		/// </summary>
		/// <param name="sender">The object that generated the event.</param>
		/// <param name="eventArgs">The event arguments.</param>
		public void LoginTestRequestHandler(object sender, EventArgs eventArgs)
		{

			// The outgoing message queue is suspended until the counterparty confirms the login and the 'Send' logic has
			// sorted out the gaps.
			this.outgoingQueue.Set();

		}
	
		/// <summary>
		/// Completes the Logout sequence.  Called after the buffers have been flushed.
		/// </summary>
		/// <param name="sender">The object that generated the event.</param>
		/// <param name="eventArgs">The event arguments.</param>
		public void LogoutTestRequestHandler(object sender, EventArgs eventArgs)
		{

			// At this point, the buffers have been flushed an any resequencing has completed.  It is time to transmit a Logout
			// message.
			FixMessage logoutMessage = new FixMessage();
			logoutMessage.MsgType = MsgType.Logout;
			PostMessage(logoutMessage);

			// At this point, we're no longer logged in, though we're still expecting the final logout message from the
			// counterparty.
			this.IsLoggedIn = false;

		}

		/// <summary>
		/// Closes a fixSession to the FIX Server.
		/// </summary>
		public void OnDisconnect(FixSessionEventArgs fixSessionEventArgs)
		{

			// Broadcast the event to any object that has subscribed to this event.
			if (this.DisconnectEvent != null)
				this.DisconnectEvent(this, fixSessionEventArgs);

			// Don't attempt to close the stream and terminate the receiver thread if the fixSession has already been closed.
			if (this.IsConnected)
			{

				FixSession.eventLog.WriteEntry(string.Format("Session '{0}' Disconnected at {1}.", this.sessionId, DateTime.Now));

				// This will indicate that the fixSession is closed.  The fixSession can be re-used by opening it again in the
				// future.
				this.IsConnected = false;

				try
				{

					// Shut down the reciever thread.
					if (this.receiverThread != null)
					{

						this.IsReceiverThreadRunning = false;

						if (this.receiverThread != Thread.CurrentThread)
						{
							this.receiverThread.Abort();
							this.receiverThread = null;
						}

					}
			
					// Shut down the sending thread.
					if (this.senderThread != null)
					{
						this.IsSenderThreadRunning = false;
						this.senderThread.Abort();
						this.senderThread = null;
					}

					// Shut down the heartbeat thread.
					if (this.heartbeatThread != null)
					{
						this.IsHeartbeatThreadRunning = false;
						this.heartbeatThread.Abort();
						this.heartbeatThread = null;
					}

					// Close the network stream and TCP Client.
					this.networkStream.Close();
					this.tcpClient.Close();

				}
				catch (Exception exception)
				{

					FixSession.eventLog.WriteEntry(string.Format("Error in session '{0}': {1}", this.sessionId, exception.ToString()), EventLogEntryType.Error);

				}
			
				// Close out the persistent data store
				this.outboundMessageStore.Close();
				this.inboundMessageStore.Close();

			}

		}

		private void OnMessageGap(MessageGapEventArgs messageGapEventArgs)
		{

			// Broadcast the event to any object that has subscribed to this event.
			if (this.MessageGapEvent != null)
				this.MessageGapEvent(this, messageGapEventArgs);

			if (!messageGapEventArgs.IsHandled)
			{
				if (this.IsLoggedIn)
				{

					FixMessage messageGapMessage = new FixMessage();
					messageGapMessage.MsgType = MsgType.ResendRequest;
					messageGapMessage.BeginSeqNo = messageGapEventArgs.BeginSeqNo;
					// Setting EndSeqNo to zero indicates "send all messages since the BeginSeqNo"
					messageGapMessage.EndSeqNo = 0;
					SendMessage(messageGapMessage);
				}
				else
				{
					// Shut down the session.
					OnDisconnect(new FixSessionEventArgs(this.sessionId, "Message Gap occurred before Login."));

				}

			}

		}

		private void SendMessage(FixMessage fixMessage)
		{

			// This method can be called by any thread.  A high-priority message will skip the message queue and try to write
			// directly to the counterparty.  This will keep the queued up messages from confusing the high-priority messages.
			lock (this)
			{
				// if this is a new message, bump up the sequence number.
				if ( !fixMessage.Contains(Tag.MsgSeqNum) )
					fixMessage.MsgSeqNum = this.outboundSequenceNumber++;

				// Affix the target and sender counterparties.
				fixMessage.BeginString = this.beginString;
				fixMessage.SenderCompID = this.senderCompID;
				fixMessage.TargetCompID = this.targetCompID;

				// Set the sending time time on this message.
				fixMessage.SendingTime = DateTime.Now.ToUniversalTime();

				// Encode the packet in a byte stream.
				byte[] packet = fixMessage.ToPacket();

				// Finally, send the encoded packet to the counterparty.
				this.networkStream.Write(packet, 0, packet.Length);

				// Write the original encoded packet to the persistent data store.
				this.outboundMessageStore[fixMessage.MsgSeqNum] = fixMessage;

				// Broadcast the event to any object that has subscribed to this event.
				if (this.SendMessageEvent != null)
				{
					FixMessageEventArgs fixMessageEventArgs = new FixMessageEventArgs(this.SessionId, fixMessage);
					this.SendMessageEvent(this, fixMessageEventArgs);
				}

			}

		}

		/// <summary>
		/// Writes a message to the Appia Server.
		/// </summary>
		/// <param name="message">The message to be sent to the Appia server.</param>
		public void PostMessage(FixMessage fixMessage)
		{

			this.fixMessageQueue.Enqueue(fixMessage);

		}

		/// <summary>
		/// This method will broadcast the Unrecognized Message Event to all listeners.
		/// </summary>
		/// <param name="message">Arguments for the event.</param>
		public virtual void OnUnrecognizedFixMessageEvent(FixMessageEventArgs fixMessageEventArgs)
		{

			// Broadcast the event to any object that has subscribed to this event.
			if (this.UnrecognizedMessageEvent != null)
				this.UnrecognizedMessageEvent(this, fixMessageEventArgs);

		}

		/// <summary>
		/// This method will broadcast the Reject Events to all listeners.
		/// </summary>
		/// <param name="message">Arguments for the event.</param>
		public virtual void OnRejectFixMessageEvent(FixMessageEventArgs fixMessageEventArgs)
		{

			// Broadcast the event to any object that has subscribed to this event.
			if (this.RejectEvent != null)
				this.RejectEvent(this, fixMessageEventArgs);

		}

		/// <summary>
		/// This method will broadcast the Login Events to all listeners.
		/// </summary>
		/// <param name="message">Arguments for the event.</param>
		public virtual void OnLoginFixMessageEvent(FixMessageEventArgs fixMessageEventArgs)
		{

			// Broadcast the event to any object that has subscribed to this event.
			if (this.LoginEvent != null)
				this.LoginEvent(this, fixMessageEventArgs);

			// The session is logged in now.
			this.IsLoggedIn = true;

			// This is the default action for a login message that isn't handled by a listener.
			if (!fixMessageEventArgs.IsHandled)
			{

				// The heartbeat interval is negociated with the counterparty.  The 'Login' message requests a heartbeat, but the
				// counterpart can resond with a different value.  For example, if zero is sent out, suggesting that the heartbeat
				// be disabled, the counterparty can respond with a default value of 30, indicating 'No Deal' and telling this part
				// of the session that it must use the heartbeat.  This will pull the counterparty's finaly offer from the 'Login'
				// message and use it to set up the heartbeat monitor.
				this.heartbeatInterval = fixMessageEventArgs.FixMessage.HeartBtInt;

				// The heartbeat thread will monitor the heartbeats and provide a sanity check for lost communications.  A
				// heartbeat interval of zero indicates that the heartbeat check should be disabled.
				if (this.heartbeatInterval != 0)
				{
					this.heartbeatThread = new Thread(new ThreadStart(HeartbeatThread));
					this.heartbeatThread.Start();
				}
				else
					this.heartbeatThread = null;

				// This will flush the line and insure that we've received and processed any resequencing messages before the
				// normal message queue is opened up for traffic.  When this test request packet is returned (in the form of a
				// heartbeat), the vector 'LoginTestRequestHandler' will finish the login procedure by opening up the message queue
				// for normal traffic.
				string testReqID = string.Format("TestRequest{0}", this.IncrementTestRequestCounter());
				this.testRequestHandlerVector[testReqID] = new EventHandler(LoginTestRequestHandler);
				
				// Transmit a TestReq message.
				FixMessage testRequestMessage = new FixMessage();
				testRequestMessage.MsgType = MsgType.TestRequest;
				testRequestMessage.TestReqID = testReqID;
				SendMessage(testRequestMessage);

			}

		}

		/// <summary>
		/// This method will broadcast the Logout Events to all listeners.
		/// </summary>
		/// <param name="message">Arguments for the event.</param>
		public virtual void OnLogoutFixMessageEvent(FixMessageEventArgs fixMessageEventArgs)
		{

			// Broadcast the event to any object that has subscribed to this event.
			if (this.LogoutEvent != null)
				this.LogoutEvent(this, fixMessageEventArgs);

			if (this.IsLoggedIn)
			{

				// The session is no longer logged in.
				this.IsLoggedIn = false;

				// The default action for the logout message is to respond with a logout message in kind
				if (!fixMessageEventArgs.IsHandled)
				{

					// Transmit a Logout message.
					FixMessage logoutMessage = new FixMessage();
					logoutMessage.MsgType = MsgType.Logout;
					PostMessage(logoutMessage);

				}

			}

			// If we were logged in, then this message came from the counterparty as a logout request.
			// Or, if we just sent the logout to the counterparty, then this is the reply.
			// Or FIX message may have come as a response to our Login message and may have some error info in the Text field.
			// In any case, shut down the connection.
			OnDisconnect(new FixSessionEventArgs(this.sessionId, "Logout complete."));

		}

		/// <summary>
		/// This method will broadcast the Heartbeat Events to all listeners.
		/// </summary>
		/// <param name="message">Arguments for the event.</param>
		public virtual void OnHeartbeatFixMessageEvent(FixMessageEventArgs fixMessageEventArgs)
		{

			// Broadcast the event to any object that has subscribed to this event.
			if (this.HeartbeatEvent != null)
				this.HeartbeatEvent(this, fixMessageEventArgs);

			// This is the default action for a heartbeat message.  If a heartbeat isn't recieved in the agreed upon interval, the 
			// socket connection will be shut down and restarted. If this flag is clear when the heart monitor thread wakes up, it
			// will indicate that the initiator of this conversation has locked up and stopped sending heartbeats.
			if (!fixMessageEventArgs.IsHandled)
			{

				// The heartbeat message can also be used as a response from an explicit TestReq message.  In these exchanges, the
				// heartbeat will have an additional tag that contains the original 'TestReqID' string sent.  This string can be 
				// used to find a method to be executed when the return trip is complete.
				object testReqID = fixMessageEventArgs.FixMessage[Tag.TestReqID];
				if (testReqID != null)
				{

					// If a vector has been created for this 'TestReqID', then execute it.  These vectors are one-time only 
					// events, so it should be purged from the vector table when it's finished.
					EventHandler eventHandler = this.testRequestHandlerVector[(string)testReqID];
					if (eventHandler != null)
					{
						eventHandler(this, new EventArgs());
						this.testRequestHandlerVector.Remove((string)testReqID);
					}

				}

			}

		}

		/// <summary>
		/// This method will broadcast the TestRequest Events to all listeners.
		/// </summary>
		/// <param name="message">Arguments for the event.</param>
		public virtual void OnTestRequestFixMessageEvent(FixMessageEventArgs fixMessageEventArgs)
		{

			// Broadcast the event to any object that has subscribed to this event.
			if (this.TestRequestEvent != null)
				this.TestRequestEvent(this, fixMessageEventArgs);

			// If the message isn't handled by one of the listeners, then respond with a heartbeat.
			if (!fixMessageEventArgs.IsHandled)
			{

				// Respond to the test request with a heartbeat that includes the TestReqID, which is a text that should uniquely
				// identify the request for a heartbeat.
				FixMessage fixMessage = new FixMessage();
				fixMessage.MsgType = MsgType.Heartbeat;
				fixMessage.TestReqID = fixMessageEventArgs.FixMessage.TestReqID;
				PostMessage(fixMessage);

			}

		}

		/// <summary>
		/// This method will broadcast the ResendRequest Events to all listeners.
		/// </summary>
		/// <param name="message">Arguments for the event.</param>
		public virtual void OnSequenceResetFixMessageEvent(FixMessageEventArgs fixMessageEventArgs)
		{

			// Broadcast the event to any object that has subscribed to this event.
			if (this.ResendRequestEvent != null)
				this.ResendRequestEvent(this, fixMessageEventArgs);

			// The default action is to reset the incoming sequence number.
			if (!fixMessageEventArgs.IsHandled)
				this.inboundSequenceNumber = fixMessageEventArgs.FixMessage.NewSeqNo;

			// Debug Message.
			FixSession.eventLog.WriteEntry(string.Format("Session '{0}' - New Sequence Number #{1}", this.sessionId, this.inboundSequenceNumber));

		}
			
		/// <summary>
		/// This method will broadcast the ResendRequest Events to all listeners.
		/// </summary>
		/// <param name="message">Arguments for the event.</param>
		public virtual void OnResendRequestFixMessageEvent(FixMessageEventArgs fixMessageEventArgs)
		{

			// Broadcast the event to any object that has subscribed to this event.
			if (this.ResendRequestEvent != null)
				this.ResendRequestEvent(this, fixMessageEventArgs);

			// This is the default processing for the ResendRequest message.
			if (!fixMessageEventArgs.IsHandled)
			{

				// Suspend the normal outgoing messages until the resequencing is complete.
				this.outgoingQueue.Reset();

				// The main idea behind handling the request for resend is to cycle through the message store looking for all the
				// messages in the requested range.  The original message from the counterparty has the range of missing messages.
				// Note that the FIX message sequences starts at 1 where the internal message store is offset from zero.  The upper
				// bound of the missing messages can either be explicitly stated from the counterparty, or more commonly, it will 
				// be zero which is the predefined symbol for 'everything'.
				FixMessage fixMessage = fixMessageEventArgs.FixMessage;
				int beginSequenceNumber = fixMessage.BeginSeqNo - 1;
				int endSequenceNumber = fixMessage.EndSeqNo == 0 ? this.outboundSequenceNumber - 1 : fixMessage.EndSeqNo - 1;

				// Admin messages are not transmitted.  Instead, a message sequence gap is sent which saves a lot of time and 
				// confusion since most of the message exchanged are heartbeats and 'Test Request' messages.  These variables keep
				// track of where the gap begins and when to to issue a gap fill message.
				bool isMessageGap = false;
				int beginGapNumber = int.MinValue;

				// Resend the messages requested by the counterparty.
				for (int sequenceNumber = beginSequenceNumber; sequenceNumber <= endSequenceNumber; sequenceNumber++)
				{

					// Extract the message from the message store.
					FixMessage outgoingMessage = this.outboundMessageStore[sequenceNumber];

					if ( outgoingMessage == null)
					{
						isMessageGap = true;
						beginGapNumber = sequenceNumber;
					}
					else
					{

						// Use the message type to determine the resend action.
						switch (outgoingMessage.MsgType)
						{

							case MsgType.Login:
							case MsgType.Logout:
							case MsgType.Heartbeat:
							case MsgType.TestRequest:
							case MsgType.ResendRequest:
							case MsgType.SequenceReset:

								// Admin messages will generate a 'Sequence Reset' message rather than a retransmission of the original
								// message.  'beginGapNumber' keeps track of the starting point for the gap.
								if (!isMessageGap)
								{
									isMessageGap = true;
									beginGapNumber = sequenceNumber;
								}

								break;

							default:

								// Before resending the message, check to see if a message gap was generated from previous admin messages
								// in the message store.  If messages were surpressed, then generate a 'SequenceReset' message to fill in
								// the gap.
								if (isMessageGap)
								{

									// Create a message to reset the sequence.  Note the FIX messages are indexed from 1 and that the next
									// expected sequence number is the current sequence number.  This is the sequence number of the current
									// message that was read from the store.
									FixMessage gapFillMessage = new FixMessage();
									gapFillMessage.MsgType = MsgType.SequenceReset;
									gapFillMessage.MsgSeqNum = beginGapNumber + 1;
									gapFillMessage.GapFillFlag = true;
									gapFillMessage.NewSeqNo = sequenceNumber + 1;
									SendMessage(gapFillMessage);

									// Clear the gap until the next admin message is read from the message store.
									isMessageGap = false;

								}
						
								// Resend the message with the 'Possible Duplication' flag on.  Remove the 'BodyLength' and 'Checksum' tags
								// so they can be recalcualted when the message is sent.  Other than that, the message is the original one.
								outgoingMessage.PossDupFlag = true;
								outgoingMessage.OrigSendingTime = outgoingMessage.SendingTime;
								outgoingMessage.Remove(Tag.BodyLength);
								outgoingMessage.Remove(Tag.CheckSum);
								outgoingMessage.MsgSeqNum = sequenceNumber + 1;
								SendMessage(outgoingMessage);

								break;

						}
					}

				}

				// After the messages have been resent, check to see if there is a gap to be filled.
				if (isMessageGap)
				{

					// Send a message to fill the gap.  Note the 'NewSeqNo' must point to the next that will be sent.  Note also 
					// that the FIX messages are indexed from one instead of zero, giving a final offset of '2' to the sequence
					// number.
					FixMessage gapFillMessage = new FixMessage();
					gapFillMessage.MsgType = MsgType.SequenceReset;
					gapFillMessage.MsgSeqNum = beginGapNumber + 1;
					gapFillMessage.GapFillFlag = true;
					gapFillMessage.NewSeqNo = endSequenceNumber + 2;
					SendMessage(gapFillMessage);

				}

				// The stream should be synchronized at this point.  The normal messages in the queue can be sent to the
				// counterparty at this point.
				this.outgoingQueue.Set();

			}

		}

		/// <summary>
		/// This method will broadcast the CancelEvent to all listeners.
		/// </summary>
		/// <param name="message">Arguments for the event.</param>
		public virtual void OnCancelFixMessageEvent(FixMessageEventArgs fixMessageEventArgs)
		{

			// Broadcast the event to any object that has subscribed to this event.
			if (this.CancelEvent != null)
				this.CancelEvent(this, fixMessageEventArgs);

		}

		/// <summary>
		/// This method will broadcast the CancelReplaceEvent to all listeners.
		/// </summary>
		/// <param name="message">Arguments for the event.</param>
		public virtual void OnCancelReplaceFixMessageEvent(FixMessageEventArgs fixMessageEventArgs)
		{

			// Broadcast the event to any object that has subscribed to this event.
			if (this.CancelReplaceEvent != null)
				this.CancelReplaceEvent(this, fixMessageEventArgs);

		}

		/// <summary>
		/// This method will broadcast the Order Event to all listeners.
		/// </summary>
		/// <param name="message">Arguments for the event.</param>
		public virtual void OnOrderFixMessageEvent(FixMessageEventArgs fixMessageEventArgs)
		{

			// Broadcast the event to any object that has subscribed to this event.
			if (this.OrderEvent != null)
				this.OrderEvent(this, fixMessageEventArgs);

		}

		/// <summary>
		/// This method will broadcast the ExecutionReport Event to all listeners.
		/// </summary>
		/// <param name="message">Arguments for the event.</param>
		public virtual void OnExecutionReportFixMessageEvent(FixMessageEventArgs fixMessageEventArgs)
		{

			// Broadcast the event to any object that has subscribed to this event.
			if (this.ExecutionReportEvent != null)
				this.ExecutionReportEvent(this, fixMessageEventArgs);

		}

		/// <summary>
		/// Handles unrecognized messages.
		/// </summary>
		private void UnrecognizedFixMessageHandler(FixMessage fixMessage)
		{

			// Broadcast the unrecognized message to any listeners.
			OnUnrecognizedFixMessageEvent(new FixMessageEventArgs(this.sessionId, fixMessage));

		}
		
		/// <summary>
		/// Handles rejected messages.
		/// </summary>
		private void RejectFixMessageHandler(FixMessage fixMessage)
		{

            OnRejectFixMessageEvent(new FixMessageEventArgs(this.sessionId, fixMessage));

		}
		
		/// <summary>
		/// Handles Login messages.
		/// </summary>
		private void LoginFixMessageHandler(FixMessage fixMessage)
		{

			OnLoginFixMessageEvent(new FixMessageEventArgs(this.sessionId, fixMessage));

		}
		
		/// <summary>
		/// Handles Logout messages.
		/// </summary>
		private void LogoutFixMessageHandler(FixMessage fixMessage)
		{

			OnLogoutFixMessageEvent(new FixMessageEventArgs(this.sessionId, fixMessage));

		}
		
		private void HeartbeatFixMessageHandler(FixMessage fixMessage)
		{

			OnHeartbeatFixMessageEvent(new FixMessageEventArgs(this.sessionId, fixMessage));

		}
		
		private void TestRequestFixMessageHandler(FixMessage fixMessage)
		{

			OnTestRequestFixMessageEvent(new FixMessageEventArgs(this.sessionId, fixMessage));

		}
		
		private void ResendRequestFixMessageHandler(FixMessage fixMessage)
		{

			OnResendRequestFixMessageEvent(new FixMessageEventArgs(this.sessionId, fixMessage));

		}

		private void SequenceResetFixMessageHandler(FixMessage fixMessage)
		{

			OnSequenceResetFixMessageEvent(new FixMessageEventArgs(this.sessionId, fixMessage));

		}

		private void CancelFixMessageHandler(FixMessage fixMessage)
		{

			OnCancelFixMessageEvent(new FixMessageEventArgs(this.sessionId, fixMessage));

		}

		private void CancelReplaceFixMessageHandler(FixMessage fixMessage)
		{

			OnCancelReplaceFixMessageEvent(new FixMessageEventArgs(this.sessionId, fixMessage));

		}

		private void OrderFixMessageHandler(FixMessage fixMessage)
		{

			OnOrderFixMessageEvent(new FixMessageEventArgs(this.sessionId, fixMessage));

		}
		
		private void ExecutionReportFixMessageHandler(FixMessage fixMessage)
		{

			OnExecutionReportFixMessageEvent(new FixMessageEventArgs(this.sessionId, fixMessage));

		}
		
		public static void RunEndOfDay(string sessionId)
		{

			// Open the persistent message store so it can be cleared.
			MessageStore inboundMessageStore = new MessageStore();
			inboundMessageStore.Open(string.Format("{0}_inbound", sessionId));

			MessageStore outboundMessageStore = new MessageStore();
			outboundMessageStore.Open(string.Format("{0}_outbound", sessionId));

			// Truncate the message store.
			outboundMessageStore.Truncate();
			inboundMessageStore.Truncate();

			// Close out the message stores.
			outboundMessageStore.Close();
			inboundMessageStore.Close();

		}

		public void ResetIncomingSequence()
		{

			// Resets the inbound squence number.
			this.inboundSequenceNumber = this.inboundMessageStore.Count;

		}

		public void ResetOutgoingSequence()
		{

			// Resets the outbound squence number.
			this.outboundSequenceNumber = this.outboundMessageStore.Count + 1;

		}

	}
	
}
