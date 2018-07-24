namespace FluidTrade.Guardian
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
	using FluidTrade.Guardian.Windows;
	using System.Windows.Threading;

	/// <summary>
	/// Interaction logic for WindowMoveError.xaml
	/// </summary>
	public partial class WindowMoveError : Window
	{

		/// <summary>
		/// Indicates the Destination dependency property.
		/// </summary>
		public static readonly DependencyProperty DestinationProperty =
			DependencyProperty.Register("Destination", typeof(Entity), typeof(WindowMoveError), new PropertyMetadata(WindowMoveError.OnDestinationChanged));
		/// <summary>
		/// Indicates the IsDestinationSameAsSource dependency property.
		/// </summary>
		public static readonly DependencyProperty IsDestinationSameAsSourceProperty =
			DependencyProperty.Register("IsDestinationSameAsSource", typeof(Boolean), typeof(WindowMoveError), new PropertyMetadata(WindowMoveError.OnIsDestinationSameAsSourceChanged));
		/// <summary>
		/// Indicates the Source dependency property.
		/// </summary>
		public static readonly DependencyProperty SourceProperty =
			DependencyProperty.Register("Source", typeof(Entity), typeof(WindowMoveError), new PropertyMetadata(WindowMoveError.OnSourceChanged));

		/// <summary>
		/// Create a new entity move error message box.
		/// </summary>
		public WindowMoveError()
		{
	
			InitializeComponent();

		}

		/// <summary>
		/// Gets or sets the entity/folder that is the destination of the move operation.
		/// </summary>
		public Entity Destination
		{

			get { return this.GetValue(WindowMoveError.DestinationProperty) as Entity; }
			set { this.SetValue(WindowMoveError.DestinationProperty, value); }

		}

		/// <summary>
		/// If true, a message indicating that the destination folder is the same as the source folder is displayed. If false, a message indicating
		/// that the destination folder is a subfolder of the entity to be moved.
		/// </summary>
		public Boolean IsDestinationSameAsSource
		{

			get { return (Boolean)this.GetValue(WindowMoveError.IsDestinationSameAsSourceProperty); }
			set { this.SetValue(WindowMoveError.IsDestinationSameAsSourceProperty, value); }

		}

		/// <summary>
		/// Gets or sets the entity that is the entity to be moved.
		/// </summary>
		public Entity Source
		{

			get { return this.GetValue(WindowMoveError.SourceProperty) as Entity; }
			set { this.SetValue(WindowMoveError.SourceProperty, value); }

		}

		/// <summary>
		/// Handle the cancel button being pressed. Just close the window.
		/// </summary>
		/// <param name="sender">The cancel button.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnCancel(object sender, EventArgs eventArgs)
		{

			this.Close();

		}

		/// <summary>
		/// Handle the Destination changing. Fill in the destination fields of the dialog.
		/// </summary>
		/// <param name="sender">The WindowMoveError who owns the property.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private static void OnDestinationChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			WindowMoveError moveError = sender as WindowMoveError;

			if (moveError != null)
			{

				moveError.destinationIcon.Source = moveError.Destination.ImageSource;
				moveError.destinationName.Text = moveError.Destination.Name;

			}

		}

		private static void OnIsDestinationSameAsSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			WindowMoveError moveError = sender as WindowMoveError;

			if (moveError != null)
			{

				moveError.isSubfolder.Visibility = moveError.IsDestinationSameAsSource ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
				moveError.isSameFolder.Visibility = moveError.IsDestinationSameAsSource ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

			}

		}

		/// <summary>
		/// Handle the Source changing. Fill in the source fields of the dialog.
		/// </summary>
		/// <param name="sender">The WindowMoveError who owns the property.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private static void OnSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			WindowMoveError moveError = sender as WindowMoveError;

			if (moveError != null)
			{

				moveError.sourceIcon.Source = moveError.Source.ImageSource;
				moveError.sourceName.Text = moveError.Source.Name;

			}

		}

		/// <summary>
		/// Throw up a move error in the UI thread.
		/// </summary>
		/// <param name="source">The entity that was meant to be move.</param>
		/// <param name="destination">The destination folder for the move.</param>
		/// <param name="isDestinationSameAsSource">What kind of error occurred.</param>
		public static void Show(Entity source, Entity destination, Boolean isDestinationSameAsSource)
		{

			WindowMoveError moveError = new WindowMoveError() {
				Source = source,
				Destination = destination,
				IsDestinationSameAsSource = isDestinationSameAsSource
			};

			moveError.ShowDialog();

		}

	}

}
