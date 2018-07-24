namespace FluidTrade.Guardian.Windows.Controls
{

	using System;
	using System.ComponentModel;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Data;
	using System.Windows.Media;
	using FluidTrade.Core;
	using System.Windows.Threading;

	/// <summary>
	/// A status bar for displaying entities.
	/// </summary>
	public class EntityStatusBar : StatusBar
	{

		/// <summary>
		/// Identifies the Entity dependency property.
		/// </summary>
		public static readonly DependencyProperty EntityProperty =
			DependencyProperty.Register("Entity", typeof(Entity), typeof(EntityStatusBar), new PropertyMetadata(null, EntityStatusBar.OnEntityChanged));

		private TextBlock entityType;
		private Double progressMinimum;
		private ProgressBar progressBar;
		private TextBlock progressLabel;

		/// <summary>
		/// Create a new status bar.
		/// </summary>
		public EntityStatusBar()
		{

			TextBlock entityName = new TextBlock()
			{
				FontSize = 12.5,
				HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
				Margin = new Thickness(5, 0, 10, 1),
				Visibility = System.Windows.Visibility.Visible
			};
			TextBlock dateModified = new TextBlock()
			{
				Text = "Date modified: ",
				FontSize = 12,
				Foreground = new SolidColorBrush(Colors.Gray),
				HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
				VerticalAlignment = System.Windows.VerticalAlignment.Center,
				Margin = new Thickness(5, 0, 10, 0)
			};
			TextBlock entityDate = new TextBlock()
			{
				FontSize = 12,
				HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
				VerticalAlignment = System.Windows.VerticalAlignment.Center,
				Margin = new Thickness(0, 0, 10, 2)
			};

			this.SetBinding(StatusBar.DataContextProperty, new Binding("Entity") { Source = this });

			// Build the entity name.
			entityName.SetValue(Grid.ColumnSpanProperty, 2);
			entityName.SetBinding(TextBlock.TextProperty, new Binding("Name"));
			base.Children.Add(new FlowGridRow() { entityName });

			// Build the entity type.
			this.entityType = new TextBlock()
			{
				FontSize = 12.5,
				HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
				Margin = new Thickness(5, 0, 10, 1),
				Visibility = System.Windows.Visibility.Visible
			};
			this.entityType.SetValue(Grid.ColumnSpanProperty, 2);
			this.entityType.SetBinding(TextBlock.TextProperty, new Binding("TypeName"));
			base.Children.Add(new FlowGridRow() { entityType });

			// Build the progress bar.
			this.progressLabel = new TextBlock()
			{
				Text = "Loading data: ",
				FontSize = 12,
				Foreground = new SolidColorBrush(Colors.Gray),
				HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
				VerticalAlignment = System.Windows.VerticalAlignment.Center,
				Visibility = Visibility.Collapsed,
				Margin = new Thickness(5, 0, 10, 0)
			};
			this.progressBar = new ProgressBar()
			{
				Margin = new Thickness(0, 0, 10, 2),
				Name = "progressBar",
				Height = 15,
				Width = 140,
				Maximum = 1,
				Minimum = 0,
				Value = .5,
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Center,
				Visibility = Visibility.Collapsed
			};
			base.Children.Add(new FlowGridRow() { progressLabel, progressBar });

			// Setup the modified time text.
			entityDate.SetBinding(TextBlock.TextProperty, new Binding("ModifiedTime"));
			base.Children.Add(new FlowGridRow() { dateModified, entityDate });

			this.SetBinding(EntityStatusBar.IconProperty, new Binding("ImageSource"));

			this.progressMinimum = Double.MinValue;
			this.Loaded += new RoutedEventHandler(OnLoaded);
			this.Unloaded += new RoutedEventHandler(OnUnloaded);
		}

		/// <summary>
		/// Add the progress event handler when the status bar is loaded.
		/// </summary>
		/// <param name="sender">The status bar control.</param>
		/// <param name="e">The event arguments.</param>
		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			DataModel.Progress += OnProgress;
		}

		/// <summary>
		/// Remove the progress event handler when the status bar is unloaded.
		/// </summary>
		/// <param name="sender">The status bar control.</param>
		/// <param name="e">The event arguments.</param>
		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			DataModel.Progress -= OnProgress;			
		}

		
		/// <summary>
		/// The entity whose properties we will display.
		/// </summary>
		public Entity Entity
		{

			get { return this.GetValue(EntityStatusBar.EntityProperty) as Entity; }
			set { this.SetValue(EntityStatusBar.EntityProperty, value); }

		}

		/// <summary>
		/// Handle the Entity changing.
		/// </summary>
		/// <param name="sender">The WindowEntityProperties.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private static void OnEntityChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			EntityStatusBar statusBar = sender as EntityStatusBar;

			statusBar.OnEntityChanged(eventArgs);

		}

		/// <summary>
		/// Handle the entity changing.
		/// </summary>
		/// <param name="eventArgs">The event arguments.</param>
		protected virtual void OnEntityChanged(DependencyPropertyChangedEventArgs eventArgs)
		{

			Entity entity = eventArgs.NewValue as Entity;

			if (eventArgs.OldValue != null)
				(eventArgs.OldValue as Entity).PropertyChanged -= this.OnEntityPropertyChanged;

			this.DataContext = entity;
			entity.PropertyChanged += this.OnEntityPropertyChanged;

		}

		/// <summary>
		/// Handle changes to the current entity.
		/// </summary>
		/// <param name="sender">The entity itself.</param>
		/// <param name="e">The event arguments.</param>
		protected virtual void OnEntityPropertyChanged(object sender, PropertyChangedEventArgs e)
		{


		}

		/// <summary>
		/// Handle progress updates.
		/// </summary>
		/// <param name="sender">The data model.</param>
		/// <param name="progressEventArgs">The event arguments.</param>
		private void OnProgress(object sender, ProgressEventArgs progressEventArgs)
		{

			// Remove the progress bar when the client has reconciled itself to the server.  When the client is behind the server, initialize and display the
			// progress.
			if (progressEventArgs.Current == progressEventArgs.Maximum)
			{

				// The 'progressMinimum' is the starting point for the progress bar.  It is set the first time the client falls behind the server.
				this.progressMinimum = Double.MinValue;

				// This will hide the progress bar when the client has caught up to the server.
				if (this.progressBar.Visibility == Visibility.Visible)
					this.Dispatcher.BeginInvoke(
						DispatcherPriority.Normal,
						(Action)delegate
							{
								this.progressBar.Visibility = Visibility.Collapsed;
								this.progressLabel.Visibility = Visibility.Collapsed;
							});

			}
			else
			{

				// The visual elements of the progress bar are initialized the first time the client falls behind the server.

				// The progress of the reconcilliation is updated in the foreground thread.
				this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)delegate
					{
						if (this.progressMinimum == Double.MinValue)
						{

							this.progressMinimum = progressEventArgs.Current - progressEventArgs.Current * 0.1;
							this.progressBar.Visibility = Visibility.Visible;
							this.progressLabel.Visibility = Visibility.Visible;
						}

						this.progressBar.Minimum = this.progressMinimum;
						this.progressBar.Maximum = progressEventArgs.Maximum;
						this.progressBar.Value = progressEventArgs.Current;
					});

			}

		}


	}
}
