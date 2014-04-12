using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections;

namespace DynamicExpresso.UnitTest
{
	[TestClass]
	public class AnalysisTest
	{
		[TestMethod]
		public void Analyze_a_valid_expression()
		{
			var target = new Interpreter()
									.SetVariable("x", 10.0);

			var analysis = target.Analyze("x + y * Math.Pow(x, 2)", typeof(void), new Parameter("y", 5.0));

			Assert.IsTrue(analysis.Success);
			Assert.IsNull(analysis.Exception);
			Assert.AreEqual(typeof(double), analysis.ReturnType);
		}

		[TestMethod]
		public void Analyze_an_invalid_expression_unknown_identifier_x()
		{
			var target = new Interpreter();

			var analysis = target.Analyze("x + y * Math.Pow(x, 2)", typeof(void));

			Assert.IsFalse(analysis.Success);
			Assert.AreEqual("Unknown identifier 'x' (at index 0).", analysis.Exception.Message);
			Assert.IsNull(analysis.ReturnType);
			Assert.IsInstanceOfType(analysis.Exception, typeof(UnknownIdentifierException));
			var exception = (UnknownIdentifierException)analysis.Exception;
			Assert.AreEqual("x", exception.Identifier);
			Assert.AreEqual(0, exception.Position);
		}

		[TestMethod]
		public void Analyze_an_invalid_expression_unknown_identifier_y()
		{
			var target = new Interpreter()
									.SetVariable("x", 10.0);

			var analysis = target.Analyze("x + y * Math.Pow(x, 2)", typeof(void));

			Assert.IsFalse(analysis.Success);
			Assert.IsNull(analysis.ReturnType);
			Assert.IsInstanceOfType(analysis.Exception, typeof(UnknownIdentifierException));
			var exception = (UnknownIdentifierException)analysis.Exception;
			Assert.AreEqual("y", exception.Identifier);
			Assert.AreEqual(4, exception.Position);
		}

		[TestMethod]
		public void Analyze_an_invalid_expression_unknown_method()
		{
			var target = new Interpreter()
									.SetVariable("x", 10.0);

			var analysis = target.Analyze("Math.NotExistingMethod(x, 2)", typeof(void));

			Assert.IsFalse(analysis.Success);
			Assert.IsNull(analysis.ReturnType);
			Assert.IsInstanceOfType(analysis.Exception, typeof(NoApplicableMethodException));
			var exception = (NoApplicableMethodException)analysis.Exception;
			Assert.AreEqual("NotExistingMethod", exception.MethodName);
			Assert.AreEqual("Math", exception.MethodTypeName);
			Assert.AreEqual(5, exception.Position);
		}
	}
}
