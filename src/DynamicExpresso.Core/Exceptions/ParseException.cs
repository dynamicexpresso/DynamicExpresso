#if !NETSTANDARD1_6
using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
#endif
using DynamicExpresso.Resources;

namespace DynamicExpresso.Exceptions
{
	#if !NETSTANDARD1_6
	[Serializable]
	#endif
	public class ParseException : DynamicExpressoException
	{
		public ParseException(string message, int position)
			: base(string.Format(ErrorMessages.Format, message, position)) 
		{
			Position = position;
		}

		public int Position { get; private set; }

		#if !NETSTANDARD1_6
		protected ParseException(
			SerializationInfo info,
			StreamingContext context)
			: base(info, context) 
		{
			Position = info.GetInt32("Position");
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("Position", Position);

			base.GetObjectData(info, context);
		}
		#endif
	}
}
