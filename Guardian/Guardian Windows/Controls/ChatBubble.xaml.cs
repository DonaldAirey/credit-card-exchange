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
using System.Windows.Threading;
using System.IO;

namespace FluidTrade.Guardian.Windows.Controls
{
    /// <summary>
    /// Interaction logic for ChatBubble.xaml
    /// </summary>
    public partial class ChatBubble : UserControl
    {

        // Public Enumerations

        /// <summary>
        /// Describes which side of the chat is displayed.
        /// </summary>
        public enum PartyEnum
		{
			/// <summary>
			/// Describes the chat bubble items that belong to you.
			/// </summary>
			You,
			/// <summary>
			/// Describes the chat bubble items that belong to the counter party.
			/// </summary>
			Contra
		};

        // Public Classes

        /// <summary>
        /// Object that describes an incoming chat message.
        /// </summary>
        public class MessagePackage
        {

			/// <summary>
			/// The source of the dialog item.
			/// </summary>
            public PartyEnum Source { get; set; }

			/// <summary>
			/// The message displayed in the chat dialog.
			/// </summary>
            public string Message { get; set; }

            /// <summary>
            /// Overloaded Constructor
            /// </summary>
            /// <param name="source">Is this the You or Contra</param>
            /// <param name="message">Text of the chat message</param>
            public MessagePackage(PartyEnum source, string message)
            {
                Source = source;
                Message = message;
            }
        }

        // Dependency Properties to be used in foreground threads only.

		/// <summary>
		/// A bubble in the chat dialog window.
		/// </summary>
		public static DependencyProperty ChatObjectProperty = DependencyProperty.Register("ChatObject", typeof(MessagePackage), typeof(ChatBubble));
		
		/// <summary>
		///A property to limit the width of the property of the console. 
		/// </summary>
        public static DependencyProperty LimitWidthProperty = DependencyProperty.Register("LimitWidth", typeof(double), typeof(ChatBubble));


        /// <summary>
        /// Object that describes the chat. 
        /// </summary>
        public MessagePackage ChatObject
        {
            get { return (MessagePackage)this.GetValue(ChatBubble.ChatObjectProperty); }
            set { this.SetValue(ChatBubble.ChatObjectProperty, value); }
        }

        /// <summary>
        /// Sets the maximum size of child controls.
        /// </summary>
        public double LimitWidth
        {
            get { return (double)this.GetValue(ChatBubble.LimitWidthProperty); }
            set { this.SetValue(ChatBubble.LimitWidthProperty, value); }
        }

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public ChatBubble()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handle the event when the container is changed.  This will allow us to always get the latest size of the parent.
        /// </summary>
        /// <param name="sender">The object that originated the event.</param>
        /// <param name="e">The event arguments.</param>
        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Set all the visual components in this control to be limited to only 75% of the container size.
            LimitWidth = e.NewSize.Width * 0.75;
        }

    }

    /// <summary>
    /// Convert PartyEnum to HorizontalAlignment
    /// </summary>
    public class ConvertPartyToJustification : IValueConverter
    {

		/// <summary>
		/// Converts a party type to justification instructions in the chat dialog.
		/// </summary>
		/// <param name="value">The value to be displayed.</param>
		/// <param name="targetType">The CLR type of the item to be displayed.</param>
		/// <param name="parameter">Additional information for the dislay.</param>
		/// <param name="culture">The culture for the dispaly.</param>
		/// <returns>The alignment of the chat bubble.</returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ChatBubble.PartyEnum party = (ChatBubble.PartyEnum)value;

            if (party == ChatBubble.PartyEnum.You)
                return HorizontalAlignment.Right;
            else
                return HorizontalAlignment.Left;
        }

        /// <summary>
        /// This method is not supported.  Usage will cause a NotImplementedException exception being thrown.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }

    /// <summary>
    /// Convert Party definition from ChatBubble into the BubbleDecorator class.
    /// </summary>
    public class ConvertPartyToChatParty : IValueConverter
    {
        /// <summary>
        /// Converts the negotiation party between the ChatBubble object and the Negotiation client.
        /// </summary>
        /// <param name="value">The value to be displayed.</param>
        /// <param name="targetType">The CLR type of the item to be displayed.</param>
        /// <param name="parameter">Additional information for the dislay.</param>
        /// <param name="culture">The culture for the dispaly.</param>
        /// <returns>The alignment of the chat bubble.</returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ChatBubble.PartyEnum party = (ChatBubble.PartyEnum)value;

            if (party == ChatBubble.PartyEnum.You)
                return BalloonDecorator.ChatPartyEnum.You;
            else
                return BalloonDecorator.ChatPartyEnum.Contra;
        }

        /// <summary>
        /// This method is not supported.  Usage will cause a NotImplementedException exception being thrown.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// A class derived from Canvas in order to override the Measure, Arrange and Render functions for customized drawing.
    /// </summary>
    public class BalloonDecorator : Canvas
    {

        // Private Fields
		private static BitmapSource rightChatBubble = null;
		private static BitmapSource leftChatBubble = null;

        // Public Enumerations.

        /// <summary>
        /// An enumeration that identifies the side of the negotiation.
        /// </summary>
        public enum ChatPartyEnum
        {
            /// <summary>
            /// Identifies the originator of a negotiation.
            /// </summary>
            You,
            /// <summary>
            /// Identifies the counterparty of a negotiation.
            /// </summary>
            Contra
        }

        // Dependency Properties

        /// <summary>
        /// Dependency property for ChatPartyEnum object.
        /// </summary>
        public static readonly DependencyProperty ChatPartyProperty =
            DependencyProperty.Register("ChatParty", typeof(ChatPartyEnum), typeof(BalloonDecorator),
            new FrameworkPropertyMetadata(ChatPartyEnum.You, FrameworkPropertyMetadataOptions.AffectsRender));

        // Public Fields

        /// <summary>
        /// Defines the side of the Party.
        /// </summary>
        public ChatPartyEnum ChatParty
        {
            get { return (ChatPartyEnum)GetValue(ChatPartyProperty); }
            set { SetValue(ChatPartyProperty, value); }
        }


		/// <summary>
		/// Default constructor.
		/// </summary>
		static BalloonDecorator()
		{
			System.Drawing.Bitmap bitmap = (System.Drawing.Bitmap)FluidTrade.Guardian.Properties.Resources.ChatRightBubble.Clone();
			bitmap.MakeTransparent(bitmap.GetPixel(0, 0));
			rightChatBubble = LoadBitmap(bitmap);

			bitmap = (System.Drawing.Bitmap)FluidTrade.Guardian.Properties.Resources.ChatLeftBubble.Clone();
			bitmap.MakeTransparent(bitmap.GetPixel(0, 0));
			leftChatBubble = LoadBitmap(bitmap);
		}

        /// <summary>
        /// Default constructor.
        /// </summary>
        public BalloonDecorator()
        {
			
		}

		/// <summary>
		/// Convert a System.Drawing.Bitmap to a BitmapSource
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		public static BitmapSource LoadBitmap(System.Drawing.Bitmap source)
		{
			return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(source.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
				System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
		}

        /// <summary>
        /// Arrange the location of child controls.
        /// </summary>
        /// <param name="arrangeSize"></param>
        /// <returns></returns>
        protected override Size ArrangeOverride(Size arrangeSize)
        {

            foreach (UIElement child in base.InternalChildren)
            {
                Rect innerRect = new Rect(0, 0, arrangeSize.Width, arrangeSize.Height);

                child.Arrange(innerRect);
            }

            return arrangeSize;

        }

        /// <summary>
        /// Measure the space required to lay this control out.
        /// </summary>
        /// <param name="constraint"></param>
        /// <returns></returns>
        protected override Size MeasureOverride(Size constraint)
        {

            Size childSize = new Size();
            foreach (UIElement child in base.InternalChildren)
            {

                Size size = new Size(constraint.Width, constraint.Height);
                child.Measure(size);
                childSize.Width = child.DesiredSize.Width;
                childSize.Height = child.DesiredSize.Height;

            }

            return childSize;

        }

        /// <summary>
        /// Actual rendering of the content of this control.
        /// </summary>
        /// <param name="dc"></param>
        protected override void OnRender(DrawingContext dc)
        {

            Rect rect = new Rect(0, 0, RenderSize.Width, RenderSize.Height);

            if (ChatParty == ChatPartyEnum.You)
                dc.DrawImage(rightChatBubble, rect);
            else
                dc.DrawImage(leftChatBubble, rect);

        }

    }

}
