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
    public class DebtHolderStatusBar : BlotterStatusBar
    {

        private delegate void SetStatusBarDelegate(double matchedDollars, double totalDollars, double percentage, int totalRecords, int matchedRecords);

        private TextBlock totalLabel;
        private TextBlock matchedLabel;
        private TextBlock settledLabel;
        private TextBlock ratioLabel;

        /// <summary>
        /// Initialize the status bar.
        /// </summary>
        public DebtHolderStatusBar()
            : base()
        {

			TextBlock totalDollarsLabel = new TextBlock()
				{
					Text = "Total Value: ",
					FontSize = 12,
					Foreground = new SolidColorBrush(Colors.Gray),
					HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
					VerticalAlignment = System.Windows.VerticalAlignment.Center,
					Margin = new Thickness(5, 0, 10, 0)
				};
			TextBlock matchedDollarsLabel = new TextBlock()
				{
					Text = "Matched Value: ",
					FontSize = 12,
					Foreground = new SolidColorBrush(Colors.Gray),
					HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
					VerticalAlignment = System.Windows.VerticalAlignment.Center,
					Margin = new Thickness(5, 0, 10, 0)
				};
			TextBlock settledDollarsLabel = new TextBlock()
			{
				Text = "Minimum Settlement Value: ",
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
			this.matchedLabel = new TextBlock()
				{
					FontSize = 12,
					HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
					VerticalAlignment = System.Windows.VerticalAlignment.Center,
					Margin = new Thickness(0, 0, 10, 2)
				};
			this.settledLabel = new TextBlock()
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

            base.Children.Add(new FlowGridRow() { totalDollarsLabel, this.totalLabel });
            base.Children.Add(new FlowGridRow() { matchedDollarsLabel, this.matchedLabel });
            base.Children.Add(new FlowGridRow() { settledDollarsLabel, this.settledLabel });
			base.Children.Add(new FlowGridRow() { ratioLabel, this.ratioLabel });

        }

        /// <summary>
        /// Count all of the working orders and matches in blotters below the parent entity. The caller should lock the DataModel.
        /// </summary>
        /// <param name="parent">The parent entity.</param>
        /// <param name="matchedDollars">The total face value of the valid matches. Should initially be 0.0.</param>
        /// <param name="totalDollars">The total face value of all the records. Should initially be 0.0.</param>
		/// <param name="totalRecords">The total number of records. Should initially be 0.</param>
		/// <param name="matchedRecords">The number of of valid matches. Should initially be 0.</param>
		private void CountChildMoney(EntityRow parent, ref decimal matchedDollars, ref decimal totalDollars, ref int totalRecords, ref int matchedRecords)
        {

            BlotterRow blotter = DataModel.Blotter.BlotterKey.Find(parent.EntityId);

            if (blotter != null)
            {

				MatchRow[] matches = blotter.GetMatchRows();
				WorkingOrderRow[] orders = blotter.GetWorkingOrderRows();

				matchedRecords += matches.Length;
				totalRecords += orders.Length;

                // Count up the match/match with funds credit card balances.
                foreach (MatchRow match in matches)
                {

					if (match.StatusRow.StatusCode == Status.ValidMatch || match.StatusRow.StatusCode == Status.ValidMatchFunds)
                    {

                        Guid security = match.WorkingOrderRow.SecurityId;
                        matchedDollars += DataModel.ConsumerDebt.ConsumerDebtKey.Find(security).CreditCardRow.AccountBalance;

                    }

                }

                // Count up the credit card balances of _everything_, matched or not.
                foreach (WorkingOrderRow order in orders)
                {

                    Guid security = order.SecurityId;
                    ConsumerDebtRow debt = DataModel.ConsumerDebt.ConsumerDebtKey.Find(security);

                    if (debt != null)
                        totalDollars += debt.CreditCardRow.AccountBalance;

                }

            }

            // Do the same for all of the blotters under this one.
            foreach (EntityTreeRow tree in DataModel.EntityTree)
                if (tree.ParentId == parent.EntityId)
                    this.CountChildMoney(tree.EntityRowByFK_Entity_EntityTree_ChildId, ref matchedDollars, ref totalDollars, ref totalRecords, ref matchedRecords);

        }

        /// <summary>
        /// Initializes the status bar.
        /// </summary>
		/// <param name="matchedDollars">The total valid-match dollar amount.</param>
		/// <param name="totalDollars">The total dollar amount.</param>
		/// <param name="percentage">The settlement percentage.</param>
		/// <param name="totalRecords">The total number of records.</param>
		/// <param name="matchedRecords">The number of matched records.</param>
		private void Populate(double matchedDollars, double totalDollars, double percentage, int totalRecords, int matchedRecords)
        {

            this.totalLabel.Text = String.Format("{0:C}", totalDollars);
            this.matchedLabel.Text = String.Format("{0:C}", matchedDollars);
            this.settledLabel.Text = String.Format("{0:C}", matchedDollars * percentage);
			this.ratioLabel.Text = totalRecords > 0? String.Format("{0:#0%}", ((double)matchedRecords) / ((double)totalRecords)) : "";

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
                decimal matchedDollars = 0m;
                decimal totalDollars = 0m;
				int totalRecords = 0;
				int matchedRecords = 0;
                Guid blotterId = this.entityId;
				DebtRule rule = null;

				lock (DataModel.SyncRoot)
				{

					EntityRow entityRow = DataModel.Entity.EntityKey.Find(blotterId);

					if (entityRow != null)
					{

						CountChildMoney(entityRow, ref matchedDollars, ref totalDollars, ref totalRecords, ref matchedRecords);
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
						Decimal.ToDouble(matchedDollars),
						Decimal.ToDouble(totalDollars),
						Decimal.ToDouble(rule.SettlementValue),
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
