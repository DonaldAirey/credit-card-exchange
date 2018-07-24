namespace FluidTrade.Guardian.Windows.Controls
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Data;
	using System.Windows.Documents;
	using System.Windows.Input;
	using System.Windows.Media;
	using System.Windows.Media.Imaging;
	using System.Windows.Navigation;
	using System.Windows.Shapes;
	using System.Diagnostics;
	using FluidTrade.Core;
	using FluidTrade.Guardian.Windows;
	using System.Windows.Threading;
	using System.IO;
	using System.Threading;
	using Microsoft.Win32;

	/// <summary>
	/// Interaction logic for EditSettlementTemplateControl.xaml
	/// </summary>
	public partial class EditSettlementTemplateControl : UserControl
	{

		private User currentUser;

		/// <summary>
		/// Indicates the DebtClass dependency property.
		/// </summary>
		public static readonly DependencyProperty DebtClassProperty =
			DependencyProperty.Register("DebtClass", typeof(DebtClass), typeof(EditSettlementTemplateControl), new PropertyMetadata());

	
		/// <summary>
		/// Build new settlement template control.
		/// </summary>
		public EditSettlementTemplateControl()
		{

			InitializeComponent();
			ThreadPoolHelper.QueueUserWorkItem(data =>
				this.InitializeUser());

		}

		/// <summary>
		/// Gets or sets the the DebtClass we're currently editing.
		/// </summary>
		public DebtClass DebtClass
		{

			set { this.SetValue(EditSettlementTemplateControl.DebtClassProperty, value); }
			get { return this.GetValue(EditSettlementTemplateControl.DebtClassProperty) as DebtClass; }

		}

		/// <summary>
		/// Determine whether the Open command can be processed.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="eventArgs"></param>
		private void CanOpen(object sender, CanExecuteRoutedEventArgs eventArgs)
		{

			if (this.CanUpdateTemplate())
				this.review.Content = Properties.Resources.TemplateReviewEdit;

			eventArgs.CanExecute =
				this.DebtClass != null &&
				!String.IsNullOrEmpty(this.DebtClass.SettlementTemplate);


		}

		/// <summary>
		/// Determine whether the New command can be processed.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="eventArgs"></param>
		private void CanNew(object sender, CanExecuteRoutedEventArgs eventArgs)
		{

			eventArgs.CanExecute = this.CanUpdateTemplate();

		}

		/// <summary>
		/// Determine whether the current user can update the template.
		/// </summary>
		/// <returns></returns>
		private Boolean CanUpdateTemplate()
		{

			return this.currentUser != null &&
				this.currentUser.Groups.Any(g =>
					g.GroupType == GroupType.FluidTradeAdmin || g.GroupType == GroupType.ExchangeAdmin);

		}

		/// <summary>
		/// Find the window that contains this control.
		/// </summary>
		/// <returns></returns>
		private Window GetContainerWindow()
		{

			DependencyObject parent = this.Parent;

			do
			{

				parent = LogicalTreeHelper.GetParent(parent);

			} while (!(parent is Window));

			return parent as Window;

		}

		/// <summary>
		/// Get information about the current user.
		/// </summary>
		private void InitializeUser()
		{

			User user = Entity.New(DataModel.Entity.EntityKey.Find(Information.UserId)) as User;

			this.Dispatcher.BeginInvoke(new Action(() =>
				this.currentUser = user),
				DispatcherPriority.Normal);
	
		}

		/// <summary>
		/// Handle the New command.
		/// </summary>
		/// <param name="sender">The Set New Template button.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnNew(object sender, ExecutedRoutedEventArgs eventArgs)
		{

			OpenFileDialog fileDialog = new OpenFileDialog();

			fileDialog.CheckFileExists = true;
			fileDialog.DefaultExt = "docx";
			fileDialog.DereferenceLinks = true;
			fileDialog.Filter = "Microsoft Word Template (*.docx)|*.docx;";
			fileDialog.Multiselect = false;
			fileDialog.ShowReadOnly = false;
			fileDialog.Title = FluidTrade.Guardian.Properties.Resources.OpenSettlementTemplateTitle;

			if (fileDialog.ShowDialog(this.GetContainerWindow()) == true)
			{

				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(delegate(object data)
					{
						try
						{

							this.LoadTemplateFile(data as String);

						}
						catch (Exception exception)
						{
							Core.EventLog.Warning("{0}: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);
							this.Dispatcher.BeginInvoke(new Action(delegate ()
								{
									Window parent = this.GetContainerWindow();
									MessageBox.Show(parent, Properties.Resources.OperationFailed, parent.Name);
								}),
								DispatcherPriority.Normal);

						}
					},
					fileDialog.FileName);

			}

		}

		/// <summary>
		/// Handle the Open command.
		/// </summary>
		/// <param name="sender">The Review Template button.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnOpen(object sender, ExecutedRoutedEventArgs eventArgs)
		{

			FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(data => this.LaunchTemplate(data as String), this.DebtClass.SettlementTemplate);

		}

		/// <summary>
		/// Launch a template in to be viewed in the default application.
		/// </summary>
		/// <param name="template">The base64 encoded template contents.</param>
		private void LaunchTemplate(String template)
		{

			String tempFileName = null;
			FileStream tempFileLock = null;

			try
			{

				Byte[] document = Convert.FromBase64String(template);
				Process docViewer;
				DateTime creationTime;
				DateTime modifiedTime;

				// This is not guaranteed unique, but statistically unlikely to not be unique. From what I can tell, .Net provides no way to get a
				// guaranteed unique temporary filename with a particular extension.
				tempFileName = System.IO.Path.GetTempPath() + Guid.NewGuid() + ".docx";
				using (FileStream tempFile = new FileStream(tempFileName, FileMode.CreateNew, FileAccess.Write, FileShare.None))
				{

					tempFile.Write(document, 0, document.Length);
					tempFile.Close();

				}

				// As a big hint to the user, lock up the temp file so they can't event overwrite it unless they can modify the template.
				if (!this.CanUpdateTemplate())
					tempFileLock = new FileStream(tempFileName, FileMode.Open, FileAccess.Read, FileShare.Read);

				creationTime = File.GetLastWriteTime(tempFileName);

				docViewer = Process.Start(tempFileName);
				docViewer.WaitForExit();
				modifiedTime = File.GetLastWriteTime(tempFileName);

				if (modifiedTime > creationTime && this.CanUpdateTemplate())
					this.LoadTemplateFile(tempFileName);

			}
			catch (Exception exception)
			{

				Core.EventLog.Warning("{0}: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);
				this.Dispatcher.BeginInvoke(new Action(delegate ()
					{
						Window parent = this.GetContainerWindow();
						MessageBox.Show(parent, Properties.Resources.OperationFailed, parent.Name);
					}),
					DispatcherPriority.Normal);

			}
			finally
			{

				try
				{

					if (tempFileLock != null)
					{

						tempFileLock.Close();
						tempFileLock.Dispose();

					}

					if (tempFileName != null)
						System.IO.File.Delete(tempFileName);

				}
				catch (Exception exception)
				{

					Core.EventLog.Information("{0}: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);

				}

			}

		}

		/// <summary>
		/// Read in changes made by a "viewer" to a template.
		/// </summary>
		/// <param name="tempFileName">The name of the temporary file.</param>
		private void LoadTemplateFile(String tempFileName)
		{

			using (FileStream tempFile = new FileStream(tempFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
			{

				Byte[] document;
				String template;

				document = new Byte[tempFile.Length];
				tempFile.Read(document, 0, document.Length);
				tempFile.Close();
				template = Convert.ToBase64String(document);

				this.Dispatcher.BeginInvoke(new Action(() => this.DebtClass.SettlementTemplate = template), DispatcherPriority.Normal);

			}

		}

	}

}
