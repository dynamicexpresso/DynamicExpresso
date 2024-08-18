using System;
using System.Runtime.Serialization;

namespace DynamicExpresso.Exceptions
{
	[Serializable]
	public class ReflectionNotAllowedException : ParseException
	{
		public ReflectionNotAllowedException()
			: base("Reflection expression not allowed. To enable reflection use Interpreter.EnableReflection().", 0)
		{
		}

		protected ReflectionNotAllowedException(
			SerializationInfo info,
			StreamingContext context)
			: base(info, context)
		{
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
		}
	}
}
