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
    /// Interaction logic for DialogDatePicker.xaml
    /// </summary>
    public partial class DialogDatePicker : UserControl
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
        /// The event fired when the value of DateTime changes.
        /// </summary>
        public event RoutedEventHandler DateTimeChanged
        {
            add { AddHandler(DialogDatePicker.DateTimeChangedEvent, value); }
            remove { RemoveHandler(DialogDatePicker.DateTimeChangedEvent, value); }
        }

        /// <summary>
        /// Format of the dateTime control. For example - "MM/dd/yy" - will dispay 12/25/08
        /// </summary>
        public static readonly DependencyProperty FormatProperty;

        static DialogDatePicker()
        {

            DialogDatePicker.DateTimeProperty =
                DependencyProperty.Register("DateTime", typeof(DateTime?), typeof(DialogDatePicker), new PropertyMetadata(null, OnDateTimeChanged));

            DialogDatePicker.FormatProperty =
                DependencyProperty.Register("Format", typeof(string), typeof(DialogDatePicker), new PropertyMetadata(null));

            DialogDatePicker.DateTimeChangedEvent =
                EventManager.RegisterRoutedEvent("DateTimeChanged", RoutingStrategy.Bubble,typeof(RoutedEventHandler), typeof(DialogDatePicker));

        }

        /// <summary>
        ///Constructor
        /// </summary>
        public DialogDatePicker()
        {

            InitializeComponent();

        }
       
        /// <summary>
        /// Dependancy property accessor for CLR
        /// </summary>
        public DateTime? DateTime
        {
            get { return (DateTime?)GetValue(DialogDatePicker.DateTimeProperty); }
            set { SetValue(DialogDatePicker.DateTimeProperty, value); }
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
        /// Raise the DateTimeChanged event.
        /// </summary>
        /// <param name="sender">The date-time picker.</param>
        /// <param name="eventArgs">The event arguments.</param>
        static private void OnDateTimeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
        {

            DialogDatePicker picker = sender as DialogDatePicker;
            picker.RaiseEvent(new RoutedEventArgs(DialogDatePicker.DateTimeChangedEvent));

        }
    }
}
