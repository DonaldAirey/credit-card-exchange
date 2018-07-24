namespace FluidTrade.Guardian.Windows.Controls
{

	using System;
	using System.Collections.Generic;
	using System.Windows;
	using System.Windows.Data;
	using FluidTrade.Guardian.Windows;

	/// <summary>
	/// A ComboBox with multi-select specifically for payment methods.
	/// </summary>
	public class PaymentMethodComboBox : MultiComboBox
	{

		/// <summary>
		/// The SelectedPaymentMethods dependency property.
		/// </summary>
		public static readonly DependencyProperty SelectedPaymentMethodsProperty;

		/// <summary>
		/// Create the static resources required for this class.
		/// </summary>
		static PaymentMethodComboBox()
		{

			// Override the default settings for the Horizontal Alignment.
			PaymentMethodComboBox.HorizontalAlignmentProperty.OverrideMetadata(
				typeof(PaymentMethodComboBox),
				new FrameworkPropertyMetadata(HorizontalAlignment.Stretch));

			// Override the default settings for the alignment of the content.
			PaymentMethodComboBox.HorizontalContentAlignmentProperty.OverrideMetadata(
				typeof(PaymentMethodComboBox),
				new FrameworkPropertyMetadata(HorizontalAlignment.Right));

		}

		/// <summary>
		/// Build a new payment method combo box.
		/// </summary>
		public PaymentMethodComboBox()
			: base()
		{

			this.Separator = ", ";
			this.SelectedValuePath = "PaymentMethodTypeId";
			this.DisplayMemberPath = "Name";
			
			this.ItemsSource = PaymentMethodTypeList.Default;

		}

	}

}
