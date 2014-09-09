using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace DynamicExpresso.UnitTest
{
	[TestClass]
	public class InvalidExpressionTest
	{
		[TestMethod]
		[ExpectedException(typeof(UnknownIdentifierException))]
		public void Not_existing_variable()
		{
			var target = new Interpreter();

			target.Eval("not_existing");
		}

		[TestMethod]
		[ExpectedException(typeof(UnknownIdentifierException))]
		public void Not_existing_function()
		{
			var target = new Interpreter();

			target.Eval("pippo()");
		}

		[TestMethod]
		[ExpectedException(typeof(ParseException))]
		public void Not_valid_function()
		{
			var target = new Interpreter();

			target.Eval("2()");
		}

		[TestMethod]
		[ExpectedException(typeof(UnknownIdentifierException))]
		public void Not_valid_expression()
		{
			var target = new Interpreter();

			target.Eval("'5' + 3 /(asda");
		}

		[TestMethod]
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

		[TestMethod]
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

		[TestMethod]
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
		[TestMethod]
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
		[TestMethod]
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
		[TestMethod]
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
