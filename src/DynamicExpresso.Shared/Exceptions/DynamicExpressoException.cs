using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace DynamicExpresso
{
#if !WINDOWS_UWP
    [Serializable]
#endif
	public class DynamicExpressoException : Exception
	{
		public DynamicExpressoException() { }
		public DynamicExpressoException(string message) : base(message) { }
		public DynamicExpressoException(string message, Exception inner) : base(message, inner) { }
#if !WINDOWS_UWP
        protected DynamicExpressoException(
		System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
#endif
    }
}
