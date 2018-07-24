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

namespace FluidTrade.Guardian
{
    /// <summary>
    /// Interaction logic for HintTextBox.xaml
    /// </summary>
    public partial class HintTextBox : TextBox
    {
        const string grayedOutText = "Enter chat message here";

        // Dependency Properties

        public static DependencyProperty InfoProperty = DependencyProperty.Register(
        "Info",
        typeof(String),
        typeof(HintTextBox),
        new PropertyMetadata(String.Empty)
        );

        // Properties

        public string Info
        {
            get { return (String)GetValue(HintTextBox.InfoProperty); }
            set { SetValue(HintTextBox.InfoProperty, value); }
        }

        public HintTextBox()
        {
            InitializeComponent();

            Info = grayedOutText;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Text.Length > 0)
                Info = string.Empty;
            else
                Info = grayedOutText;
        }

        /// <summary>
        /// Remove the placeholder text when the textbox gets keyboard focus.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            Info = string.Empty;
        }

        /// <summary>
        /// If the textbox is empty and it loses keyboard focus, put the placeholder text back.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (Text.Length == 0)
                Info = grayedOutText;
        }
    }
}
