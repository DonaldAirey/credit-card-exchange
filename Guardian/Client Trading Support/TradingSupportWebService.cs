namespace FluidTrade.Guardian
{
	using System;
	using FluidTrade.Core;	
	using FluidTrade.Guardian.TradingSupportReference;
using System.Data;
	using System.Reflection;
	using System.ServiceModel;
	
	/// <summary>
    /// Base interface for rows
    /// </summary>
    /// <typeparam name="DataRowType"></typeparam>
    public  class TradingSupportWebService
    {

		/// <summary>
		/// Compare a record to its row.
		/// </summary>
		/// <param name="row">The row.</param>
		/// <param name="record">The record.</param>
		/// <returns>True if the record would update the row.</returns>
		private static Boolean ColumnChanged(DataRow row, BaseRecord record)
		{

			Type rowType = row.GetType();

			foreach (PropertyInfo property in record.GetType().GetProperties())
			{

				PropertyInfo rowProperty = rowType.GetProperty(property.Name);

				if (rowProperty != null)
				{

					object recordValue = property.GetValue(record, new object[0]);

					if (row.IsNull(property.Name))
					{

						if (recordValue != DBNull.Value && recordValue != null)
							return true;

					}
					else
					{

						object rowValue = rowProperty.GetValue(row, new object[0]);

						if (recordValue != null && !rowValue.Equals(recordValue))
							return true;

					}

				}

			}

			return false;

		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="consumer"></param>
		public static MethodResponseErrorCode UpdateConsumer(Consumer consumer)
		{
			if (consumer.RowId == Guid.Empty)
			{
				throw new MissingFieldException("RowId is not set");
			}

			lock (DataModel.SyncRoot)
			{

				ConsumerRow row = DataModel.Consumer.ConsumerKey.Find(consumer.RowId);

				if (TradingSupportWebService.ColumnChanged(row, consumer))
					return UpdateConsumer(new Consumer[] { consumer });
				else
					return null;

			}
			
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="consumers"></param>
		public static MethodResponseErrorCode UpdateConsumer(Consumer[] consumers)
		{
			MethodResponseErrorCode response = null;

			// Update the database.					
			TradingSupportClient tradingSupportClient = new TradingSupportClient(Guardian.Properties.Settings.Default.TradingSupportEndpoint);
			try
			{
				response = tradingSupportClient.UpdateConsumer(consumers);
			}
			catch (Exception exception)
			{
				// Any issues trying to communicate to the server are logged.
				EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);
			}
			finally
			{

				if (tradingSupportClient != null && tradingSupportClient.State == CommunicationState.Opened)
					tradingSupportClient.Close();
			}
			return response;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="consumerTrust"></param>
		/// <returns></returns>
		public static MethodResponseErrorCode UpdateConsumerDebt(ConsumerDebt consumerDebt)
		{
			if (consumerDebt.RowId == Guid.Empty)
			{
				throw new MissingFieldException("RowId is not set");
			}

			lock (DataModel.SyncRoot)
			{

				ConsumerDebtRow row = DataModel.ConsumerDebt.ConsumerDebtKey.Find(consumerDebt.RowId);

				if (TradingSupportWebService.ColumnChanged(row, consumerDebt))
					return UpdateConsumerDebt(new ConsumerDebt[] { consumerDebt });
				else
					return null;

			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="consumerTrusts"></param>
		/// <returns></returns>
		public static MethodResponseErrorCode UpdateConsumerDebt(ConsumerDebt[] consumerDebts)
		{
			MethodResponseErrorCode response = null;
			// Update the database.					
			TradingSupportClient tradingSupportClient = new TradingSupportClient(Guardian.Properties.Settings.Default.TradingSupportEndpoint);
			try
			{
				response = tradingSupportClient.UpdateConsumerDebt(consumerDebts);				
			}
			catch (Exception exception)
			{
				// Any issues trying to communicate to the server are logged.
				EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);
			}
			finally
			{

				if (tradingSupportClient != null && tradingSupportClient.State == CommunicationState.Opened)
					tradingSupportClient.Close();
			}
			return response;
		}

		/// <summary>
		/// Proxy to update consumerDebtPayments via TradingSupportClient.
		/// </summary>
		/// <param name="consumerDebt"></param>
		/// <returns></returns>
		public static MethodResponseErrorCode UpdateConsumerDebtPayment(ConsumerDebtPayment consumerDebtPayment)
		{
			if (consumerDebtPayment.RowId == Guid.Empty)
			{
				throw new MissingFieldException("RowId is not set");
			}

			lock (DataModel.SyncRoot)
			{

				ConsumerDebtPaymentRow row = DataModel.ConsumerDebtPayment.ConsumerDebtPaymentKey.Find(consumerDebtPayment.RowId);

				if (TradingSupportWebService.ColumnChanged(row, consumerDebtPayment))
					return UpdateConsumerDebtPayment(new ConsumerDebtPayment[] { consumerDebtPayment });
				else
					return null;

			}
		}

		/// <summary>
		/// Proxy to update consumerDebtPayments via TradingSupportClient. 
		/// </summary>
		/// <param name="consumerDebtPayments"></param>
		/// <returns></returns>
		public static MethodResponseErrorCode UpdateConsumerDebtPayment(ConsumerDebtPayment[] consumerDebtPayments)
		{
			MethodResponseErrorCode response = null;
			// Update the database.					
			TradingSupportClient tradingSupportClient = new TradingSupportClient(Guardian.Properties.Settings.Default.TradingSupportEndpoint);
			try
			{
				response = tradingSupportClient.UpdateConsumerDebtPayment(consumerDebtPayments);
			}
			catch (Exception exception)
			{
				// Any issues trying to communicate to the server are logged.
				EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);
			}
			finally
			{

				if (tradingSupportClient != null && tradingSupportClient.State == CommunicationState.Opened)
					tradingSupportClient.Close();
			}
			return response;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="consumerTrust"></param>
		/// <returns></returns>
		public static MethodResponseErrorCode UpdateConsumerTrust(ConsumerTrust consumerTrust)
		{
			if (consumerTrust.RowId == Guid.Empty)
			{
				throw new MissingFieldException("RowId is not set");
			}

			lock (DataModel.SyncRoot)
			{

				ConsumerTrustRow row = DataModel.ConsumerTrust.ConsumerTrustKey.Find(consumerTrust.RowId);

				if (TradingSupportWebService.ColumnChanged(row, consumerTrust))
					return UpdateConsumerTrust(new ConsumerTrust[] { consumerTrust });
				else
					return null;

			}

		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="consumerTrusts"></param>
		/// <returns></returns>
		public static MethodResponseErrorCode UpdateConsumerTrust(ConsumerTrust[] consumerTrusts)
		{
			MethodResponseErrorCode response = null;
			// Update the database.					
			TradingSupportClient tradingSupportClient = new TradingSupportClient(Guardian.Properties.Settings.Default.TradingSupportEndpoint);
			try
			{
				response = tradingSupportClient.UpdateConsumerTrust(consumerTrusts);				
			}
			catch (Exception exception)
			{
				// Any issues trying to communicate to the server are logged.
				EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);
			}
			finally
			{

				if (tradingSupportClient != null && tradingSupportClient.State == CommunicationState.Opened)
					tradingSupportClient.Close();
			}
			return response;
		}

		/// <summary>
		/// Proxy to update consumerTrustPayments via TradingSupportClient.
		/// </summary>
		/// <param name="consumerTrust"></param>
		/// <returns></returns>
		public static MethodResponseErrorCode UpdateConsumerTrustPayment(ConsumerTrustPayment consumerTrustPayment)
		{
			if (consumerTrustPayment.RowId == Guid.Empty)
			{
				throw new MissingFieldException("RowId is not set");
			}

			lock (DataModel.SyncRoot)
			{

				ConsumerTrustPaymentRow row = DataModel.ConsumerTrustPayment.ConsumerTrustPaymentKey.Find(consumerTrustPayment.RowId);

				if (TradingSupportWebService.ColumnChanged(row, consumerTrustPayment))
					return UpdateConsumerTrustPayment(new ConsumerTrustPayment[] { consumerTrustPayment });
				else
					return null;

			}

		}

	
		/// <summary>
		/// Proxy to update consumerTrustPayments via TradingSupportClient. 
		/// </summary>
		/// <param name="consumerTrustPayments"></param>
		/// <returns></returns>
		public static MethodResponseErrorCode UpdateConsumerTrustPayment(ConsumerTrustPayment[] consumerTrustPayments)
		{
			MethodResponseErrorCode response = null;
			// Update the database.					
			TradingSupportClient tradingSupportClient = new TradingSupportClient(Guardian.Properties.Settings.Default.TradingSupportEndpoint);
			try
			{
				response = tradingSupportClient.UpdateConsumerTrustPayment(consumerTrustPayments);
			}
			catch (Exception exception)
			{
				// Any issues trying to communicate to the server are logged.
				EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);
			}
			finally
			{

				if (tradingSupportClient != null && tradingSupportClient.State == CommunicationState.Opened)
					tradingSupportClient.Close();
			}
			return response;
		}


		public static MethodResponseErrorCode UpdateCreditCard(CreditCard creditcard)
		{
			MethodResponseErrorCode response = null;
			// Update the database.					
			TradingSupportClient tradingSupportClient = new TradingSupportClient(Guardian.Properties.Settings.Default.TradingSupportEndpoint);
			try
			{
				lock (DataModel.SyncRoot)
				{

					CreditCardRow row = DataModel.CreditCard.CreditCardKey.Find(creditcard.RowId);

					if (TradingSupportWebService.ColumnChanged(row, creditcard))
						response = tradingSupportClient.UpdateCreditCard(new CreditCard[] { creditcard });
					else
						response = null;

				}
				
			}
			catch (Exception exception)
			{
				// Any issues trying to communicate to the server are logged.
				EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);
			}
			finally
			{

				if (tradingSupportClient != null && tradingSupportClient.State == CommunicationState.Opened)
					tradingSupportClient.Close();
			}
			return response;

		}

		public static MethodResponseErrorCode UpdateEntity(Entity entity)
		{
			MethodResponseErrorCode response = null;
			// Update the database.					
			TradingSupportClient tradingSupportClient = new TradingSupportClient(Guardian.Properties.Settings.Default.TradingSupportEndpoint);
			try
			{
				lock (DataModel.SyncRoot)
				{

					EntityRow row = DataModel.Entity.EntityKey.Find(entity.RowId);

					if (TradingSupportWebService.ColumnChanged(row, entity))
						response = tradingSupportClient.UpdateEntity(new Entity[] { entity });				
					else
						response = null;

				}
			}
			catch (Exception exception)
			{
				// Any issues trying to communicate to the server are logged.
				EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);
			}
			finally
			{

				if (tradingSupportClient != null && tradingSupportClient.State == CommunicationState.Opened)
					tradingSupportClient.Close();
			}
			return response;

		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="consumer"></param>
		public static MethodResponseErrorCode UpdateNegotiaton(Negotiation negotiation)
		{
			if (negotiation.RowId == Guid.Empty)
			{
				throw new MissingFieldException("RowId is not set");
			}

			lock (DataModel.SyncRoot)
			{

				NegotiationRow row = DataModel.Negotiation.NegotiationKey.Find(negotiation.RowId);

				if (TradingSupportWebService.ColumnChanged(row, negotiation))
					return UpdateNegotiaton(new Negotiation[] { negotiation });
				else
					return null;

			}

		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="consumers"></param>
		public static MethodResponseErrorCode UpdateNegotiaton(Negotiation[] negotiations)
		{
			MethodResponseErrorCode response = null;

			// Update the database.					
			TradingSupportClient tradingSupportClient = new TradingSupportClient(Guardian.Properties.Settings.Default.TradingSupportEndpoint);
			try
			{
				response = tradingSupportClient.UpdateNegotiation(negotiations);
			}
			catch (Exception exception)
			{
				// Any issues trying to communicate to the server are logged.
				EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);
			}
			finally
			{

				if (tradingSupportClient != null && tradingSupportClient.State == CommunicationState.Opened)
					tradingSupportClient.Close();
			}
			return response;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="consumerTrust"></param>
		/// <returns></returns>
		public static MethodResponseErrorCode UpdateProvince(Province province)
		{
			if (province.RowId == Guid.Empty)
			{
				throw new MissingFieldException("RowId is not set");
			}

			return UpdateProvince(new Province[] { province });
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="consumerTrusts"></param>
		/// <returns></returns>
		public static MethodResponseErrorCode UpdateProvince(Province[] provinces)
		{
			MethodResponseErrorCode response = null;
			// Update the database.					
			TradingSupportClient tradingSupportClient = new TradingSupportClient(Guardian.Properties.Settings.Default.TradingSupportEndpoint);
			try
			{
				//response = tradingSupportClient.UpdateProvince(provinces);
				throw new NotImplementedException();
			}
			catch (Exception exception)
			{
				// Any issues trying to communicate to the server are logged.
				EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);
			}
			finally
			{

				if (tradingSupportClient != null && tradingSupportClient.State == CommunicationState.Opened)
					tradingSupportClient.Close();
			}
			return response;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="consumerTrust"></param>
		/// <returns></returns>
		public static MethodResponseErrorCode UpdateReport(Report report)
		{
			return UpdateReport(new Report[] { report });
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="consumerTrusts"></param>
		/// <returns></returns>
		public static MethodResponseErrorCode UpdateReport(Report[] reports)
		{
			MethodResponseErrorCode response = null;
			// Update the database.					
			TradingSupportClient tradingSupportClient = new TradingSupportClient(Guardian.Properties.Settings.Default.TradingSupportEndpoint);
			try
			{
				response = tradingSupportClient.UpdateReport(reports);
			}
			catch (Exception exception)
			{
				// Any issues trying to communicate to the server are logged.
				EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);
			}
			finally
			{

				if (tradingSupportClient != null && tradingSupportClient.State == CommunicationState.Opened)
					tradingSupportClient.Close();
			}
			return response;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="consumerTrust"></param>
		/// <returns></returns>
		public static MethodResponseArrayOfguid CreateReport(Report report)
		{
			return CreateReport(new Report[] { report });
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="consumerTrusts"></param>
		/// <returns></returns>
		public static MethodResponseArrayOfguid CreateReport(Report[] reports)
		{
			MethodResponseArrayOfguid response = null;
			// Update the database.					
			TradingSupportClient tradingSupportClient = new TradingSupportClient(Guardian.Properties.Settings.Default.TradingSupportEndpoint);
			try
			{
				response = tradingSupportClient.CreateReport(reports);
			}
			catch (Exception exception)
			{
				// Any issues trying to communicate to the server are logged.
				EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);
			}
			finally
			{

				if (tradingSupportClient != null && tradingSupportClient.State == CommunicationState.Opened)
					tradingSupportClient.Close();
			}
			return response;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="consumerTrust"></param>
		/// <returns></returns>
		public static MethodResponseErrorCode UpdateSecurity(Security security)
		{
			if (security.RowId == Guid.Empty)
			{
				throw new MissingFieldException("RowId is not set");
			}

			lock (DataModel.SyncRoot)
			{

				SecurityRow row = DataModel.Security.SecurityKey.Find(security.RowId);

				if (TradingSupportWebService.ColumnChanged(row, security))
					return UpdateSecurity(new Security[] { security });
				else
					return null;

			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="consumerTrusts"></param>
		/// <returns></returns>
		public static MethodResponseErrorCode UpdateSecurity(Security[] securities)
		{
			MethodResponseErrorCode response = null;
			// Update the database.					
			TradingSupportClient tradingSupportClient = new TradingSupportClient(Guardian.Properties.Settings.Default.TradingSupportEndpoint);
			try
			{
				//response = tradingSupportClient.UpdateSecurity(securities);
				throw new NotImplementedException();
			}
			catch (Exception exception)
			{
				// Any issues trying to communicate to the server are logged.
				EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);
			}
			finally
			{

				if (tradingSupportClient != null && tradingSupportClient.State == CommunicationState.Opened)
					tradingSupportClient.Close();
			}
			return response;
		}
		

		/// <summary>
		/// 
		/// </summary>
		/// <param name="workingOders"></param>
		public static MethodResponseErrorCode UpdateWorkingOrder(WorkingOrderRecord workingOrder)
		{
			 MethodResponseErrorCode response = null;
			// Update the database.					
			TradingSupportClient tradingSupportClient = new TradingSupportClient(Guardian.Properties.Settings.Default.TradingSupportEndpoint);

			try
			{
				lock (DataModel.SyncRoot)
				{

					WorkingOrderRow row = DataModel.WorkingOrder.WorkingOrderKey.Find(workingOrder.RowId);

					if (TradingSupportWebService.ColumnChanged(row, workingOrder))
						response = tradingSupportClient.UpdateWorkingOrder(new WorkingOrderRecord[] { workingOrder });
					else
						response = null;

				}
			}
			catch (Exception exception)
			{
				// Any issues trying to communicate to the server are logged.
				EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);
			}
			finally
			{

				if (tradingSupportClient != null && tradingSupportClient.State == CommunicationState.Opened)
					tradingSupportClient.Close();
			}
			return response;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="blotterId"></param>
		/// <param name="consumerTrust"></param>
		public static void MoveConsumerTrustToBlotter(Guid blotterId, BaseRecord[] consumerTrust)
		{
			// Update the database.					
			TradingSupportClient tradingSupportClient = new TradingSupportClient(Guardian.Properties.Settings.Default.TradingSupportEndpoint);

			try
			{
				MethodResponseErrorCode response = tradingSupportClient.MoveConsumerTrustToBlotter(blotterId, consumerTrust);
			}
			catch (Exception exception)
			{
				// Any issues trying to communicate to the server are logged.
				EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);
			}
			finally
			{

				if (tradingSupportClient != null && tradingSupportClient.State == CommunicationState.Opened)
					tradingSupportClient.Close();
			}
		}

		public static void MoveConsumerDebtToBlotter(Guid blotterId, BaseRecord[] consumerDebt)
		{
			// Update the database.					
			TradingSupportClient tradingSupportClient = new TradingSupportClient(Guardian.Properties.Settings.Default.TradingSupportEndpoint);

			try
			{
				MethodResponseErrorCode response = tradingSupportClient.MoveConsumerDebtToBlotter(blotterId, consumerDebt);
			}
			catch (Exception exception)
			{
				// Any issues trying to communicate to the server are logged.
				EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);
			}
			finally
			{

				if (tradingSupportClient != null && tradingSupportClient.State == CommunicationState.Opened)
					tradingSupportClient.Close();
			}
		}
	}
}
