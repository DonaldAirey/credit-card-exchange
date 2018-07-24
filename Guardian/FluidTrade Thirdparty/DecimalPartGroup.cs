namespace FluidTrade.Thirdparty
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using ActiproSoftware.Windows.Controls.Editors;
	using ActiproSoftware.Windows.Controls.Editors.Parts;
	using ActiproSoftware.Windows.Controls.Editors.Parts.Primitives;
	using ActiproSoftware.Windows.Controls.Editors.Primitives;
	using System.Windows.Data;
	using System.Windows;

	/// <summary>
	/// Part group for decimal parts.
	/// </summary>
	public class DecimalPartGroup : TypeSpecificPartGroupBase<Decimal>
	{

		/// <summary>
		/// Indicates the Maximum dependency property.
		/// </summary>
		public static readonly DependencyProperty MaximumProperty =
			DependencyProperty.Register("Maximum", typeof(Decimal), typeof(DecimalPartGroup), new PropertyMetadata(Decimal.MaxValue, OnMaximumChanged));
		/// <summary>
		/// Indicates the Minimum dependency property.
		/// </summary>
		public static readonly DependencyProperty MinimumProperty =
			DependencyProperty.Register("Minimum", typeof(Decimal), typeof(DecimalPartGroup), new PropertyMetadata(Decimal.MinValue, OnMinimumChanged));
		/// <summary>
		/// Indicates the IsReadOnly dependency property.
		/// </summary>
		public static readonly DependencyProperty IsReadOnlyProperty =
			DependencyProperty.Register("IsReadOnly", typeof(Boolean), typeof(DecimalPartGroup), new PropertyMetadata(false));
		/// <summary>
		/// Indicates the Step dependency property.
		/// </summary>
		public static readonly DependencyProperty StepProperty = DependencyProperty.Register("Step", typeof(Decimal), typeof(DecimalPartGroup), new PropertyMetadata(1m));

		static DecimalPartGroup()
		{

			DefaultStyleKeyProperty.OverrideMetadata(typeof(DecimalPartGroup), new FrameworkPropertyMetadata(typeof(DecimalPartGroup)));

		}

		/// <summary>
		/// Create a new decimal part group.
		/// </summary>
		public DecimalPartGroup()
		{

			this.IsTabStop = true;

		}

		/// <summary>
		/// Gets or sets whether the text box is read only.
		/// </summary>
		public Boolean IsReadOnly
		{
			get { return (Boolean)this.GetValue(DecimalPartGroup.IsReadOnlyProperty); }
			set { this.SetValue(DecimalPartGroup.IsReadOnlyProperty, value); }
		}

		/// <summary>
		/// Gets or sets the maximum value of the text box.
		/// </summary>
		public Decimal Maximum
		{
			get { return (Decimal)this.GetValue(DecimalPartGroup.MaximumProperty); }
			set { this.SetValue(DecimalPartGroup.MaximumProperty, value); }
		}

		/// <summary>
		/// Gets or sets the minimum value of the text box.
		/// </summary>
		public Decimal Minimum
		{
			get { return (Decimal)this.GetValue(DecimalPartGroup.MinimumProperty); }
			set { this.SetValue(DecimalPartGroup.MinimumProperty, value); }
		}

		/// <summary>
		/// Gets or sets the spinner step value of the text box.
		/// </summary>
		public Decimal Step
		{
			get { return (Decimal)this.GetValue(DecimalPartGroup.StepProperty); }
			set { this.SetValue(DecimalPartGroup.StepProperty, value); }
		}

		/// <summary>
		/// Converts the specified string to an instance of Decimal.
		/// </summary>
		/// <param name="text">The text.</param>
		/// <returns>An instance of Decimal.</returns>
		protected override Decimal ConvertFromString(string text)
		{

			return Decimal.Parse(text);

		}

		/// <summary>
		/// Converts the specified value to a string representation.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>A string representation of the specified value.</returns>
		protected override string ConvertToString(Decimal value)
		{

			return value.ToString();

		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		protected override decimal CoerceValue(decimal value)
		{

			if (value > this.Maximum)
				value = this.Maximum;
			else if (value < this.Minimum)
				value = this.Minimum;

			return value;

		}
		/// <summary>
		/// Handle the maximum value changing.
		/// </summary>
		/// <param name="sender">The decimal part whose maximum value changed.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private static void OnMaximumChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			DecimalPartGroup part = sender as DecimalPartGroup;

			if (part.Value > part.Maximum)
				part.Value = part.Maximum;

		}

		/// <summary>
		/// Handle the minimum value changing.
		/// </summary>
		/// <param name="sender">The decimal part whose minimum value changed.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private static void OnMinimumChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			DecimalPartGroup part = sender as DecimalPartGroup;

			if (part.Value < part.Minimum)
				part.Value = part.Minimum;

		}

		/// <summary>
		/// Generate default parts.
		/// </summary>
		/// <param name="defaultItems"></param>
		protected override void GenerateDefaultItems(ActiproSoftware.Windows.Controls.Editors.Primitives.SlottedItemCollection defaultItems)
		{

			base.GenerateDefaultItems(defaultItems);

			DecimalPart part = new DecimalPart();

			part.SetBinding(DecimalPart.IsEditableProperty, new Binding("IsReadOnly") { Source = this, Converter = new NotConverter() });
			part.SetBinding(DecimalPart.MaximumProperty, new Binding("Maximum") { Source = this });
			part.SetBinding(DecimalPart.MinimumProperty, new Binding("Minimum") { Source = this });
			part.SetBinding(DecimalPart.StepProperty, new Binding("Step") { Source = this, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
			part.SetBinding(DecimalPart.ValueProperty, new Binding("Value") { Source = this, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
			//SyncBinding.CreateBinding(this, DecimalPartGroup.ValueProperty, part, DecimalPart.ValueProperty);
			SyncBinding.CreateBinding(this, DecimalPartGroup.InitialValueProperty, part, DecimalPart.InitialValueProperty);
			SyncBinding.CreateBinding(
					this,
					DecimalPartGroup.PartValueCommitTriggersProperty,
					part,
					DecimalPart.PartValueCommitTriggersProperty,
					UpdateSourceTrigger.Default);
			part.IsFocusMovedOnTerminalMatches = true;
			part.IsTabStop = true;
			defaultItems.Add(part);

		}

	}

}
