using NUnit.Framework;
using System.Linq.Expressions;

namespace DynamicExpresso.UnitTest
{
	[TestFixture]
	public class WorkingContextTest
	{
		[Test]
		public void Simulate_a_working_context_using_this_keyword()
		{
			var workingContext = new { FirstName = "homer" };

			var interpreter = new Interpreter();
			interpreter.SetVariable("this", workingContext);

			Assert.That(interpreter.Eval("this.FirstName"), Is.EqualTo(workingContext.FirstName));
		}

		[Test]
		public void Injection_a_property_expresion_to_simulate_a_working_context_parsing_another_property()
		{
			var workingContext = new { FirstName = "homer" };

			var interpreter = new Interpreter();
			interpreter.SetVariable("this", workingContext);
			var firstNameExpression = interpreter.Parse("this.FirstName").Expression;
			interpreter.SetExpression("FirstName", firstNameExpression);

			Assert.That(interpreter.Eval("FirstName"), Is.EqualTo(workingContext.FirstName));
		}

		[Test]
		public void Injection_a_property_expresion_to_simulate_a_working_context()
		{
			var workingContext = new { FirstName = "homer" };

			var workingContextExpression = Expression.Constant(workingContext);
			var firstNameExpression = Expression.Property(workingContextExpression, "FirstName");

			var interpreter = new Interpreter();
			interpreter.SetExpression("FirstName", firstNameExpression);

			Assert.That(interpreter.Eval("FirstName"), Is.EqualTo(workingContext.FirstName));
		}
	}
}
