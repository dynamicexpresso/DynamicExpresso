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
	public class UnknownIdentifierException : ParseException
	{
		public UnknownIdentifierException(string identifier, int position)
			: base(string.Format("Unknown identifier '{0}'", identifier), position) 
		{
			Identifier = identifier;
		}

		public string Identifier { get; private set; }

#if !NETSTANDARD1_6
		protected UnknownIdentifierException(
			SerializationInfo info,
			StreamingContext context)
			: base(info, context) 
		{
			Identifier = info.GetString("Identifier");
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("Identifier", Identifier);

			base.GetObjectData(info, context);
		}
#endif
	}
}
