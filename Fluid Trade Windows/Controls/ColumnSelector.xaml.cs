namespace FluidTrade.Core.Windows.Controls
{

    using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Collections.Specialized;
	using System.Linq;
	using System.Windows;
	using System.Windows.Controls;

	/// <summary>
	/// Interaction logic for ColumnSelector.xaml
	/// </summary>
	public partial class ColumnSelector : System.Windows.Window
	{

		// Private Instance Fields
		private ReportColumn[] reportColumns;
		private System.Collections.Generic.List<ReportField> fieldDefinitions;
		private System.Collections.ObjectModel.ObservableCollection<ReportField> availableFields;
		private System.Collections.ObjectModel.ObservableCollection<ReportField> displayedFields;

		/// <summary>
		/// Create a dialog for selecting the columns in the report.
		/// </summary>
		public ColumnSelector()
		{

			// This list contains the fields that haven't been chosen for display in the report.
			this.availableFields = new ObservableCollection<ReportField>();

			// This list contains the field that have been chosen to be displayed in the report.  When any of the columns that are 
			// displayed are removed from the 'Display' list, they must be put back in the 'Available' list.  The opposite is true
			// for a column that is added to the 'Display' list: it must be removed from the 'Available' list.
			this.displayedFields = new ObservableCollection<ReportField>();
			this.displayedFields.CollectionChanged += new NotifyCollectionChangedEventHandler(ChangedCollection);

			// This will initialize the components declared in the XAML file.
			InitializeComponent();

			// The state of the buttons will change depending on the items available and the items selected in the lists.
			this.listBoxDisplayedFields.SelectionChanged += new SelectionChangedEventHandler(ChangedSelection);
			this.listBoxAvailableFields.SelectionChanged += new SelectionChangedEventHandler(ChangedSelection);

			// The observable collections are used to manipulate the contents of the list boxes.
			this.listBoxAvailableFields.ItemsSource = this.availableFields;
			this.listBoxDisplayedFields.ItemsSource = this.displayedFields;

		}

		/// <summary>
		/// Gets or sets the list of fields that are available for this report.
		/// </summary>
		public List<ReportField> FieldDefinitions
		{

			get { return this.fieldDefinitions; }

			set
			{

				// This is the original list of available fields.  The list that is managed by the list box contains this set of
				// fields less the displayed fields.
				this.fieldDefinitions = value;

				// Clear out the previous contents of the list and update it with any fields that are not displayed.
				this.availableFields.Clear();
				foreach (ReportField fieldDefinition in this.fieldDefinitions)
					if (!IsDisplayed(fieldDefinition.ColumnId))
						this.availableFields.Add(fieldDefinition);

				// The buttons need to reflect the new state of the list box.
				UpdateButtonStatus();

			}

		}

		public List<ReportField> DisplayedFields
		{
			get
			{
				List<ReportField> displayedFields = new List<ReportField>();
				foreach (ReportField fieldDefinition in this.displayedFields)
					displayedFields.Add(fieldDefinition);
				return displayedFields;
			}
		}

		/// <summary>
		/// Gets or sets the list of columns that are displayed in the report.
		/// </summary>
		public ReportColumn[] ColumnDefinitions
		{

			get
			{

				// These are the columns that were used to initialize the set of selected fields.
				return this.reportColumns;

			}

			set
			{

				// This list is kept around in the unlikely event it needs to be examined to see how the dialog box was 
				// initialized.
				this.reportColumns = value;

				// Populate the list box with the given items.  A trigger will remove fields from the 'Available' list as they are
				// added to the 'Display' list.
				foreach (ReportField displayedField in
					from ReportColumn reportColumn in this.reportColumns
					join ReportField fieldDefinition in this.fieldDefinitions
					on reportColumn.ColumnId equals fieldDefinition.ColumnId
					select fieldDefinition)
					if (!this.displayedFields.Contains(displayedField))
						this.displayedFields.Add(displayedField);

				// The buttons need to reflect the new state of the list box.
				UpdateButtonStatus();

			}

		}

		/// <summary>
		/// Update the status of the buttons based on the state of the list boxes and selected items.
		/// </summary>
		private void UpdateButtonStatus()
		{

			// The 'Move Up' function is only enabled when a single item is selected that is not already at the top of the list 
			// box.
			this.buttonMoveUp.IsEnabled = this.listBoxDisplayedFields.SelectedItems.Count == 1 &&
				this.listBoxDisplayedFields.SelectedIndex > 0;

			// The 'Move Down' function is only enabled when a single item is selected that is not already at the bottom of the 
			// list box.
			this.buttonMoveDown.IsEnabled = this.listBoxDisplayedFields.SelectedItems.Count == 1 &&
				this.listBoxDisplayedFields.SelectedIndex < this.displayedFields.Count - 1;

			// Fields can be added to the report when one or more available fields are selected.
			this.buttonAdd.IsEnabled = this.listBoxAvailableFields.SelectedItems.Count > 0;

			// Fields can be removed from the report when one or more displayed columns are selected.
			this.buttonRemove.IsEnabled = this.listBoxDisplayedFields.SelectedItems.Count > 0;

		}

		/// <summary>
		/// Handles a change to the selected items in the 'Display' list box.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
		private void ChangedSelection(object sender, SelectionChangedEventArgs e)
		{

			// This will update the status of the buttons when the selection changes.
			UpdateButtonStatus();

		}

		/// <summary>
		/// Handles a change to the collection of displayed fields.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
		void ChangedCollection(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{

			// This determins how the action is handled.
			switch (e.Action)
			{

			case NotifyCollectionChangedAction.Remove:

				// A field is available if it is not in the displayed report.
				foreach (ReportField oldFieldDefinition in e.OldItems)
					foreach (ReportField newFieldDefinition in this.fieldDefinitions)
						if (newFieldDefinition.ColumnId == oldFieldDefinition.ColumnId)
							this.availableFields.Add(newFieldDefinition);
				break;

			case NotifyCollectionChangedAction.Add:

				// A field is no longer available when a column is added to the report.
				foreach (ReportField newFieldDefinition in e.NewItems)
					foreach (ReportField oldFieldDefinition in this.fieldDefinitions)
						if (oldFieldDefinition.ColumnId == newFieldDefinition.ColumnId)
							this.availableFields.Remove(oldFieldDefinition);
				break;

			}

		}

		/// <summary>
		/// Gets an indication of whether the displayed columns contain a given field based on the column identifier.
		/// </summary>
		/// <param name="columnId">The column identifier.</param>
		/// <returns>true if the item is displayed.</returns>
		private bool IsDisplayed(string columnId)
		{

			// Test to see if the given column name is found in the list of displayed columns.
			foreach (ReportField fieldDefinition in this.displayedFields)
				if (columnId == fieldDefinition.ColumnId)
					return true;

			// At this point, the column is not displayed.
			return false;

		}

		/// <summary>
		/// Adds the selected fields to the report.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
		private void OnAdd(object sender, RoutedEventArgs routedEventArgs)
		{

			// This will move the selected items from the 'Available' list to the 'Displayed' list.  Note that there is a trigger
			// on the 'Display' list that removes items from the 'Available' list, so first a copy must be made of the selected
			// items before they are moved otherwise an error occurs because the collection used for the iteration is modified.
			List<ReportField> selectedItems = new List<ReportField>();
			foreach (ReportField fieldDefinition in this.listBoxAvailableFields.SelectedItems)
				selectedItems.Add(fieldDefinition);

			// This creates new column definitions from the selected fields and adds them to the 'Display' list.
			foreach (ReportField fieldDefinition in selectedItems)
				this.displayedFields.Add(fieldDefinition);

			// This will update the status of the buttons when the list changes.
			UpdateButtonStatus();

		}

		/// <summary>
		/// Removes the selected columns from the report.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
		private void OnRemove(object sender, RoutedEventArgs routedEventArgs)
		{

			// This will make a list of the selected items in the 'Display' list box before removing them from the list box.
			List<ReportField> selectedItems = new List<ReportField>();
			foreach (ReportField fieldDefinition in this.listBoxDisplayedFields.SelectedItems)
				selectedItems.Add(fieldDefinition);
			foreach (ReportField fieldDefinition in selectedItems)
				this.displayedFields.Remove(fieldDefinition);

			// This will update the status of the buttons when the list changes.
			UpdateButtonStatus();

		}

		/// <summary>
		/// Moves a column towards the front of the report.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
		private void OnMoveUp(object sender, RoutedEventArgs routedEventArgs)
		{

			// Move the column up in the order of the report if it isn't already at the beginning.
			int selectedIndex = this.listBoxDisplayedFields.SelectedIndex;
			if (selectedIndex > 0)
				this.displayedFields.Move(selectedIndex, selectedIndex - 1);

			// This insures that the item moved will stay in view.
			this.listBoxDisplayedFields.ScrollIntoView(this.listBoxDisplayedFields.SelectedItem);

			// This will update the status of the buttons when the selection changes.
			UpdateButtonStatus();

		}

		/// <summary>
		/// Moves a column towards the end of the report.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
		private void OnMoveDown(object sender, RoutedEventArgs routedEventArgs)
		{

			// Move the column down in the order of the report if it isn't already at the end.
			int selectedIndex = this.listBoxDisplayedFields.SelectedIndex;
			if (selectedIndex < this.displayedFields.Count - 1)
				this.displayedFields.Move(selectedIndex, selectedIndex + 1);

			// This insures that the item moved will stay in view.
			this.listBoxDisplayedFields.ScrollIntoView(this.listBoxDisplayedFields.SelectedItem);

			// This will update the status of the buttons when the selection changes.
			UpdateButtonStatus();

		}

		/// <summary>
		/// Successfully terminates the dialog.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
		private void OnOK(object sender, RoutedEventArgs routedEventArgs)
		{

			// This will close out the dialog and indicate a successful dialog.
			this.DialogResult = true;
			this.Close();

		}

		/// <summary>
		/// Terminates the dialog.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
		private void OnCancel(object sender, RoutedEventArgs routedEventArgs)
		{

			// This will close out the dialog and indicate a failed dialog.
			this.DialogResult = false;
			this.Close();

		}

	}

}
