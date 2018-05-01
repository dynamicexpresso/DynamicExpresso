using System;
using NUnit.Framework;
using System.Linq.Expressions;

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

	}
}
