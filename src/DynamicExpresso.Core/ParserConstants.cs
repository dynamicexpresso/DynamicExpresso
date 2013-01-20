using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace DynamicExpresso
{
    internal static class ParserConstants
    {
        public static readonly Expression trueLiteral = Expression.Constant(true);
        public static readonly Expression falseLiteral = Expression.Constant(false);
        public static readonly Expression nullLiteral = Expression.Constant(null);

        public const string keywordIt = "it";
        public const string keywordIif = "iif";
        public const string keywordNew = "new";
    }
}
