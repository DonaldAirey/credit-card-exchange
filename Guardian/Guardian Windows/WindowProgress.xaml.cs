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
	using System.Timers;
	using System.Windows.Threading;

	/// <summary>
	/// Interaction logic for WindowProgress.xaml
	/// </summary>
	public partial class WindowProgress : Window
	{

		/// <summary>
		/// Indicates the Header dependency property.
		/// </summary>
		public static readonly DependencyProperty HeaderProperty =
			DependencyProperty.Register("Header", typeof(string), typeof(WindowProgress));
		/// <summary>
		/// Indicates the IsIndeterminate dependency property.
		/// </summary>
		public static readonly DependencyProperty IsIndeterminateProperty =
			DependencyProperty.Register("IsIndeterminate", typeof(Boolean), typeof(WindowProgress), new PropertyMetadata(true));
		/// <summary>
		/// Indicates the Maximum dependency property.
		/// </summary>
		public static readonly DependencyProperty MaximumProperty =
			DependencyProperty.Register("Maximum", typeof(double), typeof(WindowProgress), new PropertyMetadata(1.0));
		/// <summary>
		/// Indicates the Message dependency property.
		/// </summary>
		public static readonly DependencyProperty MessageProperty =
			DependencyProperty.Register("Message", typeof(string), typeof(WindowProgress));
		/// <summary>
		/// Indicates the Minimum dependency property.
		/// </summary>
		public static readonly DependencyProperty MinimumProperty =
			DependencyProperty.Register("Minimum", typeof(double), typeof(WindowProgress), new PropertyMetadata(0.0));
		/// <summary>
		/// Indicates the TimeLeftVisibility dependency property.
		/// </summary>
		public static readonly DependencyProperty TimeLeftVisibilityProperty =
			DependencyProperty.Register("TimeLeftVisibility", typeof(Visibility), typeof(WindowProgress), new PropertyMetadata(Visibility.Hidden));
		/// <summary>
		/// Indicates the Time dependency property.
		/// </summary>
		public static readonly DependencyProperty TimeProperty =
			DependencyProperty.Register("Time", typeof(TimeSpan), typeof(WindowProgress));
		/// <summary>
		/// Indicates the TimeLeft dependency property.
		/// </summary>
		public static readonly DependencyProperty TimeLeftProperty =
			DependencyProperty.Register("TimeLeft", typeof(TimeSpan), typeof(WindowProgress), new PropertyMetadata(TimeSpan.MaxValue));
		/// <summary>
		/// Indicates the Value dependency property.
		/// </summary>
		public static readonly DependencyProperty ValueProperty =
			DependencyProperty.Register("Value", typeof(double), typeof(WindowProgress), new PropertyMetadata(0.0));

		/// <summary>
		/// Initialize the window.
		/// </summary>
		public WindowProgress()
		{

			InitializeComponent();
	
		}

		/// <summary>
		/// Gets or sets the message shown above the ProgressBar.
		/// </summary>
		public string Header
		{

			get { return this.GetValue(WindowProgress.HeaderProperty) as string; }
			set { this.SetValue(WindowProgress.HeaderProperty, value); }

		}

		/// <summary>
		/// Gets or sets whether the ProgressBar shows actual values or generic, continuous progress feedback.
		/// </summary>
		public Boolean IsIndeterminate
		{

			get { return (Boolean)this.GetValue(WindowProgress.IsIndeterminateProperty); }
			set { this.SetValue(WindowProgress.IsIndeterminateProperty, value); }

		}

		/// <summary>
		/// Gets or sets the highest possible Value of the ProgressBar.
		/// </summary>
		public double Maximum
		{

			get { return (double)this.GetValue(WindowProgress.MaximumProperty); }
			set { this.SetValue(WindowProgress.MaximumProperty, value); }

		}

		/// <summary>
		/// Gets or sets the message shown above the ProgressBar.
		/// </summary>
		public string Message
		{

			get { return this.GetValue(WindowProgress.MessageProperty) as string; }
			set { this.SetValue(WindowProgress.MessageProperty, value); }

		}

		/// <summary>
		/// Gets or sets the lowest possible Value of the ProgressBar.
		/// </summary>
		public double Minimum
		{

			get { return (double)this.GetValue(WindowProgress.MinimumProperty); }
			set { this.SetValue(WindowProgress.MinimumProperty, value); }

		}

		/// <summary>
		/// The visibility of the time left information.
		/// </summary>
		public Visibility TimeLeftVisibility
		{

			get { return (Visibility)this.GetValue(WindowProgress.TimeLeftVisibilityProperty); }
			set { this.SetValue(WindowProgress.TimeLeftVisibilityProperty, value); }

		}

		/// <summary>
		/// Gets or sets the total time spent.
		/// </summary>
		public TimeSpan Time
		{

			get { return (TimeSpan)this.GetValue(WindowProgress.TimeProperty); }
			set { this.SetValue(WindowProgress.TimeProperty, value); }

		}

		/// <summary>
		/// Gets or sets the approximate time left.
		/// </summary>
		public TimeSpan TimeLeft
		{

			get { return (TimeSpan)this.GetValue(WindowProgress.TimeLeftProperty); }
			set { this.SetValue(WindowProgress.TimeLeftProperty, value); }

		}

		/// <summary>
		/// Gets or sets the value of the ProgressBar.
		/// </summary>
		public double Value
		{

			get { return (double)this.GetValue(WindowProgress.ValueProperty); }
			set { this.SetValue(WindowProgress.ValueProperty, value); }

		}

		/// <summary>
		/// Handle the Cancel event.
		/// </summary>
		/// <param name="sender">The Cancel button.</param>
		/// <param name="eventArgs">The event arguments.</param>
		protected virtual void OnCancel(object sender, EventArgs eventArgs)
		{

			this.Close();

		}

	}

}
