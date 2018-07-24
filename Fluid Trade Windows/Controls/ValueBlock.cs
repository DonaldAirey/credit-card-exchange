namespace FluidTrade.Core.Windows.Controls
{

	using System;
    using System.Windows;
	using System.Windows.Controls;
    using System.Windows.Markup;

	/// <summary>
	/// Provides a lightweight control for displaying small amounts of flow content in a user specified format.
	/// </summary>
	[ContentProperty("Content")]
	public class ValueBlock : Control
	{

		/// <summary>
		/// Identifies the Content dependency property.
		/// </summary>
		public static readonly DependencyProperty ContentProperty;

		/// <summary>
		/// Identifies the Decreased routed event.
		/// </summary>
		public static readonly RoutedEvent DecreaseEvent;

		/// <summary>
		/// Identifies the Format dependency property.
		/// </summary>
		public static readonly DependencyProperty FormatProperty;

		/// <summary>
		/// Identifies the Increase routed event.
		/// </summary>
		public static readonly RoutedEvent IncreaseEvent;

		/// <summary>
		/// Identifies the IsUp dependency property.
		/// </summary>
		public static readonly DependencyProperty IsUpProperty;

		/// <summary>
		/// Identifies the IsDown dependency property.
		/// </summary>
		public static readonly DependencyProperty IsDownProperty;

		/// <summary>
		/// Identifies the TextAlignment dependency property.
		/// </summary>
		public static readonly DependencyProperty TextAlignmentProperty;

		/// <summary>
		/// Identifies the Text dependency property.
		/// </summary>
		public static readonly DependencyProperty TextProperty;

		// Private Instance Fields
		private Boolean ignoreChange;

		/// <summary>
		/// Creates the static resources used by this control.
		/// </summary>
		static ValueBlock()
		{

			// The Content Property
			ValueBlock.ContentProperty = DependencyProperty.Register("Content", typeof(Object), typeof(ValueBlock),
				new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnContentChanged)));

			// The Format Property
			ValueBlock.FormatProperty = DependencyProperty.Register("Format", typeof(String), typeof(ValueBlock),
				new FrameworkPropertyMetadata("{0}", FrameworkPropertyMetadataOptions.None,
					new PropertyChangedCallback(OnFormatChanged)));

			// The HorizontalContentAlignment Property
			Control.HorizontalContentAlignmentProperty.AddOwner(typeof(ValueBlock),
				new FrameworkPropertyMetadata(OnHorizontalContentAlignmentChanged));

			// The IsUp Property
			ValueBlock.IsUpProperty = DependencyProperty.Register("IsUp", typeof(Boolean), typeof(ValueBlock),
				new FrameworkPropertyMetadata(false));

			// The IsDown Property
			ValueBlock.IsDownProperty = DependencyProperty.Register("IsDown", typeof(Boolean), typeof(ValueBlock),
				new FrameworkPropertyMetadata(false));

			// The TextAlignment Property
			ValueBlock.TextAlignmentProperty = DependencyProperty.Register("TextAlignment", typeof(TextAlignment),
				typeof(ValueBlock), new FrameworkPropertyMetadata(TextAlignment.Left, FrameworkPropertyMetadataOptions.None));

			// The Text Property
			ValueBlock.TextProperty = DependencyProperty.Register("Text", typeof(String), typeof(ValueBlock),
				new FrameworkPropertyMetadata(String.Empty));

			// The Increase Event
			ValueBlock.IncreaseEvent = EventManager.RegisterRoutedEvent("Increase", RoutingStrategy.Bubble,
				typeof(RoutedEventHandler), typeof(ValueBlock));

			// The Decrease Event
			ValueBlock.DecreaseEvent = EventManager.RegisterRoutedEvent("Decrease", RoutingStrategy.Bubble,
				typeof(RoutedEventHandler), typeof(ValueBlock));

		}

		/// <summary>
		/// Creates a ValueBlock.
		/// </summary>
		public ValueBlock()
		{

			// The Up/Down event triggers will get confused when the data context changes if the content isn't reset.
			this.DataContextChanged += new DependencyPropertyChangedEventHandler(OnDataContextChanged);

		}

		/// <summary>
		/// Gets or sets the value displayed in this control.
		/// </summary>
		public Object Content
		{
			get { return GetValue(ValueBlock.ContentProperty); }
			set { SetValue(ValueBlock.ContentProperty, value); }
		}

		/// <summary>
		/// Gets or sets the format for the value displayed in this control.
		/// </summary>
		public String Format
		{
			get { return GetValue(ValueBlock.FormatProperty) as String; }
			set { SetValue(ValueBlock.FormatProperty, value); }
		}

		/// <summary>
		/// Adds or removes an Increase Value event handler.
		/// </summary>
		public event RoutedEventHandler Increase
		{
			add { AddHandler(ValueBlock.IncreaseEvent, value); }
			remove { RemoveHandler(ValueBlock.IncreaseEvent, value); }
		}

		/// <summary>
		/// Gets an indication that the value has increased.
		/// </summary>
		public Boolean IsUp
		{
			get { return (Boolean)GetValue(ValueBlock.IsUpProperty); }
		}

		/// <summary>
		/// Gets an indication that the value has decreased.
		/// </summary>
		public Boolean IsDown
		{
			get { return (Boolean)GetValue(ValueBlock.IsDownProperty); }
		}

		/// <summary>
		/// Adds or removes a Decrease Value event handler.
		/// </summary>
		public event RoutedEventHandler Decrease
		{
			add { AddHandler(ValueBlock.DecreaseEvent, value); }
			remove { RemoveHandler(ValueBlock.DecreaseEvent, value); }
		}

		/// <summary>
		/// Gets the formatted text of the value in this control.
		/// </summary>
		public String Text
		{
			get { return (String)GetValue(ValueBlock.TextProperty); }
		}

		/// <summary>
		/// Gets the alignment of text in this control.
		/// </summary>
		public TextAlignment TextAlignment
		{
			get { return (TextAlignment)GetValue(ValueBlock.TextAlignmentProperty); }
		}

		/// <summary>
		/// Handles a change to the value of this control.
		/// </summary>
		/// <param name="dependencyObject">The dependency object that has been changed.</param>
		/// <param name="dependencyPropertyChangedEventArgs">The event arguments describing the change to the property.</param>
		public static void OnContentChanged(DependencyObject dependencyObject,
			DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{

			// Display the value using the Format property in the Text property of the base class.
			ValueBlock valueBlock = dependencyObject as ValueBlock;
			String format = valueBlock.GetValue(ValueBlock.FormatProperty) as String;
			Object value = dependencyPropertyChangedEventArgs.NewValue;
			valueBlock.SetValue(ValueBlock.TextProperty, String.Format(format, value));

			// Recycling this control can lead to false Up/Down indications because the old value is no longer relavent to the new
			// value.  When the data context of the control has changed, the next property update will not attempt to trigger an
			// Up/Down event.
			if (valueBlock.ignoreChange)
			{
				valueBlock.ignoreChange = false;
			}
			else
			{

				// This provides feedback indicating whether the control has increased or decreased in value.
				if (dependencyPropertyChangedEventArgs.NewValue is IComparable &&
					dependencyPropertyChangedEventArgs.OldValue is IComparable)
				{

					IComparable iComparable = dependencyPropertyChangedEventArgs.NewValue as IComparable;
					switch (iComparable.CompareTo(dependencyPropertyChangedEventArgs.OldValue))
					{

					case 1:

						// Indicate that the value has increased.
						valueBlock.SetValue(ValueBlock.IsUpProperty, true);
						valueBlock.SetValue(ValueBlock.IsDownProperty, false);

						// Raise an event that indicates the value has increased.
						valueBlock.RaiseEvent(new RoutedEventArgs(ValueBlock.IncreaseEvent));

						break;

					case -1:

						// Indicate that the value has decreased.
						valueBlock.SetValue(ValueBlock.IsUpProperty, false);
						valueBlock.SetValue(ValueBlock.IsDownProperty, true);

						// Raise an event that indicates the value has decreased.
						valueBlock.RaiseEvent(new RoutedEventArgs(ValueBlock.DecreaseEvent));

						break;

					}

				}

			}


		}

		/// <summary>
		/// Handles a change to the data context.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{

			// This flag will inhibit the next property change event handler to ignore the old value.  When switching data context,
			// the old value can't be compared to the new value and leads to false indications of movement.
			this.ignoreChange = e.OldValue != null;

		}

		/// <summary>
		/// Handles a change to the format of the data.
		/// </summary>
		/// <param name="dependencyObject">The dependency object that has been changed.</param>
		/// <param name="dependencyPropertyChangedEventArgs">The event arguments describing the change to the property.</param>
		public static void OnFormatChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{

			// Reformat the text in the control when the format changes.
			ValueBlock valueBlock = dependencyObject as ValueBlock;
			String format = dependencyPropertyChangedEventArgs.NewValue as String;
			Object value = valueBlock.GetValue(ValueBlock.ContentProperty);
			valueBlock.SetValue(TextBlock.TextProperty, String.Format(format, value));

		}

		/// <summary>
		/// Translates the HorizontalContentAlignment value into a TextAlignment value.
		/// </summary>
		/// <param name="dependencyObject">The Dependency Property that has changed.</param>
		/// <param name="dependencyPropertyChangedEventArgs">The event arguments describing the property change.</param>
		public static void OnHorizontalContentAlignmentChanged(DependencyObject dependencyObject,
			DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{

			// The basic idea here is to translate the HorizontalContentAlignment used by the Control class into a TextAlignment
			// value that is usable by the TextBlock class.  This allows a Control to host a TextBlock with a consistent approach
			// to how the alignment of the content is specified.
			ValueBlock valueBlock = (ValueBlock)dependencyObject;
			HorizontalAlignment horizontalAlignment = (HorizontalAlignment)dependencyPropertyChangedEventArgs.NewValue;

			// Translate the HorizontalContentAlignment into a TextAlignment that TextBlocks can use.
			switch (horizontalAlignment)
			{
			case HorizontalAlignment.Center:

				valueBlock.SetValue(ValueBlock.TextAlignmentProperty, TextAlignment.Center);
				break;

			case HorizontalAlignment.Left:

				valueBlock.SetValue(ValueBlock.TextAlignmentProperty, TextAlignment.Left);
				break;

			case HorizontalAlignment.Right:

				valueBlock.SetValue(ValueBlock.TextAlignmentProperty, TextAlignment.Right);
				break;

			}

		}

	}

}
