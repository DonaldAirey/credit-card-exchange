namespace FluidTrade.Guardian.Windows
{

	using System.Collections.Generic;
	using System.Data;
	using System;
	using FluidTrade.Core;

	/// <summary>
	/// A self-updating list of debt rules available to a particular debt class.
	/// </summary>
	public class DebtRuleList : DataBoundList<DebtRule>
	{

		/// <summary>
		/// Keep a single comparer for all lists.
		/// </summary>
		private static DebtRuleList.CompareItems comparer = new DebtRuleList.CompareItems();

		private Guid blotterId;
		private List<Guid> blotterList;

		/// <summary>
		/// Create a new debt rule list.
		/// </summary>
		public DebtRuleList(Guid blotterId)
		{

			this.blotterId = blotterId;

			DataModel.EntityTree.RowChanged += this.OnTreeChanged;

			ThreadPoolHelper.QueueUserWorkItem(delegate(object data)
				{
					this.blotterList = this.GetBlotterList();
					this.InitializeList();
				});

		}

		/// <summary>
		/// The Comparer to use to compare to objects in the list.
		/// </summary>
		protected override IComparer<DebtRule> Comparer
		{
			get { return DebtRuleList.comparer; }
		}

		/// <summary>
		/// The DebtRule table.
		/// </summary>
		protected override DataTable Table
		{
			get { return DataModel.DebtRule; }
		}

		/// <summary>
		/// A comparer to compare two items by mnemonic.
		/// </summary>
		private class CompareItems : IComparer<DebtRule>
		{

			/// <summary>
			/// Compare two items.
			/// </summary>
			/// <param name="left">The left item.</param>
			/// <param name="right">The right item.</param>
			/// <returns>The relative order of the item.</returns>
			public int Compare(DebtRule left, DebtRule right)
			{

				return left.Name.CompareTo(right.Name);

			}

		}

		/// <summary>
		/// Removed datamodel triggers.
		/// </summary>
		public override void Dispose()
		{

			base.Dispose();
			DataModel.EntityTree.RowChanged -= this.OnTreeChanged;

		}

		/// <summary>
		/// Filter what rows are included in the list.
		/// </summary>
		/// <param name="row">The row to examine.</param>
		/// <returns>True if the row is included in the list, false if not.</returns>
		protected override bool Filter(DataRow row)
		{

			if (row is DebtRuleRow)
			{

				DebtRuleMapRow[] debtRuleMapRows = (row as DebtRuleRow).GetDebtRuleMapRows();

				foreach (DebtRuleMapRow map in debtRuleMapRows)
					if (this.blotterList.Contains(map.DebtClassId))
						return true;

			}
			else
			{

				throw new RowNotHandledException("row isn't the right kind of row");

			}

			return false;

		}

		/// <summary>
		/// Get the list of blotters we should be pulling debt rules from.
		/// </summary>
		/// <returns>The BlotterIds of the affected blotters.</returns>
		private List<Guid> GetBlotterList()
		{

			List<Guid> blotters = new List<Guid>();

			// This lock is mostly redundant, but needed for the initial call from the constructor.
			lock (DataModel.SyncRoot)
			{

				EntityRow blotter = DataModel.Entity.EntityKey.Find(this.blotterId);
				Guid type = blotter.TypeId;

				while (blotter.TypeId == type)
				{

					EntityTreeRow[] entityTree = blotter.GetEntityTreeRowsByFK_Entity_EntityTree_ChildId();

					blotters.Add(blotter.EntityId);

					if (entityTree.Length > 0)
						blotter = entityTree[0].EntityRowByFK_Entity_EntityTree_ParentId;

				}

			}

			return blotters;

		}

		/// <summary>
		/// Create a new item from a table row.
		/// </summary>
		/// <param name="dataRow">A row from the debt rule table.</param>
		/// <returns>A new DebtRule object.</returns>
		protected override DebtRule New(DataRow dataRow)
		{

			DebtRule item = null;

			if (dataRow is DebtRuleRow)
			{

				DebtRuleRow baseRow = dataRow as DebtRuleRow;

				item = new DebtRule(baseRow);

			}
			else
			{

				throw new RowNotHandledException("row isn't the right kind of row");

			}

			return item;

		}

		/// <summary>
		/// Update the blotter list when the tree changes.
		/// </summary>
		/// <param name="sender">The EntityTree table.</param>
		/// <param name="eventArgs">The event arguments.</param>
		protected void OnTreeChanged(object sender, DataRowChangeEventArgs eventArgs)
		{
			if (eventArgs.Action == DataRowAction.Commit && eventArgs.Row.RowState != DataRowState.Detached)
				this.blotterList = GetBlotterList();

		}

		/// <summary>
		/// Update a DebtRule object with another.
		/// </summary>
		/// <param name="old">The original object to update.</param>
		/// <param name="update">The new object to update with</param>
		protected override void Update(DebtRule old, DebtRule update)
		{

			old.Update(update);

		}

	}

}
