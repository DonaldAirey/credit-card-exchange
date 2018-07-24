namespace FluidTrade.Core.Windows.Controls
{

	using System;
	using System.IO;
	using System.Windows;
    using System.Windows.Media;
	using System.Windows.Media.Imaging;

    public class BitmapString : BitmapSource
	{

		// Public Static Properties
		public static System.Windows.DependencyProperty SourceProperty;

		// Private Static Properties
		private System.Double dpiX;
		private System.Double dpiY;
		private System.Double height;
		private System.Double width;
		private System.Int32 pixelWidth;
		private System.Int32 pixelHeight;
		private System.Windows.Media.Imaging.BitmapPalette bitmapPalette;
		private System.Windows.Media.PixelFormat pixelFormat;

		static BitmapString()
		{

			BitmapString.SourceProperty = DependencyProperty.Register("Source", typeof(string), typeof(BitmapString),
				new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.None, new PropertyChangedCallback(OnSourceChanged)));

		}

		public string Source
		{

			get { return this.GetValue(BitmapString.SourceProperty) as string; }
			set { this.SetValue(BitmapString.SourceProperty, value); }

		}

		private static void OnSourceChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{

			try
			{

				string sourceString = dependencyPropertyChangedEventArgs.NewValue as string;
				MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(sourceString));
				BitmapDecoder bitmapDecoder = BitmapDecoder.Create(memoryStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);

				BitmapString bitmapString = sender as BitmapString;
				BitmapSource bitmapSource = bitmapDecoder.Frames[0];

				// The BitmapSource that was decoded from the string is used to seed this bitmapSource.  Cloning, however, is not 
				// enought to initialize the BitmapSource: none of the basic values are provided as properties.  They need to be
				// copied out of the generated bitmap.
				bitmapString.CloneCore(bitmapSource);
				bitmapString.dpiX = bitmapSource.DpiX;
				bitmapString.dpiY = bitmapSource.DpiY;
				bitmapString.pixelFormat = bitmapSource.Format;
				bitmapString.height = bitmapSource.Height;
				bitmapString.width = bitmapSource.Width;
				bitmapString.pixelHeight = bitmapSource.PixelHeight;
				bitmapString.pixelWidth = bitmapSource.PixelWidth;
				bitmapString.bitmapPalette = bitmapSource.Palette;

			}
			catch { }

		}

		public override double DpiX { get { return this.dpiX; } }

		public override double DpiY { get { return this.dpiY; } }

		public override double Height { get { return this.height; } }

		public override double Width { get { return this.width; } }

		public override PixelFormat Format { get { return this.pixelFormat; } }

		public override BitmapPalette Palette { get { return this.bitmapPalette; } }

		public override int PixelHeight { get { return this.pixelHeight; } }

		public override int PixelWidth { get { return this.pixelWidth; } }

		protected override Freezable CreateInstanceCore() { return null; }

	}

}
