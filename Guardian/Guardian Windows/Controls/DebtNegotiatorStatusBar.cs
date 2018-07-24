namespace FluidTrade.Guardian.Windows
{
	using FluidTrade.Guardian.Windows.Controls;
	using FluidTrade.Actipro;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Windows.Controls;
	using System.Windows.Media;
	using System.Windows;
	using System.Threading;
	using System.Windows.Threading;

	/// <summary>
	/// The custom status bar for debt holders.
	/// </summary>
	public class DebtNegotiatorStatusBar : BlotterStatusBar
	{

		private delegate void SetStatusBarDelegate(int totalRecords, int matchedRecords);

		private TextBlock totalLabel;
		private TextBlock ratioLabel;

		/// <summary>
		/// Initialize the status bar.
		/// </summary>
		public DebtNegotiatorStatusBar()
			: base()
		{

			TextBlock totalCardsLabel = new TextBlock()
			{
				Text = "Total Credit Cards: ",
				FontSize = 12,
				Foreground = new SolidColorBrush(Colors.Gray),
				HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
				VerticalAlignment = System.Windows.VerticalAlignment.Center,
				Margin = new Thickness(5, 0, 10, 0)
			};
			TextBlock ratioLabel = new TextBlock()
			{
				Text = "Matched: ",
				FontSize = 12,
				Foreground = new SolidColorBrush(Colors.Gray),
				HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
				VerticalAlignment = System.Windows.VerticalAlignment.Center,
				Margin = new Thickness(5, 0, 10, 0)
			};

			this.totalLabel = new TextBlock()
			{
				FontSize = 12,
				HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
				VerticalAlignment = System.Windows.VerticalAlignment.Center,
				Margin = new Thickness(0, 0, 10, 2)
			};
			this.ratioLabel = new TextBlock()
			{
				FontSize = 12,
				HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
				VerticalAlignment = System.Windows.VerticalAlignment.Center,
				Margin = new Thickness(0, 0, 10, 2)
			};

			base.Children.Add(new FlowGridRow() { totalCardsLabel, this.totalLabel });
			base.Children.Add(new FlowGridRow() { ratioLabel, this.ratioLabel });

		}

		/// <summary>
		/// Count all of the working orders and matches in blotters below the parent entity. The caller should lock the DataModel.
		/// </summary>
		/// <param name="parent">The parent entity.</param>
		/// <param name="totalRecords">The total number of records. Should initially be 0.</param>
		/// <param name="matchedRecords">The number of of valid matches. Should initially be 0.</param>
		private void CountAccounts(EntityRow parent, ref int totalRecords, ref int matchedRecords)
		{

			BlotterRow blotter = DataModel.Blotter.BlotterKey.Find(parent.EntityId);

			if (blotter != null)
			{

				MatchRow[] matches = blotter.GetMatchRows();
				WorkingOrderRow[] orders = blotter.GetWorkingOrderRows();

				matchedRecords += matches.Length;

				// Count up the credit cards for everything.
				foreach (WorkingOrderRow order in orders)
				{

					Guid security = order.SecurityId;
					ConsumerTrustRow trust = DataModel.ConsumerTrust.ConsumerTrustKey.Find(security);

					if(trust != null)
						totalRecords += trust.ConsumerRow.GetCreditCardRows().Length;

				}

			}

			// Do the same for all of the blotters under this one.
			foreach (EntityTreeRow tree in DataModel.EntityTree)
				if (tree.ParentId == parent.EntityId)
					this.CountAccounts(tree.EntityRowByFK_Entity_EntityTree_ChildId, ref totalRecords, ref matchedRecords);

		}

		/// <summary>
		/// Initializes the status bar.
		/// </summary>
		/// <param name="totalRecords">The total number of records.</param>
		/// <param name="matchedRecords">The number of matched records.</param>
		private void Populate(int totalRecords, int matchedRecords)
		{

			this.totalLabel.Text = String.Format("{0}", totalRecords);
			this.ratioLabel.Text = totalRecords > 0 ? String.Format("{0:#0%}", ((double)matchedRecords) / ((double)totalRecords)) : "";

		}

		/// <summary>
		/// Background thread that initializes the status bar.
		/// </summary>
		protected override void Populate()
		{

			// Make sure the common properties are updated before addressing the extended properties.
			base.Populate();

			try
			{

				Boolean entityDeleted = false;
				int totalRecords = 0;
				int matchedRecords = 0;
				Guid blotterId = this.entityId;
				DebtRule rule = null;

				lock (DataModel.SyncRoot)
				{

					EntityRow entityRow = DataModel.Entity.EntityKey.Find(blotterId);

					if (entityRow != null)
					{

						CountAccounts(entityRow, ref totalRecords, ref matchedRecords);
						rule = DebtClass.GetDebtRule(this.entityId, entityRow.TypeId);

					}
					else
					{

						entityDeleted = true;

					}

				}

				if (!entityDeleted && rule != null)
				{

					this.Dispatcher.BeginInvoke(
						DispatcherPriority.Normal,
						new SetStatusBarDelegate(Populate),
						totalRecords,
						matchedRecords);

				}

			}
			catch
			{

				// If we fail, we'll just keep up the old status bar.

			}

		}

	}

}
