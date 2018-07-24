namespace FluidTrade.Actipro
{

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

    /// <summary>
    /// Interaction logic for RandyOMeter.xaml
    /// </summary>
    public partial class RandOMeter : UserControl
    {

        /// <summary>
        /// The identifier for the Maximum dependency property.
        /// </summary>
        public static DependencyProperty MaximumProperty;
        /// <summary>
        /// The identifier for the Minimum dependency property.
        /// </summary>
        public static DependencyProperty MinimumProperty;
        /// <summary>
        /// The identifier for the Percent dependency property.
        /// </summary>
        internal static DependencyProperty PercentProperty;
        /// <summary>
        /// The identifier for the Value dependency property.
        /// </summary>
        public static DependencyProperty ValueProperty;

        /// <summary>
        /// Initialize the dependency properties.
        /// </summary>
        static RandOMeter()
        {

            RandOMeter.MaximumProperty = DependencyProperty.Register("Maximum", typeof(double), typeof(RandOMeter), new PropertyMetadata(1.0, OnMaximumChanged));
            RandOMeter.MinimumProperty = DependencyProperty.Register("Minimum", typeof(double), typeof(RandOMeter), new PropertyMetadata(0.0, OnMinimumChanged));
            RandOMeter.PercentProperty = DependencyProperty.Register("Percent", typeof(double), typeof(RandOMeter));
            RandOMeter.ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(RandOMeter), new PropertyMetadata(0.5, OnValueChanged));

        }

        /// <summary>
        /// Initialize the randiness meter.
        /// </summary>
        public RandOMeter()
        {

            InitializeComponent();

        }

        /// <summary>
        /// The maximum value the meter can have. This is used to judge where the meter should point.
        /// </summary>
        public double Maximum
        {

            get { return (double)this.GetValue(RandOMeter.MaximumProperty); }
            set { this.SetValue(RandOMeter.MaximumProperty, value); }

        }

        /// <summary>
        /// The minimum value the meter can have. This is used to judge where the meter should point.
        /// </summary>
        public double Minimum
        {

            get { return (double)this.GetValue(RandOMeter.MinimumProperty); }
            set { this.SetValue(RandOMeter.MinimumProperty, value); }

        }

        /// <summary>
        /// The percentage of the available range that the value lies on.
        /// </summary>
        internal double Percent
        {

            get { return (double)this.GetValue(RandOMeter.PercentProperty); }
            set { this.SetValue(RandOMeter.PercentProperty, value); }

        }

        /// <summary>
        /// When the maximum value changes, make sure the value of the meter doesn't fall outside of it.
        /// </summary>
        /// <param name="meterObject">The meter itself.</param>
        /// <param name="eventArgs">The event arguments</param>
        public static void OnMaximumChanged(DependencyObject meterObject, DependencyPropertyChangedEventArgs eventArgs)
        {

            RandOMeter meter = meterObject as RandOMeter;

            if (meter.Maximum < meter.Value)
                meter.Value = meter.Maximum;

            if (meter.Maximum - meter.Minimum != 0.0)
                meter.Percent = (meter.Value - meter.Minimum) / (meter.Maximum - meter.Minimum);

        }

        /// <summary>
        /// When the minimum value changes, make sure the value of the meter doesn't fall outside of it.
        /// </summary>
        /// <param name="meterObject">The meter itself.</param>
        /// <param name="eventArgs">The event arguments</param>
        public static void OnMinimumChanged(DependencyObject meterObject, DependencyPropertyChangedEventArgs eventArgs)
        {

            RandOMeter meter = meterObject as RandOMeter;

            if (meter.Minimum > meter.Value)
                meter.Value = meter.Minimum;

            if (meter.Maximum - meter.Minimum != 0.0)
                meter.Percent = (meter.Value - meter.Minimum) / (meter.Maximum - meter.Minimum);

        }

        /// <summary>
        /// Adjust the meter to reflect the change in the value.
        /// </summary>
        /// <param name="meterObject">The meter itself.</param>
        /// <param name="eventArgs">The event arguments</param>
        public static void OnValueChanged(DependencyObject meterObject, DependencyPropertyChangedEventArgs eventArgs)
        {

            RandOMeter meter = meterObject as RandOMeter;

            if (meter.Maximum - meter.Minimum != 0.0)
                meter.Percent = (meter.Value - meter.Minimum) / (meter.Maximum - meter.Minimum);

        }

        /// <summary>
        /// The current absolute value of the meter.
        /// </summary>
        public double Value
        {

            get { return (double)this.GetValue(RandOMeter.ValueProperty); }
            set { this.SetValue(RandOMeter.ValueProperty, value); }

        }

    }

}
