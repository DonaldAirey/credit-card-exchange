namespace FluidTrade.Guardian.Windows
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Windows;
	using FluidTrade.Core;
	using FluidTrade.Guardian.TradingSupportReference;
	using System.Threading;
	using System.Windows.Threading;
	using System.ServiceModel;
	using System.Windows.Input;

	class WindowUnlinkFolder : WindowFolderChooser
	{
		/// <summary>
		/// Indicates the SelectedFolder dependency property.
		/// </summary>
		public static readonly DependencyProperty ParentFolderProperty = DependencyProperty.Register("ParentFolder", typeof(Entity), typeof(WindowUnlinkFolder), new PropertyMetadata(OnParentFolderChanged));

		/// <summary>
		/// Create a new link folder window.
		/// </summary>
		public WindowUnlinkFolder()
			: base()
		{


		}

		/// <summary>
		/// Gets or sets the parent folder.
		/// </summary>
		public Entity ParentFolder
		{
			get { return this.GetValue(WindowUnlinkFolder.ParentFolderProperty) as Entity; }
			set { this.SetValue(WindowUnlinkFolder.ParentFolderProperty, value); }
		}

		/// <summary>
		/// Commit the new link to the database.
		/// </summary>
		/// <param name="parent">The parent folder.</param>
		/// <param name="child">The child folder.</param>
		private void Commit(Entity parent, Entity child)
		{

			try
			{

				lock (DataModel.SyncRoot)
				{

					EntityTreeRow entityTreeRow = DataModel.EntityTree.EntityTreeKeyChildIdParentId.Find(child.EntityId, parent.EntityId);
					TradingSupportClient client = new TradingSupportClient(Properties.Settings.Default.TradingSupportEndpoint);
					MethodResponseErrorCode response = client.DeleteEntityTree(
						new EntityTree[] { new EntityTree() { RowId = entityTreeRow.EntityTreeId, RowVersion = entityTreeRow.RowVersion } });

					if (!response.IsSuccessful)
						if (response.Errors.Length > 0)
							GuardianObject.ThrowErrorInfo(response.Errors[0]);
						else
							GuardianObject.ThrowErrorInfo(response.Result);

					client.Close();

				}

			}
			catch (FaultException<RecordNotFoundFault> exception)
			{

				EventLog.Warning("{0}: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);

			}
			catch (Exception exception)
			{

				EventLog.Error("{0}: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);
				Application.Current.Dispatcher.BeginInvoke(new WaitCallback(data =>
					MessageBox.Show(
						Application.Current.MainWindow,
						String.Format(FluidTrade.Guardian.Properties.Resources.UnlinkFolderFailed, child, parent),
						Application.Current.MainWindow.Title)),
					DispatcherPriority.Normal);

			}

		}

		/// <summary>
		/// Initialize the list of root folders.
		/// </summary>
		private void InitializeFolders(Guid parentId)
		{

			lock (DataModel.SyncRoot)
			{

				LambdaComparer<FolderTreeNode> folderTreeNodeComparer = new LambdaComparer<FolderTreeNode>((l, r) => l.Entity.Name.CompareTo(r.Entity.Name));
				List<FolderTreeNode> folders = new List<FolderTreeNode>();
				EntityRow parentEntityRow = DataModel.Entity.EntityKey.Find(parentId);

				foreach (EntityTreeRow entityTreeRow in parentEntityRow.GetEntityTreeRowsByFK_Entity_EntityTree_ParentId())
				{

					FolderTreeNode folderTreeNode = new FolderTreeNode() { Entity = new Entity(entityTreeRow.EntityRowByFK_Entity_EntityTree_ChildId) };
					Int32 index = folders.BinarySearch(folderTreeNode, folderTreeNodeComparer);

					if (index < 0) index = ~index;

					folders.Insert(index, folderTreeNode);

				}

				this.Dispatcher.BeginInvoke(new Action(() => this.Folders = folders), DispatcherPriority.Normal);

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

				Entity parent = this.ParentFolder.Clone() as Entity;
				Entity child = this.Folder.Clone() as Entity;
				ThreadPoolHelper.QueueUserWorkItem(data => this.Commit(parent, child));
				this.DialogResult = true;

			}
			else
			{

				this.DialogResult = false;

			}

			this.Close();

		}

		/// <summary>
		/// Handle the parent folder changing.
		/// </summary>
		/// <param name="sender">The unlink folder window.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private static void OnParentFolderChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			WindowUnlinkFolder chooser = sender as WindowUnlinkFolder;

			if (chooser.ParentFolder != null)
				ThreadPoolHelper.QueueUserWorkItem(data => chooser.InitializeFolders((Guid)data), chooser.ParentFolder.EntityId);
			else
				chooser.Folders = null;

		}

	}

}
