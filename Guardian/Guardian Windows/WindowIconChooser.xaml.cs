namespace FluidTrade.Guardian.Windows
{

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
    using System.IO;
    using System.Threading;
	using System.Windows.Threading;

    /// <summary>
    /// Interaction logic for WindowIconChooser.xaml
    /// </summary>
    public partial class WindowIconChooser : Window
    {

        /// <summary>
        /// We'll need the icons ListView to keep track of both the icons' Guid and their actual bitmap, so we'll use this 'pair' to populate the
        /// ListView.
        /// </summary>
		public class IconData
        {

            private ImageSource image = null;

			/// <summary>
			/// The ImageId of the selected image.
			/// </summary>
            public Guid Id { get; set; }
			/// <summary>
			/// The (base64) ImageData of the selected image.
			/// </summary>
            public string ImageData { get; set; }
			/// <summary>
			/// The image source used to display the image.
			/// </summary>
            public ImageSource Image
            {

                get
                {

                    if (this.image == null)
                        this.image = WindowIconChooser.GenerateImage(this.ImageData);
                    return this.image;

                }

            }

        }

        private IconData selected = null;

        private delegate void populate(List<IconData> icons);

        /// <summary>
        /// Create a new icon-chooser window and initialize its components.
        /// </summary>
        public WindowIconChooser()
        {
            InitializeComponent();
            this.iconList.Focus();

            this.Loaded += delegate(object s, RoutedEventArgs e)
            {
                FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(data => this.Populate());
                DataModel.Image.TableNewRow += this.Populate;
                DataModel.Image.RowDeleted += this.Populate;
                DataModel.Image.RowChanged += this.Populate;
            };
            this.Unloaded += delegate(object s, RoutedEventArgs e)
            {
                DataModel.Image.TableNewRow -= this.Populate;
                DataModel.Image.RowDeleted -= this.Populate;
                DataModel.Image.RowChanged -= this.Populate;
            };

        }

        /// <summary>
        /// The id in the Image table of the selected icon. If no icon was selected, SelectedIconId is null.
        /// </summary>
        public Guid? SelectedIconId
        {

            get
            {
                if (this.selected != null)
                    return this.selected.Id;
                else
                    return null;
            }

        }

        /// <summary>
        /// The actual image of the selected icon. If no icon was selected, SelectedIcon is null.
        /// </summary>
        public IconData SelectedIcon
        {

            get
            {

				return this.selected;

            }

        }

        /// <summary>
        /// Fill the icon list with the list of icons from the background.
        /// </summary>
        /// <param name="icons">The new icon list.</param>
        private void Populate(List<IconData> icons)
        {

            IconData selected = this.iconList.SelectedItem == null? null : icons.Find(i => i.Id == (this.iconList.SelectedItem as IconData).Id);

            this.iconList.ItemsSource = icons;
            this.iconList.SelectedItem = null;

            if (selected == null)
                if (this.iconList.Items.Count > 0)
                    this.iconList.SelectedIndex = 0;
            else
                this.iconList.SelectedItem = selected;

        }

        /// <summary>
        /// Retrieve the available icons and push that list to the foreground.
        /// </summary>
        private void Populate()
        {

            try
            {

                lock (DataModel.SyncRoot)
                {

                    var images = DataModel.Image.Select(image =>
                        new IconData { ImageData = image.Image, Id = image.ImageId });

					this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new populate(Populate), images.ToList());

                }

            }
            catch
            {


            }

        }

        /// <summary>
        /// Re-populate the icon list.
        /// </summary>
        /// <param name="sender">The originator of the event.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void Populate(object sender, EventArgs eventArgs)
        {

            FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(data => this.Populate());

        }

        /// <summary>
        /// Generate the image for an icon from it's string-encoded source.
        /// </summary>
        /// <param name="image">The base-64 encoded image.</param>
        /// <returns>The image source for the icon.</returns>
        private static ImageSource GenerateImage(String image)
        {

            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = new MemoryStream(Convert.FromBase64String(image));
            bitmapImage.EndInit();
            return bitmapImage;

        }

        /// <summary>
        /// The user clicked cancel; close the window and cancel the selection.
        /// </summary>
        /// <param name="sender">The cancel button</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void OnCancel(object sender, RoutedEventArgs eventArgs)
        {

            this.Close();

        }

        /// <summary>
        /// The user clicked okay; close the window and remember the selected icon.
        /// </summary>
        /// <param name="sender">The okay button.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void OnOkay(object sender, RoutedEventArgs eventArgs)
        {

            if (this.iconList.SelectedItem != null)
            {

                this.selected = iconList.SelectedItem as IconData;

            }
            this.Close();

        }

    }

}
