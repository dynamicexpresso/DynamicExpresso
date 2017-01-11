using System;
#if !NET_COREAPP
using System.Runtime.Serialization;
using System.Security.Permissions;
#endif

namespace DynamicExpresso
{
#if !NET_COREAPP
	[Serializable]
#endif
    public class ReflectionNotAllowedException : ParseException
    {
        public ReflectionNotAllowedException()
            : base("Reflection expression not allowed. To enable reflection use Interpreter.EnableReflection().", 0)
        {
        }

#if !NET_COREAPP
		protected ReflectionNotAllowedException(
			SerializationInfo info,
			StreamingContext context)
			: base(info, context) 
		{
		}

		[SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
		}
#endif
    }
}
