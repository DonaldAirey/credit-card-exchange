namespace FluidTrade.Guardian
{

	using System;
	using System.Transactions;

	/// <summary>
	/// Summary description for Negotiation.
	/// </summary>
	internal class Negotiation
	{

		private static Guid destinationId;

		static Negotiation()
		{

			try
			{

				// Lock the data model while the object initializes itself.
				DataModel.DataLock.EnterReadLock();

				// The destination for all orders matched through this system is fixed by a setting in the configuration file.
				DestinationRow destinationRow = DataModel.Destination.DestinationKeyExternalId0.Find(Guardian.Properties.Settings.Default.GuardianECN);
				if (destinationRow == null)
					throw new Exception("There is no Destination specified in the configuration file for matched trades.");
				Negotiation.destinationId = destinationRow.DestinationId;

			}
			finally
			{

				// Release the tables.
				DataModel.DataLock.ExitReadLock();

			}

		}
		
		internal static void Decline(Guid matchId)
		{

			Guid negotiationId = Guid.Empty;
			long rowVersion = long.MinValue;

			DataModel dataModel = new DataModel();

			try
			{

				DataModel.DataLock.EnterReadLock();

				// This context is used to keep track of the locks aquired for the ancillary data.
				TransactionScope transactionScope = new TransactionScope();
				DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

				MatchRow matchRow = DataModel.Match.MatchKey.Find(matchId);
				if (matchRow != null)
				{

					WorkingOrderRow workingOrderRow = matchRow.WorkingOrderRow;

					// See if there is already a pending offer.
					bool isFound = false;
					foreach (NegotiationRow innerNegotiationRow in matchRow.GetNegotiationRows())
					{

						if (innerNegotiationRow.StatusId == StatusMap.FromCode(Status.Declined))
							throw new Exception("This offer has previously been declined.");

						if (innerNegotiationRow.StatusId == StatusMap.FromCode(Status.Pending))
						{

							// Call the internal method to complete the operation.
							rowVersion = innerNegotiationRow.RowVersion;
							negotiationId = innerNegotiationRow.NegotiationId;
							dataModel.UpdateNegotiation(
								null, 
								null,
								false,
								null, 
								null, 
								new Object[] { negotiationId }, 
								null, 
								rowVersion,
								StatusMap.FromCode(Status.Declined));
							isFound = true;

						}

					}

					// Call the internal method to complete the operation.
					if (!isFound)
					{
						negotiationId = Guid.NewGuid();
						dataModel.CreateNegotiation(workingOrderRow.BlotterId, 
							null,
							false,
							matchId, 
							negotiationId, 
							0.0m, 
							StatusMap.FromCode(Status.Declined));
					}

					// If there's a counter offer, then notify the couter part that the offer has been declined. This will find the contra matching record.
					MatchRow contraMatchRow = DataModel.Match.MatchKeyWorkingOrderIdContraOrderId.Find(
						new object[] { matchRow.ContraOrderId, matchRow.WorkingOrderId });
					if (contraMatchRow == null)
						throw new Exception(string.Format("Corruption: the match record for {0}, {1} can't be found", matchRow.ContraOrderId, matchRow.WorkingOrderId));

					// When both sides have agreed to the Negotiation, the Destination Orders are generated.
					foreach (NegotiationRow contraNegotiationRow in contraMatchRow.GetNegotiationRows())
						if (contraNegotiationRow.StatusId == StatusMap.FromCode(Status.Pending))
						{
							dataModel.UpdateNegotiation(
								null,
								null,
								false,
								null,
								null,
								new Object[] {contraNegotiationRow.NegotiationId},
								null,
								contraNegotiationRow.RowVersion,
								StatusMap.FromCode(Status.Declined));
						}

				}

			}
			finally
			{

				// Release the tables.
				DataModel.DataLock.ExitReadLock();

			}

		}

		/// <summary>Inserts a Negotiation record using Metadata Parameters.</summary>		
		internal static void Offer(Guid matchId, Decimal quantity)
		{

			Guid negotiationId = Guid.Empty;
			DataModel dataModel = new DataModel();

			try
			{

				DataModel.DataLock.EnterReadLock();

				MatchRow matchRow = DataModel.Match.MatchKey.Find(matchId);
				if (matchRow != null)
				{

					// Rule #1: Insure that there are no pending offers.
					foreach (NegotiationRow innerNegotiationRow in matchRow.GetNegotiationRows())
					{

						if (innerNegotiationRow.StatusId == StatusMap.FromCode(Status.Pending))
							throw new Exception("There is already an offer pending.");

						if (innerNegotiationRow.StatusId == StatusMap.FromCode(Status.Declined))
							throw new Exception("This offer has previously been declined.");

					}

					// Time stamps and user stamps
					Guid createdUserId = TradingSupport.UserId;
					DateTime createdTime = DateTime.UtcNow;
					Guid modifiedUserId = TradingSupport.UserId;
					DateTime modifiedTime = createdTime;

					// This will find the contra matching record.
					MatchRow contraMatchRow = DataModel.Match.MatchKeyWorkingOrderIdContraOrderId.Find(
						new Object[] { matchRow.ContraOrderId, matchRow.WorkingOrderId });
					if (contraMatchRow == null)
						throw new Exception(
							string.Format("Corruption: the match record for {0}, {1} can't be found", matchRow.ContraOrderId, matchRow.WorkingOrderId));

					// This is the order on the other side of the match.
					WorkingOrderRow contraWorkingOrderRow = contraMatchRow.WorkingOrderRow;

					// When both sides have agreed to the Negotiation, the Destination Orders are generated.
					NegotiationRow contraNegotiationRow = null;
					foreach (NegotiationRow innerNegotiationRow in contraMatchRow.GetNegotiationRows())
						if (innerNegotiationRow.StatusId == StatusMap.FromCode(Status.Pending))
						{
							contraNegotiationRow = innerNegotiationRow;
							break;
						}

					// This means that there's an offer on the other side.
					if (contraNegotiationRow == null)
					{

						// There is no opposite side of this transaction yet.  It will be placed in the negotation table and wait there
						// until it times out, or the other side accepts the offer.
						negotiationId = Guid.NewGuid();
						dataModel.CreateNegotiation(
							contraWorkingOrderRow.BlotterId, 
							null, 
							false,
							contraMatchRow.MatchId, 
							negotiationId, 
							quantity, 
							StatusMap.FromCode(Status.Pending));

					}
					else
					{

						// At this point, there is an offer on both sides of the match for a follow-on order.  We'll create orders and 
						// executions for both sides of the trade for the minimum agreed upon quantity.
						WorkingOrderRow workingOrderRow = matchRow.WorkingOrderRow;
						WorkingOrderRow contraOrderRow = contraNegotiationRow.MatchRow.WorkingOrderRow;

						// The quantity of this negotiation will be the minimum of the two offers.
						decimal matchedQuantity = quantity < contraNegotiationRow.Quantity ? quantity : contraNegotiationRow.Quantity;

						// Create the order on this side of the trade.
						Guid destinationOrderId = Guid.NewGuid();
						dataModel.CreateDestinationOrder(
							workingOrderRow.BlotterId,
							null,
							null,
							createdTime,
							createdUserId,
							destinationId,
							destinationOrderId,
							null,
							null,
							null,
							null,
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
							null,
							workingOrderRow.WorkingOrderId);

						// The the trade is executed at the current price.
						PriceRow priceRow = DataModel.Price.PriceKey.Find(workingOrderRow.SecurityId, workingOrderRow.SettlementId);

						// Create the Execution for this side of the trade.
						Guid executionId = Guid.NewGuid();
						dataModel.CreateExecution(
							null,
							workingOrderRow.BlotterId,
							null,
							null,
							null,
							createdTime,
							createdUserId,
							destinationOrderId,
							StateMap.FromCode(State.Acknowledged),
							executionId,
							priceRow.LastPrice,
							matchedQuantity,
							null,
							null,
							null,
							modifiedTime,
							modifiedUserId,
							null,
							null,
							null,
							null,
							StateMap.FromCode(State.Sent),
							null,
							null,
							null,
							null);

						// There is no opposite side of this transaction yet.  It will be placed in the negotation table and wait there
						// until it times out, or the other side accepts the offer.
						negotiationId = Guid.NewGuid();
						dataModel.CreateNegotiation(
							workingOrderRow.BlotterId, 
							executionId, 
							false,
							matchId, 
							negotiationId, 
							quantity, 
							StatusMap.FromCode(Status.Accepted));

						// Create an order for the agreed upon quantity.
						// Create the order on this side of the trade.
						Guid contraDestinationOrderId = Guid.NewGuid();
						dataModel.CreateDestinationOrder(
							contraWorkingOrderRow.BlotterId,
							null,
							null,
							createdTime,
							createdUserId,
							destinationId,
							contraDestinationOrderId,
							null,
							null,
							null,
							null,
							contraWorkingOrderRow[DataModel.WorkingOrder.LimitPriceColumn],
							modifiedTime,
							modifiedUserId,
							matchedQuantity,
							contraWorkingOrderRow.OrderTypeId,
							contraWorkingOrderRow.SecurityId,
							modifiedTime,
							contraWorkingOrderRow.SettlementId,
							contraWorkingOrderRow.SideId,
							StateMap.FromCode(State.Acknowledged),
							StatusMap.FromCode(Status.New),
							contraWorkingOrderRow[DataModel.WorkingOrder.StopPriceColumn],
							contraWorkingOrderRow.TimeInForceId,
							modifiedTime,
							null,
							contraWorkingOrderRow.WorkingOrderId);

						// Create the Execution for this side of the trade.
						Guid contraExecutionId = Guid.NewGuid();
						dataModel.CreateExecution(
							null,
							contraWorkingOrderRow.BlotterId,
							null,
							null,
							null,
							createdTime,
							createdUserId,
							contraDestinationOrderId,
							StateMap.FromCode(State.Acknowledged),
							contraExecutionId,
							priceRow.LastPrice,
							matchedQuantity,
							null,
							null,
							null,
							modifiedTime,
							modifiedUserId,
							null,
							null,
							null,
							null,
							StateMap.FromCode(State.Sent),
							null,
							null,
							null,
							null);

						// Update the contra offer.
						dataModel.UpdateNegotiation(
							contraWorkingOrderRow.BlotterId,
							contraExecutionId,
							false,
							null,
							null,
							new Object[] {contraNegotiationRow.NegotiationId},
							null,
							contraNegotiationRow.RowVersion,
							StatusMap.FromCode(Status.Accepted));

					}

				}

			}
			finally
			{

				// Release the tables.
				DataModel.DataLock.ExitReadLock();

			}

		}

	}

}
