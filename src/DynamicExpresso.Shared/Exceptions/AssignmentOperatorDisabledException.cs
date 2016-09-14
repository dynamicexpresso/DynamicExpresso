using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
#if !WINDOWS_UWP
using System.Security.Permissions;
#endif
using System.Runtime.Serialization;

namespace DynamicExpresso
{
#if !WINDOWS_UWP
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

#if !WINDOWS_UWP
		protected AssignmentOperatorDisabledException(
			SerializationInfo info,
			StreamingContext context)
			: base(info, context) 
		{
			OperatorString = info.GetString("OperatorString");
		}

		[SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("OperatorString", OperatorString);

			base.GetObjectData(info, context);
		}
#endif
    }
}
