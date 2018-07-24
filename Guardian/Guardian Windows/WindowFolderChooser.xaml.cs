namespace FluidTrade.Guardian.Windows
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
	using FluidTrade.Core;
	using System.Windows.Threading;
	using FluidTrade.Guardian.TradingSupportReference;
	using System.Threading;

	/// <summary>
	/// Interaction logic for WindowFolderChooser.xaml
	/// </summary>
	public partial class WindowFolderChooser : Window
	{

		/// <summary>
		/// Indicates the Folder dependency property.
		/// </summary>
		public static readonly DependencyProperty FolderProperty = DependencyProperty.Register("Folder", typeof(Entity), typeof(WindowFolderChooser));

		/// <summary>
		/// Indicates the Folders dependency property.
		/// </summary>
		public static readonly DependencyProperty FoldersProperty = DependencyProperty.Register("Folders", typeof(List<FolderTreeNode>), typeof(WindowFolderChooser));

		/// <summary>
		/// Create a new folder chooser window.
		/// </summary>
		public WindowFolderChooser()
		{

			this.InitializeComponent();

		}

		/// <summary>
		/// The folder Entity that the user selected (if any).
		/// </summary>
		public Entity Folder
		{
			get { return this.GetValue(WindowFolderChooser.FolderProperty) as Entity; }
			set { this.SetValue(WindowFolderChooser.FolderProperty, value); }
		}

		/// <summary>
		/// The list of folders to select from.
		/// </summary>
		public List<FolderTreeNode> Folders
		{
			get { return this.GetValue(WindowFolderChooser.FoldersProperty) as List<FolderTreeNode>; }
			set { this.SetValue(WindowFolderChooser.FoldersProperty, value); }
		}

		/// <summary>
		/// Handle the Cancel event.
		/// </summary>
		/// <param name="sender">The cancel button.</param>
		/// <param name="eventArgs">The event arguments.</param>
		protected virtual void OnCancel(object sender, ExecutedRoutedEventArgs eventArgs)
		{

			this.DialogResult = false;
			this.Close();

		}

		/// <summary>
		/// Handle the Okay event.
		/// </summary>
		/// <param name="sender">The cancel button.</param>
		/// <param name="eventArgs">The event arguments.</param>
		protected virtual void OnOkay(object sender, ExecutedRoutedEventArgs eventArgs)
		{

			this.DialogResult = true;
			this.Close();

		}

	}

}
