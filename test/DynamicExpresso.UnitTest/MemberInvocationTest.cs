using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DynamicExpresso.UnitTest
{
	[TestClass]
	public class MemberInvocationTest
	{
		[TestMethod]
		public void Method_Property_Field_basic_test()
		{
			var x = new MyTestService();

			var target = new Interpreter().SetVariable("x", x);

			Assert.AreEqual(x.HelloWorld(), target.Eval("x.HelloWorld()"));
			Assert.AreEqual(x.AProperty, target.Eval("x.AProperty"));
			Assert.AreEqual(x.AField, target.Eval("x.AField"));
		}

		[TestMethod]
		public void Indexer()
		{
			var target = new Interpreter();

			var x = "ciao";
			target.SetVariable("x", x);
			var y = new MyTestService();
			target.SetVariable("y", y);

			Assert.AreEqual(x[2], target.Eval("x[2]"));
			Assert.AreEqual(y[2], target.Eval("y[2]"));
			Assert.AreEqual(y[2].ToString(), target.Eval("y[2].ToString()"));
		}

		[TestMethod]
		public void String_format()
		{
			var target = new Interpreter();

			Assert.AreEqual(string.Format("ciao {0}, today is {1}", "mondo", DateTime.Today),
											target.Eval("string.Format(\"ciao {0}, today is {1}\", \"mondo\", DateTime.Today.ToString())"));
		}

		[TestMethod]
		public void String_format_With_Object_Params()
		{
			var target = new Interpreter();

			Assert.AreEqual(string.Format("ciao mondo, today is {0}", DateTime.Today),
											target.Eval("string.Format(\"ciao {0}, today is {1}\", \"mondo\", DateTime.Today)"));
		}

		[TestMethod]
		public void Methods_Fields_And_Properties_Are_Case_Sensitive()
		{
			var target = new Interpreter();

			var x = new MyTestService();
			var parameters = new[] {
                            new Parameter("x", x.GetType(), x)
                            };

			Assert.AreEqual(x.HelloWorld(), target.Eval("x.HelloWorld()", parameters));
			Assert.AreEqual(x.HELLOWORLD(), target.Eval("x.HELLOWORLD()", parameters));
			Assert.AreEqual(x.AProperty, target.Eval("x.AProperty", parameters));
			Assert.AreEqual(x.APROPERTY, target.Eval("x.APROPERTY", parameters));
			Assert.AreEqual(x.AField, target.Eval("x.AField", parameters));
			Assert.AreEqual(x.AFIELD, target.Eval("x.AFIELD", parameters));
		}

		[TestMethod]
		public void Void_Method()
		{
			var service = new MyTestService();
			var target = new Interpreter()
											.SetVariable("service", service);

			Assert.AreEqual(0, service.VoidMethodCalled);
			target.Eval("service.VoidMethod()");
			Assert.AreEqual(1, service.VoidMethodCalled);

			Assert.AreEqual(typeof(void), target.Parse("service.VoidMethod()").ReturnType);
		}

		[TestMethod]
		public void Method_with_nullable_param()
		{
			var target = new Interpreter();

			var x = new MyTestService();
			var y = "davide";
			var z = 5;
			int? w = null;
			var parameters = new[] {
                            new Parameter("x", x.GetType(), x),
                            new Parameter("y", y.GetType(), y),
                            new Parameter("z", z.GetType(), z),
                            new Parameter("w", typeof(int?), w)
                            };

			Assert.AreEqual(x.MethodWithNullableParam(y, z), target.Eval("x.MethodWithNullableParam(y, z)", parameters));
			Assert.AreEqual(x.MethodWithNullableParam(y, w), target.Eval("x.MethodWithNullableParam(y, w)", parameters));
			Assert.AreEqual(x.MethodWithNullableParam(y, 30), target.Eval("x.MethodWithNullableParam(y, 30)", parameters));
			Assert.AreEqual(x.MethodWithNullableParam(y, null), target.Eval("x.MethodWithNullableParam(y, null)", parameters));
		}

		[TestMethod]
		public void Method_with_generic_param()
		{
			var target = new Interpreter();

			var x = new MyTestService();
			var y = "davide";
			double z = 5;
			int? w = null;
			var parameters = new[] {
                            new Parameter("x", x.GetType(), x),
                            new Parameter("y", y.GetType(), y),
                            new Parameter("z", z.GetType(), z),
                            new Parameter("w", typeof(int?), w)
                            };

			Assert.AreEqual(x.MethodWithGenericParam(x), target.Eval("x.MethodWithGenericParam(x)", parameters));
			Assert.AreEqual(x.MethodWithGenericParam(y), target.Eval("x.MethodWithGenericParam(y)", parameters));
			Assert.AreEqual(x.MethodWithGenericParam(z), target.Eval("x.MethodWithGenericParam(z)", parameters));
			Assert.AreEqual(x.MethodWithGenericParam(w), target.Eval("x.MethodWithGenericParam(w)", parameters));

			Assert.AreEqual(x.MethodWithGenericParam(y, x), target.Eval("x.MethodWithGenericParam(y, x)", parameters));
			Assert.AreEqual(x.MethodWithGenericParam(y, y), target.Eval("x.MethodWithGenericParam(y, y)", parameters));
			Assert.AreEqual(x.MethodWithGenericParam(y, z), target.Eval("x.MethodWithGenericParam(y, z)", parameters));
			Assert.AreEqual(x.MethodWithGenericParam(y, w), target.Eval("x.MethodWithGenericParam(y, w)", parameters));
		}

		class MyTestService
		{
			public DateTime AField = DateTime.Now;
			public DateTime AFIELD = DateTime.UtcNow;

			public int AProperty
			{
				get { return 769; }
			}

			public int APROPERTY
			{
				get { return 887; }
			}

			public string HelloWorld()
			{
				return "Ciao mondo";
			}

			public string HELLOWORLD()
			{
				return "HELLO";
			}

			public int VoidMethodCalled { get; set; }
			public void VoidMethod()
			{
				System.Diagnostics.Debug.WriteLine("VoidMethod called");
				VoidMethodCalled++;
			}

			public string MethodWithNullableParam(string param1, int? param2)
			{
				return string.Format("{0} {1}", param1, param2);
			}

			public string MethodWithGenericParam<T>(T p)
			{
				return string.Format("{0}", p);
			}

			public string MethodWithGenericParam<T>(string a, T p)
			{
				return string.Format("{0} {1}", a, p);
			}

			public DateTime this[int i]
			{
				get { return DateTime.Today.AddDays(i); }
			}
		}

	}
}
