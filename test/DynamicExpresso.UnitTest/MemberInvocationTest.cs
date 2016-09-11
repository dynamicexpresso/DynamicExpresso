using System;
using System.Linq;
using NUnit.Framework;

namespace DynamicExpresso.UnitTest
{
	[TestFixture]
	public class MemberInvocationTest
	{
		[Test]
		public void Method_Property_Field_basic_test()
		{
			var x = new MyTestService();

			var target = new Interpreter().SetVariable("x", x);

			Assert.AreEqual(x.HelloWorld(), target.Eval("x.HelloWorld()"));
			Assert.AreEqual(x.AProperty, target.Eval("x.AProperty"));
			Assert.AreEqual(x.AField, target.Eval("x.AField"));
		}

		[Test]
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

		[Test]
		public void String_format()
		{
			var target = new Interpreter();

			Assert.AreEqual(string.Format("ciao {0}, today is {1}", "mondo", DateTime.Today),
											target.Eval("string.Format(\"ciao {0}, today is {1}\", \"mondo\", DateTime.Today.ToString())"));
		}

		[Test]
		public void String_format_with_type_conversion()
		{
			var target = new Interpreter();

			Assert.AreEqual(string.Format("ciao {0}, today is {1}", "mondo", DateTime.Today),
											target.Eval("string.Format(\"ciao {0}, today is {1}\", \"mondo\", DateTime.Today)"));
		}

		[Test]
		public void String_format_with_empty_string()
		{
			var target = new Interpreter();

			Assert.AreEqual(string.Format("ciao {0}", ""),
											target.Eval("string.Format(\"ciao {0}\", \"\")"));
		}

		[Test]
		public void String_format_With_Object_Params()
		{
			var target = new Interpreter();

			Assert.AreEqual(string.Format("ciao mondo, today is {0}", DateTime.Today),
											target.Eval("string.Format(\"ciao {0}, today is {1}\", \"mondo\", DateTime.Today)"));
		}

		[Test]
		public void Methods_Fields_And_Properties_By_Default_Are_Case_Sensitive()
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

		[Test]
		public void Methods_Fields_And_Properties_Can_Be_Case_Insensitive()
		{
			var target = new Interpreter(InterpreterOptions.DefaultCaseInsensitive);

			var x = new MyTestServiceCaseInsensitive();
			var parameters = new[] {
                            new Parameter("x", x.GetType(), x)
                            };

			Assert.AreEqual(x.AMethod(), target.Eval("x.AMethod()", parameters));
			Assert.AreEqual(x.AMethod(), target.Eval("x.AMETHOD()", parameters));
			Assert.AreEqual(x.AProperty, target.Eval("x.AProperty", parameters));
			Assert.AreEqual(x.AProperty, target.Eval("x.APROPERTY", parameters));
			Assert.AreEqual(x.AField, target.Eval("x.AField", parameters));
			Assert.AreEqual(x.AField, target.Eval("x.AFIELD", parameters));
		}

		[Test]
		public void Void_Method()
		{
			var service = new MyTestService();
			var target = new Interpreter()
											.SetVariable("service", service);

			Assert.AreEqual(0, service.VoidMethodCalls);
			target.Eval("service.VoidMethod()");
			Assert.AreEqual(1, service.VoidMethodCalls);

			Assert.AreEqual(typeof(void), target.Parse("service.VoidMethod()").ReturnType);
		}

		[Test]
		public void ToString_Method_on_a_custom_type()
		{
			var service = new MyTestService();
			var target = new Interpreter()
											.SetVariable("service", service);

			Assert.AreEqual("DynamicExpresso.UnitTest.MemberInvocationTest+MyTestService", target.Eval("service.ToString()"));
		}

		[Test]
		public void ToString_Method_on_a_custom_interface_type()
		{
			MyTestInterface service = new MyTestInterfaceImp();
			var target = new Interpreter()
											.SetVariable("service", service, typeof(MyTestInterface));

			Assert.AreEqual("DynamicExpresso.UnitTest.MemberInvocationTest+MyTestInterfaceImp", target.Eval("service.ToString()"));
		}

		[Test]
		public void ToString_Method_on_a_primitive_type()
		{
			var target = new Interpreter();
			Assert.AreEqual("3", target.Eval("(3).ToString()"));
		}

		[Test]
		public void GetType_Method_on_a_custom_type()
		{
			var service = new MyTestService();
			var target = new Interpreter()
											.SetVariable("service", service);

			Assert.AreEqual(typeof(MyTestService), target.Eval("service.GetType()"));
		}

		[Test]
		public void GetType_Method_on_a_custom_interface_type()
		{
			MyTestInterface service = new MyTestInterfaceImp();
			var target = new Interpreter()
											.SetVariable("service", service, typeof(MyTestInterface));

			Assert.AreEqual(typeof(MyTestInterfaceImp), target.Eval("service.GetType()"));
		}

		[Test]
		public void GetType_Method_on_a_primitive_type()
		{
			var target = new Interpreter();
			Assert.AreEqual((3).GetType(), target.Eval("(3).GetType()"));
		}

		[Test]
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

		[Test]
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

		[Test]
		public void Method_with_params_array()
		{
			var target = new Interpreter();

			var x = new MyTestService();

			target.SetVariable("x", x);

			Assert.AreEqual(0, x.MethodWithParamsArrayCalls);

			Assert.AreEqual(x.MethodWithParamsArray(DateTime.Now, 2, 1, 34), target.Eval("x.MethodWithParamsArray(DateTime.Now, 2, 1, 34)"));
			Assert.AreEqual(2, x.MethodWithParamsArrayCalls);

			var myParamArray = new int[] { 2, 1, 34 };
			target.SetVariable("myParamArray", myParamArray);

			Assert.AreEqual(x.MethodWithParamsArray(DateTime.Now, myParamArray), target.Eval("x.MethodWithParamsArray(DateTime.Now, myParamArray)"));
			Assert.AreEqual(4, x.MethodWithParamsArrayCalls);
		}

		[Test]
		public void ParamsArray_methods_are_not_called_when_there_is_an_exact_method_match()
		{
			var target = new Interpreter();

			var x = new MyTestService();

			target.SetVariable("x", x);

			target.Eval("x.AmbiguousMethod(DateTime.Now, 2, 3)");
			Assert.AreEqual(1, x.AmbiguousMethod_NormalCalls);
			Assert.AreEqual(0, x.AmbiguousMethod_ParamsArrayCalls);

			target.Eval("x.AmbiguousMethod(DateTime.Now, 2, 3, 4)");
			Assert.AreEqual(1, x.AmbiguousMethod_NormalCalls);
			Assert.AreEqual(1, x.AmbiguousMethod_ParamsArrayCalls);
		}

		[Test]
		public void Overload_paramsArray_methods_with_compatible_type_params()
		{
			var target = new Interpreter();
			var x = new MyTestService();
			target.SetVariable("x", x);
			Assert.AreEqual(3, target.Eval("x.OverloadMethodWithParamsArray(2, 3, 1)"));
		}

		interface MyTestInterface
		{
		}
		class MyTestInterfaceImp : MyTestInterface
		{

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

			public int VoidMethodCalls { get; set; }
			public void VoidMethod()
			{
				System.Diagnostics.Debug.WriteLine("VoidMethod called");
				VoidMethodCalls++;
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

			public int MethodWithParamsArrayCalls { get; set; }
			public int MethodWithParamsArray(DateTime fixedParam, params int[] paramsArray)
			{
				MethodWithParamsArrayCalls++;

				return paramsArray.Sum();
			}

			public int AmbiguousMethod_ParamsArrayCalls { get; set; }
			public void AmbiguousMethod(DateTime fixedParam, params int[] paramsArray)
			{
				AmbiguousMethod_ParamsArrayCalls++;
			}
			public int AmbiguousMethod_NormalCalls { get; set; }
			public void AmbiguousMethod(DateTime fixedParam, int p1, int p2)
			{
				AmbiguousMethod_NormalCalls++;
			}

			public int OverloadMethodWithParamsArray(params int[] paramsArray)
			{
				return paramsArray.Max();
			}
			public long OverloadMethodWithParamsArray(params long[] paramsArray)
			{
				return paramsArray.Max();
			}
		}

		class MyTestServiceCaseInsensitive
		{
			public DateTime AField = DateTime.Now;

			public int AProperty
			{
				get { return 769; }
			}

			public string AMethod()
			{
				return "Ciao mondo";
			}
		}
	}
}
