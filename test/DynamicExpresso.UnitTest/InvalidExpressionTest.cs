using System;
using DynamicExpresso.Exceptions;
using NUnit.Framework;

namespace DynamicExpresso.UnitTest
{
	[TestFixture]
	public class InvalidExpressionTest
	{
		[Test]
		public void Not_existing_variable()
		{
			var target = new Interpreter();

			Assert.Throws<UnknownIdentifierException>(() => target.Eval("not_existing"));
		}

		[Test]
		public void Invalid_equal_assignment_operator_left()
		{
			var target = new Interpreter();

			Assert.Throws<ParseException>(() => target.Eval("=234"));
		}

		[Test]
		public void Invalid_equal_assignment_operator_left_is_literal()
		{
			var target = new Interpreter();

			Assert.Throws<ParseException>(() => target.Eval("352=234"));
		}

		[Test]
		public void Unkonwn_operator_triple_equal()
		{
			var target = new Interpreter();

			Assert.Throws<ParseException>(() => target.Eval("352===234"));
		}

		[Test]
		public void Not_existing_function()
		{
			var target = new Interpreter();

			Assert.Throws<UnknownIdentifierException>(() => target.Eval("pippo()"));
		}

		[Test]
		public void Not_valid_function()
		{
			var target = new Interpreter();

			
			Assert.Throws<ParseException>(() => target.Eval("2()"));
		}

		[Test]
		public void Not_valid_expression()
		{
			var target = new Interpreter();

			Assert.Throws<UnknownIdentifierException>(() => target.Eval("'5' + 3 /(asda"));
		}

		[Test]
		public void TryParse_an_invalid_expression_unknown_identifier_x()
		{
			var target = new Interpreter();

			var ex = Assert.Throws<UnknownIdentifierException>(() => target.Parse("x + y * Math.Pow(x, 2)", typeof(void)));

			Assert.AreEqual("Unknown identifier 'x' (at index 0).", ex.Message);
			Assert.AreEqual("x", ex.Identifier);
			Assert.AreEqual(0, ex.Position);
		}

		[Test]
		public void Parse_an_invalid_expression_unknown_identifier_y()
		{
			var target = new Interpreter()
									.SetVariable("x", 10.0);

			var ex = Assert.Throws<UnknownIdentifierException>(() => target.Parse("x + y * Math.Pow(x, 2)", typeof(void)));

			Assert.AreEqual("y", ex.Identifier);
			Assert.AreEqual(4, ex.Position);
		}

		[Test]
		public void Parse_an_invalid_expression_unknown_method()
		{
			var target = new Interpreter()
									.SetVariable("x", 10.0);

			var ex = Assert.Throws<NoApplicableMethodException>(() => target.Parse("Math.NotExistingMethod(x, 2)", typeof(void)));

			Assert.AreEqual("NotExistingMethod", ex.MethodName);
			Assert.AreEqual("Math", ex.MethodTypeName);
			Assert.AreEqual(5, ex.Position);
		}

		[Test]
		public void SystemExceptions_are_preserved_using_delegate_variable()
		{
			var target = new Interpreter();

			Func<string> testException = () =>
			{
				throw new InvalidOperationException("Test");
			};

			target.SetVariable("testException", testException);

			Assert.Throws<InvalidOperationException>(() => target.Eval("testException()"));
		}

		[Test]
		public void CustomExceptions_WithoutSerializationConstructor_are_preserved()
		{
			var target = new Interpreter();

			Func<string> testException = () =>
			{
				throw new MyException("Test");
			};

			target.SetVariable("testException", testException);

			Assert.Throws<MyException>(() => target.Eval("testException()"));
		}

		[Test]
		public void SystemExceptions_are_preserved_using_method_invocation()
		{
			var target = new Interpreter();
			target.SetVariable("a", new MyTestService());

			Assert.Throws<NotImplementedException>(() => target.Eval("a.ThrowException()"));
		}

		public class MyException : Exception
		{
			public MyException(string message) : base(message) { }
		}

		// ReSharper disable once UnusedMember.Local
		private class MyTestService
		{
			// ReSharper disable once UnusedMember.Local
			public string ThrowException()
			{
				throw new NotImplementedException("AppException");
			}
		}
	}
}
