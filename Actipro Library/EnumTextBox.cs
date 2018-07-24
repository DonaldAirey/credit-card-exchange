namespace FluidTrade.Actipro
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using ActiproSoftware.Windows.Controls.Editors;
	using ActiproSoftware.Windows.Controls.Editors.Parts;
	using ActiproSoftware.Windows.Controls.Editors.Parts.Primitives;
	using System.Windows;
	using System.Windows.Data;
	using ActiproSoftware.Windows.Controls.Editors.Primitives;
	using System.Diagnostics;

	/// <summary>
	/// An text box for editing amounts of a unit.
	/// </summary>
	public class EnumTextBox : MaskedTextBox
	{

		private class EnumLengthConverter : IMultiValueConverter
		{

			private EnumTextBox box;

			/// <summary>
			/// Create a converter and bind it to a TermLength.
			/// </summary>
			/// <param name="box">The TermLength object this converter is associated with.</param>
			public EnumLengthConverter(EnumTextBox box)
			{

				this.box = box;

			}

			/// <summary>
			/// Convert between decimal&amp;TimeUnit and a string.
			/// </summary>
			/// <param name="values">The new value just set.</param>
			/// <param name="targetType">The type of object we should return.</param>
			/// <param name="parameter">Paremeter to the converter (ignored).</param>
			/// <param name="culture">The culture to do the conversion in.</param>
			/// <returns>The converted value.</returns>
			public object Convert(Object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
			{

				try
				{

					Decimal length = (Decimal)values[0];
					object unit = values[1];

					if (unit == null)
						return String.Format("{0:0.###} ", length);
					else
						return String.Format("{0:0.###} {1}", length, unit);

				}
				catch
				{


				}

				return null;

			}

			/// <summary>
			/// Convert between decimal&amp;TimeUnit and a string.
			/// </summary>
			/// <param name="value">The new value just set.</param>
			/// <param name="targetTypes">The type of object we should return.</param>
			/// <param name="parameter">Paremeter to the converter (ignored).</param>
			/// <param name="culture">The culture to do the conversion in.</param>
			/// <returns>The converted value.</returns>
			public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
			{

				Object[] results = new Object[] { (Object)box.Length, box.Unit };

				try
				{

					Decimal parsedDecimal = box.Length;
					String[] valueAndUnits = (value as string).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

					if (valueAndUnits.Length > 0)
						if (valueAndUnits.Length > 1)
						{

							Boolean parsed = false;
							object parsedEnum = Enum.Parse(this.box.Type, valueAndUnits[1]);

							parsed = Decimal.TryParse(valueAndUnits[0], out parsedDecimal);

							parsedDecimal = Math.Abs(parsedDecimal);

							results = new Object[] { parsedDecimal, parsedEnum == null ? box.Unit : parsedEnum };

						}
						else if (Decimal.TryParse(valueAndUnits[0], out parsedDecimal))
						{

							results = new Object[] { parsedDecimal, box.Unit };

						}
						else
						{


							object parsedEnum = Enum.Parse(this.box.Type, valueAndUnits[1]);

							results = new Object[] { box.Length, parsedEnum == null ? box.Unit : parsedEnum };

						}


				}
				catch
				{


				}

				return results;

			}

		}

		/// <summary>
		/// Indicates the Length dependency property.
		/// </summary>
		public static readonly DependencyProperty LengthProperty = DependencyProperty.Register("Length", typeof(Decimal), typeof(EnumTextBox), new PropertyMetadata(EnumTextBox.OnLengthChanged));
		/// <summary>
		/// Indicates the IsDisplayOnly dependency property.
		/// </summary>
		public static readonly DependencyProperty IsDisplayOnlyProperty =
			DependencyProperty.Register("IsDisplayOnly", typeof(Boolean), typeof(EnumTextBox), new PropertyMetadata(false));
		/// <summary>
		/// Indicates the Type dependency property.
		/// </summary>
		public static readonly DependencyProperty TypeProperty = DependencyProperty.Register("Type", typeof(Type), typeof(EnumTextBox), new PropertyMetadata(EnumTextBox.OnTypeChanged));
		/// <summary>
		/// Indicates the Unit dependency property.
		/// </summary>
		public static readonly DependencyProperty UnitProperty = DependencyProperty.Register("Unit", typeof(Enum), typeof(EnumTextBox), new PropertyMetadata(EnumTextBox.OnUnitChanged));

		/// <summary>
		/// Raised when the Length property changes.
		/// </summary>
		public event DependencyPropertyChangedEventHandler LengthChanged;
		/// <summary>
		/// Raised when the Unit property changes.
		/// </summary>
		public event DependencyPropertyChangedEventHandler UnitChanged;
		
		static EnumTextBox()
		{

			DefaultStyleKeyProperty.OverrideMetadata(typeof(EnumTextBox), new FrameworkPropertyMetadata(typeof(EnumTextBox)));
		}

		/// <summary>
		/// Create a new enum text box.
		/// </summary>
		public EnumTextBox()
		{

			MultiBinding textBinding = new MultiBinding() { Converter = new EnumLengthConverter(this) };

			textBinding.Bindings.Add(new Binding("Length") { Source = this });
			textBinding.Bindings.Add(new Binding("Unit") { Source = this });

			textBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
			textBinding.Mode = BindingMode.TwoWay;

			this.Mask = @"((\d{1,3}(,\d\d\d)*)|\d+)(\.\d+)?|\.\d+ +";
			this.SetBinding(EnumTextBox.TextProperty, textBinding);
			this.PromptIndicatorVisibility = IndicatorVisibility.Always;

		}

		/// <summary>
		/// Create a new enum text box.
		/// </summary>
		public EnumTextBox(Type type)
		{

			MultiBinding textBinding = new MultiBinding() { Converter = new EnumLengthConverter(this) };

			textBinding.Bindings.Add(new Binding("Length") { Source = this });
			textBinding.Bindings.Add(new Binding("Unit") { Source = this });

			textBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
			textBinding.Mode = BindingMode.TwoWay;

			this.Mask = @"((\d{1,3}(,\d\d\d)*)|\d+)(\.\d+)?|\.\d+ +";
			this.SetBinding(EnumTextBox.TextProperty, textBinding);
			this.PromptIndicatorVisibility = IndicatorVisibility.Always;

			this.Type = type;

		}

		/// <summary>
		/// Gets or sets whether the text box is for display only.
		/// </summary>
		public Boolean IsDisplayOnly
		{
			get { return (Boolean)this.GetValue(EnumTextBox.IsDisplayOnlyProperty); }
			set { this.SetValue(EnumTextBox.IsDisplayOnlyProperty, value); }
		}

		/// <summary>
		/// The integer portion of the text box.
		/// </summary>
		public Decimal Length
		{
			get { return (Decimal)this.GetValue(EnumTextBox.LengthProperty); }
			set { this.SetValue(EnumTextBox.LengthProperty, value); }
		}

		/// <summary>
		/// The type of the enum used in the text box.
		/// </summary>
		public Type Type
		{
			get { return this.GetValue(EnumTextBox.TypeProperty) as Type; }
			set { this.SetValue(EnumTextBox.TypeProperty, value); }
		}

		/// <summary>
		/// The enum portion of the text box.
		/// </summary>
		public Enum Unit
		{
			get { return (Enum)this.GetValue(EnumTextBox.UnitProperty); }
			set { this.SetValue(EnumTextBox.UnitProperty, value); }
		}

		/// <summary>
		/// Get a regex mask to fit the enum.
		/// </summary>
		/// <returns>An appropriate mask for the enum.</returns>
		private String GetEnumValues()
		{

			String[] names = Enum.GetNames(this.Type);
			StringBuilder nameRegex = new StringBuilder();

			nameRegex.Append('(');

			for (Int32 index = 0; index < names.Length; ++index)
			{

				nameRegex.Append(names[index]);

				if (index < names.Length - 1)
					nameRegex.Append('|');

			}

			nameRegex.Append(')');

			return nameRegex.ToString();

		}

		/// <summary>
		/// Update the text when focus is lost.
		/// </summary>
		/// <param name="e">The event arguments.</param>
		protected override void  OnLostKeyboardFocus(System.Windows.Input.KeyboardFocusChangedEventArgs e)
		{

			MultiBindingExpression bindingExpression = BindingOperations.GetMultiBindingExpression(this, EnumTextBox.TextProperty);

			base.OnLostKeyboardFocus(e);

			// For some reason, the selection isn't getting cleared when we lose keyboard focus, so we force it here.
			this.SelectionLength = 0;

			// Make sure the display looks reflects the underlying values.
			bindingExpression.UpdateTarget();

		}

		/// <summary>
		/// Handle the Length property changing.
		/// </summary>
		/// <param name="sender">The enum text box whose length changed.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private static void OnLengthChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			EnumTextBox box = sender as EnumTextBox;
			//MultiBindingExpression bindingExpression = BindingOperations.GetMultiBindingExpression(box, EnumTextBox.TextProperty);

			//bindingExpression.UpdateTarget();

			if (box.LengthChanged != null)
				box.LengthChanged(sender, eventArgs);

		}
		
		/// <summary>
		/// Handle the Type property changing.
		/// </summary>
		/// <param name="sender">The enum text box whose unit changed.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private static void OnTypeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			EnumTextBox box = sender as EnumTextBox;

			if (box.Type != null)
			{

				box.Mask = @"(((\d{1,3}(,\d\d\d)*)|\d+)(\.\d+)?|\.\d+) +" + box.GetEnumValues();
				box.Unit = (Enum)Enum.GetValues(box.Type).GetValue(0);

			}

		}

		/// <summary>
		/// Handle the Unit property changing.
		/// </summary>
		/// <param name="sender">The enum text box whose unit changed.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private static void OnUnitChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			EnumTextBox box = sender as EnumTextBox;
			//MultiBindingExpression bindingExpression = BindingOperations.GetMultiBindingExpression(box, EnumTextBox.TextProperty);

			//bindingExpression.UpdateTarget();

			if (box.UnitChanged != null)
				box.UnitChanged(sender, eventArgs);

		}

	}

#if false
	/// <summary>
	/// An text box for editing amounts of a unit.
	/// </summary>
	/// <typeparam name="E"></typeparam>
	public class EnumTextBox<E> : PartEditBox
	{

		/// <summary>
		/// Indicates the Length dependency property.
		/// </summary>
		public static readonly DependencyProperty LengthProperty = DependencyProperty.Register("Length", typeof(Decimal), typeof(EnumTextBox<E>), new PropertyMetadata(EnumTextBox<E>.OnLengthChanged));
		/// <summary>
		/// Indicates the IsReadOnly dependency property.
		/// </summary>
		public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(Boolean), typeof(EnumTextBox<E>), new PropertyMetadata(false));
		/// <summary>
		/// Indicates the Unit dependency property.
		/// </summary>
		public static readonly DependencyProperty UnitProperty = DependencyProperty.Register("Unit", typeof(E), typeof(EnumTextBox<E>), new PropertyMetadata(EnumTextBox<E>.OnUnitChanged));

		/// <summary>
		/// Raised when the Length property changes.
		/// </summary>
		public event DependencyPropertyChangedEventHandler LengthChanged;
		/// <summary>
		/// Raised when the Unit property changes.
		/// </summary>
		public event DependencyPropertyChangedEventHandler UnitChanged;

		/// <summary>
		/// Create a new enum text box.
		/// </summary>
		public EnumTextBox()
		{

			this.ArePartGroupsSelectable = true;

		}

		/// <summary>
		/// The integer portion of the text box.
		/// </summary>
		public Decimal Length
		{
			get { return (Decimal)this.GetValue(EnumTextBox<E>.LengthProperty); }
			set { this.SetValue(EnumTextBox<E>.LengthProperty, value); }
		}

		/// <summary>
		/// Gets or sets whether the text box is read only.
		/// </summary>
		public Boolean IsReadOnly
		{
			get { return (Boolean)this.GetValue(EnumTextBox<E>.IsReadOnlyProperty); }
			set { this.SetValue(EnumTextBox<E>.IsReadOnlyProperty, value); }
		}

		/// <summary>
		/// The enum portion of the text box.
		/// </summary>
		public E Unit
		{
			get { return (E)this.GetValue(EnumTextBox<E>.UnitProperty); }
			set { this.SetValue(EnumTextBox<E>.UnitProperty, value); }
		}
		
		/// <summary>
		/// Generates a list of objects to be used in the DefaultItems collection.
		/// </summary>
		/// <param name="defaultItems">The collection that should be updated.</param>
		protected override void GenerateDefaultItems(SlottedItemCollection defaultItems)
		{

			base.GenerateDefaultItems(defaultItems);
			DecimalPartGroup lengthGroup = new DecimalPartGroup();
			TextBlockPartGroup spaceGroup = new TextBlockPartGroup();
			PartGroup enumGroup = new PartGroup();
			EnumPartBase<E> enumPart = new EnumPartBase<E>();

			lengthGroup.SetBinding(DecimalPartGroup.IsReadOnlyProperty, new Binding("IsReadOnly") { Source = this });
			SyncBinding.CreateBinding(this, EnumTextBox<E>.LengthProperty, lengthGroup, DecimalPartGroup.ValueProperty);
			spaceGroup.Text = " ";
			enumPart.SetBinding(EnumPartBase<E>.IsEditableProperty, new Binding("IsReadOnly") { Source = this, Converter = new NotConverter() });
			enumPart.SetBinding(EnumPartBase<E>.ValueProperty, new Binding("Unit") { Source = this });
			enumPart.PartValueCommitTriggers = PartValueCommitTriggers.All;
			enumGroup.Items.Add(enumPart);

			defaultItems.Add(lengthGroup);
			defaultItems.Add(spaceGroup);
			defaultItems.Add(enumGroup);

		}

		/// <summary>
		/// Handle the Length property changing.
		/// </summary>
		/// <param name="sender">The enum text box whose length changed.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private static void OnLengthChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			EnumTextBox<E> box = sender as EnumTextBox<E>;

			if (box.LengthChanged != null)
				box.LengthChanged(sender, eventArgs);

		}

		/// <summary>
		/// Handle the Unit property changing.
		/// </summary>
		/// <param name="sender">The enum text box whose unit changed.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private static void OnUnitChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			EnumTextBox<E> box = sender as EnumTextBox<E>;

			if (box.UnitChanged != null)
				box.UnitChanged(sender, eventArgs);

		}

	}
#endif
}
