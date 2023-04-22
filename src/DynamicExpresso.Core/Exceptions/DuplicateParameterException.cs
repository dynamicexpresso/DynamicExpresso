using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace DynamicExpresso.Exceptions
{
	[Serializable]
	public class DuplicateParameterException : DynamicExpressoException
	{
		public DuplicateParameterException(string identifier)
			: base(string.Format("The parameter '{0}' was defined more than once", identifier))
		{
			Identifier = identifier;
		}
		public string Identifier { get; private set; }

		protected DuplicateParameterException(
			SerializationInfo info,
			StreamingContext context)
			: base(info, context)
		{
			Identifier = info.GetString("Identifier");
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("Identifier", Identifier);

			base.GetObjectData(info, context);
		}
	}
}
