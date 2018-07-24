namespace FluidTrade.Guardian
{
	using System;
	using System.ServiceModel;
	using FluidTrade.Core;
	using FluidTrade.Guardian.Records;


	/// <summary>
	/// 
	/// </summary>
	internal class ProvincePersistence : DataModelPersistence<Province>
	{

		/// <summary>
		/// Constructor
		/// </summary>
		public ProvincePersistence()
		{
		}

		/// <summary>
		/// Create a new DebtHolder
		/// </summary>
		/// <returns></returns>
		public override Guid Create(Province record)
		{
			DataModel dataModel = new DataModel();
			Guid provinceId = Guid.NewGuid();

			DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

			////Create a entry in Province			
			dataModel.CreateProvince(
				record.Abbreviation,
				record.CountryId.GetValueOrDefault(),
				record.ExternalId0,
				record.ExternalId1,
				record.Name,
				provinceId);

			return provinceId;

		}

		/// <summary>
		/// Update Province
		/// </summary>
		/// <returns></returns>
		public override void Update(Province record)
		{
			DataModel dataModel = new DataModel();

			//Sanity check
			if (record.RowId == null || DataModel.Province.ProvinceKey.Find(record.RowId) == null)
			{
				throw new FaultException<RecordNotFoundFault>(new RecordNotFoundFault("Province", new object[] { record.RowId }));
			}

			dataModel.UpdateProvince(
				record.Abbreviation,
				record.CountryId,
				record.ExternalId0,
				record.ExternalId1,
				record.Name,
				null,
				new object[] { record.RowId },
				record.RowVersion);
		}

		/// <summary>
		/// Delete a Province
		/// </summary>
		/// <returns>True for sucess</returns>
		public override ErrorCode Delete(Province record)
		{
			DataModel dataModel = new DataModel();

			if (record.RowId == null || DataModel.Province.ProvinceKey.Find(record.RowId) == null)
			{
				return ErrorCode.RecordNotFound;
			}

			dataModel.DestroyProvince(
					new object[] { record.RowId },
					record.RowVersion);

			return ErrorCode.Success;
		}


		public override Province Get(Guid id)
		{
			throw new NotImplementedException();
		}
	}
}
