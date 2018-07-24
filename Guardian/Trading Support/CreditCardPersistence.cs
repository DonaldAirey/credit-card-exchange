namespace FluidTrade.Guardian
{
	using System;
	using System.ServiceModel;
	using System.ServiceModel.Security;
	using FluidTrade.Core;
	using FluidTrade.Guardian.Records;


	/// <summary>
	/// Credit Card persistance
	/// </summary>
	internal class CreditCardPersistence : DataModelPersistence<CreditCard>
	{

		/// <summary>
		/// Constructor
		/// </summary>
		public CreditCardPersistence()
		{
		}

		/// <summary>
		/// Create a new Credit card
		/// </summary>
		/// <returns></returns>
		public override Guid Create(CreditCard record)
		{
			DataModel dataModel = new DataModel();
			DataModelTransaction transaction = DataModelTransaction.Current;
			Guid blotterId = this.FindContainingBlotter(transaction, record.ConsumerId.GetValueOrDefault());
			Guid tenantId = PersistenceHelper.GetTenantForEntity(
				transaction,
				blotterId);

			if (!DataModelFilters.HasAccess(transaction, TradingSupport.UserId, blotterId, AccessRight.Write))
				throw new SecurityAccessDeniedException("The current user does not write permission to the selected blotter");

			Guid ccId = Guid.NewGuid();

			////Create a entry in credit card			
			dataModel.CreateCreditCard(
				record.AccountBalance,
				record.AccountNumber,
				record.ConsumerId.GetValueOrDefault(),
				ccId,
				record.DebtHolder,
				record.DebtRuleId,
				Guid.NewGuid().ToString(),
				record.AccountNumber,
				record.OriginalAccountNumber,
				tenantId);

			return ccId;

		}

		/// <summary>
		/// Update Credit Card
		/// </summary>
		/// <returns></returns>
		public override void Update(CreditCard record)
		{
			DataModel dataModel = new DataModel();
			DataModelTransaction transaction = DataModelTransaction.Current;
			Guid blotterId = this.FindContainingBlotter(transaction, record.RowId);

			if (record.RowId == null || DataModel.CreditCard.CreditCardKey.Find(record.RowId) == null)
			{
				throw new FaultException<RecordNotFoundFault>(new RecordNotFoundFault("Credit Card", new object[] { record.RowId }));				
			}
			if (!DataModelFilters.HasAccess(transaction, TradingSupport.UserId, blotterId, AccessRight.Write))
			{
				throw new SecurityAccessDeniedException("The current user does not write permission to the selected blotter");
			}

			dataModel.UpdateCreditCard(
				record.AccountBalance,
				record.AccountNumber,
				record.ConsumerId,
				null,
				new object[] { record.RowId },
				record.DebtHolder,
				record.DebtRuleId,
				null,
				record.AccountNumber,
				record.OriginalAccountNumber,
				record.RowVersion,
				null);

		}

		/// <summary>
		/// Delete a credit card
		/// </summary>
		/// <returns>True for sucess</returns>
		public override ErrorCode Delete(CreditCard record)
		{
			DataModel dataModel = new DataModel();
			DataModelTransaction transaction = DataModelTransaction.Current;
			CreditCardRow creditCardRow = DataModel.CreditCard.CreditCardKey.Find(record.RowId);
			WorkingOrderRow workingOrderRow = null;
			MatchRow[] matchRows;
			Guid blotterId;

			if (record.RowId == null || creditCardRow == null)
			{
				return ErrorCode.RecordNotFound;
			}

			workingOrderRow = this.FindWorkingOrder(transaction, record.RowId);
			workingOrderRow.AcquireReaderLock(transaction);
			matchRows = workingOrderRow.GetMatchRows();
			
			blotterId = workingOrderRow.BlotterId;
			workingOrderRow.ReleaseLock(transaction.TransactionId);

			if (!DataModelFilters.HasAccess(transaction, TradingSupport.UserId, blotterId, AccessRight.Write))
			{

				return ErrorCode.AccessDenied;

			}

			MatchPersistence matchPersistence = new MatchPersistence();

			foreach (MatchRow matchRow in matchRows)
			{
				matchRow.AcquireReaderLock(transaction);
				Records.Match matchRecord = new Records.Match() { RowId = matchRow.MatchId, RowVersion = matchRow.RowVersion };
				ConsumerTrustNegotiationRow[] consumerTrustNegotiationRows = matchRow.GetConsumerTrustNegotiationRows();
				ConsumerDebtNegotiationRow[] consumerDebtNegotiationRows = matchRow.GetConsumerDebtNegotiationRows();
				matchRow.ReleaseLock(transaction.TransactionId);

				foreach (ConsumerTrustNegotiationRow negotiationRow in consumerTrustNegotiationRows)
				{

					negotiationRow.AcquireReaderLock(transaction);
					Guid negRowCreditCardId = negotiationRow.CreditCardId;
					negotiationRow.ReleaseLock(transaction.TransactionId);
					if (negRowCreditCardId == record.RowId)
					{
						if(MatchDataTable.IsSettled(transaction, matchRow, true) == true)
						{
							return ErrorCode.RecordExists;
						}
						matchPersistence.Delete(matchRecord);
						break;
					}

				}

			}

			dataModel.DestroyCreditCard(
					new object[] { record.RowId },
					record.RowVersion);

			return ErrorCode.Success;
		}

		/// <summary>
		/// Get a credit card
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public override CreditCard Get(Guid id)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Find the blotter than contains this credit card.
		/// </summary>
		/// <param name="transaction">The transaction object.</param>
		/// <param name="creditCardId">The credit card id.</param>
		/// <returns>The blotter id of the blotter that contains this credit card.</returns>
		private Guid FindContainingBlotter(DataModelTransaction transaction, Guid creditCardId)
		{

			WorkingOrderRow workingOrderRow = this.FindWorkingOrder(transaction, creditCardId);

			workingOrderRow.AcquireReaderLock(transaction);
			Guid blotterId = workingOrderRow.BlotterId;
			workingOrderRow.ReleaseLock(transaction.TransactionId);

			return blotterId;

		}

		/// <summary>
		/// Find the working order than contains this credit card.
		/// </summary>
		/// <param name="transaction">The transaction object.</param>
		/// <param name="creditCardId">The credit card id.</param>
		/// <returns>The working order that contains this credit card.</returns>
		private WorkingOrderRow FindWorkingOrder(DataModelTransaction transaction, Guid creditCardId)
		{

			CreditCardRow creditCardRow = DataModel.CreditCard.CreditCardKey.Find(creditCardId);
			creditCardRow.AcquireReaderLock(transaction);

			ConsumerRow consumerRow = creditCardRow.ConsumerRow;
			creditCardRow.ReleaseLock(transaction.TransactionId);
			consumerRow.AcquireReaderLock(transaction);

			ConsumerDebtRow[] consumerDebtRows = consumerRow.GetConsumerDebtRows();
			ConsumerTrustRow[] consumerTrustRows = consumerRow.GetConsumerTrustRows();
			consumerRow.ReleaseLock(transaction.TransactionId);

			// There really should only be one ConsumerDebtRow (if there is one at all).
			foreach (ConsumerDebtRow consumerDebtRow in consumerDebtRows)
			{

				consumerDebtRow.AcquireReaderLock(transaction);
				SecurityRow securityRow = consumerDebtRow.SecurityRow;
				consumerDebtRow.ReleaseLock(transaction.TransactionId);
				securityRow.AcquireReaderLock(transaction);
				WorkingOrderRow[] workingOrderRows = securityRow.GetWorkingOrderRowsByFK_Security_WorkingOrder_SecurityId();
				securityRow.ReleaseLock(transaction.TransactionId);

				// There really should only be one WorkingOrderRow, so return the first one we find.
				foreach (WorkingOrderRow workingOrderRow in workingOrderRows)
					return workingOrderRow;

			}
			
			// There really should only be one ConsumerTrustRow (if there is one at all).
			foreach (ConsumerTrustRow consumerTrustRow in consumerTrustRows)
			{

				consumerTrustRow.AcquireReaderLock(transaction);
				SecurityRow securityRow = consumerTrustRow.SecurityRow;
				consumerTrustRow.ReleaseLock(transaction.TransactionId);
				securityRow.AcquireReaderLock(transaction);
				WorkingOrderRow[] workingOrderRows = securityRow.GetWorkingOrderRowsByFK_Security_WorkingOrder_SecurityId();
				securityRow.ReleaseLock(transaction.TransactionId);

				// There really should only be one WorkingOrderRow, so return the first one we find.
				foreach (WorkingOrderRow workingOrderRow in workingOrderRows)
					return workingOrderRow;

			}

			return null;

		}

	}

}
