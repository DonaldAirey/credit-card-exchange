namespace FluidTrade.Guardian
{

    using System;
	using System.Collections.Generic;
	using System.ComponentModel;
    using System.Data;
    using System.Threading;
	using System.Windows.Forms;
    using FluidTrade.Guardian;

	/// <summary>
	/// Prompts a user to select a destination.
	/// </summary>
	public partial class ListBoxDestination : ListBox
	{

		// Private Fields
		private System.Guid blotterId;

		// Private Delegates
		private delegate void DestinationListDelegate(List<DestinationItem> destinationItems);

		/// <summary>
		/// Used to manage the list of destinations that appears in the list box.
		/// </summary>
		[Serializable]
		private class DestinationItem
		{

			// Public Fields
			public System.Guid destintationId;
			public System.String name;
			public System.String shortName;

			/// <summary>
			/// Used to manage the 'units' combo box which allows the user to select shares or quantity.
			/// </summary>
			/// <param name="unit"></param>
			/// <param name="text"></param>
			public DestinationItem(Guid destinationId, string name, string shortName)
			{

				// Initialize the object.
				this.destintationId = destinationId;
				this.name = name;
				this.shortName = shortName;

			}

			public Guid DestinationId { get { return this.destintationId; } }
			
			public string Name { get { return this.name; } }

			public string ShortName { get { return this.shortName; } }

		}

		public Guid BlotterId
		{

			get { return this.blotterId; }

			set
			{

				this.blotterId = value;
				if (this.IsHandleCreated && LicenseManager.UsageMode == LicenseUsageMode.Runtime)
					FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(InitializeThread, this.blotterId);

			}

		}

		private void SetBlotterId(object state)
		{

			Guid blotterId = (Guid)state;

		}

		public ListBoxDestination()
		{

			InitializeComponent();

		}

		protected override void OnHandleCreated(EventArgs e)
		{

			if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(InitializeThread, this.blotterId);

			base.OnHandleCreated(e);

		}

		private void InitializeListBox(List<DestinationItem> destinationItems)
		{

			object selectedObject = this.SelectedValue;

			this.DataSource = destinationItems;
			this.DisplayMember = "ShortName";
			this.ValueMember = "DestinationId";

			this.SelectedValue = selectedObject;

			this.Enabled = destinationItems.Count > 0;

		}

		private void InitializeThread(object state)
		{

			// Extract the initialization parameters.
			Guid blotterId = (Guid)state;

			List<DestinationItem> destinationItems = new List<DestinationItem>();

			// Initialize the Destination list box
			try
			{

				Monitor.Enter(DataModel.SyncRoot);

				DataModel.Destination.DestinationRowChanged += new DestinationRowChangeEventHandler(Destination_DestinationRowChanged);

				BlotterRow blotterRow = DataModel.Blotter.BlotterKey.Find(blotterId);
				foreach (BlotterDestinationMapRow blotterDestinationRowMap in blotterRow.GetBlotterDestinationMapRows())
				{
					DestinationRow destinationRow = blotterDestinationRowMap.DestinationRow;
					destinationItems.Add(new DestinationItem(destinationRow.DestinationId, destinationRow.Name,
						destinationRow.ShortName));
				}

			}
			finally
			{

				Monitor.Exit(DataModel.SyncRoot);

			}

			Invoke(new DestinationListDelegate(InitializeListBox), destinationItems);

		}

		void Destination_DestinationRowChanged(object sender, DestinationRowChangeEventArgs e)
		{

			if (e.Action == DataRowAction.Commit && e.Row.RowState != DataRowState.Detached)
			{

				List<DestinationItem> destinationItems = new List<DestinationItem>();

				foreach (DestinationRow destinationRow in DataModel.Destination)
					destinationItems.Add(new DestinationItem(destinationRow.DestinationId, destinationRow.Name,
						destinationRow.ShortName));

				BeginInvoke(new DestinationListDelegate(InitializeListBox), destinationItems);

			}

		}

	}

}
