#if !NETSTANDARD1_6
using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
#endif

namespace DynamicExpresso.Exceptions
{
#if !NETSTANDARD1_6
	[Serializable]
#endif
	public class NoApplicableMethodException : ParseException
	{
		public NoApplicableMethodException(string methodName, string methodTypeName, int position)
			: base(string.Format("No applicable method '{0}' exists in type '{1}'", methodName, methodTypeName), position) 
		{
			MethodTypeName = methodTypeName;
			MethodName = methodName;
		}

		public string MethodTypeName { get; private set; }
		public string MethodName { get; private set; }

#if !NETSTANDARD1_6
		protected NoApplicableMethodException(
			SerializationInfo info,
			StreamingContext context)
			: base(info, context) 
		{
			MethodTypeName = info.GetString("MethodTypeName");
			MethodName = info.GetString("MethodName");
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("MethodName", MethodName);
			info.AddValue("MethodTypeName", MethodTypeName);

			base.GetObjectData(info, context);
		}
#endif
	}
}
