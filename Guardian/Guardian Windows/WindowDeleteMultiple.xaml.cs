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

	/// <summary>
	/// Interaction logic for WindowDeleteMultiple.xaml
	/// </summary>
	public partial class WindowDeleteMultiple : Window
	{

		/// <summary>
		/// Identifies the Count dependency property.
		/// </summary>
		public static DependencyPropertyKey CountProperty =
			DependencyProperty.RegisterReadOnly("Count", typeof(Int32), typeof(WindowDeleteMultiple), new PropertyMetadata(0));
		/// <summary>
		/// Identifies the Count dependency property.
		/// </summary>
		public static DependencyProperty DeleteListProperty = DependencyProperty.Register(
			"DeleteList",
			typeof(List<GuardianObject>),
			typeof(WindowDeleteMultiple),
			new PropertyMetadata() { PropertyChangedCallback = OnDeleteListChanged });

		/// <summary>
		/// Create a new delete-multiple window.
		/// </summary>
		public WindowDeleteMultiple()
		{

			InitializeComponent();
			this.Owner = Application.Current.MainWindow;

		}

		/// <summary>
		/// The number of items to be deleted.
		/// </summary>
		public Int32 Count
		{
			get { return this.DeleteList == null? 0 : this.DeleteList.Count; }
		}

		/// <summary>
		/// The list of items to delete.
		/// </summary>
		public List<GuardianObject> DeleteList
		{
			get { return (List<GuardianObject>)this.GetValue(WindowDeleteMultiple.DeleteListProperty); }
			set { this.SetValue(WindowDeleteMultiple.DeleteListProperty, value); }
		}
				/// <summary>
		/// Abort the delete thread and close the window.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="eventArgs"></param>
		private void OnCancel(object sender, EventArgs eventArgs)
		{

			this.Close();

		}

		/// <summary>
		/// Handle a change of DeleteList.
		/// </summary>
		/// <param name="sender">The WindowDeleteMultiple whose DeleteList changed.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private static void OnDeleteListChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			sender.SetValue(WindowDeleteMultiple.CountProperty, (eventArgs.NewValue as List<GuardianObject>).Count);

		}

		/// <summary>
		/// Handles clicking of the 'Yes' button. Delete the target entity and related entities etc. and close the window.
		/// </summary>
		/// <param name="sender">The 'Yes' button.</param>
		/// <param name="arguments">Parameters for the event.</param>
		private void OnOkay(object sender, RoutedEventArgs arguments)
		{

			WindowDeleteProgress deleteEntity = new WindowDeleteProgress();

			deleteEntity.DeleteList = this.DeleteList;
			this.Close();
			deleteEntity.Show();

		}

	}

}
