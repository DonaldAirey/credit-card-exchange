namespace FluidTrade.Guardian
{
	using System;
	using FluidTrade.Core;
	using FluidTrade.Guardian.Records;


	/// <summary>
	/// 
	/// </summary>
	internal class DebtHolderImportTranslationPersistence : DataModelPersistence<DebtHolderImportTranslation>
	{

		/// <summary>
		/// Constructor
		/// </summary>
		public DebtHolderImportTranslationPersistence()
		{
		}
		/// <summary>
		/// Create a new Consumer Trust
		/// </summary>
		/// <returns></returns>
		public override Guid Create(DebtHolderImportTranslation record)
		{

			DataModel dataModel = new DataModel();
			Guid entityId = Guid.NewGuid();

			// Since DebtHolder object requires entries in Blotter, DebtClass and DebtHolder, 
			//a transaction is required to lock the records and change the data model.

			DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

			////Create a entry in credit card
			dataModel.CreateDebtHolderImportTranslation(
				record.AccountBalance,
				record.AccountCode,
				record.Address1,
				record.Address2,
				record.City,
				record.DateOfBirth,
				record.DateOfDelinquency,
				record.DebtHolder,
				entityId,
				record.ExternalId,
				record.FirstName,
				record.LastName,
				record.MiddleName,
				record.OriginalAccountNumber,
				record.PhoneNumber,
				record.PostalCode,
				record.ProvinceCode,
				record.SocialSecurityNumber,
				record.Suffix,
				record.VendorCode);



			return entityId;

		}

		public override DebtHolderImportTranslation Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public override void Update(DebtHolderImportTranslation record)
		{
			throw new NotImplementedException();
		}

		public override ErrorCode Delete(DebtHolderImportTranslation record)
		{
			throw new NotImplementedException();
		}
	}
		
}
