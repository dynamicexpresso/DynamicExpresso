using System;
using NUnit.Framework;
using System.Linq.Expressions;
using DynamicExpresso.Exceptions;
using System.Linq;
using System.Reflection;

namespace DynamicExpresso.UnitTest
{
	[TestFixture]
	public class VariablesTest
	{
		[Test]
		public void Cannot_create_variables_with_reserved_keywords()
		{
			var target = new Interpreter();

			foreach (var keyword in LanguageConstants.ReservedKeywords)
				Assert.Throws<InvalidOperationException>(() => target.SetVariable(keyword, 1));
		}

		[Test]
		public void Can_create_variables_that_override_known_types()
		{
			// Note that in C# some of these keywords are not permitted, like `bool`,
			// but other , like `Boolean` is permitted. (c# keywords are not permitted, .NET types yes)
			// In my case are all considered in the same way, so now are permitted.
			// But in real case scenarios try to use variables names with a more ubiquitous name to reduce possible conflict for the future.

			var target = new Interpreter()
				.SetVariable("bool", 1) // this in c# is not permitted
				.SetVariable("Int32", 2)
				.SetVariable("Math", 3);

			Assert.AreEqual(1, target.Eval("bool"));
			Assert.AreEqual(2, target.Eval("Int32"));
			Assert.AreEqual(3, target.Eval("Math"));
		}

		[Test]
		public void Assign_and_use_variables()
		{
			var target = new Interpreter()
											.SetVariable("myk", 23);

			Assert.AreEqual(23, target.Eval("myk"));
			Assert.AreEqual(typeof(int), target.Parse("myk").ReturnType);
		}

		[Test]
		public void Variables_by_default_are_case_sensitive()
		{
			var target = new Interpreter()
											.SetVariable("x", 23)
											.SetVariable("X", 50);

			Assert.AreEqual(23, target.Eval("x"));
			Assert.AreEqual(50, target.Eval("X"));
		}

		[Test]
		public void Variables_can_be_case_insensitive()
		{
			var target = new Interpreter(InterpreterOptions.DefaultCaseInsensitive)
											.SetVariable("x", 23);

			Assert.AreEqual(23, target.Eval("x"));
			Assert.AreEqual(23, target.Eval("X"));
		}

		[Test]
		public void Variables_can_be_overwritten()
		{
			var target = new Interpreter()
											.SetVariable("myk", 23);

			Assert.AreEqual(23, target.Eval("myk"));

			target.SetVariable("myk", 3489);

			Assert.AreEqual(3489, target.Eval("myk"));

			Assert.AreEqual(typeof(int), target.Parse("myk").ReturnType);
		}

		[Test]
		public void Variables_can_be_overwritten_in_a_case_insensitive_setting()
		{
			var target = new Interpreter(InterpreterOptions.DefaultCaseInsensitive)
											.SetVariable("myk", 23);

			Assert.AreEqual(23, target.Eval("myk"));

			target.SetVariable("MYK", 3489);

			Assert.AreEqual(3489, target.Eval("myk"));

			Assert.AreEqual(typeof(int), target.Parse("myk").ReturnType);
		}

		[Test]
		public void Null_Variables()
		{
			var target = new Interpreter()
											.SetVariable("myk", null);

			Assert.AreEqual(null, target.Eval("myk"));
			Assert.AreEqual(true, target.Eval("myk == null"));
			Assert.AreEqual(typeof(object), target.Parse("myk").ReturnType);
		}

		[Test]
		public void Null_Variables_With_Type_Specified()
		{
			var target = new Interpreter()
											.SetVariable("myk", null, typeof(string));

			Assert.AreEqual(null, target.Eval("myk"));
			Assert.AreEqual(true, target.Eval("myk == null"));
			Assert.AreEqual(typeof(string), target.Parse("myk").ReturnType);
		}

		[Test]
		public void Keywords_with_lambda()
		{
			Expression<Func<double, double, double>> pow = (x, y) => Math.Pow(x, y);
			var target = new Interpreter()
									.SetExpression("pow", pow);

			Assert.AreEqual(9.0, target.Eval("pow(3, 2)"));
		}

		[Test]
		public void Keywords_with_delegate()
		{
			Func<double, double, double> pow = (x, y) => Math.Pow(x, y);
			var target = new Interpreter()
									.SetFunction("pow", pow);

			Assert.AreEqual(9.0, target.Eval("pow(3, 2)"));
		}

		[Test]
		public void Keywords_with_invalid_delegate_call()
		{
			Func<double, double, double> pow = (x, y) => Math.Pow(x, y);
			var target = new Interpreter()
									.SetFunction("pow", pow);

			Assert.Throws<ParseException>(() => target.Eval("pow(3)"));
		}

		[Test]
		public void Keywords_with_overloaded_delegates()
		{
			Func<decimal, decimal> roundFunction1 = (userNumber) => Math.Round(userNumber);
			Func<decimal, int, decimal> roundFunction2 = (userNumber, decimals) => Math.Round(userNumber, decimals);

			var interpreter = new Interpreter();
			interpreter.SetFunction("ROUND", roundFunction1);
			interpreter.SetFunction("ROUND", roundFunction2);

			Assert.AreEqual(3.13M, interpreter.Eval("ROUND(3.12789M, 2)"));
			Assert.AreEqual(3M, interpreter.Eval("ROUND(3.12789M)"));
		}

		[Test]
		public void Keywords_with_ambiguous_delegates()
		{
			Func<string, string> ambiguous1 = (val) => val;
			Func<int?, string> ambiguous2 = (val) => "integer";

			var interpreter = new Interpreter();
			interpreter.SetFunction("MyFunc", ambiguous1);
			interpreter.SetFunction("MyFunc", ambiguous2);

			// ambiguous call: null can either be a string or an object
			// note: if there's no ambiguous exception, it means that the resolution
			// lifted the parameters from the string overload, which prevented the int? overload 
			// from being considered
			Assert.Throws<ParseException>(() => interpreter.Eval("MyFunc(null)"));

			// call resolved to the string overload
			Assert.AreEqual("test", interpreter.Eval("MyFunc(\"test\")"));
		}

		[Test]
		public void Keywords_with_non_ambiguous_delegates()
		{
			Func<double, string> ambiguous1 = (val) => "double";
			Func<int, string> ambiguous2 = (val) => "integer";

			var interpreter = new Interpreter();
			interpreter.SetFunction("MyFunc", ambiguous1);
			interpreter.SetFunction("MyFunc", ambiguous2);

			// there should be no ambiguous exception: int can implicitly be converted to double, 
			// but there's a perfect match 
			Assert.AreEqual("integer", interpreter.Eval("MyFunc(5)"));
		}

		[Test]
		public void Set_function_With_Object_Params()
		{
			var target = new Interpreter();

			// import static method with params array 
			var methodInfo = typeof(VariablesTest).GetMethod("Sum", BindingFlags.Static | BindingFlags.NonPublic);
			var types = methodInfo.GetParameters().Select(p => p.ParameterType).Concat(new[] { methodInfo.ReturnType });
			var del = methodInfo.CreateDelegate(Expression.GetDelegateType(types.ToArray()));
			target.SetFunction(methodInfo.Name, del);

			// the imported Sum function can be called with any parameters
			Assert.AreEqual(6, target.Eval<int>("Sum(1, 2, 3)"));
		}

		internal static int Sum(params int[] integers)
		{
			return integers.Sum();
		}
	}
}
