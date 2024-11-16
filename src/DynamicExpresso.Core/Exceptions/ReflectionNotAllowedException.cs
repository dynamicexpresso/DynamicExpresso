using System;
using System.Runtime.Serialization;
using DynamicExpresso.Resources;

namespace DynamicExpresso.Exceptions
{
	[Serializable]
	public class ReflectionNotAllowedException : ParseException
	{
		public ReflectionNotAllowedException()
			: base(ErrorMessages.ReflectionNotAllowed, 0)
		{
		}

		protected ReflectionNotAllowedException(
			SerializationInfo info,
			StreamingContext context)
			: base(info, context)
		{
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
		}
	}
}
