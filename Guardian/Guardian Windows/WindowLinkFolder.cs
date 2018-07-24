namespace FluidTrade.Guardian.Windows
{
	using System;
	using System.Linq;
	using System.Threading;
	using System.Windows;
	using System.Windows.Threading;
	using FluidTrade.Core;
	using FluidTrade.Guardian.TradingSupportReference;
	using System.Collections.Generic;
	using System.Windows.Input;
	using System.ServiceModel;

	/// <summary>
	/// A window for selecting (and then adding) a blotter/entity to link to from another entity.
	/// </summary>
	public class WindowLinkFolder : WindowFolderChooser
	{

		/// <summary>
		/// Indicates the SelectedFolder dependency property.
		/// </summary>
		public static readonly DependencyProperty ParentFolderProperty = DependencyProperty.Register("ParentFolder", typeof(Entity), typeof(WindowLinkFolder));

		/// <summary>
		/// Create a new link folder window.
		/// </summary>
		public WindowLinkFolder() : base()
		{

			ThreadPoolHelper.QueueUserWorkItem(data => this.InitializeFolders());

		}

		/// <summary>
		/// Gets or sets the parent folder.
		/// </summary>
		public Entity ParentFolder
		{
			get { return this.GetValue(WindowLinkFolder.ParentFolderProperty) as Entity; }
			set { this.SetValue(WindowLinkFolder.ParentFolderProperty, value); }
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

				TradingSupportClient client = new TradingSupportClient(Properties.Settings.Default.TradingSupportEndpoint);
				MethodResponseArrayOfguid response = client.CreateEntityTree(new EntityTree[] { new EntityTree() { ChildId = child.EntityId, ParentId = parent.EntityId } });

				if (!response.IsSuccessful)
					if (response.Errors.Length > 0)
						GuardianObject.ThrowErrorInfo(response.Errors[0]);
					else
						Application.Current.Dispatcher.BeginInvoke(new WaitCallback(data =>
							MessageBox.Show(
								Application.Current.MainWindow,
								String.Format(FluidTrade.Guardian.Properties.Resources.LinkFolderFailed, data),
								Application.Current.MainWindow.Title)),
							DispatcherPriority.Normal,
							child);

				client.Close();

			}
			catch (FaultException<RecordExistsFault> exception)
			{

				EventLog.Warning("{0}: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);
				Application.Current.Dispatcher.BeginInvoke(new WaitCallback(data =>
					MessageBox.Show(
						Application.Current.MainWindow,
						String.Format(FluidTrade.Guardian.Properties.Resources.LinkFolderFailedRecordExists, data),
						Application.Current.MainWindow.Title)),
					DispatcherPriority.Normal,
					child);

			}
			catch (Exception exception)
			{

				EventLog.Error("{0}: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);
				Application.Current.Dispatcher.BeginInvoke(new WaitCallback(data =>
					MessageBox.Show(
						Application.Current.MainWindow,
						String.Format(FluidTrade.Guardian.Properties.Resources.LinkFolderFailed, data),
						Application.Current.MainWindow.Title)),
					DispatcherPriority.Normal,
					child);

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
				var entities = DataModel.Entity.Where(row =>
					(row.GetFolderRows().Length > 0 || row.GetBlotterRows().Length > 0));

				foreach (EntityRow entityRow in entities)
				{

					Boolean topLevel = true;

					// Determine whether a folder/blotter has any parent folders or blotters.
					foreach (EntityTreeRow entityTreeRow in entityRow.GetEntityTreeRowsByFK_Entity_EntityTree_ChildId())
					{

						EntityRow parent = entityTreeRow.EntityRowByFK_Entity_EntityTree_ParentId;

						if (parent.GetFolderRows().Length > 0 || parent.GetBlotterRows().Length > 0)
						{

							topLevel = false;
							break;

						}

					}

					// If they don't add them to our list.
					if (topLevel)
					{

						FolderTreeNode folderTreeNode = new FolderTreeNode(entityRow);
						Int32 index = folders.BinarySearch(folderTreeNode, folderTreeNodeComparer);

						if (index < 0) index = ~index;

						folders.Insert(index, folderTreeNode);

					}

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

	}

}
