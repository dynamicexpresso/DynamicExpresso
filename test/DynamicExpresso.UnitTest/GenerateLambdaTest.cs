using System;
using NUnit.Framework;
using System.Linq.Expressions;

namespace DynamicExpresso.UnitTest
{
	[TestFixture]
	public class GenerateLambdaTest
	{
		[Test]
		public void Parse_as_LambdaExpression()
		{
			var target = new Interpreter();

			var lambdaExpression = target.Parse<Func<double, double>>("arg + 5").AsExpression();

			Assert.AreEqual(15, lambdaExpression.Compile()(10));
		}

		[Test]
		public void Parse_as_LambdaExpression_with_parameter()
		{
			var target = new Interpreter();

			var lambdaExpression = target.Parse<Func<double, double>>("arg + 5").AsExpression();

			Assert.AreEqual(15, lambdaExpression.Compile()(10));

			lambdaExpression = target.Parse<Func<double, double>>("arg + .5").AsExpression();
			Assert.AreEqual(10.5, lambdaExpression.Compile()(10));
		}

		[Test]
		public void Parse_To_a_Delegate_With_Two_Parameters()
		{
			var target = new Interpreter();

			var lambdaExpression = target.Parse<Func<double, double, double>>("arg1 * arg2").AsExpression();

			Assert.AreEqual(6, lambdaExpression.Compile()(3, 2));
			Assert.AreEqual(50, lambdaExpression.Compile()(5, 10));
		}

		[Test]
		public void Parse_To_a_Delegate_With_Two_Parameters_With_Custom_Name()
		{
			var target = new Interpreter();

			var argumentNames = new[] { "x", "y" };
			var lambdaExpression = target.Parse<Func<double, double, double>>("x * y", argumentNames).AsExpression();

			Assert.AreEqual(6, lambdaExpression.Compile()(3, 2));
			Assert.AreEqual(50, lambdaExpression.Compile()(5, 10));
		}

		[Test]
		public void Generate_a_LambdaExpression_From_Lambda()
		{
			var target = new Interpreter();

			var parseResult = target.Parse("Math.Pow(x, y) + 5",
				Expression.Parameter(typeof(double), "x"),
				Expression.Parameter(typeof(double), "y")
			);

			var lambdaExpression = parseResult.AsExpression<Func<double, double, double>>();

			Assert.AreEqual(Math.Pow(10, 2) + 5, lambdaExpression.Compile()(10, 2));
		}

		[Test]
		public void Cannot_Generate_a_LambdaExpression_From_Lambda_with_parameters_count_mismatch()
		{
			var target = new Interpreter();

			// Func delegate has 2 inputs, I just use one

			var parseResult = target.Parse("x + 5", Expression.Parameter(typeof(double), "x"));

			Assert.Throws<ArgumentException>(() => parseResult.AsExpression<Func<double, double, double>>());
		}

		[Test]
		public void Cannot_Generate_a_LambdaExpression_From_Lambda_with_parameters_type_mismatch()
		{
			var target = new Interpreter();

			// Func delegate takes a string, I pass a double

			var parseResult = target.Parse("x + 5", Expression.Parameter(typeof(double), "x"));

			Assert.Throws<ArgumentException>(() => parseResult.AsExpression<Func<string, double>>());
		}

		[Test]
		public void Cannot_generate_a_Lambda_with_return_type_mismatch()
		{
			var target = new Interpreter();

			// Func delegate returns a string, I return a double

			var parseResult = target.Parse("x + 5", Expression.Parameter(typeof(double), "x"));

			Assert.Throws<ArgumentException>(() => parseResult.AsExpression<Func<double, string>>());
		}

	}
}
