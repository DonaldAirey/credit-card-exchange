namespace FluidTrade.Actipro
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Windows.Data;
	using ActiproSoftware.Windows.Controls.Editors.Parts;
	using ActiproSoftware.Windows.Controls.Editors.Parts.Primitives;
	using ActiproSoftware.Windows.Controls.Editors.Primitives;
	using System.Windows;

	/// <summary>
	/// A text box for editing percentages.
	/// </summary>
	public class PercentageTextBox : TypeSpecificEditBoxBase<Decimal>
	{

		/// <summary>
		/// The Format dependency property.
		/// </summary>
		public static readonly DependencyProperty FormatProperty = DependencyProperty.Register(
			"Format",
			typeof(String),
			typeof(PercentageTextBox),
			new PropertyMetadata("0"));

		/// <summary>
		/// Indicates the IsDisplayOnly dependency property.
		/// </summary>
		public static readonly DependencyProperty IsDisplayOnlyProperty = DependencyProperty.Register(
			"IsDisplayOnly",
			typeof(Boolean),
			typeof(PercentageTextBox),
			new PropertyMetadata(false));

		/// <summary>
		/// Indicates the IsReadOnly dependency property.
		/// </summary>
		public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
			"IsReadOnly",
			typeof(Boolean),
			typeof(PercentageTextBox),
			new PropertyMetadata(false));

		/// <summary>
		/// Indicates the Maximum dependency property.
		/// </summary>
		public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
			"Maximum",
			typeof(Decimal),
			typeof(PercentageTextBox),
			new PropertyMetadata(1.00m));

		/// <summary>
		/// Indicates the Minimum dependency property.
		/// </summary>
		public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
			"Minimum",
			typeof(Decimal),
			typeof(PercentageTextBox),
			new PropertyMetadata(0.00m));

		/// <summary>
		/// Indicates the Step dependency property.
		/// </summary>
		public static readonly DependencyProperty StepProperty = DependencyProperty.Register(
			"Step",
			typeof(Decimal),
			typeof(PercentageTextBox),
			new PropertyMetadata(0.01m));

		/// <summary>
		/// Create the static resources required for this class.
		/// </summary>
		static PercentageTextBox()
		{

			// This control will have a distinct style from the base class.
			PercentageTextBox.DefaultStyleKeyProperty.OverrideMetadata(typeof(PercentageTextBox), new FrameworkPropertyMetadata(typeof(PercentageTextBox)));

		}

		/// <summary>
		/// Gets or sets whether the text box is for display only.
		/// </summary>
		public Boolean IsDisplayOnly
		{
			get { return (Boolean)this.GetValue(PercentageTextBox.IsDisplayOnlyProperty); }
			set { this.SetValue(PercentageTextBox.IsDisplayOnlyProperty, value); }
		}

		/// <summary>
		/// Gets or sets whether the text box is read only.
		/// </summary>
		public Boolean IsReadOnly
		{
			get { return (Boolean)this.GetValue(PercentageTextBox.IsReadOnlyProperty); }
			set { this.SetValue(PercentageTextBox.IsReadOnlyProperty, value); }
		}

		/// <summary>
		/// Gets or sets the maximum value of the text box.
		/// </summary>
		public Decimal Maximum
		{
			get { return (Decimal)this.GetValue(PercentageTextBox.MaximumProperty); }
			set { this.SetValue(PercentageTextBox.MaximumProperty, value); }
		}

		/// <summary>
		/// Gets or sets the minimum value of the text box.
		/// </summary>
		public Decimal Minimum
		{
			get { return (Decimal)this.GetValue(PercentageTextBox.MinimumProperty); }
			set { this.SetValue(PercentageTextBox.MinimumProperty, value); }
		}

		/// <summary>
		/// Gets or sets the spinner step value of the text box.
		/// </summary>
		public Decimal Step
		{
			get { return (Decimal)this.GetValue(PercentageTextBox.StepProperty); }
			set { this.SetValue(PercentageTextBox.StepProperty, value); }
		}

		/// <summary>
		/// Generates a list of objects to be used in the DefaultItems collection.
		/// </summary>
		/// <param name="defaultItems">The collection that should be updated.</param>
		protected override void GenerateDefaultItems(SlottedItemCollection defaultItems)
		{

			// Allow the base class to provide most of the default items.
			base.GenerateDefaultItems(defaultItems);

			// This control will create a special group of parts for a decimal implementation of a percent value.	
			DecimalPartGroup decimalPartGroup = new DecimalPartGroup();
			decimalPartGroup.SetBinding(DecimalPartGroup.StringFormatProperty, new Binding("Format") { Source = this });
			decimalPartGroup.SetBinding(DecimalPartGroup.IsReadOnlyProperty, new Binding("IsReadOnly") { Source = this });
			decimalPartGroup.SetBinding(
				DecimalPartGroup.MaximumProperty,
				new Binding("Maximum") { Source = this, Converter = new PercentageConverter(), UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
			decimalPartGroup.SetBinding(
				DecimalPartGroup.MinimumProperty,
				new Binding("Minimum") { Source = this, Converter = new PercentageConverter(), UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
			decimalPartGroup.SetBinding(
				DecimalPartGroup.StepProperty,
				new Binding("Step") { Source = this, Converter = new PercentageConverter(), UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
			SyncBinding.CreateBinding(
				this,
				DecimalTextBox.ValueProperty,
				decimalPartGroup,
				DecimalPartGroup.ValueProperty,
				UpdateSourceTrigger.PropertyChanged, new PercentageConverter());
			defaultItems.Add(decimalPartGroup);
			defaultItems.Add(new TextBlockPartGroup() { Text = "%" });

		}

	}

}
