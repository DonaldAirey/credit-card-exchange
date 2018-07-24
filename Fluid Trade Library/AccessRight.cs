namespace FluidTrade.Core
{

	using System;

	/// <summary>
	/// The rights that can be granted to an object.
	/// </summary>
	[Flags]
	public enum AccessRight
	{
		/// <summary>
		/// No rights.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// The right to read the contents of a folder.
		/// </summary>
		Browse = 0x0001,
		/// <summary>
		/// The right to execute code.
		/// </summary>
		Execute = 0x0002,
		/// <summary>
		/// Complete control over an object.
		/// </summary>
		FullControl = 0x000F,
		/// <summary>
		/// The right to read an object.w
		/// </summary>
		Read = 0x0004,
		/// <summary>
		/// The right to write an object.
		/// </summary>
		Write = 0x0008,

		// These are here to accomodate the datamodel's foreign keys.
		ExecuteBrowse = 0x0003,
		ReadBrowse = 0x0005,
		ReadExecute = 0x0006,
		ReadExecuteBrowse = 0x0007,
		WriteBrowse = 0x0009,
		WriteExecute = 0x000A,
		WriteExecuteBrowse = 0x000B,
		ReadWrite = 0x000C,
		ReadWriteBrowse = 0x000D,
		ReadWriteExecute = 0x000E,
	};

}
