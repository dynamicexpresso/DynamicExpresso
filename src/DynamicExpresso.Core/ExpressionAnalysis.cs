using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DynamicExpresso
{
	public class ExpressionAnalysis
	{
		public static ExpressionAnalysis Valid(Type returnType)
		{
			return new ExpressionAnalysis
			{
				Success = true,
				ReturnType = returnType,
				Exception = null
			};
		}

		public static ExpressionAnalysis Invalid(ParseException exception)
		{
			return new ExpressionAnalysis
			{
				Success = false,
				Exception = exception
			};
		}

		public bool Success { get; private set; }
		public Type ReturnType { get; private set; }
		public ParseException Exception { get; private set; }
	}
}
