namespace FluidTrade.Core
{

	using System;

	/// <summary>
	/// A Tag and Value pair.
	/// </summary>
	[Serializable()]
	public class Field
	{

		public Tag Tag;
		public object Value;

		/// <summary>
		/// Creates a Tag and Value pair.
		/// </summary>
		/// <param name="tag">Describes the function of the value in this field.</param>
		/// <param name="value">A generic value that has no meaning without the tag.</param>
		public Field(Tag tag, object value)
		{

			// Initialize the object
			this.Tag = tag;
			this.Value = value;

		}

	}

}
