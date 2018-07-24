namespace FluidTrade.Guardian
{

	using FluidTrade.Core;
	using FluidTrade.Guardian.Windows;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Data;
	using System.Windows.Input;
	using System.Windows.Threading;
	using System.Xml.Linq;
	using System.Threading;
	
	class EquityOrders
	{

		// Private Constants
		private const String DefaultDirectory = @"%USERPROFILE%\Documents\Visual Studio 2008\Projects\Fluid Trade\Guardian\Database\Unit Test";
		private const string DefaultFileExtension = "xml";

		/// <summary>
		/// Create working orders.
		/// </summary>
		/// <param name="sender">The generic thread initialization parameter.</param>
		public static void CreateOrders(object state)
		{

			GenerateTradeInfo generateTradeInfo = state as GenerateTradeInfo;

			// Create a random number generator with about as random a number as possible.
			Random random = new Random(DateTime.Now.TimeOfDay.Milliseconds);

			XDocument xDocument = new XDocument();

			lock (DataModel.SyncRoot)
			{

				// This is the selected blotter for the orders.
				BlotterRow blotterRow = DataModel.Blotter.BlotterKey.Find(generateTradeInfo.BlotterId);
				if (blotterRow == null)
					throw new Exception(String.Format("Blotter {0} not found", generateTradeInfo.BlotterId));

				// This is the current user creating the orders.
				UserRow userRow = DataModel.User.UserKey.Find(Information.UserId);
				if (userRow == null)
					throw new Exception("The current user isn't mapped to a database user.");

				CountryRow countryRow = DataModel.Country.CountryKeyExternalId0.Find("US");
				if (countryRow == null)
					throw new Exception("The country isn't part of the database.");

				//<script name="Automatically Generated Orders">
				XElement elementRoot = new XElement("script", new XAttribute("name", "Automatically Generated Orders"));
				xDocument.Add(elementRoot);

				//  <client name="DataModelClient" type="DataModelClient, Client Data Model" endpoint="TcpDataModelEndpoint" />
				elementRoot.Add(
					new XElement("client",
						new XAttribute("name", "DataModelClient"),
						new XAttribute("type", "DataModelClient, Client Data Model"),
						new XAttribute("endpoint", "TcpDataModelEndpoint")));

				for (int index = 0; index < generateTradeInfo.OrderCount; index++)
				{

					//  <transaction>
					XElement elementTransaction = new XElement("transaction");
					elementRoot.Add(elementTransaction);

					// Generate a working order identifier.
					Guid workingOrderId = Guid.NewGuid();

					// Generate the status of the new order.
					StatusRow statusRow = DataModel.Status.StatusKeyStatusCode.Find(Status.New);

					// Generate a random US Equity.
					SecurityRow securityRow = null;
					SecurityRow settlementCurrencyRow = null;
					while (true)
					{

						// Select a random equity.
						EquityRow equityRow = DataModel.Equity[random.Next(DataModel.Equity.Count - 1)];
						securityRow = equityRow.SecurityRowByFK_Security_Equity_EquityId;

						// Generate the settlement currency
						EntityRow usdEntityRow = DataModel.Entity.EntityKeyExternalId0.Find("USD");
						settlementCurrencyRow = DataModel.Security.SecurityKey.Find(usdEntityRow.EntityId);

						PriceRow priceRow = DataModel.Price.PriceKey.Find(securityRow.SecurityId, settlementCurrencyRow.SecurityId);
						if (priceRow != null)
							break;

					}

					// Generate the side for the order.
					Boolean isBuy = random.Next(2) == 0;
					Boolean isLong = random.Next(6) != 0;
					Side side = isBuy && isLong ? Side.Buy :
						!isBuy && isLong ? Side.Sell :
						isBuy && !isLong ? Side.BuyCover :
						Side.SellShort;
					SideRow sideRow = DataModel.Side.SideKeySideCode.Find(side);

					// Generate the time in force for this order.
					TimeInForce timeInForce = random.Next(6) == 0 ? TimeInForce.GoodTillCancel : TimeInForce.Day;
					TimeInForceRow timeInForceRow = DataModel.TimeInForce.TimeInForceKeyTimeInForceCode.Find(timeInForce);

					// Generate trade and settlement dates.
					DateTime tradeDate = DateTime.Now;
					DateTime settlementDate = DateTime.Now;
					for (int dayIndex = 0; dayIndex < 3; dayIndex++)
					{
						settlementDate += TimeSpan.FromDays(1.0);
						if (settlementDate.DayOfWeek == DayOfWeek.Saturday)
							settlementDate += TimeSpan.FromDays(1.0);
						if (settlementDate.DayOfWeek == DayOfWeek.Sunday)
							settlementDate += TimeSpan.FromDays(1.0);
					}

					// Generate matching selections.
					Boolean isBrokerMatch = random.Next(10) == 0;
					Boolean isHedgeMatch = random.Next(5) == 0;
					Boolean isInstitutionMatch = true;
					if (random.Next(5) == 0)
					{
						isBrokerMatch = true;
						isHedgeMatch = true;
						isInstitutionMatch = true;
					}
					if (random.Next(10) == 0)
					{
						isBrokerMatch = false;
						isHedgeMatch = false;
						isInstitutionMatch = false;
					}

					// Generate a submission type for crossing.
					Crossing crossing = Crossing.NeverMatch;
					CrossingRow crossingRow = DataModel.Crossing.CrossingKeyCrossingCode.Find(crossing);

					//    <method name="CreateWorkingOrderEx" client="DataModelClient">
					XElement elementWorkingOrder = new XElement(
						"method",
						new XAttribute("name", "CreateWorkingOrderEx"),
						new XAttribute("client", "DataModelClient"));
					elementTransaction.Add(elementWorkingOrder);

					//      <parameter name="blotterKey" value="TONY DE SILVA BLOTTER" />
					elementWorkingOrder.Add(
						new XElement("parameter",
							new XAttribute("name", "blotterKey"),
							new XAttribute("value", blotterRow.EntityRow.ExternalId0)));

					//      <parameter name="configurationId" value="US TICKER" />
					elementWorkingOrder.Add(
						new XElement("parameter",
							new XAttribute("name", "configurationId"),
							new XAttribute("value", "US TICKER")));

					//      <parameter name="createdTime" value="5/26/2006 11:57:19 AM" />
					elementWorkingOrder.Add(
						new XElement("parameter",
							new XAttribute("name", "createdTime"),
							new XAttribute("value", DateTime.Now.ToString("G"))));

					//      <parameter name="crossingKey" value="ALWAYS" />
					elementWorkingOrder.Add(
						new XElement("parameter",
							new XAttribute("name", "crossingKey"),
							new XAttribute("value", crossingRow.ExternalId0)));

					//      <parameter name="externalId0" value="{fed508fb-b2a9-44df-8aa9-760f43a5d768}" />
					elementWorkingOrder.Add(
						new XElement("parameter",
							new XAttribute("name", "externalId0"),
							new XAttribute("value", workingOrderId.ToString("B"))));

					//      <parameter name="isBrokerMatch" value="True" />
					elementWorkingOrder.Add(
						new XElement("parameter",
							new XAttribute("name", "isBrokerMatch"),
							new XAttribute("value", isBrokerMatch)));

					//      <parameter name="isHedgeMatch" value="True" />
					elementWorkingOrder.Add(
						new XElement("parameter",
							new XAttribute("name", "isHedgeMatch"),
							new XAttribute("value", isHedgeMatch)));

					//      <parameter name="isInstitutionMatch" value="True" />
					elementWorkingOrder.Add(
						new XElement("parameter",
							new XAttribute("name", "isInstitutionMatch"),
							new XAttribute("value", isInstitutionMatch)));

					//      <parameter name="modifiedTime" value="5/26/2006 11:57:19 AM" />
					elementWorkingOrder.Add(
						new XElement("parameter",
							new XAttribute("name", "modifiedTime"),
							new XAttribute("value", DateTime.Now.ToString("G"))));

					//      <parameter name="orderTypeKey" value="MKT" />
					elementWorkingOrder.Add(
						new XElement("parameter",
							new XAttribute("name", "orderTypeKey"),
							new XAttribute("value", "MKT")));

					//      <parameter name="securityKeyBySecurityId" value="LMT" />
					elementWorkingOrder.Add(
						new XElement("parameter",
							new XAttribute("name", "securityKeyBySecurityId"),
							new XAttribute("value", securityRow.EntityRow.ExternalId3)));

					//      <parameter name="securityKeyBySettlementId" value="USD" />
					elementWorkingOrder.Add(
						new XElement("parameter",
							new XAttribute("name", "securityKeyBySettlementId"),
							new XAttribute("value", settlementCurrencyRow.EntityRow.ExternalId0)));

					//		<parameter name="settlementDate" value="3/31/2008 10:00:00 AM" />
					elementWorkingOrder.Add(
						new XElement("parameter",
							new XAttribute("name", "settlementDate"),
							new XAttribute("value", settlementDate.ToString("G"))));

					//      <parameter name="sideKey" value="BUY" />
					elementWorkingOrder.Add(
						new XElement("parameter",
							new XAttribute("name", "sideKey"),
							new XAttribute("value", sideRow.ExternalId0)));

					//      <parameter name="statusKey" value="PARTIALFILL" />
					elementWorkingOrder.Add(
						new XElement("parameter",
							new XAttribute("name", "statusKey"),
							new XAttribute("value", statusRow.ExternalId0)));

					//      <parameter name="timeInForceKey" value="DAY" />
					elementWorkingOrder.Add(
						new XElement("parameter",
							new XAttribute("name", "timeInForceKey"),
							new XAttribute("value", timeInForceRow.ExternalId0)));

					//		<parameter name="tradeDate" value="3/28/2008 10:00:00 AM" />
					elementWorkingOrder.Add(
						new XElement("parameter",
							new XAttribute("name", "tradeDate"),
							new XAttribute("value", tradeDate.ToString("G"))));

					//      <parameter name="userKeyByCreatedUserId" value="TONY DE SILVA" />
					elementWorkingOrder.Add(
						new XElement("parameter",
							new XAttribute("name", "userKeyByCreatedUserId"),
							new XAttribute("value", userRow.EntityRow.ExternalId0)));

					//      <parameter name="userKeyByModifiedUserId" value="TONY DE SILVA" />
					elementWorkingOrder.Add(
						new XElement("parameter",
							new XAttribute("name", "userKeyByModifiedUserId"),
							new XAttribute("value", userRow.EntityRow.ExternalId0)));

					// Most working orders have only a single source order but occationally they are blocked together and 
					// allocated as a single ticket.
					Int32 sourceOrderCount = random.Next(6) == 0 ? random.Next(4) + 1 : 1;
					for (int sourceOrderIndex = 0; sourceOrderIndex < sourceOrderCount; sourceOrderIndex++)
					{

						// Generate the source order identifier.
						Guid sourceOrderId = Guid.NewGuid();

						// Generate the quantity of this order.
						Decimal orderedQuantity = Convert.ToDecimal(random.Next(1, 100)) * 100.0M;
						if (orderedQuantity == 0.0M)
							throw new Exception("The Quantity is zero!!");

						//    <method name="CreateSourceOrderEx" client="DataModelClient">
						XElement elementSourceOrder = new XElement(
							"method",
							new XAttribute("name", "CreateSourceOrderEx"),
							new XAttribute("client", "DataModelClient"));
						elementTransaction.Add(elementSourceOrder);

						//      <parameter name="blotterKey" value="TONY DE SILVA BLOTTER" />
						elementSourceOrder.Add(
							new XElement("parameter",
								new XAttribute("name", "blotterKey"),
								new XAttribute("value", blotterRow.EntityRow.ExternalId0)));

						//      <parameter name="configurationId" value="US TICKER" />
						elementSourceOrder.Add(
							new XElement("parameter",
								new XAttribute("name", "configurationId"),
								new XAttribute("value", "US TICKER")));

						//      <parameter name="createdTime" value="5/26/2006 11:57:19 AM" />
						elementSourceOrder.Add(
							new XElement("parameter",
								new XAttribute("name", "createdTime"),
								new XAttribute("value", DateTime.Now)));

						//      <parameter name="externalId0" value="{3d289495-9c66-4582-b50f-3548c8c260f1}" />
						elementSourceOrder.Add(
							new XElement("parameter",
								new XAttribute("name", "externalId0"),
								new XAttribute("value", sourceOrderId.ToString("B"))));

						//      <parameter name="modifiedTime" value="5/26/2006 11:57:19 AM" />
						elementSourceOrder.Add(
							new XElement("parameter",
								new XAttribute("name", "modifiedTime"),
								new XAttribute("value", DateTime.Now.ToString("G"))));

						//      <parameter name="orderedQuantity" value="4300.0000000" />
						elementSourceOrder.Add(
							new XElement("parameter",
								new XAttribute("name", "orderedQuantity"),
								new XAttribute("value", orderedQuantity)));

						//      <parameter name="orderTypeKey" value="MKT" />
						elementSourceOrder.Add(
							new XElement("parameter",
								new XAttribute("name", "orderTypeKey"),
								new XAttribute("value", "MKT")));

						//      <parameter name="securityKeyBySecurityId" value="LMT" />
						elementSourceOrder.Add(
							new XElement("parameter",
								new XAttribute("name", "securityKeyBySecurityId"),
								new XAttribute("value", securityRow.EntityRow.ExternalId3)));

						//      <parameter name="securityKeyBySettlementId" value="USD" />
						elementSourceOrder.Add(
							new XElement("parameter",
								new XAttribute("name", "securityKeyBySettlementId"),
								new XAttribute("value", settlementCurrencyRow.EntityRow.ExternalId0)));

						//		<parameter name="settlementDate" value="3/31/2008 10:00:00 AM" />
						elementSourceOrder.Add(
							new XElement("parameter",
								new XAttribute("name", "settlementDate"),
								new XAttribute("value", settlementDate)));

						//      <parameter name="sideKey" value="BUY" />
						elementSourceOrder.Add(
							new XElement("parameter",
								new XAttribute("name", "sideKey"),
								new XAttribute("value", sideRow.ExternalId0)));

						//      <parameter name="statusKey" value="PARTIALFILL" />
						elementSourceOrder.Add(
							new XElement("parameter",
								new XAttribute("name", "statusKey"),
								new XAttribute("value", statusRow.ExternalId0)));
						//      <parameter name="timeInForceKey" value="DAY" />
						elementSourceOrder.Add(
							new XElement("parameter",
								new XAttribute("name", "timeInForceKey"),
								new XAttribute("value", timeInForceRow.ExternalId0)));

						//		<parameter name="tradeDate" value="3/28/2008 10:00:00 AM" />
						elementSourceOrder.Add(
							new XElement("parameter",
								new XAttribute("name", "tradeDate"),
								new XAttribute("value", tradeDate.ToString("G"))));

						//      <parameter name="userKeyByCreatedUserId" value="TONY DE SILVA" />
						elementSourceOrder.Add(
							new XElement("parameter",
								new XAttribute("name", "userKeyByCreatedUserId"),
								new XAttribute("value", userRow.EntityRow.ExternalId0)));

						//      <parameter name="userKeyByModifiedUserId" value="TONY DE SILVA" />
						elementSourceOrder.Add(
							new XElement("parameter",
								new XAttribute("name", "userKeyByModifiedUserId"),
								new XAttribute("value", userRow.EntityRow.ExternalId0)));

						//      <parameter name="workingOrderKey" value="{fed508fb-b2a9-44df-8aa9-760f43a5d768}" />
						elementSourceOrder.Add(
							new XElement("parameter",
								new XAttribute("name", "workingOrderKey"),
								new XAttribute("value", workingOrderId.ToString("B"))));

					}

				}

				// Fill out the file name with a default directory and an extension if they are required before saving the
				// generated orders.
				String fileName = generateTradeInfo.FileName;
				if (!Path.IsPathRooted(fileName))
					fileName = Path.Combine(Environment.ExpandEnvironmentVariables(EquityOrders.DefaultDirectory), fileName);
				if (!Path.HasExtension(fileName))
					fileName = Path.ChangeExtension(fileName, EquityOrders.DefaultFileExtension);
				xDocument.Save(fileName);

			}

		}

	}

}
