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

			Assert.That(target.Eval("x.HelloWorld()"), Is.EqualTo(x.HelloWorld()));
			Assert.That(target.Eval("x.AProperty"), Is.EqualTo(x.AProperty));
			Assert.That(target.Eval("x.AField"), Is.EqualTo(x.AField));
		}

		[Ignore("See issue 65")]
		[Test]
		public void Null_conditional_property()
		{
			var target = new Interpreter().SetVariable("x", null, typeof(MyTestService));
			Assert.That(target.Eval("x?.AProperty"), Is.Null);
		}

		[Test]
		public void Indexer_Getter()
		{
			var target = new Interpreter();

			var x = "ciao";
			target.SetVariable("x", x);
			var y = new MyTestService();
			target.SetVariable("y", y);
			var z = new[] { 7, 8, 9, 10 };
			target.SetVariable("z", z);

			Assert.That(target.Eval("x[2]"), Is.EqualTo(x[2]));
			Assert.That(target.Eval("y[2]"), Is.EqualTo(y[2]));
			Assert.That(target.Eval("y[2].ToString()"), Is.EqualTo(y[2].ToString()));
			Assert.That(target.Eval("y[(Int16)2]"), Is.EqualTo(y[(short)2]));
			Assert.That(target.Eval("z[2]"), Is.EqualTo(z[2]));
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
			Assert.That(x.ToString(), Is.EqualTo("tire"));
			target.Eval("y[(Int16)9] = y.Today");
			Assert.That(y.Today.AddYears(9), Is.EqualTo(y.AField));
			target.Eval("y[(Int64)7] = y.Today");
			Assert.That(y.Today.AddSeconds(7), Is.EqualTo(y.AField));
			target.Eval("z[2] = 4");
			Assert.That(z, Is.EqualTo(new[] { 7, 8, 4, 10 }));
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

			Assert.That(target.Eval("x[2]"), Is.EqualTo(x[2]));
			Assert.That(target.Eval("y[\"second\"]"), Is.EqualTo(y["second"]));

			target.Eval("x[2] = 1");
			Assert.That(new List<int> { 3, 4, 1, 6 }, Is.EqualTo(x));
			target.Eval("y[\"second\"] = 2000");
			Assert.That(new Dictionary<string, int> { { "first", 1 }, { "second", 2000 }, { "third", 3 } }, Is.EqualTo(y));
		}

		[Test]
		public void Indexer_Getter_MultiDimensional()
		{
			var target = new Interpreter();

			var x = new[,] { { 11, 12, 13, 14 }, { 21, 22, 23, 24 }, { 31, 32, 33, 34 } };
			target.SetVariable("x", x);
			var y = new MyTestService();
			target.SetVariable("y", y);

			Assert.That(target.Eval("x[1, 2]"), Is.EqualTo(x[1, 2]));
			Assert.That(target.Eval("y[y.Today, 2]"), Is.EqualTo(y[y.Today, 2]));
			Assert.That(target.Eval("y[y.Today]"), Is.EqualTo(y[y.Today]));
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
			Assert.That(new[,] { { 11, 12, 13, 14 }, { 21, 22, 7, 24 }, { 31, 32, 33, 34 } }, Is.EqualTo(x));
			target.Eval("y[y.Today, 2] = span");
			Assert.That(y.Today.AddDays(2).Add(span), Is.EqualTo(y.AField));
			target.Eval("y[y.Today] = span");
			Assert.That(y.Today.AddDays(3).Add(span), Is.EqualTo(y.AField));
		}

		[Test]
		public void String_format()
		{
			var target = new Interpreter();

			Assert.That(target.Eval("string.Format(\"ciao {0}, today is {1}\", \"mondo\", DateTime.Today.ToString())"), Is.EqualTo(string.Format("ciao {0}, today is {1}", "mondo", DateTime.Today)));
		}

		[Test]
		public void String_format_with_type_conversion()
		{
			var target = new Interpreter();

			Assert.That(target.Eval("string.Format(\"ciao {0}, today is {1}\", \"mondo\", DateTime.Today)"), Is.EqualTo(string.Format("ciao {0}, today is {1}", "mondo", DateTime.Today)));
		}

		[Test]
		public void String_format_with_empty_string()
		{
			var target = new Interpreter();

			Assert.That(target.Eval("string.Format(\"ciao {0}\", \"\")"), Is.EqualTo(string.Format("ciao {0}", "")));
		}

		[Test]
		public void String_format_With_Object_Params()
		{
			var target = new Interpreter();

			Assert.That(target.Eval("string.Format(\"ciao {0}, today is {1}\", \"mondo\", DateTime.Today)"), Is.EqualTo(string.Format("ciao mondo, today is {0}", DateTime.Today)));
		}

		[Test]
		public void Methods_Fields_And_Properties_By_Default_Are_Case_Sensitive()
		{
			var target = new Interpreter();

			var x = new MyTestService();
			var parameters = new[] {
							new Parameter("x", x.GetType(), x)
							};

			Assert.That(target.Eval("x.HelloWorld()", parameters), Is.EqualTo(x.HelloWorld()));
			Assert.That(target.Eval("x.HELLOWORLD()", parameters), Is.EqualTo(x.HELLOWORLD()));
			Assert.That(target.Eval("x.AProperty", parameters), Is.EqualTo(x.AProperty));
			Assert.That(target.Eval("x.APROPERTY", parameters), Is.EqualTo(x.APROPERTY));
			Assert.That(target.Eval("x.AField", parameters), Is.EqualTo(x.AField));
			Assert.That(target.Eval("x.AFIELD", parameters), Is.EqualTo(x.AFIELD));
		}

		[Test]
		public void Methods_Fields_And_Properties_Can_Be_Case_Insensitive()
		{
			var target = new Interpreter(InterpreterOptions.DefaultCaseInsensitive);

			var x = new MyTestServiceCaseInsensitive();
			var parameters = new[] {
							new Parameter("x", x.GetType(), x)
							};

			Assert.That(target.Eval("x.AMethod()", parameters), Is.EqualTo(x.AMethod()));
			Assert.That(target.Eval("x.AMETHOD()", parameters), Is.EqualTo(x.AMethod()));
			Assert.That(target.Eval("x.AProperty", parameters), Is.EqualTo(x.AProperty));
			Assert.That(target.Eval("x.APROPERTY", parameters), Is.EqualTo(x.AProperty));
			Assert.That(target.Eval("x.AField", parameters), Is.EqualTo(x.AField));
			Assert.That(target.Eval("x.AFIELD", parameters), Is.EqualTo(x.AField));
		}

		[Test]
		public void Void_Method()
		{
			var service = new MyTestService();
			var target = new Interpreter()
											.SetVariable("service", service);

			Assert.That(service.VoidMethodCalls, Is.EqualTo(0));
			target.Eval("service.VoidMethod()");
			Assert.That(service.VoidMethodCalls, Is.EqualTo(1));

			Assert.That(target.Parse("service.VoidMethod()").ReturnType, Is.EqualTo(typeof(void)));
		}

		[Test]
		public void ToString_Method_on_a_custom_type()
		{
			var service = new MyTestService();
			var target = new Interpreter()
											.SetVariable("service", service);

			Assert.That(target.Eval("service.ToString()"), Is.EqualTo("DynamicExpresso.UnitTest.MemberInvocationTest+MyTestService"));
		}

		[Test]
		public void ToString_Method_on_a_custom_interface_type()
		{
			MyTestInterface service = new MyTestInterfaceImp();
			var target = new Interpreter()
											.SetVariable("service", service, typeof(MyTestInterface));

			Assert.That(target.Eval("service.ToString()"), Is.EqualTo("DynamicExpresso.UnitTest.MemberInvocationTest+MyTestInterfaceImp"));
		}

		[Test]
		public void ToString_Method_on_a_primitive_type()
		{
			var target = new Interpreter();
			Assert.That(target.Eval("(3).ToString()"), Is.EqualTo("3"));
		}

		[Test]
		public void GetType_Method_on_a_custom_type()
		{
			var service = new MyTestService();
			var target = new Interpreter()
											.SetVariable("service", service);

			Assert.That(target.Eval("service.GetType()"), Is.EqualTo(typeof(MyTestService)));
		}

		[Test]
		public void GetType_Method_on_a_custom_interface_type()
		{
			MyTestInterface service = new MyTestInterfaceImp();
			var target = new Interpreter()
											.SetVariable("service", service, typeof(MyTestInterface));

			Assert.That(target.Eval("service.GetType()"), Is.EqualTo(typeof(MyTestInterfaceImp)));
		}

		[Test]
		public void GetType_Method_on_a_primitive_type()
		{
			var target = new Interpreter();
			Assert.That(target.Eval("(3).GetType()"), Is.EqualTo((3).GetType()));
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

			Assert.That(target.Eval("x.MethodWithNullableParam(y, z)", parameters), Is.EqualTo(x.MethodWithNullableParam(y, z)));
			Assert.That(target.Eval("x.MethodWithNullableParam(y, w)", parameters), Is.EqualTo(x.MethodWithNullableParam(y, w)));
			Assert.That(target.Eval("x.MethodWithNullableParam(y, 30)", parameters), Is.EqualTo(x.MethodWithNullableParam(y, 30)));
			Assert.That(target.Eval("x.MethodWithNullableParam(y, null)", parameters), Is.EqualTo(x.MethodWithNullableParam(y, null)));
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

			Assert.That(target.Eval("x.MethodWithGenericParam(x)", parameters), Is.EqualTo(x.MethodWithGenericParam(x)));
			Assert.That(target.Eval("x.MethodWithGenericParam(y)", parameters), Is.EqualTo(x.MethodWithGenericParam(y)));
			Assert.That(target.Eval("x.MethodWithGenericParam(z)", parameters), Is.EqualTo(x.MethodWithGenericParam(z)));
			Assert.That(target.Eval("x.MethodWithGenericParam(w)", parameters), Is.EqualTo(x.MethodWithGenericParam(w)));

			Assert.That(target.Eval("x.MethodWithGenericParam(y, x)", parameters), Is.EqualTo(x.MethodWithGenericParam(y, x)));
			Assert.That(target.Eval("x.MethodWithGenericParam(y, y)", parameters), Is.EqualTo(x.MethodWithGenericParam(y, y)));
			Assert.That(target.Eval("x.MethodWithGenericParam(y, z)", parameters), Is.EqualTo(x.MethodWithGenericParam(y, z)));
			Assert.That(target.Eval("x.MethodWithGenericParam(y, w)", parameters), Is.EqualTo(x.MethodWithGenericParam(y, w)));

			Assert.That(target.Eval("x.MethodWithGenericParamAndDefault(y,y)", parameters), Is.EqualTo(x.MethodWithGenericParamAndDefault(y, y)));
			Assert.That(target.Eval("x.MethodWithGenericParamAndDefault(y)", parameters), Is.EqualTo(x.MethodWithGenericParamAndDefault(y)));
			Assert.That(target.Eval("x.MethodWithGenericParamAndDefault1Levels(y)", parameters), Is.EqualTo(x.MethodWithGenericParamAndDefault1Levels(y)));
			Assert.That(target.Eval("x.MethodWithGenericParamAndDefault2Levels(y)", parameters), Is.EqualTo(x.MethodWithGenericParamAndDefault2Levels(y)));
			Assert.That(target.Eval("x.MethodWithGenericParamAndDefault2Levels(y, w)", parameters), Is.EqualTo(x.MethodWithGenericParamAndDefault2Levels(y, w)));
		}

		[Test]
		public void Method_with_generic_constraints()
		{
			var target = new Interpreter();

			var x = new MyTestService();
			target.SetVariable("x", x);

			Assert.That(target.Eval("x.GenericMethodWithConstraint(\"works\")"), Is.EqualTo("works"));
			Assert.Throws<NoApplicableMethodException>(() => target.Eval("x.GenericMethodWithConstraint(5)"), "This shouldn't throw a System.ArgumentException \"Violates the constraint of type 'T'\"");
		}

		[Test]
		public void Method_with_params_array()
		{
			var target = new Interpreter();

			var x = new MyTestService();

			target.SetVariable("x", x);

			Assert.That(x.MethodWithParamsArrayCalls, Is.EqualTo(0));

			Assert.That(target.Eval("x.MethodWithParamsArray(DateTime.Now, 2, 1, 34)"), Is.EqualTo(x.MethodWithParamsArray(DateTime.Now, 2, 1, 34)));
			Assert.That(x.MethodWithParamsArrayCalls, Is.EqualTo(2));

			var myParamArray = new int[] { 2, 1, 34 };
			target.SetVariable("myParamArray", myParamArray);

			Assert.That(target.Eval("x.MethodWithParamsArray(DateTime.Now, myParamArray)"), Is.EqualTo(x.MethodWithParamsArray(DateTime.Now, myParamArray)));
			Assert.That(x.MethodWithParamsArrayCalls, Is.EqualTo(4));
		}

		[Test]
		public void ParamsArray_methods_are_not_called_when_there_is_an_exact_method_match()
		{
			var target = new Interpreter();

			var x = new MyTestService();

			target.SetVariable("x", x);

			target.Eval("x.AmbiguousMethod(DateTime.Now, 2, 3)");
			Assert.That(x.AmbiguousMethod_NormalCalls, Is.EqualTo(1));
			Assert.That(x.AmbiguousMethod_ParamsArrayCalls, Is.EqualTo(0));

			target.Eval("x.AmbiguousMethod(DateTime.Now, 2, 3, 4)");
			Assert.That(x.AmbiguousMethod_NormalCalls, Is.EqualTo(1));
			Assert.That(x.AmbiguousMethod_ParamsArrayCalls, Is.EqualTo(1));
		}

		[Test]
		public void Overload_paramsArray_methods_with_compatible_type_params()
		{
			var target = new Interpreter();
			var x = new MyTestService();
			target.SetVariable("x", x);
			Assert.That(target.Eval("x.OverloadMethodWithParamsArray(2, 3, 1)"), Is.EqualTo(3));
		}


		[Test]
		public void Generic_method_with_params()
		{
			var target = new Interpreter();
			target.Reference(typeof(Utils));

			var listInt = target.Eval<List<int>>("Utils.Array(1, 2, 3)");
			Assert.That(listInt, Is.EqualTo(new[] { 1, 2, 3 }));

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

			Assert.That(target.Eval("x.MethodWithOptionalParam(y)", parameters), Is.EqualTo(x.MethodWithOptionalParam(y)));
			Assert.That(target.Eval("x.MethodWithOptionalParam(y, z)", parameters), Is.EqualTo(x.MethodWithOptionalParam(y, z)));
			Assert.That(target.Eval("x.MethodWithOptionalParam(z, y)", parameters), Is.EqualTo(x.MethodWithOptionalParam(z, y)));
			Assert.That(target.Eval("x.MethodWithOptionalParam(y, z, w)", parameters), Is.EqualTo(x.MethodWithOptionalParam(y, z, w)));
			Assert.That(target.Eval("x.MethodWithOptionalParam(w, y, z)", parameters), Is.EqualTo(x.MethodWithOptionalParam(w, y, z)));
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

			Assert.That(target.Eval("x.MethodWithOptionalNullParam(y)", parameters), Is.EqualTo(x.MethodWithOptionalNullParam(y)));
			Assert.That(target.Eval("x.MethodWithOptionalNullParam(y, z)", parameters), Is.EqualTo(x.MethodWithOptionalNullParam(y, z)));
		}

		[Test]
		public void Chaining_Methods()
		{
			var x = new MyTestService();

			var target = new Interpreter();
			target.SetVariable("x", x);

			Assert.That(target.Eval("x.HelloWorld().ToUpper()"), Is.EqualTo(x.HelloWorld().ToUpper()));
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

			Assert.That(target.Eval("Utils.GenericVsNonGeneric(12345)"), Is.EqualTo(1));
			Assert.That(target.Eval("Utils.GenericVsNonGeneric('a')"), Is.EqualTo(2));
		}

		[Test]
		public void Method_overload_params_array()
		{
			var target = new Interpreter();
			target.Reference(typeof(Utils));

			var arr = new int[] { 2 };
			target.SetVariable("arr", arr);

			Assert.That(target.Eval("Utils.WithParamsArray(arr)"), Is.EqualTo(3));
			Assert.That(target.Eval("Utils.WithParamsArray(1)"), Is.EqualTo(4));
			Assert.That(target.Eval("Utils.WithParamsArray(1, arr)"), Is.EqualTo(4));
			Assert.That(target.Eval("Utils.WithParamsArray(1, 2)"), Is.EqualTo(5));
			Assert.That(target.Eval("Utils.WithParamsArray(1, 2, 3)"), Is.EqualTo(4));
			Assert.That(target.Eval("Utils.WithParamsArray(1, 2, 3, 4)"), Is.EqualTo(4));
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
			Assert.That(target.Eval("Utils.WithParamsArray2(str, e)"), Is.EqualTo(6));
			Assert.That(target.Eval("Utils.WithParamsArray2(str, e, str, str)"), Is.EqualTo(7));
			Assert.That(target.Eval("Utils.WithParamsArray2(str, e, intg, intg)"), Is.EqualTo(8));
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
