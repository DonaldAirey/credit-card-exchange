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

namespace FluidTrade.Actipro
{
    /// <summary>
    /// Interaction logic for DateTimePicker.xaml
    /// </summary>
    public partial class DateTimePicker : UserControl
    {

        /// <summary>
        /// Dependency property for current DateTime of the picker.
        /// </summary>
        public static readonly DependencyProperty DateTimeProperty;
        /// <summary>
        /// The DateTimeChanged event.
        /// </summary>
        public static readonly RoutedEvent DateTimeChangedEvent;
		/// <summary>
		/// Format of the dateTime control. For example - "MM/dd/yy" - will dispay 12/25/08
		/// </summary>
		public static readonly DependencyProperty FormatProperty;
		/// <summary>
		/// Dependency property for the IsFocusInside property;
		/// </summary>
		public static readonly DependencyPropertyKey IsFocusInsideProperty;

        /// <summary>
        /// The event fired when the value of DateTime changes.
        /// </summary>
        public event RoutedEventHandler DateTimeChanged
        {
            add { AddHandler(DateTimePicker.DateTimeChangedEvent, value); }
            remove { RemoveHandler(DateTimePicker.DateTimeChangedEvent, value); }
        }

		private Boolean isFocusInside = false;
		private TextBox editableTextBox = null;
		private DateTime? originalValue = null;

        static DateTimePicker()
        {

            DateTimePicker.DateTimeProperty =
				DependencyProperty.Register("DateTime", typeof(DateTime?), typeof(DateTimePicker), new PropertyMetadata(null, OnDateTimeChanged));
			IsFocusInsideProperty = DependencyProperty.RegisterReadOnly(
				"IsFocusInside",
				typeof(Boolean),
				typeof(DateTimePicker),
				new PropertyMetadata(false));
            DateTimePicker.FormatProperty =
                DependencyProperty.Register("Format", typeof(string), typeof(DateTimePicker), new PropertyMetadata(null));

            DateTimePicker.DateTimeChangedEvent =
                EventManager.RegisterRoutedEvent("DateTimeChanged", RoutingStrategy.Bubble,typeof(RoutedEventHandler), typeof(DateTimePicker));
            
        }

        /// <summary>
        ///Constructor
        /// </summary>
        public DateTimePicker()
        {

            InitializeComponent();
			this.picker.IsKeyboardFocusWithinChanged += this.OnLostKeyboardFocus;
			this.Loaded += delegate(object s, RoutedEventArgs e)
			{

				this.picker.ApplyTemplate();
				this.editableTextBox = this.FindTextBox(this.picker);
				this.IsFocusInside = false;
			};

        }
       
        /// <summary>
        /// Dependancy property accessor for CLR
        /// </summary>
        public DateTime? DateTime
        {
            get { return (DateTime?)GetValue(DateTimePicker.DateTimeProperty); }
            set { SetValue(DateTimePicker.DateTimeProperty, value); }
        }

        /// <summary>
        /// Dependancy property accessor for CLR
        /// </summary>
        public string Format
        {
            get { return (string)GetValue(FormatProperty); }
            set { SetValue(FormatProperty, value); }
		}

		/// <summary>
		/// Internal state flag to determine what overrite mode
		/// </summary>
		public bool IsFocusInside
		{
			get { return this.isFocusInside; }
			protected set
			{

				System.Diagnostics.Debug.WriteLine(String.Format("Focus changed to {0}", value));

				if (value && !this.isFocusInside)
					this.originalValue = this.picker.Value;

				this.isFocusInside = value;
				this.SetValue(DateTimePicker.IsFocusInsideProperty, value);

				if (value)
				{

					this.editableTextBox.Focus();
					this.editableTextBox.IsReadOnly = false;

				}
				else
				{

					this.editableTextBox.IsReadOnly = true;
					//this.editableTextBox.Focus();

				}

			}
		}

		/// <summary>
		/// Find the text box in the picker.
		/// </summary>
		/// <param name="element">The element we're diving into.</param>
		/// <returns>The text box that edits the date, if it's find. If it isn't, null.</returns>
		private TextBox FindTextBox(DependencyObject element)
		{

			TextBox textBox = null;

			for (Int32 index = 0, total = VisualTreeHelper.GetChildrenCount(element); index < total && textBox == null; ++index)
			{

				DependencyObject child = VisualTreeHelper.GetChild(element, index);

				if (child is TextBox && (child as TextBox).Name == "PART_EditableTextBox")
					textBox = child as TextBox;
				else
					textBox = this.FindTextBox(child);

			}

			return textBox;

		}

        /// <summary>
        /// Raise the DateTimeChanged event.
        /// </summary>
        /// <param name="sender">The date-time picker.</param>
        /// <param name="eventArgs">The event arguments.</param>
        static private void OnDateTimeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
        {

            DateTimePicker picker = sender as DateTimePicker;
            picker.RaiseEvent(new RoutedEventArgs(DateTimePicker.DateTimeChangedEvent));

        }

		/// <summary>
		/// Handle losing keyboard focus.
		/// </summary>
		/// <param name="sender">The date-time picker.</param>
		/// <param name="e">The event arguments</param>
		protected void OnLostKeyboardFocus(object sender, DependencyPropertyChangedEventArgs e)
		{

			if (!this.IsKeyboardFocusWithin)
			{

				DateTime dateTime;

				System.Diagnostics.Debug.WriteLine(String.Format("Lost keyboard focus"));
				this.IsFocusInside = false;

				// The client's report-grid focus model is... eccentric, so we need to update the date ourselves, rather than rely on the binding mechanism.
				if (System.DateTime.TryParse(this.picker.Text, out dateTime))
					this.DateTime = dateTime;
				else
					this.DateTime = this.originalValue;

			}

		}

		/// <summary>
		/// Handle a double click.
		/// </summary>
		/// <param name="e">The event arguments.</param>
		protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine(String.Format("Mouse click"));

			this.IsFocusInside = true;
			base.OnMouseDoubleClick(e);

		}

		/// <summary>
		/// Swap in the actual text box before a key comes in.
		/// </summary>
		/// <param name="eventArgs">The event arguments.</param>
		protected override void OnPreviewKeyDown(KeyEventArgs eventArgs)
		{

			System.Diagnostics.Debug.WriteLine(String.Format("Preview key down"));
			if (eventArgs.Key == Key.Escape)
			{

				this.picker.Value = this.originalValue;
				this.IsFocusInside = false;

			}
			else
			{

				this.IsFocusInside = true;
				base.OnPreviewKeyDown(eventArgs);

			}

		}

		/// <summary>
		/// Handle text input.
		/// </summary>
		/// <param name="e">The event arguments.</param>
		protected override void OnTextInput(TextCompositionEventArgs e)
		{

			//this.IsFocusInside = true;
			base.OnTextInput(e);

		}

    }

}
