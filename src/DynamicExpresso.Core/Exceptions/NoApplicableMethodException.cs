using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
#if !NET_COREAPP
using System.Security.Permissions;
using System.Runtime.Serialization;
#endif

namespace DynamicExpresso
{
#if !NET_COREAPP
	[Serializable]
#endif
	public class NoApplicableMethodException : ParseException
	{
        public string MethodTypeName { get; private set; }
        public string MethodName { get; private set; }

        public NoApplicableMethodException(string methodName, string methodTypeName, int position)
			: base(string.Format("No applicable method '{0}' exists in type '{1}'", methodName, methodTypeName), position) 
		{
			MethodTypeName = methodTypeName;
			MethodName = methodName;
		}
#if !NET_COREAPP
		protected NoApplicableMethodException(
			SerializationInfo info,
			StreamingContext context)
			: base(info, context) 
		{
			MethodTypeName = info.GetString("MethodTypeName");
			MethodName = info.GetString("MethodName");
		}

		[SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("MethodName", MethodName);
			info.AddValue("MethodTypeName", MethodTypeName);

			base.GetObjectData(info, context);
		}
#endif
    }
}
