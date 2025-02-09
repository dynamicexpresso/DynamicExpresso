using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DynamicExpresso.Exceptions;
using NUnit.Framework;

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

			Assert.That(target.Eval("bool"), Is.EqualTo(1));
			Assert.That(target.Eval("Int32"), Is.EqualTo(2));
			Assert.That(target.Eval("Math"), Is.EqualTo(3));
		}

		[Test]
		public void Assign_and_use_variables()
		{
			var target = new Interpreter()
											.SetVariable("myk", 23);

			Assert.That(target.Eval("myk"), Is.EqualTo(23));
			Assert.That(target.Parse("myk").ReturnType, Is.EqualTo(typeof(int)));
		}

		[Test]
		public void Variables_by_default_are_case_sensitive()
		{
			var target = new Interpreter()
											.SetVariable("x", 23)
											.SetVariable("X", 50);

			Assert.That(target.Eval("x"), Is.EqualTo(23));
			Assert.That(target.Eval("X"), Is.EqualTo(50));
		}

		[Test]
		public void Variables_can_be_case_insensitive()
		{
			var target = new Interpreter(InterpreterOptions.DefaultCaseInsensitive)
											.SetVariable("x", 23);

			Assert.That(target.Eval("x"), Is.EqualTo(23));
			Assert.That(target.Eval("X"), Is.EqualTo(23));
		}

		[Test]
		public void Variables_can_be_overwritten()
		{
			var target = new Interpreter()
											.SetVariable("myk", 23);

			Assert.That(target.Eval("myk"), Is.EqualTo(23));

			target.SetVariable("myk", 3489);

			Assert.That(target.Eval("myk"), Is.EqualTo(3489));

			Assert.That(target.Parse("myk").ReturnType, Is.EqualTo(typeof(int)));
		}

		[Test]
		public void Variables_can_be_overwritten_in_a_case_insensitive_setting()
		{
			var target = new Interpreter(InterpreterOptions.DefaultCaseInsensitive)
											.SetVariable("myk", 23);

			Assert.That(target.Eval("myk"), Is.EqualTo(23));

			target.SetVariable("MYK", 3489);

			Assert.That(target.Eval("myk"), Is.EqualTo(3489));

			Assert.That(target.Parse("myk").ReturnType, Is.EqualTo(typeof(int)));
		}

		[Test]
		public void Null_Variables()
		{
			var target = new Interpreter()
											.SetVariable("myk", null);

			Assert.That(target.Eval("myk"), Is.EqualTo(null));
			Assert.That(target.Eval("myk == null"), Is.EqualTo(true));
			Assert.That(target.Parse("myk").ReturnType, Is.EqualTo(typeof(object)));
		}

		[Test]
		public void Null_Variables_With_Type_Specified()
		{
			var target = new Interpreter()
											.SetVariable("myk", null, typeof(string));

			Assert.That(target.Eval("myk"), Is.EqualTo(null));
			Assert.That(target.Eval("myk == null"), Is.EqualTo(true));
			Assert.That(target.Parse("myk").ReturnType, Is.EqualTo(typeof(string)));
		}

		[Test]
		public void Keywords_with_lambda()
		{
			Expression<Func<double, double, double>> pow = (x, y) => Math.Pow(x, y);
			var target = new Interpreter()
									.SetExpression("pow", pow);

			Assert.That(target.Eval("pow(3, 2)"), Is.EqualTo(9.0));
		}

		[Test]
		public void Keywords_with_delegate()
		{
			Func<double, double, double> pow = (x, y) => Math.Pow(x, y);
			var target = new Interpreter()
									.SetFunction("pow", pow);

			Assert.That(target.Eval("pow(3, 2)"), Is.EqualTo(9.0));
		}

		[Test]
		public void Keywords_with_same_overload_twice()
		{
			Func<double, double, double> pow = (x, y) => Math.Pow(x, y);
			var target = new Interpreter()
									.SetFunction("pow", pow)
									.SetFunction("pow", pow);

			Assert.That(target.Eval("pow(3, 2)"), Is.EqualTo(9.0));
		}

		[Test]
		public void Replace_same_overload_signature()
		{
			Func<double, int> f1 = d => 1;
			Func<double, int> f2 = d => 2;

			var target = new Interpreter()
									.SetFunction("f", f1)
									.SetFunction("f", f2);

			// f2 should override the f1 registration, because both delegates have the same signature
			Assert.That(target.Eval("f(0d)"), Is.EqualTo(2));
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

			Assert.That(interpreter.Eval("ROUND(3.12789M, 2)"), Is.EqualTo(3.13M));
			Assert.That(interpreter.Eval("ROUND(3.12789M)"), Is.EqualTo(3M));
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
			Assert.That(interpreter.Eval("MyFunc(\"test\")"), Is.EqualTo("test"));
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
			Assert.That(interpreter.Eval("MyFunc(5)"), Is.EqualTo("integer"));
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

			Assert.That(del.Method.GetParameters()[0].GetCustomAttribute<ParamArrayAttribute>(), Is.Not.Null);

			var flags = BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance;
			var invokeMethod = (MethodInfo)(del.GetType().FindMembers(MemberTypes.Method, flags, Type.FilterName, "Invoke")[0]);
			Assert.That(invokeMethod.GetParameters()[0].GetCustomAttribute<ParamArrayAttribute>(), Is.Null); // should be not null!

			// the imported Sum function can be called with any parameters
			Assert.That(target.Eval<int>("Sum(1, 2, 3)"), Is.EqualTo(6));
		}

		internal static int Sum(params int[] integers)
		{
			return integers.Sum();
		}
	}
}
