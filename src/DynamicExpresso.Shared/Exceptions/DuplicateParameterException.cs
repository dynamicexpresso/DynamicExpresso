using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Security.Permissions;
using System.Runtime.Serialization;

namespace DynamicExpresso
{
	[Serializable]
	public class DuplicateParameterException : DynamicExpressoException
	{
		public DuplicateParameterException(string identifier)
			: base(string.Format("The parameter '{0}' was defined more than once", identifier)) 
		{
			Identifier = identifier;
		}

		protected DuplicateParameterException(
			SerializationInfo info,
			StreamingContext context)
			: base(info, context) 
		{
			Identifier = info.GetString("Identifier");
		}

		public string Identifier { get; private set; }

		[SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("Identifier", Identifier);

			base.GetObjectData(info, context);
		}
	}
}
