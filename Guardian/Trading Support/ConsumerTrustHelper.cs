namespace FluidTrade.Guardian
{

	using System;
	using System.Collections.Generic;
	using System.ServiceModel;
	using FluidTrade.Core;

	/// <summary>
	/// Additional methods to help manage the Consumer Trust records.
	/// </summary>
	/// <copyright>Copyright © 2007 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	internal class ConsumerTrustHelper
	{

		/// <summary>
		/// Information about a payment method for a given settlement.
		/// </summary>
		private class PaymentMethodInfo
		{

			// Public Instance Fields
			public Guid ConsumerTrustSettlementPaymentMethodId;
			public Guid PaymentMethodId;

			/// <summary>
			/// Create a payment method for a settlement.
			/// </summary>
			/// <param name="consumerTrustSettlementPaymentMethodId">The unique identifier for a payment method in this settlement.</param>
			/// <param name="paymentMethodId">The unique identifier of a payment method.</param>
			public PaymentMethodInfo(Guid consumerTrustSettlementPaymentMethodId, Guid paymentMethodId)
			{

				// Initialize the object
				this.ConsumerTrustSettlementPaymentMethodId = consumerTrustSettlementPaymentMethodId;
				this.PaymentMethodId = paymentMethodId;
			}

		}

		/// <summary>
		/// Information about a Consumer Trust Settlement
		/// </summary>
		private class TrustSettlementInfo
		{

			// Public Instance Fields
			public Decimal AccountBalance;
			public Guid AcceptedStatusId;
			public Guid BlotterId;
			public Guid ConsumerTrustNegotiationId;
			public Guid ConsumerTrustSettlementId;
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
			public TrustSettlementInfo()
			{

				// Initialize the object
				this.PaymentMethods = new List<PaymentMethodInfo>();

			}

		};

		/// <summary>
		/// Creates a settlement for a Consumer Trust representative.
		/// </summary>
		/// <param name="consumerTrustNegotiationId">The identifier of the negotiation that has been agreed to by both parties.</param>
		public static void CreateConsumerTrustSettlement(Guid consumerTrustNegotiationId)
		{

			// A reference to the data model is required to query the database within the scope of a transaction.
			DataModel dataModel = new DataModel();

			// The locking model does not provide for recursive reader locks or promoting reader locks to writer locks.  So the data is collected
			// during a phase then the table can be locked determinstically to prevent deadlocks, then the calls to update the data model are made
			// once all the reader locks have been released.  This structure holds the information required for the creation of a settlement record.
			TrustSettlementInfo trustSettlementInfo = new TrustSettlementInfo();

			// Extract the ambient transaction.
			DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

			// These rows will be locked momentarily while the data is collected and released before the actual transaction.
			ConsumerTrustNegotiationRow consumerTrustNegotiationRow = null;
			MatchRow matchRow = null;
			MatchRow contraMatchRow = null;
			BlotterRow blotterRow = null;
			BlotterRow contraBlotterRow = null;
			DebtClassRow debtClassRow = null;
			ConsumerDebtNegotiationRow consumerDebtNegotiationRow = null;
			WorkingOrderRow workingOrderRow = null;
			SecurityRow securityRow = null;
			ConsumerTrustRow consumerTrustRow = null;
			WorkingOrderRow contraWorkingOrderRow = null;
			SecurityRow contraSecurityRow = null;
			ConsumerDebtRow consumerDebtRow = null;
			ConsumerRow consumerRow = null;
			CreditCardRow creditCardRow = null;

			try
			{

				// The starting point for creating a settlement record is to find the negotiation record that has been agreed to by both parties.
				consumerTrustNegotiationRow = DataModel.ConsumerTrustNegotiation.ConsumerTrustNegotiationKey.Find(consumerTrustNegotiationId);
				consumerTrustNegotiationRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

				// This is the record used to match this asset against another.
				matchRow = consumerTrustNegotiationRow.MatchRow;
				matchRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

				// The next step is to find the counter parties matching information which will lead us to the counter parties asset which, in turn, contains
				// more information for the settlement.
				contraMatchRow = DataModel.Match.MatchKey.Find(matchRow.ContraMatchId);
				contraMatchRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

				// The blotter contains a link to the Debt Class which is where the Payee information is found.
				blotterRow = matchRow.BlotterRow;
				blotterRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

				// The blotter contains a link to the Debt Class which is where the Payee information is found.
				contraBlotterRow = contraMatchRow.BlotterRow;
				contraBlotterRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

				// The debt class of the debt holder provides information about the Payee.
				debtClassRow = contraBlotterRow.GetDebtClassRows()[0];
				debtClassRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

				// The negotiation table has a historical component. Ever time a change is made to the negotiation on either side a completely new record
				// is created to record the change.  While the earlier versions are useful for a historical context and for reports, this console is only
				// interested in the current version of the negotiations.
				Int64 maxVersion = Int64.MinValue;
				foreach (ConsumerDebtNegotiationRow versionRow in contraMatchRow.GetConsumerDebtNegotiationRows())
				{
					try
					{
						versionRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
						if (versionRow.Version > maxVersion)
						{
							maxVersion = versionRow.Version;
							consumerDebtNegotiationRow = versionRow;
						}
					}
					finally
					{
						versionRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
					}
				}
				consumerDebtNegotiationRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

				// The working order record is part of the object oriented path that will lead to the the asset information.  This info is also needed for the
				// settlement record.
				workingOrderRow = matchRow.WorkingOrderRow;
				workingOrderRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

				// The Security record will lead us to the asset.
				securityRow = workingOrderRow.SecurityRowByFK_Security_WorkingOrder_SecurityId;
				securityRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

				// This row contains the actual asset that is to be matched.
				consumerTrustRow = securityRow.GetConsumerTrustRows()[0];
				consumerTrustRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

				// The counter party's asset information is also required.
				contraWorkingOrderRow = contraMatchRow.WorkingOrderRow;
				contraWorkingOrderRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

				// This will lead to the counter party's asset.
				contraSecurityRow = contraWorkingOrderRow.SecurityRowByFK_Security_WorkingOrder_SecurityId;
				contraSecurityRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

				// This is the asset belonging to the counter party that has just agreed to a settlement.
				consumerDebtRow = contraSecurityRow.GetConsumerDebtRows()[0];
				consumerDebtRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

				// This is the Debt Negotiator's version of the Consumer will be used to settle the account.
				consumerRow = consumerTrustRow.ConsumerRow;
				consumerRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

				// We also need to know which credit card was settled.
				creditCardRow = consumerDebtRow.CreditCardRow;
				creditCardRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

				// These values are extracted from the data model while reader locks are in place on the related records.  Since the locks aren't recursive
				// and reader locks can't be promoted, any locks held for this collection process must be released before the middle tier methods are
				// called to create the record.
				trustSettlementInfo.AccountBalance = consumerDebtNegotiationRow.AccountBalance;
				trustSettlementInfo.BlotterId = contraWorkingOrderRow.BlotterId;
				trustSettlementInfo.ConsumerTrustNegotiationId = consumerDebtNegotiationRow.ConsumerDebtNegotiationId;
				trustSettlementInfo.ConsumerTrustSettlementId = Guid.NewGuid();
				trustSettlementInfo.ContraMatchId = contraMatchRow.MatchId;
				trustSettlementInfo.ContraMatchRowVersion = contraMatchRow.RowVersion;
				trustSettlementInfo.CreatedTime = DateTime.UtcNow;
				trustSettlementInfo.CreatedUserId = TradingSupport.DaemonUserId;					
				trustSettlementInfo.DebtorAccountNumber = creditCardRow.AccountNumber;
				trustSettlementInfo.DebtorAddress1 = consumerRow.IsAddress1Null() ? null : consumerRow.Address1;
				trustSettlementInfo.DebtorAddress2 = consumerRow.IsAddress2Null() ? null : consumerRow.Address2;
				trustSettlementInfo.DebtorBankAccountNumber = consumerRow.IsBankAccountNumberNull() ? null : consumerRow.BankAccountNumber;
				trustSettlementInfo.DebtorBankRoutingNumber = consumerRow.IsBankRoutingNumberNull() ? null : consumerRow.BankRoutingNumber;
				trustSettlementInfo.DebtorFirstName = consumerRow.IsFirstNameNull() ? null : consumerRow.FirstName;
				trustSettlementInfo.DebtorLastName = consumerRow.IsLastNameNull() ? null : consumerRow.LastName;
				trustSettlementInfo.DebtorMiddleName = consumerRow.IsMiddleNameNull() ? null : consumerRow.MiddleName;
				trustSettlementInfo.DebtorOriginalAccountNumber = creditCardRow.OriginalAccountNumber;
				trustSettlementInfo.DebtorSalutation = consumerRow.IsSalutationNull() ? null : consumerRow.Salutation;
				trustSettlementInfo.DebtorSuffix = consumerRow.IsSuffixNull() ? null : consumerRow.Suffix;
				trustSettlementInfo.DebtorCity = consumerRow.IsCityNull() ? null : consumerRow.City;
				trustSettlementInfo.DebtorProvinceId = consumerRow.IsProvinceIdNull() ? null : (Object)consumerRow.ProvinceId;
				trustSettlementInfo.DebtorPostalCode = consumerRow.IsPostalCodeNull() ? null : consumerRow.PostalCode;
				trustSettlementInfo.DebtStatusId = contraMatchRow.StatusId;
				trustSettlementInfo.MatchId = matchRow.MatchId;
				trustSettlementInfo.MatchRowVersion = matchRow.RowVersion;
				trustSettlementInfo.ModifiedTime = trustSettlementInfo.CreatedTime;
				trustSettlementInfo.ModifiedUserId = trustSettlementInfo.CreatedUserId;
				trustSettlementInfo.PayeeAddress1 = debtClassRow.IsAddress1Null() ? null : debtClassRow.Address1;
				trustSettlementInfo.PayeeAddress2 = debtClassRow.IsAddress2Null() ? null : debtClassRow.Address2;
				trustSettlementInfo.PayeeBankAccountNumber = debtClassRow.IsBankAccountNumberNull() ? null : debtClassRow.BankAccountNumber;
				trustSettlementInfo.PayeeBankRoutingNumber = debtClassRow.IsBankRoutingNumberNull() ? null : debtClassRow.BankRoutingNumber;
				trustSettlementInfo.PayeeCity = debtClassRow.IsCityNull() ? null : debtClassRow.City;
				trustSettlementInfo.PayeeCompanyName = debtClassRow.IsCompanyNameNull() ? null : debtClassRow.CompanyName;
				trustSettlementInfo.PayeeContactName = debtClassRow.IsContactNameNull() ? null : debtClassRow.ContactName;
				trustSettlementInfo.PayeeDepartment = debtClassRow.IsDepartmentNull() ? null : debtClassRow.Department;
				trustSettlementInfo.PayeeEmail = debtClassRow.IsEmailNull() ? null : debtClassRow.Email;
				trustSettlementInfo.PayeeFax = debtClassRow.IsFaxNull() ? null : debtClassRow.Fax;
				trustSettlementInfo.PayeeForBenefitOf = debtClassRow.IsForBenefitOfNull() ? null : debtClassRow.ForBenefitOf;
				trustSettlementInfo.PayeePhone = debtClassRow.IsPhoneNull() ? null : debtClassRow.Phone;
				trustSettlementInfo.PayeeProvinceId = debtClassRow.IsProvinceIdNull() ? null : (Object)debtClassRow.ProvinceId;
				trustSettlementInfo.PayeePostalCode = debtClassRow.IsPostalCodeNull() ? null : debtClassRow.PostalCode;
				trustSettlementInfo.PaymentLength = consumerDebtNegotiationRow.OfferPaymentLength;
				trustSettlementInfo.TrustStatusId = matchRow.StatusId;

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
									trustSettlementInfo.PaymentMethods.Add(
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
					trustSettlementInfo.PaymentStartDate = CommonConversion.ToDateTime(
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

						trustSettlementInfo.SettlementAmount = Math.Round(
							consumerDebtNegotiationRow.AccountBalance * consumerDebtNegotiationRow.OfferSettlementValue, 2);
						break;

					case SettlementUnit.MarketValue:

						trustSettlementInfo.SettlementAmount = consumerDebtNegotiationRow.OfferSettlementValue;
						break;

					case SettlementUnit.Percent:

						trustSettlementInfo.SettlementAmount = Math.Round(
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
					trustSettlementInfo.AcceptedStatusId = acceptedStatusRow.StatusId;
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
					trustSettlementInfo.NewStatusId = newStatusRow.StatusId;
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
					trustSettlementInfo.OfferAcceptedStatusId = offerAcceptedStatusRow.StatusId;
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
				if (consumerTrustNegotiationRow != null)
					consumerTrustNegotiationRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				if (matchRow != null)
					matchRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				if (contraMatchRow != null)
					contraMatchRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				if (blotterRow != null)
					blotterRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				if (contraBlotterRow != null)
					contraBlotterRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				if (debtClassRow != null)
					debtClassRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				if (consumerDebtNegotiationRow != null)
					consumerDebtNegotiationRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				if (workingOrderRow != null)
					workingOrderRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				if (securityRow != null)
					securityRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				if (consumerTrustRow != null)
					consumerTrustRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				if (contraWorkingOrderRow != null)
					contraWorkingOrderRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				if (contraSecurityRow != null)
					contraSecurityRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				if (consumerDebtRow != null)
					consumerDebtRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				if (consumerRow != null)
					consumerRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				if (creditCardRow != null)
					creditCardRow.ReleaseReaderLock(dataModelTransaction.TransactionId);

			}

			// Nothing is done if this order has already been settled.
			if (trustSettlementInfo.TrustStatusId == trustSettlementInfo.AcceptedStatusId)
			{
				return;
			}

			// Nothing is done if this side has already accepted the negotiation and is waiting for the other side to respond.
			if (trustSettlementInfo.TrustStatusId == trustSettlementInfo.PendingStatusId)
			{
				return;
			}

			// Busines Rule #1: Don't allow a settlement if the payment methods are not compatible.
			if (trustSettlementInfo.PaymentMethods.Count == 0)
			{
				throw new FaultException<PaymentMethodFault>(new PaymentMethodFault("The negotiation doesn't contain compatible payment methods."));
			}

			// Busines Rule #2: Insure there is a Payee Address
			if (trustSettlementInfo.PayeeAddress1 == null)
			{
				throw new FaultException<ArgumentFault>(new ArgumentFault("The Payee Address was not provided."));
			}
			// Busines Rule #2: Insure there is a Payee City
			if (trustSettlementInfo.PayeeCity == null)
			{
				throw new FaultException<ArgumentFault>(new ArgumentFault("The Payee City was not provided."));
			}

			// Busines Rule #3: Insure there is a Payee Province
			if (trustSettlementInfo.PayeeProvinceId == null)
			{
				throw new FaultException<ArgumentFault>(new ArgumentFault("The Payee Province was not provided."));
			}

			// Busines Rule #4: Insure there is a Payee Company Name
			if (String.IsNullOrEmpty((String)trustSettlementInfo.PayeeCompanyName))
			{
				throw new FaultException<ArgumentFault>(new ArgumentFault("The Payee Company Name was not provided."));
			}

			// Busines Rule #5: Insure there is a Debtor City
			if (String.IsNullOrEmpty((String)trustSettlementInfo.DebtorAddress1))
			{
				throw new FaultException<ArgumentFault>(new ArgumentFault("The Debtor Address was not provided."));
			}

			// Busines Rule #5: Insure there is a Debtor City
			if (String.IsNullOrEmpty((String)trustSettlementInfo.DebtorCity))
			{
				throw new FaultException<ArgumentFault>(new ArgumentFault("The Debtor City was not provided."));
			}

			// Busines Rule #6: Insure there is a Debtor Province
			if (trustSettlementInfo.DebtorProvinceId == null)
			{
				throw new FaultException<ArgumentFault>(new ArgumentFault("The Debtor State was not provided."));
			}

			// Busines Rule #7: Insure there is a first or last name.
			if (String.IsNullOrEmpty((String)trustSettlementInfo.DebtorFirstName) && String.IsNullOrEmpty((String)trustSettlementInfo.DebtorLastName))
			{
				throw new FaultException<ArgumentFault>(new ArgumentFault("The Debtor Name was not provided."));
			}

			// Only when the Consumer Debt side is awaiting a settlement is the settlement generated.
			if (trustSettlementInfo.DebtStatusId == trustSettlementInfo.PendingStatusId)
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
					new Object[] { trustSettlementInfo.MatchId },
					trustSettlementInfo.MatchRowVersion,
					trustSettlementInfo.AcceptedStatusId,
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
					new Object[] { trustSettlementInfo.ContraMatchId },
					trustSettlementInfo.ContraMatchRowVersion,
					trustSettlementInfo.AcceptedStatusId,
					null);

				// This records the terms of the settlement between the Consumer Trust account and the Consumer Debt account.
				dataModel.CreateConsumerDebtSettlement(
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
					null,
					trustSettlementInfo.NewStatusId);

				// Each of the acceptable payment methods is also written as part of this transaction.
				foreach (PaymentMethodInfo paymentMethodInfo in trustSettlementInfo.PaymentMethods)
				{
					dataModel.CreateConsumerDebtSettlementPaymentMethod(
						trustSettlementInfo.BlotterId,
						trustSettlementInfo.ConsumerTrustSettlementId,
						paymentMethodInfo.ConsumerTrustSettlementPaymentMethodId,
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
					new Object[] { trustSettlementInfo.MatchId },
					trustSettlementInfo.MatchRowVersion,
					trustSettlementInfo.PendingStatusId,
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
					new Object[] { trustSettlementInfo.ContraMatchId },
					trustSettlementInfo.ContraMatchRowVersion,
					trustSettlementInfo.OfferAcceptedStatusId,
					null);




			}

		}

	}

}
