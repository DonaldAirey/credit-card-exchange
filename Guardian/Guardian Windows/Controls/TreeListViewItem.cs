namespace FluidTrade.Guardian.Windows.Controls
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Windows.Controls;
	using System.Windows;

	/// <summary>
	/// A container in 
	/// </summary>
	public class TreeListViewItem : TreeViewItem
	{

		private Int32 level = -1;

		static TreeListViewItem()
		{

			DefaultStyleKeyProperty.OverrideMetadata(typeof(TreeListViewItem), new FrameworkPropertyMetadata(typeof(TreeListViewItem)));

		}

		/// <summary>
		/// The depth of this node into the tree.
		/// </summary>
		public Int32 Level
		{

			get
			{

				if (this.level < 0)
				{

					TreeListViewItem parent = ItemsControl.ItemsControlFromItemContainer(this) as TreeListViewItem;

					this.level = parent != null ? parent.Level + 1 : 0;

				}

				return this.level;

			}

		}

		/// <summary>
		/// An object to use as an item in this tree.
		/// </summary>
		/// <returns>The new object.</returns>
		protected override System.Windows.DependencyObject GetContainerForItemOverride()
		{

			return new TreeListViewItem();

		}

		/// <summary>
		/// Determin whether an item in tree is actually one of display items.
		/// </summary>
		/// <param name="item">The item to check.</param>
		/// <returns>True if the item is a TreeListViewItem - false otherwise.</returns>
		protected override bool IsItemItsOwnContainerOverride(object item)
		{

			return item is TreeListViewItem;

		}

	}

}
