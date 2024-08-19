using System;
using System.Collections.Generic;
using System.Linq;
using DynamicExpresso.Exceptions;
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

		[Ignore("See issue 65")]
		[Test]
		public void Null_conditional_property()
		{
			var target = new Interpreter().SetVariable("x", null, typeof(MyTestService));
			Assert.IsNull(target.Eval("x?.AProperty"));
		}

		[Test]
		public void Indexer_Getter()
		{
			var target = new Interpreter();

			var x = "ciao";
			target.SetVariable("x", x);
			var y = new MyTestService();
			target.SetVariable("y", y);
			var z = new[] {7, 8, 9, 10};
			target.SetVariable("z", z);

			Assert.AreEqual(x[2], target.Eval("x[2]"));
			Assert.AreEqual(y[2], target.Eval("y[2]"));
			Assert.AreEqual(y[2].ToString(), target.Eval("y[2].ToString()"));
			Assert.AreEqual(y[(short)2], target.Eval("y[(Int16)2]"));
			Assert.AreEqual(z[2], target.Eval("z[2]"));
		}

		[Test]
		public void Indexer_Setter()
		{
			var target = new Interpreter();

			var x = new System.Text.StringBuilder("time");
			target.SetVariable("x", x);
			var y = new MyTestService();
			target.SetVariable("y", y);
			var z = new[] { 7, 8, 9, 10 };
			target.SetVariable("z", z);

			target.Eval("x[2] = 'r'");
			Assert.AreEqual(x.ToString(), "tire");
			target.Eval("y[(Int16)9] = y.Today");
			Assert.AreEqual(y.AField, y.Today.AddYears(9));
			target.Eval("y[(Int64)7] = y.Today");
			Assert.AreEqual(y.AField, y.Today.AddSeconds(7));
			target.Eval("z[2] = 4");
			Assert.AreEqual(z, new[] { 7, 8, 4, 10 });
		}

		[Test]
		public void Cannot_assign_without_setter()
		{
			var target = new Interpreter()
				.SetVariable("x", new MyTestService());

			Assert.Throws<Exceptions.ParseException>(() => target.Parse("x[8] = x.Today"));
			Assert.Throws<Exceptions.ParseException>(() => target.Parse("x.AProperty = x.Today"));
		}

		[Test]
		public void Indexer_Collections()
		{
			var target = new Interpreter();

			var x = new List<int> { 3, 4, 5, 6 };
			target.SetVariable("x", x);
			var y = new Dictionary<string, int> { { "first", 1 }, { "second", 2 }, { "third", 3 } };
			target.SetVariable("y", y);

			Assert.AreEqual(x[2], target.Eval("x[2]"));
			Assert.AreEqual(y["second"], target.Eval("y[\"second\"]"));

			target.Eval("x[2] = 1");
			Assert.AreEqual(x, new List<int> { 3, 4, 1, 6 });
			target.Eval("y[\"second\"] = 2000");
			Assert.AreEqual(y, new Dictionary<string, int> { { "first", 1 }, { "second", 2000 }, { "third", 3 } });
		}

		[Test]
		public void Indexer_Getter_MultiDimensional()
		{
			var target = new Interpreter();

			var x = new[,] { { 11, 12, 13, 14 }, { 21, 22, 23, 24 }, { 31, 32, 33, 34 } };
			target.SetVariable("x", x);
			var y = new MyTestService();
			target.SetVariable("y", y);

			Assert.AreEqual(x[1, 2], target.Eval("x[1, 2]"));
			Assert.AreEqual(y[y.Today, 2], target.Eval("y[y.Today, 2]"));
			Assert.AreEqual(y[y.Today], target.Eval("y[y.Today]"));
		}

		[Test]
		public void Indexer_Setter_MultiDimensional()
		{
			var target = new Interpreter();

			var x = new[,] { { 11, 12, 13, 14 }, { 21, 22, 23, 24 }, { 31, 32, 33, 34 } };
			target.SetVariable("x", x);
			var y = new MyTestService();
			target.SetVariable("y", y);

			var span = TimeSpan.FromDays(3.5);
			target.SetVariable("span", span);

			target.Eval("x[1, 2] = 7");
			Assert.AreEqual(x, new[,] { { 11, 12, 13, 14 }, { 21, 22, 7, 24 }, { 31, 32, 33, 34 } });
			target.Eval("y[y.Today, 2] = span");
			Assert.AreEqual(y.AField, y.Today.AddDays(2).Add(span));
			target.Eval("y[y.Today] = span");
			Assert.AreEqual(y.AField, y.Today.AddDays(3).Add(span));
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

			Assert.AreEqual(x.MethodWithGenericParamAndDefault(y,y), target.Eval("x.MethodWithGenericParamAndDefault(y,y)", parameters));
			Assert.AreEqual(x.MethodWithGenericParamAndDefault(y), target.Eval("x.MethodWithGenericParamAndDefault(y)", parameters));
			Assert.AreEqual(x.MethodWithGenericParamAndDefault1Levels(y), target.Eval("x.MethodWithGenericParamAndDefault1Levels(y)", parameters));
			Assert.AreEqual(x.MethodWithGenericParamAndDefault2Levels(y), target.Eval("x.MethodWithGenericParamAndDefault2Levels(y)", parameters));
			Assert.AreEqual(x.MethodWithGenericParamAndDefault2Levels(y, w), target.Eval("x.MethodWithGenericParamAndDefault2Levels(y, w)", parameters));
		}

		[Test]
		public void Method_with_generic_constraints()
		{
			var target = new Interpreter();

			var x = new MyTestService();
			target.SetVariable("x", x);

			Assert.AreEqual("works", target.Eval("x.GenericMethodWithConstraint(\"works\")"));
			Assert.Throws<NoApplicableMethodException>(() => target.Eval("x.GenericMethodWithConstraint(5)"), "This shouldn't throw a System.ArgumentException \"Violates the constraint of type 'T'\"");
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


		[Test]
		public void Generic_method_with_params()
		{
			var target = new Interpreter();
			target.Reference(typeof(Utils));

			var listInt = target.Eval<List<int>>("Utils.Array(1, 2, 3)");
			Assert.AreEqual(new[] { 1, 2, 3 }, listInt);

			// type parameter can't be inferred from usage
			Assert.Throws<ParseException>(() => target.Eval<List<int>>("Utils.Array(1,\"str\", 3)"));
		}

		[Test]
		public void Method_with_optional_param()
		{
			var target = new Interpreter();

			var x = new MyTestService();
			var y = "1";
			var z = "2";
			var w = "3";
			var parameters = new[] {
				new Parameter("x", x.GetType(), x),
				new Parameter("y", y.GetType(), y),
				new Parameter("z", z.GetType(), z),
				new Parameter("w", w.GetType(), w)
			};

			Assert.AreEqual(x.MethodWithOptionalParam(y), target.Eval("x.MethodWithOptionalParam(y)", parameters));
			Assert.AreEqual(x.MethodWithOptionalParam(y, z), target.Eval("x.MethodWithOptionalParam(y, z)", parameters));
			Assert.AreEqual(x.MethodWithOptionalParam(z, y), target.Eval("x.MethodWithOptionalParam(z, y)", parameters));
			Assert.AreEqual(x.MethodWithOptionalParam(y, z, w), target.Eval("x.MethodWithOptionalParam(y, z, w)", parameters));
			Assert.AreEqual(x.MethodWithOptionalParam(w, y, z), target.Eval("x.MethodWithOptionalParam(w, y, z)", parameters));
		}

		[Test]
		public void Method_with_optional_null_param()
		{
			var target = new Interpreter();

			var x = new MyTestService();
			var y = "1";
			var z = "2";
			var parameters = new[] {
				new Parameter("x", x.GetType(), x),
				new Parameter("y", y.GetType(), y),
				new Parameter("z", z.GetType(), z),
			};

			Assert.AreEqual(x.MethodWithOptionalNullParam(y), target.Eval("x.MethodWithOptionalNullParam(y)", parameters));
			Assert.AreEqual(x.MethodWithOptionalNullParam(y, z), target.Eval("x.MethodWithOptionalNullParam(y, z)", parameters));
		}

		[Test]
		public void Chaining_Methods()
		{
			var x = new MyTestService();

			var target = new Interpreter();
			target.SetVariable("x", x);

			Assert.AreEqual(x.HelloWorld().ToUpper(), target.Eval("x.HelloWorld().ToUpper()"));
		}

		internal static class Utils
		{
			public static int GenericVsNonGeneric(int i) => 1;
			public static int GenericVsNonGeneric<T>(T i) => 2;

			public static int WithParamsArray(params int[] i) => 3;
			public static int WithParamsArray(int i, params int[] j) => 4;
			public static int WithParamsArray(int i, int j) => 5;

			public static int WithParamsArray2(string str, Exception e) => 6;
			public static int WithParamsArray2(string str, Exception e, params string[] args) => 7;
			public static int WithParamsArray2(string str, Exception e, params int[] args) => 8;


			public static List<T> Array<T>(params T[] array)
			{
				return new List<T>(array);
			}
		}

		[Test]
		public void Method_overload_generic_vs_non_generic()
		{
			var target = new Interpreter();
			target.Reference(typeof(Utils));

			Assert.AreEqual(1, target.Eval("Utils.GenericVsNonGeneric(12345)"));
			Assert.AreEqual(2, target.Eval("Utils.GenericVsNonGeneric('a')"));
		}

		[Test]
		public void Method_overload_params_array()
		{
			var target = new Interpreter();
			target.Reference(typeof(Utils));

			var arr = new int[] { 2 };
			target.SetVariable("arr", arr);

			Assert.AreEqual(3, target.Eval("Utils.WithParamsArray(arr)"));
			Assert.AreEqual(4, target.Eval("Utils.WithParamsArray(1)"));
			Assert.AreEqual(4, target.Eval("Utils.WithParamsArray(1, arr)"));
			Assert.AreEqual(5, target.Eval("Utils.WithParamsArray(1, 2)"));
			Assert.AreEqual(4, target.Eval("Utils.WithParamsArray(1, 2, 3)"));
			Assert.AreEqual(4, target.Eval("Utils.WithParamsArray(1, 2, 3, 4)"));
		}

		[Test]
		public void Method_overload_params_array_2()
		{
			var target = new Interpreter();
			target.Reference(typeof(Utils));

			var str = "str";
			var e = new Exception();
			var intg = 4;
			target.SetVariable("str", str);
			target.SetVariable("e", e);
			target.SetVariable("intg", intg);
			Assert.AreEqual(6, target.Eval("Utils.WithParamsArray2(str, e)"));
			Assert.AreEqual(7, target.Eval("Utils.WithParamsArray2(str, e, str, str)"));
			Assert.AreEqual(8, target.Eval("Utils.WithParamsArray2(str, e, intg, intg)"));
		}

		private interface MyTestInterface
		{
		}

		private class MyTestInterfaceImp : MyTestInterface
		{

		}

		private class MyTestService
		{
			public DateTime AField = DateTime.Now;
			public DateTime AFIELD = DateTime.UtcNow;
			public DateTime Today = DateTime.Today;

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

			public T MethodWithGenericParamAndDefault<T>(T a, T b = default)
			{
				return a;
			}

			public T MethodWithGenericParamAndDefault1Levels<T>(T a, List<T> b = default)
			{
				return a;
			}

			public T MethodWithGenericParamAndDefault2Levels<T>(T a, List<List<T>> b = default)
			{
				return a;
			}

			public T MethodWithGenericParamAndDefault2Levels<T, T2>(T a, T2 b, List<T> c = default, List<List<T2>> d = default)
			{
				return a;
			}

			public string MethodWithOptionalParam(string param1, string param2 = "2", string param3 = "3")
			{
				return string.Format("{0} {1} {2}", param1, param2, param3);
			}

			public string MethodWithOptionalNullParam(string param1, string param2 = null)
			{
				return string.Format("{0} {1}", param1, param2 ?? "(null)");
			}

			public DateTime this[int i]
			{
				get { return AField.AddDays(i); }
			}

			public DateTime this[long i]
			{
				set { AField = value.AddSeconds(i); }
			}

			public DateTime this[short i]
			{
				get => AField.AddYears(i);
				set => AField = value.AddYears(i);
			}

			public TimeSpan this[DateTime dateTime, int i = 3]
			{
				get => AField.AddDays(-i).Subtract(dateTime);
				set => AField = dateTime.AddDays(i).Add(value);
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

			public T GenericMethodWithConstraint<T>(T input) where T : class
			{
				return input;
			}
		}

		private class MyTestServiceCaseInsensitive
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
