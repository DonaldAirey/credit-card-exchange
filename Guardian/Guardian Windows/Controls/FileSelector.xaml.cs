namespace FluidTrade.Guardian.Windows.Controls
{

    using System;
    using System.Windows;
    using System.Windows.Controls;
    using Microsoft.Win32;

    /// <summary>
    /// Interaction logic for FileSelector.xaml
    /// </summary>
    public partial class FileSelector : UserControl
    {
        /// <summary>
        /// Create a new file selector control.
        /// </summary>
        public FileSelector()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The filename in the text box.
        /// </summary>
        public String Text
        {

            get { return fileNameBox.Text; }
            set { fileNameBox.Text = value; }

        }

        /// <summary>
        /// Launch a open-file dialog box and fill the text box with the selected filename.
        /// </summary>
        /// <param name="sender">The '...' button.</param>
        /// <param name="e">The event arguments.</param>
        private void LaunchDialog(object sender, RoutedEventArgs e)
        {

            OpenFileDialog dialog = new OpenFileDialog();

            dialog.ShowDialog();

            fileNameBox.Text = dialog.FileName;

        }

    }

}
