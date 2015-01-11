using System;
using System.Linq;
using NUnit.Framework;
using System.Collections.Generic;

namespace DynamicExpresso.UnitTest
{
	[TestFixture]
	public class ExtensionsMethodsTest
	{
		[Test]
		public void Invoke_extension_method()
		{
			var x = new MyClass();

			var target = new Interpreter()
									.Reference(typeof(TestExtensionsMethods))
									.SetVariable("x", x);

			Assert.AreEqual(x.HelloWorld(), target.Eval("x.HelloWorld()"));
			Assert.AreEqual(x.HelloWorldWithParam(DateTime.Now), target.Eval("x.HelloWorldWithParam(DateTime.Now)"));
		}

		[Test]
		public void Invoke_generic_extension_method()
		{
			var x = new MyClass();

			var target = new Interpreter()
									.Reference(typeof(TestExtensionsMethods))
									.SetVariable("x", x);

			Assert.AreEqual(x.GenericHello(), target.Eval("x.GenericHello()"));
		}

		[Test]
		public void Invoke_generic_parameter_extension_method()
		{
			var x = new MyClass[0];

			var target = new Interpreter()
									.Reference(typeof(TestExtensionsMethods))
									.SetVariable("x", x);

			Assert.AreEqual(x.GenericParamHello(), target.Eval("x.GenericParamHello()"));
		}

		[Test]
		public void Invoke_generic_with_2_parameters_and_output_extension_method()
		{
			var x = new Dictionary<string, MyClass>();
			x.Add("i1", new MyClass());

			var target = new Interpreter()
									.Reference(typeof(TestExtensionsMethods))
									.SetVariable("x", x);

			Assert.AreEqual(x.GenericWith2Params(), target.Eval("x.GenericWith2Params()"));
		}

		[Test]
		public void Invoke_generic_mixed_parameter_extension_method()
		{
			var x = new Dictionary<string, MyClass>();

			var target = new Interpreter()
									.Reference(typeof(TestExtensionsMethods))
									.SetVariable("x", x);

			Assert.AreEqual(x.GenericMixedParamHello(), target.Eval("x.GenericMixedParamHello()"));
		}

		public class MyClass
		{
		}
	}

	public static class TestExtensionsMethods
	{
		public static string HelloWorld(this ExtensionsMethodsTest.MyClass test)
		{
			return "Hello Test Class";
		}

		public static string HelloWorldWithParam(this ExtensionsMethodsTest.MyClass test, DateTime date)
		{
			return "Hello Test Class " + date.Year;
		}

		public static string GenericHello<T>(this T test)
			where T : ExtensionsMethodsTest.MyClass
		{
			return "Hello with generic!";
		}

		public static string GenericParamHello<T>(this IEnumerable<T> test)
			where T : ExtensionsMethodsTest.MyClass
		{
			return "Hello with generic param!";
		}

		public static string GenericMixedParamHello<T>(this IDictionary<string, T> test)
			where T : ExtensionsMethodsTest.MyClass
		{
			return "Hello with generic param!";
		}

		public static T2 GenericWith2Params<T1, T2>(this IDictionary<T1, T2> test)
				where T2 : ExtensionsMethodsTest.MyClass
		{
			return test.First().Value;
		}
	}
}
