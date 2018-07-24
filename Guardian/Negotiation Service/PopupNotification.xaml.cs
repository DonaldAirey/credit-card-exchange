namespace FluidTrade.Guardian
{

	using System;
	using System.Collections.Generic;
    using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Controls.Primitives;
    using System.Windows.Media;
	using System.Windows.Media.Animation;

    /// <summary>
	/// Handles a pop-up window that dislays notification that a counter party has been found for a possible trade.
	/// </summary>
	public partial class PopupNotification : Popup
	{

		// Private Enumerations
		private enum AnimationState { Initial, Opening, Waiting, Closing, Closed };

		// Private Constants
		private const Int32 animationPeriod = 500;
		private const Double bottomMargin = 1.0;
		private const Double rightMargin = 17.0;
		private const Double fullHeight = 116.0;

		// Private Static Fields
		private static Duration duration;
		private static List<Int32> slots;

		// Private Instance Fields
		private ImageSource logo;
		private Guid matchId;
		private MediaPlayer mediaPlayer;
		private Int32 slotNumber;
		private String symbol;

		// Public Events
		internal MatchEventHandler Accept;
		internal MatchEventHandler Decline;
		internal EventHandler ChangeOptions;

		/// <summary>
		/// Initialize the static resources used by the MarkThree.Guardian.PopupNotification window.
		/// </summary>
		static PopupNotification()
		{

			// To prevent notifications from appearing on top of each other when there are multiple notifications, each one is given a 'slot' to indicate its
			// position on the screen.
			PopupNotification.slots = new List<int>();

			// This indicates how long each notification takes to appear or disappear.
			PopupNotification.duration = new Duration(TimeSpan.FromMilliseconds(PopupNotification.animationPeriod));

		}

		/// <summary>
		/// Creates a window that notifies the user of a potential match.
		/// </summary>
		public PopupNotification()
		{

			// Initialize the IDE maintained elements of the notification window.
			InitializeComponent();

			// This Popup is placed in an absolute location on the screen relative to the upper left hand corner.  Note that the height of the control is fixed
			// at zero.  Note also that the control is animated, so its something of a philisophical discussion as to what the proper 'Height' of the control
			// is.  Using zero allows the first Popup to be placed at the proper position in the lower left hand corner of the screen.  If the full height of
			// the control was used then the placement logic would try to move it to the edge of the screen, even though the animation would keep it from spilling
			// over the edge.
			this.Placement = PlacementMode.Absolute;

			// This keeps the notification windows from popping up on top of each other.  The 'slot' is basically the position on the screen where the
			// notification will be posted.  This position is guaranteed to be free of other notification windows because all the used slots are kept in a
			// static list of used slots.
			this.slotNumber = 0;
			while (true)
			{
				int index = PopupNotification.slots.BinarySearch(this.slotNumber);
				if (index < 0)
				{
					PopupNotification.slots.Insert(~index, this.slotNumber);
					break;
				}
				this.slotNumber++;
			}

		}

		/// <summary>
		/// Gets or sets the logo that is displayed in the notification.
		/// </summary>
		public ImageSource Logo
		{
			get { return this.logo; }
			set { this.logo = value; }
		}

		/// <summary>
		/// Gets or sets the unique identifier of the match.
		/// </summary>
		public Guid MatchId
		{
			get { return this.matchId; }
			set { this.matchId = value; }
		}

		/// <summary>
		/// Gets or sets the symbol displayed in the notification window.
		/// </summary>
		public string Symbol
		{
			get { return this.symbol; }
			set { this.symbol = value; }
		}

		/// <summary>
		/// Gets or sets the message displayed in the notification window.
		/// </summary>
		public string Title
		{
			get { return this.textBlockTitle.Text; }
			set { this.textBlockTitle.Text = value; }
		}

		/// <summary>
		/// Closes the notification window.
		/// </summary>
		public void Close()
		{

			// Animating the height makes it appear that the popup is hiding itself again.
			DoubleAnimation heightAnimation = new DoubleAnimation();
			heightAnimation.From = this.Height;
			heightAnimation.To = 0.0;
			heightAnimation.Duration = PopupNotification.duration;
			this.BeginAnimation(Popup.HeightProperty, heightAnimation);

			// Animating the vertical offset makes it appear that the popup is hiding itself again.
			DoubleAnimation verticalOffsetAnimation = new DoubleAnimation();
			verticalOffsetAnimation.From = this.VerticalOffset;
			verticalOffsetAnimation.To = this.VerticalOffset + PopupNotification.fullHeight;
			verticalOffsetAnimation.Duration = PopupNotification.duration;
			verticalOffsetAnimation.Completed += new EventHandler(OnClosingAnimationCompleted);
			this.BeginAnimation(Popup.VerticalOffsetProperty, verticalOffsetAnimation);

			// Once the notification has been removed from the screen, the slot is made available for another notification.
			PopupNotification.slots.RemoveAt(PopupNotification.slots.BinarySearch(this.slotNumber));

		}

		/// <summary>
		/// Handles the click of the Cancel Button.
		/// </summary>
		/// <param name="sender">The object that originated this event.</param>
		/// <param name="e">The event arguments.</param>
		private void OnButtonCancelClick(object sender, EventArgs e)
		{

			// Immediately close the notification window.
			this.IsOpen = false;

			// Advise any listeners that the negotiation has been rejected.
			if (this.Decline != null)
				this.Decline(this, new MatchEventArgs(this.matchId));

		}

		/// <summary>
		/// Handles the selection of the company logo to accept the negotiation.
		/// </summary>
		/// <param name="sender">The object that originated this event.</param>
		/// <param name="e">The event arguments.</param>
		private void OnButtonLogoClick(object sender, EventArgs e)
		{

			// Immediately close the notification window.
			this.IsOpen = false;

			// Advise any listeners that the negotiation has been rejected.
			if (this.Accept != null)
				this.Accept(this, new MatchEventArgs(this.matchId));

		}

		/// <summary>
		/// Handles the completion of the animation sequence to close the notification window.
		/// </summary>
		/// <param name="sender">The object that originated this event.</param>
		/// <param name="e">The unused event data.</param>
		private void OnClosingAnimationCompleted(object sender, EventArgs e)
		{

			// This indicates that the notification window has completed the animation sequence and is finally closed.
			this.IsOpen = false;

		}

		/// <summary>
		/// Handles the opening of the Popup control.
		/// </summary>
		/// <param name="e">Unused event data.</param>
		protected override void OnOpened(EventArgs e)
		{

			// Play a sound when opening the window.
			this.mediaPlayer = new MediaPlayer();
			mediaPlayer.Open(new Uri("Type.wav", UriKind.Relative));
			mediaPlayer.Play();

			// Find the button used to accept the negotiation and give it the logo of the security that was matched.
			Image image = this.buttonLogo.Template.FindName("imageLogo", this.buttonLogo) as Image;
			image.Source = this.logo;

			// It is critical to calculate the vertical offset using the full height of the notification window.  Tha actual height is likely to be zero as the
			// placement logic needs to work with the shape that the window has now, not the shape it will have when fully realized.  This caused a problem when
			// trying to position the first popup.  Even though the animated height was zero, the realized height of 110 made the Popup arrangement logic think
			// that the window edges were outside of the visible screen and would attempt to move them back.  The placement logic didn't know that the animated
			// height was actually zero.  So, in order to work with the placement logic, the quiescent height of the window is zero.
			this.HorizontalOffset = SystemParameters.WorkArea.Width - this.Width - PopupNotification.rightMargin;
			this.VerticalOffset = SystemParameters.WorkArea.Height - (this.slotNumber + 1) * PopupNotification.fullHeight - PopupNotification.bottomMargin;

			// Animate the height to make it appear that the Popup is growing from the bottom of the screen.
			DoubleAnimation heightAnimation = new DoubleAnimation();
			heightAnimation.From = 0.0;
			heightAnimation.To = PopupNotification.fullHeight;
			heightAnimation.Duration = PopupNotification.duration;
			this.BeginAnimation(Popup.HeightProperty, heightAnimation);

			// Animate the vertical offset to make it appear that the Popup is growing from the bottom of the screen.
			DoubleAnimation verticalOffsetAnimation = new DoubleAnimation();
			verticalOffsetAnimation.From = this.VerticalOffset + PopupNotification.fullHeight;
			verticalOffsetAnimation.To = this.VerticalOffset;
			verticalOffsetAnimation.Duration = PopupNotification.duration;
			this.BeginAnimation(Popup.VerticalOffsetProperty, verticalOffsetAnimation);

			// Allow the base class to finish any remaning initialization.
			base.OnOpened(e);

		}

		/// <summary>
		/// Handles the selection of the "Options" text.
		/// </summary>
		/// <param name="sender">The object that originated this event.</param>
		/// <param name="e">The event arguments.</param>
		private void OnTextBlockOptionsClick(object sender, EventArgs e)
		{

			// Broadcast the event to anyone listening.
			if (this.ChangeOptions != null)
				this.ChangeOptions(this, EventArgs.Empty);

		}

	}

}
