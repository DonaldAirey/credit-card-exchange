namespace FluidTrade.Guardian
{

	using System;
	using System.Data;
	using System.Security.Principal;
	using System.Threading;
	using System.Transactions;
	using FluidTrade.Core;

	/// <summary>Finds matching orders and negotiates the exchange of assets.</summary>
	/// <copyright>Copyright (C) 1998-2005 Fluid Trade -- All Rights Reserved.</copyright>
	internal class EquityCross : IExchange
	{

		// Private Constants
		private const Int32 timerPeriod = 1000;
		private const Int32 crossingTime = 30000;
		private const Int32 threadWait = 100;
		private const Int32 marketSleep = 900;

		// Private Static Methods
		private static WaitQueue<ObjectAction> actionQueue;
		//private static ClaimsPrincipal claimsPrincipal;
		private static Guid destinationId;
		private static TimeSpan negotiationTime;
		private static Object syncRoot;

		/// <summary>
		/// Brings buyers and sellers of equities together.
		/// </summary>
		static EquityCross()
		{

			// This object is used for multithreaded coordination.
			EquityCross.syncRoot = new Object();

			// This is the time a user is given to respond to the notification of a possible match.
			EquityCross.negotiationTime = TimeSpan.FromMilliseconds(EquityCross.crossingTime);

			// This queue is filled up with Working Orders that need to be serviced because something changed the matching criteria.
			EquityCross.actionQueue = new WaitQueue<ObjectAction>();

			// A broker destination is required when creating destination orders and executions from the successful negotiations.  The identifier for this
			// destination is found in the settings.
			using (TransactionScope transactionScope = new TransactionScope())
			{

				// Provides a context for the accessing the data model.
				DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

				// The destination for all orders crossed is found in the settings.  This should eventually be a more data driven process.
				DestinationRow destinationRow = DataModel.Destination.DestinationKeyExternalId0.Find("GUARDIAN ECN");
				if (destinationRow != null)
				{

					try
					{

						// This destination is used for all executions in the simulated exchange.
						destinationRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
						EquityCross.destinationId = destinationRow.DestinationId;

					}
					finally
					{

						// The lock used here is ephemeral.
						destinationRow.ReleaseReaderLock(dataModelTransaction.TransactionId);

					}

				}

			}

		}

		/// <summary>
		/// Pulls actions and their parameters off the queue and executes them.
		/// </summary>
		private static void CrossingThread()
		{

			// All the actions added to the generic list of actions and parameter will execute with this claims principal.
			Thread.CurrentPrincipal = new ClaimsPrincipal(WindowsIdentity.GetCurrent(), null);
			Thread.CurrentPrincipal = new ClaimsPrincipal(new GenericIdentity("NT AUTHORITY\\NETWORK SERVICE"), null);			

			// The event handlers for the data model can't wait on locks and resources outside the data model.  There would simply be too many resources that 
			// could deadlock.  This code will pull requests off of a generic queue of actions and parameters and execute them using the authentication created
			// above.
			while (true)
			{

				try
				{

					// The thread will wait here until an action has been placed in the queue to be processed in this thread context.
					ObjectAction objectAction = EquityCross.actionQueue.Dequeue();
					objectAction.DoAction(objectAction.Key, objectAction.Parameters);

				}
				catch (Exception exception)
				{

					// This will catch any exceptions thrown during the processing of the generic actions.
					EventLog.Error("{0}: {1}", exception.Message, exception.StackTrace);

				}

			}
		}

		/// <summary>
		/// Evaluates whether a given working order is eligible for a cross with another order.
		/// </summary>		
		public static void CrossWorkingOrder(Object[] key, params Object[] parameter)
		{

			// Extract the strongly typed parameter from the arguments.
			Guid workingOrderId = (Guid)key[0];

			// An instance is required to make modifications to the data model.
			DataModel dataModel = new DataModel();

			// If two counterparties agree on a transaction then destination orders and executions will be created and the status of the negotiations will be
			// updated.  A transaction is required for this.
			using (TransactionScope transactionScope = new TransactionScope())
			{

				// This provides a context for any transactions.
				DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

				// The logic below will examine the order and see if a contra order is available for a match.  These values will indicate whether a match is
				// possible after all the locks have been released.
				Boolean isMatched = false;
				Guid contraOrderId = Guid.Empty;
				Guid blotterId = Guid.Empty;
				Guid contraBlotterId = Guid.Empty;

				// It is important to minimize the locking for these transactions since they will drag the system performance down and create deadlock 
				// sitiations if too many are held for too long.
				WorkingOrderRow workingOrderRow = null;

				try
				{

					// The working order is where all the matching starts.
					workingOrderRow = DataModel.WorkingOrder.WorkingOrderKey.Find(workingOrderId);
					workingOrderRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

					// Only submitted orders can be matched against other submitted orders.
					if (workingOrderRow.StatusId == StatusMap.FromCode(Status.Submitted))
					{

						// The security data for this order is needed below.  The security provides the primarly list of orders that can be on the opposite 
						// side of this order.
						SecurityRow securityRow = workingOrderRow.SecurityRowByFK_Security_WorkingOrder_SecurityId;
						securityRow.AcquireReaderLock(dataModelTransaction);

						// There is no need to try to match this security if it isn't an Equity asset.
						if (securityRow.GetEquityRowsByFK_Security_Equity_EquityId().Length == 0)
							return;

						// The blotter row needs to be examined for this operation also.
						BlotterRow blotterRow = workingOrderRow.BlotterRow;
						blotterRow.AcquireReaderLock(dataModelTransaction);

						// If a match is made it will be assigned to this blotter.
						blotterId = blotterRow.BlotterId;

						// Find the party to which this order belongs.
						Guid partyTypeId = EquityCross.GetPartyTypeId(dataModelTransaction, blotterRow);
						if (partyTypeId == PartyTypeMap.FromCode(PartyType.NotValid))
							return;

						// Reject any orders that don't meet the minimums set for a given security.  If a minimum hasn't been set, then assume that anything goes.
						Decimal submittedQuantity = workingOrderRow.SubmittedQuantity;
						foreach (DestinationOrderRow destinationOrderRow in workingOrderRow.GetDestinationOrderRows())
						{
							destinationOrderRow.AcquireReaderLock(dataModelTransaction);
							foreach (ExecutionRow executionRow in destinationOrderRow.GetExecutionRows())
							{
								executionRow.AcquireReaderLock(dataModelTransaction);
								submittedQuantity -= executionRow.ExecutionQuantity;
							}
						}
						if (!securityRow.IsMinimumQuantityNull() && submittedQuantity < securityRow.MinimumQuantity)
							return;

						// Reject any working orders that have negotiations pending with other counter parties.
						Boolean isWorkingOrderActive = false;
						foreach (MatchRow matchRow in workingOrderRow.GetMatchRows())
						{
							matchRow.AcquireReaderLock(dataModelTransaction);
							if (matchRow.StatusId == StatusMap.FromCode(Status.Active))
							{
								isWorkingOrderActive = true;
								break;
							}
						}
						if (isWorkingOrderActive)
							return;

						// If there is more than one possible match, the oldest one will go first.
						WorkingOrderRow oldestOrderRow = null;
						DateTime oldestSubmittedTime = DateTime.MinValue;

						// This iteration will look for counter parties for matching.  If more than one order is found that qualifies, the one that has been
						// dormant the longest will be chosen for a negotiation.
						foreach (WorkingOrderRow contraOrderRow in securityRow.GetWorkingOrderRowsByFK_Security_WorkingOrder_SecurityId())
						{

							// Each potential contra working order must be locked before it can be examined.
							contraOrderRow.AcquireReaderLock(dataModelTransaction);

							// Test #1: Reject orders that are not on an opposite side
							if ((workingOrderRow.SideId != SideMap.FromCode(Side.Buy) || contraOrderRow.SideId != SideMap.FromCode(Side.Sell)) &&
								(workingOrderRow.SideId != SideMap.FromCode(Side.Sell) || contraOrderRow.SideId != SideMap.FromCode(Side.Buy)))
								continue;

							// Test #2: Reject any orders that are not submitted.
							if (contraOrderRow.StatusId != StatusMap.FromCode(Status.Submitted))
								continue;

							// Test #3: Reject any orders that don't meet the minimums set for a given security.
							if (!securityRow.IsMinimumQuantityNull())
							{
								Decimal contraSubmittedQuantity = contraOrderRow.SubmittedQuantity;
								foreach (DestinationOrderRow destinationOrderRow in contraOrderRow.GetDestinationOrderRows())
								{
									destinationOrderRow.AcquireReaderLock(dataModelTransaction);
									foreach (ExecutionRow executionRow in destinationOrderRow.GetExecutionRows())
									{
										executionRow.AcquireReaderLock(dataModelTransaction);
										contraSubmittedQuantity -= executionRow.ExecutionQuantity;
									}
								}
								if (contraSubmittedQuantity < securityRow.MinimumQuantity)
									continue;
							}

							// Test #4: Reject any parties that aren't valid for crossing.
							BlotterRow contraBlotterRow = contraOrderRow.BlotterRow;
							contraBlotterRow.AcquireReaderLock(dataModelTransaction);
							Guid counterPartyTypeId = EquityCross.GetPartyTypeId(dataModelTransaction, contraBlotterRow);
							if (counterPartyTypeId == PartyTypeMap.FromCode(PartyType.NotValid))
								continue;

							// Test #5: Make sure this order matches the counter parties preference for a match.
							if ((!workingOrderRow.IsBrokerMatch || counterPartyTypeId != PartyTypeMap.FromCode(PartyType.Broker)) &&
								(!workingOrderRow.IsHedgeMatch || counterPartyTypeId != PartyTypeMap.FromCode(PartyType.Hedge)) &&
								(!workingOrderRow.IsInstitutionMatch || counterPartyTypeId != PartyTypeMap.FromCode(PartyType.Instutition)))
								continue;

							// Test #6: Make sure that the attrributes of this order match the counter party's preference for a match.
							if ((!contraOrderRow.IsBrokerMatch || partyTypeId != PartyTypeMap.FromCode(PartyType.Broker)) &&
								(!contraOrderRow.IsHedgeMatch || partyTypeId != PartyTypeMap.FromCode(PartyType.Hedge)) &&
								(!contraOrderRow.IsInstitutionMatch || partyTypeId != PartyTypeMap.FromCode(PartyType.Instutition)))
								continue;

							// Test #7: Reject any orders pending other negotiations.
							Boolean isContraActive = false;
							foreach (MatchRow matchRow in contraOrderRow.GetMatchRows())
							{
								matchRow.AcquireReaderLock(dataModelTransaction);
								if (matchRow.StatusId == StatusMap.FromCode(Status.Active))
								{
									isContraActive = true;
									break;
								}
							}
							if (isContraActive)
								continue;

							// At this point a qualified counter party has been found.  The next step is to select the one that has been dormant the longest when
							// there are several counter parties that qualify.  This iteration examines the selected counter party to see if there was a previous
							// attempt to match the working order.  The most recent attempt at matching is used to determine how long the contra order has been
							// dormant.
							MatchRow youngestMatchRow = null;
							foreach (MatchRow contraMatchRow in contraOrderRow.GetMatchRows())
							{
								contraMatchRow.AcquireReaderLock(dataModelTransaction);
								if (contraMatchRow.ContraOrderId == workingOrderRow.WorkingOrderId)
									if (youngestMatchRow == null || youngestMatchRow.MatchedTime < contraMatchRow.MatchedTime)
										youngestMatchRow = contraMatchRow;
							}

							// If there are no matches yet between these two orders, then use the time that the contra order was entered to find the oldest of the
							// orders.
							if (youngestMatchRow == null)
							{

								// Select this contra order if it is the oldest.
								if (oldestOrderRow == null || oldestSubmittedTime > contraOrderRow.SubmittedTime)
								{
									oldestSubmittedTime = contraOrderRow.SubmittedTime;
									oldestOrderRow = contraOrderRow;
								}

							}
							else
							{

								// If these two orders have attempted to match previously, then select the order that has the oldest attempt at a match.
								if (youngestMatchRow.MatchedTime > oldestSubmittedTime)
								{
									oldestSubmittedTime = youngestMatchRow.MatchedTime;
									oldestOrderRow = contraOrderRow;
								}

							}

						}

						// After checking all the possible contra orders, select the one that has been dormant the longest for negotation.
						if (oldestOrderRow != null)
						{
							isMatched = true;
							contraOrderId = oldestOrderRow.WorkingOrderId;
							contraBlotterId = oldestOrderRow.BlotterId;
						}

					}
					else
					{

						// If the working order has been pulled out of the matching box, then find the counter party and decline any pending matches.
						foreach (MatchRow matchRow in workingOrderRow.GetMatchRows())
						{

							// The match must be locked before it can be examined.
							matchRow.AcquireReaderLock(dataModelTransaction);

							// If a working order is pulled out of the matching box, it must decline any pending matches.
							if (matchRow.StatusId == StatusMap.FromCode(Status.Active))
							{

								// This will update the match to show that it was declined.
								dataModel.UpdateMatch(null, null, null, null, null, null, null, new Object[] { matchRow.MatchId }, matchRow.RowVersion, StatusMap.FromCode(Status.Declined), null);

								// This match has been confirmed so the timer can be terminated.
								MatchTimerRow matchTimerRow = DataModel.MatchTimer.MatchTimerKey.Find(matchRow.MatchId);
								if (matchTimerRow != null)
								{
									matchTimerRow.AcquireReaderLock(dataModelTransaction);
									dataModel.DestroyMatchTimer(new Object[] { matchTimerRow.MatchId }, matchTimerRow.RowVersion);
								}

								// The counter party must also be sent a notification that the working order was pulled out of the box.
								MatchRow contraMatchRow = DataModel.Match.MatchKeyWorkingOrderIdContraOrderId.Find(
									matchRow.ContraOrderId,
									matchRow.WorkingOrderId);
								contraMatchRow.AcquireReaderLock(dataModelTransaction);

								// This will update the contra match to show that it was declined.
								if (contraMatchRow.StatusId != StatusMap.FromCode(Status.Declined))
									dataModel.UpdateMatch(
										null,
										null,
										null,
										null,
										null,
										null,
										null,
										new Object[] { contraMatchRow.MatchId },
										contraMatchRow.RowVersion,
										StatusMap.FromCode(Status.Declined),
										null);

								// This match has been confirmed so the timer can be terminated.
								MatchTimerRow contraMatchTimerRow = DataModel.MatchTimer.MatchTimerKey.Find(contraMatchRow.MatchId);
								if (contraMatchTimerRow != null)
								{
									contraMatchTimerRow.AcquireReaderLock(dataModelTransaction);
									dataModel.DestroyMatchTimer(new Object[] { contraMatchTimerRow.MatchId }, contraMatchTimerRow.RowVersion);
								}

								// Now that a match with this working order is no longer a possibility, the contra order should be given the chance to scan the
								// data model for a possible match.
								EquityCross.actionQueue.Enqueue(new ObjectAction(EquityCross.CrossWorkingOrder, new Object[] { matchRow.ContraOrderId }));

							}

						}

					}

				}
				finally
				{

					// The WorkingOrder record lock is no longer needed for this transaction.
					if (workingOrderRow != null)
						workingOrderRow.ReleaseLock(dataModelTransaction.TransactionId);

				}

				// If two parties were found that match all the criteria, then a negotiation session is begun to try to get the two parties to agree to a
				// transaction.  The session is given a time limit to prevent one party from having to wait around all day for a response.
				if (isMatched)
				{

					// Both parties are given a timer that will implicitly decline the transaction when it expires.
					DateTime currentTime = DateTime.Now;

					// These identifiers are used to match working orders without information leaking across the 'Chinese Wall'.
					Guid matchId = Guid.NewGuid();
					Guid contraMatchId = Guid.NewGuid();

					// This will update an existing match or create a new one for the unique working order pair.
					MatchRow matchRow = DataModel.Match.MatchKeyWorkingOrderIdContraOrderId.Find(workingOrderId, contraOrderId);
					if (matchRow == null)
					{

						// Create a match between the primary order and the contra order and give it a time limit for a decision.  Note that the architecture of 
						// the timers allow them to be purged regularly while the match record provides a persistent history of the activity on a given working
						// order.
						dataModel.CreateMatch(blotterId, contraMatchId, contraOrderId, 1.0M, null, currentTime, matchId, StatusMap.FromCode(Status.Active), workingOrderId);
						dataModel.CreateMatchTimer(currentTime, matchId, DateTime.Now + EquityCross.negotiationTime);

					}
					else
					{

						// A new set of parameters for matching will restart the counter.
						dataModel.UpdateMatch(null, null, null, null, null, currentTime, null, new object[] { matchRow.MatchId }, matchRow.RowVersion, StatusMap.FromCode(Status.Active), null);
						dataModel.CreateMatchTimer(currentTime, matchRow.MatchId, DateTime.Now + EquityCross.negotiationTime);

					}

					// This will update an existing counter match or create a new one for the unique working order pair.
					MatchRow contraMatchRow = DataModel.Match.MatchKeyWorkingOrderIdContraOrderId.Find(contraOrderId, workingOrderId);
					if (contraMatchRow == null)
					{

						// Conversely, add the match of the contra order to the primary and give it a time limit for a decision.  Note that the architecture of the
						// timers allow them to be purged regularly while the match record provides a persistent history of the activity on a given working order.
						dataModel.CreateMatch(contraBlotterId, matchId, workingOrderId, 1.0M, null, currentTime, contraMatchId, StatusMap.FromCode(Status.Active), contraOrderId);
						dataModel.CreateMatchTimer(currentTime, contraMatchId, DateTime.Now + EquityCross.negotiationTime);

					}
					else
					{

						// A new set of parameters for matching will restart the counter.
						dataModel.UpdateMatch(null, null, null, null,null, currentTime, null, new object[] { contraMatchRow.MatchId }, contraMatchRow.RowVersion, StatusMap.FromCode(Status.Active), null);
						dataModel.CreateMatchTimer(currentTime, contraMatchRow.MatchId, DateTime.Now + EquityCross.negotiationTime);

					}

				}

				// At this point the transaction is complete and can be committed as a unit.
				transactionScope.Complete();

			}

		}

		/// <summary>
		/// Gets the party type associated with a given blotter.
		/// </summary>
		/// <param name="dataModelTransaction">The current transaction.</param>
		/// <param name="blotterRow">The blotter who's type is requested.</param>
		/// <returns>The party type of the given blotter.</returns>
		internal static Guid GetPartyTypeId(DataModelTransaction dataModelTransaction, BlotterRow blotterRow)
		{

			// A blotter can have an explicit party type associated with it.
			if (blotterRow.PartyTypeId != PartyTypeMap.FromCode(PartyType.UseParent) && blotterRow.PartyTypeId != PartyTypeMap.FromCode(PartyType.NotValid))
				return blotterRow.PartyTypeId;

			// A blotter can also use the party type associated with its parent.
			if (blotterRow.PartyTypeId == PartyTypeMap.FromCode(PartyType.UseParent))
			{
				EntityRow entityRow = blotterRow.EntityRow;
				entityRow.AcquireReaderLock(dataModelTransaction);
				foreach (EntityTreeRow entityTreeRow in entityRow.GetEntityTreeRowsByFK_Entity_EntityTree_ChildId())
				{
					entityTreeRow.AcquireReaderLock(dataModelTransaction);
					EntityRow parentRow = entityTreeRow.EntityRowByFK_Entity_EntityTree_ParentId;
					parentRow.AcquireReaderLock(dataModelTransaction);
					foreach (BlotterRow parentBlotterRow in parentRow.GetBlotterRows())
					{
						parentBlotterRow.AcquireReaderLock(dataModelTransaction);
						Guid partyTypeId = GetPartyTypeId(dataModelTransaction, parentBlotterRow);
						if (partyTypeId != PartyTypeMap.FromCode(PartyType.NotValid))
							return partyTypeId;
					}
				}
			}

			// At this point, the given blotter has not been classified.
			return PartyTypeMap.FromCode(PartyType.NotValid);

		}

		/// <summary>
		/// Handles the negotiation between two parties.
		/// </summary>
		/// <param name="key">The unique identifier of the negotation record.</param>
		/// <param name="parameter">The unused generic parameters for an action handler.</param>
		private static void NegotiateCross(Object[] key, params Object[] parameter)
		{

			// Extract the specific variables from the generic arguments.
			Guid negotiationId = (Guid)key[0];

			// An instance is required to make modifications to the data model.
			DataModel dataModel = new DataModel();

			// If two counterparties agree on a transaction then destination orders and executions will be created and the status of the negotiations will be
			// updated.  A transaction is required for this.
			using (TransactionScope transactionScope = new TransactionScope())
			{

				// This provides a context for any transactions.
				DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

				// This will find the Negotiation record that has been created and lock it.
				NegotiationRow negotiationRow = DataModel.Negotiation.NegotiationKey.Find(negotiationId);
				if (negotiationRow == null)
					throw new Exception(string.Format("Negotiation {0} has been deleted", negotiationId));
				negotiationRow.AcquireReaderLock(dataModelTransaction);

				// Extract and lock the working order information from the Match record.
				MatchRow matchRow = negotiationRow.MatchRow;
				matchRow.AcquireReaderLock(dataModelTransaction);
				WorkingOrderRow workingOrderRow = matchRow.WorkingOrderRow;
				workingOrderRow.AcquireReaderLock(dataModelTransaction);

				// The last price is used to calculate the mid price from the last bid and the last offer.
				PriceRow priceRow = DataModel.Price.PriceKey.Find(workingOrderRow.SecurityId, workingOrderRow.SettlementId);
				if (priceRow == null)
					throw new Exception(string.Format("Price for '{0}' has been deleted", workingOrderRow.SecurityId));
				priceRow.AcquireReaderLock(dataModelTransaction);

				// This will find all the records associated with the counter party offer.  Note that this isn't available through a relation because the
				// contra side of the trade is filtered from the client workstation.  For all intents and purposes, the ContraOrderId is merely a token to the
				// client workstation: it conveys no real information.  Also note that the timestamp for each side of the match is identical and can be used as
				// part of the index to find the proper counter offer when there are multiple matches for two given counter parties.
				MatchRow contraMatchRow = DataModel.Match.MatchKeyWorkingOrderIdContraOrderId.Find(
					new object[] { matchRow.ContraOrderId, matchRow.WorkingOrderId });
				if (contraMatchRow == null)
					throw new Exception(
						string.Format("The match record for the pair of orders {0}, {1} can't be found",
						matchRow.ContraOrderId,
						matchRow.WorkingOrderId));
				contraMatchRow.AcquireReaderLock(dataModelTransaction);

				// The contra order needs to be locked in order to initiate the negotiation.
				WorkingOrderRow contraOrderRow = contraMatchRow.WorkingOrderRow;
				contraOrderRow.AcquireReaderLock(dataModelTransaction);

				// When both sides have responded (or timed out) the two sides of the negotiation are resoved into a set of actions.  If both sides have
				// accepted the chance to trade, then Destination Orders and Executions are generated.  If either side has rejected the chance to trade, then
				// this opportunity to match counterparties is discarded.
				foreach (NegotiationRow contraNegotiationRow in contraMatchRow.GetNegotiationRows())
				{

					// Lock the next counter party's NegotiationRow for reading.
					contraNegotiationRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
					dataModelTransaction.AddLock(contraNegotiationRow);
					if (contraNegotiationRow.RowState == DataRowState.Detached)
						continue;

					// These values are used when a Destination Order and Execution are created.
					DateTime currentTime = DateTime.Now;
					Guid createdUserId = TradingSupport.UserId;
					DateTime createdTime = DateTime.Now;
					Guid modifiedUserId = TradingSupport.UserId;
					DateTime modifiedTime = DateTime.Now;

					// These values indicate that one side or the other should be put to sleep for a while after this transaction is completed.
					Boolean isWorkingOrderTired = false;
					Boolean isContraOrderTired = false;

					// Check to see if both sides have accepted the negotiation.
					if (negotiationRow.StatusId == StatusMap.FromCode(Status.Pending) && contraNegotiationRow.StatusId == StatusMap.FromCode(Status.Pending))
					{

						// At this point, we've got a successful negotiation of a trade.  The quantity of this trade will be the minimum of the two offers and
						// the price will be a simple calculation of the mid price.
						Decimal matchedQuantity = negotiationRow.Quantity < contraNegotiationRow.Quantity ?
							negotiationRow.Quantity : contraNegotiationRow.Quantity;
						Decimal matchedPrice = (priceRow.BidPrice + priceRow.AskPrice) / 2.0m;

						// Create an implicit Destination Order on this side of the trade.
						Guid destinationOrderId = Guid.NewGuid();
						dataModel.CreateDestinationOrder(
							workingOrderRow.BlotterId,
							0.0M,
							null,
							createdTime,
							createdUserId,
							EquityCross.destinationId,
							destinationOrderId,
							null,
							null,
							false,
							false,
							workingOrderRow[DataModel.WorkingOrder.LimitPriceColumn],
							modifiedTime,
							modifiedUserId,
							matchedQuantity,
							workingOrderRow.OrderTypeId,
							workingOrderRow.SecurityId,
							modifiedTime,
							workingOrderRow.SettlementId,
							workingOrderRow.SideId,
							StateMap.FromCode(State.Acknowledged),
							StatusMap.FromCode(Status.New),
							workingOrderRow[DataModel.WorkingOrder.StopPriceColumn],
							workingOrderRow.TimeInForceId,
							modifiedTime,
							modifiedUserId,
							workingOrderRow.WorkingOrderId);

						// Create the Execution for this side of the trade.
						Guid executionId = Guid.NewGuid();
						dataModel.CreateExecution(
							0.0M,
							workingOrderRow.BlotterId,
							null,
							null,
							0.0M,
							createdTime,
							createdUserId,
							destinationOrderId,
							StateMap.FromCode(State.Acknowledged),
							executionId,
							matchedPrice,
							matchedQuantity,
							null,
							null,
							false,
							modifiedTime,
							modifiedUserId,
							null,
							null,
							null,
							null,
							StateMap.FromCode(State.Acknowledged),
							0.0M,
							0.0M,
							0.0M,
							0.0M);

						// Update the negotiation to show that it was accepted and link it to the execution that was generated.
						dataModel.UpdateNegotiation(
							null,
							executionId,
							null,
							null,
							null,
							new Object[] { negotiationRow.NegotiationId },
							null,
							negotiationRow.RowVersion,
							StatusMap.FromCode(Status.Accepted));

						// This will update the match to show that it was accepted.
						dataModel.UpdateMatch(
							null,
							null,
							null,
							null,
							null,
							null,
							null,
							new Object[] { negotiationRow.MatchId },
							matchRow.RowVersion,
							StatusMap.FromCode(Status.Accepted),
							null);

						// This match has been confirmed so the timer can be terminated.
						MatchTimerRow matchTimerRow = DataModel.MatchTimer.MatchTimerKey.Find(negotiationRow.MatchId);
						if (matchTimerRow != null)
						{
							matchTimerRow.AcquireReaderLock(dataModelTransaction);
							dataModel.DestroyMatchTimer(new Object[] { matchTimerRow.MatchId }, matchTimerRow.RowVersion);
						}

						// Create a Destinatino Order for the other side of this trade.
						Guid contraDestinationOrderId = Guid.NewGuid();
						dataModel.CreateDestinationOrder(
							contraOrderRow.BlotterId,
							0.0M,
							null,
							createdTime,
							createdUserId,
							EquityCross.destinationId,
							contraDestinationOrderId,
							null,
							null,
							false,
							false,
							contraOrderRow[DataModel.WorkingOrder.LimitPriceColumn],
							modifiedTime,
							modifiedUserId,
							matchedQuantity,
							contraOrderRow.OrderTypeId,
							contraOrderRow.SecurityId,
							modifiedTime,
							contraOrderRow.SettlementId,
							contraOrderRow.SideId,
							StateMap.FromCode(State.Acknowledged),
							StatusMap.FromCode(Status.New),
							contraOrderRow[DataModel.WorkingOrder.StopPriceColumn],
							contraOrderRow.TimeInForceId,
							modifiedTime,
							modifiedUserId,
							contraOrderRow.WorkingOrderId);

						// Create an Execution for the other side of the trade.
						Guid contraExecutionId = Guid.NewGuid();
						dataModel.CreateExecution(
							0.0M,
							contraOrderRow.BlotterId,
							null,
							null,
							0.0M,
							createdTime,
							createdUserId,
							contraDestinationOrderId,
							StateMap.FromCode(State.Acknowledged),
							contraExecutionId,
							matchedPrice,
							matchedQuantity,
							null,
							null,
							false,
							modifiedTime,
							modifiedUserId,
							null,
							null,
							null,
							null,
							StateMap.FromCode(State.Acknowledged),
							0.0M,
							0.0M,
							0.0M,
							0.0M);

						// Update the counter party Negotiation record to show that it was accepted and link it to the execution that was generated.
						dataModel.UpdateNegotiation(
							null,
							contraExecutionId,
							null,
							null,
							null,
							new Object[] { contraNegotiationRow.NegotiationId },
							null,
							contraNegotiationRow.RowVersion,
							StatusMap.FromCode(Status.Accepted));

						// This will update the counter party Match record to show that it was accepted.
						dataModel.UpdateMatch(
							null,
							null,
							null,
							null,
							null,
							null,
							null,
							new Object[] { contraNegotiationRow.MatchId },
							contraMatchRow.RowVersion,
							StatusMap.FromCode(Status.Accepted),
							null);

						// This match has been confirmed so the timer can be terminated.
						MatchTimerRow contraMatchTimerRow = DataModel.MatchTimer.MatchTimerKey.Find(contraNegotiationRow.MatchId);
						if (contraMatchTimerRow != null)
						{
							contraMatchTimerRow.AcquireReaderLock(dataModelTransaction);
							dataModel.DestroyMatchTimer(new Object[] { contraMatchTimerRow.MatchId }, contraMatchTimerRow.RowVersion);
						}

						// The next step in handling a successful negotiation is to determine if the Working Order should be put to sleep or remain in the
						// box.  An order is put to sleep if the trader got everything they wanted in the transaction.  If so, it is assumed that the trader 
						// isn't interested in giving away any more of their cards at this point.  Also, the dark quantity left should be more than the minimum
						// for the given security.  This will calculate the quantity left on this order to determine whether it should be taken out of the box.
						Decimal quantityLeaves = workingOrderRow.SubmittedQuantity;
						foreach (DestinationOrderRow destinationOrderRow in workingOrderRow.GetDestinationOrderRows())
						{
							destinationOrderRow.AcquireReaderLock(dataModelTransaction);
							foreach (ExecutionRow executionRow in destinationOrderRow.GetExecutionRows())
							{
								executionRow.AcquireReaderLock(dataModelTransaction);
								quantityLeaves -= executionRow.ExecutionQuantity;
							}
						}
						isWorkingOrderTired = negotiationRow.Quantity == matchedQuantity &&
							!workingOrderRow.SecurityRowByFK_Security_WorkingOrder_SecurityId.IsMinimumQuantityNull() &&
							quantityLeaves >= workingOrderRow.SecurityRowByFK_Security_WorkingOrder_SecurityId.MinimumQuantity;

						// The same calculation is made for the counter party to determine if they should be left in the pool or put to sleep.
						Decimal contraLeaves = contraOrderRow.SubmittedQuantity;
						foreach (DestinationOrderRow destinationOrderRow in contraOrderRow.GetDestinationOrderRows())
						{
							destinationOrderRow.AcquireReaderLock(dataModelTransaction);
							foreach (ExecutionRow executionRow in destinationOrderRow.GetExecutionRows())
							{
								executionRow.AcquireReaderLock(dataModelTransaction);
								contraLeaves -= executionRow.ExecutionQuantity;
							}
						}
						isContraOrderTired = contraNegotiationRow.Quantity == matchedQuantity &&
							!workingOrderRow.SecurityRowByFK_Security_WorkingOrder_SecurityId.IsMinimumQuantityNull() &&
							contraLeaves >= workingOrderRow.SecurityRowByFK_Security_WorkingOrder_SecurityId.MinimumQuantity;

					}
					else
					{

						// At this point, one of the parties has declined the chance to trade.  It doesn't matter which one.  Both matches need to reflect the
						// fact that the proposed transaction was declined.
						if (matchRow.StatusId != StatusMap.FromCode(Status.Declined))
							dataModel.UpdateMatch(null, null, null, null, null, null, null, new Object[] { matchRow.MatchId }, matchRow.RowVersion, StatusMap.FromCode(Status.Declined), null);

						// This will update the contra match to show that it was declined.
						if (contraMatchRow.StatusId != StatusMap.FromCode(Status.Declined))
							dataModel.UpdateMatch(
								null,
								null,
								null,
								null,
								null,
								null,
								null,
								new Object[] { contraMatchRow.MatchId },
								contraMatchRow.RowVersion,
								StatusMap.FromCode(Status.Declined),
								null);

					}

					// This will put the order to sleep if the offer was declined or if it was accepted and the trader got everything they were looking to get
					// from this negotiation session.
					if (negotiationRow.StatusId == StatusMap.FromCode(Status.Declined) || isWorkingOrderTired)
					{

						// The user who rejected the offer is put to sleep for the default time.  This section will initialize the timer to wake the order up
						// after an amount of time defined by the user.
						TraderRow traderRow = DataModel.Trader.TraderKey.Find(workingOrderRow.CreatedUserId);
						if (traderRow != null)
						{

							traderRow.AcquireReaderLock(dataModelTransaction);

							DateTime wakeupTime = currentTime + (traderRow == null ? TimeSpan.FromSeconds(EquityCross.marketSleep) :
								TimeSpan.FromSeconds(traderRow.MarketSleep));

							Guid workingOrderTimerId = Guid.NewGuid();
							dataModel.CreateWorkingOrderTimer(currentTime, wakeupTime, workingOrderRow.WorkingOrderId,
								workingOrderTimerId);

							// This will put the Working Order to sleep.
							dataModel.UpdateWorkingOrder(
								null,
								null,
								null,
								null,
								null,
								null,
								null,
								null,
								false,
								null,
								null,
								null,
								null,
								null,
								null,
								null,
								workingOrderRow.RowVersion,
								null,
								null,
								null,
								null,
								null,
								null,
								null,
								null,
								null,
								null,
								null,
								null,
								null,
								null,
								new Object[] { workingOrderRow.WorkingOrderId });

						}

					}

					// This will put the contra order to sleep if the counter offer was declined or if it was accepted and the 
					// contra trader got everything they were looking for from this negotiation session.
					if (contraNegotiationRow.StatusId == StatusMap.FromCode(Status.Declined) || isContraOrderTired)
					{

						// The user who rejected the offer is put to sleep for the default time.  This section will initialize the
						// timer to wake the order up after an amount of time defined by the user.
						TraderRow traderRow = DataModel.Trader.TraderKey.Find(workingOrderRow.CreatedUserId);
						if (traderRow != null)
						{
							traderRow.AcquireReaderLock(dataModelTransaction);

							DateTime wakeupTime = currentTime + (traderRow == null ? TimeSpan.FromSeconds(EquityCross.marketSleep) :
								TimeSpan.FromSeconds(traderRow.MarketSleep));

							Guid workingOrderTimerId = Guid.NewGuid();
							dataModel.CreateWorkingOrderTimer(currentTime, wakeupTime, contraOrderRow.WorkingOrderId,
								workingOrderTimerId);

							// This will put the Working Order to sleep.
							dataModel.UpdateWorkingOrder(
								null,
								null,
								null,
								null,
								null,
								null,
								null,
								null,
								false,
								null,
								null,
								null,
								null,
								null,
								null,
								null,
								workingOrderRow.RowVersion,
								null,
								null,
								null,
								null,
								null,
								null,
								null,
								null,
								null,
								null,
								null,
								null,
								null,
								null,
								new Object[] { workingOrderRow.WorkingOrderId });

						}

					}

				}

				// The modification to the negotiation status of both parties is committed as a unit.
				transactionScope.Complete();

			}

		}

		/// <summary>
		/// Handles a change to a record in the Negotiation table.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event data.</param>
		private static void OnNegotiationRowChanging(object sender, NegotiationRowChangeEventArgs e)
		{

			// When a negotiation record is created it needs to be passed on to the crossing thread.  There it will be examined to see if any of the values
			// need to pass through the Chinese Wall.
			if (e.Action == DataRowAction.Commit)
				if (!e.Row.HasVersion(DataRowVersion.Original) && e.Row.RowState != DataRowState.Detached)
				{
					Guid negotiationId = (Guid)e.Row[DataModel.Negotiation.NegotiationIdColumn];
					EquityCross.actionQueue.Enqueue(new ObjectAction(EquityCross.NegotiateCross, new Object[] { negotiationId }));
				}

		}

		/// <summary>
		/// Handles a change to a record in the WorkingOrder table.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="workingOrderRowChangeEventArgs">The event data.</param>
		private static void OnWorkingOrderRowChanging(object sender, WorkingOrderRowChangeEventArgs workingOrderRowChangeEventArgs)
		{

			// When a working order is committed it will be examined to see if any of the properties that control crossing have changed.  A change in any of
			// these parameters indicates that the order should be re-examined for possible matches.
			if (workingOrderRowChangeEventArgs.Action == DataRowAction.Commit && workingOrderRowChangeEventArgs.Row.RowState != DataRowState.Deleted)
			{

				// Extract the unique working order identifier from the generic event arguments.  The identifier is needed for the handler that creates crosses
				// when the right conditions occur.
				WorkingOrderRow workingOrderRow = workingOrderRowChangeEventArgs.Row;
				Guid workingOrderId = (Guid)workingOrderRow[DataModel.WorkingOrder.WorkingOrderIdColumn];

				// Brand new orders will be examined for crosses automatically.  Existing orders will check all the matching properties for changes before
				// calling out to the thread that looks for crosses.
				if (!workingOrderRow.HasVersion(DataRowVersion.Original))
				{
					Guid currentStatus = (Guid)workingOrderRow[DataModel.WorkingOrder.StatusIdColumn, DataRowVersion.Current];
					if (currentStatus == StatusMap.FromCode(Status.Submitted))
						EquityCross.actionQueue.Enqueue(new ObjectAction(EquityCross.CrossWorkingOrder, new Object[] { workingOrderId }));
				}
				else
				{

					// Extract the status.
					Guid previousStatusId = (Guid)workingOrderRow[DataModel.WorkingOrder.StatusIdColumn, DataRowVersion.Original];
					Guid currentStatusId = (Guid)workingOrderRow[DataModel.WorkingOrder.StatusIdColumn, DataRowVersion.Current];

					// Extract the submitted quantity.
					Decimal previousQuantity = (Decimal)workingOrderRow[DataModel.WorkingOrder.SubmittedQuantityColumn, DataRowVersion.Original];
					Decimal currentQuantity = (Decimal)workingOrderRow[DataModel.WorkingOrder.SubmittedQuantityColumn, DataRowVersion.Current];

					// Extract the state of matching against brokers.
					Boolean previousIsBrokerMatch = (Boolean)workingOrderRow[DataModel.WorkingOrder.IsBrokerMatchColumn, DataRowVersion.Original];
					Boolean currentIsBrokerMatch = (Boolean)workingOrderRow[DataModel.WorkingOrder.IsBrokerMatchColumn, DataRowVersion.Current];

					// Extract the state of matching against hedge funds.
					Boolean previousIsHedgeMatch = (Boolean)workingOrderRow[DataModel.WorkingOrder.IsHedgeMatchColumn, DataRowVersion.Original];
					Boolean currentIsHedgeMatch = (Boolean)workingOrderRow[DataModel.WorkingOrder.IsHedgeMatchColumn, DataRowVersion.Current];

					// Extract the state of matching against instutitions.
					Boolean previousIsInstitutionMatch = (Boolean)workingOrderRow[DataModel.WorkingOrder.IsInstitutionMatchColumn, DataRowVersion.Original];
					Boolean currentIsInstitutionMatch = (Boolean)workingOrderRow[DataModel.WorkingOrder.IsInstitutionMatchColumn, DataRowVersion.Current];

					// This will ask the crossing thread to examine the order for possible matches when any of the critical properties for crossing have
					// changed.
					if (previousStatusId != currentStatusId ||
						previousQuantity != currentQuantity ||
						previousIsBrokerMatch != currentIsBrokerMatch ||
						previousIsInstitutionMatch != currentIsInstitutionMatch ||
						previousIsHedgeMatch != currentIsHedgeMatch)
						EquityCross.actionQueue.Enqueue(new ObjectAction(EquityCross.CrossWorkingOrder, new Object[] { workingOrderId }));

				}

			}

		}

		/// <summary>
		/// Starts the exchange.
		/// </summary>
		public void Start()
		{

			// These event handlers will update the matching conditions as the underlying records change.
			DataModel.Negotiation.NegotiationRowChanging += OnNegotiationRowChanging;
			DataModel.WorkingOrder.WorkingOrderRowChanging += OnWorkingOrderRowChanging;

			// This thread will execution the actions that are created by changes to the data model.  The triggers themselves can't modify the data
			// model because the triggers are called from the commit handlers.
			// HACK - Removed to prevent collision with Debt Cross.
			//EquityCross.crossingThread = new Thread(new ThreadStart(EquityCross.CrossingThread));
			//EquityCross.crossingThread.Name = "Crossing Thread";
			//EquityCross.crossingThread.IsBackground = true;
			//EquityCross.crossingThread.Start();

		}

		/// <summary>
		/// Stops the exchange.
		/// </summary>
		public void Stop()
		{

			// These event handlers must be removed from the data model.
			DataModel.Negotiation.NegotiationRowChanging -= OnNegotiationRowChanging;
			DataModel.WorkingOrder.WorkingOrderRowChanging -= OnWorkingOrderRowChanging;

			// Shut down thread that handles the trigger driven actions.
			// HACK - Removed to prevent collision with Debt Cross.
			//if (!EquityCross.crossingThread.Join(EquityCross.threadWait))
			//    EquityCross.crossingThread.Abort();

		}

		/// <summary>
		/// Suspend the exchange.
		/// </summary>
		public void Suspend()
		{

		}

	}

}