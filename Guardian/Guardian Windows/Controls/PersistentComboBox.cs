namespace FluidTrade.Guardian.Windows.Controls
{

	using System;
	using System.Collections.Specialized;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Data;

	/// <summary>
	/// A combobox that keeps its selection while the selection changes.
	/// </summary>
	public class PersistentComboBox : ComboBox
	{

		/// <summary>
		/// Indicates PersistentSelectedValue dependency property.
		/// </summary>
		public static readonly DependencyProperty PersistentSelectedValueProperty = DependencyProperty.Register(
			"PersistentSelectedValue",
			typeof(object),
			typeof(PersistentComboBox),
			new PropertyMetadata(null, OnSelectedValueChanged, CoerceSelectedValue));

		/// <summary>
		/// Indicates the PersistentSelectedValueChanged routed event.
		/// </summary>
		public static readonly RoutedEvent PersistentSelectedValueChangedEvent = EventManager.RegisterRoutedEvent(
			"PersistentSelectedValueChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PersistentComboBox));

		private object selectedValue;

		/// <summary>
		/// Event raised when PersistentSelectedValue changes.
		/// </summary>
		public event RoutedEventHandler PersistentSelectedValueChanged
		{
			add { this.AddHandler(PersistentComboBox.PersistentSelectedValueChangedEvent, value); }
			remove { this.RemoveHandler(PersistentComboBox.PersistentSelectedValueChangedEvent, value); }
		}

		static PersistentComboBox()
		{

			ItemsSourceProperty.AddOwner(typeof(PersistentComboBox), new FrameworkPropertyMetadata(null, OnItemsSourceChanged, BeforeItemsSourceChanged));

		}

		/// <summary>
		/// Create a new combobox.
		/// </summary>
		public PersistentComboBox()
		{

			this.SetBinding(
				PersistentComboBox.SelectedValueProperty,
				new Binding("PersistentSelectedValue")
				{
					Source = this,
					Mode = BindingMode.TwoWay,
					UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
				});

		}

		/// <summary>
		/// Gets or sets the current selected value.
		/// </summary>
		public object PersistentSelectedValue
		{
			get { return this.GetValue(PersistentComboBox.PersistentSelectedValueProperty); }
			set { this.SetValue(PersistentComboBox.PersistentSelectedValueProperty, value); }
		}

		/// <summary>
		/// Do any setup before the ItemsSource changes.
		/// </summary>
		/// <param name="sender">The combo box.</param>
		/// <param name="baseValue">The proposed value of ItemsSource.</param>
		/// <returns></returns>
		private static object BeforeItemsSourceChanged(DependencyObject sender, object baseValue)
		{

			PersistentComboBox box = sender as PersistentComboBox;
			box.selectedValue = box.SelectedValue;

			return baseValue;

		}

		/// <summary>
		/// Do any setup before the ItemsSource changes.
		/// </summary>
		/// <param name="sender">The combo box.</param>
		/// <param name="baseValue">The proposed value of ItemsSource.</param>
		/// <returns></returns>
		private static object CoerceSelectedValue(DependencyObject sender, object baseValue)
		{

			return baseValue;

		}

		/// <summary>
		/// Handle the Initialized event from the ItemsSource (if it is a IDataBoundList).
		/// </summary>
		/// <param name="sender">The ItemsSource.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnInitialized(object sender, EventArgs eventArgs)
		{

		}

		/// <summary>
		/// Handle the ItemsSource changing.
		/// </summary>
		/// <param name="sender">The combo box.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private static void OnItemsSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			PersistentComboBox box = sender as PersistentComboBox;

			box.SelectedValue = box.selectedValue;

			if (eventArgs.OldValue is INotifyCollectionChanged)
			{

				(eventArgs.OldValue as INotifyCollectionChanged).CollectionChanged -= (sender as PersistentComboBox).OnItemsChanged;

				if (eventArgs.OldValue is IDataBoundList)
					(eventArgs.OldValue as IDataBoundList).CollectionChanging -= (sender as PersistentComboBox).OnItemsChanging;

			}
			if (eventArgs.NewValue is INotifyCollectionChanged)
			{

				(eventArgs.NewValue as INotifyCollectionChanged).CollectionChanged += (sender as PersistentComboBox).OnItemsChanged;
				
				if (eventArgs.NewValue is IDataBoundList)
					(eventArgs.NewValue as IDataBoundList).CollectionChanging -= (sender as PersistentComboBox).OnItemsChanging;

			}

		}

		/// <summary>
		/// Handle a change in the contents of ItemsSource.
		/// </summary>
		/// <param name="sender">The ItemsSource collection.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnItemsChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
		{

			this.SelectedValue = this.selectedValue;

		}

		/// <summary>
		/// Handle the contents of ItemsSource changing.
		/// </summary>
		/// <param name="sender">The ItemsSource collection.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnItemsChanging(object sender, EventArgs eventArgs)
		{

			this.selectedValue = this.SelectedValue;

		}

		/// <summary>
		/// Handle a change in the selected value.
		/// </summary>
		/// <param name="sender">The combo box.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private static void OnSelectedValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			PersistentComboBox box = sender as PersistentComboBox;

			box.RaiseEvent(new RoutedEventArgs(PersistentComboBox.PersistentSelectedValueChangedEvent, box));

			if (box.selectedValue == null)
				box.selectedValue = eventArgs.NewValue;

		}

	}

}
