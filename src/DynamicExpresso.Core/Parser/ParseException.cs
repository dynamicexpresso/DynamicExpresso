using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace DynamicExpresso
{
	public sealed class ParseException : Exception
	{
		int position;

		public ParseException(string message, int position)
			: base(message)
		{
			this.position = position;
		}

		public int Position
		{
			get { return position; }
		}

		public override string ToString()
		{
			return string.Format(ErrorMessages.ParseExceptionFormat, Message, position);
		}
	}
}
