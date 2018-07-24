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
    /// Interaction logic for HeatIndexControl.xaml
    /// </summary>
	public partial class HeatIndexControl : Control
    {
        /// <summary>
        /// Dependency property for value of the index.
        /// </summary>
        public static readonly DependencyProperty PercentProperty;
		/// <summary>
		/// Dependency property for gauge color of the index.
		/// </summary>
		public static readonly DependencyProperty GuageBrushProperty;

        static HeatIndexControl()
        {

            HeatIndexControl.PercentProperty =
                DependencyProperty.Register("Percent", typeof(decimal), typeof(HeatIndexControl));
			HeatIndexControl.GuageBrushProperty =
				DependencyProperty.Register("GuageBrush", typeof(Brush), typeof(HeatIndexControl));

			HeatIndexControl.DefaultStyleKeyProperty.OverrideMetadata(typeof(HeatIndexControl), new FrameworkPropertyMetadata(typeof(HeatIndexControl)));

        }

		/// <summary>
		/// Create a new heat index control.
		/// </summary>
		public HeatIndexControl()
		{

			InitializeComponent();

		}

		/// <summary>
		/// Dependancy property accessor for CLR
		/// </summary>
		public decimal Percent
		{
			get { return (decimal)GetValue(HeatIndexControl.PercentProperty); }
			set { SetValue(HeatIndexControl.PercentProperty, (value)); }
		}

		/// <summary>
		/// Dependancy property accessor for CLR
		/// </summary>
		public Brush GuageBrush
		{
			get { return GetValue(HeatIndexControl.GuageBrushProperty) as Brush; }
			set { SetValue(HeatIndexControl.GuageBrushProperty, (value)); }
		}

    }

}
