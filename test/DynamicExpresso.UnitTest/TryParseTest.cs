using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections;

namespace DynamicExpresso.UnitTest
{
	[TestClass]
	public class TryParseTest
	{
		[TestMethod]
		public void TryParse_a_valid_expression()
		{
			var target = new Interpreter()
									.SetVariable("x", 10.0);

			var analysis = target.TryParse("x + y * Math.Pow(x, 2)", typeof(void), new Parameter("y", 5.0));

			Assert.IsTrue(analysis.Success);
			Assert.IsNull(analysis.Exception);
			Assert.AreEqual(typeof(double), analysis.Lambda.ReturnType);
		}

		[TestMethod]
		public void TryParse_an_invalid_expression_unknown_identifier_x()
		{
			var target = new Interpreter();

			var analysis = target.TryParse("x + y * Math.Pow(x, 2)", typeof(void));

			Assert.IsFalse(analysis.Success);
			Assert.AreEqual("Unknown identifier 'x' (at index 0).", analysis.Exception.Message);
			Assert.IsNull(analysis.Lambda);
			Assert.IsInstanceOfType(analysis.Exception, typeof(UnknownIdentifierException));
			var exception = (UnknownIdentifierException)analysis.Exception;
			Assert.AreEqual("x", exception.Identifier);
			Assert.AreEqual(0, exception.Position);
		}

		[TestMethod]
		public void TryParse_an_invalid_expression_unknown_identifier_y()
		{
			var target = new Interpreter()
									.SetVariable("x", 10.0);

			var analysis = target.TryParse("x + y * Math.Pow(x, 2)", typeof(void));

			Assert.IsFalse(analysis.Success);
			Assert.IsNull(analysis.Lambda);
			Assert.IsInstanceOfType(analysis.Exception, typeof(UnknownIdentifierException));
			var exception = (UnknownIdentifierException)analysis.Exception;
			Assert.AreEqual("y", exception.Identifier);
			Assert.AreEqual(4, exception.Position);
		}

		[TestMethod]
		public void TryParse_an_invalid_expression_unknown_method()
		{
			var target = new Interpreter()
									.SetVariable("x", 10.0);

			var analysis = target.TryParse("Math.NotExistingMethod(x, 2)", typeof(void));

			Assert.IsFalse(analysis.Success);
			Assert.IsNull(analysis.Lambda);
			Assert.IsInstanceOfType(analysis.Exception, typeof(NoApplicableMethodException));
			var exception = (NoApplicableMethodException)analysis.Exception;
			Assert.AreEqual("NotExistingMethod", exception.MethodName);
			Assert.AreEqual("Math", exception.MethodTypeName);
			Assert.AreEqual(5, exception.Position);
		}
	}
}
