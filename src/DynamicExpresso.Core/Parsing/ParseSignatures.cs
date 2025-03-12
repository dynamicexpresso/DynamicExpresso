using System;
using System.Collections.Generic;
using System.Linq;
using DynamicExpresso.Reflection;

namespace DynamicExpresso.Parsing
{
	internal class ParseSignatures
	{
		private static MethodData[] MakeUnarySignatures(params Type[] possibleParamTypes)
		{
			var signatures = new MethodData[possibleParamTypes.Length];
			for (var i = 0; i < possibleParamTypes.Length; i++)
			{
				signatures[i] = new MethodData
				{
					Parameters = new[] { new SimpleParameterInfo(possibleParamTypes[i]) },
				};
			}
			return signatures;
		}

		private static MethodData[] MakeBinarySignatures(IList<(Type, Type)> possibleParamTypes)
		{
			var signatures = new MethodData[possibleParamTypes.Count];
			for (var i = 0; i < possibleParamTypes.Count; i++)
			{
				var (left, right) = possibleParamTypes[i];
				signatures[i] = new MethodData
				{
					Parameters = new[] { new SimpleParameterInfo(left), new SimpleParameterInfo(right) },
				};
			}
			return signatures;
		}

		/// <summary>
		/// Signatures for the binary logical operators.
		/// </summary>
		public static MethodData[] LogicalSignatures = MakeBinarySignatures(new[]
		{
			(typeof(bool),  typeof(bool) ),
			(typeof(bool?), typeof(bool?)),
		});

		/// <summary>
		/// Signatures for the binary arithmetic operators.
		/// </summary>
		public static MethodData[] ArithmeticSignatures = MakeBinarySignatures(new[]
		{
			(typeof(int),      typeof(int)     ),
			(typeof(uint),     typeof(uint)    ),
			(typeof(long),     typeof(long)    ),
			(typeof(ulong),    typeof(ulong)   ),
			(typeof(float),    typeof(float)   ),
			(typeof(double),   typeof(double)  ),
			(typeof(decimal),  typeof(decimal) ),
			(typeof(int?),     typeof(int?)    ),
			(typeof(uint?),    typeof(uint?)   ),
			(typeof(long?),    typeof(long?)   ),
			(typeof(ulong?),   typeof(ulong?)  ),
			(typeof(float?),   typeof(float?)  ),
			(typeof(double?),  typeof(double?) ),
			(typeof(decimal?), typeof(decimal?)),
		});

		/// <summary>
		/// Signatures for the binary relational operators.
		/// </summary>
		public static MethodData[] RelationalSignatures = ArithmeticSignatures.Concat(MakeBinarySignatures(new[]
		{
			(typeof(string),    typeof(string)   ),
			(typeof(char),      typeof(char)     ),
			(typeof(DateTime),  typeof(DateTime) ),
			(typeof(TimeSpan),  typeof(TimeSpan) ),
			(typeof(char?),     typeof(char?)    ),
			(typeof(DateTime?), typeof(DateTime?)),
			(typeof(TimeSpan?), typeof(TimeSpan?)),
		})).ToArray();

		/// <summary>
		/// Signatures for the binary equality operators.
		/// </summary>
		public static MethodData[] EqualitySignatures = RelationalSignatures.Concat(LogicalSignatures).ToArray();

		/// <summary>
		/// Signatures for the binary + operators.
		/// </summary>
		public static MethodData[] AddSignatures = ArithmeticSignatures.Concat(MakeBinarySignatures(new[]
		{
			(typeof(DateTime),  typeof(TimeSpan) ),
			(typeof(TimeSpan),  typeof(TimeSpan) ),
			(typeof(DateTime?), typeof(TimeSpan?)),
			(typeof(TimeSpan?), typeof(TimeSpan?)),
		})).ToArray();

		/// <summary>
		/// Signatures for the binary - operators.
		/// </summary>
		public static MethodData[] SubtractSignatures = AddSignatures.Concat(MakeBinarySignatures(new[]
		{
			(typeof(DateTime),  typeof(DateTime)),
			(typeof(DateTime?), typeof(DateTime?)),
		})).ToArray();

		/// <summary>
		/// Signatures for the unary - operators.
		/// </summary>
		public static MethodData[] NegationSignatures = MakeUnarySignatures(
			typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal),
			typeof(int?), typeof(uint?), typeof(long?), typeof(ulong?), typeof(float?), typeof(double?), typeof(decimal?)
		);

		/// <summary>
		/// Signatures for the unary not (!) operator.
		/// </summary>
		public static MethodData[] NotSignatures = MakeUnarySignatures(typeof(bool), typeof(bool?));

		/// <summary>
		/// Signatures for the bitwise completement operators.
		/// </summary>
		public static MethodData[] BitwiseComplementSignatures = MakeUnarySignatures(
			typeof(int), typeof(uint), typeof(long), typeof(ulong),
			typeof(int?), typeof(uint?), typeof(long?), typeof(ulong?)
		);

		/// <summary>
		/// Signatures for the left and right shift operators.
		/// </summary>
		public static MethodData[] ShiftSignatures = MakeBinarySignatures(new[]
		{
			(typeof(int),   typeof(int) ),
			(typeof(uint),  typeof(int) ),
			(typeof(long),  typeof(int) ),
			(typeof(ulong), typeof(int) ),
			(typeof(int?),  typeof(int?) ),
			(typeof(uint?), typeof(int?) ),
			(typeof(long?), typeof(int?) ),
			(typeof(ulong?),typeof(int?) ),
		});

		//interface IEnumerableSignatures
		//{
		//    void Where(bool predicate);
		//    void Any();
		//    void Any(bool predicate);
		//    void All(bool predicate);
		//    void Count();
		//    void Count(bool predicate);
		//    void Min(object selector);
		//    void Max(object selector);
		//    void Sum(int selector);
		//    void Sum(int? selector);
		//    void Sum(long selector);
		//    void Sum(long? selector);
		//    void Sum(float selector);
		//    void Sum(float? selector);
		//    void Sum(double selector);
		//    void Sum(double? selector);
		//    void Sum(decimal selector);
		//    void Sum(decimal? selector);
		//    void Average(int selector);
		//    void Average(int? selector);
		//    void Average(long selector);
		//    void Average(long? selector);
		//    void Average(float selector);
		//    void Average(float? selector);
		//    void Average(double selector);
		//    void Average(double? selector);
		//    void Average(decimal selector);
		//    void Average(decimal? selector);
		//}
	}
}
