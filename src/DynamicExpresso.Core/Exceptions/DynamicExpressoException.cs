using System;

namespace DynamicExpresso.Exceptions
{
#if !NETSTANDARD1_6
	[Serializable]
#endif
	public class DynamicExpressoException : Exception
	{
		public DynamicExpressoException() { }
		public DynamicExpressoException(string message) : base(message) { }
		public DynamicExpressoException(string message, Exception inner) : base(message, inner) { }

#if !NETSTANDARD1_6
		protected DynamicExpressoException(
		System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
#endif
	}
}
