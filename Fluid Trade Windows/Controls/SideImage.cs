namespace FluidTrade.Core.Windows.Controls
{

    using System.Windows;
    using System.Windows.Markup;

	/// <summary>
	/// Provides a lightweight control for displaying small amounts of flow content in a user specified format.
	/// </summary>
	[ContentProperty("Content")]
	public class SideImage : System.Windows.Controls.Image
	{

		// Public Static Fields
		public static readonly DependencyProperty ContentProperty;

		/// <summary>
		/// Creates the static resources used by this control.
		/// </summary>
		static SideImage()
		{

			// This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
			DefaultStyleKeyProperty.OverrideMetadata(typeof(SideImage), new FrameworkPropertyMetadata(typeof(SideImage)));

			// The DataProperty contains the raw data for this control.  Note that this property doesn't directly impact 
			// rendering, the measurement or the arrangement of the control.  Instead, the callback method will modify the 'Text'
			// property of the base class which already has triggers for updating the user interface.
			SideImage.ContentProperty = DependencyProperty.Register("Content", typeof(Side), typeof(SideImage));

		}

		/// <summary>
		/// The raw data.
		/// </summary>
		public Side Content
		{
			get { return (Side)GetValue(SideImage.ContentProperty); }
			set { SetValue(SideImage.ContentProperty, value); }
		}

	}

}
