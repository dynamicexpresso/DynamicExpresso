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
    public class ParseException : DynamicExpressoException
    {
        const string PARSE_EXCEPTION_FORMAT = "{0} (at index {1}).";

        public int Position { get; private set; }

        public ParseException(string message, int position)
            : base(string.Format(PARSE_EXCEPTION_FORMAT, message, position))
        {
            Position = position;
        }
#if !NET_COREAPP
		protected ParseException(
			SerializationInfo info,
			StreamingContext context)
			: base(info, context) 
		{
			Position = info.GetInt32("Position");
		}

		[SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("Position", Position);

			base.GetObjectData(info, context);
		}
#endif
    }
}
