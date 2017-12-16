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
	public class DuplicateParameterException : DynamicExpressoException
	{
		public DuplicateParameterException(string identifier)
			: base(string.Format("The parameter '{0}' was defined more than once", identifier)) 
		{
			Identifier = identifier;
		}
		public string Identifier { get; private set; }

#if !NETSTANDARD1_6
		protected DuplicateParameterException(
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
