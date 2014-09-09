using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace DynamicExpresso.UnitTest
{
	[TestClass]
	public class WorkingContextTest
	{
		[TestMethod]
		public void Simulate_a_working_context_using_this_keyword()
		{
			var workingContext = new { FirstName = "homer" };

			var interpreter = new Interpreter();
			interpreter.SetVariable("this", workingContext);

			Assert.AreEqual(workingContext.FirstName, interpreter.Eval("this.FirstName"));
		}

		[TestMethod]
		public void Injection_a_property_expresion_to_simulate_a_working_context_parsing_another_property()
		{
			var workingContext = new { FirstName = "homer" };

			var interpreter = new Interpreter();
			interpreter.SetVariable("this", workingContext);
			var firstNameExpression = interpreter.Parse("this.FirstName").Expression;
			interpreter.SetExpression("FirstName", firstNameExpression);

			Assert.AreEqual(workingContext.FirstName, interpreter.Eval("FirstName"));
		}

		[TestMethod]
		public void Injection_a_property_expresion_to_simulate_a_working_context()
		{
			var workingContext = new { FirstName = "homer" };

			var workingContextExpression = Expression.Constant(workingContext);
			var firstNameExpression = Expression.Property(workingContextExpression, "FirstName");

			var interpreter = new Interpreter();
			interpreter.SetExpression("FirstName", firstNameExpression);

			Assert.AreEqual(workingContext.FirstName, interpreter.Eval("FirstName"));
		}
	}
}
