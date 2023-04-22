using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace DynamicExpresso.Exceptions
{
	[Serializable]
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

		protected NoApplicableMethodException(
			SerializationInfo info,
			StreamingContext context)
			: base(info, context)
		{
			MethodTypeName = info.GetString("MethodTypeName");
			MethodName = info.GetString("MethodName");
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("MethodName", MethodName);
			info.AddValue("MethodTypeName", MethodTypeName);

			base.GetObjectData(info, context);
		}
	}
}
