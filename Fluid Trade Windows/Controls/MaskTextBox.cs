namespace FluidTrade.Core.Windows.Controls
{
    using System;
    using System.Windows;
    using System.Windows.Input;

    /// <summary>
    /// Provides  a maskable textbox
    /// </summary>
    public class MaskTextBox : FocusableTextbox
    {
        public static readonly DependencyProperty MaskProperty;

        #region Constructors
        /// <summary>
        /// Static Constructor for WPF to intialize this control.
        /// </summary>
        static MaskTextBox()
        {

            MaskProperty = DependencyProperty.Register(
                "Mask",
                typeof(string),
                typeof(MaskTextBox),
                new FrameworkPropertyMetadata(null, OnPropertyChanged));

            //Override the meta data for the Text Proeprty of the textbox.
            FrameworkPropertyMetadata metaData = new FrameworkPropertyMetadata();
            metaData.CoerceValueCallback = ForceText;
            TextProperty.OverrideMetadata(typeof(MaskTextBox), metaData);


        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public MaskTextBox()
            : base()
        {
            //Since mask is not applied for cut and paset operations we will disable the functionality.
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, null, CancelCommand));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Cut, null, CancelCommand));
        }

        #endregion

        #region Overrides
        /// <summary>
        /// Event handler for Textbox on preview text.  Called before displaying the entered text.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreviewTextInput(System.Windows.Input.TextCompositionEventArgs e)
        {
            if (IsReadOnly)
            {
                base.OnPreviewTextInput(e);
                return;
            }

            if (Mask != null)
            {
                if (IsValid(e.Text))
                {
                    int position = SelectionStart;
                    MaskTextFormatter maskProvider = MaskProvider;
                    if (position < maskProvider.Capacity)
                    {
                        position = maskProvider.InsertAt(e.Text, position);

                    }
                    RefreshText(maskProvider, position);
                }
                e.Handled = true;
            }


            base.OnPreviewTextInput(e);
        }

        /// <summary>
        /// Handler for special keys like delete.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            MaskTextFormatter maskProvider = MaskProvider;
            int position = SelectionStart;
            int length = SelectionLength;
            bool refresh = false;

            if (e.Key == Key.Delete && position < Text.Length)
            {
                if (length > 0)
                {
                    refresh = maskProvider.Remove(position, length);
                }
                else
                {
                    refresh = maskProvider.Remove(position);
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Back)
            {
                if (length > 0)
                {
                    refresh = maskProvider.Remove(position, length);
                }
                else
                {
                    if (position > 0)
                    {
                        position--;
                        refresh = maskProvider.Remove(position);
                    }
                }
                e.Handled = true;
            }
			else if (e.Key == Key.Escape)
			{
				this.maskTextFormatter.Set(((MaskTextBox)e.OriginalSource).Text);
			}

            if (refresh == true)
                RefreshText(maskProvider, position);
        }

        #endregion

        #region Properties
        /// <summary>
        /// Formatter property.
        /// </summary>
        public MaskTextFormatter MaskProvider
        {
            get
            {
                if (this.maskTextFormatter == null)
                {
                    if (Mask != null)
                    {
                        this.maskTextFormatter = new MaskTextFormatter(Mask);
                        this.maskTextFormatter.Set(Text);
                    }
                    else
                    {
                        throw new InvalidOperationException("Mask property missing.");
                    }
                }
                return this.maskTextFormatter;
            }
        }


        /// <summary>
        /// Mask Property associated with this textbox. 
        /// </summary>
        public string Mask
        {
            get { return (string)GetValue(MaskProperty); }
            set { SetValue(MaskProperty, value); }
        }

        #endregion

        #region Methods

        /// <summary>
        ///Force the text of the control to use the mask 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private static object ForceText(DependencyObject sender, object value)
        {
            MaskTextBox textBox = (MaskTextBox)sender;
            if (textBox.Mask != null && value != null)
            {
                MaskTextFormatter provider = new MaskTextFormatter(textBox.Mask);
                provider.Validate += delegate(char character)
                      { return textBox.IsValid(character); };
                provider.Set((string)value);
                return provider.ToString();
            }
            else
            {
                return value;
            }
        }

        /// <summary>
        /// Cancel the command
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void CancelCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
            e.Handled = true;
        }


        /// <summary>
        /// Called when mask property is changed.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            InputHelper.IsPropertyUpdate = true;
            MaskTextBox textBox = (MaskTextBox)d;
            InputHelper.IsPropertyUpdate = false;
        }

        /// <summary>
        /// Use the mask to generate a new display string.
        /// </summary>
        /// <param name="maskProvider"></param>
        /// <param name="position"></param>
        private void RefreshText(MaskTextFormatter maskProvider, int position)
        {
            // Refresh string.
            this.Text = maskProvider.ToString();

            // Position cursor.
            this.SelectionStart = position;
        }

        private MaskTextFormatter maskTextFormatter = null;
        #endregion

    }
}
