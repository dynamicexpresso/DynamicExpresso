#if !NETSTANDARD1_6
using System;
using System.Security.Permissions;
using System.Runtime.Serialization;
#endif

namespace DynamicExpresso.Exceptions
{
#if !NETSTANDARD1_6
	[Serializable]
#endif
	public class ReflectionNotAllowedException : ParseException
	{
		public ReflectionNotAllowedException()
			: base("Reflection expression not allowed. To enable reflection use Interpreter.EnableReflection().", 0) 
		{
		}

		#if !NETSTANDARD1_6
		protected ReflectionNotAllowedException(
			SerializationInfo info,
			StreamingContext context)
			: base(info, context) 
		{
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
		}
		#endif
	}
}
