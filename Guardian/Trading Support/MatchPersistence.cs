namespace FluidTrade.Guardian
{
	using System;
	using System.Security;
	using System.ServiceModel;
	using FluidTrade.Core;


	/// <summary>
	/// 
	/// </summary>
	internal class MatchPersistence : DataModelPersistence<Records.Match>
	{


		/// <summary>
		/// Constructor
		/// </summary>
		internal MatchPersistence()
		{
		}

		/// <summary>
		/// Create a new Math record
		/// </summary>
		/// <returns></returns>
		public override Guid Create(Records.Match record)
		{
			DataModel dataModel = new DataModel();
			Guid matchId = Guid.NewGuid();

			DataModelTransaction dataModelTransaction = DataModelTransaction.Current;
			Guid blotter = record.BlotterId.GetValueOrDefault();
			if (blotter == Guid.Empty || !TradingSupport.HasAccess(dataModelTransaction, blotter, AccessRight.Write))
				throw new SecurityException("Current user does not have write access to the selected blotter");

			////Create a match
			dataModel.CreateMatch(
				record.BlotterId.GetValueOrDefault(),
				record.ContraMatchId.GetValueOrDefault(),
				record.ContraOrderId.GetValueOrDefault(),
				record.HeatIndex,
				record.HeatIndexDetails,
				DateTime.UtcNow,
				matchId,
				record.StatusCodeId.GetValueOrDefault(),
				record.WorkingOrderId.GetValueOrDefault());

			return matchId;

		}

		/// <summary>
		/// Update a match
		/// </summary>
		/// <returns></returns>
		public override void Update(Records.Match record)
		{
			DataModel dataModel = new DataModel();

			DataModelTransaction dataModelTransaction = DataModelTransaction.Current;
			Guid blotter = record.BlotterId.GetValueOrDefault();
			if (blotter == Guid.Empty || !TradingSupport.HasAccess(dataModelTransaction, blotter, AccessRight.Write))
				throw new SecurityException("Current user does not have write access to the selected blotter");

			if (record.RowId == null || DataModel.Match.MatchKey.Find(record.RowId) == null)
			{
				throw new FaultException<RecordNotFoundFault>(new RecordNotFoundFault("Match", new object[] { record.RowId }));
			}

			dataModel.UpdateMatch(
				record.BlotterId,
				record.ContraMatchId,
				record.ContraOrderId,
				record.HeatIndex,
				record.HeatIndexDetails,
				record.MatchedTime,
				null, new object[] { record.RowId },
				record.RowVersion,
				record.StatusCodeId,
				record.WorkingOrderId);
		}



		/// <summary>
		/// Delete a match
		/// </summary>
		/// <returns>True for sucess</returns>
		public override ErrorCode Delete(Records.Match record)
		{

			if (record.RowId == null || DataModel.Match.MatchKey.Find(record.RowId) == null)
				return ErrorCode.RecordNotFound;

			DataModel dataModel = new DataModel();
			MatchRow matchRow = DataModel.Match.MatchKey.Find(record.RowId);
			DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

			matchRow.AcquireReaderLock(dataModelTransaction);
			if (!TradingSupport.HasAccess(dataModelTransaction, matchRow.BlotterId, AccessRight.FullControl))
				return ErrorCode.AccessDenied;
			MatchRow contraMatchRow = DataModel.Match.MatchKey.Find(matchRow.ContraMatchId);
			matchRow.ReleaseReaderLock(dataModelTransaction.TransactionId);

			dataModel.DestroyMatch(
					new object[] { record.RowId },
					record.RowVersion);
			matchRow.ReleaseWriterLock(dataModelTransaction.TransactionId);

			// Delete the contra second, in case the record.RowVersion is off.
			if (contraMatchRow != null)
			{

				contraMatchRow.AcquireWriterLock(dataModelTransaction);
				dataModel.DestroyMatch(
						new object[] { contraMatchRow.MatchId },
						contraMatchRow.RowVersion);
				contraMatchRow.ReleaseWriterLock(dataModelTransaction.TransactionId);

			}

			return ErrorCode.Success;
		}

		/// <summary>
		/// Get a match
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public override Records.Match Get(Guid id)
		{
			MatchRow row = DataModel.Match.MatchKey.Find(id);
			Records.Match record = new Records.Match();

			if (row != null)
			{
				row.AcquireReaderLock(DataModelTransaction.Current);
				record.BlotterId = row.BlotterId;
				record.ContraMatchId = row.ContraMatchId;
				record.ContraOrderId = row.ContraOrderId;
				record.HeatIndex = row.HeatIndex;
				if(row.IsHeatIndexDetailsNull() == false)
					record.HeatIndexDetails = row.HeatIndexDetails;
				record.MatchedTime = row.MatchedTime;
				record.RowId = row.MatchId;
				record.RowVersion = row.RowVersion;
				record.StatusCodeId = row.StatusId;
				record.WorkingOrderId = row.WorkingOrderId;
				row.ReleaseLock(DataModelTransaction.Current.TransactionId);
			}
			return record;
		}
	}
}
