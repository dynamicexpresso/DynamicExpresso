using System;
using NUnit.Framework;
using System.Linq.Expressions;

namespace DynamicExpresso.UnitTest
{
	[TestFixture]
	public class VariablesTest
	{
		[Test]
		public void Variables()
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
