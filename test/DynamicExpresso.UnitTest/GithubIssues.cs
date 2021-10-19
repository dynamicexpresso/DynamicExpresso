using DynamicExpresso.Exceptions;
using NUnit.Framework;
using System;
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
		public void GitHub_Issue_169()
		{
			var interpreter = new Interpreter();

			var obj = new
			{
				value = (int?) 10,
				array = new int?[]{ 10 },
				nullArray = (int?[]) null,
			};

			interpreter.SetVariable("obj", obj);
			Assert.AreEqual(obj.value, interpreter.Eval("obj?.value"));
			Assert.AreEqual(obj.array[0], interpreter.Eval("obj?.array?[0]"));
			Assert.IsNull(interpreter.Eval("obj?.nullArray?[0]"));

			obj = null;
			interpreter.SetVariable("obj", obj);
			Assert.IsNull(interpreter.Eval("obj?.value"));
			Assert.IsNull(interpreter.Eval("obj?.array?[0]"));
			Assert.IsNull(interpreter.Eval("obj?.nullArray?[0]"));
		}
	}
}
