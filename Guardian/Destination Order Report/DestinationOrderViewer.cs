namespace FluidTrade.Guardian
{
    using FluidTrade.Core;
    using FluidTrade.Core.Windows.Controls;
    using FluidTrade.Guardian.Windows;
    using System;
    using System.Threading;
    

	/// <summary>
	/// A viewer for the orders that originate from the buy side trader.
	/// </summary>
	public class DestinationOrderViewer : DynamicReport
	{

		// Public Events
		public event System.EventHandler DestinationOrder;

		// Private Instance Fields
		private System.Object content;

		/// <summary>
		/// Constructor for the DestinationOrderViewer
		/// </summary>
		public DestinationOrderViewer()
		{

		}

		/// <summary>
		/// Initializes the background redestinations for the viewer.
		/// </summary>
		public new System.Object Content
		{

			set
			{

				this.content = value;

				try
				{

					// Lock the data model while the tables are read.
					Monitor.Enter(DataModel.SyncRoot);

					// The Data Transform contains the instructions for building and displaying the customized view of the data.
					BlotterConfigurationRow blotterConfigurationRow = null;

					// This will open the viewer to show the destination orders for an entire blotter.
					if (this.content is Blotter)
					{

						// This helps to examine the values used to open this document.
						Blotter blotter = this.content as Blotter;

						// Find the Data Transform for the Destination Order viewer for this blotter.
						blotterConfigurationRow = DataModel.BlotterConfiguration.BlotterConfigurationKeyBlotterIdReportTypeId.Find(
							blotter.BlotterId,
							DataModel.ReportType.ReportTypeKeyReportTypeCode.Find(ReportType.DestinationOrder).ReportTypeId);

					}

					// This will open the viewer to show only selected working orders.
					if (this.content is BlotterWorkingOrderDetail)
					{

						// This helps to examine the values used to open this document.
						BlotterWorkingOrderDetail blotterWorkingOrderDetail = this.content as BlotterWorkingOrderDetail;

						// Find the Data Transform for the Destination Order Detail viewer for this blotter.
						blotterConfigurationRow =
							DataModel.BlotterConfiguration.BlotterConfigurationKeyBlotterIdReportTypeId.Find(
								blotterWorkingOrderDetail.Blotter.BlotterId,
								DataModel.ReportType.ReportTypeKeyReportTypeCode.Find(ReportType.DestinationOrderDetail).ReportTypeId);

					}

					// Throw an exception if there is no data transform for displaying the data associated with the tag.
					if (blotterConfigurationRow == null)
						throw new Exception(String.Format("The {0} can not display an object of {1}", this.GetType(), this.content));

				}
				catch (Exception exception)
				{

					// Write the error and stack trace out to the debug listener
					EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);

				}
				finally
				{

					// Allow other threads to access the data model.
					Monitor.Exit(DataModel.SyncRoot);

				}

			}

		}

		/// <summary>
		/// Called when an destinationOrder quantity is added to the block order.
		/// </summary>
		/// <param name="destinationOrderEventArgs">Event parameters.</param>
		protected virtual void OnDestinationOrder()
		{

			// Broadcast the event to anyone listening.
			if (this.DestinationOrder != null)
				this.DestinationOrder(this, EventArgs.Empty);

		}

	}

}

