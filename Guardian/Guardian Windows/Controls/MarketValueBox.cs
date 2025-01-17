﻿namespace FluidTrade.Guardian.Windows.Controls
{

	using System;
	using System.Globalization;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Input;
	using System.Windows.Media;

	/// <summary>
	/// Represents a control that can be used to display or edit a market value.
	/// </summary>
	public class MarketValueBox : TextBox
	{

		/// <summary>
		/// The IsNullable dependency property.
		/// </summary>
		public static readonly DependencyProperty IsNullableProperty;

		/// <summary>
		/// The IsNull dependency property.
		/// </summary>
		public static readonly DependencyProperty IsNullProperty;

		/// <summary>
		/// The Value dependency property.
		/// </summary>
		public static readonly DependencyProperty ValueProperty;

		/// <summary>
		/// The ValueChanged routed event.
		/// </summary>
		public static readonly RoutedEvent ValueChangedEvent;

		/// <summary>
		/// The original thickness of the edit box before being placed in read-only mode.
		/// </summary>
		private Thickness originalBorderThickness;

		/// <summary>
		/// The original background of the edit box before being placed in read-only mode.
		/// </summary>
		private Brush originalBackground;
		
		/// <summary>
		/// The event fired when the value of the money box changes.
		/// </summary>
		public event RoutedEventHandler ValueChanged
		{
			add { AddHandler(ValueChangedEvent, value); }
			remove { RemoveHandler(ValueChangedEvent, value); }
		}

		/// <summary>
		/// Create the static resources required for the type.
		/// </summary>
		static MarketValueBox()
		{

			// The default alignment for this type of control is to be right aligned.
			MarketValueBox.TextAlignmentProperty.OverrideMetadata(typeof(MarketValueBox), new FrameworkPropertyMetadata(TextAlignment.Right));

			// The default is to allow writing in this control.
			MarketValueBox.IsReadOnlyProperty.OverrideMetadata(typeof(MarketValueBox), new FrameworkPropertyMetadata(false, OnIsReadOnlyChanged));

			// The default Content Alignment is to center the text vertically.
			MarketValueBox.VerticalContentAlignmentProperty.OverrideMetadata(typeof(MarketValueBox), new FrameworkPropertyMetadata(VerticalAlignment.Center));

			// This property determines whether the value displayed in the dialog is allowed to be set to null.
			MarketValueBox.IsNullableProperty = DependencyProperty.Register(
				"IsNullable",
				typeof(Boolean),
				typeof(MarketValueBox),
				new PropertyMetadata(false));

			// This property determines whether the value displayed in the dialog is allowed to be set to null.
			MarketValueBox.IsNullProperty = DependencyProperty.Register(
				"IsNull",
				typeof(Boolean),
				typeof(MarketValueBox),
				new PropertyMetadata(false));

			// This property is the actual value that is set or read from this control.
			MarketValueBox.ValueProperty = DependencyProperty.Register(
				"Value",
				typeof(Decimal),
				typeof(MarketValueBox),
				new PropertyMetadata(default(Decimal), MarketValueBox.OnValueChanged));

			// This will install a handler to listen for events generated by changing the value in the control.
			MarketValueBox.ValueChangedEvent = EventManager.RegisterRoutedEvent(
				"ValueChanged",
				RoutingStrategy.Bubble,
				typeof(RoutedEventHandler),
				typeof(MarketValueBox));

		}
	
		/// <summary>
		/// Create a new MarketValueBox.
		/// </summary>
		public MarketValueBox()
		{

			// Initialize the object.
			this.originalBorderThickness = default(Thickness);
			this.originalBackground = default(Brush);

			// One problem with the event driven architecture is that it's difficult to initialize the controls with the default value.  There are no events
			// fired because setting the control to it's default value doesn't generate a 'Property Changed' event.  This code here will force the defalt value
			// into the control in the proper format.
			MarketValueBox.OnValueChanged(this, new DependencyPropertyChangedEventArgs(MarketValueBox.ValueProperty, 0.0M, 0.0M));

		}

		/// <summary>
		/// Get or set whether the market value can be null.
		/// </summary>
		public Boolean IsNull
		{
			get { return (Boolean)this.GetValue(MarketValueBox.IsNullProperty); }
			set { this.SetValue(MarketValueBox.IsNullProperty, value); }
		}

		/// <summary>
		/// Get or set whether the market value can be null.
		/// </summary>
		public Boolean IsNullable
		{
			get { return (Boolean)this.GetValue(MarketValueBox.IsNullableProperty); }
			set { this.SetValue(MarketValueBox.IsNullableProperty, value); }
		}

		/// <summary>
		/// Get or set the current integer value of the MarketValueBox.
		/// </summary>
		public Decimal Value
		{
			get { return (Decimal)this.GetValue(MarketValueBox.ValueProperty); }
			set { this.SetValue(MarketValueBox.ValueProperty, value); }
		}

		/// <summary>
		/// Handle losing keyboard focus.
		/// </summary>
		/// <param name="keyboardFocusChangedEventArgs">The event arguments.</param>
		protected override void OnPreviewLostKeyboardFocus(KeyboardFocusChangedEventArgs keyboardFocusChangedEventArgs)
		{

			try
			{

				// Until the current input text passes all the rules for this control the focus is not allowed to move on.
				keyboardFocusChangedEventArgs.Handled = true;

				// Set the value of this control to match the parsed value of text only when writing is enabled.
				if (!this.IsReadOnly)
				{

					// Blank input is handled differently for nullable controls than non-nullable ones.
					if (this.IsNullable)
					{

						// The 'IsNull' flag is set when the user places blank data in the control.
						if (String.IsNullOrEmpty(this.Text))
						{
							this.Value = default(Decimal);
							this.IsNull = true;
						}
						else
						{
							this.Value = Decimal.Parse(this.Text);
							this.IsNull = false;
						}

					}
					else
					{

						// Blank text is not allowed in the control if it is not nullable.
						if (String.IsNullOrEmpty(this.Text))
						{
							throw new Exception(Properties.Resources.NotNullable);
						}
						else
						{
							this.Value = Decimal.Parse(this.Text);
							this.IsNull = false;
						}

					}
				}

				// If we reached here, the focus can be moved.
				keyboardFocusChangedEventArgs.Handled = false;

			}
			catch (Exception exception)
			{

				// Parsing errors are presented to the user directly.
				MessageBox.Show(exception.Message, String.Format(FluidTrade.Guardian.Properties.Settings.Default.ApplicationName));

			}

			// Allow the base class to handle the rest of the event.
			base.OnPreviewLostKeyboardFocus(keyboardFocusChangedEventArgs);

		}

        /// <summary>
        /// Handle the Value of MarketValueBox changing. Change the value of Text to match.
        /// </summary>
        /// <param name="sender">The MarketValueBox.</param>
        /// <param name="eventArgs">The event arguments.</param>
		static private void OnIsReadOnlyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			// Remove the border of the control when in the read-only mode and restore it when editing is allowed.
			MarketValueBox marketValueBox = sender as MarketValueBox;
			if (Convert.ToBoolean(eventArgs.NewValue))
			{
				marketValueBox.originalBorderThickness = marketValueBox.BorderThickness;
				marketValueBox.BorderThickness = new Thickness(0);
				marketValueBox.Background = Brushes.Transparent;
			}
			else
			{
				marketValueBox.Background = marketValueBox.originalBackground;
				marketValueBox.BorderThickness = marketValueBox.originalBorderThickness;
			}

		}

		/// <summary>
		/// Handle the Value of MarketValueBox changing. Change the value of Text to match.
		/// </summary>
		/// <param name="sender">The MarketValueBox.</param>
		/// <param name="eventArgs">The event arguments.</param>
		static private void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			// Display the value formatted for the local currency using the Text property of this control.
			MarketValueBox marketValueBox = sender as MarketValueBox;
			marketValueBox.SetValue(TextBox.TextProperty, String.Format("{0:#,##0.00}", marketValueBox.Value));
			marketValueBox.RaiseEvent(new RoutedEventArgs(MarketValueBox.ValueChangedEvent));

		}

	}

}
