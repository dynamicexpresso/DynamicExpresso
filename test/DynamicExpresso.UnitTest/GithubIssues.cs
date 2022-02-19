using DynamicExpresso.Exceptions;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

// ReSharper disable SpecifyACultureInStringConversionExplicitly

namespace DynamicExpresso.UnitTest
{
	[TestFixture]
	public class GithubIssues
	{
		[Test]
		public void GitHub_Issue_19()
		{
			var interpreter = new Interpreter();

			Assert.AreEqual(5.0.ToString(), interpreter.Eval("5.0.ToString()"));
			Assert.AreEqual((5).ToString(), interpreter.Eval("(5).ToString()"));
			Assert.AreEqual((5.0).ToString(), interpreter.Eval("(5.0).ToString()"));
			Assert.AreEqual(5.ToString(), interpreter.Eval("5.ToString()"));
		}

		[Test]
		public void GitHub_Issue_43()
		{
			var interpreter = new Interpreter();

			Assert.AreEqual((-.5).ToString(), interpreter.Eval("-.5.ToString()"));
			Assert.AreEqual((.1).ToString(), interpreter.Eval(".1.ToString()"));
			Assert.AreEqual((-1 - .1 - 0.1).ToString(), interpreter.Eval("(-1-.1-0.1).ToString()"));
		}

		[Test]
		public void GitHub_Issue_68()
		{
			var interpreter = new Interpreter();

			var array = new[] { 5, 10, 6 };

			interpreter.SetVariable("array", array);

			Assert.AreEqual(array.Contains(5), interpreter.Eval("array.Contains(5)"));
			Assert.AreEqual(array.Contains(3), interpreter.Eval("array.Contains(3)"));
		}

		[Test]
		public void GitHub_Issue_64()
		{
			var interpreter = new Interpreter();
			Assert.AreEqual(null, interpreter.Eval("null ?? null"));
			Assert.AreEqual("hallo", interpreter.Eval("\"hallo\" ?? null"));
			Assert.AreEqual("hallo", interpreter.Eval("null ?? \"hallo\""));
		}

		[Test]
		public void GitHub_Issue_65_Part1()
		{
			var interpreter = new Interpreter();

			var x = new
			{
				var1 = "hallo",
				var2 = (string)null
			};

			interpreter.SetVariable("x", x);
			Assert.AreEqual("hallo", interpreter.Eval("x.var1?.ToString()"));
			Assert.AreEqual(null, interpreter.Eval("x.var2?.ToString()"));
			Assert.AreEqual("allo", interpreter.Eval("x.var1?.Substring(1)"));
		}

		[Test]
		public void GitHub_Issue_65_Part2()
		{
			var interpreter = new Interpreter();

			var x = new
			{
				var1 = "hallo",
				var2 = (string)null
			};

			interpreter.SetVariable("x", x);
			Assert.AreEqual(x.var1?[2], interpreter.Eval("x.var1?[2]"));
			Assert.AreEqual(x.var2?[2], interpreter.Eval("x.var2?[2]"));
			Assert.AreEqual(x.var1?[2] == 'l', interpreter.Eval("x.var1?[2] == 'l'"));
			Assert.AreEqual(x.var2?[2] == null, interpreter.Eval("x.var2?[2] == null"));
		}

		[Test]
		public void GitHub_Issue_88()
		{
			var interpreter = new Interpreter();

			interpreter.SetVariable("a", 1, typeof(int));
			interpreter.SetVariable("b", 1.2, typeof(double?));
			var result = interpreter.Eval("a + b");

			Assert.AreEqual(result, 2.2);
		}

		[Test]
		public void GitHub_Issue_128()
		{
			var target = new Interpreter();

			Assert.Throws<ParseException>(() => target.Eval("1+1*"));
			Assert.Throws<ParseException>(() => target.Eval("1+1*'a'"));
		}

		[Test]
		public void GitHub_Issue_133()
		{
			var interpreter = new Interpreter();

			Assert.AreEqual(10000000001, interpreter.Eval("1+1e10"));
			Assert.AreEqual(10000000001, interpreter.Eval("1+1e+10"));
			Assert.AreEqual(1.0000000001, interpreter.Eval("1+1e-10"));
			Assert.AreEqual(-20199999999, interpreter.Eval("1 - 2.02e10"));
			Assert.AreEqual(-20199999999, interpreter.Eval("1 - 2.02e+10"));
			Assert.AreEqual(0.999999999798, interpreter.Eval("1 - 2.02e-10"));
			Assert.AreEqual(1e-10, interpreter.Eval("1/1e+10"));

			interpreter.SetVariable("@Var1", 1);
			interpreter.SetVariable("@Var2", 1e+10);
			Assert.AreEqual(10000000001, interpreter.Eval("@Var1+@Var2"));

			interpreter.SetVariable("e", 2);
			Assert.AreEqual(10000000003, interpreter.Eval("@Var1+@Var2+e"));
		}

		private delegate bool GFunction(string arg = null);

		static bool GetGFunction1(string arg = null)
		{
			return arg != null;
		}

		[Test]
		public void GitHub_Issue_144_1()
		{
			// GetGFunction1 is defined outside the test function
			GFunction gFunc1 = GetGFunction1;

			Assert.True(gFunc1.Method.GetParameters()[0].HasDefaultValue);

			var flags = BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance;
			var invokeMethod1 = (MethodInfo)gFunc1.GetType().FindMembers(MemberTypes.Method, flags, Type.FilterName, "Invoke")[0];
			Assert.True(invokeMethod1.GetParameters()[0].HasDefaultValue);

			var interpreter = new Interpreter();
			interpreter.SetFunction("GFunction", gFunc1);
			interpreter.SetVariable("arg", "arg");

			Assert.True((bool)interpreter.Eval("GFunction(arg)"));
			Assert.False((bool)interpreter.Eval("GFunction()"));
		}

		[Test]
		public void GitHub_Issue_148()
		{
			Func<object[], double, double, object[]> subArray = (entries, skipFirst, skipLast) => entries.Take(entries.Length - (int)skipLast).Skip((int)skipFirst).ToArray();

			var target = new Interpreter();

			target.SetVariable("arr1", new object[] { 1d, 2d, 3d });
			target.SetFunction("SubArray", subArray);

			Assert.AreEqual(2, target.Eval("SubArray(arr1, 1, 1).First()"));
		}


#if NETCOREAPP2_1_OR_GREATER

		[Test]
		public void GitHub_Issue_144_2()
		{
			// GetGFunction2 is defined inside the test function
			static bool GetGFunction2(string arg = null)
			{
				return arg != null;
			}

			GFunction gFunc2 = GetGFunction2;
			Assert.False(gFunc2.Method.GetParameters()[0].HasDefaultValue); // should be true!

			var flags = BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance;
			var invokeMethod2 = (MethodInfo)gFunc2.GetType().FindMembers(MemberTypes.Method, flags, Type.FilterName, "Invoke")[0];
			Assert.True(invokeMethod2.GetParameters()[0].HasDefaultValue);

			var interpreter = new Interpreter();
			interpreter.SetFunction("GFunction", gFunc2);
			interpreter.SetVariable("arg", "arg");

			Assert.True((bool)interpreter.Eval("GFunction(arg)"));
			Assert.False((bool)interpreter.Eval("GFunction()"));
		}

		[Test]
		public void GitHub_Issue_144_3()
		{
			// GetGFunction2 is defined inside the test function
			static bool GetGFunction2(string arg = null)
			{
				return arg == null;
			}

			GFunction gFunc1 = GetGFunction1;
			GFunction gFunc2 = GetGFunction2;

			var interpreter = new Interpreter();
			interpreter.SetFunction("GFunction", gFunc1);
			interpreter.SetFunction("GFunction", gFunc2);
			interpreter.SetVariable("arg", "arg");

			// ambiguous call
			Assert.Throws<ParseException>(() => interpreter.Eval("GFunction(arg)"));

			// there should be an ambiguous call exception, but GFunction1 is used
			// because gFunc1.Method.GetParameters()[0].HasDefaultValue == true
			// and     gFunc2.Method.GetParameters()[0].HasDefaultValue == false
			Assert.False((bool)interpreter.Eval("GFunction()"));
		}

		[Test]
		public void Github_Issue_159()
		{
			// GetGFunction2 is defined inside the test function
			static bool GetGFunction2(string arg = null)
			{
				return arg == null;
			}

			GFunction gFunc1 = GetGFunction1;
			GFunction gFunc2 = GetGFunction2;

			var interpreter = new Interpreter();
			interpreter.SetFunction("GFunction", gFunc1);
			interpreter.SetFunction("GFunction", gFunc2);
			interpreter.SetVariable("arg", "arg");

			// ambiguous call
			var exception = Assert.Throws<ParseException>(() => interpreter.Eval("GFunction(arg)"));
			Assert.AreEqual("Ambiguous invocation of delegate (multiple overloads found) (at index 0).", exception.Message);

			interpreter = new Interpreter();
			interpreter.SetIdentifier(new FunctionIdentifier("GFunction", gFunc1));
			interpreter.SetIdentifier(new FunctionIdentifier("GFunction", gFunc2));
			interpreter.SetVariable("arg", "arg");

			// normal call
			Assert.False((bool)interpreter.Eval("GFunction(arg)"));
		}
#endif

		[Test]
		public void GitHub_Issue_164()
		{
			var interpreter = new Interpreter();

			var str = "str";

			interpreter.SetVariable("str", str);
			Assert.AreEqual(str?.Length, interpreter.Eval("str?.Length"));
			Assert.AreEqual(str?.Length == 3, interpreter.Eval<bool>("str?.Length == 3"));

			str = null;
			interpreter.SetVariable("str", str);
			Assert.AreEqual(str?.Length, interpreter.Eval("str?.Length"));
			Assert.AreEqual(str?.Length == 0, interpreter.Eval<bool>("str?.Length == 0"));
		}

		[Test]
		public void GitHub_Issue_164_bis()
		{
			var interpreter = new Interpreter();

			var lambda = interpreter.Parse("Scope?.ValueInt", new Parameter("Scope", typeof(Scope)));

			var result = lambda.Invoke((Scope)null);
			Assert.IsNull(result);

			result = lambda.Invoke(new Scope { ValueInt = 5 });
			Assert.AreEqual(5, result);

			var scope = new Scope { Value = 5 };
			interpreter.SetVariable("scope", scope);
			Assert.AreEqual(scope?.Value.HasValue, interpreter.Eval<bool>("scope?.Value.HasValue"));

			// must throw, because scope.ValueInt is not a nullable type
			Assert.Throws<ParseException>(() => interpreter.Eval<bool>("scope.ValueInt.HasValue"));
		}

		private class Scope
		{
			public int ValueInt { get; set; }
			public int? Value { get; set; }

			public int[] ArrInt { get; set; }
			public int?[] Arr { get; set; }
		}

		[Test]
		public void GitHub_Issue_169()
		{
			var interpreter = new Interpreter();

			var lambda = interpreter.Parse("Scope?.Value", new Parameter("Scope", typeof(Scope)));

			var result = lambda.Invoke((Scope)null);
			Assert.IsNull(result);

			result = lambda.Invoke(new Scope());
			Assert.IsNull(result);

			result = lambda.Invoke(new Scope { Value = null });
			Assert.IsNull(result);

			result = lambda.Invoke(new Scope { Value = 5 });
			Assert.AreEqual(5, result);
		}

		[Test]
		public void GitHub_Issue_169_bis()
		{
			var interpreter = new Interpreter();

			var lambda = interpreter.Parse("Scope?.Arr?[0]", new Parameter("Scope", typeof(Scope)));

			var result = lambda.Invoke(new Scope());
			Assert.IsNull(result);

			result = lambda.Invoke(new Scope { Arr = null });
			Assert.IsNull(result);

			result = lambda.Invoke(new Scope { Arr = new int?[] { 5 } });
			Assert.AreEqual(5, result);
		}

		[Test]
		public void GitHub_Issue_169_ter()
		{
			var interpreter = new Interpreter();

			var lambda = interpreter.Parse("Scope?.ArrInt?[0]", new Parameter("Scope", typeof(Scope)));

			var result = lambda.Invoke(new Scope());
			Assert.IsNull(result);

			result = lambda.Invoke(new Scope { ArrInt = new int[] { 5 } });
			Assert.AreEqual(5, result);

			interpreter.SetVariable("scope", new Scope { ArrInt = new int[] { 5 } });
			var resultNullableBool = interpreter.Eval<bool>("scope?.ArrInt?[0].HasValue");
			Assert.IsTrue(resultNullableBool);

			// must throw, because scope.ValueInt is not a nullable type
			Assert.Throws<ParseException>(() => interpreter.Eval<bool>("scope.ArrInt[0].HasValue"));
		}

		[Test]
		public void GitHub_Issue_169_quatro()
		{
			var interpreter = new Interpreter();

			interpreter.SetVariable("x", (int?)null);
			var result = interpreter.Eval<string>("x?.ToString()");
			Assert.IsNull(result);

			interpreter.SetVariable("x", (int?)56);
			result = interpreter.Eval<string>("x?.ToString()");
			Assert.AreEqual("56", result);
		}

		[Test]
		public void GitHub_Issue_197()
		{
			var interpreterWithLambdas = new Interpreter(InterpreterOptions.DefaultCaseInsensitive | InterpreterOptions.LambdaExpressions);
			var interpreterWithoutLambdas = new Interpreter(InterpreterOptions.DefaultCaseInsensitive);

			var stringExpression = "booleanValue ? someStringValue : \".\"";
			var parameters = new List<Parameter>
			{
				new Parameter($"someStringValue", typeof(string), $"E33"),
				new Parameter("booleanValue", typeof(bool), true)
			};

			var expressionWithoutLambdas = interpreterWithoutLambdas.Parse(stringExpression, typeof(void), parameters.ToArray());
			Assert.AreEqual("E33", expressionWithoutLambdas.Invoke(parameters.ToArray()));

			var expressionWithLambdas = interpreterWithLambdas.Parse(stringExpression, typeof(void), parameters.ToArray());
			Assert.AreEqual("E33", expressionWithLambdas.Invoke(parameters.ToArray()));
		}

		[Test]
		public void GitHub_Issue_185()
		{
			var interpreter = new Interpreter().SetVariable("a", 123L);

			// forcing the return type to object should work
			// (ie a conversion expression should be emitted from long to object)
			var del = interpreter.ParseAsDelegate<Func<object>>("a*2");
			var result = del();
			Assert.AreEqual(246, result);
		}

		[Test]
		public void GitHub_Issue_185_2()
		{
			var interpreter = new Interpreter().SetVariable("a", 123L);
			var del = interpreter.ParseAsDelegate<Func<dynamic>>("a*2");
			var result = del();
			Assert.AreEqual(246, result);
		}

		[Test]
		public void GitHub_Issue_191()
		{
			var interpreter = new Interpreter();
			interpreter.Reference(typeof(Utils));

			var result = interpreter.Eval<object>("Utils.Select(Utils.Array(\"a\", \"b\"), \"x+x\")");
			Assert.IsNotNull(result);
		}

		[Test]
		public void GitHub_Issue_203()
		{
			var target = new Interpreter();
			target.Reference(typeof(Utils));

			var list = new[] { 1, 2, 3 };

			var listInt = target.Eval<List<int>>("Utils.Array(list)", new Parameter("list", list));
			Assert.AreEqual(Utils.Array(list), listInt);
		}

		[Test]
		public void GitHub_Issue_205_Property_on_nullable()
		{
			var interpreter = new Interpreter();

			DateTime? date = DateTime.UtcNow;
			interpreter.SetVariable("date", date);

			Assert.AreEqual(date?.Day, interpreter.Eval("date?.Day"));
			Assert.AreEqual(date?.IsDaylightSavingTime(), interpreter.Eval("date?.IsDaylightSavingTime()"));

			date = null;
			interpreter.SetVariable("date", date);

			Assert.AreEqual(date?.Day, interpreter.Eval("date?.Day"));
			Assert.AreEqual(date?.IsDaylightSavingTime(), interpreter.Eval("date?.IsDaylightSavingTime()"));
		}

		[Test]
		public void GitHub_Issue_205()
		{
			var interpreter = new Interpreter();

			var date1 = DateTimeOffset.UtcNow;
			DateTimeOffset? date2 = null;

			interpreter.SetVariable("date1", date1);
			interpreter.SetVariable("date2", date2);

			Assert.IsNull(interpreter.Eval("(date1 - date2)?.Days"));

			date2 = date1.AddDays(1);
			interpreter.SetVariable("date2", date2);
			Assert.AreEqual(-1, interpreter.Eval("(date1 - date2)?.Days"));
		}

		[Test]
		public void GitHub_Issue_217()
		{
			var target = new Interpreter();
			target.Reference(typeof(Utils));
			target.Reference(typeof(IEnumerable<>));

			Assert.AreEqual(1, Utils.Any((IEnumerable<object>)null));
			Assert.AreEqual(2, Utils.Any((IEnumerable)null));
			Assert.AreEqual(2, Utils.Any(null));
			Assert.AreEqual(1, target.Eval("Utils.Any(list)", new Parameter("list", typeof(IEnumerable<object>), null)));
			Assert.AreEqual(2, target.Eval("Utils.Any(list)", new Parameter("list", typeof(IEnumerable), null)));
			Assert.AreEqual(2, target.Eval("Utils.Any(null)"));
		}

		[Test]
		public void GitHub_Issue_221_Case_insensitivity()
		{
			var interpreter = new Interpreter(InterpreterOptions.LambdaExpressions | InterpreterOptions.DefaultCaseInsensitive)
				.Reference(typeof(DateTimeOffset))
				.Reference(typeof(GithubIssuesTestExtensionsMethods))
				.SetFunction("Now", (Func<DateTimeOffset>)(() => DateTimeOffset.UtcNow))
				.SetVariable("List", new List<DateTimeOffset> { DateTimeOffset.UtcNow.AddDays(5) })
				.SetVariable("DateInThePast", DateTimeOffset.UtcNow.AddDays(-5));

			// actual case
			Assert.IsTrue(interpreter.Eval<bool>("List.Any(x => x > Now())"));
			Assert.IsTrue(interpreter.Eval<bool>("List.Any(x => x is DateTimeOffset)"));
			Assert.IsFalse(interpreter.Eval<bool>("DateInThePast.IsInFuture()"));

			// case insensivity outside lambda expressions
			Assert.IsFalse(interpreter.Eval<bool>("dateinthepast > now()")); // identifier
			Assert.IsTrue(interpreter.Eval<bool>("dateinthepast is datetimeoffset)")); // known type
			Assert.IsFalse(interpreter.Eval<bool>("dateinthepast.isinfuture()")); // extension method

			// ensure the case insensitivity option is also used in the lambda expression
			Assert.IsTrue(interpreter.Eval<bool>("list.Any(x => x > now())")); // identifier
			Assert.IsTrue(interpreter.Eval<bool>("list.Any(x => x is datetimeoffset)")); // known type
			Assert.IsTrue(interpreter.Eval<bool>("list.Any(x => x.isinfuture())")); // extension method
		}

		[Test]
		public void GitHub_Issue_221_Reflection_not_allowed()
		{
			var interpreter = new Interpreter(InterpreterOptions.LambdaExpressions | InterpreterOptions.Default)
				.SetVariable("list", new List<Type> { typeof(double) });

			Assert.Throws<ReflectionNotAllowedException>(() => interpreter.Parse("typeof(double).GetMethods()"));
			Assert.Throws<ReflectionNotAllowedException>(() => interpreter.Parse("list.SelectMany(t => t.GetMethods())"));
		}

		public static class Utils
		{
			public static List<T> Array<T>(IEnumerable<T> collection) => new List<T>(collection);
			public static List<dynamic> Array(params dynamic[] array) => Array((IEnumerable<dynamic>)array);
			public static IEnumerable<dynamic> Select<TSource>(IEnumerable<TSource> collection, string expression) => new List<dynamic>();
			public static IEnumerable<dynamic> Select(IEnumerable collection, string expression) => new List<dynamic>();
			public static int Any<T>(IEnumerable<T> collection) => 1;
			public static int Any(IEnumerable collection) => 2;
		}
	}

	internal static class GithubIssuesTestExtensionsMethods
	{
		public static bool IsInFuture(this DateTimeOffset date) => date > DateTimeOffset.UtcNow;
	}
}
