namespace FluidTrade.Guardian
{

	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Threading;
	using FluidTrade.Core;
	using System.ServiceModel;

	/// <summary>A collection of filters that remove records from the stream of data returned to the client.</summary>
	public class DataModelFilters
	{

		/// <summary>
		/// Initializes the client filters on the data model.
		/// </summary>
		static DataModelFilters()
		{

			try
			{

				// Make sure all the tables are locked before populating them.
				DataModel.DataLock.EnterWriteLock();

				// The root Entity associated with the current user defines a hierarchy of Entities that can be returned to a client data model.
				DataModel.GetFilterContextHandler = DataModelFilters.GetFilterContext;

				// Install the handlers that find the owner of a given item.
				DataModel.AccessControl.GetContainerHandler = DataModelFilters.GetContainerAccessControl;
				DataModel.Blotter.GetContainerHandler = DataModelFilters.GetContainerBlotter;
				DataModel.BlotterConfiguration.GetContainerHandler = DataModelFilters.GetContainerBlotterConfiguration;
				DataModel.Branch.GetContainerHandler = DataModelFilters.GetContainerBranch;
				DataModel.Broker.GetContainerHandler = DataModelFilters.GetContainerBroker;				
				DataModel.Chat.GetContainerHandler = DataModelFilters.GetChat;
				DataModel.ClearingBroker.GetContainerHandler = DataModelFilters.GetContainerClearingBroker;
				DataModel.Consumer.GetContainerHandler = DataModelFilters.GetContainerConsumer;
				DataModel.ConsumerDebt.GetContainerHandler = DataModelFilters.GetContainerConsumerDebt;
				DataModel.ConsumerTrust.GetContainerHandler = DataModelFilters.GetContainerConsumerTrust;
				DataModel.ConsumerDebtNegotiation.GetContainerHandler = DataModelFilters.GetContainerConsumerDebtNegotiation;
				DataModel.ConsumerDebtNegotiationCounterPaymentMethod.GetContainerHandler = DataModelFilters.GetContainerConsumerDebtNegotiationCounterPaymentMethod;
				DataModel.ConsumerDebtNegotiationOfferPaymentMethod.GetContainerHandler = DataModelFilters.GetContainerConsumerDebtNegotiationOfferPaymentMethod;
				DataModel.ConsumerDebtNegotiationOfferPaymentMethod.GetContainerHandler = DataModelFilters.GetContainerConsumerDebtNegotiationOfferPaymentMethod;
				DataModel.ConsumerDebtPayment.GetContainerHandler = DataModelFilters.GetContainerConsumerDebtPayment;
				DataModel.ConsumerDebtSettlement.GetContainerHandler = DataModelFilters.GetContainerConsumerDebtSettlement;
				DataModel.ConsumerDebtSettlementPaymentMethod.GetContainerHandler = DataModelFilters.GetContainerConsumerDebtSettlementPaymentMethod;
				DataModel.ConsumerTrustNegotiation.GetContainerHandler = DataModelFilters.GetContainerConsumerTrustNegotiation;
				DataModel.ConsumerTrustNegotiationCounterPaymentMethod.GetContainerHandler = DataModelFilters.GetContainerConsumerTrustNegotiationCounterPaymentMethod;
				DataModel.ConsumerTrustNegotiationOfferPaymentMethod.GetContainerHandler = DataModelFilters.GetContainerConsumerTrustNegotiationOfferPaymentMethod;
				DataModel.ConsumerTrustPayment.GetContainerHandler = DataModelFilters.GetContainerConsumerTrustPayment;
				DataModel.ConsumerTrustSettlement.GetContainerHandler = DataModelFilters.GetContainerConsumerTrustSettlement;
				DataModel.ConsumerTrustSettlementPaymentMethod.GetContainerHandler = DataModelFilters.GetContainerConsumerTrustSettlementPaymentMethod;
				DataModel.ComplianceOfficer.GetContainerHandler = DataModelFilters.GetContainerComplianceOfficer;
				DataModel.CreditCard.GetContainerHandler = DataModelFilters.GetContainerCreditCard;
				DataModel.DebtBlotter.GetContainerHandler = DataModelFilters.GetContainerDebtBlotter;
				DataModel.DebtClass.GetContainerHandler = DataModelFilters.GetContainerDebtClass;
				DataModel.DebtHolder.GetContainerHandler = DataModelFilters.GetContainerDebtHolder;
				DataModel.DebtNegotiator.GetContainerHandler = DataModelFilters.GetContainerDebtNegotiator;
				DataModel.DebtRuleMap.GetContainerHandler = DataModelFilters.GetContainerDebtRuleMap;				
				DataModel.DestinationOrder.GetContainerHandler = DataModelFilters.GetContainerDestinationOrder;
				DataModel.Entity.GetContainerHandler = DataModelFilters.GetContainerEntity;
				DataModel.EntityTree.GetContainerHandler = DataModelFilters.GetContainerEntityTree;
				DataModel.EquityBlotter.GetContainerHandler = DataModelFilters.GetContainerEquityBlotter;
				DataModel.Execution.GetContainerHandler = DataModelFilters.GetContainerExecution;
				DataModel.Folder.GetContainerHandler = DataModelFilters.GetContainerFolder;				
				DataModel.Group.GetContainerHandler = DataModelFilters.GetContainerGroup;
				DataModel.GroupUsers.GetContainerHandler = DataModelFilters.GetContainerGroupUsers;
				DataModel.Institution.GetContainerHandler = DataModelFilters.GetContainerInstitution;
				DataModel.Match.GetContainerHandler = DataModelFilters.GetContainerMatch;
				DataModel.Negotiation.GetContainerHandler = DataModelFilters.GetContainerNegotiation;
				DataModel.RightsHolder.GetContainerHandler = DataModelFilters.GetContainerRightsHolder;
				DataModel.Security.GetContainerHandler = DataModelFilters.GetContainerSecurity;
				DataModel.Tenant.GetContainerHandler = DataModelFilters.GetContainerOrganization;
				DataModel.TenantTree.GetContainerHandler = DataModelFilters.GetContainerTenantTree;				
				DataModel.Trader.GetContainerHandler = DataModelFilters.GetContainerTrader;				
				DataModel.Source.GetContainerHandler = DataModelFilters.GetContainerSource;				
				DataModel.SourceOrder.GetContainerHandler = DataModelFilters.GetContainerSourceOrder;
				DataModel.SystemFolder.GetContainerHandler = DataModelFilters.GetContainerSystemFolder;
				DataModel.User.GetContainerHandler = DataModelFilters.GetContainerUser;
				DataModel.WorkingOrder.GetContainerHandler = DataModelFilters.GetContainerWorkingOrder;
	
				// Install the filters
				DataModel.AccessControl.FilterRowHandler = DataModelFilters.FilterTenantContainers;
				DataModel.Blotter.FilterRowHandler = DataModelFilters.FilterContainers;
				DataModel.BlotterConfiguration.FilterRowHandler = DataModelFilters.FilterContainers;				
				DataModel.Branch.FilterRowHandler = DataModelFilters.FilterContainers;
				DataModel.Broker.FilterRowHandler = DataModelFilters.FilterContainers;
				DataModel.Chat.FilterRowHandler = DataModelFilters.FilterContainers;
				DataModel.ClearingBroker.FilterRowHandler = DataModelFilters.FilterContainers;
				DataModel.Consumer.FilterRowHandler = DataModelFilters.FilterConsumerContainers;
				DataModel.ConsumerDebt.FilterRowHandler = DataModelFilters.FilterTenantContainers;
				DataModel.ConsumerTrust.FilterRowHandler = DataModelFilters.FilterTenantContainers;
				DataModel.ConsumerDebtNegotiation.FilterRowHandler = DataModelFilters.FilterContainers;
				DataModel.ConsumerDebtNegotiationCounterPaymentMethod.FilterRowHandler = DataModelFilters.FilterContainers;
				DataModel.ConsumerDebtNegotiationOfferPaymentMethod.FilterRowHandler = DataModelFilters.FilterContainers;
				DataModel.ConsumerDebtPayment.FilterRowHandler = DataModelFilters.FilterContainers;
				DataModel.ConsumerDebtSettlement.FilterRowHandler = DataModelFilters.FilterContainers;
				DataModel.ConsumerDebtSettlementPaymentMethod.FilterRowHandler = DataModelFilters.FilterContainers;
				DataModel.ConsumerTrustNegotiation.FilterRowHandler = DataModelFilters.FilterContainers;
				DataModel.ConsumerTrustNegotiationCounterPaymentMethod.FilterRowHandler = DataModelFilters.FilterContainers;
				DataModel.ConsumerTrustNegotiationOfferPaymentMethod.FilterRowHandler = DataModelFilters.FilterContainers;
				DataModel.ConsumerTrustPayment.FilterRowHandler = DataModelFilters.FilterContainers;
				DataModel.ConsumerTrustSettlement.FilterRowHandler = DataModelFilters.FilterContainers;
				DataModel.ConsumerTrustSettlementPaymentMethod.FilterRowHandler = DataModelFilters.FilterContainers;
				DataModel.ComplianceOfficer.FilterRowHandler = DataModelFilters.FilterTenantContainers;
				DataModel.CreditCard.FilterRowHandler = DataModelFilters.FilterTenantContainers;
				DataModel.DebtBlotter.FilterRowHandler = DataModelFilters.FilterContainers;
				DataModel.DebtClass.FilterRowHandler = DataModelFilters.FilterContainers;
				DataModel.DebtHolder.FilterRowHandler = DataModelFilters.FilterContainers;
				DataModel.DebtNegotiator.FilterRowHandler = DataModelFilters.FilterContainers;
				DataModel.DebtRuleMap.FilterRowHandler = DataModelFilters.FilterContainers;
				DataModel.DestinationOrder.FilterRowHandler = DataModelFilters.FilterContainers;
				DataModel.Entity.FilterRowHandler = DataModelFilters.FilterTenantContainers;
				DataModel.EntityTree.FilterRowHandler = DataModelFilters.FilterBrowsableContainers;
				DataModel.EquityBlotter.FilterRowHandler = DataModelFilters.FilterContainers;
				DataModel.Execution.FilterRowHandler = DataModelFilters.FilterContainers;
				DataModel.Folder.FilterRowHandler = DataModelFilters.FilterTenantContainers;
				DataModel.Group.FilterRowHandler = DataModelFilters.FilterTenantContainers;
				DataModel.GroupUsers.FilterRowHandler = DataModelFilters.FilterTenantContainers;
				DataModel.Institution.FilterRowHandler = DataModelFilters.FilterTenantContainers;
				DataModel.Match.FilterRowHandler = DataModelFilters.FilterContainers;
				DataModel.Negotiation.FilterRowHandler = DataModelFilters.FilterContainers;
				DataModel.RightsHolder.FilterRowHandler = DataModelFilters.FilterTenantContainers;
				DataModel.Security.FilterRowHandler = DataModelFilters.FilterTenantContainers;
				DataModel.Tenant.FilterRowHandler = DataModelFilters.FilterTenantContainers;
				DataModel.TenantTree.FilterRowHandler = DataModelFilters.FilterTenantContainers;
				DataModel.Trader.FilterRowHandler = DataModelFilters.FilterTenantContainers;
				DataModel.Source.FilterRowHandler = DataModelFilters.FilterContainers;
				DataModel.SourceOrder.FilterRowHandler = DataModelFilters.FilterContainers;
				DataModel.SystemFolder.FilterRowHandler = DataModelFilters.FilterTenantContainers;
				DataModel.User.FilterRowHandler = DataModelFilters.FilterTenantContainers;
				DataModel.WorkingOrder.FilterRowHandler = DataModelFilters.FilterContainers;									
			}
			finally
			{

				// Release all of the table locks.
				DataModel.DataLock.ExitWriteLock();

			}

		}


		/// <summary>
		/// Gets a context for the Filtered Read operation.
		/// </summary>
		/// <returns>The root node of a family of Entities that belong to the current user.</returns>
		public static Object GetFilterContext(DataModelTransaction dataModelTransaction)
		{

			// The current security principal is used to find the root Entity.  All records that are descendants of this record can be returned to the client
			// using the filtering methods.  Conversely, all records that don't fall into this hierarchy are hidden from the user when the filters are
			// enabled.  Note that it is safe to access the original version of the record as the records can't be updated until a table lock is acquired.
			Guid userId = Guid.Empty;
			UserRow userRow = DataModel.User.UserKeyIdentityName.Find(Thread.CurrentPrincipal.Identity.Name.ToLower());
			if (userRow != null)
			{
				userRow.AcquireReaderLock(dataModelTransaction);
				userId = userRow.UserId;
			}
			return userId;

		}

		/// <summary>
		/// Gets the container for the given Blotter.
		/// </summary>
		/// <param name="iRow">A Blotter row to be examined for a container.</param>
		/// <returns>The owner Entity that is used to determine if this row should be filtered.</returns>		
		public static Object GetContainerAccessControl(IRow iRow)
		{

			// Return the Tenant to which this row belongs.  The tenantId is then used in the filtering logic.
			DataRowVersion dataRowVersion = iRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;
			return iRow[DataModel.AccessControl.TenantIdColumn, dataRowVersion];			
		}
		
		/// <summary>
		/// Gets the container for the given Blotter.
		/// </summary>
		/// <param name="iRow">A Blotter row to be examined for a container.</param>
		/// <returns>The owner Entity that is used to determine if this row should be filtered.</returns>
		public static Object GetContainerBlotter(IRow iRow)
		{

			// Return the blotter to which this row belongs.  The blotter is then used in the filtering logic.
			DataRowVersion dataRowVersion = iRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;
			return iRow[DataModel.Blotter.BlotterIdColumn, dataRowVersion];

		}
				
		/// <summary>
		/// Gets the container for the given BlotterConfiguration.
		/// </summary>
		/// <param name="iRow">A Blotter row to be examined for a container.</param>
		/// <returns>The owner Entity that is used to determine if this row should be filtered.</returns>
		public static Object GetContainerBlotterConfiguration(IRow iRow)
		{

			// Return the blotter to which this row belongs.  The blotter is then used in the filtering logic.
			DataRowVersion dataRowVersion = iRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;
			return iRow[DataModel.BlotterConfiguration.BlotterIdColumn, dataRowVersion];

		}
		
		/// <summary>
		/// Gets the container for the given Branch.
		/// </summary>
		/// <param name="iRow">A Branch row to be examined for a container.</param>
		/// <returns>The owner Entity that is used to determine if this row should be filtered.</returns>
		public static Object GetContainerBranch(IRow iRow)
		{

			// Return the blotter to which this row belongs.  The blotter is then used in the filtering logic.
			DataRowVersion dataRowVersion = iRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;
			return iRow[DataModel.Branch.BranchIdColumn, dataRowVersion];

		}

		
		/// <summary>
		/// Gets the container for the given sell side Broker.
		/// </summary>
		/// <param name="iRow">A Broker row to be examined for a container.</param>
		/// <returns>The owner Entity that is used to determine if this row should be filtered.</returns>
		public static Object GetContainerBroker(IRow iRow)
		{

			// Return the blotter to which this row belongs.  The blotter is then used in the filtering logic.
			DataRowVersion dataRowVersion = iRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;
			return iRow[DataModel.Broker.BrokerIdColumn, dataRowVersion];

		}

		/// <summary>
		/// Gets the container for the given Chat.
		/// </summary>
		/// <param name="iRow">A Chat row to be examined for a container.</param>
		/// <returns>The owner Entity that is used to determine if this row should be filtered.</returns>
		public static Object GetChat(IRow iRow)
		{

			// Return the blotter to which this row belongs.  The blotter is then used in the filtering logic.
			DataRowVersion dataRowVersion = iRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;
			return iRow[DataModel.Chat.BlotterIdColumn, dataRowVersion];

		}

		
		/// <summary>
		/// Gets the container for the given ClearingBroker.
		/// </summary>
		/// <param name="iRow">A ClearingBroker row to be examined for a container.</param>
		/// <returns>The owner Entity that is used to determine if this row should be filtered.</returns>
		public static Object GetContainerClearingBroker(IRow iRow)
		{

			// Return the blotter to which this row belongs.  The blotter is then used in the filtering logic.
			DataRowVersion dataRowVersion = iRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;
			return iRow[DataModel.ClearingBroker.ClearingBrokerIdColumn, dataRowVersion];
		
		}
	
		/// <summary>
		/// Gets the container for the given ConsumerDebt.
		/// </summary>
		/// <param name="iRow">An Execution row to be examined for a container.</param>
		/// <returns>The owner Entity that is used to determine if this row should be filtered.</returns>
		public static Object GetContainerConsumer(IRow iRow)
		{
			// Return the Consumer to which this row belongs.  The Consumer is then used in the filtering logic.
			DataRowVersion dataRowVersion = iRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;
			return iRow[DataModel.Consumer.ConsumerIdColumn, dataRowVersion];
		
		}
		
		
		/// <summary>
		/// Gets the container for the given ConsumerDebt.
		/// </summary>
		/// <param name="iRow">An Execution row to be examined for a container.</param>
		/// <returns>The owner Entity that is used to determine if this row should be filtered.</returns>
		public static Object GetContainerConsumerDebt(IRow iRow)
		{

			// Return the ConsumerDebt to which this row belongs.  The ConsumerDebt is then used in the filtering logic.
			DataRowVersion dataRowVersion = iRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;
			return iRow[DataModel.ConsumerDebt.TenantIdColumn, dataRowVersion];
			
		}
		
		/// <summary>
		/// Gets the container for the given ConsumerTrust.
		/// </summary>
		/// <param name="iRow">An Execution row to be examined for a container.</param>
		/// <returns>The owner Entity that is used to determine if this row should be filtered.</returns>
		public static Object GetContainerConsumerTrust(IRow iRow)
		{
			// Return the ConsumerDebt to which this row belongs.  The ConsumerDebt is then used in the filtering logic.
			DataRowVersion dataRowVersion = iRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;			
			return iRow[DataModel.ConsumerTrust.TenantIdColumn, dataRowVersion];	
		}

		/// <summary>
		/// Gets the container for the given ConsumerDebtNegotiation.
		/// </summary>
		/// <param name="iRow">A ConsumerDebtNegotiation row to be examined for a container.</param>
		/// <returns>The owner Entity that is used to determine if this row should be filtered.</returns>
		public static Object GetContainerConsumerDebtNegotiation(IRow iRow)
		{

			// Return the blotter to which this row belongs.  The blotter is then used in the filtering logic.
			DataRowVersion dataRowVersion = iRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;
			return iRow[DataModel.ConsumerDebtNegotiation.BlotterIdColumn, dataRowVersion];

		}

		/// <summary>
		/// Gets the container for the given ConsumerDebtNegotiationCounterPaymentMethod.
		/// </summary>
		/// <param name="iRow">A ConsumerDebtNegotiationCounterPaymentMethod row to be examined for a container.</param>
		/// <returns>The owner Entity that is used to determine if this row should be filtered.</returns>
		public static Object GetContainerConsumerDebtNegotiationCounterPaymentMethod(IRow iRow)
		{

			// Return the blotter to which this row belongs.  The blotter is then used in the filtering logic.
			DataRowVersion dataRowVersion = iRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;
			return iRow[DataModel.ConsumerDebtNegotiationCounterPaymentMethod.BlotterIdColumn, dataRowVersion];

		}

		/// <summary>
		/// Gets the container for the given ConsumerDebtNegotiationOfferPaymentMethod.
		/// </summary>
		/// <param name="iRow">A ConsumerDebtNegotiationOfferPaymentMethod row to be examined for a container.</param>
		/// <returns>The owner Entity that is used to determine if this row should be filtered.</returns>
		public static Object GetContainerConsumerDebtNegotiationOfferPaymentMethod(IRow iRow)
		{

			// Return the blotter to which this row belongs.  The blotter is then used in the filtering logic.
			DataRowVersion dataRowVersion = iRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;
			return iRow[DataModel.ConsumerDebtNegotiationOfferPaymentMethod.BlotterIdColumn, dataRowVersion];

		}

		/// <summary>
		/// Gets the container for the given ConsumerDebtSettlementPaymentMethod.
		/// </summary>
		/// <param name="iRow">A ConsumerDebtSettlementPaymentMethod row to be examined for a container.</param>
		/// <returns>The owner Entity that is used to determine if this row should be filtered.</returns>
		public static Object GetContainerConsumerDebtPayment(IRow iRow)
		{

			// Return the blotter to which this row belongs.  The blotter is then used in the filtering logic.
			DataRowVersion dataRowVersion = iRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;
			return iRow[DataModel.ConsumerDebtPayment.BlotterIdColumn, dataRowVersion];

		}

	
		/// <summary>
		/// Gets the container for the given ConsumerDebtSettlement.
		/// </summary>
		/// <param name="iRow">A ConsumerDebtSettlement row to be examined for a container.</param>
		/// <returns>The owner Entity that is used to determine if this row should be filtered.</returns>
		public static Object GetContainerConsumerDebtSettlement(IRow iRow)
		{

			// Return the blotter to which this row belongs.  The blotter is then used in the filtering logic.
			DataRowVersion dataRowVersion = iRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;
			return iRow[DataModel.ConsumerDebtSettlement.BlotterIdColumn, dataRowVersion];

		}

		/// <summary>
		/// Gets the container for the given ConsumerDebtSettlementPaymentMethod.
		/// </summary>
		/// <param name="iRow">A ConsumerDebtSettlementPaymentMethod row to be examined for a container.</param>
		/// <returns>The owner Entity that is used to determine if this row should be filtered.</returns>
		public static Object GetContainerConsumerDebtSettlementPaymentMethod(IRow iRow)
		{

			// Return the blotter to which this row belongs.  The blotter is then used in the filtering logic.
			DataRowVersion dataRowVersion = iRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;
			return iRow[DataModel.ConsumerDebtSettlementPaymentMethod.BlotterIdColumn, dataRowVersion];

		}


		/// <summary>
		/// Gets the container for the given ConsumerDebtSettlementPaymentMethod.
		/// </summary>
		/// <param name="iRow">A ConsumerDebtSettlementPaymentMethod row to be examined for a container.</param>
		/// <returns>The owner Entity that is used to determine if this row should be filtered.</returns>
		public static Object GetContainerConsumerTrustPayment(IRow iRow)
		{

			// Return the blotter to which this row belongs.  The blotter is then used in the filtering logic.
			DataRowVersion dataRowVersion = iRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;
			return iRow[DataModel.ConsumerTrustPayment.BlotterIdColumn, dataRowVersion];

		}

	
		/// <summary>
		/// Gets the container for the given ConsumerTrustNegotiation.
		/// </summary>
		/// <param name="iRow">A ConsumerTrustNegotiation row to be examined for a container.</param>
		/// <returns>The owner Entity that is used to determine if this row should be filtered.</returns>
		public static Object GetContainerConsumerTrustNegotiation(IRow iRow)
		{

			// Return the blotter to which this row belongs.  The blotter is then used in the filtering logic.
			DataRowVersion dataRowVersion = iRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;
			return iRow[DataModel.ConsumerTrustNegotiation.BlotterIdColumn, dataRowVersion];

		}

		/// <summary>
		/// Gets the container for the given ConsumerTrustNegotiationCounterPaymentMethod.
		/// </summary>
		/// <param name="iRow">A ConsumerTrustNegotiationCounterPaymentMethod row to be examined for a container.</param>
		/// <returns>The owner Entity that is used to determine if this row should be filtered.</returns>
		public static Object GetContainerConsumerTrustNegotiationCounterPaymentMethod(IRow iRow)
		{

			// Return the blotter to which this row belongs.  The blotter is then used in the filtering logic.
			DataRowVersion dataRowVersion = iRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;
			return iRow[DataModel.ConsumerTrustNegotiationCounterPaymentMethod.BlotterIdColumn, dataRowVersion];

		}

		/// <summary>
		/// Gets the container for the given ConsumerTrustNegotiationOfferPaymentMethod.
		/// </summary>
		/// <param name="iRow">A ConsumerTrustNegotiationOfferPaymentMethod row to be examined for a container.</param>
		/// <returns>The owner Entity that is used to determine if this row should be filtered.</returns>
		public static Object GetContainerConsumerTrustNegotiationOfferPaymentMethod(IRow iRow)
		{

			// Return the blotter to which this row belongs.  The blotter is then used in the filtering logic.
			DataRowVersion dataRowVersion = iRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;
			return iRow[DataModel.ConsumerTrustNegotiationOfferPaymentMethod.BlotterIdColumn, dataRowVersion];

		}

		/// <summary>
		/// Gets the container for the given ConsumerTrustSettlement.
		/// </summary>
		/// <param name="iRow">A ConsumerTrustSettlement row to be examined for a container.</param>
		/// <returns>The owner Entity that is used to determine if this row should be filtered.</returns>
		public static Object GetContainerConsumerTrustSettlement(IRow iRow)
		{

			// Return the blotter to which this row belongs.  The blotter is then used in the filtering logic.
			DataRowVersion dataRowVersion = iRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;
			return iRow[DataModel.ConsumerTrustSettlement.BlotterIdColumn, dataRowVersion];

		}

		/// <summary>
		/// Gets the container for the given ConsumerTrustSettlementPaymentMethod.
		/// </summary>
		/// <param name="iRow">A ConsumerTrustSettlementPaymentMethod row to be examined for a container.</param>
		/// <returns>The owner Entity that is used to determine if this row should be filtered.</returns>
		public static Object GetContainerConsumerTrustSettlementPaymentMethod(IRow iRow)
		{

			// Return the blotter to which this row belongs.  The blotter is then used in the filtering logic.
			DataRowVersion dataRowVersion = iRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;
			return iRow[DataModel.ConsumerTrustSettlementPaymentMethod.BlotterIdColumn, dataRowVersion];

		}

		/// <summary>
		/// Gets the container for the given DebtBlotter.
		/// </summary>
		/// <param name="iRow">An Execution row to be examined for a container.</param>
		/// <returns>The owner Entity that is used to determine if this row should be filtered.</returns>
		public static Object GetContainerComplianceOfficer(IRow iRow)
		{

			// Return the blotter to which this row belongs.  The blotter is then used in the filtering logic.
			DataRowVersion dataRowVersion = iRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;
			return iRow[DataModel.ComplianceOfficer.TenantIdColumn, dataRowVersion];			
		}

		/// <summary>
		/// Gets the container for the given CreditCard
		/// </summary>
		/// <param name="iRow">An Execution row to be examined for a container.</param>
		/// <returns>The owner Entity that is used to determine if this row should be filtered.</returns>
		public static Object GetContainerCreditCard(IRow iRow)
		{

			// Return the blotter to which this row belongs.  The blotter is then used in the filtering logic.
			DataRowVersion dataRowVersion = iRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;
			return iRow[DataModel.CreditCard.TenantIdColumn, dataRowVersion];

		}

		/// <summary>
		/// Gets the container for the given DebtBlotter.
		/// </summary>
		/// <param name="iRow">An Execution row to be examined for a container.</param>
		/// <returns>The owner Entity that is used to determine if this row should be filtered.</returns>
		public static Object GetContainerDebtBlotter(IRow iRow)
		{

			// Return the blotter to which this row belongs.  The blotter is then used in the filtering logic.
			DataRowVersion dataRowVersion = iRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;
			return iRow[DataModel.DebtBlotter.DebtBlotterIdColumn, dataRowVersion];

		}

		/// <summary>
		/// Gets the container for the given Debt Class.
		/// </summary>
		/// <param name="iRow">A Destination Order row to be examined for a container.</param>
		/// <returns>The owner Entity that is used to determine if this row should be filtered.</returns>
		public static Object GetContainerDebtClass(IRow iRow)
		{

			// Return the blotter to which this row belongs.  The blotter is then used in the filtering logic.
			DataRowVersion dataRowVersion = iRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;
			return iRow[DataModel.DebtClass.DebtClassIdColumn, dataRowVersion];

		}
			
		/// <summary>
		/// Gets the container for the given Destination Order.
		/// </summary>
		/// <param name="iRow">A Destination Order row to be examined for a container.</param>
		/// <returns>The owner Entity that is used to determine if this row should be filtered.</returns>
		public static Object GetContainerDebtHolder(IRow iRow)
		{

			// Return the blotter to which this row belongs.  The blotter is then used in the filtering logic.
			DataRowVersion dataRowVersion = iRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;
			return iRow[DataModel.DebtHolder.DebtHolderIdColumn, dataRowVersion];

		}
		
		/// <summary>
		/// Gets the container for the given Destination Order.
		/// </summary>
		/// <param name="iRow">A Destination Order row to be examined for a container.</param>
		/// <returns>The owner Entity that is used to determine if this row should be filtered.</returns>
		public static Object GetContainerDebtNegotiator(IRow iRow)
		{

			// Return the blotter to which this row belongs.  The blotter is then used in the filtering logic.
			DataRowVersion dataRowVersion = iRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;
			return iRow[DataModel.DebtNegotiator.DebtNegotiatorIdColumn, dataRowVersion];

		}
		
		
		/// <summary>
		/// Gets the container for the given DebtRuleMap Order.
		/// </summary>
		/// <param name="iRow">A Destination Order row to be examined for a container.</param>
		/// <returns>The owner Entity that is used to determine if this row should be filtered.</returns>
		public static Object GetContainerDebtRuleMap(IRow iRow)
		{

			// Return the blotter to which this row belongs.  The blotter is then used in the filtering logic.
			DataRowVersion dataRowVersion = iRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;
			return iRow[DataModel.DebtRuleMap.DebtClassIdColumn, dataRowVersion];

		}
		
		/// <summary>
		/// Gets the container for the given Destination Order.
		/// </summary>
		/// <param name="iRow">A Destination Order row to be examined for a container.</param>
		/// <returns>The owner Entity that is used to determine if this row should be filtered.</returns>
		public static Object GetContainerDestinationOrder(IRow iRow)
		{

			// Return the blotter to which this row belongs.  The blotter is then used in the filtering logic.
			DataRowVersion dataRowVersion = iRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;
			return iRow[DataModel.DestinationOrder.BlotterIdColumn, dataRowVersion];

		}
		
		
		/// <summary>
		/// Gets the container for the given Destination Order.
		/// </summary>
		/// <param name="iRow">A Destination Order row to be examined for a container.</param>
		/// <returns>The owner Entity that is used to determine if this row should be filtered.</returns>
		public static Object GetContainerEntity(IRow iRow)
		{

			// Return the tenantId to which this row belongs.  The tenant is then used in the filtering logic.
			DataRowVersion dataRowVersion = iRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;
			return iRow[DataModel.Entity.TenantIdColumn, dataRowVersion];

		}

		/// <summary>
		/// Gets the container for the given Destination Order.
		/// </summary>
		/// <param name="iRow">A Destination Order row to be examined for a container.</param>
		/// <returns>The owner Entity that is used to determine if this row should be filtered.</returns>
		public static Object GetContainerEntityTree(IRow iRow)
		{

			// Return the blotter to which this row belongs.  The blotter is then used in the filtering logic.
			DataRowVersion dataRowVersion = iRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;
			return iRow[DataModel.EntityTree.ParentIdColumn, dataRowVersion];

		}
		/// <summary>
		/// Gets the container for the given EquityBlotter.
		/// </summary>
		/// <param name="iRow">An Execution row to be examined for a container.</param>
		/// <returns>The owner Entity that is used to determine if this row should be filtered.</returns>
		public static Object GetContainerEquityBlotter(IRow iRow)
		{

			// Return the blotter to which this row belongs.  The blotter is then used in the filtering logic.
			DataRowVersion dataRowVersion = iRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;
			return iRow[DataModel.EquityBlotter.EquityBlotterIdColumn, dataRowVersion];

		}


		/// <summary>
		/// Gets the container for the given Execution.
		/// </summary>
		/// <param name="iRow">An Execution row to be examined for a container.</param>
		/// <returns>The owner Entity that is used to determine if this row should be filtered.</returns>
		public static Object GetContainerExecution(IRow iRow)
		{

			// Return the blotter to which this row belongs.  The blotter is then used in the filtering logic.
			DataRowVersion dataRowVersion = iRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;
			return iRow[DataModel.Execution.BlotterIdColumn, dataRowVersion];

		}

		/// <summary>
		/// Gets the container for the given Institution.
		/// </summary>
		/// <param name="iRow">A Match row to be examined for a container.</param>
		/// <returns>The owner Entity that is used to determine if this row should be filtered.</returns>
		public static Object GetContainerGroup(IRow iRow)
		{
			// Return the blotter to which this row belongs.  The blotter is then used in the filtering logic.
			DataRowVersion dataRowVersion = iRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;
			return iRow[DataModel.Group.TenantIdColumn, dataRowVersion];			
		}

		/// <summary>
		/// Gets the container for the given Blotter.
		/// </summary>
		/// <param name="iRow">A Blotter row to be examined for a container.</param>
		/// <returns>The owner Entity that is used to determine if this row should be filtered.</returns>		
		public static Object GetContainerFolder(IRow iRow)
		{

			// Return the Tenant to which this row belongs.  The tenantId is then used in the filtering logic.
			DataRowVersion dataRowVersion = iRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;
			return iRow[DataModel.Folder.TenantIdColumn, dataRowVersion];			
		}
		
		/// <summary>
		/// Gets the container for the given Institution.
		/// </summary>
		/// <param name="iRow">A Match row to be examined for a container.</param>
		/// <returns>The owner Entity that is used to determine if this row should be filtered.</returns>
		public static Object GetContainerGroupUsers(IRow iRow)
		{

			// Return the blotter to which this row belongs.  The blotter is then used in the filtering logic.
			DataRowVersion dataRowVersion = iRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;
			return iRow[DataModel.GroupUsers.TenantIdColumn, dataRowVersion];			
		}

		/// <summary>
		/// Gets the container for the given Institution.
		/// </summary>
		/// <param name="iRow">A Match row to be examined for a container.</param>
		/// <returns>The owner Entity that is used to determine if this row should be filtered.</returns>
		public static Object GetContainerInstitution(IRow iRow)
		{

			// Return the blotter to which this row belongs.  The blotter is then used in the filtering logic.
			DataRowVersion dataRowVersion = iRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;
			return iRow[DataModel.Institution.TenantIdColumn, dataRowVersion];

		}

		
		/// <summary>
		/// Gets the container for the given Match.
		/// </summary>
		/// <param name="iRow">A Match row to be examined for a container.</param>
		/// <returns>The owner Entity that is used to determine if this row should be filtered.</returns>
		public static Object GetContainerMatch(IRow iRow)
		{

			// Return the blotter to which this row belongs.  The blotter is then used in the filtering logic.
			DataRowVersion dataRowVersion = iRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;
			return iRow[DataModel.Match.BlotterIdColumn, dataRowVersion];

		}

		/// <summary>
		/// Gets the container for the given Negotiation.
		/// </summary>
		/// <param name="iRow">A Negotiation row to be examined for a container.</param>
		/// <returns>The owner Entity that is used to determine if this row should be filtered.</returns>
		public static Object GetContainerOrganization(IRow iRow)
		{

			// Return the blotter to which this row belongs.  The blotter is then used in the filtering logic.
			DataRowVersion dataRowVersion = iRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;
			return iRow[DataModel.Tenant.TenantIdColumn, dataRowVersion];

		}
		
		/// <summary>
		/// Gets the container for the given Negotiation.
		/// </summary>
		/// <param name="iRow">A Negotiation row to be examined for a container.</param>
		/// <returns>The owner Entity that is used to determine if this row should be filtered.</returns>
		public static Object GetContainerNegotiation(IRow iRow)
		{

			// Return the blotter to which this row belongs.  The blotter is then used in the filtering logic.
			DataRowVersion dataRowVersion = iRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;
			return iRow[DataModel.Negotiation.BlotterIdColumn, dataRowVersion];

		}
		
		/// <summary>
		/// Gets the container for the given Source.
		/// </summary>
		/// <param name="iRow">A SourceOrder row to be examined for a container.</param>
		/// <returns>The owner Entity that is used to determine if this row should be filtered.</returns>
		public static Object GetContainerRightsHolder(IRow iRow)
		{

			// Return the blotter to which this row belongs.  The blotter is then used in the filtering logic.		
			DataRowVersion dataRowVersion = iRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;
			return iRow[DataModel.RightsHolder.TenantIdColumn, dataRowVersion];
		}
				
		/// <summary>
		/// Gets the container for the given Source.
		/// </summary>
		/// <param name="iRow">A SourceOrder row to be examined for a container.</param>
		/// <returns>The owner Entity that is used to determine if this row should be filtered.</returns>
		public static Object GetContainerTenantTree(IRow iRow)
		{

			// Return the blotter to which this row belongs.  The blotter is then used in the filtering logic.
			DataRowVersion dataRowVersion = iRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;
			return iRow[DataModel.TenantTree.ChildIdColumn, dataRowVersion];
		}
	
		/// <summary>
		/// Gets the container for the given Source.
		/// </summary>
		/// <param name="iRow">A SourceOrder row to be examined for a container.</param>
		/// <returns>The owner Entity that is used to determine if this row should be filtered.</returns>
		public static Object GetContainerTrader(IRow iRow)
		{

			// Return the blotter to which this row belongs.  The blotter is then used in the filtering logic.
			DataRowVersion dataRowVersion = iRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;
			return iRow[DataModel.Trader.TenantIdColumn, dataRowVersion];			
		}

		/// <summary>
		/// Gets the container for the given Security.
		/// </summary>
		/// <param name="iRow">A Blotter row to be examined for a container.</param>
		/// <returns>The owner Entity that is used to determine if this row should be filtered.</returns>		
		public static Object GetContainerSecurity(IRow iRow)
		{

			// Return the Tenant to which this row belongs.  The tenantId is then used in the filtering logic.
			//Grab the base entiyRow. Instead of propagating TenantId into all the tables we just have it on Entity.
			DataRowVersion dataRowVersion = iRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;
			return iRow[DataModel.Security.TenantIdColumn, dataRowVersion];			
		}

		/// <summary>
		/// Gets the container for the given Source.
		/// </summary>
		/// <param name="iRow">A SourceOrder row to be examined for a container.</param>		/// <returns>The owner Entity that is used to determine if this row should be filtered.</returns>
		public static Object GetContainerSource(IRow iRow)
		{

			// Return the blotter to which this row belongs.  The blotter is then used in the filtering logic.
			DataRowVersion dataRowVersion = iRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;
			return iRow[DataModel.Source.SourceIdColumn, dataRowVersion];

		}
		

		/// <summary>
		/// Gets the container for the given SourceOrder.
		/// </summary>
		/// <param name="iRow">A SourceOrder row to be examined for a container.</param>
		/// <returns>The owner Entity that is used to determine if this row should be filtered.</returns>
		public static Object GetContainerSourceOrder(IRow iRow)
		{

			// Return the blotter to which this row belongs.  The blotter is then used in the filtering logic.
			DataRowVersion dataRowVersion = iRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;
			return iRow[DataModel.SourceOrder.BlotterIdColumn, dataRowVersion];			
		}

		/// <summary>
		/// Gets the container for the given Blotter.
		/// </summary>
		/// <param name="iRow">A Blotter row to be examined for a container.</param>
		/// <returns>The owner Entity that is used to determine if this row should be filtered.</returns>		
		public static Object GetContainerSystemFolder(IRow iRow)
		{

			// Return the Tenant to which this row belongs.  The tenantId is then used in the filtering logic.
			DataRowVersion dataRowVersion = iRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;
			return iRow[DataModel.SystemFolder.TenantIdColumn, dataRowVersion];			
		}
		
		/// <summary>
		/// Gets the container for the given Execution.
		/// </summary>
		/// <param name="iRow">An Execution row to be examined for a container.</param>
		/// <returns>The owner Entity that is used to determine if this row should be filtered.</returns>
		public static Object GetContainerUser(IRow iRow)
		{
			// Return the blotter to which this row belongs.  The blotter is then used in the filtering logic.
			DataRowVersion dataRowVersion = iRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;
			return iRow[DataModel.User.TenantIdColumn, dataRowVersion];

		}
		
		/// <summary>
		/// Gets the container for the given Execution.
		/// </summary>
		/// <param name="iRow">An Execution row to be examined for a container.</param>
		/// <returns>The owner Entity that is used to determine if this row should be filtered.</returns>
		public static Object GetContainerWorkingOrder(IRow iRow)
		{

			// Return the blotter to which this row belongs.  The blotter is then used in the filtering logic.
			DataRowVersion dataRowVersion = iRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;
			return iRow[DataModel.WorkingOrder.BlotterIdColumn, dataRowVersion];

		}

		/// <summary>
		/// Determines if a given row can be returned to the client.
		/// </summary>
		/// <param name="dataModelTransaction">The transaction context for the operation.</param>
		/// <param name="filterContext">This is the user identifier used to look up access in an ACL.</param>
		/// <param name="containerContext">This is the object that is controlled by the ACL.</param>
		/// <returns>True if the given row can be returned to the client, false otherwise.</returns>
		public static Boolean FilterContainers(IDataModelTransaction dataModelTransaction, Object filterContext, Object containerContext)
		{

			// The idea here is to check to see if the given user has the rights to read the given object.  To do this, an access control list is maintained for
			// every major object in the database.  If an attempt is made to read something without a record, all access is denied.
			bool isAllowed = false;
			Guid rightsHolder = (Guid)filterContext;
			Guid entity = (Guid)containerContext;

			isAllowed = DataModelFilters.HasAccess(dataModelTransaction, rightsHolder, entity, AccessRight.Read);
#if false
			// Each unique combination of user and entity contains a set of flags that determine the access.
			AccessControlRow accessControlRow = DataModel.AccessControl.AccessControlKeyRightsHolderIdEntityId.Find((Guid)filterContext, (Guid)containerContext);
			if (accessControlRow != null)
			{

				try
				{

					// If an access control record exists for the given user and entity then it needs to be locked so we can examine what kind of access is
					// available.
					accessControlRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
					if (accessControlRow.RowState == DataRowState.Detached)
						throw new Exception(String.Format("AccesControl record {0}, {1} has been deleted", filterContext, containerContext));

					// This will use the constants in the AccessRight table to determine the user's access to this object.
					AccessRightRow accessRightRow = accessControlRow.AccessRightRow;
					try
					{
						accessRightRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
						isAllowed = (accessRightRow.AccessRightCode & AccessRight.Read) != 0;
					}
					finally
					{
						accessRightRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
					}

				}
				finally
				{
					accessControlRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				}

			}
#endif
			// This indicates if the user has the right to read the given object.
			return isAllowed;

		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="dataModelTransaction"></param>
		/// <param name="filterContext"></param>
		/// <param name="containerContext"></param>
		/// <returns></returns>
		public static Boolean FilterDebtNegotiationContainers(IDataModelTransaction dataModelTransaction, Object filterContext, Object containerContext)
		{
			Guid debtNegotiationId = (Guid)containerContext;
			Guid entityId = Guid.Empty;

			ConsumerDebtNegotiationRow debtNegotiationRow = DataModel.ConsumerDebtNegotiation.ConsumerDebtNegotiationKey.Find(debtNegotiationId);
			if (debtNegotiationRow != null)
			{
				if (debtNegotiationRow.IsLockHeld())
					throw new Exception(String.Format("Match record {0} is already locked", debtNegotiationId));

				debtNegotiationRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
				try
				{								
					if (debtNegotiationRow.RowState == DataRowState.Detached)
						throw new Exception(String.Format("ConsumerDebtNegotiation record {0} has been deleted", debtNegotiationId));

					entityId = debtNegotiationRow.MatchId;

				}
				finally
				{
					debtNegotiationRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				}

			}

			return FilterMatchContainers(dataModelTransaction, filterContext, entityId);
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="dataModelTransaction"></param>
		/// <param name="filterContext"></param>
		/// <param name="containerContext"></param>
		/// <returns></returns>
		public static Boolean FilterDebtSettlementContainers(IDataModelTransaction dataModelTransaction, Object filterContext, Object containerContext)
		{
			Guid debtSettlementId = (Guid)containerContext;
			Guid entityId = Guid.Empty;

			ConsumerDebtSettlementRow debtSettlementRow = DataModel.ConsumerDebtSettlement.ConsumerDebtSettlementKey.Find(debtSettlementId);
			if (debtSettlementRow != null)
			{
				debtSettlementRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
				try
				{										
					if (debtSettlementRow.RowState == DataRowState.Detached)
						throw new Exception(String.Format("ConsumerDebtSettlement record {0} has been deleted", debtSettlementId));

					entityId = debtSettlementRow.BlotterId;

				}
				finally
				{
					debtSettlementRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				}

			}

			return FilterContainers(dataModelTransaction, filterContext, entityId);
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="dataModelTransaction"></param>
		/// <param name="filterContext"></param>
		/// <param name="containerContext"></param>
		/// <returns></returns>
		public static Boolean FilterMatchContainers(IDataModelTransaction dataModelTransaction, Object filterContext, Object containerContext)
		{
			Guid matchId = (Guid)containerContext;
			Guid entityId = Guid.Empty;

			MatchRow matchRow = DataModel.Match.MatchKey.Find(matchId);
			if (matchRow != null)
			{
				if (matchRow.IsLockHeld())
					throw new Exception(String.Format("Match record {0} is already locked", matchId));

				matchRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
				try
				{										
					if (matchRow.RowState == DataRowState.Detached)
						throw new Exception(String.Format("Match record {0} has been deleted", matchId));

					entityId = matchRow.BlotterId;

				}
				finally
				{
					matchRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				}

			}

			return FilterContainers(dataModelTransaction, filterContext, entityId);
		}
		

		/// <summary>
		/// Determines if a given row can be returned to the client.
		/// </summary>
		/// <param name="dataModelTransaction">The transaction context for the operation.</param>
		/// <param name="filterContext">This is the user identifier used to look up access in an ACL.</param>
		/// <param name="containerContext">This is the object that is controlled by the ACL.</param>
		/// <returns>True if the given row can be returned to the client, false otherwise.</returns>
		public static Boolean FilterBrowsableContainers(IDataModelTransaction dataModelTransaction, Object filterContext, Object containerContext)
		{

			// The idea here is to check to see if the given user has the rights to read the given object.  To do this, an access control list is maintained for
			// every major object in the database.  If an attempt is made to read something without a record, all access is denied.
			bool isAllowed = false;
			Guid rightsHolder = (Guid)filterContext;
			Guid entity = (Guid)containerContext;

			isAllowed = DataModelFilters.HasAccess(dataModelTransaction, rightsHolder, entity, AccessRight.Browse);

			// This indicates if the user has the right to read the given object.
			return isAllowed;

		}

		/// <summary>
		/// Determines if a given consumer row can be returned to the client.
		/// </summary>
		/// <param name="dataModelTransaction">The transaction context for the operation.</param>
		/// <param name="filterContext">This is the user identifier used to look up access in an ACL.</param>
		/// <param name="containerContext">This is the object that is controlled by the ACL.</param>
		/// <returns>True if the given row can be returned to the client, false otherwise.</returns>
		public static Boolean FilterConsumerContainers(IDataModelTransaction dataModelTransaction, Object filterContext, Object containerContext)
		{
			// The idea here is to check if given record belongs to a certain tenant			
			Guid userId = (Guid)filterContext;
			Guid consumerId = (Guid)containerContext;
			Guid tenantId = Guid.Empty;
			
			ConsumerRow consumerRow = DataModel.Consumer.ConsumerKey.Find(consumerId);

			if (consumerRow != null)
			{
				
				try
				{
					//Do dirty reads
					ConsumerDebtRow[] consumerDebtRows = consumerRow.GetConsumerDebtRows_NoLockCheck();
					if (consumerDebtRows != null && consumerDebtRows.Length > 0)
					{
						ConsumerDebtRow debtRow = consumerDebtRows[0];						
						tenantId = debtRow.TenantId_NoLockCheck;						
					}

					//Do dirty reads
					ConsumerTrustRow[] consumerTrustRows = consumerRow.GetConsumerTrustRows_NoLockCheck();
					if (consumerTrustRows != null && consumerTrustRows.Length > 0)
					{
						ConsumerTrustRow trustRow = consumerTrustRows[0];						
						tenantId = trustRow.TenantId_NoLockCheck;
						
					}
				}				
				catch (Exception exception)
				{
					EventLog.Error(exception);
					tenantId = Guid.Empty;
				}				
			}
			
			if(tenantId != Guid.Empty)
			{
				return FilterTenantContainers(dataModelTransaction, filterContext, tenantId);
			}
			
			return false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dataModelTransaction"></param>
		/// <param name="filterContext"></param>
		/// <param name="containerContext"></param>
		/// <returns></returns>
		public static Boolean FilterTrustNegotiationContainers(IDataModelTransaction dataModelTransaction, Object filterContext, Object containerContext)
		{
			Guid trustNegotiationId = (Guid)containerContext;
			Guid entityId = Guid.Empty;

			ConsumerTrustNegotiationRow trustNegotiationRow = DataModel.ConsumerTrustNegotiation.ConsumerTrustNegotiationKey.Find(trustNegotiationId);
			if (trustNegotiationRow != null)
			{
				trustNegotiationRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
				try
				{
					if (trustNegotiationRow.RowState == DataRowState.Detached)
						throw new Exception(String.Format("ConsumerTrustNegotiation record {0} has been deleted", trustNegotiationId));

					entityId = trustNegotiationRow.MatchId;

				}
				finally
				{
					trustNegotiationRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				}

			}

			return FilterMatchContainers(dataModelTransaction, filterContext, entityId);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dataModelTransaction"></param>
		/// <param name="filterContext"></param>
		/// <param name="containerContext"></param>
		/// <returns></returns>
		public static Boolean FilterTrustSettlementContainers(IDataModelTransaction dataModelTransaction, Object filterContext, Object containerContext)
		{
			Guid trustSettlementId = (Guid)containerContext;
			Guid entityId = Guid.Empty;

			ConsumerTrustSettlementRow trustSettlementRow = DataModel.ConsumerTrustSettlement.ConsumerTrustSettlementKey.Find(trustSettlementId);
			if (trustSettlementRow != null)
			{
				trustSettlementRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
				try
				{
					if (trustSettlementRow.RowState == DataRowState.Detached)
						throw new Exception(String.Format("ConsumerTrustSettlement record {0} has been deleted", trustSettlementId));

					entityId = trustSettlementRow.BlotterId;

				}
				finally
				{
					trustSettlementRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				}

			}

			return FilterContainers(dataModelTransaction, filterContext, entityId);
		}
		
		
		/// <summary>
		/// Determines if a given row can be returned to the client.
		/// </summary>
		/// <param name="dataModelTransaction">The transaction context for the operation.</param>
		/// <param name="filterContext">This is the user identifier used to look up access in an ACL.</param>
		/// <param name="containerContext">This is the object that is controlled by the ACL.</param>
		/// <returns>True if the given row can be returned to the client, false otherwise.</returns>
		public static Boolean FilterTenantContainers(IDataModelTransaction dataModelTransaction, Object filterContext, Object containerContext)
		{
			
			//Some entities do not have an TenantID.  These could be securities like IBM, MSFT that every
			// organization will have access to in an exchange.
			if (containerContext == DBNull.Value)
				return true;

			// The idea here is to check if given record belongs to a certain tenant			
			Guid userId = (Guid)filterContext;
			Guid tenantId = (Guid)containerContext;
			bool isAllowed = false;

			UserRow userRow = null;
			TenantRow tenantParentRow = null;
			//Determine the tenantId that is user belongs to
			try
			{
				userRow = DataModel.User.UserKey.Find(userId);
				userRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
				Guid parentTenantId = userRow.TenantId;				
				userRow.ReleaseLock(dataModelTransaction.TransactionId);

				//Check if the tenant is present and is public
				TenantRow tenantRow = DataModel.Tenant.TenantKey.Find(tenantId);
				if (tenantRow == null)
					return false;

				tenantRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
				try
				{
					if (tenantRow.IsPublic == true)
						return true;
				}
				finally
				{
					tenantRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				}


				//If user has access to this tennant then no further checks are needed.
				if (tenantId == parentTenantId)
					return true;

				//Check the hierachy.
				tenantParentRow = DataModel.Tenant.TenantKey.Find(parentTenantId);
				tenantParentRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

				foreach(var tenantTreeRow in  tenantParentRow.GetTenantTreeRowsByFK_Tenant_TenantTree_ParentId())
				{
					try
					{
						tenantTreeRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
						if (tenantTreeRow.ChildId == tenantId)
							isAllowed = true;

					}
					finally
					{
						if(tenantTreeRow.IsReaderLockHeld(dataModelTransaction.TransactionId))
							tenantTreeRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
					}

					//If tenant is found than no need for further processing
					if(isAllowed == true)
						break;
				}

			}
			finally
			{
				if(userRow != null &&
					userRow.IsReaderLockHeld(dataModelTransaction.TransactionId))
				{
					userRow.ReleaseLock(dataModelTransaction.TransactionId);
				}

				if (tenantParentRow != null &&
					tenantParentRow.IsReaderLockHeld(dataModelTransaction.TransactionId))
				{
					tenantParentRow.ReleaseLock(dataModelTransaction.TransactionId);
				}
			}

			return isAllowed;
		}
		

		/// <summary>
		/// Determines if a given row can be returned to the client.
		/// </summary>
		/// <param name="dataModelTransaction">The transaction context for the operation.</param>
		/// <param name="userId">This is the user identifier used to look up access in an ACL.</param>
		/// <param name="entityId">This is the object that is controlled by the ACL.</param>
		/// <param name="accessRight">The right desired.</param>
		/// <returns>True if the given object has the given access for the given user, false otherwise.</returns>
		public static Boolean HasAccess(IDataModelTransaction dataModelTransaction, Guid userId, Guid entityId, AccessRight accessRight)
		{

			// The idea here is to check to see if the given user (or any of the groups they are in) has the rights to read the given object.  To do
			// this, an access control list is maintained for every major object in the database.  If an attempt is made to read something without a
			// record, all access is denied.
			bool isAllowed = false;
			List<Guid> rightsHolders = new List<Guid>();
			UserRow userRow = DataModel.User.UserKey.Find(userId);

			rightsHolders.Add(userId);

			userRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

			// Build a list of the all rights holders related to the given user (that is, the user itself, and all the groups it belongs to).
			try
			{

				foreach (GroupUsersRow groupUsersRow in userRow.GetGroupUsersRows())
				{

					groupUsersRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

					try
					{

						rightsHolders.Add(groupUsersRow.GroupId);

					}
					finally
					{

						groupUsersRow.ReleaseLock(dataModelTransaction.TransactionId);

					}

				}

			}
			finally
			{

				userRow.ReleaseLock(dataModelTransaction.TransactionId);

			}

			// Paw through the list of rights holders trying to find one with the requested rights to the entity.
			foreach (Guid rightsHolder in rightsHolders)
			{

				// Each unique combination of user and entity contains a set of flags that determine the access.
				AccessControlRow accessControlRow = DataModel.AccessControl.AccessControlKeyRightsHolderIdEntityId.Find(rightsHolder, entityId);
				if (accessControlRow != null)
				{

					// If an access control record exists for the given rights holder and entity then it needs to be locked so we can examine what
					// kind of access is available.
					accessControlRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

					try
					{

						if (accessControlRow.RowState == DataRowState.Detached)
							throw new Exception(String.Format("AccesControl record {0}, {1} has been deleted", rightsHolder, entityId));

						// This will use the constants in the AccessRight table to determine the user's access to this object.
						AccessRightRow accessRightRow = accessControlRow.AccessRightRow;
						try
						{
							accessRightRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
							isAllowed = (accessRightRow.AccessRightCode & accessRight) != 0;

							if (isAllowed)
								break;
						}
						finally
						{
							accessRightRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
						}

					}
					finally
					{
						accessControlRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
					}

				}

			}

			// This indicates if the user has the right to read the given object.
			return isAllowed;

		}

		/// <summary>
		/// Determine whether a user has "write level" access to a tenant, that is, whether the user's tenant the specified tenant, or above the
		/// specified tenant in the tenant tree.
		/// </summary>
		/// <param name="dataModelTransaction">The current transaction.</param>
		/// <param name="userId">The user's id.</param>
		/// <param name="tenantId">The tenant's id.</param>
		/// <returns>True if user has "write leve" access, false if not.</returns>
		public static Boolean HasTenantAccess(IDataModelTransaction dataModelTransaction, Guid userId, Guid tenantId)
		{

			bool isAllowed = false;
			UserRow userRow = null;
			TenantRow tenantParentRow = null;

			try
			{

				//Determine the tenantId that is user belongs to.
				userRow = DataModel.User.UserKey.Find(userId);
				userRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
				Guid parentTenantId = userRow.TenantId;
				userRow.ReleaseLock(dataModelTransaction.TransactionId);

				//If user has access to this tennant then no further checks are needed.
				if (tenantId == parentTenantId)
					return true;

				//Check the hierachy.
				tenantParentRow = DataModel.Tenant.TenantKey.Find(parentTenantId);
				tenantParentRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);

				foreach (var tenantTreeRow in tenantParentRow.GetTenantTreeRowsByFK_Tenant_TenantTree_ParentId())
				{
					try
					{
						tenantTreeRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
						if (tenantTreeRow.ChildId == tenantId)
							isAllowed = true;

					}
					finally
					{
						if (tenantTreeRow.IsReaderLockHeld(dataModelTransaction.TransactionId))
							tenantTreeRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
					}

					//If tenant is found than no need for further processing
					if (isAllowed == true)
						break;
				}

			}
			finally
			{
				if (userRow != null &&
					userRow.IsReaderLockHeld(dataModelTransaction.TransactionId))
				{
					userRow.ReleaseLock(dataModelTransaction.TransactionId);
				}

				if (tenantParentRow != null &&
					tenantParentRow.IsReaderLockHeld(dataModelTransaction.TransactionId))
				{
					tenantParentRow.ReleaseLock(dataModelTransaction.TransactionId);
				}
			}

			return isAllowed;

		}

	}

}
