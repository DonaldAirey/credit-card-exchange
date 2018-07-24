namespace FluidTrade.Guardian
{

	using System;
	using System.IO;
	using System.Windows;
	using System.Windows.Threading;
	using FluidTrade.Guardian.Windows;

	/// <summary>
	/// Interaction logic for PDFReport.xaml
	/// </summary>
	public partial class PDFReport : PDFReportBase
	{
		public Byte[] settlementLetter = null;
		public bool IsLetterLoaded = false;
		Guid currentConsumerTrustSettlementId;

		/// <summary>
		/// Refresh the letter if the loading of the letter was still in process.
		/// </summary>
		/// <param name="settlementLetterGuid"></param>
		public void RefreshLetter(Guid settlementLetterGuid)
		{
			InitializeData(settlementLetterGuid);
		}
        /// <summary>
		/// Default constructor.
		/// </summary>
		public PDFReport()
		{

			InitializeComponent();

		}

		private delegate void SettlementLetterHandler(Byte[] settlementLetter);

		/// <summary>
		/// Initialize the GUI.
		/// </summary>
		/// <param name="parameter"></param>
		protected override void InitializeData(object parameter)
		{

			settlementLetter = null;
			Guid consumerTrustSettlementId = (Guid)parameter;

			if ((consumerTrustSettlementId != null) && (consumerTrustSettlementId != Guid.Empty))
			{
				// Lock the data model to extract information from the settlementRow.
				lock (DataModel.SyncRoot)
				{
					ConsumerTrustSettlementRow consumerTrustSettlementRow = DataModel.ConsumerTrustSettlement.ConsumerTrustSettlementKey.Find(consumerTrustSettlementId);
					if (!consumerTrustSettlementRow.IsSettlementLetterNull())
						settlementLetter = Convert.FromBase64String(consumerTrustSettlementRow.SettlementLetter);
				}

				// Dispatch to foreground thread for updating the GUI.
				if (settlementLetter != null)
				{
					this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SettlementLetterHandler(UpdateConsole), settlementLetter);
				}

				currentConsumerTrustSettlementId = consumerTrustSettlementId;
			}
			else
			{
				//Clear out when switching between Debt Classes.
				if ((IsLetterLoaded) && (consumerTrustSettlementId == Guid.Empty))
				{
					// Sending null to clear out the settlement from the viewer.
					this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SettlementLetterHandler(UpdateConsole), null);
					settlementLetter = null;
				}
				else if ((currentConsumerTrustSettlementId != null) && (currentConsumerTrustSettlementId != Guid.Empty))
				{
					// Lock the data model to extract information from the settlementRow.
					lock (DataModel.SyncRoot)
					{
						ConsumerTrustSettlementRow consumerTrustSettlementRow = DataModel.ConsumerTrustSettlement.ConsumerTrustSettlementKey.Find(currentConsumerTrustSettlementId);
						if (!consumerTrustSettlementRow.IsSettlementLetterNull())
							settlementLetter = Convert.FromBase64String(consumerTrustSettlementRow.SettlementLetter);
					}

					// Dispatch to foreground thread for updating the GUI.
					if (settlementLetter != null)
					{
						this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SettlementLetterHandler(UpdateConsole), settlementLetter);
					}

				}
			}

        }

		// Private Functions

		/// <summary>
		/// Update the GUI based on information from the SettlementInfo object.
		/// </summary>
		/// <param name="settlementInfo">SettlementInfo object.</param>
		private void UpdateConsole(Byte[] settlementLetter)
		{
			if (settlementLetter != null)
			{
				// Load up the PDF from a memory stream.
				this.pdfViewer.Source = new MemoryStream(settlementLetter);
				IsLetterLoaded = true;
			}
			else
			{
				// Clear out the settlement in the viewer.
				this.pdfViewer.Source = null;
				IsLetterLoaded = false;
			}

		}

	}

}
