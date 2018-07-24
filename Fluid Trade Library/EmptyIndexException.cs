namespace FluidTrade.Core
{

	using System;
    using System.Runtime.Serialization;

    [Serializable]
	public class EmptyIndexException : Exception
	{

		public EmptyIndexException(string format, params object[] args) : base(string.Format(format, args)) { }

		protected EmptyIndexException(SerializationInfo info, StreamingContext context) : base(info, context) { }

	}
}
