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

namespace FluidTrade.Guardian.Controls
{
    /// <summary>
    /// Interaction logic for ProgressBar.xaml
    /// </summary>
    public partial class ProgressBar : UserControl
    {
        /// <summary>
        /// Keep track of the current value of the progress bar.
        /// </summary>
        public double currentProgressBarValue;

        /// <summary>
        /// Constructor for ProgressBar.
        /// </summary>
        public ProgressBar()
        {
            InitializeComponent();
            currentProgressBarValue = 0.5;
        }

        /// <summary>
        /// Allow the progress bars value to be updated.
        /// </summary>
        /// <param name="progress"></param>
        public void UpdateProgress(double progress)
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, (System.Threading.SendOrPostCallback)
                delegate { this.myProgressBar.SetValue(System.Windows.Controls.ProgressBar.ValueProperty, progress); }, null);
        }

    }
}
