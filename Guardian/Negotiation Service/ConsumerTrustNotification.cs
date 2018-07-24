namespace FluidTrade.Guardian
{

	using System;
	using System.Linq;
	using System.ServiceModel;
	using System.Threading;
	using FluidTrade.Core;
	using FluidTrade.Guardian.TradingSupportReference;

	/// <summary>
	/// A class handling notification and auto-settlement of consumer trust matches.
	/// </summary>
	public class ConsumerTrustNotification
	{

		/// <summary>
		/// Create a new consumer debt notification handler.
		/// </summary>
		/// <param name="negotiationService">The negotiation service that </param>
		/// <param name="notificationInfo"></param>
		public ConsumerTrustNotification(NegotiationService negotiationService, NotificationInfo notificationInfo)
		{

			if (notificationInfo.Status == Status.ValidMatchFunds ||
				notificationInfo.Status == Status.ValidMatch)
			{

				//System.Diagnostics.Debug.WriteLine(String.Format("Attempting settlement of '{0}' from the trust side.", notificationInfo.MatchId));
				this.TryAutoSettle(notificationInfo);

			}
			else
			{

				//System.Diagnostics.Debug.WriteLine(String.Format("Trust side Match '{0}' is {1}.", notificationInfo.MatchId, notificationInfo.Status));

			}

		}

		private Boolean IsCashValueAcceptable(Decimal chargeOff, Decimal holderValue, SettlementUnit holderUnit, Decimal negotiatorValue, SettlementUnit negotiatorUnit)
		{

			Decimal holderDollarAmount;
			Decimal negotiatorDollarAmount;

			switch (holderUnit)
			{

				case SettlementUnit.MarketValue:
					holderDollarAmount = holderValue;
					break;
				case SettlementUnit.Percent:
					holderDollarAmount = chargeOff * holderValue;
					break;
				default:
					return false;

			}

			switch (negotiatorUnit)
			{

				case SettlementUnit.MarketValue:
					negotiatorDollarAmount = negotiatorValue;
					break;
				case SettlementUnit.Percent:
					negotiatorDollarAmount = chargeOff * negotiatorValue;
					break;
				default:
					return false;

			}

			return negotiatorDollarAmount >= holderDollarAmount;

		}

		private Boolean IsPaymentLengthAcceptable(Decimal holderLength, Decimal negotiatorLength)
		{

			return negotiatorLength <= holderLength;

		}

		private Boolean IsPaymentStartDateAcceptable(Decimal holderLength, Guid holderUnit, Decimal negotiatorLength, Guid negotiatorUnit)
		{

			TimeUnitRow holderTimeUnitRow = DataModel.TimeUnit.TimeUnitKey.Find(holderUnit);
			TimeUnitRow negotiatorTimeUnitRow = DataModel.TimeUnit.TimeUnitKey.Find(negotiatorUnit);
			Decimal holderTime = holderTimeUnitRow.InDays * holderLength;
			Decimal negotiatorTime = negotiatorTimeUnitRow.InDays * negotiatorLength;

			// Frankly, a unit less than a day is really strange, and auto-settling with it is likely both erroneous and dangerous.
			if (holderTimeUnitRow.TimeUnitCode == TimeUnit.Hours ||
				holderTimeUnitRow.TimeUnitCode == TimeUnit.Minutes ||
				holderTimeUnitRow.TimeUnitCode == TimeUnit.Seconds)
				return false;
			if (negotiatorTimeUnitRow.TimeUnitCode == TimeUnit.Hours ||
				negotiatorTimeUnitRow.TimeUnitCode == TimeUnit.Minutes ||
				negotiatorTimeUnitRow.TimeUnitCode == TimeUnit.Seconds)
				return false;

			return negotiatorTime <= holderTime;

		}

		private Boolean ArePaymentTypesAcceptable(
			ConsumerTrustNegotiationCounterPaymentMethodRow[] holderPaymentMethodRows,
			ConsumerTrustNegotiationOfferPaymentMethodRow[] negotiatorPaymentMethodRows)
		{

			foreach (ConsumerTrustNegotiationCounterPaymentMethodRow row in holderPaymentMethodRows)
			{

				if (negotiatorPaymentMethodRows.FirstOrDefault(r => r.PaymentMethodTypeId == row.PaymentMethodTypeId) != null)
					return true;

			}

			return false;

		}

		/// <summary>
		/// Determine whether we can automatically accept this negotiation and do so if possible.
		/// </summary>
		/// <param name="info">The notification information.</param>
		private void TryAutoSettle(NotificationInfo info)
		{

			Guid? negotiationId = null;

			lock (DataModel.SyncRoot)
			{

				MatchRow matchRow = DataModel.Match.MatchKey.Find(info.MatchId);
				DebtClassRow debtClassRow;
				Guardian.Windows.DebtRule debtRuleRow;

				if (matchRow == null)
				{

					EventLog.Information("Match ('{0}') has been deleted before it could be auto-settled from the trust side", info.MatchId);
					return;

				}

				debtClassRow = DataModel.DebtClass.DebtClassKey.Find(matchRow.WorkingOrderRow.BlotterId);

				if (debtClassRow == null)
				{

					EventLog.Warning(
						"The debt class that the match ('{0}') was in has been deleted before it could be auto-settled from the trust side",
						info.MatchId);
					return;

				}

				debtRuleRow =
					Guardian.Windows.DebtClass.GetDebtRule(debtClassRow.DebtClassId, debtClassRow.BlotterRow.EntityRow.TypeId);

				if (debtRuleRow == null)
				{

					EventLog.Error("Debt class ('{0}') or its parents does not have a debt rule", debtClassRow.DebtClassId);
					return;

				}

				if (debtRuleRow.IsAutoSettled)
				{

					ConsumerTrustNegotiationRow[] negotiationRows = matchRow.GetConsumerTrustNegotiationRows();
					ConsumerTrustNegotiationRow negotiationRow;

					// If, for whatever reason, there isn't exactly one negotiation row, log the error and bail.
					if (negotiationRows.Length < 1)
					{

						EventLog.Error("Consumer trust match ('{0}') has no negotiation rows", info.MatchId, negotiationRows.Length);
						return;

					}

					negotiationRow = negotiationRows[0];

					foreach (ConsumerTrustNegotiationRow row in negotiationRows)
						if (row.RowVersion > negotiationRow.RowVersion)
							negotiationRow = row;

					if (IsCashValueAcceptable(
							negotiationRow.AccountBalance,
							negotiationRow.CounterSettlementValue,
							DataModel.SettlementUnit.SettlementUnitKey.Find(negotiationRow.CounterSettlementUnitId).SettlementUnitCode,
							negotiationRow.OfferSettlementValue,
							DataModel.SettlementUnit.SettlementUnitKey.Find(debtRuleRow.SettlementUnitId).SettlementUnitCode) &&
						IsPaymentLengthAcceptable(
							negotiationRow.CounterPaymentLength,
							debtRuleRow.PaymentLength) &&
						IsPaymentStartDateAcceptable(
							negotiationRow.CounterPaymentStartDateLength,
							negotiationRow.CounterPaymentStartDateUnitId,
							negotiationRow.OfferPaymentStartDateLength,
							negotiationRow.OfferPaymentStartDateUnitId) &&
						ArePaymentTypesAcceptable(
							negotiationRow.GetConsumerTrustNegotiationCounterPaymentMethodRows(),
							negotiationRow.GetConsumerTrustNegotiationOfferPaymentMethodRows()))
					{

						negotiationId = negotiationRow.ConsumerTrustNegotiationId;

					}

				}

			}

			if (negotiationId != null)
				this.AcceptSettlement(negotiationId.Value);

		}

		/// <summary>
		/// Accept a settlement.
		/// </summary>
		/// <param name="negotiationId"></param>
		private void AcceptSettlement(Guid negotiationId)
		{

			TradingSupportClient tradingSupportClient = new TradingSupportClient(Guardian.Properties.Settings.Default.TradingSupportEndpoint);

			try
			{
				tradingSupportClient.CreateConsumerTrustSettlement(negotiationId);

			}
			catch (FaultException<ArgumentFault> exception)
			{

				EventLog.Information(
					"Server rejected auto-accept of consumer trust negotiation {0}: {1}: {2}\n {3}",
					negotiationId,
					exception.GetType(),
					exception.Detail.Message,
					exception.StackTrace);

			}
			catch (Exception exception)
			{

				System.Diagnostics.Debug.WriteLine(String.Format("Accepting negotiation {0} failed from trust side inexplicably.", negotiationId));
				EventLog.Error(
					"Failed to auto-settle consumer trust negotiation {0}: {1}: {2}\n {3}",
					negotiationId,
					exception.GetType(),
					exception.Message,
					exception.StackTrace);

			}
			finally
			{

				if (tradingSupportClient != null && tradingSupportClient.State == CommunicationState.Opened)
					tradingSupportClient.Close();
			}

			Thread.Sleep(5000);

		}

	}

}
