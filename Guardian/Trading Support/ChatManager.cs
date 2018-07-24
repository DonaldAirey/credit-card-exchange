namespace FluidTrade.Guardian
{
	using System;
	using System.Data;
	using System.Security.Principal;
	using System.Threading;
	using System.Transactions;
	using FluidTrade.Core;
	using FluidTrade.Guardian.Records;

	/// <summary>Finds matching orders and negotiations the exchange of assets.</summary>
	/// <copyright>Copyright (C) 1998-2005 Fluid Trade -- All Rights Reserved.</copyright>
	internal class ChatManager
	{

		// Private Constants
		private const Int32 threadWait = 100;

		// Private Static Methods
		private static WaitQueue<ObjectAction> actionQueue;
		//private static ClaimsPrincipal claimsPrincipal;
		private static Boolean isChatActive;
		private static Thread chatThread;
		private static Object syncRoot;

		/// <summary>
		/// Brings buyers and sellers together.
		/// </summary>
		static ChatManager()
		{

			// This object is used for multithreaded coordination.
			ChatManager.syncRoot = new Object();

			// This queue is filled up with Working Orders that need to be serviced because something changed the matching criteria.
			ChatManager.actionQueue = new WaitQueue<ObjectAction>();

		}

		/// <summary>
		/// Gets a queue of actions which are executed asynchronously.
		/// </summary>
		internal static WaitQueue<ObjectAction> ActionQueue
		{
			get { return ChatManager.actionQueue; }
		}

		/// <summary>
		/// Gets or sets the status of the thread that handles the actions that enforce the business rules.
		/// </summary>
		internal static Boolean IsChatActive
		{

			get
			{

				// This is the current status of the crossing activity.
				lock (ChatManager.syncRoot)
					return ChatManager.isChatActive;

			}

			set
			{

				// The idea here is to start the thread and install the event handlers when the crossingn is activated and to kill the thread and uninstall the
				// handlers when the crossing is deactivated.
				lock (ChatManager.syncRoot)
				{

					// This will turn on crossing when it has been inactive.
					if (!ChatManager.isChatActive && value)
					{

						// These event handlers will update the matching conditions as the underlying records change.
						DataModel.Chat.ChatRowChanging += OnChatRowChanging;

						// This thread will execution the actions that are created by changes to the data model.  The triggers themselves can't modify the data
						// model because the triggers are called from the commit handlers.
						ChatManager.chatThread = new Thread(new ThreadStart(ChatManager.ChatThread));
						ChatManager.chatThread.Name = "Chat Thread";
						ChatManager.chatThread.IsBackground = true;
						ChatManager.chatThread.Start();

					}

					// This wil turn off crossing when it has been active.
					if (ChatManager.isChatActive && !value)
					{

						// These event handlers will must be removed from the data model.
						DataModel.Chat.ChatRowChanging -= OnChatRowChanging;

						// Shut down thread that handles the trigger driven actions.
						if (!ChatManager.chatThread.Join(100))
							ChatManager.chatThread.Abort();

					}

					// This field is used to keep track of the state of the crossing engine.
					ChatManager.isChatActive = value;

				}

			}

		}

		/// <summary>
		/// Pulls actions off the queue and executes them.
		/// </summary>
		private static void ChatThread()
		{
			// This set of claims gives the current thread the authority do anything to the data model.
			Thread.CurrentPrincipal = new ClaimsPrincipal(WindowsIdentity.GetCurrent(), null);
			Thread.CurrentPrincipal = new ClaimsPrincipal(new GenericIdentity("NT AUTHORITY\\NETWORK SERVICE"), null);

			try
			{
				// The data model triggers will indicate that something needs to be done to enforce the business rules, but the triggers themselves can't change
				// the data model because they are part of the commit logic of other transactions.  This action pump will wait here until there is an action to
				// process and then call the handler, thus offloading the action from the data model triggers.
				while (ChatManager.IsChatActive)
				{
					try
					{
						ObjectAction objectAction = ChatManager.ActionQueue.Dequeue();
						objectAction.DoAction(objectAction.Key, objectAction.Parameters);
					}
					catch (Exception exception)
					{
						EventLog.Error("{0}: {1}", exception.Message, exception.StackTrace);
					}
				}
			}
			catch (ThreadAbortException)
			{
                return;
			}
		}

		public static void CreateChat(ChatInfo[] chatInfos)
		{

			// A reference to the data model is required for actions that modify it.
			DataModel dataModel = new DataModel();

			// Everything in this transaction will be committed or rolled back as a unit.
			using (TransactionScope transactionScope = new TransactionScope())
			{

				// Extract the ambient transaction.
				DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

				// This action handles a batch of chat items in a single transaction.
				foreach (ChatInfo chatInfo in chatInfos)
				{

					// These values are required for each chat item added.
					Guid blotterId = default(Guid);
					DateTime createdTime = DateTime.UtcNow;

					// The Match record contains the blotter identifier which is used for filtering.
					MatchRow matchRow = DataModel.Match.MatchKey.Find(chatInfo.MatchId);
					try
					{
						matchRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
						blotterId = matchRow.BlotterId;
					}
					finally
					{
						matchRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
					}
					
					// Once the blotter id has been extracted the internal methods can be called to add the chat item.
					dataModel.CreateChat(blotterId, chatInfo.ChatId, createdTime, false, chatInfo.MatchId, chatInfo.Message);

				}

				// If we reached this point the transaction was successful.
				transactionScope.Complete();

			}

		}
	
		/// <summary>
		/// Evaluates whether a given working order is eligible for a cross with another order.
		/// </summary>		
		public static void CrossChat(Object[] key, params Object[] parameters)
		{

			// An instance of the data model is required for CRUD operations.
			DataModel dataModel = new DataModel();

			// The logic below will examine the order and see if a contra order is available for a match.  These values will indicate whether a match is
			// possible after all the locks have been released.
			Guid chatId = (Guid)key[0];

			// A transaction is required to lock the records and change the data model.
			using (TransactionScope transactionScope = new TransactionScope())
			{

				// This variable holds context information for the current transaction.
				DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

				// This is the working order that will be tested for a possible buyer or seller (contra party).
				ChatRow chatRow = DataModel.Chat.ChatKey.Find(chatId);
				if (chatRow == null)
					throw new Exception(string.Format("Chat {0} has been deleted", chatId));
				chatRow.AcquireReaderLock(dataModelTransaction);

				// The match record contains the identifier for the counter party.  The counter party is needed in order to create an
				// entry in the conversation that is private to that user.
				MatchRow matchRow = DataModel.Match.MatchKey.Find(chatRow.MatchId);
				matchRow.AcquireReaderLock(dataModelTransaction);

				// This is the counter party to the match.
				MatchRow contraMatchRow = DataModel.Match.MatchKey.Find(matchRow.ContraMatchId);
				contraMatchRow.AcquireReaderLock(dataModelTransaction);

				// Each side needs a private copy of the conversation in order to pass through the Chinese wall.  This will create an entry in the conversation
				// for the contra side of the conversation.
				Guid contraChatId = Guid.NewGuid();
				DateTime createdTime = DateTime.UtcNow;
				dataModel.CreateChat(
					contraMatchRow.BlotterId,
					contraChatId,
					createdTime,
					true,
					contraMatchRow.MatchId,
					chatRow.Message);

				// The working order that triggered this action has completed a scan of the data model and has notified all possible counter parties of the its
				// new status.  Once the transaction is completed, a negotiation session will be started if a new counter party is found.
				transactionScope.Complete();

			}

		}

		/// <summary>
		/// Handles a change to a record in the Negotiation table.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="chatRowChangeEventArg">The event data.</param>
		private static void OnChatRowChanging(object sender, ChatRowChangeEventArgs chatRowChangeEventArg)
		{

			// When a chat record is created, call the crossing logic to create an identical entry for the contra.
			if (chatRowChangeEventArg.Action == DataRowAction.Commit)
				if (!chatRowChangeEventArg.Row.HasVersion(DataRowVersion.Original) &&
					chatRowChangeEventArg.Row.RowState != DataRowState.Deleted &&
					chatRowChangeEventArg.Row.RowState != DataRowState.Detached)
				{
					if ((Boolean)chatRowChangeEventArg.Row[DataModel.Chat.IsReplyColumn] == false)
					{
						Guid chatId = (Guid)chatRowChangeEventArg.Row[DataModel.Chat.ChatIdColumn];
						ChatManager.actionQueue.Enqueue(new ObjectAction(CrossChat, new Object[] { chatId }));
					}
				}

		}

	}

}
