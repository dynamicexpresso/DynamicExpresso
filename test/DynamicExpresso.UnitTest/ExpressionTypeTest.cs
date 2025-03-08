using System;
using NUnit.Framework;

namespace DynamicExpresso.UnitTest
{
	[TestFixture]
	public class ExpressionTypeTest
	{
		[Test]
		public void If_no_expression_type_is_specified_the_return_type_is_inferred()
		{
			var target = new Interpreter();

			Assert.That(target.Parse("\"ciao\"").ReturnType, Is.EqualTo(typeof(string)));
			Assert.That(target.Parse("45").ReturnType, Is.EqualTo(typeof(int)));
			Assert.That(target.Parse("45.4").ReturnType, Is.EqualTo(typeof(double)));
			Assert.That(target.Parse("null").ReturnType, Is.EqualTo(typeof(object)));
		}

		[Test]
		public void If_expression_type_doesn_t_match_a_conversion_is_performed_when_possible()
		{
			var target = new Interpreter();
			var expressionType = typeof(double);

			var lambda = target.Parse("213", expressionType);

			Assert.That(lambda.ReturnType, Is.EqualTo(expressionType));
			Assert.That(lambda.Invoke(), Is.EqualTo((double)213));
		}

		[Test]
		public void If_expression_type_doesn_t_match_a_conversion_is_performed_eventually_loosing_precision()
		{
			var target = new Interpreter();
			var expressionType = typeof(int);

			var lambda = target.Parse("213.46", expressionType);

			Assert.That(lambda.ReturnType, Is.EqualTo(expressionType));
			Assert.That(lambda.Invoke(), Is.EqualTo((int)213.46));
		}

		[Test]
		public void Can_convert_a_null_expression_to_any_reference_type()
		{
			var target = new Interpreter();

			var lambda = target.Parse("null", typeof(string));
			Assert.That(lambda.ReturnType, Is.EqualTo(typeof(string)));
			Assert.That(lambda.Invoke(), Is.Null);

			lambda = target.Parse("null", typeof(TestReferenceType));
			Assert.That(lambda.ReturnType, Is.EqualTo(typeof(TestReferenceType)));
			Assert.That(lambda.Invoke(), Is.Null);
		}

		[Test]
		public void Can_convert_a_null_expression_to_any_nullable_type()
		{
			var target = new Interpreter();

			var lambda = target.Parse("null", typeof(int?));
			Assert.That(lambda.ReturnType, Is.EqualTo(typeof(int?)));
			Assert.That(lambda.Invoke(), Is.Null);

			lambda = target.Parse("null", typeof(DateTime?));
			Assert.That(lambda.ReturnType, Is.EqualTo(typeof(DateTime?)));
			Assert.That(lambda.Invoke(), Is.Null);
		}

		[Test]
		public void A_nullable_type_can_be_a_value_or_null()
		{
			var target = new Interpreter();

			var lambda = target.Parse("null", typeof(int?));
			Assert.That(lambda.ReturnType, Is.EqualTo(typeof(int?)));
			Assert.That(lambda.Invoke(), Is.Null);

			lambda = target.Parse("4651", typeof(int?));
			Assert.That(lambda.ReturnType, Is.EqualTo(typeof(int?)));
			Assert.That(lambda.Invoke(), Is.EqualTo(4651));
		}

		[Test]
		public void Typed_eval()
		{
			var target = new Interpreter();

			double result = target.Eval<double>("Math.Pow(x, y) + 5",
													new Parameter("x", typeof(double), 10),
													new Parameter("y", typeof(double), 2));

			Assert.That(result, Is.EqualTo(Math.Pow(10, 2) + 5));
		}

		private class TestReferenceType { };
	}
}
