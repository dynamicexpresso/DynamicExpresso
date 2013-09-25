using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;
using System.Reflection;

namespace DynamicExpresso.UnitTest
{
	[TestClass]
	public class ParametersTest
	{
		[TestMethod]
		public void Basic_parameters()
		{
			var target = new Interpreter();

			var parameters = new[] {
                            new Parameter("x", 23),
                            new Parameter("y", 7)
                            };

			Assert.AreEqual(30, target.Eval("x + y", parameters));
		}

		[TestMethod]
		public void Expression_Without_Parameters()
		{
			var target = new Interpreter();

			var parameters = new Parameter[0];

			var exp = target.Parse("10+5", parameters);

			Assert.AreEqual(15, exp.Invoke());
		}

		[ExpectedException(typeof(TargetParameterCountException))]
		[TestMethod]
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

			Assert.AreEqual(30, exp.Invoke(parametersMismatch));
		}

		[TestMethod]
		public void Different_parameters_values_With_Parameters()
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

			Assert.AreEqual(30, myFunc.Invoke(parameters1));

			var parameters2 = new[] {
                            new Parameter("x", 60),
                            new Parameter("y", -2)
                            };

			Assert.AreEqual(58, myFunc.Invoke(parameters2));
		}

		[TestMethod]
		public void Different_parameters_values_With_Args()
		{
			var target = new Interpreter();

			var parameters = new[] {
                            new Parameter("x", typeof(int)),
                            new Parameter("y", typeof(int))
                            };

			var myFunc = target.Parse("x + y", parameters);

			Assert.AreEqual(30, myFunc.Invoke(23, 7));
			Assert.AreEqual(30, myFunc.Invoke(32, -2));
		}

		[TestMethod]
		public void Primitive_parameters()
		{
			var target = new Interpreter();

			double x = 2;
			string y = "param y";
			var parameters = new[] {
                            new Parameter("x", x.GetType(), x),
                            new Parameter("y", y.GetType(), y)
                            };

			Assert.AreEqual(x, target.Eval("x", parameters));
			Assert.AreEqual(x + x + x, target.Eval("x+x+x", parameters));
			Assert.AreEqual(x * x, target.Eval("x * x", parameters));
			Assert.AreEqual(y, target.Eval("y", parameters));
			Assert.AreEqual(y.Length + x, target.Eval("y.Length + x", parameters));
		}

		[TestMethod]
		public void Parameters_Are_Case_Sensitive()
		{
			var target = new Interpreter();

			double x = 2;
			string X = "param y";
			var parameters = new[] {
                            new Parameter("x", x.GetType(), x),
                            new Parameter("X", X.GetType(), X)
                            };

			Assert.AreEqual(x, target.Eval("x", parameters));
			Assert.AreEqual(X, target.Eval("X", parameters));
		}

		[TestMethod]
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

			Assert.AreEqual(x, target.Eval("x", parameters));
			Assert.AreEqual(y, target.Eval("y", parameters));
			Assert.AreEqual(z, target.Eval("z", parameters));
		}

		[TestMethod]
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

			Assert.AreEqual(x.HelloWorld(), target.Eval("x.HelloWorld()", parameters));
			Assert.AreEqual(x.CallMethod(y, z, w), target.Eval("x.CallMethod( y, z,w)", parameters));
			Assert.AreEqual(x.AProperty + 1, target.Eval("x.AProperty + 1", parameters));
			Assert.AreEqual(x.AField, target.Eval("x.AField", parameters));
		}

		[TestMethod]
		public void Nullable_parameters()
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

			Assert.AreEqual(x, target.Eval("x", parameters));
			Assert.AreEqual(y, target.Eval("y", parameters));
			Assert.AreEqual(x.HasValue, target.Eval("x.HasValue", parameters));
			Assert.AreEqual(y.HasValue, target.Eval("y.HasValue", parameters));
		}

		[TestMethod]
		public void Delegates_parameters()
		{
			var target = new Interpreter();

			Func<double, double, double> pow = (x, y) => Math.Pow(x, y);
			MyDelegate myDelegate = (x) => x.Length;

			var parameters = new[] {
                            new Parameter("pow", pow.GetType(), pow),
                            new Parameter("myDelegate", myDelegate.GetType(), myDelegate)
                            };

			Assert.AreEqual(9.0, target.Eval("pow(3, 2)", parameters));
			Assert.AreEqual(4, target.Eval("myDelegate(\"test\")", parameters));
		}

		class MyTestService
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

		delegate int MyDelegate(string s);

	}
}
