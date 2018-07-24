namespace FluidTrade.Guardian.Windows.Controls
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using FluidTrade.Core.Windows;
    using FluidTrade.Core.Windows.Utilities;
    
	/// <summary>
	/// A ComboBox used to select a blotter.
	/// </summary>
	public class BlotterComboBox : ComboBox
	{

		// Private Static Fields
		private BlotterList blotterList;

		/// <summary>
		/// Identifies the FluidTrade.Guardian.Windows.Controls.SelectedValue dependency property.
		/// </summary>
		public static new readonly DependencyProperty SelectedValueProperty;

		/// <summary>
		/// Create the static resources used by this class.
		/// </summary>
		static BlotterComboBox()
		{

			// SelectedValue Property
			BlotterComboBox.SelectedValueProperty = DependencyProperty.Register(
				"SelectedValue",
				typeof(Guid),
				typeof(BlotterComboBox),
				new FrameworkPropertyMetadata(Guid.Empty, new PropertyChangedCallback(OnSelectedValueChanged)));

		}

		/// <summary>
		/// Create a combo box used to select a Blotter.
		/// </summary>
		public BlotterComboBox()
		{

			//this.SetBinding(BlotterComboBox.NewSelectedValueProperty, new Binding("SelectedValue") { Source = this, Mode = BindingMode.TwoWay });

			// This list will update dynamically.  When it has initialized itself from the data model it will invoke the event
			// handler that allows a saved value to be used to select the current item.
			this.blotterList = new BlotterList();
			this.blotterList.Initialized += new EventHandler(OnBlotterListInitialized);

			this.Unloaded += (s, e) =>
				this.blotterList.Dispose();


			//this.SelectionChanged += new SelectionChangedEventHandler(OnSelectionChanged);

			// The items displayed in the combo box is bound to the automatically updating list.
			Binding itemsSourceBinding = new Binding();
			itemsSourceBinding.Source = this.blotterList;
			BindingOperations.SetBinding(this, ComboBox.ItemsSourceProperty, itemsSourceBinding);

			// This forces the ComboBox to display the name of the blotter, but use the BlotterId for the selected value.
			this.SelectedValuePath = "BlotterId";
			this.DisplayMemberPath = "Name";

		}

		/// <summary>
		/// Gets or sets the selected value.
		/// </summary>
		public new Guid SelectedValue
		{
			get { return (Guid)this.GetValue(BlotterComboBox.SelectedValueProperty); }
			set { this.SetValue(BlotterComboBox.SelectedValueProperty, value); }
		}

		/// <summary>
		/// Handles the initialization of the specialized list holding the blotters.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The unused event arguments.</param>
		private void OnBlotterListInitialized(object sender, EventArgs e)
		{

			// The standard ComboBox has a flaw: if the ComboBox is populated after the item is selected, the selected value is
			// cleared.  This is significant because the items in this ComboBox are populated from a background thread: only a
			// background thread can lock the data model and read values.  This presents a problem because the user of this 
			// ComboBox simply wants to set the value and continue on with processing the dialog box.  This action will remember
			// that setting and update the base ComboBox once the background initialization is complete.
            try
            {
                InputHelper.IsPropertyUpdate = true;
                this.SetValue(ComboBox.SelectedValueProperty, this.GetValue(BlotterComboBox.SelectedValueProperty));
            }
            finally
            {
                InputHelper.IsPropertyUpdate = false;
            }			
			
		}

		/// <summary>
		/// Handles a change to the SelectedValue property.
		/// </summary>
		/// <param name="dependencyObject">The object that owns the property.</param>
		/// <param name="dependencyPropertyChangedEventArgs">A description of the changed property.</param>
		private static void OnSelectedValueChanged(DependencyObject dependencyObject,
			DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{

			// Extract the strongly typed variables from the generic parameters.
			BlotterComboBox viewerPrototype = dependencyObject as BlotterComboBox;
			Object obj = dependencyPropertyChangedEventArgs.NewValue;

			// The selected value of the base class is set here.
            try
            {
                InputHelper.IsPropertyUpdate = true;
                viewerPrototype.SetValue(ComboBox.SelectedValueProperty, obj);
            }
            finally
            {
                InputHelper.IsPropertyUpdate = false;
            }					

		}

		/// <summary>
		/// Handles a change to the selected value.
		/// </summary>
		/// <param name="sender">The object that owns the property.</param>
		/// <param name="e">Describes the changed selection.</param>
		private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{

			// The 'SelectedValue' property of the base class was covered up with a new property that had the ability to remember
			// the selected value and use it to select an item after the special list was initialized.  This propogates the
			// selected item up into the overriden property when the item is modified by the user.
            BlotterComboBox comboBox = sender as BlotterComboBox;
            if (comboBox.NullSafe(p => p.SelectedValue) != null)
                this.SetValue(ComboBox.SelectedValueProperty, comboBox.SelectedValue);

		}

	}

}
