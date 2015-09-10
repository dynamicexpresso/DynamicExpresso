using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace DynamicExpresso.Parsing
{
	internal static class ParserConstants
	{
		public static readonly Expression NULL_LITERAL_EXPRESSION = Expression.Constant(null);

		public const string KEYWORD_AS = "as";
		public const string KEYWORD_IS = "is";
		public const string KEYWORD_NEW = "new";
		public const string KEYWORD_TYPEOF = "typeof";

		public static readonly string[] RESERVED_KEYWORDS = new[]{
				KEYWORD_AS,
				KEYWORD_IS,
				KEYWORD_NEW,
				KEYWORD_TYPEOF
			};
	}
}
