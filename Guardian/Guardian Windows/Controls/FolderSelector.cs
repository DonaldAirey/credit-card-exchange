namespace FluidTrade.Guardian.Windows.Controls
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Windows.Controls;
	using System.Windows;
	using FluidTrade.Guardian.Windows;
	using System.Windows.Threading;
	using System.Threading;
	using System.Windows.Data;

	/// <summary>
	/// Selector for entites.
	/// </summary>
	public class FolderSelector : TreeView
	{

		/// <summary>
		/// Indicates the Folders depenendency property.
		/// </summary>
		public static readonly DependencyProperty FoldersProperty = DependencyProperty.Register("Folders", typeof(List<FolderTreeNode>), typeof(FolderSelector));
		/// <summary>
		/// Indicates the SelectedFolder depenendency property.
		/// </summary>
		public static readonly DependencyProperty SelectedFolderProperty = DependencyProperty.Register("SelectedFolder", typeof(Entity), typeof(FolderSelector));

		static FolderSelector()
		{

			DefaultStyleKeyProperty.OverrideMetadata(typeof(FolderSelector), new FrameworkPropertyMetadata(typeof(FolderSelector)));

		}

		/// <summary>
		/// Create a new selector.
		/// </summary>
		public FolderSelector()
		{

			this.SelectedValuePath = "Entity";
			this.Folders = new List<FolderTreeNode>();
			this.SetBinding(FolderSelector.ItemsSourceProperty, new Binding("Folders") { Source = this });
			this.SelectedItemChanged += (s, e) => this.SelectedFolder = this.SelectedValue as Entity;

		}

		/// <summary>
		/// The base list of "parent" folders to display.
		/// </summary>
		public List<FolderTreeNode> Folders
		{
			get { return this.GetValue(FolderSelector.FoldersProperty) as List<FolderTreeNode>; }
			set { this.SetValue(FolderSelector.FoldersProperty, value); }
		}

		/// <summary>
		/// The currently selected folder. This property is readonly - it has a mutator only so it can be bound to from Xaml.
		/// </summary>
		public Entity SelectedFolder
		{
			get { return this.GetValue(FolderSelector.SelectedFolderProperty) as Entity; }
			set { this.SetValue(FolderSelector.SelectedFolderProperty, value); }
		}

	}

}
