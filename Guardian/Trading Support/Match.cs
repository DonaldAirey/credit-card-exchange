namespace FluidTrade.Guardian
{

	using System;
	using System.Transactions;

	/// <summary>
	/// Summary description for Match.
	/// </summary>
	internal class Match
	{

		private static Guid destinationId;

		static Match()
		{

			try
			{

				// Lock the data model while the object initializes itself.
				DataModel.DataLock.EnterReadLock();

				// The destination for all orders matched through this system is fixed by a setting in the configuration file.
				DestinationRow destinationRow = DataModel.Destination.DestinationKeyExternalId0.Find(Guardian.Properties.Settings.Default.GuardianECN);
				if (destinationRow == null)
					throw new Exception("There is no Destination specified in the configuration file for matched trades.");
				Match.destinationId = destinationRow.DestinationId;

			}
			finally
			{

				// Release the tables.
				DataModel.DataLock.ExitReadLock();

			}

		}
				
		internal static void Decline(Guid matchId, Int64 rowVersion)
		{

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

					// Time stamps and user stamps
					Guid modifiedUserId = TradingSupport.UserId;
					DateTime modifiedTime = DateTime.UtcNow;

					// Call the internal method to complete the operation.
					dataModel.UpdateMatch(null, null, null, null, null, null, null, new Object[] { matchRow.MatchId }, rowVersion, StatusMap.FromCode(Status.Declined), null);

					// Call the internal method to decline the contra side of this match.
					foreach (MatchRow contraMatchRow in DataModel.Match.Rows)
						if (contraMatchRow.WorkingOrderId == matchRow.ContraOrderId && contraMatchRow.ContraOrderId == matchRow.WorkingOrderId)
						{
							rowVersion = contraMatchRow.RowVersion;
							dataModel.UpdateMatch(null, null, null, null, null, null, null, new Object[] { contraMatchRow.MatchId }, rowVersion, StatusMap.FromCode(Status.Declined), null);
						}

				}

			}
			finally
			{

				// Release the tables.
				DataModel.DataLock.ExitReadLock();

			}

		}
		
		internal static void Accept(Guid matchId, Int64 rowVersion)
		{

			DataModel dataModel = new DataModel();

			try
			{

				DataModel.DataLock.EnterReadLock();

				MatchRow matchRow = DataModel.Match.MatchKey.Find(matchId);
				if (matchRow != null)
				{

					// Time stamps and user stamps
					Guid createdUserId = TradingSupport.UserId;
					DateTime createdTime = DateTime.UtcNow;
					Guid modifiedUserId = createdUserId;
					DateTime modifiedTime = createdTime;

					// Call the internal method to complete the operation.
					dataModel.UpdateMatch(null, null, null, null, null, null, null, new Object[] { matchRow.MatchId }, rowVersion, StatusMap.FromCode(Status.Accepted), null);

					// This is the working order associated with the match.
					WorkingOrderRow workingOrderRow = matchRow.WorkingOrderRow;

					// This will find the contra order.
					MatchRow contraMatchRow = DataModel.Match.MatchKeyWorkingOrderIdContraOrderId.Find(
						matchRow.ContraOrderId,
						matchRow.WorkingOrderId);
					if (contraMatchRow == null)
						throw new Exception(string.Format("Corruption: the match record for {0}, {1} can't be found", matchRow.ContraOrderId, matchRow.WorkingOrderId));

					// When both sides have agreed to the match, the Destination Orders are generated.
					if (contraMatchRow.StatusId != StatusMap.FromCode(Status.Accepted))
						return;

					// At this point, both sides have agreed to a trade.  This is the working order of the contra side of the trade.
					WorkingOrderRow contraOrderRow = contraMatchRow.WorkingOrderRow;

					// The quantity of this transaction is the lesser of the two sides of the trade.
					decimal quantity = workingOrderRow.SubmittedQuantity < contraOrderRow.SubmittedQuantity ?
						workingOrderRow.SubmittedQuantity :
						contraOrderRow.SubmittedQuantity;

					PriceRow priceRow = DataModel.Price.PriceKey.Find(workingOrderRow.SecurityId, workingOrderRow.SettlementId);
					if (priceRow == null)
						throw new Exception("The price of this trade can't be determined.");

					Guid destinationOrderId = Guid.NewGuid();
					dataModel.CreateDestinationOrder(
						workingOrderRow.BlotterId,
						null,
						null,
						createdTime,
						createdUserId,
						Match.destinationId,
						destinationOrderId,
						null,
						null,
						false,
						false,
						workingOrderRow[DataModel.WorkingOrder.LimitPriceColumn],
						modifiedTime,
						modifiedUserId,
						quantity,
						workingOrderRow.OrderTypeId,
						workingOrderRow.SecurityId,
						createdTime,
						workingOrderRow.SettlementId,
						workingOrderRow.SideId,
						StateMap.FromCode(State.Acknowledged),
						StatusMap.FromCode(Status.New),
						workingOrderRow[DataModel.WorkingOrder.StopPriceColumn],
						workingOrderRow.TimeInForceId,
						createdTime,
						createdUserId,
						workingOrderRow.WorkingOrderId);

					Guid executionId = Guid.NewGuid();
					dataModel.CreateExecution(
						0.0m,
						workingOrderRow.BlotterId,
						null,
						null,
						0.0m,
						createdTime,
						createdUserId,
						destinationOrderId,
						StateMap.FromCode(State.Acknowledged),
						executionId,
						priceRow.LastPrice,
						quantity,
						null,
						null,
						false,
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

					dataModel.UpdateMatch(null, null, null, null, null, null, null, new Object[] { matchId }, matchRow.RowVersion, StatusMap.FromCode(Status.Accepted), null);

					Guid contraDestinationOrderId = Guid.NewGuid();
					dataModel.CreateDestinationOrder(
						contraOrderRow.BlotterId,
						0.0m,
						null,
						createdTime,
						createdUserId,
						Match.destinationId,
						contraDestinationOrderId,
						null,
						null,
						false,
						false,
						contraOrderRow[DataModel.WorkingOrder.LimitPriceColumn],
						modifiedTime,
						modifiedUserId,
						quantity,
						contraOrderRow.OrderTypeId,
						contraOrderRow.SecurityId,
						createdTime,
						contraOrderRow.SettlementId,
						contraOrderRow.SideId,
						StateMap.FromCode(State.Acknowledged),
						StatusMap.FromCode(Status.New),
						contraOrderRow[DataModel.WorkingOrder.StopPriceColumn],
						contraOrderRow.TimeInForceId,
						createdTime,
						createdUserId,
						contraOrderRow.WorkingOrderId);

					Guid contraExecutionId = Guid.NewGuid();
					dataModel.CreateExecution(
						0.0m,
						contraOrderRow.BlotterId,
						null,
						null,
						0.0m,
						createdTime,
						createdUserId,
						contraDestinationOrderId,
						StateMap.FromCode(State.Acknowledged),
						contraExecutionId,
						priceRow.LastPrice,
						quantity,
						null,
						null,
						false,
						modifiedTime,
						modifiedUserId,
						null,
						null,
						null,
						null,
						StateMap.FromCode(State.Sent),
						0.0m,
						0.0m,
						0.0m,
						0.0m);

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
