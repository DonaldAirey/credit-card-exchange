namespace FluidTrade.Core.Windows.Controls
{

	using System;
	using System.Collections;
	using System.Reflection;
	using System.Windows;

    /// <summary>
	/// Selects an item from a list of items.
	/// </summary>
	public class ItemSelector : System.Windows.Controls.ContentControl
	{

		// Public Static Fields
		public static readonly System.Windows.DependencyProperty ItemsSourceProperty;
		public static readonly System.Windows.DependencyProperty SelectedValueProperty;
		public static readonly System.Windows.DependencyProperty SelectedValuePathProperty;

		/// <summary>
		/// Create the static resources for the ItemSelector.
		/// </summary>
		static ItemSelector()
		{

			// ItemsSource Property
			ItemSelector.ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable),
				typeof(ItemSelector));

			// SelectedValue Property
			ItemSelector.SelectedValueProperty = DependencyProperty.Register("SelectedValue", typeof(Object), typeof(ItemSelector),
				new FrameworkPropertyMetadata(OnSelectedValueChanged));

			// SelectedValuePath Property
			ItemSelector.SelectedValuePathProperty = DependencyProperty.Register("SelectedValuePath", typeof(String),
				typeof(ItemSelector));

		}

		/// <summary>
		/// Gets or sets a collection used to generate the content of the ItemsControl. This is a dependency property.
		/// </summary>
		public IEnumerable ItemsSource
		{
			get { return (IEnumerable)this.GetValue(ItemSelector.ItemsSourceProperty); }
			set { this.SetValue(ItemSelector.ItemsSourceProperty, value); }
		}

		/// <summary>
		/// Gets or sets the value of the SelectedItem, obtained by using SelectedValuePath. This is a dependency property.
		/// </summary>
		public Object SelectedValue
		{
			get { return (Object)this.GetValue(ItemSelector.SelectedValueProperty); }
			set { this.SetValue(ItemSelector.SelectedValueProperty, value); }
		}

		/// <summary>
		/// Gets or sets the path that is used to get the SelectedValue from the SelectedItem. This is a dependency property.
		/// </summary>
		public String SelectedValuePath
		{
			get { return (String)this.GetValue(ItemSelector.SelectedValuePathProperty); }
			set { this.SetValue(ItemSelector.SelectedValuePathProperty, value); }
		}

		/// <summary>
		/// Handles a change to the selected value.
		/// </summary>
		/// <param name="dependencyObject">The owner of the dependency object that was changed.</param>
		/// <param name="dependencyPropertyChangedEventArgs">The arguments describing the change.</param>
		private static void OnSelectedValueChanged(DependencyObject dependencyObject,
			DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{

			// When the selected value has changed, all the items in the list will be searched for a matching value.  The first
			// item in the list associated with this control that contains a property that matches the given value will become the
			// new content for this control.  Note that the property is selected using Reflection and the name of the property.
			ItemSelector itemSelector = dependencyObject as ItemSelector;
			foreach (Object item in itemSelector.ItemsSource)
			{
				PropertyInfo propertyInfo = item.GetType().GetProperty(itemSelector.SelectedValuePath);
				if (propertyInfo != null)
				{
					object itemValue = propertyInfo.GetValue(item, null);
					if (itemValue.Equals(dependencyPropertyChangedEventArgs.NewValue))
						itemSelector.Content = item;
				}
			}
		}

	}

}
