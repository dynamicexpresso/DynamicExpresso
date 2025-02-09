using System;
using NUnit.Framework;
using System.Globalization;
using System.Reflection;
using System.Linq;

namespace DynamicExpresso.UnitTest
{
	[TestFixture]
	public class ParametersTest
	{
		[Test]
		public void Basic_parameters()
		{
			var target = new Interpreter();

			var parameters = new[] {
                            new Parameter("x", 23),
                            new Parameter("y", 7)
                            };

			Assert.That(target.Eval("x + y", parameters), Is.EqualTo(30));
		}

		[Test]
		public void Parameters_orders_is_not_important_for_eval()
		{
			var target = new Interpreter();

			var parameters = new[] {
                            new Parameter("B", "B"),
                            new Parameter("A", "A"),
                            };

			Assert.That(target.Eval("A + B", parameters), Is.EqualTo("AB"));
		}

		[Test]
		public void Parameters_orders_can_be_different_between_parse_and_invoke()
		{
			var target = new Interpreter();

			var parameters = new[] {
                            new Parameter("A", "A"),
                            new Parameter("B", "B"),
                            };

			var lambda = target.Parse("A + B", parameters);

			Assert.That(lambda.Invoke(parameters.Reverse()), Is.EqualTo("AB"));
		}

		[Test]
		public void Expression_Without_Parameters()
		{
			var target = new Interpreter();

			var parameters = new Parameter[0];

			var exp = target.Parse("10+5", parameters);

			Assert.That(exp.Invoke(), Is.EqualTo(15));
		}

		[Test]
		public void Parameters_Mismatch()
		{
			var target = new Interpreter();

			var parameters = new[] {
                            new Parameter("x", 23),
                            new Parameter("y", 7)
                            };

			var exp = target.Parse("x + y", parameters);

			var parametersMismatch = new[] {
                            new Parameter("x", 546)
                            };

			Assert.Throws<TargetParameterCountException>(() => exp.Invoke(parametersMismatch));
		}

		[Test]
		public void Invoke_the_lambda_using_different_parameters_values()
		{
			var target = new Interpreter();

			var parameters = new[] {
                            new Parameter("x", typeof(int)),
                            new Parameter("y", typeof(int))
                            };

			var myFunc = target.Parse("x + y", parameters);

			var parameters1 = new[] {
                            new Parameter("x", 25),
                            new Parameter("y", 5)
                            };

			Assert.That(myFunc.Invoke(parameters1), Is.EqualTo(30));

			var parameters2 = new[] {
                            new Parameter("x", 60),
                            new Parameter("y", -2)
                            };

			Assert.That(myFunc.Invoke(parameters2), Is.EqualTo(58));
		}

		[Test]
		public void Different_parameters_values_With_Args()
		{
			var target = new Interpreter();

			var parameters = new[] {
                            new Parameter("x", typeof(int)),
                            new Parameter("y", typeof(int))
                            };

			var myFunc = target.Parse("x + y", parameters);

			Assert.That(myFunc.Invoke(23, 7), Is.EqualTo(30));
			Assert.That(myFunc.Invoke(32, -2), Is.EqualTo(30));
		}

		[Test]
		public void Primitive_parameters()
		{
			var target = new Interpreter();

			double x = 2;
			string y = "param y";
			var parameters = new[] {
                            new Parameter("x", x.GetType(), x),
                            new Parameter("y", y.GetType(), y)
                            };

			Assert.That(target.Eval("x", parameters), Is.EqualTo(x));
			Assert.That(target.Eval("x+x+x", parameters), Is.EqualTo(x + x + x));
			Assert.That(target.Eval("x * x", parameters), Is.EqualTo(x * x));
			Assert.That(target.Eval("y", parameters), Is.EqualTo(y));
			Assert.That(target.Eval("y.Length + x", parameters), Is.EqualTo(y.Length + x));
		}

		[Test]
		public void Parameters_by_default_Are_Case_Sensitive()
		{
			var target = new Interpreter();

			double x = 2;
			string X = "param y";
			var parameters = new[] {
                            new Parameter("x", x.GetType(), x),
                            new Parameter("X", X.GetType(), X)
                            };

			Assert.That(target.Eval("x", parameters), Is.EqualTo(x));
			Assert.That(target.Eval("X", parameters), Is.EqualTo(X));
		}

		[Test]
		public void Parameters_can_be_Case_insensitive()
		{
			var target = new Interpreter(InterpreterOptions.DefaultCaseInsensitive);

			double x = 2;
			var parameters = new[] {
                            new Parameter("x", x.GetType(), x)
                            };

			Assert.That(target.Eval("x", parameters), Is.EqualTo(x));
			Assert.That(target.Eval("X", parameters), Is.EqualTo(x));
		}

		[Test]
		public void Parameters_cannot_be_parsed_with_one_case_and_invoked_in_another_case()
		{
			var target = new Interpreter();

			double x = 2;

			var lambda = target.Parse("x", new Parameter("x", x.GetType()));

			Assert.Throws<TargetParameterCountException>(() => lambda.Invoke(new Parameter("X", x)));
		}

		[Test]
		public void Parameters_can_be_parsed_with_one_case_and_invoked_in_another_case_with_case_insensitive_option()
		{
			var target = new Interpreter(InterpreterOptions.DefaultCaseInsensitive);

			double x = 2;

			var lambda = target.Parse("x", new Parameter("x", x.GetType()));

			Assert.That(lambda.Invoke(new Parameter("X", x)), Is.EqualTo(x));
		}

		[Test]
		public void Complex_parameters()
		{
			var target = new Interpreter();

			var x = new MyTestService();
			var y = new Uri("http://www.google.com");
			var z = CultureInfo.GetCultureInfo("en-US");
			var parameters = new[] {
                            new Parameter("x", x.GetType(), x),
                            new Parameter("y", y.GetType(), y),
                            new Parameter("z", z.GetType(), z)
                            };

			Assert.That(target.Eval("x", parameters), Is.EqualTo(x));
			Assert.That(target.Eval("y", parameters), Is.EqualTo(y));
			Assert.That(target.Eval("z", parameters), Is.EqualTo(z));
		}

		[Test]
		public void Methods_Fields_and_Properties_On_Parameters()
		{
			var target = new Interpreter();

			var x = new MyTestService();
			var y = "davide";
			var z = 5;
			var w = DateTime.Today;
			var parameters = new[] {
                            new Parameter("x", x.GetType(), x),
                            new Parameter("y", y.GetType(), y),
                            new Parameter("z", z.GetType(), z),
                            new Parameter("w", w.GetType(), w)
                            };

			Assert.That(target.Eval("x.HelloWorld()", parameters), Is.EqualTo(x.HelloWorld()));
			Assert.That(target.Eval("x.CallMethod( y, z,w)", parameters), Is.EqualTo(x.CallMethod(y, z, w)));
			Assert.That(target.Eval("x.AProperty + 1", parameters), Is.EqualTo(x.AProperty + 1));
			Assert.That(target.Eval("x.AField", parameters), Is.EqualTo(x.AField));
		}

		[Test]
		public void Nullable_as_parameters()
		{
			var target = new Interpreter();

			int? x;
			x = 39;
			int? y;
			y = null;

			var parameters = new[] {
                            new Parameter("x", typeof(int?), x),
                            new Parameter("y", typeof(int?), y)
                            };

			Assert.That(target.Eval("x", parameters), Is.EqualTo(x));
			Assert.That(target.Eval("y", parameters), Is.EqualTo(y));
			Assert.That(target.Eval("x.HasValue", parameters), Is.EqualTo(x.HasValue));
			Assert.That(target.Eval("y.HasValue", parameters), Is.EqualTo(y.HasValue));
		}

		[Test]
		public void Delegates_as_parameters()
		{
			var target = new Interpreter();

			Func<double, double, double> pow = (x, y) => Math.Pow(x, y);
			MyDelegate myDelegate = (x) => x.Length;

			var parameters = new[] {
                            new Parameter("pow", pow.GetType(), pow),
                            new Parameter("myDelegate", myDelegate.GetType(), myDelegate)
                            };

			Assert.That(target.Eval("pow(3, 2)", parameters), Is.EqualTo(9.0));
			Assert.That(target.Eval("myDelegate(\"test\")", parameters), Is.EqualTo(4));
		}

		[Test]
		public void When_parsing_an_expression_only_the_actually_used_parameters_should_be_included_in_the_lambda()
		{
			var target = new Interpreter();

			var parameters = new[] {
                            new Parameter("x", 23),
                            new Parameter("y", 7),
                            new Parameter("z", 54),
                            };

			var lambda = target.Parse("x + y", parameters);

			// parameter 'z' is not used
			Assert.That(lambda.UsedParameters.Count(), Is.EqualTo(2));
			Assert.That(lambda.UsedParameters.ElementAt(0).Name, Is.EqualTo("x"));
			Assert.That(lambda.UsedParameters.ElementAt(1).Name, Is.EqualTo("y"));

			Assert.That(lambda.DeclaredParameters.Count(), Is.EqualTo(3));
			Assert.That(lambda.DeclaredParameters.ElementAt(0).Name, Is.EqualTo("x"));
			Assert.That(lambda.DeclaredParameters.ElementAt(1).Name, Is.EqualTo("y"));
			Assert.That(lambda.DeclaredParameters.ElementAt(2).Name, Is.EqualTo("z"));
		}

		[Test]
		public void Using_the_same_parameters_multiple_times_doesnt_produce_multiple_parameters_in_the_lambda()
		{
			var target = new Interpreter();

			var parameters = new[] {
                            new Parameter("x", 23),
                            new Parameter("y", 7),
                            };

			var lambda = target.Parse("x * y + x * y", parameters);

			Assert.That(lambda.UsedParameters.Count(), Is.EqualTo(2));
			Assert.That(lambda.UsedParameters.ElementAt(0).Name, Is.EqualTo("x"));
			Assert.That(lambda.UsedParameters.ElementAt(1).Name, Is.EqualTo("y"));
		}

		[Test]
		public void When_lambda_is_invoked_input_parameters_must_follow_in_the_same_order_in_which_they_were_transmitted_to_the_interpreter()
		{
			var target = new Interpreter();

			var parameters = new[]{
                            new Parameter("x", typeof(int)),
                            new Parameter("y", typeof(int))
                            };

			var lambda = target.Parse("y-x", parameters);

			Assert.That(lambda.Invoke(1, 5), Is.EqualTo(4));
		}

		[Test]
		public void When_lambda_is_invoked_I_can_omit_parameters_not_used()
		{
			var target = new Interpreter();

			var parameters = new[]{
                            new Parameter("x", typeof(int)),
                            new Parameter("y", typeof(int))
                            };

			var lambda = target.Parse("y+5", parameters);

			Assert.That(lambda.Invoke(new Parameter("y", 2)), Is.EqualTo(7));
		}

		[Test]
		public void When_parsing_an_expression_to_a_delegate_the_delegate_parameters_are_respected_also_if_the_expression_doesnt_use_it()
		{
			var target = new Interpreter();

			var myDelegate = target.ParseAsDelegate<TestDelegate>("x + y");

			// parameter 'z' is not used but the delegate accept it in any case without problem
			Assert.That(myDelegate(1, 2, 123123), Is.EqualTo(3));
			Assert.That(myDelegate(21, 3, 433123), Is.EqualTo(24));
		}

		public delegate int TestDelegate(int x, int y, int z);

		private class MyTestService
		{
			public DateTime AField = DateTime.Now;

			public int AProperty
			{
				get { return 769; }
			}

			public string HelloWorld()
			{
				return "Ciao mondo";
			}

			public string CallMethod(string param1, int param2, DateTime param3)
			{
				return string.Format("{0} {1} {2}", param1, param2, param3);
			}
		}

		private delegate int MyDelegate(string s);

	}
}
