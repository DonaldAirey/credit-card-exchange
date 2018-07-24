namespace FluidTrade.Thirdparty
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Windows;
	using ActiproSoftware.Windows.Controls.Editors;
	using ActiproSoftware.Windows.Controls.Editors.Primitives;
	using System.Windows.Data;

	/// <summary>
	/// An text box for editing amounts of a unit.
	/// </summary>
	public class DecimalTextBox : TypeSpecificEditBoxBase<Decimal>
	{

		/// <summary>
		/// Indicates the Maximum dependency property.
		/// </summary>
		public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register("Maximum", typeof(Decimal), typeof(DecimalTextBox), new PropertyMetadata(Decimal.MaxValue));
		/// <summary>
		/// Indicates the Minimum dependency property.
		/// </summary>
		public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register("Minimum", typeof(Decimal), typeof(DecimalTextBox), new PropertyMetadata(Decimal.MinValue));
		/// <summary>
		/// Indicates the IsReadOnly dependency property.
		/// </summary>
		public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(Boolean), typeof(DecimalTextBox), new PropertyMetadata(false));

		/// <summary>
		/// Gets or sets whether the text box is read only.
		/// </summary>
		public Boolean IsReadOnly
		{
			get { return (Boolean)this.GetValue(DecimalTextBox.IsReadOnlyProperty); }
			set { this.SetValue(DecimalTextBox.IsReadOnlyProperty, value); }
		}

		/// <summary>
		/// Gets or sets the maximum value of the text box.
		/// </summary>
		public Decimal Maximum
		{
			get { return (Decimal)this.GetValue(DecimalTextBox.MaximumProperty); }
			set { this.SetValue(DecimalTextBox.MaximumProperty, value); }
		}

		/// <summary>
		/// Gets or sets the minimum value of the text box.
		/// </summary>
		public Decimal Minimum
		{
			get { return (Decimal)this.GetValue(DecimalTextBox.MinimumProperty); }
			set { this.SetValue(DecimalTextBox.MinimumProperty, value); }
		}

		static DecimalTextBox()
		{

			DefaultStyleKeyProperty.OverrideMetadata(typeof(DecimalTextBox), new FrameworkPropertyMetadata(typeof(DecimalTextBox)));
		}

		/// <summary>
		/// Generates a list of objects to be used in the DefaultItems collection.
		/// </summary>
		/// <param name="defaultItems">The collection that should be updated.</param>
		protected override void GenerateDefaultItems(SlottedItemCollection defaultItems)
		{

			base.GenerateDefaultItems(defaultItems);
			DecimalPartGroup decimalGroup = new DecimalPartGroup();

			decimalGroup.SetBinding(DecimalPartGroup.IsReadOnlyProperty, new Binding("IsReadOnly") { Source = this });
			decimalGroup.SetBinding(DecimalPartGroup.MaximumProperty, new Binding("Maximum") { Source = this });
			decimalGroup.SetBinding(DecimalPartGroup.MinimumProperty, new Binding("Minimum") { Source = this });
			SyncBinding.CreateBinding(this, DecimalTextBox.ValueProperty, decimalGroup, DecimalPartGroup.ValueProperty);

			defaultItems.Add(decimalGroup);

		}

	}

}
