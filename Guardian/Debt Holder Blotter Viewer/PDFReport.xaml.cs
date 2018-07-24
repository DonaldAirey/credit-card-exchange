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
		Guid currentConsumerDebtSettlementId;

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
			Guid consumerDebtSettlementId = (Guid)parameter;

			if ((consumerDebtSettlementId != null) && (consumerDebtSettlementId != Guid.Empty))
			{

				// Lock the data model to extract information from the settlementRow.
				lock (DataModel.SyncRoot)
				{
					ConsumerDebtSettlementRow consumerDebtSettlementRow = DataModel.ConsumerDebtSettlement.ConsumerDebtSettlementKey.Find(consumerDebtSettlementId);
					if (!consumerDebtSettlementRow.IsSettlementLetterNull())
						settlementLetter = Convert.FromBase64String(consumerDebtSettlementRow.SettlementLetter);
				}

				// Dispatch to foreground thread for updating the GUI.
				if (settlementLetter != null)
				{
					this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SettlementLetterHandler(UpdateConsole), settlementLetter);
				}

				currentConsumerDebtSettlementId = consumerDebtSettlementId;


			}
			else
			{
				//Clear out when switching between Debt Classes.
				if ((IsLetterLoaded) && (consumerDebtSettlementId == Guid.Empty))
				{
					// Sending null to clear out the settlement from the viewer.
					this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SettlementLetterHandler(UpdateConsole), null);
					settlementLetter = null;
				}
				else if ((currentConsumerDebtSettlementId != null) && (currentConsumerDebtSettlementId != Guid.Empty))
				{
					// Lock the data model to extract information from the settlementRow.
					lock (DataModel.SyncRoot)
					{
						ConsumerDebtSettlementRow consumerDebtSettlementRow = DataModel.ConsumerDebtSettlement.ConsumerDebtSettlementKey.Find(currentConsumerDebtSettlementId);
						if (!consumerDebtSettlementRow.IsSettlementLetterNull())
							settlementLetter = Convert.FromBase64String(consumerDebtSettlementRow.SettlementLetter);
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
