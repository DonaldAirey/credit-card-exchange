namespace FluidTrade.Guardian
{
    using FluidTrade.Core;
    using FluidTrade.Core.Windows.Controls;
    using FluidTrade.Guardian.Windows;
    using System;
    using System.Collections.Generic;
    using System.Threading;

	/// <summary>
	/// A viewer for the orders that originate from the buy side trader.
	/// </summary>
	public class SourceOrderViewer : Viewer
	{

		// Public Events
		public event System.EventHandler SourceOrder;

		// Private Instance Fields
		private System.Object content;

		/// <summary>
		/// Constructor for the SourceOrderViewer
		/// </summary>
		public SourceOrderViewer()
		{

		}

		/// <summary>
		/// Initializes the background resources for the viewer.
		/// </summary>
		public new System.Object Content
		{

			get { return this.content; }
			set
			{

				try
				{

					this.content = value;

					// Lock the data model while the tables are read.
					Monitor.Enter(DataModel.SyncRoot);

					// The Data Transform contains the instructions for building and displaying the customized view of the data.
					BlotterConfigurationRow blotterConfigurationRow = null;

					// This will open the viewer to show the source orders for an entire blotter.
					if (value is Blotter)
					{

						// This helps to examine the values used to open this document.
						Blotter blotter = value as Blotter;

						// Find the Data Transform for the Source Order viewer for this blotter.
						blotterConfigurationRow =DataModel.BlotterConfiguration.BlotterConfigurationKeyBlotterIdReportTypeId.Find(
							blotter.BlotterId,
							DataModel.ReportType.ReportTypeKeyReportTypeCode.Find(ReportType.SourceOrder).ReportTypeId);

					}

					// This will open the viewer to show only selected working orders.
					if (value is BlotterWorkingOrderDetail)
					{

						// This helps to examine the values used to open this document.
						BlotterWorkingOrderDetail blotterWorkingOrderDetail = value as BlotterWorkingOrderDetail;

						// Find the Data Transform for the Source Order Detail viewer for this blotter.
						blotterConfigurationRow = DataModel.BlotterConfiguration.BlotterConfigurationKeyBlotterIdReportTypeId.Find(
							blotterWorkingOrderDetail.Blotter.BlotterId,
							DataModel.ReportType.ReportTypeKeyReportTypeCode.Find(ReportType.SourceOrderDetail).ReportTypeId);

					}

					// Throw an exception if there is no data transform for displaying the data associated with the tag.
					if (blotterConfigurationRow == null)
						throw new Exception(String.Format("The {0} can not display an object of {1}", this.GetType(), value));

					// The filter selects the working order rows that will appear in this viewer.  All the descendants of the selected
					// blotter are included in this view.
					SetFilter();

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
		/// Sets the filter used to select the rows that appear in this viewer.
		/// </summary>
		protected void SetFilter()
		{

			// This sets the filter when the viewer shows all working orders in a blotter.
			if (this.content is Blotter)
			{

				// This helps to examine the values used to open this document.
				Blotter blotter = this.content as Blotter;

				// The root object row is used to find all the descendants to this blotter in the tree structure of objects.  When
				// all the descendants have been found, a string is constructed that will be compiled into the view to select the
				// rows at the top level of the document.
				EntityRow entityRow = DataModel.Entity.EntityKey.Find(blotter.BlotterId);
				if (entityRow != null)
				{
					List<Guid> descendants = Hierarchy.GetDescendants(entityRow);
					string filterSnippet = string.Empty;
					foreach (Guid objectId in descendants)
						filterSnippet += string.Format(filterSnippet == string.Empty ?
							"sourceOrderRow.WorkingOrderRow.BlotterId=={0}" :
							"||sourceOrderRow.WorkingOrderRow.BlotterId=={0}", objectId);
				}
			}

			// This sets the filter when the viewer shows only the selected working orders in a blotter.
			if (this.content is BlotterWorkingOrderDetail)
			{

				// This helps to examine the values used to open this document.
				BlotterWorkingOrderDetail blotterWorkingOrderDetail = this.content as BlotterWorkingOrderDetail;

				// This constructs a filter that will show only the selected working orders.  If no Working Orders are selected,
				// then a primitive 'false' is used to satisfy the conditions of the search.
				string filterSnippet = string.Empty;
				foreach (WorkingOrder workingOrder in blotterWorkingOrderDetail.WorkingOrders)
					filterSnippet += string.Format(filterSnippet == string.Empty ?
						"sourceOrderRow.WorkingOrderId=={0}" :
						"||sourceOrderRow.WorkingOrderId=={0}", workingOrder.WorkingOrderId);

			}

		}

#if DEBUG_SOURCE
		/// <summary>
		/// Compile and load the view from the Report.
		/// </summary>
		/// <param name="parameter">Unused thread initialization parameter.</param>
		public override void LoadView()
		{

			// Normally the View is compiled and loaded dynamically, which makes source level debugging pretty much impossible (as
			// there is no real source code on disk that the debugger can use).  This trick allows a developer to step through the
			// generated code on those occations when something needs more scrutiny.
			SetView(new SourceOrderView(this));

		}
#endif

		/// <summary>
		/// Called when an sourceOrder quantity is added to the block order.
		/// </summary>
		/// <param name="sourceOrderEventArgs">Event parameters.</param>
		protected virtual void OnSourceOrder()
		{

			// Broadcast the event to anyone listening.
			if (this.SourceOrder != null)
				this.SourceOrder(this, EventArgs.Empty);

		}

	}

}

