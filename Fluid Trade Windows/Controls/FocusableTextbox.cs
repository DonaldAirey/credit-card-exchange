using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FluidTrade.Core.Windows.Controls
{
    /// <summary>
    /// Resctrictions on input
    /// </summary>
    public enum FocusableTextInputType : int
    {
        None = 0,
        LettersOnly,
        NumbersOnly,
        PricesOnly,
        AlphaNumOnly
    }

    /// <summary>
    /// 
    /// </summary>
    /// 
    public class FocusableTextbox : TextBox
    {

        public static readonly DependencyProperty InputProperty;
		public static readonly DependencyPropertyKey IsFocusInsideProperty;
		public static readonly DependencyProperty StringFormatProperty;

		private String originalString = String.Empty;
		private Boolean isFocusInside = false;

        #region Constructors
        /// <summary>
        /// Static Constructor for WPF
        /// </summary>
        static FocusableTextbox()
        {
            InputProperty = DependencyProperty.Register(
                "InputType",
                typeof(FocusableTextInputType),
				typeof(FocusableTextbox),
                new FrameworkPropertyMetadata(FocusableTextInputType.None));

			IsFocusInsideProperty = DependencyProperty.RegisterReadOnly(
				"IsFocusInside",
				typeof(Boolean),
				typeof(FocusableTextbox),
				new PropertyMetadata(false));

			StringFormatProperty = DependencyProperty.Register(
				"StringFormat",
				typeof(String),
				typeof(FocusableTextbox),
				new FrameworkPropertyMetadata(String.Empty));


			//Override the meta data for the Text Property of the textbox.
			FrameworkPropertyMetadata metaData = new FrameworkPropertyMetadata();
			metaData.CoerceValueCallback = ForceText;
			TextProperty.OverrideMetadata(typeof(FocusableTextbox), metaData);
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public FocusableTextbox()
            : base()
        {
            this.LostFocus += new RoutedEventHandler(OnLostFocus);
            this.GotFocus += new RoutedEventHandler(OnGotFocus);
            //this.KeyDown += new KeyEventHandler(OnKeyDown);
            //this.MouseDoubleClick += new MouseButtonEventHandler(OnMouseDoubleClick);
            IsFocusInside = false;
			//Hide the caret until the user actually starts typing.  This seems like the simplest
			//solution to have excel like functionality.
			this.IsReadOnly = true;

			// enable undo support, note the default value is true but setting this anyways to see the effect.
			this.IsUndoEnabled = true;

            CommandBindings.Add(new CommandBinding(ApplicationCommands.SelectAll, SelectAll, CancelCommand));
        }

        private void SelectAll(object sender, ExecutedRoutedEventArgs e)
        {
            this.SelectAll();
        }

        private void CancelCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            if (IsFocusInside)
            {
                e.CanExecute = true;
                e.Handled = true;
                return;//default behavior
            }

            e.ContinueRouting = true;
            e.CanExecute = false;
            e.Handled = false;
        }

        #endregion

        #region Event handlers

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnTextInput(TextCompositionEventArgs e)
        {
            base.OnTextInput(e);
            if (IsFocusInside == false)
            {
                SetFocusInside(true);
            }
        }

         /// Event handler for Textbox on preview text.  Called before displaying the entered text.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreviewTextInput(System.Windows.Input.TextCompositionEventArgs e)
        {
            if (!IsValid(e.Text))
            {
                e.Handled = true;
            }
            else
            {
                base.OnPreviewTextInput(e);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            
            if (e.RightButton == MouseButtonState.Pressed)
            {
				//e.Handled = true;
				// Just return and let the mouse down event continue when the right mouse button is click.
				//base.OnMouseDown(e);
				return;
            }
            else
            {
				//Let the BodyCanvas handle this event.  Bodycanvas Mousedown event handle things like selecting multiple rectangles.
				base.OnMouseDown(e);
				e.Handled = false;
            }
        }

		protected override void  OnPreviewMouseDoubleClick(MouseButtonEventArgs e)
		{
			if (IsFocusInside == false)
			{
				if (SelectionLength == Text.Length)
				{
					SelectionStart = SelectionLength;
				}

				RoutedEventArgs newEventArgs = new RoutedEventArgs(ReportGrid.ChildFocusEvent);
				RaiseEvent(newEventArgs);
				SetFocusInside(true);
				IsReadOnly = false;
			}
			
			//base.OnPreviewMouseDoubleClick(e);

		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
		protected override void OnPreviewKeyDown(KeyEventArgs e)
		{
			base.OnPreviewKeyDown(e);

			this.IsReadOnly = false;
			switch (e.Key)
			{
				case Key.F2:
					if (IsFocusInside == false)
					{
						if (SelectionLength == Text.Length)
						{
							SelectionStart = SelectionLength;
						}

						SetFocusInside(true);
					}
					break;
				case Key.Escape:
					this.Text = originalString;
					this.IsReadOnly = true;
					break;

			}


		}

        void OnLostFocus(object sender, RoutedEventArgs e)
        {
            SetFocusInside(false);
        }


        void OnGotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
			//Save for undo.
			originalString = tb.Text;
			//Select all the text for quick entry.
            tb.SelectAll();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Character types accepted.
        /// </summary>
        public FocusableTextInputType InputType
        {
            get { return (FocusableTextInputType)GetValue(InputProperty); }
            set { SetValue(InputProperty, value); }
        }

		/// <summary>
		/// 
		/// </summary>
		public String StringFormat
		{
			get { return (String)GetValue(StringFormatProperty); }
			set { SetValue(StringFormatProperty, value); }
		}

        /// <summary>
        /// Internal state flag to determine what overrite mode
        /// </summary>
		public bool IsFocusInside
		{
			get { return this.isFocusInside; }
			protected set
			{

				this.isFocusInside = value;
				this.SetValue(FocusableTextbox.IsFocusInsideProperty, value);

			}
		}
        #endregion


		private static object ForceText(DependencyObject sender, object value)
		{

			try
			{
				////HACK - use valueconvertor to do this
				FocusableTextbox textBox = (FocusableTextbox)sender;
				if (String.IsNullOrEmpty(textBox.StringFormat) == false && value != null)
				{
					Decimal val;
					Decimal.TryParse((String)value, out val);
					return String.Format(textBox.StringFormat, val);
				}
			}
			catch
			{				
			}

			return value;
		}

        /// <summary>
        /// Checks for restrictions 
        /// </summary>
        /// <param name="p"></param>
        /// <returns>bool - true if valid</returns>
        public bool IsValid(string stringToValidate)
        {
            foreach (var character in stringToValidate)
            {
                if (IsValid(character) == false)
                    return false;
            }
            return true;
        }


        /// <summary>
        /// Checks for restrictions
        /// </summary> 
        /// <param name="character"></param>
        /// <returns>bool - true if valid</returns>
        public bool IsValid(char character)
        {
            switch (InputType)
            {
            case FocusableTextInputType.LettersOnly:
                return Char.IsLetter(character);
            case FocusableTextInputType.PricesOnly:    
                    return IsPrice(character);
            case FocusableTextInputType.NumbersOnly:
                return Char.IsNumber(character);
            case FocusableTextInputType.AlphaNumOnly:
                return Char.IsLetterOrDigit(character);
            default:
                return true;
            }
        }

        private bool IsPrice(char character)
        {   
            bool isMatch = Regex.IsMatch(character.ToString(), @"^[\d\.]$");
            return isMatch;
        }

        private void SetFocusInside(bool focusInside)
        {

			try
			{
				if (focusInside)
				{
					this.Style = (Style)this.FindResource("TextBoxBaseStyle");
				}
				else
				{
					this.Style = (Style)this.FindResource("TextBoxStyle");
				}
			}
			catch
			{
			}

            IsFocusInside = focusInside;
        }
    }
}
