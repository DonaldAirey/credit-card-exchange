using System;
using System.Security;
using System.ServiceModel;
using FluidTrade.Core;

namespace FluidTrade.Guardian
{
	/// <summary>
	/// Handles CRUD operations for Negotiation records
	/// </summary>
	internal class NegotiationPersistence : DataModelPersistence<Records.Negotiation>
	{
		/// <summary>
		/// Public constructor for generic methods to create this object
		/// </summary>
		public NegotiationPersistence()
		{
		
		}

		/// <summary>
		/// Create Negotiation record
		/// </summary>
		/// <param name="record"></param>
		/// <returns></returns>
		public override Guid Create(FluidTrade.Guardian.Records.Negotiation record)
		{
			DataModel dataModel = new DataModel();
			Guid negotiationId = Guid.NewGuid();

			DataModelTransaction dataModelTransaction = DataModelTransaction.Current;
			Guid blotter = record.BlotterId.GetValueOrDefault();
			if (blotter == Guid.Empty || !TradingSupport.HasAccess(dataModelTransaction, blotter, AccessRight.Write))
				throw new SecurityException("Current user does not have write access to the selected blotter");

			////Create a negotiation
			dataModel.CreateNegotiation(
				blotter,
				record.ExecutionId.GetValueOrDefault(),
				record.IsRead,
				record.MatchId.GetValueOrDefault(),
				negotiationId,
				record.Quantity,
				record.StatusCodeId.GetValueOrDefault());

			return negotiationId;
		}

		/// <summary>
		/// Get  Negotiation record
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public override FluidTrade.Guardian.Records.Negotiation Get(Guid id)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Update Negotiation record
		/// </summary>
		/// <param name="record"></param>
		public override void Update(FluidTrade.Guardian.Records.Negotiation record)
		{
			DataModel dataModel = new DataModel();

			DataModelTransaction dataModelTransaction = DataModelTransaction.Current;
			Guid blotter = record.BlotterId.GetValueOrDefault();
			if (blotter == Guid.Empty || !TradingSupport.HasAccess(dataModelTransaction, blotter, AccessRight.Write))
				throw new SecurityException("Current user does not have write access to the selected blotter");

			if (record.RowId == null || DataModel.Negotiation.NegotiationKey.Find(record.RowId) == null)
			{
				throw new FaultException<RecordNotFoundFault>(new RecordNotFoundFault("Negotiation", new object[] { record.RowId }));
			}

			dataModel.UpdateNegotiation(
				record.BlotterId,
				record.ExecutionId,
				record.IsRead,
				record.MatchId,
				null,
				new object[] { record.RowId },
				record.Quantity,
				record.RowVersion,
				record.StatusCodeId);
		}

		/// <summary>
		/// Delete Negotiation record
		/// </summary>
		/// <param name="record"></param>
		/// <returns></returns>
		public override ErrorCode Delete(FluidTrade.Guardian.Records.Negotiation record)
		{
			DataModel dataModel = new DataModel();

			if (record.RowId == null || DataModel.Negotiation.NegotiationKey.Find(record.RowId) == null)
			{
				return ErrorCode.RecordNotFound;
			}

			dataModel.DestroyNegotiation(
					new object[] { record.RowId },
					record.RowVersion);

			return ErrorCode.Success;
		}
	}
}
