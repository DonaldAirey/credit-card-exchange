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
    public class DebtRuleComboBox : ComboBox
    {

        // Private Static Fields
        private DebtRuleList debtRuleList;

		/// <summary>
		/// Identifies the NewSelectedValue dependency property. This dependency property works around a bug in how
		/// WPF and the reflection framework interact.
		/// </summary>
		public static readonly DependencyProperty NewSelectedValueProperty;

        /// <summary>
        /// Identifies the FluidTrade.Guardian.Windows.Controls.SelectedValue dependency property.
        /// </summary>
        public static new readonly DependencyProperty SelectedValueProperty;

        /// <summary>
        /// Identifies the FluidTrade.Guardian.Windows.Controls.SelectedValue dependency property.
        /// </summary>
        public static DependencyProperty ParentBlotterIdProperty;

        /// <summary>
        /// Create the static resources used by this class.
        /// </summary>
        static DebtRuleComboBox()
        {

            // SelectedValue Property
            DebtRuleComboBox.SelectedValueProperty = DependencyProperty.Register(
                "SelectedValue",
                typeof(Guid?),
                typeof(DebtRuleComboBox),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnSelectedValueChanged)));
			//DebtRuleComboBox.NewSelectedValueProperty = DependencyProperty.Register(
			//    "NewSelectedValue",
			//    typeof(Guid),
			//    typeof(DebtRuleComboBox),
			//    new FrameworkPropertyMetadata(Guid.Empty, new PropertyChangedCallback(OnSelectedValueChanged)));

            DebtRuleComboBox.ParentBlotterIdProperty = DependencyProperty.Register(
                "ParentBlotterId",
                typeof(Guid),
                typeof(DebtRuleComboBox),
                new FrameworkPropertyMetadata(Guid.Empty, new PropertyChangedCallback(OnBlotterIdChanged)));

        }

        /// <summary>
        /// Create a combo box used to select a Blotter.
        /// </summary>
        public DebtRuleComboBox()
        {

            // This forces the ComboBox to display the name of the blotter, but use the BlotterId for the selected value.
            this.SelectedValuePath = "DebtRuleId";
            this.DisplayMemberPath = "Name";

			this.Unloaded += (s, e) =>
				this.debtRuleList.Dispose();

        }

        /// <summary>
        /// Gets or sets the selected value.
        /// </summary>
        public new Guid? SelectedValue
        {
            get { return (Guid?)this.GetValue(DebtRuleComboBox.SelectedValueProperty); }
            set { this.SetValue(DebtRuleComboBox.SelectedValueProperty, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public Guid ParentBlotterId
        {
            get { return (Guid)this.GetValue(DebtRuleComboBox.ParentBlotterIdProperty); }
            set { this.SetValue(DebtRuleComboBox.ParentBlotterIdProperty, value); }
        }

		/// <summary>
		/// The list used to fill the combo box.
		/// </summary>
		public DebtRuleList DebtRuleList
		{ 
			get { return this.debtRuleList; } 			
		}

        /// <summary>
        /// Handles the initialization of the specialized list holding the blotters.
        /// </summary>
        /// <param name="sender">The object that originated the event.</param>
        /// <param name="e">The unused event arguments.</param>
        private void OnDebtRuleListInitialized(object sender, EventArgs e)
        {

            // The standard ComboBox has a flaw: if the ComboBox is populated after the item is selected, the selected value is
            // cleared.  This is significant because the items in this ComboBox are populated from a background thread: only a
            // background thread can lock the data model and read values.  This presents a problem because the user of this 
            // ComboBox simply wants to set the value and continue on with processing the dialog box.  This action will remember
            // that setting and update the base ComboBox once the background initialization is complete.
            try
            {
                InputHelper.IsPropertyUpdate = true;
				Guid? setValue = this.SelectedValue;
				DebtRule useParentDebtRule = new DebtRule()
					{
						DebtRuleId = Guid.Empty,
						Name = "<inherit from parent>",
					};

				this.DebtRuleList.Insert(0, useParentDebtRule);

				if (setValue == null && this.DebtRuleList.Count > 1)
					setValue = this.DebtRuleList[1].DebtRuleId;
			
				this.SetValue(ComboBox.SelectedValueProperty, null);
				this.SetValue(ComboBox.SelectedValueProperty, setValue);

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
			DebtRuleComboBox comboBox = dependencyObject as DebtRuleComboBox;
			object obj = dependencyPropertyChangedEventArgs.NewValue;


            // The selected value of the base class is set here.
			try
			{
				InputHelper.IsPropertyUpdate = true;
				comboBox.SetValue(ComboBox.SelectedValueProperty, obj);
			}
			finally
			{
				InputHelper.IsPropertyUpdate = false;
			}

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dependencyObject"></param>
        /// <param name="dependencyPropertyChangedEventArgs"></param>
        private static void OnBlotterIdChanged(DependencyObject dependencyObject, 
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {

            // Extract the strongly typed variables from the generic parameters.
            DebtRuleComboBox comboBox = dependencyObject as DebtRuleComboBox;
            Guid blotterId = (Guid)dependencyPropertyChangedEventArgs.NewValue;

			if (comboBox.debtRuleList != null)
			{

				comboBox.debtRuleList.Initialized -= comboBox.OnDebtRuleListInitialized;
				comboBox.debtRuleList.Dispose();

			}

			comboBox.debtRuleList = new DebtRuleList(blotterId);
			comboBox.debtRuleList.Initialized += comboBox.OnDebtRuleListInitialized;

			comboBox.SetBinding(ComboBox.ItemsSourceProperty, new Binding() { Source = comboBox.debtRuleList });

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
            DebtRuleComboBox comboBox = sender as DebtRuleComboBox;
            if (comboBox.NullSafe(p => p.SelectedValue) != null)
                this.SetValue(ComboBox.SelectedValueProperty, comboBox.SelectedValue);

        }

    }

}
