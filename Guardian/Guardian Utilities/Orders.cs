namespace FluidTrade.Guardian
{

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
	
	/// <summary>
	/// Handles the helper functions for managing orders.
	/// </summary>
	class Orders
	{

		// Private Constants
		private const Int32 batchSize = 1000;

		/// <summary>
		/// Destroy the executions and destination orders on the shared data model.
		/// </summary>
		/// <param name="sender">The generic thread initialization parameter.</param>
		public static void DestroyOrders(object sender)
		{

			// This will establish an endpoint to the web services that support trading functions.
			TradingSupportClient tradingSupportClient = new TradingSupportClient(Properties.Settings.Default.TradingSupportEndpoint);

			lock (DataModel.SyncRoot)
			{

				// To prevent a buffer of an arbitrary size, the commands to delete orders are batched up and sent in chunks.
				int batchCounter = 0;

				// This will construct a list of references to the orders.  This list will be transmitted to the server where a web service will
				// pull it apart and
				List<DestinationOrderReference> destinationOrderReferences = new List<DestinationOrderReference>();
				foreach (DestinationOrderRow destinationOrderRow in DataModel.DestinationOrder)
				{

					DestinationOrderReference destinationOrderReference = new DestinationOrderReference();
					destinationOrderReference.DestinationId = destinationOrderRow.DestinationOrderId;
					destinationOrderReference.RowVersion = destinationOrderRow.RowVersion;
					destinationOrderReferences.Add(destinationOrderReference);

					if (batchCounter++ == batchSize)
					{
						batchCounter = 0;
						tradingSupportClient.DestroyDestinationOrders(destinationOrderReferences.ToArray());
						destinationOrderReferences = new List<DestinationOrderReference>();
					}

				}

				if (destinationOrderReferences.Count != 0)
					tradingSupportClient.DestroyDestinationOrders(destinationOrderReferences.ToArray());

			}

			tradingSupportClient.Close();

		}

		/// <summary>
		/// Destroy the executions and destination orders on the shared data model.
		/// </summary>
		/// <param name="sender">The generic thread initialization parameter.</param>
		public static void ClearCross(object sender)
		{

			DataModelClient dataModelClient =  new DataModelClient(Properties.Settings.Default.DataModelEndpoint);

			lock (DataModel.SyncRoot)
			{

				try
				{

					// Destroy all the negotations
					foreach (NegotiationRow negotiationRow in DataModel.Negotiation)
						dataModelClient.DestroyNegotiation(new Object[] { negotiationRow.NegotiationId }, negotiationRow.RowVersion);

					// Destroy all the Chat items.
					foreach (ChatRow chatRow in DataModel.Chat)
						dataModelClient.DestroyChat(new Object[] { chatRow.ChatId }, chatRow.RowVersion);

					// Destroy all the matches
					foreach (MatchRow matchRow in DataModel.Match)
						dataModelClient.DestroyMatch(new Object[] { matchRow.MatchId }, matchRow.RowVersion);

					// Destroy all the match timers
					foreach (MatchTimerRow matchTimerRow in DataModel.MatchTimer)
						dataModelClient.DestroyMatchTimer(new Object[] { matchTimerRow.MatchId }, matchTimerRow.RowVersion);

					// Destroy all the Destionation Orders
					foreach (DestinationOrderRow destinationOrderRow in DataModel.DestinationOrder)
						dataModelClient.DestroyDestinationOrder(new Object[] { destinationOrderRow.DestinationOrderId }, destinationOrderRow.RowVersion);

					// Destroy all the Orders involved in the Consumer Trust/Consumer Debt demo.
					DestroyWorkingOrders(dataModelClient, "Ingrid Yeoh");
					DestroyWorkingOrders(dataModelClient, "Kai Hitori");
					DestroyWorkingOrders(dataModelClient, "High Risk");
					DestroyWorkingOrders(dataModelClient, "Low Risk");
					DestroyWorkingOrders(dataModelClient, "Medium Risk");

					// Reset all the working orders in the Debt Matching Demo.
					ResetWorkingOrders(dataModelClient, "Russell Jackson");
					ResetWorkingOrders(dataModelClient, "Kareem Rao");


				}
				catch (Exception exception)
				{
					Console.WriteLine("{0}, {1}", exception.Message, exception.StackTrace);
				}

			}

			// Shut down the channel gracefully.
			dataModelClient.Close();

		}

		/// <summary>
		/// Clears the given blotter of all working orders.
		/// </summary>
		/// <param name="dataModelClient">Used to execute Web Services.</param>
		/// <param name="blotterName">The name of the blotter to be reset.</param>
		private static void DestroyWorkingOrders(DataModelClient dataModelClient, String blotterName)
		{

			// Destroy all the Working Orders for Kai Hitori
			foreach (WorkingOrderRow workingOrderRow in DataModel.WorkingOrder)
			{
				EntityRow entityRow = workingOrderRow.BlotterRow.EntityRow;
				if (entityRow.Name == blotterName)
					dataModelClient.DestroyWorkingOrder(workingOrderRow.RowVersion, new object[] { workingOrderRow.WorkingOrderId });
			}

		}

		/// <summary>
		/// Resets the matching criteria for all orders in the selected blotter.
		/// </summary>
		/// <param name="dataModelClient">Used to execute Web Services.</param>
		/// <param name="blotterName">The name of the blotter to be reset.</param>
		private static void ResetWorkingOrders(DataModelClient dataModelClient, String blotterName)
		{

			// This will reset the matching flags for the given blotter.
			foreach (WorkingOrderRow workingOrderRow in DataModel.WorkingOrder)
			{
				EntityRow entityRow = workingOrderRow.BlotterRow.EntityRow;
				if (entityRow.Name == blotterName)
				{
					if (workingOrderRow.CrossingRow.CrossingCode != Crossing.NeverMatch || workingOrderRow.StatusRow.StatusCode != Status.New)
					{
						dataModelClient.UpdateWorkingOrder(null, null, null, null, Crossing.NeverMatch, null, null, null, false,
							null, null, null, null, null, null, null, workingOrderRow.RowVersion, null, null, null, null, null,
							Status.New, null, null, null, null, null, null, null, null, new Object[] { workingOrderRow.WorkingOrderId });
					}
				}
			}

		}

	}

}
