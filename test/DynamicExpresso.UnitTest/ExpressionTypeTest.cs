using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DynamicExpresso.UnitTest
{
	[TestClass]
	public class ExpressionTypeTest
	{
		[TestMethod]
		public void If_no_expression_type_is_specified_the_return_type_is_inferred()
		{
			var target = new Interpreter();

			Assert.AreEqual(typeof(string), target.Parse("\"ciao\"").ReturnType);
			Assert.AreEqual(typeof(int), target.Parse("45").ReturnType);
			Assert.AreEqual(typeof(double), target.Parse("45.4").ReturnType);
			Assert.AreEqual(typeof(object), target.Parse("null").ReturnType);
		}

		[TestMethod]
		public void If_expression_type_doesn_t_match_a_conversion_is_performed()
		{
			var target = new Interpreter();
			var expressionType = typeof(double);

			var lambda = target.Parse("213", expressionType);

			Assert.AreEqual(expressionType, lambda.ReturnType);
			Assert.AreEqual((double)213, lambda.Invoke());
		}

		[TestMethod]
		public void If_expression_type_doesn_t_match_a_conversion_is_performed_eventually_loosing_precision()
		{
			var target = new Interpreter();
			var expressionType = typeof(int);

			var lambda = target.Parse("213.46", expressionType);

			Assert.AreEqual(expressionType, lambda.ReturnType);
			Assert.AreEqual((int)213.46, lambda.Invoke());
		}

		[TestMethod]
		public void Can_convert_a_null_expression_to_any_reference_type()
		{
			var target = new Interpreter();

			var lambda = target.Parse("null", typeof(string));
			Assert.AreEqual(typeof(string), lambda.ReturnType);
			Assert.IsNull(lambda.Invoke());

			lambda = target.Parse("null", typeof(TestReferenceType));
			Assert.AreEqual(typeof(TestReferenceType), lambda.ReturnType);
			Assert.IsNull(lambda.Invoke());
		}

		[TestMethod]
		public void Can_convert_a_null_expression_to_any_nullable_type()
		{
			var target = new Interpreter();

			var lambda = target.Parse("null", typeof(int?));
			Assert.AreEqual(typeof(int?), lambda.ReturnType);
			Assert.IsNull(lambda.Invoke());

			lambda = target.Parse("null", typeof(DateTime?));
			Assert.AreEqual(typeof(DateTime?), lambda.ReturnType);
			Assert.IsNull(lambda.Invoke());
		}

		[TestMethod]
		public void A_nullable_type_can_be_a_value_or_null()
		{
			var target = new Interpreter();

			var lambda = target.Parse("null", typeof(int?));
			Assert.AreEqual(typeof(int?), lambda.ReturnType);
			Assert.IsNull(lambda.Invoke());

			lambda = target.Parse("4651", typeof(int?));
			Assert.AreEqual(typeof(int?), lambda.ReturnType);
			Assert.AreEqual(4651, lambda.Invoke());
		}

		[TestMethod]
		public void Typed_eval()
		{
			var target = new Interpreter();

			double result = target.Eval<double>("Math.Pow(x, y) + 5",
													new Parameter("x", typeof(double), 10),
													new Parameter("y", typeof(double), 2));

			Assert.AreEqual(Math.Pow(10, 2) + 5, result);
		}

		class TestReferenceType { };
	}
}
