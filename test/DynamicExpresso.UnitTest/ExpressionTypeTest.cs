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

			Assert.AreEqual(typeof(string), target.Parse("\"ciao\"").Expression.Type);
			Assert.AreEqual(typeof(int), target.Parse("45").Expression.Type);
			Assert.AreEqual(typeof(double), target.Parse("45.4").Expression.Type);
			Assert.AreEqual(typeof(object), target.Parse("null").Expression.Type);
		}

		[Test]
		public void If_expression_type_doesn_t_match_a_conversion_is_performed_when_possible()
		{
			var target = new Interpreter();
			var expressionType = typeof(double);

			var parseResult = target.Parse("213", expressionType);

			Assert.AreEqual(expressionType, parseResult.Expression.Type);
			Assert.AreEqual((double)213, parseResult.Compile().DynamicInvoke());
		}

		[Test]
		public void If_expression_type_doesn_t_match_a_conversion_is_performed_eventually_loosing_precision()
		{
			var target = new Interpreter();
			var expressionType = typeof(int);

			var parseResult = target.Parse("213.46", expressionType);

			Assert.AreEqual(expressionType, parseResult.Expression.Type);
			Assert.AreEqual((int)213.46, parseResult.Compile().DynamicInvoke());
		}

		[Test]
		public void Can_convert_a_null_expression_to_any_reference_type()
		{
			var target = new Interpreter();

			var parseResult = target.Parse("null", typeof(string));
			Assert.AreEqual(typeof(string), parseResult.Expression.Type);
			Assert.IsNull(parseResult.Compile().DynamicInvoke());

			parseResult = target.Parse("null", typeof(TestReferenceType));
			Assert.AreEqual(typeof(TestReferenceType), parseResult.Expression.Type);
			Assert.IsNull(parseResult.Compile().DynamicInvoke());
		}

		[Test]
		public void Can_convert_a_null_expression_to_any_nullable_type()
		{
			var target = new Interpreter();

			var parseResult = target.Parse("null", typeof(int?));
			Assert.AreEqual(typeof(int?), parseResult.Expression.Type);
			Assert.IsNull(parseResult.Compile().DynamicInvoke());

			parseResult = target.Parse("null", typeof(DateTime?));
			Assert.AreEqual(typeof(DateTime?), parseResult.Expression.Type);
			Assert.IsNull(parseResult.Compile().DynamicInvoke());
		}

		[Test]
		public void A_nullable_type_can_be_a_value_or_null()
		{
			var target = new Interpreter();

			var parseResult = target.Parse("null", typeof(int?));
			Assert.AreEqual(typeof(int?), parseResult.Expression.Type);
			Assert.IsNull(parseResult.Compile().DynamicInvoke());

			parseResult = target.Parse("4651", typeof(int?));
			Assert.AreEqual(typeof(int?), parseResult.Expression.Type);
			Assert.AreEqual(4651, parseResult.Compile().DynamicInvoke());
		}

		[Test]
		public void Typed_eval()
		{
			var target = new Interpreter();

			double result = target.Eval<double>("Math.Pow(x, y) + 5",
													new Parameter("x", typeof(double), 10),
													new Parameter("y", typeof(double), 2));

			Assert.AreEqual(Math.Pow(10, 2) + 5, result);
		}

		private class TestReferenceType { };
	}
}
