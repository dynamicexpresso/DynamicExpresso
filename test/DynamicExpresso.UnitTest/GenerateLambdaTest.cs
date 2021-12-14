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

			Expression<Func<double, double>> lambdaExpression = target.ParseAsExpression<Func<double, double>>("arg + 5");

			Assert.AreEqual(15, lambdaExpression.Compile()(10));
		}

		[Test]
		public void Parse_as_LambdaExpression_with_parameter()
		{
			var target = new Interpreter();

			Expression<Func<double, double>> lambdaExpression = target.ParseAsExpression<Func<double, double>>("arg + 5");

			Assert.AreEqual(15, lambdaExpression.Compile()(10));

			lambdaExpression = target.ParseAsExpression<Func<double, double>>("arg + .5");
			Assert.AreEqual(10.5, lambdaExpression.Compile()(10));
		}

		[Test]
		public void Parse_To_a_Delegate_With_Two_Parameters()
		{
			var target = new Interpreter();

			var lambdaExpression = target.ParseAsExpression<Func<double, double, double>>("arg1 * arg2");

			Assert.AreEqual(6, lambdaExpression.Compile()(3, 2));
			Assert.AreEqual(50, lambdaExpression.Compile()(5, 10));
		}

		[Test]
		public void Parse_To_a_Delegate_With_Two_Parameters_With_Custom_Name()
		{
			var target = new Interpreter();

			var argumentNames = new string[] { "x", "y" };
			var lambdaExpression = target.ParseAsExpression<Func<double, double, double>>("x * y", argumentNames);

			Assert.AreEqual(6, lambdaExpression.Compile()(3, 2));
			Assert.AreEqual(50, lambdaExpression.Compile()(5, 10));
		}

		[Test]
		public void Generate_a_LambdaExpression_From_Lambda()
		{
			var target = new Interpreter();

			var lambda = target.Parse("Math.Pow(x, y) + 5",
				new Parameter("x", typeof(double)),
				new Parameter("y", typeof(double))
			);

			Expression<Func<double, double, double>> lambdaExpression = lambda.LambdaExpression<Func<double, double, double>>();

			Assert.AreEqual(Math.Pow(10, 2) + 5, lambdaExpression.Compile()(10, 2));
		}

		[Test]
		public void Cannot_Generate_a_LambdaExpression_From_Lambda_with_parameters_count_mismatch()
		{
			var target = new Interpreter();

			// Func delegate has 2 inputs, I just use one

			var lambda = target.Parse("x + 5",
				new Parameter("x", typeof(double))
			);

			Assert.Throws<ArgumentException>(() => lambda.LambdaExpression<Func<double, double, double>>());
		}

		[Test]
		public void Cannot_Generate_a_LambdaExpression_From_Lambda_with_parameters_type_mismatch()
		{
			var target = new Interpreter();

			// Func delegate takes a string, I pass a double

			var lambda = target.Parse("x + 5",
				new Parameter("x", typeof(double))
			);

			Assert.Throws<ArgumentException>(() => lambda.LambdaExpression<Func<string, double>>());
		}

		[Test]
		public void Cannot_generate_a_Lambda_with_return_type_mismatch()
		{
			var target = new Interpreter();

			// Func delegate returns a string, I return a double

			var lambda = target.Parse("x + 5",
				new Parameter("x", typeof(double))
			);

			Assert.Throws<ArgumentException>(() => lambda.LambdaExpression<Func<double, string>>());
		}

	}
}
