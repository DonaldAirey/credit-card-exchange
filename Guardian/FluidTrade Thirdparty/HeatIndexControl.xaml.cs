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

namespace FluidTrade.Thirdparty
{
    /// <summary>
    /// Interaction logic for HeatIndexControl.xaml
    /// </summary>
    public partial class HeatIndexControl : UserControl
    {
        /// <summary>
        /// Dependency property for current DateTime of the picker.
        /// </summary>
        public static readonly DependencyProperty PercentProperty;

        /// <summary>
        /// 
        /// </summary>
		public static readonly DependencyProperty BarBackgroundProperty;

        static HeatIndexControl()
        {

            HeatIndexControl.PercentProperty =
                DependencyProperty.Register("Percent", typeof(decimal), typeof(HeatIndexControl), new PropertyMetadata(OnPercentValueChanged));

			HeatIndexControl.BarBackgroundProperty =
				DependencyProperty.Register("BarBackground", typeof(LinearGradientBrush), typeof(HeatIndexControl));
            
            
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
		public LinearGradientBrush BarBackground
        {
			get { return (LinearGradientBrush)GetValue(HeatIndexControl.BarBackgroundProperty); }
			set { SetValue(HeatIndexControl.BarBackgroundProperty, value); }
        }


        /// <summary>
        /// 
        /// </summary>
        public HeatIndexControl()
        {
            InitializeComponent();			

        }

	
        /// <summary>
        /// Changes the brush depending on the 
        /// </summary>
        /// <param name="dependencyObject"></param>
        /// <param name="eventArgs"></param>
        private static void OnPercentValueChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {

            HeatIndexControl heatIndexControl = dependencyObject as HeatIndexControl;

            if (heatIndexControl != null)
            {
                decimal percent = heatIndexControl.Percent  * 100;				
				
				if (percent == 100)
					heatIndexControl.BarBackground = new LinearGradientBrush(Colors.LimeGreen, Colors.LimeGreen, 0);
				else
					heatIndexControl.BarBackground = new LinearGradientBrush(Colors.Gray, Colors.Gray, 0);
            }		

        }
        
    }
}
