namespace FluidTrade.Guardian.Windows.Controls
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Windows.Controls;
	using System.Windows;
	using System.Windows.Media;
	using System.Windows.Data;
	using System.Windows.Input;

	/// <summary>
	/// A TextBox with the ability to display a default text string.
	/// </summary>
	public class DefaultTextBox : TextBox
	{

		/// <summary>
		/// Indicates the FilledBackground dependency property.
		/// </summary>
		public static readonly DependencyProperty FilledForegroundProperty = DependencyProperty.Register(
			"FilledForeground",
			typeof(Brush),
			typeof(DefaultTextBox),
			new PropertyMetadata(SystemColors.ControlTextBrush));
		/// <summary>
		/// Indicates the DefaultForeground dependency property.
		/// </summary>
		public static readonly DependencyProperty DefaultForegroundProperty = DependencyProperty.Register(
			"DefaultForeground",
			typeof(Brush),
			typeof(DefaultTextBox),
			new PropertyMetadata(SystemColors.GrayTextBrush));
		/// <summary>
		/// Indicates the DefaultText dependency property.
		/// </summary>
		public static readonly DependencyProperty DefaultTextProperty = DependencyProperty.Register(
			"DefaultText",
			typeof(string),
			typeof(DefaultTextBox));
		/// <summary>
		/// Indicates the FilledText dependency property.
		/// </summary>
		public static readonly DependencyProperty FilledTextProperty = DependencyProperty.Register(
			"FilledText",
			typeof(string),
			typeof(DefaultTextBox),
			new PropertyMetadata("", DefaultTextBox.OnFilledTextChanged, DefaultTextBox.CoerceFilledText));
		/// <summary>
		/// Indicates the UserOnly dependency property.
		/// </summary>
		public static readonly DependencyProperty UserOnlyProperty = DependencyProperty.Register("UserOnly", typeof(Boolean), typeof(DefaultTextBox));

		/// <summary>
		/// Indicates the ActualTextChanged routed event.
		/// </summary>
		public static readonly RoutedEvent ActualTextChangedEvent =
			EventManager.RegisterRoutedEvent("ActualTextChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(DefaultTextBox));

		private Boolean useDefault = false;

		/// <summary>
		/// Event raised when the user changes the text of the TextBox.
		/// </summary>
		public event RoutedEventHandler ActualTextChanged
		{
			add { AddHandler(ActualTextChangedEvent, value); }
			remove { RemoveHandler(ActualTextChangedEvent, value); }
		}

		/// <summary>
		/// Create a new DefaultTextBox.
		/// </summary>
		public DefaultTextBox()
			: base()
		{

			this.PreviewTextInput += this.OnInput;
			this.PreviewKeyDown += this.OnPreviewKeyDown;
			DataObject.AddPastingHandler(this, this.OnInput);
			this.DisplayDefaultText();

		}

		/// <summary>
		/// The foreground brush used when the DefaultText is displayed.
		/// </summary>
		public Brush DefaultForeground
		{

			get { return this.GetValue(DefaultTextBox.DefaultForegroundProperty) as Brush; }
			set { this.SetValue(DefaultTextBox.DefaultForegroundProperty, value); }

		}

		/// <summary>
		/// The text displayed when the FilledText is empty.
		/// </summary>
		public string DefaultText
		{

			get { return this.GetValue(DefaultTextBox.DefaultTextProperty) as string; }
			set { this.SetValue(DefaultTextBox.DefaultTextProperty, value); }

		}

		/// <summary>
		/// The foreground brush used when the FilledText is displayed.
		/// </summary>
		public Brush FilledForeground
		{

			get { return this.GetValue(DefaultTextBox.FilledForegroundProperty) as Brush; }
			set { this.SetValue(DefaultTextBox.FilledForegroundProperty, value); }

		}

		/// <summary>
		/// The text of the TextBox. When FilledText is empty or null, DefaultText is displayed instead. If the user has changed the text of the
		/// TextBox, then FillText is "locked" to whatever the user inputs.
		/// </summary>
		public string FilledText
		{

			get { return this.GetValue(DefaultTextBox.FilledTextProperty) as string; }
			set { this.SetValue(DefaultTextBox.FilledTextProperty, value); }

		}

		/// <summary>
		/// If true, only the user can change the value of FilledText.
		/// </summary>
		public Boolean UserOnly
		{

			get { return (Boolean)this.GetValue(DefaultTextBox.UserOnlyProperty); }
			set { this.SetValue(DefaultTextBox.UserOnlyProperty, value); }

		}

		/// <summary>
		/// Display the default text in the TextBox rather than the FilledText.
		/// </summary>
		private void DisplayDefaultText()
		{

			if (!this.useDefault)
			{

				this.useDefault = true;

				this.SetBinding(TextBox.TextProperty, new Binding("DefaultText") { Source = this });
				this.SetBinding(TextBox.ForegroundProperty, new Binding("DefaultForeground") { Source = this });
				this.FontStyle = FontStyles.Italic;

			}

		}

		/// <summary>
		/// Display the filled text in the TextBox.
		/// </summary>
		private void DisplayFilledText()
		{

			if (this.useDefault)
			{

				this.useDefault = false;

				this.SetBinding(TextBox.TextProperty, new Binding("FilledText") { Source = this });
				this.SetBinding(TextBox.ForegroundProperty, new Binding("FilledForeground") { Source = this });
				this.FontStyle = FontStyles.Normal;

			}

		}

		/// <summary>
		/// Handle (some kinds of) user input. When the user enters text, stop bindings and the code behind from modifying FilledText.
		/// </summary>
		/// <param name="sender">The DefaultTextBox.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnInput(object sender, RoutedEventArgs eventArgs)
		{

			this.UserOnly = true;
			this.DisplayFilledText();

		}

		/// <summary>
		/// Handle some keydown events. When the user makes changes to the text, stop bindings and the code behind from modifying FilledText.
		/// </summary>
		/// <param name="sender">The DefaultTextBox.</param>
		/// <param name="eventArgs">The event arguments.</param>
		void OnPreviewKeyDown(object sender, KeyEventArgs eventArgs)
		{

			if (eventArgs.Key == Key.Back || eventArgs.Key == Key.Delete || eventArgs.Key == Key.Space)
			{

				this.UserOnly = true;
				this.DisplayFilledText();

			}

		}

		/// <summary>
		/// Handle the changing of the Text property. If the change was made by the user, raise the ActualTextChanged event.
		/// </summary>
		/// <param name="eventArgs"></param>
		protected override void OnTextChanged(TextChangedEventArgs eventArgs)
		{

			base.OnTextChanged(eventArgs);

			if (this.UserOnly)
			{

				BindingExpression bind = this.GetBindingExpression(TextBox.TextProperty);

				if (bind != null)
					bind.UpdateSource();

				this.RaiseEvent(new RoutedEventArgs(DefaultTextBox.ActualTextChangedEvent));

			}

		}

		/// <summary>
		/// Handle the changing of the FilledText property. If is empty, display the DefaultText, otherwise display the FilledText.
		/// </summary>
		/// <param name="sender">The DefaultTextBox.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private static void OnFilledTextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			DefaultTextBox textBox = sender as DefaultTextBox;

			if (String.IsNullOrEmpty(textBox.FilledText))
				textBox.DisplayDefaultText();
			else
				textBox.DisplayFilledText();

		}

		/// <summary>
		/// Handle the text box losing focus. If FilledText is empty, revert to the DefaultText.
		/// </summary>
		/// <param name="eventArgs"></param>
		protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs eventArgs)
		{

			if (String.IsNullOrEmpty(this.FilledText))
				this.Text = this.DefaultText;

			base.OnLostKeyboardFocus(eventArgs);

		}

		/// <summary>
		/// If the UserOnly property is set, keep the FilledText from varying from the Text.
		/// </summary>
		/// <param name="sender">The DefaultTextBox.</param>
		/// <param name="newValue">The new value of FilledText.</param>
		/// <returns></returns>
		private static object CoerceFilledText(DependencyObject sender, object newValue)
		{

			if ((Boolean)sender.GetValue(DefaultTextBox.UserOnlyProperty))
				return sender.GetValue(TextBox.TextProperty);
			else
				return newValue;

		}

	}

}
