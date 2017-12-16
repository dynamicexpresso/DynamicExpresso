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
	public class AssignmentOperatorDisabledException : ParseException
	{
		public AssignmentOperatorDisabledException(string operatorString, int position)
			: base(string.Format("Assignment operator '{0}' not allowed", operatorString), position) 
		{
			OperatorString = operatorString;
		}

		public string OperatorString { get; private set; }

		#if !NETSTANDARD1_6
		protected AssignmentOperatorDisabledException(
			SerializationInfo info,
			StreamingContext context)
			: base(info, context) 
		{
			OperatorString = info.GetString("OperatorString");
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("OperatorString", OperatorString);

			base.GetObjectData(info, context);
		}
		#endif
	}
}
