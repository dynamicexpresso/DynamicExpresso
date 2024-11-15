using System;
using System.Runtime.Serialization;
using DynamicExpresso.Resources;

namespace DynamicExpresso.Exceptions
{
	[Serializable]
	public class DuplicateParameterException : DynamicExpressoException
	{
		public DuplicateParameterException(string identifier)
			: base(string.Format(ErrorMessages.DuplicateParameter, identifier))
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
