using System;
using NUnit.Framework;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace DynamicExpresso.UnitTest
{
	[TestFixture]
	public class InvalidExpressionTest
	{
		[Test]
		[ExpectedException(typeof(UnknownIdentifierException))]
		public void Not_existing_variable()
		{
			var target = new Interpreter();

			target.Eval("not_existing");
		}

		[Test]
		[ExpectedException(typeof(ParseException))]
		public void Invalid_equal_assignment_operator_left()
		{
			var target = new Interpreter();

			target.Eval("=234");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void Invalid_equal_assignment_operator_left_is_literal()
		{
			var target = new Interpreter();

			target.Eval("352=234");
		}

		[Test]
		[ExpectedException(typeof(ParseException))]
		public void Unkonwn_operator_triple_equal()
		{
			var target = new Interpreter();

			target.Eval("352===234");
		}

		[Test]
		[ExpectedException(typeof(UnknownIdentifierException))]
		public void Not_existing_function()
		{
			var target = new Interpreter();

			target.Eval("pippo()");
		}

		[Test]
		[ExpectedException(typeof(ParseException))]
		public void Not_valid_function()
		{
			var target = new Interpreter();

			target.Eval("2()");
		}

		[Test]
		[ExpectedException(typeof(UnknownIdentifierException))]
		public void Not_valid_expression()
		{
			var target = new Interpreter();

			target.Eval("'5' + 3 /(asda");
		}

		[Test]
		[ExpectedException(typeof(UnknownIdentifierException))]
		public void TryParse_an_invalid_expression_unknown_identifier_x()
		{
			var target = new Interpreter();

			try
			{
				target.Parse("x + y * Math.Pow(x, 2)", typeof(void));
			}
			catch(UnknownIdentifierException ex)
			{
				Assert.AreEqual("Unknown identifier 'x' (at index 0).", ex.Message);
				Assert.AreEqual("x", ex.Identifier);
				Assert.AreEqual(0, ex.Position);
				throw;
			}
		}

		[Test]
		[ExpectedException(typeof(UnknownIdentifierException))]
		public void Parse_an_invalid_expression_unknown_identifier_y()
		{
			var target = new Interpreter()
									.SetVariable("x", 10.0);

			try
			{
				target.Parse("x + y * Math.Pow(x, 2)", typeof(void));
			}
			catch (UnknownIdentifierException ex)
			{
				Assert.AreEqual("y", ex.Identifier);
				Assert.AreEqual(4, ex.Position);
				throw;
			}
		}

		[Test]
		[ExpectedException(typeof(NoApplicableMethodException))]
		public void Parse_an_invalid_expression_unknown_method()
		{
			var target = new Interpreter()
									.SetVariable("x", 10.0);

			try
			{
				target.Parse("Math.NotExistingMethod(x, 2)", typeof(void));
			}
			catch (NoApplicableMethodException ex)
			{
				Assert.AreEqual("NotExistingMethod", ex.MethodName);
				Assert.AreEqual("Math", ex.MethodTypeName);
				Assert.AreEqual(5, ex.Position);
				throw;
			}
		}

		[ExpectedException(typeof(InvalidOperationException))]
		[Test]
		public void SystemExceptions_are_preserved_using_delegate_variable()
		{
			var target = new Interpreter();

			Func<string> testException = new Func<string>(() =>
			{
				throw new InvalidOperationException("Test");
			});

			target.SetVariable("testException", testException);

			target.Eval("testException()");
		}

		[ExpectedException(typeof(MyException))]
		[Test]
		public void CustomExceptions_WithoutSerializationConstructor_are_preserved()
		{
			var target = new Interpreter();

			Func<string> testException = new Func<string>(() =>
			{
				throw new MyException("Test");
			});

			target.SetVariable("testException", testException);

			target.Eval("testException()");
		}

		[ExpectedException(typeof(NotImplementedException))]
		[Test]
		public void SystemExceptions_are_preserved_using_method_invocation()
		{
			var target = new Interpreter();
			target.SetVariable("a", new MyTestService());

			target.Eval("a.ThrowException()");
		}

		public class MyException : Exception
		{
			public MyException(string message) : base(message) { }
		}

		class MyTestService
		{
			public string ThrowException()
			{
				throw new NotImplementedException("AppException");
			}
		}
	}
}
