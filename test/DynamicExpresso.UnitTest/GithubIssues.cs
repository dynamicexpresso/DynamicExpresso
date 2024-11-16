using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using DynamicExpresso.Exceptions;
using NUnit.Framework;

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

		[Test]
		public void GitHub_Issue_159_ambiguous_call()
		{
			Func<double?, int> f1 = d => 1;
			Func<string, int> f2 = o => 2;

			var interpreter = new Interpreter();
			interpreter.SetFunction("f", f1);
			interpreter.SetFunction("f", f2);

			// we should properly throw an ambiguous invocation exception (multiple matching overloads found)
			// and not an Argument list incompatible with delegate expression (no matching overload found)
			var exc = Assert.Throws<ParseException>(() => interpreter.Eval("f(null)"));
			StringAssert.StartsWith("Ambiguous invocation of delegate (multiple overloads found)", exc.Message);
		}


		[Test]
		public void GitHub_Issue_159_unset_identifier()
		{
			Func<int> f1 = () => 1;

			var interpreter = new Interpreter();
			interpreter.SetFunction("f", f1);

			Assert.AreEqual(1, interpreter.Eval("f()"));

			// calls to f should lead to an unknown identifier exception
			interpreter.UnsetFunction("f");
			Assert.Throws<UnknownIdentifierException>(() => interpreter.Eval("f()"));
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
			static bool GetGFunction2(string arg)
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

			// GFunction1 is used
			// because gFunc1.Method.GetParameters()[0].HasDefaultValue == true
			// and     gFunc2.Method.GetParameters()[0].HasDefaultValue == false
			Assert.False((bool)interpreter.Eval("GFunction()"));
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
			Assert.IsTrue(interpreter.Eval<bool>("dateinthepast is datetimeoffset")); // known type
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
			public static int ParamArrayObjects(params object[] values) => values.Length;
			public static IEnumerable<dynamic> Select<TSource>(IEnumerable<TSource> collection, string expression) => new List<dynamic>();
			public static IEnumerable<dynamic> Select(IEnumerable collection, string expression) => new List<dynamic>();
			public static int Any<T>(IEnumerable<T> collection) => 1;
			public static int Any(IEnumerable collection) => 2;
		}

		[Test]
		public void GitHub_Issue_235()
		{
			var target = new Interpreter();
			target.Reference(typeof(RegexOptions));
			target.Reference(typeof(DateTimeKind));

			var result = target.Eval<RegexOptions>("RegexOptions.Compiled | RegexOptions.Singleline");
			Assert.True(result.HasFlag(RegexOptions.Compiled));
			Assert.True(result.HasFlag(RegexOptions.Singleline));

			// DateTimeKind doesn't have the Flags attribute: the bitwise operation returns an integer
			var result2 = target.Eval<DateTimeKind>("DateTimeKind.Local | DateTimeKind.Utc");
			Assert.AreEqual((DateTimeKind)3, result2);
		}

		[Test]
		public void GitHub_Issue_212()
		{
			var target = new Interpreter(InterpreterOptions.Default | InterpreterOptions.LambdaExpressions);
			var list = new Parameter("list", new[] { 1, 2, 3 });
			var value1 = new Parameter("value", 1);
			var value2 = new Parameter("value", 2);
			var expression = "list.Where(x => x > value)";
			var lambda = target.Parse(expression, list, value1);
			var result = lambda.Invoke(list, value2);
			Assert.AreEqual(new[] { 3 }, result);
		}

		[Test]
		public void GitHub_Issue_212_bis()
		{
			var target = new Interpreter(InterpreterOptions.Default | InterpreterOptions.LambdaExpressions);
			var list = new Parameter("list", new[] { 1, 2, 3 });
			var value1 = new Parameter("value", 1);
			var value2 = new Parameter("value", 2);
			var expression = "list.Where(x => x > value)";
			var lambda = target.Parse(expression, (new[] { list, value1 }).Select(p => new Parameter(p.Name, p.Type)).ToArray());
			var result = lambda.Invoke(list, value1);
			Assert.AreEqual(new[] { 2, 3 }, result);
		}

		[Test]
		public void GitHub_Issue_200_capture()
		{
			var target = new Interpreter(InterpreterOptions.Default | InterpreterOptions.LambdaExpressions);
			var list = new List<string> { "ab", "cdc" };
			target.SetVariable("myList", list);

			// the str parameter is captured, and can be used in the nested lambda
			var results = target.Eval("myList.Select(str => str.Select(c => str.Length))");
			Assert.AreEqual(new[] { new[] { 2, 2 }, new[] { 3, 3, 3 } }, results);
		}

		[Test]
		public void Lambda_Issue_256()
		{
			ICollection<BonusMatrix> annualBonus = new List<BonusMatrix> {
				new BonusMatrix() { Grade = 1, BonusFactor = 7 },
				new BonusMatrix() { Grade = 2, BonusFactor = 5.5 },
				new BonusMatrix() { Grade = 3, BonusFactor = 4 },
				new BonusMatrix() { Grade = 4, BonusFactor = 3.5 },
				new BonusMatrix() { Grade = 5, BonusFactor = 3 }
			};

			ICollection<Employee> employees = new List<Employee> {
				new Employee() { Id = "01", Name = "A", Grade = 5, Salary = 20000}, //bonus = 20000 * 7   = 60000
                new Employee() { Id = "02", Name = "B", Grade = 5, Salary = 18000}, //bonus = 18000 * 7   = 54000
                new Employee() { Id = "03", Name = "C", Grade = 4, Salary = 12000}, //bonus = 12000 * 5.5 = 42000
                new Employee() { Id = "04", Name = "D", Grade = 4, Salary = 10000}, //bonus = 10000 * 5.5 = 35000
                new Employee() { Id = "05", Name = "E", Grade = 3, Salary = 8500},  //bonus = 8500  * 4   = 34000
                new Employee() { Id = "06", Name = "F", Grade = 3, Salary = 8000},  //bonus = 8000  * 4   = 32000
                new Employee() { Id = "07", Name = "G", Grade = 2, Salary = 5000},  //bonus = 5000  * 3.5 = 27500
                new Employee() { Id = "08", Name = "H", Grade = 2, Salary = 4750},  //bonus = 4750  * 3.5 = 26125
                new Employee() { Id = "09", Name = "I", Grade = 1, Salary = 3500},  //bonus = 3500  * 3   = 24500
                new Employee() { Id = "10", Name = "J", Grade = 1, Salary = 3250}   //bonus = 3250  * 3   = 22750
            };

			var interpreter = new Interpreter(InterpreterOptions.LambdaExpressions | InterpreterOptions.Default);
			interpreter.SetVariable(nameof(annualBonus), annualBonus);
			interpreter.SetVariable(nameof(employees), employees);

			var totalBonus = employees.Sum(x => x.Salary * (annualBonus.SingleOrDefault(y => y.Grade == x.Grade).BonusFactor)); //total = 357875

			var evalSum = interpreter.Eval("employees.Sum(x => x.Salary * (annualBonus.SingleOrDefault(y => y.Grade == x.Grade).BonusFactor))");
			Assert.AreEqual(totalBonus, evalSum);
		}

		public class Employee
		{
			public string Id { get; set; }
			public string Name { get; set; }
			public int Grade { get; set; }
			public double Salary { get; set; }
		}

		public class BonusMatrix
		{
			public int Grade { get; set; }
			public double BonusFactor { get; set; }
		}

		[Test]
		public void Lambda_Issue_259()
		{
			var options = InterpreterOptions.Default | InterpreterOptions.LambdaExpressions;
			var interpreter = new Interpreter(options);
			interpreter.SetVariable("courseList", new[] { new { PageName = "Test" } });
			interpreter.Reference(typeof(PageType));

			var results = interpreter.Eval<IEnumerable<PageType>>(@"courseList.Select(x => new PageType() { PageName = x.PageName, VisualCount = 5 })");
			Assert.AreEqual(1, results.Count());

			var result = results.Single();
			Assert.AreEqual("Test", result.PageName);
			Assert.AreEqual(5, result.VisualCount);
		}

		public class PageType
		{
			public string PageName { get; set; }
			public int VisualCount { get; set; }
		}

		[Test]
		public void GitHub_Issue_261()
		{
			var target = new Interpreter();
			target.Reference(typeof(RegexOptions));
			target.Reference(typeof(DateTimeKind));

			var result = target.Eval<RegexOptions>("~RegexOptions.None");
			Assert.AreEqual(~RegexOptions.None, result);

			// DateTimeKind doesn't have the Flags attribute: the bitwise operation returns an integer
			var result2 = target.Eval<DateTimeKind>("~DateTimeKind.Local");
			Assert.AreEqual((DateTimeKind)(-3), result2);
		}

		[Test]
		public void GitHub_Issue_262()
		{
			var list = new List<int> { 10, 30, 4 };

			var options = InterpreterOptions.Default | InterpreterOptions.LambdaExpressions;
			var interpreter = new Interpreter(options);
			interpreter.SetVariable("b", new Functions());
			interpreter.SetVariable("list", list);

			var results = interpreter.Eval<List<int>>(@"b.Add(list, (int t) => t + 10)");
			Assert.AreEqual(new List<int> { 20, 40, 14 }, results);

			// ensure that list, t are not parsed as two arguments of a lambda expression
			results = interpreter.Eval<List<int>>(@"b.Add(list, t => t + 10)");
			Assert.AreEqual(new List<int> { 20, 40, 14 }, results);
		}

		public class Functions
		{
			public List<int> Add(List<int> list, Func<int, int> transform)
			{
				return list.Select(i => transform(i)).ToList();
			}
		}

		[Test]
		[TestCase("0, null, 0, 0")]
		[TestCase("null, null, 0, 0")]
		[TestCase("new object[] { null, null, null, null }")]
		public void GitHub_Issue_263(string paramsArguments)
		{
			var interpreter = new Interpreter();
			interpreter.Reference(typeof(Utils));

			Assert.Throws<NullReferenceException>(() => interpreter.Eval<int>("Utils.ParamArrayObjects(null)"));

			var result = interpreter.Eval<int>($"Utils.ParamArrayObjects({paramsArguments})");
			Assert.AreEqual(4, result);
		}

		[Test]
		public void GitHub_Issue_276()
		{
			var interpreter = new Interpreter();

			var result = interpreter.Eval<bool>("((int?)5)>((double?)4)");
			Assert.IsTrue(result);
		}

		[Test]
		public void GitHub_Issue_287()
		{
			var interpreter = new Interpreter();
			interpreter.Reference(typeof(IEnumerable<>));

			object str = "test";
			interpreter.SetVariable("str", str, typeof(object));

			Assert.AreEqual(string.IsNullOrEmpty(str as string), interpreter.Eval("string.IsNullOrEmpty(str as string)"));
			Assert.AreEqual(str is string, interpreter.Eval("str is string"));
			Assert.AreEqual(str is int?, interpreter.Eval("str is int?"));
			Assert.AreEqual(str is int[], interpreter.Eval("(str is int[])"));
			Assert.AreEqual(str is int?[], interpreter.Eval("(str is int?[])"));
			Assert.AreEqual(str is int?[][], interpreter.Eval("(str is int?[][])"));
			Assert.AreEqual(str is IEnumerable<int>[][], interpreter.Eval("(str is IEnumerable<int>[][])"));
			Assert.AreEqual(str is IEnumerable<int?>[][], interpreter.Eval("(str is IEnumerable<int?>[][])"));
			Assert.AreEqual(str is IEnumerable<int[]>[][], interpreter.Eval("(str is IEnumerable<int[]>[][])"));
			Assert.AreEqual(str is IEnumerable<int?[][]>[][], interpreter.Eval("(str is IEnumerable<int?[][]>[][])"));
		}

		private class Npc
		{
			public int money { get; set; }
		}

		[Test]
		public void GitHub_Issue_292()
		{
			var interpreter = new Interpreter(InterpreterOptions.LambdaExpressions);

			var testnpcs = new List<Npc>();
			for (var i = 0; i < 5; i++)
				testnpcs.Add(new Npc { money = 0 });

			interpreter.Reference(typeof(GithubIssuesTestExtensionsMethods));
			interpreter.SetVariable("NearNpcs", testnpcs);

			var func = interpreter.ParseAsDelegate<Action>("NearNpcs.ActionToAll(n => n.money = 10)");
			func.Invoke();

			Assert.IsTrue(testnpcs.All(n => n.money == 10));
		}

		[Test]
		[Ignore("The fix suggested in #296 break other use cases, so let's ignore this test for now")]
		public void GitHub_Issue_295()
		{
			var evaluator = new Interpreter();

			// create path helper functions in expressions...
			Func<string, string, string> pathCombine = string.Concat;
			evaluator.SetFunction("StringConcat", pathCombine);

			// add a GlobalSettings dynamic object...
			dynamic globalSettings = new ExpandoObject();
			globalSettings.MyTestPath = "C:\\delme\\";
			evaluator.SetVariable("GlobalSettings", globalSettings);

			var works = (string)evaluator.Eval("StringConcat((string)GlobalSettings.MyTestPath,\"test.txt\")");
			Assert.That(works, Is.EqualTo("C:\\delme\\test.txt"));

			var doesntWork = (string)evaluator.Eval("StringConcat(GlobalSettings.MyTestPath,\"test.txt\")");
			Assert.That(doesntWork, Is.EqualTo("C:\\delme\\test.txt"));
		}

		#region GitHub_Issue_305

		public class _305_A
		{
			public string this[int index] => "some string";
		}

		public class _305_B : _305_A
		{
			public new int this[int index] => 25;
		}

		[Test]
		public void GitHub_Issue_305()
		{
			var b = new _305_B();

			var interpreter = new Interpreter();
			var lambda = interpreter.Parse("this[0]", new Parameter("this", b));
			var res = lambda.Invoke(b);

			Assert.AreEqual(25, res);
		}

		#endregion

		[Test]
		public void GitHub_Issue_311()
		{
			var a = "AABB";

			var interpreter1 = new Interpreter();
			interpreter1.SetVariable("a", a);
			Assert.AreEqual("AA", interpreter1.Eval("a.Substring(0, 2)"));

			var interpreter2 = new Interpreter().SetDefaultNumberType(DefaultNumberType.Decimal);
			interpreter2.SetVariable("a", a);
			// expected to throw because Substring is not defined for decimal
			Assert.Throws<NoApplicableMethodException>(() => interpreter2.Eval("a.Substring(0, 2)"));
			// It works if we cast to int
			Assert.AreEqual("AA", interpreter2.Eval("a.Substring((int)0, (int)2)"));
		}

		[Test]
		public void GitHub_Issue_314()
		{
			var interpreter = new Interpreter();

			var exception1 = Assert.Throws<UnknownIdentifierException>(() => interpreter.Eval("b < 1"));
			Assert.AreEqual("b", exception1.Identifier);

			var exception2 = Assert.Throws<UnknownIdentifierException>(() => interpreter.Eval("b > 1"));
			Assert.AreEqual("b", exception2.Identifier);
		}

		[Test]
		public void GitHub_Issue_325()
		{
			var options = InterpreterOptions.Default | InterpreterOptions.LateBindObject;
			var interpreter = new Interpreter(options);

			var input = new
			{
				Prop1 = 4,
			};

			var expressionDelegate = interpreter.ParseAsDelegate<Func<object, bool>>($"input.Prop1 == null", "input");
			Assert.IsFalse(expressionDelegate(input));
		}
	}

	internal static class GithubIssuesTestExtensionsMethods
	{
		public static bool IsInFuture(this DateTimeOffset date) => date > DateTimeOffset.UtcNow;
		public static void ActionToAll<T>(this IEnumerable<T> source, Action<T> action)
		{
			foreach (var item in source)
				action(item);
		}
	}
}
