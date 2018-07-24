namespace FluidTrade.Core
{

	using System;
	using System.Collections.Generic;
	using System.IO;

	/// <summary>
	/// Creates a merged Word document from a template and a dictionary of merge fields.
	/// </summary>
	public interface IMailMerge
	{
		MemoryStream CreateDocument(Byte[] sourceDocument, Dictionary<String, Object> dictionary);
	}

}
