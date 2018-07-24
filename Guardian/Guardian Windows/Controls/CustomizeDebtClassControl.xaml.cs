namespace FluidTrade.Guardian.Windows.Controls
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
	using System.ComponentModel;
	using System.Windows.Threading;
	using System.Threading;

	/// <summary>
	/// Interaction logic for CustomizeDebtClassControl.xaml
	/// </summary>
	public partial class CustomizeDebtClassControl : UserControl
	{

		/// <summary>
		/// Indicates the BankAccountVisibility dependency property.
		/// </summary>
		public static readonly DependencyProperty BankAccountVisibilityProperty =
			DependencyProperty.Register("BankAccountVisibility", typeof(Visibility), typeof(CustomizeDebtClassControl), new PropertyMetadata(Visibility.Collapsed));
		/// <summary>
		/// Indicates the DebtClass dependency property.
		/// </summary>
		public static readonly DependencyProperty DebtClassProperty =
			DependencyProperty.Register("DebtClass", typeof(DebtClass), typeof(CustomizeDebtClassControl), new PropertyMetadata(CustomizeDebtClassControl.OnDebtClassChanged));

		/// <summary>
		/// Indicates the DebtClassChanged routed event.
		/// </summary>
		public static readonly RoutedEvent DebtClassChangedEvent =
			EventManager.RegisterRoutedEvent("DebtClassChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CustomizeDebtClassControl));

		/// <summary>
		/// The event raised when any of the propertiese in the debt class have changed.
		/// </summary>
		public event RoutedEventHandler DebtClassChanged
		{
			add { AddHandler(DebtClassChangedEvent, value); }
			remove { RemoveHandler(DebtClassChangedEvent, value); }
		}

		private delegate void populate(EffectiveDebtClass debtClass);

		/// <summary>
		/// Create the customize debt class control.
		/// </summary>
		public CustomizeDebtClassControl()
		{

			InitializeComponent();
			this.Unloaded += this.OnUnloaded;

		}

		/// <summary>
		/// Gets or sets the the DebtClass we're currently editing.
		/// </summary>
		public Visibility BankAccountVisibility
		{

			set { this.SetValue(CustomizeDebtClassControl.BankAccountVisibilityProperty, value); }
			get { return (Visibility)this.GetValue(CustomizeDebtClassControl.BankAccountVisibilityProperty); }

		}

		/// <summary>
		/// Gets or sets the the DebtClass we're currently editing.
		/// </summary>
		public DebtClass DebtClass
		{

			set { this.SetValue(CustomizeDebtClassControl.DebtClassProperty, value);  }
			get { return this.GetValue(CustomizeDebtClassControl.DebtClassProperty) as DebtClass; }

		}

		/// <summary>
		/// Handle the unloaded event. Dispose of the province list.
		/// </summary>
		/// <param name="sender">The debt class control.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnUnloaded(object sender, EventArgs eventArgs)
		{

		}

		/// <summary>
		/// Handle a child control changing. Raise the DebtClassChanged event.
		/// </summary>
		/// <param name="sender">The control that changed.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnControlChanged(object sender, RoutedEventArgs eventArgs)
		{

			this.RaiseEvent(new RoutedEventArgs(CustomizeDebtClassControl.DebtClassChangedEvent, sender));

		}

		/// <summary>
		/// Handle changes to the actual debt class.
		/// </summary>
		/// <param name="sender">The debtClass object.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private static void OnDebtClassChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			sender.SetValue(CustomizeDebtClassControl.DataContextProperty, eventArgs.NewValue);

		}

	}

}
