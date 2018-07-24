namespace FluidTrade.Guardian
{

	using System;
	using System.Collections.Generic;
	using System.ServiceModel;
	using FluidTrade.Core;
	using FluidTrade.Guardian.Records;

	/// <summary>
	/// Additional methods to help manage the Consumer Trust records.
	/// </summary>
	/// <copyright>Copyright © 2007 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	internal class ConsumerDebtHelper
	{

		/// <summary>
		/// Information about a payment method for a given settlement.
		/// </summary>
		private class PaymentMethodInfo
		{

			// Public Instance Fields
			public Guid ConsumerDebtSettlementPaymentMethodId;
			public Guid PaymentMethodId;

			/// <summary>
			/// Create a payment method for a settlement.
			/// </summary>
			/// <param name="consumerDebtSettlementPaymentMethodId">The unique identifier for a payment method in this settlement.</param>
			/// <param name="paymentMethodId">The unique identifier of a payment method.</param>
			public PaymentMethodInfo(Guid consumerDebtSettlementPaymentMethodId, Guid paymentMethodId)
			{

				// Initialize the object
				this.ConsumerDebtSettlementPaymentMethodId = consumerDebtSettlementPaymentMethodId;
				this.PaymentMethodId = paymentMethodId;
			}

		}

		/// <summary>
		/// Information about a Consumer Debt Settlement
		/// </summary>
		private class DebtSettlementInfo
		{

			// Public Instance Fields
			public Decimal AccountBalance;
			public Guid AcceptedStatusId;
			public Guid BlotterId;
			public Guid ConsumerDebtNegotiationId;
			public Guid ConsumerDebtSettlementId;
			public Guid ContraMatchId;
			public Int64 ContraMatchRowVersion;
			public DateTime CreatedTime;
			public Guid CreatedUserId;
			public String DebtorAccountNumber;
			public Object DebtorAddress1;
			public Object DebtorAddress2;
			public Object DebtorBankAccountNumber;
			public Object DebtorBankRoutingNumber;
			public Object DebtorFirstName;
			public Object DebtorLastName;
			public Object DebtorMiddleName;
			public String DebtorOriginalAccountNumber;
			public Object DebtorSalutation;
			public Object DebtorSuffix;
			public Object DebtorCity;
			public Object DebtorProvinceId;
			public Object DebtorPostalCode;
			public Guid DebtStatusId;
			public Guid OfferAcceptedStatusId;
			public Guid MatchId;
			public Int64 MatchRowVersion;
			public DateTime ModifiedTime;
			public Guid ModifiedUserId;
			public Guid NewStatusId;
			public Object PayeeAddress1;
			public Object PayeeAddress2;
			public Object PayeeBankAccountNumber;
			public Object PayeeBankRoutingNumber;
			public Object PayeeCity;
			public Object PayeeCompanyName;
			public Object PayeeContactName;
			public Object PayeeDepartment;
			public Object PayeeEmail;
			public Object PayeeFax;
			public Object PayeeForBenefitOf;
			public Object PayeePhone;
			public Object PayeeProvinceId;
			public Object PayeePostalCode;
			public Decimal PaymentLength;
			public List<PaymentMethodInfo> PaymentMethods;
			public DateTime PaymentStartDate;
			public Guid PendingStatusId;
			public Decimal SettlementAmount;
			public Guid TrustStatusId;

			/// <summary>
			/// Create an object to hold information about a settlement.
			/// </summary>
			public DebtSettlementInfo()
			{

				// Initialize the object
				this.PaymentMethods = new List<PaymentMethodInfo>();

			}

		};

		/// <summary>
		/// Creates a settlement for a Consumer Trust representative.
		/// </summary>
		/// <param name="consumerDebtNegotiationId">The identifier of the negotiation that has been agreed to by both parties.</param>
		public static void CreateConsumerDebtSettlement(Guid consumerDebtNegotiationId)
		{

			// A reference to the data model is required to query the database within the scope of a transaction.
			DataModel dataModel = new DataModel();

			// The locking model does not provide for recursive reader locks or promoting reader locks to writer locks.  So the data is collected
			// during a phase then the table can be locked determinstically to prevent deadlocks, then the calls to update the data model are made
			// once all the reader locks have been released.  This structure holds the information required for the creation of a settlement record.
			DebtSettlementInfo debtSettlementInfo = new DebtSettlementInfo();

			// Extract the ambient transaction.
			DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

			// These rows will be locked momentarily while the data is collected and released before the actual transaction.
			ConsumerDebtNegotiationRow consumerDebtNegotiationRow = null;
			MatchRow matchRow = null;
			MatchRow contraMatchRow = null;
			BlotterRow blotterRow = null;
			DebtClassRow debtClassRow = null;
			ConsumerTrustNegotiationRow consumerTrustNegotiationRow = null;
			WorkingOrderRow workingOrderRow = null;
			SecurityRow securityRow = null;
			ConsumerDebtRow consumerDebtRow = null;
			WorkingOrderRow contraWorkingOrderRow = null;
			SecurityRow contraSecurityRow = null;
			ConsumerTrustRow consumerTrustRow = null;
			ConsumerRow consumerRow = null;
			CreditCardRow creditCardRow = null;

			try
			{

				// The starting point for creating a settlement record is to find the negotiation record that has been agreed to by both parties.
				consumerDebtNegotiationRow = DataModel.ConsumerDebtNegotiation.ConsumerDebtNegotiationKey.Find(consumerDebtNegotiationId);
				consumerDebtNegotiationRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

				// This is the record used to match this asset against another.
				matchRow = consumerDebtNegotiationRow.MatchRow;
				matchRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

				// The next step is to find the counter parties matching information which will lead us to the counter parties asset which, in turn, contains
				// more information for the settlement.
				contraMatchRow = DataModel.Match.MatchKey.Find(matchRow.ContraMatchId);
				contraMatchRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

				// The blotter contains a link to the Debt Class which is where the Payee information is found.
				blotterRow = matchRow.BlotterRow;
				blotterRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

				// The debt class provides information about the Payee.
				debtClassRow = blotterRow.GetDebtClassRows()[0];
				debtClassRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

				// The negotiation table has a historical component. Ever time a change is made to the negotiation on either side a completely new record
				// is created to record the change.  While the earlier versions are useful for a historical context and for reports, this console is only
				// interested in the current version of the negotiations.
				Int64 maxVersion = Int64.MinValue;
				foreach (ConsumerTrustNegotiationRow versionRow in contraMatchRow.GetConsumerTrustNegotiationRows())
				{
					try
					{
						versionRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
						if (versionRow.Version > maxVersion)
						{
							maxVersion = versionRow.Version;
							consumerTrustNegotiationRow = versionRow;
						}
					}
					finally
					{
						versionRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
					}
				}
				consumerTrustNegotiationRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

				// The working order record is part of the object oriented path that will lead to the the asset information.  This info is also needed for the
				// settlement record.
				workingOrderRow = matchRow.WorkingOrderRow;
				workingOrderRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

				// The Security record will lead us to the asset.
				securityRow = workingOrderRow.SecurityRowByFK_Security_WorkingOrder_SecurityId;
				securityRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

				// This row contains the actual asset that is to be matched.
				consumerDebtRow = securityRow.GetConsumerDebtRows()[0];
				consumerDebtRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

				// The counter party's asset information is also required.
				contraWorkingOrderRow = contraMatchRow.WorkingOrderRow;
				contraWorkingOrderRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

				// This will lead to the counter party's asset.
				contraSecurityRow = contraWorkingOrderRow.SecurityRowByFK_Security_WorkingOrder_SecurityId;
				contraSecurityRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

				// This is the asset belonging to the counter party that has just agreed to a settlement.
				consumerTrustRow = contraSecurityRow.GetConsumerTrustRows()[0];
				consumerTrustRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

				// This is the Debt Negotiator's version of the Consumer will be used to settle the account.
				consumerRow = consumerTrustRow.ConsumerRow;
				consumerRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

				// We also need to know which credit card was settled.
				creditCardRow = consumerDebtRow.CreditCardRow;
				creditCardRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

				// These values are extracted from the data model while reader locks are in place on the related records.  Since the locks aren't recursive
				// and reader locks can't be promoted, any locks held for this collection process must be released before the middle tier methods are
				// called to create the record.
				debtSettlementInfo.AccountBalance = consumerDebtNegotiationRow.AccountBalance;
				debtSettlementInfo.BlotterId = workingOrderRow.BlotterId;
				debtSettlementInfo.ConsumerDebtNegotiationId = consumerDebtNegotiationId;
				debtSettlementInfo.ConsumerDebtSettlementId = Guid.NewGuid();
				debtSettlementInfo.ContraMatchId = contraMatchRow.MatchId;
				debtSettlementInfo.ContraMatchRowVersion = contraMatchRow.RowVersion;
				debtSettlementInfo.CreatedTime = DateTime.UtcNow;
				debtSettlementInfo.CreatedUserId = TradingSupport.DaemonUserId;
				debtSettlementInfo.DebtorAccountNumber = creditCardRow.AccountNumber;
				debtSettlementInfo.DebtorAddress1 = consumerRow.IsAddress1Null() ? null : consumerRow.Address1;
				debtSettlementInfo.DebtorAddress2 = consumerRow.IsAddress2Null() ? null : consumerRow.Address2;
				debtSettlementInfo.DebtorBankAccountNumber = consumerRow.IsBankAccountNumberNull() ? null : consumerRow.BankAccountNumber;
				debtSettlementInfo.DebtorBankRoutingNumber = consumerRow.IsBankRoutingNumberNull() ? null : consumerRow.BankRoutingNumber;
				debtSettlementInfo.DebtorFirstName = consumerRow.IsFirstNameNull() ? null : consumerRow.FirstName;
				debtSettlementInfo.DebtorLastName = consumerRow.IsLastNameNull() ? null : consumerRow.LastName;
				debtSettlementInfo.DebtorMiddleName = consumerRow.IsMiddleNameNull() ? null : consumerRow.MiddleName;
				debtSettlementInfo.DebtorOriginalAccountNumber = creditCardRow.OriginalAccountNumber;
				debtSettlementInfo.DebtorSalutation = consumerRow.IsSalutationNull() ? null : consumerRow.Salutation;
				debtSettlementInfo.DebtorSuffix = consumerRow.IsSuffixNull() ? null : consumerRow.Suffix;
				debtSettlementInfo.DebtorCity = consumerRow.IsCityNull() ? null : consumerRow.City;
				debtSettlementInfo.DebtorProvinceId = consumerRow.IsProvinceIdNull() ? null : (Object)consumerRow.ProvinceId;
				debtSettlementInfo.DebtorPostalCode = consumerRow.IsPostalCodeNull() ? null : consumerRow.PostalCode;
				debtSettlementInfo.DebtStatusId = matchRow.StatusId;
				debtSettlementInfo.MatchId = matchRow.MatchId;
				debtSettlementInfo.MatchRowVersion = matchRow.RowVersion;
				debtSettlementInfo.ModifiedTime = debtSettlementInfo.CreatedTime;
				debtSettlementInfo.ModifiedUserId = debtSettlementInfo.CreatedUserId;
				debtSettlementInfo.PayeeAddress1 = debtClassRow.IsAddress1Null() ? null : debtClassRow.Address1;
				debtSettlementInfo.PayeeAddress2 = debtClassRow.IsAddress2Null() ? null : debtClassRow.Address2;
				debtSettlementInfo.PayeeBankAccountNumber = debtClassRow.IsBankAccountNumberNull() ? null : debtClassRow.BankAccountNumber;
				debtSettlementInfo.PayeeBankRoutingNumber = debtClassRow.IsBankRoutingNumberNull() ? null : debtClassRow.BankRoutingNumber;
				debtSettlementInfo.PayeeCity = debtClassRow.IsCityNull() ? null : debtClassRow.City;
				debtSettlementInfo.PayeeCompanyName = debtClassRow.IsCompanyNameNull() ? null : debtClassRow.CompanyName;
				debtSettlementInfo.PayeeContactName = debtClassRow.IsContactNameNull() ? null : debtClassRow.ContactName;
				debtSettlementInfo.PayeeDepartment = debtClassRow.IsDepartmentNull() ? null : debtClassRow.Department;
				debtSettlementInfo.PayeeEmail = debtClassRow.IsEmailNull() ? null : debtClassRow.Email;
				debtSettlementInfo.PayeeFax = debtClassRow.IsFaxNull() ? null : debtClassRow.Fax;
				debtSettlementInfo.PayeeForBenefitOf = debtClassRow.IsForBenefitOfNull() ? null : debtClassRow.ForBenefitOf;
				debtSettlementInfo.PayeePhone = debtClassRow.IsPhoneNull() ? null : debtClassRow.Phone;
				debtSettlementInfo.PayeeProvinceId = debtClassRow.IsProvinceIdNull() ? null : (Object)debtClassRow.ProvinceId;
				debtSettlementInfo.PayeePostalCode = debtClassRow.IsPostalCodeNull() ? null : debtClassRow.PostalCode;
				debtSettlementInfo.PaymentLength = consumerDebtNegotiationRow.OfferPaymentLength;
				debtSettlementInfo.TrustStatusId = contraMatchRow.StatusId;

				// The payment methods acceptable for this settlement is the union of the payment methods acceptable to both sides.
				foreach (ConsumerDebtNegotiationOfferPaymentMethodRow consumerDebtNegotiationOfferPaymentMethodRow
					in consumerDebtNegotiationRow.GetConsumerDebtNegotiationOfferPaymentMethodRows())
				{

					try
					{

						// The payment method needs to be locked while we check the counter party to see what types of payment methods are used there.
						consumerDebtNegotiationOfferPaymentMethodRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

						// This loop will compare all of the counter party's acceptable payment methods against the current one offered by the
						// Debt Holder.  If there are any that are compatible, they are moved to a list that is used to generate the settlement.
						foreach (ConsumerTrustNegotiationOfferPaymentMethodRow consumerTrustNegotiationOfferPaymentMethodRow
							in consumerTrustNegotiationRow.GetConsumerTrustNegotiationOfferPaymentMethodRows())
						{

							try
							{

								// Lock each of the counter party payment methods while a compatible one is found.
								consumerTrustNegotiationOfferPaymentMethodRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

								// All compatible payment methods between the parties become part of the settlement information.
								if (consumerDebtNegotiationOfferPaymentMethodRow.PaymentMethodTypeId ==
									consumerTrustNegotiationOfferPaymentMethodRow.PaymentMethodTypeId)
									debtSettlementInfo.PaymentMethods.Add(
										new PaymentMethodInfo(Guid.NewGuid(), consumerTrustNegotiationOfferPaymentMethodRow.PaymentMethodTypeId));

							}
							finally
							{

								// The counter party payment method is no longer needed.
								consumerTrustNegotiationOfferPaymentMethodRow.ReleaseReaderLock(dataModelTransaction.TransactionId);

							}

						}

					}
					finally
					{

						// This payment method is no longer needed.
						consumerDebtNegotiationOfferPaymentMethodRow.ReleaseReaderLock(dataModelTransaction.TransactionId);

					}

				}

				// This will calculate the amount of time until the first payment based on the amount of time in the negotiation and the time units.
				TimeUnitRow timeUnitRow = DataModel.TimeUnit.TimeUnitKey.Find(consumerDebtNegotiationRow.OfferPaymentStartDateUnitId);
				try
				{
					timeUnitRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
					debtSettlementInfo.PaymentStartDate = CommonConversion.ToDateTime(
						consumerDebtNegotiationRow.OfferPaymentStartDateLength,
						timeUnitRow.TimeUnitCode);
				}
				finally
				{
					timeUnitRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				}

				// This will calculate the real value of the settlement from the negotiated parameters.  All settlements are in terms of market value while
				// the negotiations may take place in terms of percentages, market value or basis points.
				SettlementUnitRow settlementUnitRow = consumerDebtNegotiationRow.SettlementUnitRowByFK_SettlementUnit_ConsumerDebtNegotiation_OfferSettlementUnitId;
				try
				{

					// Lock the SettlementUnit row down while the actual settlement value is calculated.
					settlementUnitRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

					// This will calclate the actual settlement value from the negotiated value and the units used to negotiate.
					switch (settlementUnitRow.SettlementUnitCode)
					{

						case SettlementUnit.BasisPoint:

							debtSettlementInfo.SettlementAmount = Math.Round(
								consumerDebtNegotiationRow.AccountBalance * consumerDebtNegotiationRow.OfferSettlementValue, 2);
							break;

						case SettlementUnit.MarketValue:

							debtSettlementInfo.SettlementAmount = consumerDebtNegotiationRow.OfferSettlementValue;
							break;

						case SettlementUnit.Percent:

							debtSettlementInfo.SettlementAmount = Math.Round(
								consumerDebtNegotiationRow.AccountBalance * consumerDebtNegotiationRow.OfferSettlementValue, 2);
							break;

					}
				}
				finally
				{

					// The SettlementUnit row is no longer needed.
					settlementUnitRow.ReleaseReaderLock(dataModelTransaction.TransactionId);

				}

				// The 'Accepted' status indicates that one side of the negotiation has accepted the offer.
				StatusRow acceptedStatusRow = DataModel.Status.StatusKeyStatusCode.Find(Status.Accepted);
				try
				{
					acceptedStatusRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
					debtSettlementInfo.AcceptedStatusId = acceptedStatusRow.StatusId;
				}
				finally
				{
					acceptedStatusRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				}

				// The 'New' status is use for all freshly crated settlements.  This state is used to tell the settlement engine that a settlement document
				// should be created from the parameters.
				StatusRow newStatusRow = DataModel.Status.StatusKeyStatusCode.Find(Status.New);
				try
				{
					newStatusRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
					debtSettlementInfo.NewStatusId = newStatusRow.StatusId;
				}
				finally
				{
					newStatusRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				}

				// The 'Locked' status indicates that one side of the negotiation has accepted the offer.
				StatusRow offerAcceptedStatusRow = DataModel.Status.StatusKeyStatusCode.Find(Status.OfferAccepted);
				try
				{
					offerAcceptedStatusRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
					debtSettlementInfo.OfferAcceptedStatusId = offerAcceptedStatusRow.StatusId;
				}
				finally
				{
					offerAcceptedStatusRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				}

				// The 'Pending' status indicates that one side of the negotiation has accepted the offer.
				StatusRow pendingStatusRow = DataModel.Status.StatusKeyStatusCode.Find(Status.Pending);
				try
				{
					pendingStatusRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
					debtSettlementInfo.PendingStatusId = pendingStatusRow.StatusId;
				}
				finally
				{
					pendingStatusRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				}

			}
			finally
			{

				// Release the record locks.
				if (consumerDebtNegotiationRow != null)
					consumerDebtNegotiationRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				if (matchRow != null)
					matchRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				if (contraMatchRow != null)
					contraMatchRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				if (blotterRow != null)
					blotterRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				if (debtClassRow != null)
					debtClassRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				if (consumerTrustNegotiationRow != null)
					consumerTrustNegotiationRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				if (workingOrderRow != null)
					workingOrderRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				if (securityRow != null)
					securityRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				if (consumerDebtRow != null)
					consumerDebtRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				if (contraWorkingOrderRow != null)
					contraWorkingOrderRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				if (contraSecurityRow != null)
					contraSecurityRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				if (consumerTrustRow != null)
					consumerTrustRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				if (consumerRow != null)
					consumerRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				if (creditCardRow != null)
					creditCardRow.ReleaseReaderLock(dataModelTransaction.TransactionId);

			}

			// Nothing is done if this order has already been settled.
			if (debtSettlementInfo.DebtStatusId == debtSettlementInfo.AcceptedStatusId)
				return;

			// Nothing is done if this side has already accepted the negotiation and is waiting for the other side to respond.
			if (debtSettlementInfo.DebtStatusId == debtSettlementInfo.PendingStatusId)
				return;

			// Busines Rule #1: Don't allow a settlement if the payment methods are not compatible.
			if (debtSettlementInfo.PaymentMethods.Count == 0)
			{
				throw new FaultException<PaymentMethodFault>(new PaymentMethodFault("The negotiation doesn't contain compatible payment methods."));
			}
			
			// Only when the Consumer Trust side is awaiting a settlement is the settlement generated.
			if (debtSettlementInfo.TrustStatusId == debtSettlementInfo.PendingStatusId)
			{

				// The state of the Match must be updated to reflect that this record is no longer available for negotiation.
				dataModel.UpdateMatch(
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					new Object[] { debtSettlementInfo.MatchId },
					debtSettlementInfo.MatchRowVersion,
					debtSettlementInfo.AcceptedStatusId,
					null);

				// The contra is also updated to reflect the settled state of this negotiation.
				dataModel.UpdateMatch(
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					new Object[] { debtSettlementInfo.ContraMatchId },
					debtSettlementInfo.ContraMatchRowVersion,
					debtSettlementInfo.AcceptedStatusId,
					null);

				// This records the terms of the settlement between the Consumer Trust account and the Consumer Debt account.
				dataModel.CreateConsumerDebtSettlement(
					debtSettlementInfo.AccountBalance,
					debtSettlementInfo.BlotterId,
					debtSettlementInfo.ConsumerDebtNegotiationId,
					debtSettlementInfo.ConsumerDebtSettlementId,
					debtSettlementInfo.CreatedTime,
					debtSettlementInfo.CreatedUserId,
					debtSettlementInfo.DebtorAccountNumber,
					debtSettlementInfo.DebtorAddress1,
					debtSettlementInfo.DebtorAddress2,
					debtSettlementInfo.DebtorBankAccountNumber,
					debtSettlementInfo.DebtorBankRoutingNumber,
					debtSettlementInfo.DebtorCity,
					debtSettlementInfo.DebtorFirstName,
					debtSettlementInfo.DebtorLastName,
					debtSettlementInfo.DebtorMiddleName,
					debtSettlementInfo.DebtorOriginalAccountNumber,
					debtSettlementInfo.DebtorPostalCode,
					debtSettlementInfo.DebtorProvinceId,
					debtSettlementInfo.DebtorSalutation,
					debtSettlementInfo.DebtorSuffix,
					null,
					debtSettlementInfo.ModifiedTime,
					debtSettlementInfo.ModifiedUserId,
					debtSettlementInfo.PayeeAddress1,
					debtSettlementInfo.PayeeAddress2,
					debtSettlementInfo.PayeeBankAccountNumber,
					debtSettlementInfo.PayeeBankRoutingNumber,
					debtSettlementInfo.PayeeCity,
					debtSettlementInfo.PayeeCompanyName,
					debtSettlementInfo.PayeeContactName,
					debtSettlementInfo.PayeeDepartment,
					debtSettlementInfo.PayeeEmail,
					debtSettlementInfo.PayeeFax,
					debtSettlementInfo.PayeeForBenefitOf,
					debtSettlementInfo.PayeePhone,
					debtSettlementInfo.PayeePostalCode,
					debtSettlementInfo.PayeeProvinceId,
					debtSettlementInfo.PaymentLength,
					debtSettlementInfo.PaymentStartDate,
					debtSettlementInfo.SettlementAmount,
					null,
					debtSettlementInfo.NewStatusId);

				// Each of the acceptable payment methods is also written as part of this transaction.
				foreach (PaymentMethodInfo paymentMethodInfo in debtSettlementInfo.PaymentMethods)
				{
					dataModel.CreateConsumerDebtSettlementPaymentMethod(
						debtSettlementInfo.BlotterId,
						debtSettlementInfo.ConsumerDebtSettlementId,
						paymentMethodInfo.ConsumerDebtSettlementPaymentMethodId,
						paymentMethodInfo.PaymentMethodId);
				}

			}
			else
			{

				// At this point, the other party has not yet accepted the offer so we set the status of the Match and wait.
				dataModel.UpdateMatch(
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					new Object[] { debtSettlementInfo.MatchId },
					debtSettlementInfo.MatchRowVersion,
					debtSettlementInfo.PendingStatusId,
					null);

				// The counter party must be advised that they can not alter the state of the settlement once it has been accepted.
				dataModel.UpdateMatch(
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					new Object[] { debtSettlementInfo.ContraMatchId },
					debtSettlementInfo.ContraMatchRowVersion,
					debtSettlementInfo.OfferAcceptedStatusId,
					null);

			}

		}

		/// <summary>
		/// Reset the settlement status bit to New
		/// </summary>
		/// <param name="record"></param>
		public static void ResetSettlement(BaseRecord record)
		{
			DataModelTransaction dataModelTransaction = DataModelTransaction.Current;
			DataModel dataModel = new DataModel();
			Guid modifiedByUser = TradingSupport.DaemonUserId;
			Guid newStatusId = Guid.Empty;
			Guid blotterId = Guid.Empty;

			ConsumerDebtSettlementRow consumerDebtSettlement = DataModel.ConsumerDebtSettlement.ConsumerDebtSettlementKey.Find(record.RowId);
			consumerDebtSettlement.AcquireReaderLock(dataModelTransaction);

			try
			{
				blotterId = consumerDebtSettlement.BlotterId;				
			}
			finally
			{
				consumerDebtSettlement.ReleaseReaderLock(dataModelTransaction.TransactionId);
			}

			// If we switch from explicitly deleting the working order to explicitly deleting the security, then we need this.
			if (!TradingSupport.HasAccess(dataModelTransaction, blotterId, AccessRight.Write))
				throw new FaultException<FluidTrade.Core.SecurityFault>(new SecurityFault("You do not have write access to the selected object."));



			// The 'New' status is use for all freshly crated settlements.  This state is used to tell the settlement engine that a settlement document
			// should be created from the parameters.
			StatusRow newStatusRow = DataModel.Status.StatusKeyStatusCode.Find(Status.New);
			try
			{
				newStatusRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
				newStatusId = newStatusRow.StatusId;
			}
			finally
			{
				newStatusRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
			}

			dataModel.UpdateConsumerDebtSettlement(
			null,
			null,
			null,
			null,
			new object[] { record.RowId },
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
			null,
			null,
			record.RowVersion,
			null,
			null,
			newStatusId);
				
		}
	
		/// <summary>
		/// Information about a Consumer Trust Settlement
		/// </summary>
		private class TrustSettlementInfo
		{

			// Public Instance Fields
			public Guid AcceptedStatusId;
			public Decimal AccountBalance;
			public Guid BlotterId;
			public Guid ConsumerTrustNegotiationId;
			public Guid ConsumerTrustSettlementId;
			public DateTime CreatedTime;
			public Guid CreatedUserId;			
			public String DebtorAccountNumber;
			public Object DebtorAddress1;
			public Object DebtorAddress2;
			public Object DebtorBankAccountNumber;
			public Object DebtorBankRoutingNumber;
			public Object DebtorFirstName;
			public Object DebtorLastName;
			public Object DebtorMiddleName;
			public String DebtorOriginalAccountNumber;
			public Object DebtorSalutation;
			public Object DebtorSuffix;
			public Object DebtorCity;
			public Object DebtorProvinceId;
			public Object DebtorPostalCode;
			public DateTime ModifiedTime;
			public Guid ModifiedUserId;
			public Object PayeeAddress1;
			public Object PayeeAddress2;
			public Object PayeeBankAccountNumber;
			public Object PayeeBankRoutingNumber;
			public Object PayeeCity;
			public Object PayeeCompanyName;
			public Object PayeeContactName;
			public Object PayeeDepartment;
			public Object PayeeEmail;
			public Object PayeeFax;
			public Object PayeeForBenefitOf;
			public Object PayeePhone;
			public Object PayeeProvinceId;
			public Object PayeePostalCode;
			public Decimal PaymentLength;
			public Guid PendingStatusId;
			public List<PaymentMethodInfo> PaymentMethods;
			public DateTime PaymentStartDate;
			public Decimal SettlementAmount;
			public String SettlementLetter;

			/// <summary>
			/// Create an object to hold information about a settlement.
			/// </summary>
			public TrustSettlementInfo()
			{

				// Initialize the object
				this.PaymentMethods = new List<PaymentMethodInfo>();

			}

		};

		/// <summary>
		/// Accept the terms of the settlement and pass the settlement letter and terms to the counter party.
		/// </summary>
		/// <param name="consumerDebtSettlementAcceptInfos">An array of elements describing all the settlements to be approved.</param>
		public static void AcceptConsumerDebtSettlement(ConsumerDebtSettlementAcceptInfo[] consumerDebtSettlementAcceptInfos)
		{

			// A reference to the data model is required to query the database within the scope of a transaction.
			DataModel dataModel = new DataModel();
			Decimal effectivePaymentValue = 0.0m;
			Decimal fee0 = 0.0m;

			// This method can approve zero or more settlements as a batch.
			foreach (ConsumerDebtSettlementAcceptInfo consumerDebtSettlementAcceptInfo in consumerDebtSettlementAcceptInfos)
			{

				// The locking model does not provide for recursive reader locks or promoting reader locks to writer locks.  So the data is collected during a
				// phase then the table can be locked determinstically to prevent deadlocks, then the calls to update the data model are made once all the
				// reader locks have been released.  This structure holds the information required for the creation of a settlement record.
				TrustSettlementInfo trustSettlementInfo = new TrustSettlementInfo();

				// Extract the ambient transaction.
				DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

				// These rows will be locked momentarily while the data is collected and released before the actual transaction.
				ConsumerDebtSettlementRow consumerDebtSettlementRow = null;
				MatchRow matchRow = null;
				MatchRow contraMatchRow = null;
				BlotterRow contraBlotterRow = null;
				ConsumerDebtNegotiationRow consumerDebtNegotiationRow = null;
				ConsumerTrustNegotiationRow consumerTrustNegotiationRow = null;

				try
				{

					// The starting point for creating a settlement record is to find the negotiation record that has been agreed to by both parties.
					consumerDebtSettlementRow = DataModel.ConsumerDebtSettlement.ConsumerDebtSettlementKey.Find(
						consumerDebtSettlementAcceptInfo.ConsumerDebtSettlementId);
					consumerDebtSettlementRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

					// The negotiation row is used to find the match.
					consumerDebtNegotiationRow = consumerDebtSettlementRow.ConsumerDebtNegotiationRow;
					consumerDebtNegotiationRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

					// This is the record used to match this asset against another.
					matchRow = consumerDebtNegotiationRow.MatchRow;
					matchRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

					// The next step is to find the counter parties matching information which will lead us to the counter parties asset which, in turn, contains
					// more information for the settlement.
					contraMatchRow = DataModel.Match.MatchKey.Find(matchRow.ContraMatchId);
					contraMatchRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

					// This record contains the information that the Consumer Trust representative uses to settle an account.
					consumerTrustNegotiationRow = contraMatchRow.GetConsumerTrustNegotiationRows()[0];
					consumerTrustNegotiationRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

					// The blotter contains a link to the Debt Class which is where the Payee information is found.
					contraBlotterRow = contraMatchRow.BlotterRow;
					contraBlotterRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

					// These values are extracted from the data model while reader locks are in place on the related records.  Since the locks aren't recursive
					// and reader locks can't be promoted, any locks held for this collection process must be released before the middle tier methods are
					// called to create the record.
					trustSettlementInfo.AccountBalance = consumerDebtSettlementRow.AccountBalance;
					trustSettlementInfo.BlotterId = contraBlotterRow.BlotterId;
					trustSettlementInfo.ConsumerTrustNegotiationId = consumerTrustNegotiationRow.ConsumerTrustNegotiationId;
					trustSettlementInfo.ConsumerTrustSettlementId = Guid.NewGuid();
					trustSettlementInfo.CreatedTime = DateTime.UtcNow;
					trustSettlementInfo.CreatedUserId = TradingSupport.DaemonUserId;
					trustSettlementInfo.DebtorAccountNumber = consumerDebtSettlementRow.DebtorAccountNumber;
					trustSettlementInfo.DebtorAddress1 = consumerDebtSettlementRow.IsDebtorAddress1Null() ? null : consumerDebtSettlementRow.DebtorAddress1;
					trustSettlementInfo.DebtorAddress2 = consumerDebtSettlementRow.IsDebtorAddress2Null() ? null : consumerDebtSettlementRow.DebtorAddress2;
					trustSettlementInfo.DebtorBankAccountNumber = consumerDebtSettlementRow.IsDebtorBankAccountNumberNull() ? null : consumerDebtSettlementRow.DebtorBankAccountNumber;
					trustSettlementInfo.DebtorBankRoutingNumber = consumerDebtSettlementRow.IsDebtorBankRoutingNumberNull() ? null : consumerDebtSettlementRow.DebtorBankRoutingNumber;
					trustSettlementInfo.DebtorFirstName = consumerDebtSettlementRow.IsDebtorFirstNameNull() ? null : consumerDebtSettlementRow.DebtorFirstName;
					trustSettlementInfo.DebtorLastName = consumerDebtSettlementRow.IsDebtorLastNameNull() ? null : consumerDebtSettlementRow.DebtorLastName;
					trustSettlementInfo.DebtorMiddleName = consumerDebtSettlementRow.IsDebtorMiddleNameNull() ? null : consumerDebtSettlementRow.DebtorMiddleName;
					trustSettlementInfo.DebtorOriginalAccountNumber = consumerDebtSettlementRow.DebtorOriginalAccountNumber;
					trustSettlementInfo.DebtorSalutation = consumerDebtSettlementRow.IsDebtorSalutationNull() ? null : consumerDebtSettlementRow.DebtorSalutation;
					trustSettlementInfo.DebtorSuffix = consumerDebtSettlementRow.IsDebtorSuffixNull() ? null : consumerDebtSettlementRow.DebtorSuffix;
					trustSettlementInfo.DebtorCity = consumerDebtSettlementRow.IsDebtorCityNull() ? null : consumerDebtSettlementRow.DebtorCity;
					trustSettlementInfo.DebtorProvinceId = consumerDebtSettlementRow.IsDebtorProvinceIdNull() ? null : (Object)consumerDebtSettlementRow.DebtorProvinceId;
					trustSettlementInfo.DebtorPostalCode = consumerDebtSettlementRow.IsDebtorPostalCodeNull() ? null : consumerDebtSettlementRow.DebtorPostalCode;
					trustSettlementInfo.ModifiedTime = trustSettlementInfo.CreatedTime;
					trustSettlementInfo.ModifiedUserId = trustSettlementInfo.CreatedUserId;
					trustSettlementInfo.PayeeAddress1 = consumerDebtSettlementRow.IsPayeeAddress1Null() ? null : consumerDebtSettlementRow.PayeeAddress1;
					trustSettlementInfo.PayeeAddress2 = consumerDebtSettlementRow.IsPayeeAddress2Null() ? null : consumerDebtSettlementRow.PayeeAddress2;
					trustSettlementInfo.PayeeBankAccountNumber = consumerDebtSettlementRow.IsPayeeBankAccountNumberNull() ? null : consumerDebtSettlementRow.PayeeBankAccountNumber;
					trustSettlementInfo.PayeeBankRoutingNumber = consumerDebtSettlementRow.IsPayeeBankRoutingNumberNull() ? null : consumerDebtSettlementRow.PayeeBankRoutingNumber;
					trustSettlementInfo.PayeeCity = consumerDebtSettlementRow.IsPayeeCityNull() ? null : consumerDebtSettlementRow.PayeeCity;
					trustSettlementInfo.PayeeCompanyName = consumerDebtSettlementRow.IsPayeeCompanyNameNull() ? null : consumerDebtSettlementRow.PayeeCompanyName;
					trustSettlementInfo.PayeeContactName = consumerDebtSettlementRow.IsPayeeContactNameNull() ? null : consumerDebtSettlementRow.PayeeContactName;
					trustSettlementInfo.PayeeDepartment = consumerDebtSettlementRow.IsPayeeDepartmentNull() ? null : consumerDebtSettlementRow.PayeeDepartment;
					trustSettlementInfo.PayeeEmail = consumerDebtSettlementRow.IsPayeeEmailNull() ? null : consumerDebtSettlementRow.PayeeEmail;
					trustSettlementInfo.PayeeFax = consumerDebtSettlementRow.IsPayeeFaxNull() ? null : consumerDebtSettlementRow.PayeeFax;
					trustSettlementInfo.PayeeForBenefitOf = consumerDebtSettlementRow.IsPayeeForBenefitOfNull() ? null : consumerDebtSettlementRow.PayeeForBenefitOf;
					trustSettlementInfo.PayeePhone = consumerDebtSettlementRow.IsPayeePhoneNull() ? null : consumerDebtSettlementRow.PayeePhone;
					trustSettlementInfo.PayeeProvinceId = consumerDebtSettlementRow.IsPayeeProvinceIdNull() ? null : (Object)consumerDebtSettlementRow.PayeeProvinceId;
					trustSettlementInfo.PayeePostalCode = consumerDebtSettlementRow.IsPayeePostalCodeNull() ? null : consumerDebtSettlementRow.PayeePostalCode;
					trustSettlementInfo.PaymentLength = consumerDebtSettlementRow.PaymentLength;
					trustSettlementInfo.PaymentStartDate = consumerDebtSettlementRow.PaymentStartDate;
					trustSettlementInfo.SettlementAmount = consumerDebtSettlementRow.SettlementAmount;
					trustSettlementInfo.SettlementLetter = consumerDebtSettlementRow.SettlementLetter;

					// This next section will calculate the commissions for the transactions.
					DebtClassRow debtClassRow = null;
					CommissionScheduleRow commissionScheduleRow = null;
					effectivePaymentValue = Math.Round(trustSettlementInfo.SettlementAmount / trustSettlementInfo.PaymentLength, 2);
					Guid commissionScheduleId = Guid.Empty;
					
					try
					{

						// The Debt Class describes the commissions applied to the payments which are generated below.
						debtClassRow = DataModel.DebtClass.DebtClassKey.Find(contraBlotterRow.BlotterId);
						debtClassRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
						

						if (debtClassRow.IsCommissionScheduleIdNull())
							commissionScheduleId = GetParentScheduleId(debtClassRow, dataModelTransaction);
						else
							commissionScheduleId = debtClassRow.CommissionScheduleId;
					}
					finally
					{

						// These locks are no longer required.
						if (debtClassRow != null && debtClassRow.IsReaderLockHeld(dataModelTransaction.TransactionId))
							debtClassRow.ReleaseReaderLock(dataModelTransaction.TransactionId);

					}

					if (commissionScheduleId == null || commissionScheduleId == Guid.Empty)
					{
						throw new FaultException<ArgumentFault>(new ArgumentFault("Commission Schedule Id"),
							new FaultReason("No commission schedule found for this account."));
					}

					try
					{

						// This schedule describes how the commission is calculated for the different tranches.
						commissionScheduleRow = DataModel.CommissionSchedule.CommissionScheduleKey.Find(commissionScheduleId);
						commissionScheduleRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

						// Cycle through all the commission tranches and find out which range is applicable to this transaction.
						foreach (CommissionTrancheRow commissionTranchRow in commissionScheduleRow.GetCommissionTrancheRows())
						{

							// The ranges determine if the given tranche is applied.
							Decimal startingRange = 0.0m;
							Decimal endingRange = 0.0m;
							CommissionTypeRow commissionTypeRow = null;

							try
							{

								// Lock each Tranch as we pass through the schedule looking for applicable ranges.
								commissionTranchRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

								// The tranche type is also needed to determine how to calculate the commission.
								commissionTypeRow = commissionTranchRow.CommissionTypeRow;
								commissionTypeRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

								// Extract from the commission tranch the start and end point.
								startingRange = commissionTranchRow.StartRange;
								endingRange = commissionTranchRow.IsEndRangeNull() ? Decimal.MaxValue : commissionTranchRow.EndRange;

								// Currently Commission units are assumed to be market value.  This should be changed to allow for other units.
								if (startingRange < effectivePaymentValue && effectivePaymentValue <= endingRange)
								{

									switch (commissionTypeRow.CommissionTypeCode)
									{
									case CommissionType.Fee:

										fee0 = 0.0m;
										break;

									case CommissionType.Percent:

										fee0 = effectivePaymentValue * commissionTranchRow.Value;
										break;

									case CommissionType.BasisPoint:

										fee0 = effectivePaymentValue * (commissionTranchRow.Value / 100);
										break;

									}

								}

							}
							finally
							{

								// The locks can be released after each pass through the loop.
								if (commissionTranchRow.IsReaderLockHeld(dataModelTransaction.TransactionId))
									commissionTranchRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
								if (commissionTypeRow.IsReaderLockHeld(dataModelTransaction.TransactionId))
									commissionTypeRow.ReleaseReaderLock(dataModelTransaction.TransactionId);

							}

						}

					}
					finally
					{

						// These locks are no longer needed.
						if (commissionScheduleRow != null && commissionScheduleRow.IsReaderLockHeld(dataModelTransaction.TransactionId))
							commissionScheduleRow.ReleaseReaderLock(dataModelTransaction.TransactionId);

					}

					
					// Copy the consumer debt settlement payment types into the new settlement record.
					foreach (ConsumerDebtSettlementPaymentMethodRow consumerDebtSettlementPaymentMethodRow
						in consumerDebtSettlementRow.GetConsumerDebtSettlementPaymentMethodRows())
					{

						try
						{

							// The payment method needs to be locked while we check the counter party to see what types of payment methods are used there.
							consumerDebtSettlementPaymentMethodRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
							trustSettlementInfo.PaymentMethods.Add(
								new PaymentMethodInfo(Guid.NewGuid(), consumerDebtSettlementPaymentMethodRow.PaymentMethodTypeId));

						}
						finally
						{

							// This payment method is no longer needed.
							if (consumerDebtSettlementPaymentMethodRow.IsReaderLockHeld(dataModelTransaction.TransactionId))
								consumerDebtSettlementPaymentMethodRow.ReleaseReaderLock(dataModelTransaction.TransactionId);

						}

					}

					// The 'Accepted' status indicates that one side of the negotiation has accepted the offer.
					StatusRow acceptedStatusRow = DataModel.Status.StatusKeyStatusCode.Find(Status.Accepted);
					try
					{
						acceptedStatusRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
						trustSettlementInfo.AcceptedStatusId = acceptedStatusRow.StatusId;
					}
					finally
					{
						acceptedStatusRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
					}

					// The 'Accepted' status indicates that one side of the negotiation has accepted the offer.
					StatusRow pendingStatusRow = DataModel.Status.StatusKeyStatusCode.Find(Status.Pending);
					try
					{
						pendingStatusRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
						trustSettlementInfo.PendingStatusId = pendingStatusRow.StatusId;
					}
					finally
					{
						pendingStatusRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
					}

				}
				finally
				{

					// Release the record locks.
					if (consumerDebtSettlementRow != null && consumerDebtSettlementRow.IsReaderLockHeld(dataModelTransaction.TransactionId))
						consumerDebtSettlementRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
					if (matchRow != null && matchRow.IsReaderLockHeld(dataModelTransaction.TransactionId))
						matchRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
					if (contraMatchRow != null && contraMatchRow.IsReaderLockHeld(dataModelTransaction.TransactionId))
						contraMatchRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
					if (contraBlotterRow != null && contraBlotterRow.IsReaderLockHeld(dataModelTransaction.TransactionId))
						contraBlotterRow.ReleaseLock(dataModelTransaction.TransactionId);
					if (consumerDebtNegotiationRow != null && consumerDebtNegotiationRow.IsReaderLockHeld(dataModelTransaction.TransactionId))
						consumerDebtNegotiationRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
					if (consumerTrustNegotiationRow != null && consumerTrustNegotiationRow.IsReaderLockHeld(dataModelTransaction.TransactionId))
						consumerTrustNegotiationRow.ReleaseReaderLock(dataModelTransaction.TransactionId);

				}

				// This updates the state of the settlement on the Debt Holder of the transaction.
				dataModel.UpdateConsumerDebtSettlement(
					null,
					null,
					null,
					null,
					new Object[] { consumerDebtSettlementAcceptInfo.ConsumerDebtSettlementId },
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
					null,
					null,
					null,
					null,
					null,
					null,
					trustSettlementInfo.ModifiedTime,
					trustSettlementInfo.ModifiedUserId,
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
					null,
					null,
					null,
					null,
					null,
					consumerDebtSettlementAcceptInfo.RowVersion,
					null,
					null,
					trustSettlementInfo.AcceptedStatusId);

				// This records the terms of the settlement between the Consumer Trust account and the Consumer Debt account.  That is, it effectively moves 
				// the settlement through the Chinese Wall.
				dataModel.CreateConsumerTrustSettlement(
					trustSettlementInfo.AccountBalance,
					trustSettlementInfo.BlotterId,
					trustSettlementInfo.ConsumerTrustNegotiationId,
					trustSettlementInfo.ConsumerTrustSettlementId,
					trustSettlementInfo.CreatedTime,
					trustSettlementInfo.CreatedUserId,
					trustSettlementInfo.DebtorAccountNumber,
					trustSettlementInfo.DebtorAddress1,
					trustSettlementInfo.DebtorAddress2,
					trustSettlementInfo.DebtorBankAccountNumber,
					trustSettlementInfo.DebtorBankRoutingNumber,
					trustSettlementInfo.DebtorCity,
					trustSettlementInfo.DebtorFirstName,
					trustSettlementInfo.DebtorLastName,
					trustSettlementInfo.DebtorMiddleName,
					trustSettlementInfo.DebtorOriginalAccountNumber,
					trustSettlementInfo.DebtorPostalCode,
					trustSettlementInfo.DebtorProvinceId,
					trustSettlementInfo.DebtorSalutation,
					trustSettlementInfo.DebtorSuffix,
					null,
					trustSettlementInfo.ModifiedTime,
					trustSettlementInfo.ModifiedUserId,
					trustSettlementInfo.PayeeAddress1,
					trustSettlementInfo.PayeeAddress2,
					trustSettlementInfo.PayeeBankAccountNumber,
					trustSettlementInfo.PayeeBankRoutingNumber,
					trustSettlementInfo.PayeeCity,
					trustSettlementInfo.PayeeCompanyName,
					trustSettlementInfo.PayeeContactName,
					trustSettlementInfo.PayeeDepartment,
					trustSettlementInfo.PayeeEmail,
					trustSettlementInfo.PayeeFax,
					trustSettlementInfo.PayeeForBenefitOf,
					trustSettlementInfo.PayeePhone,
					trustSettlementInfo.PayeePostalCode,
					trustSettlementInfo.PayeeProvinceId,
					trustSettlementInfo.PaymentLength,
					trustSettlementInfo.PaymentStartDate,
					trustSettlementInfo.SettlementAmount,
					trustSettlementInfo.SettlementLetter,
					trustSettlementInfo.AcceptedStatusId);

				// Each of the acceptable payment methods is also written as part of this transaction.
				foreach (PaymentMethodInfo paymentMethodInfo in trustSettlementInfo.PaymentMethods)
				{
					dataModel.CreateConsumerTrustSettlementPaymentMethod(
						trustSettlementInfo.BlotterId,
						trustSettlementInfo.ConsumerTrustSettlementId,
						paymentMethodInfo.ConsumerDebtSettlementPaymentMethodId,
						paymentMethodInfo.PaymentMethodId);
				}

				// Create Payment records for both sides of the transaction.
				DateTime paymentEffectiveDate = trustSettlementInfo.PaymentStartDate;
				Decimal totalEffectivePaymentValue = 0.0M;
				for (int payment = 0; payment < trustSettlementInfo.PaymentLength; payment++)
				{

					// Generate a new identifier for each payment in the schedule.
					Guid paymentID = Guid.NewGuid();

					// If this is the last record then we need to take care of the rounding error.
					if (payment == trustSettlementInfo.PaymentLength - 1)
					{
						effectivePaymentValue = trustSettlementInfo.SettlementAmount - totalEffectivePaymentValue;
					}
					else
						totalEffectivePaymentValue += effectivePaymentValue;


					// Create a payment record on the Debt Settlement side.
					dataModel.CreateConsumerTrustPayment(
						 null,
						0,
						trustSettlementInfo.BlotterId,
						0,
						DateTime.UtcNow,
						paymentID,
						trustSettlementInfo.ConsumerTrustSettlementId,
						trustSettlementInfo.CreatedTime,
						trustSettlementInfo.CreatedUserId,
						paymentEffectiveDate,
						effectivePaymentValue,
						fee0,
						0,
						0,
						0,
						null,
						null,
						null,
						null,
						null,
						null,
						trustSettlementInfo.ModifiedTime,
						trustSettlementInfo.ModifiedUserId,
						trustSettlementInfo.PendingStatusId,
						null
						);

					// Create a payment record on the Debt Holder side.
					dataModel.CreateConsumerDebtPayment(
						 null,
						0,
						consumerDebtSettlementRow.BlotterId,
						0,
						DateTime.UtcNow,
						paymentID,
						consumerDebtSettlementAcceptInfo.ConsumerDebtSettlementId,
						trustSettlementInfo.CreatedTime,
						trustSettlementInfo.CreatedUserId,
						consumerDebtSettlementRow.BlotterId,
						paymentEffectiveDate,
						effectivePaymentValue,
						fee0,
						0,
						0,
						0,
						null,
						null,
						null,
						null,
						null,
						null,
						trustSettlementInfo.ModifiedTime,
						trustSettlementInfo.ModifiedUserId,
						trustSettlementInfo.PendingStatusId,
						null
						);


					// Calculate the next effective payment.
					paymentEffectiveDate = paymentEffectiveDate.AddMonths(1);

				}
			}
		}

		/// <summary>
		/// Get Parents Schedule Id if available
		/// </summary>
		/// <param name="debtClassRow"></param>
		/// <param name="dataModelTransaction"></param>
		/// <returns>Guid.Empty if parent Schedule Id is not available</returns>
		private static Guid GetParentScheduleId(DebtClassRow debtClassRow, DataModelTransaction dataModelTransaction)
		{
			//Sanity check - we do not need to acquire a lock if is already held
			bool acquiredLock = false;
			if (debtClassRow.IsReaderLockHeld(dataModelTransaction.TransactionId) == false)
			{
				acquiredLock = true;
				debtClassRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
			}

			try
			{
				//Walk up the entitytree table to see if we can find a debtclass with schedule Id.
				EntityRow entityRow = DataModel.Entity.EntityKey.Find(debtClassRow.DebtClassId);
				entityRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
				try
				{
					foreach (EntityTreeRow entityTreeRow in entityRow.GetEntityTreeRowsByFK_Entity_EntityTree_ChildId())
					{
						entityTreeRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
						DebtClassRow parentDebtClassRow = null;
						try 
						{
							parentDebtClassRow = DataModel.DebtClass.DebtClassKey.Find(entityTreeRow.ParentId);
							if(parentDebtClassRow != null)
							{
								parentDebtClassRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

								if (parentDebtClassRow.IsCommissionScheduleIdNull() == false)
									return parentDebtClassRow.CommissionScheduleId;
								else
									//Recusively probe the parents for a valid CommissionScheduleId
									return GetParentScheduleId(parentDebtClassRow, dataModelTransaction);
							}
						}
						finally
						{
							//Release any locks held.
							entityTreeRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
							if(parentDebtClassRow != null && parentDebtClassRow.IsReaderLockHeld(dataModelTransaction.TransactionId))
								parentDebtClassRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
						}
					}
				}
				finally
				{
					entityRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				}
			}
			finally
			{
				if (acquiredLock)
					debtClassRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
			}

			return Guid.Empty;
		}

	}

}
