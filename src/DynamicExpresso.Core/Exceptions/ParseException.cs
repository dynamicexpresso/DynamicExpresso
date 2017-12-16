using System;
using System.Security.Permissions;
using System.Runtime.Serialization;
using DynamicExpresso.Resources;

namespace DynamicExpresso
{
	[Serializable]
	public class ParseException : DynamicExpressoException
	{
		public ParseException(string message, int position)
			: base(string.Format(ErrorMessages.Format, message, position)) 
		{
			Position = position;
		}

		protected ParseException(
			SerializationInfo info,
			StreamingContext context)
			: base(info, context) 
		{
			Position = info.GetInt32("Position");
		}

		public int Position { get; private set; }

		[SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("Position", Position);

			base.GetObjectData(info, context);
		}
	}
}
