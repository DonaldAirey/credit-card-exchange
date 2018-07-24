namespace FluidTrade.Actipro
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
	/// Part group for enum parts.
	/// </summary>
	/// <typeparam name="E">The enum type this group is for.</typeparam>
	public class EnumPartGroupBase<E> : TypeSpecificPartGroupBase<E>
	{

		/// <summary>
		/// Indicates the IsReadOnly dependency property.
		/// </summary>
		public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(Boolean), typeof(EnumPartGroupBase<E>), new PropertyMetadata(false));

		/// <summary>
		/// Gets or sets whether the text box is read only.
		/// </summary>
		public Boolean IsReadOnly
		{
			get { return (Boolean)this.GetValue(DecimalPartGroup.IsReadOnlyProperty); }
			set { this.SetValue(DecimalPartGroup.IsReadOnlyProperty, value); }
		}

		/// <summary>
		/// Converts the specified string to an instance of Decimal.
		/// </summary>
		/// <param name="text">The text.</param>
		/// <returns>An instance of Decimal.</returns>
		protected override E ConvertFromString(string text)
		{

			return (E)Enum.Parse(typeof(E), text);

		}

		/// <summary>
		/// Converts the specified value to a string representation.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>A string representation of the specified value.</returns>
		protected override string ConvertToString(E value)
		{

			return value.ToString();

		}

		/// <summary>
		/// Generate default parts.
		/// </summary>
		/// <param name="defaultItems"></param>
		protected override void GenerateDefaultItems(ActiproSoftware.Windows.Controls.Editors.Primitives.SlottedItemCollection defaultItems)
		{

			base.GenerateDefaultItems(defaultItems);

			EnumPartBase<E> part = new EnumPartBase<E>();

			part.SetBinding(EnumPartBase<E>.IsEditableProperty, new Binding("IsReadOnly") { Source = this, Converter = new NotConverter() });
			SyncBinding.CreateBinding(this, EnumPartBase<E>.ValueProperty, part, EnumPartBase<E>.ValueProperty);
			SyncBinding.CreateBinding(this, EnumPartBase<E>.InitialValueProperty, part, EnumPartBase<E>.InitialValueProperty);
			part.PartValueCommitTriggers = PartValueCommitTriggers.All;
			part.IsFocusMovedOnTerminalMatches = true;
			part.IsTabStop = true;
			defaultItems.Add(part);

		}

	}

}
