namespace FluidTrade.Core.Windows.Controls
{

    using System.IO;

    public static class StringHelper
    {

		public static Stream ToStream(this string sourceText)
		{
        
			MemoryStream memoryStream = new MemoryStream();
			StreamWriter streamWriter = new StreamWriter(memoryStream);
			streamWriter.WriteLine(sourceText);
			streamWriter.Flush();
			memoryStream.Position = 0L;
			return memoryStream;

		}


	}
}
