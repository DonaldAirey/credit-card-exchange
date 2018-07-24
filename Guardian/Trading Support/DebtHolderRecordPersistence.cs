namespace FluidTrade.Guardian
{

	using System;
	using System.Security;
	using FluidTrade.Core;
	using FluidTrade.Guardian.Records;

	/// <summary>
	/// 
	/// </summary>
	internal class DebtHolderRecordPersistence
	{

		/// <summary>
		/// The record to insert.
		/// </summary>
		private DebtHolderRecord Record { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		internal DebtHolderRecordPersistence(DebtHolderRecord record)
		{

			this.Record = record;

		}

		/// <summary>
		/// Create a new Debt Holder Record
		/// </summary>	
		/// <returns></returns>
		internal Guid Create()
		{

			DataModel dataModel = new DataModel();
			DataModelTransaction dataModelTransaction = DataModelTransaction.Current;
			Guid userId = TradingSupport.UserId;
			Guid tenantId = PersistenceHelper.GetTenantForEntity(dataModelTransaction, this.Record.Blotter);
			Guid entityId = Guid.Empty;
			Guid consumerId;
			Guid creditCardId;
			Guid workingOrderId;
			CountryRow country;
			Guid countryId;
			Guid? provinceId = null;
			TypeRow type;
			Guid typeId;
			ImageRow image;
			Guid imageId;

			// These variables are used for auditing the changes to this record.
			DateTime createdTime = DateTime.UtcNow;
			Guid createdUserId = TradingSupport.UserId;
			DateTime modifiedTime = createdTime;
			Guid modifiedUserId = createdUserId;
			EntityRow dollars;
			Guid dollarsId;

			// We need write access to the containing blotter in order to add a record to it.
			if (!DataModelFilters.HasAccess(dataModelTransaction, userId, this.Record.Blotter, AccessRight.Write))
				throw new SecurityException("Current user does not have write access to the selected blotter");

			country = TradingSupport.FindCountryByKey(
				this.Record.ConfigurationId,
				"FK_Country_Security",
				new object[] { this.Record.CountryCode });
			countryId = country.CountryId;
			country.ReleaseReaderLock(dataModelTransaction.TransactionId);

			if (this.Record.ProvinceCode != null)
			{

				ProvinceRow province = TradingSupport.FindProvinceByKey(
					this.Record.ConfigurationId,
					"FK_Province_Consumer",
					new object[] { this.Record.ProvinceCode });
				provinceId = province.ProvinceId;
				province.ReleaseReaderLock(dataModelTransaction.TransactionId);

			}

			dollars = TradingSupport.FindEntityByKey(
				this.Record.ConfigurationId,
				"FK_Security_WorkingOrder_SettlementId",
				new object[] { this.Record.Currency });
			dollarsId = dollars.EntityId;
			dollars.ReleaseReaderLock(dataModelTransaction.TransactionId);

			image = TradingSupport.FindImageByKey(
				this.Record.ConfigurationId,
				"FK_Image_Entity",
				new object[] { "OBJECT" });
			imageId = image.ImageId;
			image.ReleaseReaderLock(dataModelTransaction.TransactionId);

			type = TradingSupport.FindTypeByKey(
				this.Record.ConfigurationId,
				"FK_Type_Entity",
				new object[] { "CONSUMER DEBT" });
			typeId = type.TypeId;
			type.ReleaseReaderLock(dataModelTransaction.TransactionId);

			entityId = Guid.NewGuid();
			consumerId = Guid.NewGuid();
			creditCardId = Guid.NewGuid();
			workingOrderId = Guid.NewGuid();

			dataModel.CreateEntity(
				createdTime,
				null,
				entityId,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				this.Record.AccountCode,
				imageId,
				false,
				false,
				modifiedTime,
				this.Record.OriginalAccountNumber,
				tenantId,
				typeId);

			dataModel.CreateSecurity(
				null,
				countryId,
				null,
				null,
				null,
				1,
				1,
				entityId,
				this.Record.AccountCode,
				tenantId,
				VolumeCategoryMap.FromCode(VolumeCategory.Unknown));

			dataModel.CreateConsumer(
				this.Record.Address1,
				this.Record.Address2,
				null,
				null,
				this.Record.City,
				consumerId,
				this.Record.DateOfBirth != null ? (object)this.Record.DateOfBirth.Value : null,
				null,
				null,
				this.Record.FirstName,
				null,
				this.Record.LastName,
				this.Record.MiddleName,
				StringUtilities.CleanUpAlphaNumericString(this.Record.PhoneNumber),
				this.Record.PostalCode,
				provinceId,
				null,
				StringUtilities.CleanUpAlphaNumericString(this.Record.SocialSecurityNumber),
				this.Record.Suffix);
			dataModel.CreateCreditCard(
				this.Record.AccountBalance,
				this.Record.AccountCode,
				consumerId,
				creditCardId,
				this.Record.DebtHolder,
				null,
				null,
				this.Record.AccountCode,
				StringUtilities.CleanUpAlphaNumericString(this.Record.OriginalAccountNumber),
				tenantId);
			dataModel.CreateConsumerDebt(
				this.Record.CollectionDate,
				entityId,
				consumerId,
				creditCardId,
				this.Record.DateOfDelinquency != null ? (object)this.Record.DateOfDelinquency.Value : null,
				null,
				null,
				this.Record.Representative,
				this.Record.Tag,
				tenantId,
				this.Record.VendorCode);

			dataModel.CreateWorkingOrder(
				null,
				this.Record.Blotter,
				createdTime,
				createdUserId,
				CrossingMap.FromCode(Crossing.AlwaysMatch),
				null,
				null,
				null,
				null,
				true,
				true,
				true,
				null,
				modifiedTime,
				modifiedUserId,
				OrderTypeMap.FromCode(OrderType.Market),
				entityId,
				createdTime,
				dollarsId,
				SideMap.FromCode(Side.Sell),
				createdTime,
				StatusMap.FromCode(Status.New),
				null,
				null,
				null,
				null,
				TimeInForceMap.FromCode(TimeInForce.GoodTillCancel),
				createdTime,
				createdTime,
				workingOrderId);

			// Create the access control record for this new entity.
			dataModel.CreateAccessControl(
				Guid.NewGuid(), 
				AccessRightMap.FromCode(AccessRight.FullControl), 
				entityId, 
				userId,
				tenantId);

			return entityId;

		}

		/// <summary>
		/// If a matching consumer debt record already exists, update the account with this, rather than creating a new one.
		/// </summary>
		/// <param name="entity">The entity row of the consumer debt record.</param>
		/// <returns>The entityId.</returns>
		internal Guid Update(EntityRow entity)
		{

			DataModel dataModel = new DataModel();
			DataModelTransaction dataModelTransaction = DataModelTransaction.Current;
			CountryRow country;
			Guid countryId;
			Guid? provinceId = null;
			EntityRow dollars;
			Guid dollarsId;
			ConsumerRow consumer;
			ConsumerDebtRow consumerDebt;
			CreditCardRow creditCard;
			SecurityRow security;
			WorkingOrderRow workingOrder;
			Guid consumerId;
			Guid consumerDebtId = entity.EntityId;
			Guid creditCardId;
			Guid entityId = entity.EntityId;
			Guid securityId = entity.EntityId;
			Guid workingOrderId;
			Int64 consumerVersion;
			Int64 consumerDebtVersion;
			Int64 creditCardVersion;
			Int64 entityVersion = entity.RowVersion;
			Int64 securityVersion;			
			Int64 workingOrderVersion;
			Boolean updateConsumer = false;
			Boolean updateConsumerDebt = false;
			Boolean updateCreditCard = false;
			Boolean updateEntity = false;
			Boolean updateSecurity = false;
			DateTime currentUTCTime = DateTime.UtcNow;

			entity.ReleaseReaderLock(dataModelTransaction.TransactionId);

			// We need write access to the containing blotter in order to add a record to it.
			if (!DataModelFilters.HasAccess(dataModelTransaction, TradingSupport.UserId, this.Record.Blotter, AccessRight.Write))
				throw new SecurityException("Current user does not have write access to the selected blotter");
#if false
			// We need write access to the consumer debt's entity in order to update it.
			if (!TradingSupport.HasAccess(dataModelTransaction, entityId, AccessRight.Write))
				throw new SecurityException("Current user does not have write access to the selected consumer debt");
#endif
			// Via the country row...
			country = TradingSupport.FindCountryByKey(
				this.Record.ConfigurationId,
				"FK_Country_Security",
				new object[] { this.Record.CountryCode });
			countryId = country.CountryId;
			country.ReleaseReaderLock(dataModelTransaction.TransactionId);

			// ... get the province Id.
			if (this.Record.ProvinceCode != null)
			{

				ProvinceRow province = TradingSupport.FindProvinceByKey(
					this.Record.ConfigurationId,
					"FK_Province_Consumer",
					new object[] { this.Record.ProvinceCode });
				provinceId = province.ProvinceId;
				province.ReleaseReaderLock(dataModelTransaction.TransactionId);

			}

			// Get the USD security Id.
			dollars = TradingSupport.FindEntityByKey(
				this.Record.ConfigurationId,
				"FK_Security_WorkingOrder_SettlementId",
				new object[] { this.Record.Currency });
			dollarsId = dollars.EntityId;
			dollars.ReleaseReaderLock(dataModelTransaction.TransactionId);

			// See if the entity needs to be updated.
			entity.AcquireReaderLock(dataModelTransaction);
			entityVersion = entity.RowVersion;
			if (TradingSupport.IsColumnOld(entity, "Name", this.Record.OriginalAccountNumber))
				updateEntity = true;
			entity.ReleaseLock(dataModelTransaction.TransactionId);


			// Get security's children see if we need to update securityRow.
			security = DataModel.Security.SecurityKey.Find(entityId);
			security.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
			securityVersion = security.RowVersion;
			consumerDebt = DataModel.ConsumerDebt.ConsumerDebtKey.Find(security.SecurityId);
			workingOrder = security.GetWorkingOrderRowsByFK_Security_WorkingOrder_SecurityId()[0];
			if (TradingSupport.IsColumnOld(security, "CountryId", countryId))
				updateSecurity = true;
			security.ReleaseLock(dataModelTransaction.TransactionId);

			// Control the working order:
			workingOrder.AcquireWriterLock(dataModelTransaction);

			// Get the consumer debt's children and see if we need to update the consumer debt.
			consumerDebt.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
			consumerDebtVersion = consumerDebt.RowVersion;
			creditCard = DataModel.CreditCard.CreditCardKey.Find(consumerDebt.CreditCardId);
			if (TradingSupport.IsColumnOld(consumerDebt, "DateOfDelinquency", this.Record.DateOfDelinquency) ||
				TradingSupport.IsColumnOld(consumerDebt, "Representative", this.Record.Representative) ||
				TradingSupport.IsColumnOld(consumerDebt, "Tag", this.Record.Tag))
				updateConsumerDebt = true;
			consumerDebt.ReleaseLock(dataModelTransaction.TransactionId);

			creditCard.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
			creditCardId = creditCard.CreditCardId;
			creditCardVersion = creditCard.RowVersion;
			consumer = DataModel.Consumer.ConsumerKey.Find(creditCard.ConsumerId);
			if (TradingSupport.IsColumnOld(creditCard, "AccountBalance", this.Record.AccountBalance) ||
				TradingSupport.IsColumnOld(creditCard, "AccountNumber", this.Record.AccountCode) ||
				TradingSupport.IsColumnOld(creditCard, "DebtHolder", this.Record.DebtHolder) ||
				TradingSupport.IsColumnOld(creditCard, "OriginalAccountNumber", this.Record.OriginalAccountNumber))
				updateCreditCard = true;
			creditCard.ReleaseLock(dataModelTransaction.TransactionId);

			consumer.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
			consumerId = consumer.ConsumerId;
			consumerVersion = consumer.RowVersion;
			if (TradingSupport.IsColumnOld(consumer, "Address1", this.Record.Address1) ||
				TradingSupport.IsColumnOld(consumer, "Address2", this.Record.Address2) ||
				TradingSupport.IsColumnOld(consumer, "City", this.Record.City) ||
				TradingSupport.IsColumnOld(consumer, "DateOfBirth", this.Record.DateOfBirth) ||
				TradingSupport.IsColumnOld(consumer, "FirstName", this.Record.FirstName) ||
				TradingSupport.IsColumnOld(consumer, "LastName", this.Record.LastName) ||
				TradingSupport.IsColumnOld(consumer, "PostalCode", this.Record.PostalCode) ||
				TradingSupport.IsColumnOld(consumer, "MiddleName", this.Record.MiddleName) ||
				TradingSupport.IsColumnOld(consumer, "PhoneNumber", this.Record.PhoneNumber) ||
				TradingSupport.IsColumnOld(consumer, "ProvinceId", provinceId) ||
				TradingSupport.IsColumnOld(consumer, "SocialSecurityNumber", this.Record.SocialSecurityNumber) ||
				TradingSupport.IsColumnOld(consumer, "Suffix", this.Record.Suffix))
				updateConsumer = true;
			consumer.ReleaseLock(dataModelTransaction.TransactionId);

			workingOrder.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
			workingOrderId = workingOrder.WorkingOrderId;
			workingOrderVersion = workingOrder.RowVersion;

			//workingOrder.ReleaseLock(dataModelTransaction.TransactionId);

			// We need write access to the containing blotter in order to add a record to it.
			if (!TradingSupport.HasAccess(dataModelTransaction, PersistenceHelper.GetBlotterForConsumer(dataModelTransaction, consumerId), AccessRight.Write))
				throw new SecurityException("Current user does not have write access to the original blotter");

			if (updateConsumer)
				dataModel.UpdateConsumer(
					this.Record.Address1 != null ? (object)this.Record.Address1 : DBNull.Value,
					this.Record.Address2 != null ? (object)this.Record.Address2 : DBNull.Value,
					null,
					null,
					this.Record.City != null ? (object)this.Record.City : DBNull.Value,
					consumerId,
					new object[] { consumerId },
					this.Record.DateOfBirth != null ? (object)this.Record.DateOfBirth.Value : DBNull.Value,
					null,
					null,
					this.Record.FirstName != null ? (object)this.Record.FirstName : DBNull.Value,
					null,
					this.Record.LastName != null ? (object)this.Record.LastName : DBNull.Value,
					this.Record.MiddleName != null ? (object)this.Record.MiddleName : DBNull.Value,
					this.Record.PhoneNumber != null ? (object)StringUtilities.CleanUpAlphaNumericString(this.Record.PhoneNumber) : DBNull.Value,
					this.Record.PostalCode != null ? (object)this.Record.PostalCode : DBNull.Value,
					provinceId,
					consumerVersion,
					null,
					StringUtilities.CleanUpAlphaNumericString(this.Record.SocialSecurityNumber),
					this.Record.Suffix != null ? (object)this.Record.Suffix : DBNull.Value);

			if (updateConsumerDebt)
				dataModel.UpdateConsumerDebt(
					null,
					consumerDebtId,
					new object[] { consumerDebtId },
					null,
					null,
					this.Record.DateOfDelinquency != null ? (object)this.Record.DateOfDelinquency.Value : DBNull.Value,
					null,
					null,
					this.Record.Representative != null ? (object)this.Record.Representative : DBNull.Value,
					consumerDebtVersion,
					this.Record.Tag != null ? (object)this.Record.Tag : DBNull.Value,
					null,
					null);

			if (updateCreditCard)
				dataModel.UpdateCreditCard(
					this.Record.AccountBalance,
					this.Record.AccountCode,
					null,
					creditCardId,
					new object[] { creditCardId },
					this.Record.DebtHolder,
					null,
					null,
					null,
					StringUtilities.CleanUpAlphaNumericString(this.Record.OriginalAccountNumber),
					creditCardVersion,
					null);

			if (updateEntity)
				dataModel.UpdateEntity(
					null,
					null,
					entityId,
					new object[] { entityId },
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
					currentUTCTime,
					this.Record.OriginalAccountNumber,
					entityVersion,
					null,
					null);

			if (updateSecurity)
				dataModel.UpdateSecurity(
					null,
					countryId,
					null,
					null,
					null,
					1,
					1,
					securityVersion,
					securityId,
					new object[] { securityId },
					null,
					null,
					null);

		
			dataModel.UpdateWorkingOrder(
				null,
				this.Record.Blotter,
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
				currentUTCTime,
				TradingSupport.UserId,
				null,
				workingOrderVersion,
				null,
				null,
				dollarsId,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				currentUTCTime,
				workingOrderId,
				new object[] { workingOrderId });

			return entityId;

		}

	}

}
