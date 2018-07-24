namespace FluidTrade.Guardian.Windows.Controls
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Windows.Controls;
	using System.Windows;
	
	/// <summary>
	/// A tree view with columns.
	/// </summary>
	public class TreeListView : TreeView
	{

		/// <summary>
		/// Indicates the View dependency property.
		/// </summary>
		public static readonly DependencyProperty ViewProperty = DependencyProperty.Register(
			"View",
			typeof(GridView),
			typeof(TreeListView),
			new PropertyMetadata(null));

		static TreeListView()
		{

			DefaultStyleKeyProperty.OverrideMetadata(typeof(TreeListView), new FrameworkPropertyMetadata(typeof(TreeListView)));

		}

		/// <summary>
		/// The current view.
		/// </summary>
		public GridView View
		{
			get { return this.GetValue(TreeListView.ViewProperty) as GridView; }
			set { this.SetValue(TreeListView.ViewProperty, value); }
		}

		/// <summary>
		/// An object to use as an item in this tree.
		/// </summary>
		/// <returns>The new object.</returns>
		protected override DependencyObject GetContainerForItemOverride()
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
