namespace FluidTrade.Guardian
{

	using Microsoft.Win32;
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Data;
	using System.Windows.Input;
	using System.Windows.Media;
	using System.Windows.Media.Imaging;
	using FluidTrade.Guardian.Windows.Controls;
	using FluidTrade.Guardian.Windows;

	/// <summary>
	/// Prompts the user for information about how to generate orders.
	/// </summary>
	public partial class WindowOrder : Window
	{

		public WindowOrder()
		{

			InitializeComponent();

			this.blotterComboBox.SelectionChanged += new SelectionChangedEventHandler(blotterSelectionChanged);
		}

		void blotterSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			BlotterItem  item = e.AddedItems[0] as BlotterItem;
			if (item != null)
				this.blotterComboBox.SelectedValue = item.BlotterId;
		}

		public Guid BlotterId
		{
			get { return this.blotterComboBox.SelectedValue; }
			set { this.blotterComboBox.SelectedValue = value; }
		}

		public string FileName
		{
			get { return this.textBoxFileName.Text; }
			set { this.textBoxFileName.Text = value; }
		}

		public Int32 OrderCount
		{
			get { return Convert.ToInt32(this.textBoxOrders.Text); }
			set { this.textBoxOrders.Text = Convert.ToString(value); }
		}

		private void OnBrowseClick(object sender, RoutedEventArgs e)
		{

			// Configure the 'Open File' dialog box to look for the available XML files.
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.DefaultExt = ".xml";
			saveFileDialog.Filter = "XML Documents (.xml)|*.xml";

			// Show open file dialog box
			Nullable<bool> result = saveFileDialog.ShowDialog();
			if (result == true)
				this.textBoxFileName.Text = saveFileDialog.FileName;

		}

		private void OnCancelButtonClick(object sender, RoutedEventArgs e)
		{

			this.DialogResult = false;
			this.Close();

		}

		private void OnOkButtonClick(object sender, RoutedEventArgs e)
		{

			this.DialogResult = true;
			this.Close();

		}

	}

}
