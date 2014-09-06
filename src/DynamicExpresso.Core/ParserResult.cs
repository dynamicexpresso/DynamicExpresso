using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DynamicExpresso
{
	public class ParserResult
	{
		public static ParserResult Valid(Lambda lambda)
		{
			return new ParserResult
			{
				Success = true,
				Lambda = lambda,
				Exception = null
			};
		}

		public static ParserResult Invalid(ParseException exception)
		{
			return new ParserResult
			{
				Success = false,
				Exception = exception
			};
		}

		public bool Success { get; private set; }
		public Lambda Lambda { get; private set; }
		public ParseException Exception { get; private set; }
	}
}
