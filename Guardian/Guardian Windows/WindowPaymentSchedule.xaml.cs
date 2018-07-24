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
	using FluidTrade.Core;
	using System.ServiceModel.Security;
	using System.Windows.Threading;

	/// <summary>
	/// Interaction logic for WindowPaymentSchedule.xaml
	/// </summary>
	public partial class WindowPaymentSchedule : Window
	{

		/// <summary>
		/// Indicates the CommissionSchedule dependency property.
		/// </summary>
		public static readonly DependencyProperty EntityProperty = DependencyProperty.Register(
			"Entity",
			typeof(Blotter),
			typeof(WindowPaymentSchedule),
			new PropertyMetadata() { CoerceValueCallback = WindowPaymentSchedule.CoerceEntity });

		/// <summary>
		/// Create a new schedule window.
		/// </summary>
		public WindowPaymentSchedule()
		{
			InitializeComponent();
		}

		/// <summary>
		/// The payment schedule that the control is editing.
		/// </summary>
		public Blotter Entity
		{

			get { return this.GetValue(WindowPaymentSchedule.EntityProperty) as Blotter; }
			set { this.SetValue(WindowPaymentSchedule.EntityProperty, value); }

		}

		/// <summary>
		/// Apply changes.
		/// </summary>
		/// <param name="entity">The entity to apply changes to.</param>
		private void Apply(Blotter entity)
		{

			try
			{

				entity.Commit();

				this.Dispatcher.BeginInvoke(
					new Action(() =>
						this.Entity.Reset()),
					DispatcherPriority.Normal);

			}
			catch (SecurityAccessDeniedException)
			{

				this.Dispatcher.BeginInvoke(new Action(delegate()
				{

					MessageBox.Show(this, Properties.Resources.CommitFailedAccessDenied, this.Title);

				}), DispatcherPriority.Normal);

			}
			catch (Exception exception)
			{

				this.Dispatcher.BeginInvoke(new Action(delegate()
				{

					MessageBox.Show(this, Properties.Resources.OperationFailed, this.Title);

				}), DispatcherPriority.Normal);
				EventLog.Warning("Unknown error in entity commit: {0}: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);

			}

		}

		/// <summary>
		/// Determine whether the Apply button can be pushed.
		/// </summary>
		/// <param name="sender">The apply button.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void CanApply(object sender, CanExecuteRoutedEventArgs eventArgs)
		{

			eventArgs.CanExecute = this.Entity == null? false : this.Entity.Modified;

		}

		/// <summary>
		/// Make sure we're editing a own copy of the blotter.
		/// </summary>
		/// <param name="sender">The schedule window.</param>
		/// <param name="baseValue">The blotter the property was set to.</param>
		/// <returns>A clone of the value.</returns>
		private static Entity CoerceEntity(DependencyObject sender, object baseValue)
		{

			Blotter clone = (baseValue as Blotter).Clone() as Blotter;

			clone.Reset();

			return clone;

		}

		/// <summary>
		/// Handle the apply command.
		/// </summary>
		/// <param name="sender">The apply button.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnApply(object sender, EventArgs eventArgs)
		{

			ThreadPoolHelper.QueueUserWorkItem(
				data =>
					this.Apply(data as Blotter),
				this.Entity.Clone());

		}

		/// <summary>
		/// Handle the cancel command.
		/// </summary>
		/// <param name="sender">The cancel button.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnCancel(object sender, EventArgs eventArgs)
		{

			this.Close();

		}

		/// <summary>
		/// Handle the cancel command.
		/// </summary>
		/// <param name="sender">The cancel button.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnOK(object sender, EventArgs eventArgs)
		{

			ThreadPoolHelper.QueueUserWorkItem(
				data =>
					this.Apply(data as Blotter),
				this.Entity.Clone());
			this.Close();

		}

	}

}
