namespace FluidTrade.Guardian.Windows.Controls
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using FluidTrade.Actipro;
	using FluidTrade.Core;
	using System.Windows;
	using System.Windows.Data;

	/// <summary>
	/// A text box the user can input a length of time into.
	/// </summary>
	public class TermTextBox : EnumTextBox
	{

		/// <summary>
		/// Indicates the TimeUnitId dependency property.
		/// </summary>
		public static readonly DependencyProperty TimeUnitIdProperty = DependencyProperty.Register(
			"TimeUnitId",
			typeof(Guid),
			typeof(TermTextBox),
			new PropertyMetadata(TermTextBox.OnTimeUnitIdChanged));

		/// <summary>
		/// Raised when the TimeUnitId property changes.
		/// </summary>
		public event DependencyPropertyChangedEventHandler TimeUnitIdChanged;

		/// <summary>
		/// Create a new term text box.
		/// </summary>
		public TermTextBox()
			: base(typeof(TimeUnit))
		{

			this.UnitChanged += this.OnUnitChanged;
			this.Type = typeof(TimeUnit);

		}

		/// <summary>
		/// The TimeUnitId of the current TimeUnit.
		/// </summary>
		public Guid TimeUnitId
		{
			get { return (Guid)this.GetValue(TermTextBox.TimeUnitIdProperty); }
			set { this.SetValue(TermTextBox.TimeUnitIdProperty, value); }
		}

		/// <summary>
		/// Handle the TimeUnitId property changing.
		/// </summary>
		/// <param name="sender">The enum text box whose unit changed.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private static void OnTimeUnitIdChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			TermTextBox box = sender as TermTextBox;
			TimeUnitItem timeUnit = TimeUnitList.Default.Find(box.TimeUnitId);

			if (timeUnit != null)
			{

				box.Unit = timeUnit.TimeUnitCode;

				if (box.TimeUnitIdChanged != null)
					box.TimeUnitIdChanged(sender, eventArgs);

			}

		}

		/// <summary>
		/// Handle the Unit value changing.
		/// </summary>
		/// <param name="sender">The TermTextBox whose Unit changed.</param>
		/// <param name="e">The event arguments.</param>
		private void OnUnitChanged(object sender, DependencyPropertyChangedEventArgs e)
		{

			this.TimeUnitId = TimeUnitList.Default.Find((TimeUnit)this.Unit).TimeUnitId;

		}

	}

}
