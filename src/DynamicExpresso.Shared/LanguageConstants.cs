using DynamicExpresso.Parsing;
using System;
using System.Linq.Expressions;

namespace DynamicExpresso
{
	public static class LanguageConstants
	{
		public static readonly ReferenceType[] PrimitiveTypes = {
            new ReferenceType(typeof(Object)),
            new ReferenceType(typeof(Boolean)),
            new ReferenceType(typeof(Char)),
            new ReferenceType(typeof(String)),
            new ReferenceType(typeof(SByte)),
            new ReferenceType(typeof(Byte)),
            new ReferenceType(typeof(Int16)),
            new ReferenceType(typeof(UInt16)),
            new ReferenceType(typeof(Int32)),
            new ReferenceType(typeof(UInt32)),
            new ReferenceType(typeof(Int64)),
            new ReferenceType(typeof(UInt64)),
            new ReferenceType(typeof(Single)),
            new ReferenceType(typeof(Double)),
            new ReferenceType(typeof(Decimal)),
            new ReferenceType(typeof(DateTime)),
            new ReferenceType(typeof(TimeSpan)),
            new ReferenceType(typeof(Guid))
        };

		/// <summary>
		/// Primitive types alias (string, int, ...)
		/// </summary>
		public static readonly ReferenceType[] CSharpPrimitiveTypes = {
						new ReferenceType("object", typeof(object)),
						new ReferenceType("string", typeof(string)),
						new ReferenceType("char", typeof(char)),
						new ReferenceType("bool", typeof(bool)),
						new ReferenceType("byte", typeof(byte)),
						new ReferenceType("int", typeof(int)),
						new ReferenceType("long", typeof(long)),
						new ReferenceType("double", typeof(double)),
						new ReferenceType("decimal", typeof(decimal))
				};

		/// <summary>
		/// Common .NET Types (Math, Convert, Enumerable)
		/// </summary>
		public static readonly ReferenceType[] CommonTypes = {
            new ReferenceType(typeof(Math)),
            new ReferenceType(typeof(Convert)),
            new ReferenceType(typeof(System.Linq.Enumerable))
        };

		/// <summary>
		/// true, false, null
		/// </summary>
		public static readonly Identifier[] Literals = {
					new Identifier("true", Expression.Constant(true)),
					new Identifier("false", Expression.Constant(false)),
					new Identifier("null", ParserConstants.NULL_LITERAL_EXPRESSION)
				};

		public static readonly string[] ReserverKeywords = ParserConstants.RESERVED_KEYWORDS;
	}
}
