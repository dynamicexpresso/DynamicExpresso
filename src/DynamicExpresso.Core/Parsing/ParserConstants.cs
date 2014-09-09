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
		public static readonly Expression nullLiteral = Expression.Constant(null);

		public const string keywordAs = "as";
		public const string keywordIs = "is";
		public const string keywordNew = "new";
		public const string keywordTypeof = "typeof";
	}
}
