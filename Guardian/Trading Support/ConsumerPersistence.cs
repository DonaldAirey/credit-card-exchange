namespace FluidTrade.Guardian
{
	using System;
	using FluidTrade.Core;
	using FluidTrade.Guardian.Records;


	/// <summary>
	/// 
	/// </summary>
	internal class ConsumerPersistence : DataModelPersistence<Consumer>
	{


		/// <summary>
		/// Constructor
		/// </summary>
		public ConsumerPersistence()
		{
		}

		/// <summary>
		/// Create a new Consumer Trust
		/// </summary>
		/// <returns></returns>
		public override Guid Create(Consumer consumer)
		{

			DataModel dataModel = new DataModel();
			Guid consumerId = Guid.NewGuid();

			DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

			dataModel.CreateConsumer(
				consumer.Address1,
				consumer.Address2,
				null,
				null,
				consumer.City,
				consumerId,
				consumer.DateOfBirth,
				consumer.ExternalId0,
				consumer.ExternalId1,
				consumer.FirstName,
				consumer.IsEmployed,
				consumer.LastName,				
				consumer.MiddleName,
				consumer.PhoneNumber,
				consumer.PostalCode,
				consumer.ProvinceId,
				consumer.Salutation,
				consumer.SocialSecurityNumber,
				consumer.Suffix);

			return consumerId;

		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override void Update(Consumer record)
		{

			// These variables are used for auditing the changes to this record.
			DateTime createdTime = DateTime.UtcNow;
			Guid createdUserId = TradingSupport.UserId;
			DateTime modifiedTime = createdTime;
			Guid modifiedUserId = createdUserId;

			DataModel dataModel = new DataModel();
			DataModelTransaction datamodelTransaction = DataModelTransaction.Current;
			if (record.RowId == null || DataModel.Consumer.ConsumerKey.Find(record.RowId) == null)
			{
				record.RowId = Create(record);
			}

			dataModel.UpdateConsumer(
				record.Address1,
				record.Address2,
				null,
				null,
				record.City,
				null,
				new object[] { record.RowId },
				record.DateOfBirth,
				record.ExternalId0,
				record.ExternalId1,
				record.FirstName,
				record.IsEmployed,
				record.LastName,				
				record.MiddleName,
				record.PhoneNumber,
				record.PostalCode,
				record.ProvinceId,
				record.RowVersion,
				record.Salutation,
				record.SocialSecurityNumber,
				record.Suffix);

				//If a working order is given, Update modifyTime.
				Guid workingOrderId = record.WorkingOrderId.GetValueOrDefault(Guid.Empty);
				if (workingOrderId != Guid.Empty)
				{
					WorkingOrderPersistence workingOrderPersistence = new WorkingOrderPersistence();
					workingOrderPersistence.UpdateModifyTime(workingOrderId);
				}

				//If a working order is given, Update modifyTime.
				//Guid matchId = record.MatchId.GetValueOrDefault(Guid.Empty);
				//if (matchId != Guid.Empty)
				//{
				//    MatchPersistence matchPersistence = new MatchPersistence();
				//    matchPersistence.UpdateModifyTime(workingOrderId);
				//}


		}

		/// <summary>
		/// Delete a debt holder
		/// </summary>
		/// <returns>True for sucess</returns>
		public override ErrorCode Delete(Consumer record)
		{
			DataModel dataModel = new DataModel();

			if (record.RowId == null || DataModel.Consumer.ConsumerKey.Find(record.RowId) == null)
			{
				return ErrorCode.RecordNotFound;
			}

			dataModel.DestroyConsumer(
					new object[] { record.RowId },
					record.RowVersion);

			return ErrorCode.Success;
		}


		public override Consumer Get(Guid id)
		{
			throw new NotImplementedException();
		}
	}
}
