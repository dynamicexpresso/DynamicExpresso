using System;
using System.Runtime.Serialization;
using DynamicExpresso.Resources;

namespace DynamicExpresso.Exceptions
{
	[Serializable]
	public class ParseException : DynamicExpressoException
	{
		public ParseException(string message, int position)
			: base(string.Format(ErrorMessages.Format, message, position))
		{
			Position = position;
		}

		public ParseException(string message, int position, Exception innerException)
			: base(string.Format(ErrorMessages.Format, message, position), innerException)
		{
			Position = position;
		}

		public int Position { get; private set; }

		protected ParseException(
			SerializationInfo info,
			StreamingContext context)
			: base(info, context)
		{
			Position = info.GetInt32("Position");
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("Position", Position);

			base.GetObjectData(info, context);
		}
	}
}
