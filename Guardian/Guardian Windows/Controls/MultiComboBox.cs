namespace FluidTrade.Guardian.Windows.Controls
{

	using System;
	using System.Collections;
	using System.Text;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Controls.Primitives;
	using System.Windows.Input;
	using System.Windows.Media;
	using System.Collections.ObjectModel;
	using System.Collections.Specialized;
	using System.Reflection;
	using FluidTrade.Core;

	/// <summary>
    /// Interaction logic for MultiComboBox.xaml
    /// </summary>
	[TemplatePart(Name = "PART_ListBox", Type = typeof(ListBox))]
	public class MultiComboBox : ComboBox
    {

        private ListBox listBox;
		private Boolean initialized;
		private ObservableCollection<object> selectedValues;
		private Boolean populating = false;

		/// <summary>
		/// The command checkboxes should send they're clicked.
		/// </summary>
		public static readonly RoutedCommand SelectCommand =
			new RoutedCommand("SelectCommand", typeof(MultiComboBox));
		/// <summary>
		/// Indicates the SelectedValues dependency property.
		/// </summary>
		public static readonly DependencyProperty SelectedValuesProperty =
			DependencyProperty.Register("SelectedValues", typeof(IList), typeof(MultiComboBox), new FrameworkPropertyMetadata() { CoerceValueCallback = MultiComboBox.CoerceSelectedValues, BindsTwoWayByDefault = false });
		/// <summary>
		/// Indicates the SelectedItems dependency property.
		/// </summary>
		public static readonly DependencyPropertyKey SelectedItemsProperty =
			DependencyProperty.RegisterReadOnly("SelectedItems", typeof(IList), typeof(MultiComboBox), new PropertyMetadata(null));
        /// <summary>
        /// Indicates the Separator dependency property.
        /// </summary>
        public static readonly DependencyProperty SeparatorProperty =
            DependencyProperty.Register("Separator", typeof(string), typeof(MultiComboBox), new PropertyMetadata(" ", MultiComboBox.OnSeparatorChanged));
		/// <summary>
		/// Indicates the SelectedIndex dependency property.
		/// </summary>
		public new static readonly DependencyProperty SelectedIndexProperty =
			DependencyProperty.Register("SelectedIndex", typeof(int), typeof(MultiComboBox), new PropertyMetadata(-1, MultiComboBox.OnSelectedIndexChanged));
		/// <summary>
		/// Indicates the SelectionChanged routed event.
		/// </summary>
		public new static readonly RoutedEvent SelectionChangedEvent =
			EventManager.RegisterRoutedEvent("SelectionChanged", RoutingStrategy.Bubble, typeof(SelectionChangedEventHandler), typeof(MultiComboBox));

		/// <summary>
		/// Raised when the selection changes.
		/// </summary>
		public new event SelectionChangedEventHandler SelectionChanged
		{

			add { this.AddHandler(SelectionChangedEvent, value); }
			remove { this.RemoveHandler(SelectionChangedEvent, value); }			

		}

		static MultiComboBox()
		{

			DefaultStyleKeyProperty.OverrideMetadata(typeof(MultiComboBox), new FrameworkPropertyMetadata(typeof(MultiComboBox)));

		}

        /// <summary>
        /// Initialize the combox components.
        /// </summary>
        public MultiComboBox()
        {

			this.initialized = false;
			this.DropDownOpened += this.OnPopupOpened;

			this.selectedValues = new ObservableCollection<object>();
			this.SetValue(MultiComboBox.SelectedValuesProperty, this.selectedValues);
			this.initialized = true;
			this.selectedValues.CollectionChanged += OnSelectedValuesChanged;
			
			this.CommandBindings.Add(new CommandBinding(MultiComboBox.SelectCommand, this.OnSelectItem));

			// Keep the base combobox from changing its selection.
			this.PreviewKeyDown += delegate(object s, KeyEventArgs e)
				{
					if (e.Key != Key.Tab)
						e.Handled = true;
				};

        }

		/// <summary>
		/// Get or set the "currently selected" index. Really, setting the SelectedIndex selects the item at that index, but does not otherwise affect
		/// the selection (that is, no other items are de-selected). The value of SelectedIndex is always the index of the item that was last selected.
		/// </summary>
		public new int SelectedIndex
		{

			get { return (int)this.GetValue(MultiComboBox.SelectedIndexProperty); }
			set { this.SetValue(MultiComboBox.SelectedIndexProperty, value); }

		}

        /// <summary>
        /// Get the collection of the currently selected items.
        /// </summary>
        public IList SelectedItems
        {

            get { return this.listBox == null? null : this.listBox.SelectedItems; }

        }

        /// <summary>
        /// Get the collection of the currently selected values.
        /// </summary>
        public IList SelectedValues
        {

            get { return this.GetValue(MultiComboBox.SelectedValuesProperty) as IList; }
            set { this.SetValue(MultiComboBox.SelectedValuesProperty, value); }

        }

        /// <summary>
        /// Gets or sets the separator added inbetween the items in the text display.
        /// </summary>
        public string Separator
        {

            get { return this.GetValue(MultiComboBox.SeparatorProperty) as string; }
            set { this.SetValue(MultiComboBox.SeparatorProperty, value); }

        }

		/// <summary>
		/// Deceit. Rather than allowing the value of SelectedValues to change, simply duplicate the contents of the incoming list to our internal
		/// list.
		/// </summary>
		/// <param name="sender">The MultiComboBox.</param>
		/// <param name="newValue">The incoming list.</param>
		/// <returns>The combobox's selectedItems list.</returns>
		private static object CoerceSelectedValues(DependencyObject sender, object newValue)
		{

			MultiComboBox comboBox = sender as MultiComboBox;

			// The first time through we're actually setting SelectedValues to the private selectedValues list, so we'll let it get set.
			if (!comboBox.initialized)
			{

				return newValue;

			}
			else
			{

				comboBox.selectedValues.Clear();

				if (newValue != null)
					foreach (object value in newValue as IList)
						comboBox.selectedValues.Add(value);

				return comboBox.selectedValues;

			}

		}

        /// <summary>
        /// Find the first visual child of an object with a particular type.
        /// </summary>
        /// <typeparam name="childItem">The type of the child.</typeparam>
        /// <param name="obj">The object to look in.</param>
        /// <returns>The first child in the visual tree whose type matches.</returns>
        private childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
        {

            for (int index = 0; index < VisualTreeHelper.GetChildrenCount(obj); index++)
            {

                DependencyObject child = VisualTreeHelper.GetChild(obj, index);

                if (child != null && child is childItem)
                {

                    return (childItem)child;

                }
                else
                {

                    childItem childOfChild = FindVisualChild<childItem>(child);

                    if (childOfChild != null)
                        return childOfChild;

                }

            }

            return null;

        }

        /// <summary>
        /// Get the checkbox control for an item in the list.
        /// </summary>
        /// <param name="item">The item whose checkbox we want.</param>
        /// <returns>The CheckBox control.</returns>
        private CheckBox GetCheckBox(object item)
        {

            ListBoxItem listItem = listBox.ItemContainerGenerator.ContainerFromItem(item) as ListBoxItem;

			if (listItem != null)
			{

				ContentPresenter presenter = this.FindVisualChild<ContentPresenter>(listItem);

				return presenter.ContentTemplate.FindName("checkBox", presenter) as CheckBox;

			}
			else
			{

				return null;

			}

        }

		/// <summary>
		/// Get the item with a particular value, as indicated by SelectedValuePath.
		/// </summary>
		/// <param name="value">The value of the item to retrieve.</param>
		/// <returns>The item with the indicated value, or null if no item with that value could be found.</returns>
		private object GetItemByValue(object value)
		{

			foreach (object item in this.Items)
			{

				if (this.GetValueFromItem(item).Equals(value))
					return item;

			}

			return null;

		}

		/// <summary>
		/// Get the display value of an item via DisplayMemberPath.
		/// </summary>
		/// <param name="item">The item whose display value should be retrieved.</param>
		/// <returns>The display value of the item, gotten from the property indicated by DisplayMemberPath. If DisplayMemberPath is null (or some
		/// other error occurs), return null.</returns>
		private object GetDisplayFromItem(object item)
		{

			if (!String.IsNullOrEmpty(this.DisplayMemberPath))
			{

				PropertyInfo valueProperty = item.GetType().GetProperty(this.DisplayMemberPath);

				if (valueProperty != null)
					return valueProperty.GetValue(item, null);
				else
					return null;

			}
			else
			{

				return null;

			}

		}

		/// <summary>
		/// Get the value of an item via the SelectedValuePath.
		/// </summary>
		/// <param name="item">The item whose value should be retrieved.</param>
		/// <returns>The value of the item, gotten from the property indicated by SelectedValuePath. If SelectedValuePath is null (or some other
		/// error occurs), return null.</returns>
		private object GetValueFromItem(object item)
		{

			if (!String.IsNullOrEmpty(this.SelectedValuePath))
			{

				PropertyInfo valueProperty = item.GetType().GetProperty(this.SelectedValuePath);

				if (valueProperty != null)
					return valueProperty.GetValue(item, null);
				else
					return null;

			}
			else
			{

				return null;

			}

		}

		/// <summary>
		/// Do additional setup once the template is applied.
		/// </summary>
		public override void OnApplyTemplate()
		{

			base.OnApplyTemplate();

			this.listBox = this.Template.FindName("PART_ListBox", this) as ListBox;
			this.listBox.SelectionChanged += this.OnSelectionChanged;
			this.listBox.PreviewMouseDown += this.OnSwallowedMouseEvent;
			this.listBox.PreviewMouseUp += this.OnSwallowedMouseEvent;
			this.listBox.ItemContainerGenerator.StatusChanged += this.OnCheckBoxesGenerated;
			this.SetValue(MultiComboBox.SelectedItemsProperty, this.listBox.SelectedItems);

			foreach (object value in this.Items)
			{

				object item = this.GetValueFromItem(value);

				if (this.SelectedValues.Contains(item) && !this.SelectedItems.Contains(item))
					this.SelectedItems.Add(value);

			}

		}

		/// <summary>
		/// Handle a checkbox being checked. Make sure it's selected in the listbox.
		/// </summary>
		/// <param name="sender">The checkbox that was checked.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnSelectItem(object sender, ExecutedRoutedEventArgs eventArgs)
		{

			CheckBox item = sender as CheckBox;

			if (!this.populating)
			{

				object value = this.GetValueFromItem(eventArgs.Parameter);

				if (!this.listBox.SelectedItems.Contains(eventArgs.Parameter))
				{

					this.listBox.SelectedItems.Add(eventArgs.Parameter);

					if (value != null)
						this.selectedValues.Add(value);

				}
				else
				{

					this.listBox.SelectedItems.Remove(eventArgs.Parameter);

					if (value != null)
						this.selectedValues.Remove(value);

				}

			}

		}

		/// <summary>
		/// Handle the items source changing. Swap the collection changed event handler to the new source and add any delay selected values to the
		/// SelectedItems list.
		/// </summary>
		/// <param name="oldValue">The old source.</param>
		/// <param name="newValue">The new source.</param>
		protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
		{

			base.OnItemsSourceChanged(oldValue, newValue);

			if (oldValue is INotifyCollectionChanged)
				(oldValue as INotifyCollectionChanged).CollectionChanged -= OnItemsCollectionChanged;

			if (newValue is INotifyCollectionChanged)
				(newValue as INotifyCollectionChanged).CollectionChanged += OnItemsCollectionChanged;

			if (newValue != null)
				foreach (object item in newValue)
					if (this.SelectedValues.Contains(this.GetValueFromItem(item)))
						this.SelectedItems.Add(item);

		}

		/// <summary>
		/// Handle changes to the items collection. Add any delay selected values to the SelectedItems list.
		/// </summary>
		/// <param name="sender">The items collection.</param>
		/// <param name="eventArgs">The event arguments.</param>
		void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
		{

			if (eventArgs.NewItems != null && this.SelectedItems != null)
				foreach (object item in eventArgs.NewItems)
				{

					object value = this.GetValueFromItem(item);

					if (this.SelectedValues.Contains(value) && !this.SelectedItems.Contains(item))
						this.SelectedItems.Add(item);

				}

		}

		/// <summary>
		/// Handle a status change in the listBox's item generator. When the checkboxes are newly generated, we need to run through them and set
		/// their checked state.
		/// </summary>
		/// <param name="sender">The item generator.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnCheckBoxesGenerated(object sender, EventArgs eventArgs)
		{

			try
			{

				if (this.listBox.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
				{

					this.populating = true;
					foreach (object item in this.Items)
						if (!this.SelectedItems.Contains(item))
							this.GetCheckBox(item).IsChecked = false;
					foreach (object item in this.SelectedItems)
						this.GetCheckBox(item).IsChecked = true;
					this.populating = false;

				}

			}
			catch (Exception exception)
			{

				EventLog.Warning(String.Format("{0}: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace));

			}

		}

		/// <summary>
		/// Handle the popup window opening.
		/// </summary>
		/// <param name="sender">The popup window.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnPopupOpened(object sender, EventArgs eventArgs)
		{

			this.listBox.Focus();

		}

        /// <summary>
        /// Handle the selection changing. Make sure that the check boxes of the of the items that have changed state are appropriately check or
        /// unchecked, then reset the display text.
        /// </summary>
        /// <param name="sender">The ListBox.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void OnSelectionChanged(object sender, SelectionChangedEventArgs eventArgs)
        {

			this.SelectedIndex = -1;

			if (eventArgs.AddedItems != null)
				foreach (object item in eventArgs.AddedItems)
				{

					CheckBox checkBox = this.GetCheckBox(item);

					if (checkBox != null)
						checkBox.IsChecked = true;

					this.SelectedIndex = this.listBox.Items.IndexOf(item);

				}

			if (eventArgs.RemovedItems != null)
				foreach (object item in eventArgs.RemovedItems)
				{

					CheckBox checkBox = this.GetCheckBox(item);

					if (checkBox != null)
						checkBox.IsChecked = false;

				}

            this.ResetDisplay();

			eventArgs.RoutedEvent = MultiComboBox.SelectionChangedEvent;
			this.RaiseEvent(eventArgs);

        }

		/// <summary>
		/// Handle the selected index changing. If the checkbox for the new selection isn't checked, check it. Leave all the other checkboxes alone.
		/// </summary>
		/// <param name="sender">The MultiComboBox.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private static void OnSelectedIndexChanged(object sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			MultiComboBox comboBox = sender as MultiComboBox;

			if (comboBox.SelectedIndex >= 0 && !comboBox.SelectedItems.Contains(comboBox.listBox.Items[comboBox.SelectedIndex]))
				comboBox.listBox.SelectedIndex = comboBox.SelectedIndex;

		}

		/// <summary>
		/// Handle the selected values collection changing. Update the SelectedItems collection with the changes.
		/// </summary>
		/// <param name="sender">The selected values collection.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnSelectedValuesChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
		{

			if (eventArgs.NewItems != null)
				foreach (object value in eventArgs.NewItems)
				{

					object item = this.GetItemByValue(value);

					if (item != null && !this.SelectedItems.Contains(item))
						this.SelectedItems.Add(item);

				}

			if (eventArgs.OldItems != null)
				foreach (object value in eventArgs.OldItems)
				{

					object item = this.GetItemByValue(value);

					if (item != null && this.SelectedItems.Contains(item))
						this.SelectedItems.Remove(item);

				}

		}

		/// <summary>
		/// Handle the separator changing. Update the display text.
		/// </summary>
		/// <param name="sender">The MultiComboBox.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private static void OnSeparatorChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			MultiComboBox comboBox = sender as MultiComboBox;

			if (comboBox.IsLoaded)
				comboBox.ResetDisplay();

		}


		/// <summary>
		/// Swallow mouse events in the dropdown when the control is readonly.
		/// </summary>
		/// <param name="sender">The ListBox in the dropdown.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnSwallowedMouseEvent(object sender, MouseButtonEventArgs eventArgs)
		{

			eventArgs.Handled = this.IsReadOnly || eventArgs.RightButton == MouseButtonState.Pressed;

		}
        /// <summary>
        /// Set the Text property of the base ComboBox to reflect the currently selected items.
        /// </summary>
        private void ResetDisplay()
        {

            StringBuilder stringBuilder = new StringBuilder();
			String text;

			for (int item = 0; item < this.Items.Count; ++item)
			{

				if (this.SelectedItems.Contains(this.Items[item]))
				{

					object display = this.GetDisplayFromItem(this.Items[item]);

					if (display != null)
						stringBuilder.Append(display.ToString());
					else
						stringBuilder.Append(this.Items[item].ToString());

					stringBuilder.Append(this.Separator);

				}

			}

			text = stringBuilder.ToString();

			if (text.EndsWith(this.Separator))
				text = text.Substring(0, text.Length - this.Separator.Length);

            base.Text = text;

        }

    }

}
