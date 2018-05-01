using DynamicExpresso.Parsing;
using System;
using System.Linq.Expressions;

namespace DynamicExpresso
{
	public static class LanguageConstants
	{
		public static readonly ReferenceType[] PrimitiveTypes = {
            new ReferenceType(typeof(object)),
            new ReferenceType(typeof(bool)),
            new ReferenceType(typeof(char)),
            new ReferenceType(typeof(string)),
            new ReferenceType(typeof(sbyte)),
            new ReferenceType(typeof(byte)),
            new ReferenceType(typeof(short)),
            new ReferenceType(typeof(ushort)),
            new ReferenceType(typeof(int)),
            new ReferenceType(typeof(uint)),
            new ReferenceType(typeof(long)),
            new ReferenceType(typeof(ulong)),
            new ReferenceType(typeof(float)),
            new ReferenceType(typeof(double)),
            new ReferenceType(typeof(decimal)),
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
					new Identifier("null", ParserConstants.NullLiteralExpression)
				};

		[Obsolete("Use ReservedKeywords")]
		public static readonly string[] ReserverKeywords = ParserConstants.ReservedKeywords;

		public static readonly string[] ReservedKeywords = ParserConstants.ReservedKeywords;
	}
}
