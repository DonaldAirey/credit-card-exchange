namespace FluidTrade.Core.Windows.Controls
{

	using System;
    using System.Windows;
	using System.Windows.Controls;

	/// <summary>
	/// Displays the instructions for executing an order.
	/// </summary>
	public class OrderTypeBlock : Control
	{

		/// <summary>
		/// Identifies the LimitPrice dependency property.
		/// </summary>
		public static readonly DependencyProperty LimitPriceProperty;

		/// <summary>
		/// Identifies the Mnemonic dependency property.
		/// </summary>
		public static readonly DependencyProperty MnemonicProperty;

		/// <summary>
		/// Identifies the OrderType dependency property.
		/// </summary>
		public static readonly DependencyProperty OrderTypeProperty;

		/// <summary>
		/// Identifies the StopPrice dependency property.
		/// </summary>
		public static readonly DependencyProperty StopPriceProperty;

		/// <summary>
		/// Identifies the TextAlignment dependency property.
		/// </summary>
		public static readonly DependencyProperty TextAlignmentProperty;

		/// <summary>
		/// Identifies the Text dependency property.
		/// </summary>
		public static readonly DependencyProperty TextProperty;

		/// <summary>
		/// Creates the static resources used by this control.
		/// </summary>
		static OrderTypeBlock()
		{

			// HorizontalContentAlignment Property
			Control.HorizontalContentAlignmentProperty.AddOwner(typeof(OrderTypeBlock), new FrameworkPropertyMetadata(OnHorizontalContentAlignmentChanged));

			// LimitPrice Property
			OrderTypeBlock.LimitPriceProperty = DependencyProperty.Register(
				"LimitPrice",
				typeof(Decimal),
				typeof(OrderTypeBlock),
				new FrameworkPropertyMetadata(OnContentChanged));

			// Mnemonic Property
			OrderTypeBlock.MnemonicProperty = DependencyProperty.Register(
				"Mnemonic",
				typeof(String),
				typeof(OrderTypeBlock),
				new FrameworkPropertyMetadata(OnContentChanged));

			// OrderType Property
			OrderTypeBlock.OrderTypeProperty = DependencyProperty.Register(
				"OrderType",
				typeof(OrderType),
				typeof(OrderTypeBlock),
				new FrameworkPropertyMetadata(OrderType.Market, OnContentChanged));

			// StopPrice Property
			OrderTypeBlock.StopPriceProperty = DependencyProperty.Register(
				"StopPrice",
				typeof(Decimal),
				typeof(OrderTypeBlock),
				new FrameworkPropertyMetadata(OnContentChanged));

			// TextAlignment Property
			OrderTypeBlock.TextAlignmentProperty = DependencyProperty.Register(
				"TextAlignment",
				typeof(TextAlignment),
				typeof(OrderTypeBlock),
				new FrameworkPropertyMetadata(TextAlignment.Left));

			// Text Property
			OrderTypeBlock.TextProperty = DependencyProperty.Register(
				"Text",
				typeof(String),
				typeof(OrderTypeBlock),
				new FrameworkPropertyMetadata(String.Empty));

		}

		/// <summary>
		/// Gets or sets the LimitPrice for this control.
		/// </summary>
		public Decimal LimitPrice
		{
			get { return (Decimal)this.GetValue(OrderTypeBlock.LimitPriceProperty); }
			set { this.SetValue(OrderTypeBlock.LimitPriceProperty, value); }
		}

		/// <summary>
		/// Gets or sets the Mnemonic for this control.
		/// </summary>
		public String Mnemonic
		{
			get { return (String)this.GetValue(OrderTypeBlock.MnemonicProperty); }
			set { this.SetValue(OrderTypeBlock.MnemonicProperty, value); }
		}

		/// <summary>
		/// Gets or sets the OrderType for this control.
		/// </summary>
		public OrderType OrderType
		{
			get { return (OrderType)this.GetValue(OrderTypeBlock.OrderTypeProperty); }
			set { this.SetValue(OrderTypeBlock.OrderTypeProperty, value); }
		}

		/// <summary>
		/// Gets or sets the StopPrice for this control.
		/// </summary>
		public Decimal StopPrice
		{
			get { return (Decimal)this.GetValue(OrderTypeBlock.StopPriceProperty); }
			set { this.SetValue(OrderTypeBlock.StopPriceProperty, value); }
		}

		/// <summary>
		/// Gets or sets the Text for this control.
		/// </summary>
		public String Text
		{
			get { return (String)GetValue(OrderTypeBlock.TextProperty); }
		}

		/// <summary>
		/// Gets or sets the Text Alignment for this control.
		/// </summary>
		public TextAlignment TextAlignment
		{
			get { return (TextAlignment)GetValue(OrderTypeBlock.TextAlignmentProperty); }
		}

		/// <summary>
		/// Formats the content of this control.
		/// </summary>
		private void Format()
		{

			// The order type defines the format of the content.
			switch (this.OrderType)
			{
			case OrderType.Market:
			case OrderType.MarketOnClose:
			case OrderType.OnClose:

				// These Order Types simply show the mnemonic to describe how the order is priced.
				SetValue(OrderTypeBlock.TextProperty, this.Mnemonic);
				break;

			case OrderType.Limit:

				// The simple price is displayed for Limit.
				SetValue(OrderTypeBlock.TextProperty, String.Format("{0:#,##0.00}", this.LimitPrice));
				break;

			case OrderType.Stop:

				// The price and mnemonic are displayed for Stop orders.
				SetValue(OrderTypeBlock.TextProperty, String.Format("{0:#,##0.00} {1}", this.StopPrice, this.Mnemonic));
				break;

			}

		}

		/// <summary>
		/// Translates the HorizontalContentAlignment value into a TextAlignment value.
		/// </summary>
		/// <param name="dependencyObject">The Dependency Property that has changed.</param>
		/// <param name="dependencyPropertyChangedEventArgs">The event arguments describing the property change.</param>
		public static void OnHorizontalContentAlignmentChanged(
			DependencyObject dependencyObject,
			DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{

			// The basic idea here is to translate the HorizontalContentAlignment used by the Control class into a TextAlignment value that is usable by the
			// TextBlock class.  This allows a Control to host a TextBlock with a consistent approach to how the alignment of the content is specified.
			OrderTypeBlock orderTypeBlock = (OrderTypeBlock)dependencyObject;
			HorizontalAlignment horizontalAlignment = (HorizontalAlignment)dependencyPropertyChangedEventArgs.NewValue;

			// Translate the HorizontalContentAlignment into a TextAlignment that TextBlocks can use.
			switch (horizontalAlignment)
			{
			case HorizontalAlignment.Center:

				orderTypeBlock.SetValue(OrderTypeBlock.TextAlignmentProperty, TextAlignment.Center);
				break;

			case HorizontalAlignment.Left:

				orderTypeBlock.SetValue(OrderTypeBlock.TextAlignmentProperty, TextAlignment.Left);
				break;

			case HorizontalAlignment.Right:

				orderTypeBlock.SetValue(OrderTypeBlock.TextAlignmentProperty, TextAlignment.Right);
				break;

			}

		}

		/// <summary>
		/// Handles a change to the LimitPrice property
		/// </summary>
		/// <param name="dependencyObject">The Dependency Property that has changed.</param>
		/// <param name="dependencyPropertyChangedEventArgs">The event arguments describing the property change.</param>
		private static void OnContentChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{

			InputHelper.IsPropertyUpdate = true;
			OrderTypeBlock orderTypeBlock = dependencyObject as OrderTypeBlock;
			orderTypeBlock.Format();
			InputHelper.IsPropertyUpdate = false;

		}

	}

}
