namespace FluidTrade.Thirdparty
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using ActiproSoftware.Windows.Controls.Editors.Parts.Primitives;
	using System.Windows;
	using ActiproSoftware.Windows.Controls.Editors;

	/// <summary>
	/// A type specific part for enum types.
	/// </summary>
	/// <typeparam name="E">The enum the part is for.</typeparam>
	public class EnumPartBase<E> : TypeSpecificPartBase<E>
	{

		static EnumPartBase()
		{

			if (!typeof(E).IsEnum)
				throw new ArgumentOutOfRangeException("E", "EnumPart can only be used on Enum types");

		}

		/// <summary>
		/// Create a new part for enums.
		/// </summary>
		public EnumPartBase()
		{

			this.Mask = this.GetEnumValues();
			this.IsNullAllowed = false;
			this.PartValueCommitTriggers = PartValueCommitTriggers.All;

		}

		/// <summary>
		/// Get the string representation of a value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The string represenation of the value.</returns>
		protected override string GetString(E value)
		{

			return value.ToString();

		}

		/// <summary>
		/// Get a regex mask to fit the enum.
		/// </summary>
		/// <returns>An appropriate mask for the enum.</returns>
		private String GetEnumValues()
		{

			String[] names = Enum.GetNames(typeof(E));
			StringBuilder nameRegex = new StringBuilder();

			for (Int32 index = 0; index < names.Length; ++index)
			{

				nameRegex.Append(names[index]);

				if (index < names.Length - 1)
					nameRegex.Append('|');

			}

			return nameRegex.ToString();

		}

		/// <summary>
		/// Attempt to get an enum value by its name.
		/// </summary>
		/// <param name="stringValue">The string name.</param>
		/// <param name="value">The enum value.</param>
		/// <returns>True if the value was successfully retrieved.</returns>
		protected override bool TryGetEffectiveValue(string stringValue, out E value)
		{

			Boolean got = true;

			value = (E)Enum.GetValues(typeof(E)).GetValue(0);

			try
			{

				value = (E)Enum.Parse(typeof(E), stringValue, true);

			}
			catch
			{

				got = false;

			}

			return got;

		}

	}

}
