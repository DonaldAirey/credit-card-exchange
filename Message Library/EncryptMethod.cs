namespace FluidTrade.Core
{

	using System;

	/// <summary>
	/// Summary description for FixEncryptMethod.
	/// </summary>
	[Serializable()]
	public enum EncryptMethod
	{
		None = 0,
		PKCS = 1,
		DES = 2,
		PKCSDES = 3,
		PGPDES = 4,
		PGPDESMD5 = 5,
		PEMDESMD5 = 6
	}

}
