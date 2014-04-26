using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DynamicExpresso.UnitTest
{
	[TestClass]
	public class ExceptionTest
	{
		[TestMethod]
		[ExpectedException(typeof(UnknownIdentifierException))]
		public void Unknown_Keyword_Is_Not_Supported()
		{
			var target = new Interpreter();

			target.Parse("unkkeyword");
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
