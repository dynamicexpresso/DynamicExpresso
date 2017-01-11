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
    public class UnknownIdentifierException : ParseException
    {
        public string Identifier { get; private set; }

        public UnknownIdentifierException(string identifier, int position)
            : base(string.Format("Unknown identifier '{0}'", identifier), position)
        {
            Identifier = identifier;
        }

#if !NET_COREAPP
        protected UnknownIdentifierException(
			SerializationInfo info,
			StreamingContext context)
			: base(info, context) 
		{
			Identifier = info.GetString("Identifier");
		}
		[SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("Identifier", Identifier);

			base.GetObjectData(info, context);
		}
#endif
    }
}
