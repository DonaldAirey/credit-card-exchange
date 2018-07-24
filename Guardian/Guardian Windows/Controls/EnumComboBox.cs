namespace FluidTrade.Guardian.Windows.Controls
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Windows.Controls;
	using System.Windows;
	using System.Windows.Data;

	/// <summary>
	/// A drop-down for enums.
	/// </summary>
	public class EnumComboBox : ComboBox
	{

		/// <summary>
		/// Indicates the EnumType dependency property.
		/// </summary>
		public static readonly DependencyProperty EnumTypeProperty = DependencyProperty.Register("EnumType", typeof(Type), typeof(EnumComboBox));

		/// <summary>
		/// 
		/// </summary>
		public EnumComboBox()
		{

			this.Loaded += OnLoaded;

		}

		/// <summary>
		/// The enum this combobox is for.
		/// </summary>
		public Type EnumType
		{
			get { return this.GetValue(EnumComboBox.EnumTypeProperty) as Type; }
			set { this.SetValue(EnumComboBox.EnumTypeProperty, value); }
		}

		/// <summary>
		/// Populate the combobox once it has been loaded.
		/// </summary>
		/// <param name="sender">The EnumComboBox whose type changed.</param>
		/// <param name="e">The event arguments.</param>
		void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
		{

			ObjectDataProvider values = new ObjectDataProvider() { MethodName = "GetValues", ObjectType = typeof(Enum) };
			CollectionViewSource sortedValues = new CollectionViewSource() { Source = values };

			values.MethodParameters.Add(this.EnumType);

			this.DataContext = sortedValues;
			this.SetBinding(EnumComboBox.ItemsSourceProperty, new Binding());

		}

	}

}
