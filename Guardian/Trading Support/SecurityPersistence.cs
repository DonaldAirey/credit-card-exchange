namespace FluidTrade.Guardian
{
	using System;
	using System.ServiceModel;
	using FluidTrade.Core;
	using FluidTrade.Guardian.Records;


	/// <summary>
	/// 
	/// </summary>
	internal class SecurityPersistence : DataModelPersistence<Security>
	{

		/// <summary>
		/// Constructor
		/// </summary>
		public SecurityPersistence()
		{
		}

		/// <summary>
		/// Create a new DebtHolder
		/// </summary>
		/// <returns></returns>
		public override Guid Create(Security record)
		{
			DataModel dataModel = new DataModel();
			Guid securityId = Guid.NewGuid();

			DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

			VolumeCategoryRow volumeCategoryRow = DataModel.VolumeCategory.VolumeCategoryKey.Find(record.VolumeCategoryId);
			volumeCategoryRow.AcquireReaderLock(dataModelTransaction);

			////Create a entry in security			
			dataModel.CreateSecurity(
				record.AverageDailyVolume,
				record.CountryId.GetValueOrDefault(),
				record.Logo,
				record.MarketCapitalization,
				record.MinimumQuantity,
				record.PriceFactor,
				record.QuantityFactor,
				securityId,
				record.Symbol,
				record.TenantId,
				volumeCategoryRow.VolumeCategoryId);

			return securityId;

		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override void Update(Security record)
		{
			DataModel dataModel = new DataModel();


			if (record.RowId == null || DataModel.Security.SecurityKey.Find(record.RowId) == null)
			{
				throw new FaultException<RecordNotFoundFault>(new RecordNotFoundFault("Security", new object[] { record.RowId }));
			}

			dataModel.UpdateSecurity(
				record.AverageDailyVolume,
				record.CountryId,
				record.Logo,
				record.MarketCapitalization,
				record.MinimumQuantity,
				record.PriceFactor,
				record.QuantityFactor,
				record.RowVersion, null,
				new object[] { record.RowId },
				record.Symbol,
				null,
				record.VolumeCategoryId);
		}

		/// <summary>
		/// Delete a Seciurity
		/// </summary>
		/// <returns>True for sucess</returns>
		public override ErrorCode Delete(Security record)
		{
			DataModel dataModel = new DataModel();

			if (record.RowId == null || DataModel.Security.SecurityKey.Find(record.RowId) == null)
			{
				return ErrorCode.RecordNotFound;
			}

			dataModel.DestroySecurity(
					record.RowVersion,
					new object[] { record.RowId });

			return ErrorCode.Success;
		}


		public override Security Get(Guid id)
		{
			throw new NotImplementedException();
		}
	}
}
