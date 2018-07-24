namespace FluidTrade.Guardian
{
	using System;
	using System.Security;
	using System.ServiceModel;
	using FluidTrade.Core;
	using FluidTrade.Guardian.Records;


	/// <summary>
	/// 
	/// </summary>
	internal class DebtNegotiatorRecordPersistence
	{

		/// <summary>
		/// The record to import.
		/// </summary>
		private DebtNegotiatorRecord Record { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		internal DebtNegotiatorRecordPersistence(DebtNegotiatorRecord record)
		{

			this.Record = record;
		}

		/// <summary>
		/// Create a new Debt Holder Record
		/// </summary>
		/// <returns></returns>
		internal Guid Create(EntityRow existingTrust, CreditCardRow existingCard)
		{

			DataModelTransaction dataModelTransaction = DataModelTransaction.Current;
			DataModel dataModel = new DataModel();
			Guid tenantId = PersistenceHelper.GetTenantForEntity(dataModelTransaction, this.Record.Blotter);
			Guid consumerId;
			Guid entityId = Guid.Empty;
			Guid existingCardId = Guid.Empty;
			Int64 existingCardVersion = 0;
			Boolean updateExistingCard = false;
			
			if (existingTrust != null)
			{
				
				entityId = existingTrust.EntityId;
				existingTrust.ReleaseReaderLock(dataModelTransaction.TransactionId);

			}

			if (existingCard != null)
			{

				existingCard.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
				existingCardId = existingCard.CreditCardId;
				existingCardVersion = existingCard.RowVersion;
				if (TradingSupport.IsColumnOld(existingCard, "AccountBalance", this.Record.AccountBalance) ||
					TradingSupport.IsColumnOld(existingCard, "AccountNumber", this.Record.AccountCode) ||
					TradingSupport.IsColumnOld(existingCard, "DebtHolder", this.Record.DebtHolder) ||
					TradingSupport.IsColumnOld(existingCard, "OriginalAccountNumber", this.Record.OriginalAccountNumber))
					updateExistingCard = true;
			}

			// We need write access to the containing blotter in order to add a record to it.
			if (!TradingSupport.HasAccess(dataModelTransaction, this.Record.Blotter, AccessRight.Write))
				throw new SecurityException("Current user does not have write access to the selected blotter");
#if false
			if (existingTrust != null &&
				!TradingSupport.HasAccess(dataModelTransaction, entityId, AccessRight.Write))
				throw new SecurityException("Current user does not have write access to the selected consumer");
#endif
			if (existingTrust == null)
			{

				consumerId = this.CreateConsumer();
					
			}
			else
			{
				
				consumerId = this.UpdateConsumer(existingTrust);

			}

			if (existingCard == null)
				dataModel.CreateCreditCard(
					this.Record.AccountBalance,
					this.Record.AccountCode,
					consumerId,
					Guid.NewGuid(),
					this.Record.DebtHolder,
					null,
					null,
					this.Record.AccountCode,
					StringUtilities.CleanUpAlphaNumericString(this.Record.OriginalAccountNumber),
					tenantId);
			else if (updateExistingCard)
				dataModel.UpdateCreditCard(
					this.Record.AccountBalance,
					this.Record.AccountCode,
					null,
					existingCardId,
					new object[] { existingCardId },
					this.Record.DebtHolder,
					null,
					null,
					this.Record.AccountCode,
					StringUtilities.CleanUpAlphaNumericString(this.Record.OriginalAccountNumber),
					existingCardVersion,
					null);

			return consumerId;

		}

		/// <summary>
		/// Create a new consumer.
		/// </summary>
		/// <returns>The ConsumerId of the consumer.</returns>
		private Guid CreateConsumer()
		{

			DataModel dataModel = new DataModel();
			DataModelTransaction dataModelTransaction = DataModelTransaction.Current;
			Guid userId = TradingSupport.UserId;
			Guid tenantId = PersistenceHelper.GetTenantForEntity(DataModelTransaction.Current, this.Record.Blotter);
			Guid entityId = Guid.NewGuid();
			Guid consumerId = Guid.NewGuid();
			Guid creditCardId = Guid.NewGuid();
			Guid workingOrderId = Guid.NewGuid();
			CountryRow country;
			Guid countryId;
			Guid? provinceId = null;
			EntityRow dollars;
			Guid dollarsId;
			TypeRow type;
			Guid typeId;
			ImageRow image;
			Guid imageId;
			DateTime currentUTCTime = DateTime.UtcNow;

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
				new object[] { "CONSUMER TRUST" });
			typeId = type.TypeId;
			type.ReleaseReaderLock(dataModelTransaction.TransactionId);

			dataModel.CreateEntity(
				currentUTCTime,
				null,
				entityId,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				this.Record.CustomerCode,
				imageId,
				false,
				false,
				currentUTCTime,
				this.Record.SavingsEntityCode,
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
				this.Record.BankAccountNumber,
				this.Record.BankRoutingNumber,
				this.Record.City,
				consumerId,
				this.Record.DateOfBirth != null ? (object)this.Record.DateOfBirth.Value : null,
				null,
				null,
				this.Record.FirstName,
				this.Record.IsEmployed != null ? (object)this.Record.IsEmployed.Value : null,
				this.Record.LastName,
				this.Record.MiddleName,
				StringUtilities.CleanUpAlphaNumericString(this.Record.PhoneNumber),
				this.Record.PostalCode,
				provinceId,
				this.Record.Salutation,
				StringUtilities.CleanUpAlphaNumericString(this.Record.SocialSecurityNumber),
				this.Record.Suffix);
			dataModel.CreateConsumerTrust(
				consumerId,
				entityId,
				null,
				null,
				this.Record.SavingsAccount,
				this.Record.SavingsBalance,
				this.Record.Tag,
				tenantId,
				this.Record.VendorCode);

			//If this not found, there will be an exception. Let the exception propagate up.
			dataModel.CreateWorkingOrder(
				null,
				this.Record.Blotter,
				currentUTCTime,
				userId,
				CrossingMap.FromCode(Crossing.AlwaysMatch),
				null,
				null,
				null,
				null,
				true,
				true,
				true,
				null,
				DateTime.UtcNow,
				TradingSupport.UserId,
				OrderTypeMap.FromCode(OrderType.Market),
				entityId,
				DateTime.UtcNow,
				dollarsId,
				SideMap.FromCode(Side.Sell),
				DateTime.UtcNow,
				StatusMap.FromCode(Status.New),
				null,
				null,
				null,
				null,
				TimeInForceMap.FromCode(TimeInForce.GoodTillCancel),
				DateTime.UtcNow,
				DateTime.UtcNow,
				workingOrderId);
			
			// Create the access control record for this new entity.
			dataModel.CreateAccessControl(
				Guid.NewGuid(), 
				AccessRightMap.FromCode(AccessRight.FullControl), 
				entityId, 
				userId,
				tenantId);

			return consumerId;

		}

		/// <summary>
		/// Update a consumer.
		/// </summary>
		/// <param name="entity">The ConsumerTrust's Entity row.</param>
		/// <returns>The ConsumerId of the Consumer row.</returns>
		private Guid UpdateConsumer(EntityRow entity)
		{

            DataModel dataModel = new DataModel();
            DataModelTransaction dataModelTransaction = DataModelTransaction.Current;
			DateTime modified = DateTime.UtcNow;
			CountryRow country;
            Guid countryId;
            Guid? provinceId = null;
			EntityRow dollars;
            Guid dollarsId;
			ConsumerRow consumer = null;
			ConsumerTrustRow consumerTrust = null;
			SecurityRow security = null;
			WorkingOrderRow workingOrder = null;
			MatchRow[] matches;
			Guid consumerId;
			Guid consumerTrustId;
            Guid entityId;
			Guid securityId;			
			Guid workingOrderId;
			Int64 consumerVersion;
			Int64 consumerTrustVersion;
			Int64 entityVersion;
			Int64 securityVersion;			
			Int64 workingOrderVersion;
			Boolean updateConsumer = false;
			Boolean updateConsumerTrust = false;
			Boolean updateEntity = false;
			Boolean updateSecurity = false;

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

			try
			{

				entity.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
				entityId = entity.EntityId;
				entityVersion = entity.RowVersion;
				if (TradingSupport.IsColumnOld(entity, "Name", this.Record.OriginalAccountNumber))
					updateEntity = true;

			}
			finally
			{

				entity.ReleaseLock(dataModelTransaction.TransactionId);

			}

			try
			{

				security = DataModel.Security.SecurityKey.Find(entityId);
				security.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
				securityId = entityId;
				securityVersion = security.RowVersion;
				workingOrder = security.GetWorkingOrderRowsByFK_Security_WorkingOrder_SecurityId()[0];
				if (TradingSupport.IsColumnOld(security, "CountryId", countryId))
					updateSecurity = true;

			}
			finally
			{

				security.ReleaseLock(dataModelTransaction.TransactionId);

			}


			// Control the working order:
			workingOrder.AcquireWriterLock(dataModelTransaction);

			try
			{

				consumerTrust = DataModel.ConsumerTrust.ConsumerTrustKey.Find(entityId);
				consumerTrust.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
				consumerTrustId = consumerTrust.ConsumerTrustId;
				consumerTrustVersion = consumerTrust.RowVersion;
				consumer = DataModel.Consumer.ConsumerKey.Find(consumerTrust.ConsumerId);
				if (TradingSupport.IsColumnOld(consumerTrust, "SavingsAccount", this.Record.SavingsAccount) ||
					TradingSupport.IsColumnOld(consumerTrust, "SavingsBalance", this.Record.SavingsBalance) ||
					TradingSupport.IsColumnOld(consumerTrust, "Tag", this.Record.Tag))
					updateConsumerTrust = true;

			}
			finally
			{

				consumerTrust.ReleaseLock(dataModelTransaction.TransactionId);

			}

			try
			{

				consumer.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
				consumerId = consumer.ConsumerId;
				consumerVersion = consumer.RowVersion;
				if (TradingSupport.IsColumnOld(consumer, "Address1", this.Record.Address1) ||
					TradingSupport.IsColumnOld(consumer, "Address2", this.Record.Address2) ||
					TradingSupport.IsColumnOld(consumer, "BankAccountNumber", this.Record.BankAccountNumber) ||
					TradingSupport.IsColumnOld(consumer, "BankRoutingNumber", this.Record.BankRoutingNumber) ||
					TradingSupport.IsColumnOld(consumer, "City", this.Record.City) ||
					TradingSupport.IsColumnOld(consumer, "DateOfBirth", this.Record.DateOfBirth) ||
					TradingSupport.IsColumnOld(consumer, "FirstName", this.Record.FirstName) ||
					TradingSupport.IsColumnOld(consumer, "IsEmployed", this.Record.IsEmployed) ||
					TradingSupport.IsColumnOld(consumer, "LastName", this.Record.LastName) ||
					TradingSupport.IsColumnOld(consumer, "PostalCode", this.Record.PostalCode) ||
					TradingSupport.IsColumnOld(consumer, "MiddleName", this.Record.MiddleName) ||
					TradingSupport.IsColumnOld(consumer, "PhoneNumber", this.Record.PhoneNumber) ||
					TradingSupport.IsColumnOld(consumer, "ProvinceId", provinceId) ||
					TradingSupport.IsColumnOld(consumer, "SocialSecurityNumber", this.Record.SocialSecurityNumber) ||
					TradingSupport.IsColumnOld(consumer, "Suffix", this.Record.Suffix))
					updateConsumer = true;

			}
			finally
			{

				consumer.ReleaseLock(dataModelTransaction.TransactionId);

			}

			try
			{

				//workingOrder.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
				workingOrderId = workingOrder.WorkingOrderId;
				workingOrderVersion = workingOrder.RowVersion;
				matches = workingOrder.GetMatchRows();

			}
			finally
			{

				//workingOrder.ReleaseLock(dataModelTransaction.TransactionId);

			}

			foreach (MatchRow match in matches)
				if (WorkingOrderPersistence.IsSettled(dataModelTransaction, match))
					throw new FaultException<SecurityFault>(
						new SecurityFault("Cannot update account that is settled") { FaultCode = ErrorCode.RecordExists },
						"Cannot update account that is settled");

			// We need write access to the containing blotter in order to add a record to it.
			if (!TradingSupport.HasAccess(dataModelTransaction, PersistenceHelper.GetBlotterForConsumer(dataModelTransaction, consumerId), AccessRight.Write))
				throw new SecurityException("Current user does not have write access to the original blotter");

			if (updateConsumer)
				dataModel.UpdateConsumer(
					this.Record.Address1 != null ? (object)this.Record.Address1 : DBNull.Value,
					this.Record.Address2 != null ? (object)this.Record.Address2 : DBNull.Value,
					this.Record.BankAccountNumber != null ? (object)this.Record.BankAccountNumber : DBNull.Value,
					this.Record.BankRoutingNumber != null ? (object)this.Record.BankRoutingNumber : DBNull.Value,
					this.Record.City != null ? (object)this.Record.City : DBNull.Value,
					consumerId,
					new object[] { consumerId },
					this.Record.DateOfBirth != null ? (object)this.Record.DateOfBirth.Value : DBNull.Value,
					null,
					null,
					this.Record.FirstName != null ? (object)this.Record.FirstName : DBNull.Value,
					this.Record.IsEmployed != null ? (object)this.Record.IsEmployed.Value : DBNull.Value,
					this.Record.LastName != null ? (object)this.Record.LastName : DBNull.Value,
					this.Record.MiddleName != null ? (object)this.Record.MiddleName : DBNull.Value,
					this.Record.PhoneNumber != null ? (object)StringUtilities.CleanUpAlphaNumericString(this.Record.PhoneNumber) : DBNull.Value,
					this.Record.PostalCode != null ? (object)this.Record.PostalCode : DBNull.Value,
					provinceId,
					consumerVersion,
					this.Record.Salutation,
					StringUtilities.CleanUpAlphaNumericString(this.Record.SocialSecurityNumber),
					this.Record.Suffix != null ? (object)this.Record.Suffix : DBNull.Value);

			if (updateConsumerTrust)
				dataModel.UpdateConsumerTrust(
					null,
					consumerTrustId,
					new object[] { consumerTrustId },
					null,
					null,
					consumerTrustVersion,
					this.Record.SavingsAccount,
					this.Record.SavingsBalance,
					this.Record.Tag != null ? (object)this.Record.Tag : DBNull.Value,
					null,
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
					modified,
					this.Record.SavingsEntityCode,
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
				modified,
				TradingSupport.UserId,
				null,
				workingOrderVersion,
				null,
				null,
				dollarsId,
				null,
				null,
				StatusMap.FromCode(Status.New),
				null,
				null,
				null,
				null,
				null,
				null,
				modified,
				workingOrderId,
				new object[] { workingOrderId });

			return consumerId;

		}
	}

}
