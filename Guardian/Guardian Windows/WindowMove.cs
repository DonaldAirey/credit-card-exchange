namespace FluidTrade.Guardian.Windows
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using FluidTrade.Core;
	using System.Windows.Threading;
	using System.Windows.Controls.Primitives;
	using System.Windows.Controls;
	using System.Windows;
	using System.Windows.Input;

	/// <summary>
	/// A window that prompts the user for where to move a set of objects and launches the WindowMoveProgress dialog with the acquired information.
	/// </summary>
	public class WindowMove : WindowFolderChooser
	{

		/// <summary>
		/// Indicates the MoveList dependency property.
		/// </summary>
		public static readonly DependencyProperty MoveListProperty =
			DependencyProperty.Register("MoveList", typeof(List<IMovableObject>), typeof(WindowMove));

		/// <summary>
		/// Create a new link folder window.
		/// </summary>
		public WindowMove()
			: base()
		{

			this.Title = Properties.Resources.MoveTitle;
			ThreadPoolHelper.QueueUserWorkItem(data => this.InitializeFolders());

		}

		/// <summary>
		/// The list of items to move.
		/// </summary>
		public List<IMovableObject> MoveList
		{

			get { return this.GetValue(WindowMove.MoveListProperty) as List<IMovableObject>; }
			set { this.SetValue(WindowMove.MoveListProperty, value); }

		}

		/// <summary>
		/// Expand the first folder in the list (like outlook).
		/// </summary>
		private void ExpandFolder()
		{

			if (this.folderSelector.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
			{

				EventHandler onStatusChanged = null;

				onStatusChanged = delegate(object s, EventArgs e)
				{
					if (this.folderSelector.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
					{

						this.folderSelector.ItemContainerGenerator.StatusChanged -= onStatusChanged;
						this.ExpandFolder();

					}
				};

				this.folderSelector.ItemContainerGenerator.StatusChanged += onStatusChanged;

			}
			else
			{

				TreeViewItem item = this.folderSelector.ItemContainerGenerator.ContainerFromIndex(0) as TreeViewItem;

				item.IsExpanded = true;

			}

		}

		/// <summary>
		/// Initialize the list of root folders.
		/// </summary>
		private void InitializeFolders()
		{

			lock (DataModel.SyncRoot)
			{

				LambdaComparer<FolderTreeNode> folderTreeNodeComparer = new LambdaComparer<FolderTreeNode>((l, r) => l.Entity.Name.CompareTo(r.Entity.Name));
				List<FolderTreeNode> folders = new List<FolderTreeNode>();
				EntityRow userEntityRow = DataModel.Entity.EntityKey.Find(UserContext.Instance.UserId);

				foreach (EntityTreeRow entityTreeRow in userEntityRow.GetEntityTreeRowsByFK_Entity_EntityTree_ParentId())
				{

					FolderTreeNode folderTreeNode = new FolderTreeNode(entityTreeRow);
					Int32 index = folders.BinarySearch(folderTreeNode, folderTreeNodeComparer);

					if (index < 0) index = ~index;

					folders.Insert(index, folderTreeNode);

				}

				this.Dispatcher.BeginInvoke(new Action(delegate()
					{
						this.Folders = folders;
						this.ExpandFolder();
					}), DispatcherPriority.Normal);

			}

		}

		/// <summary>
		/// Handle the Okay event.
		/// </summary>
		/// <param name="sender">The cancel button.</param>
		/// <param name="eventArgs">The event arguments.</param>
		protected override void OnOkay(object sender, ExecutedRoutedEventArgs eventArgs)
		{

			if (this.Folder != null)
			{
				this.DialogResult = true;
			}
			else
			{
				this.DialogResult = false;
			}

			this.Close();

		}

	}

}
