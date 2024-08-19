using System;
using System.Runtime.Serialization;

namespace DynamicExpresso.Exceptions
{
	[Serializable]
	public class UnknownIdentifierException : ParseException
	{
		public UnknownIdentifierException(string identifier, int position)
			: base(string.Format("Unknown identifier '{0}'", identifier), position)
		{
			Identifier = identifier;
		}

		public string Identifier { get; private set; }

		protected UnknownIdentifierException(
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
