using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using DynamicExpresso.Exceptions;
using Microsoft.CSharp.RuntimeBinder;
using NUnit.Framework;

namespace DynamicExpresso.UnitTest
{
	[TestFixture]
	public class DynamicTest
	{
		[Test]
		public void Get_Property_of_an_ExpandoObject()
		{
			dynamic dyn = new ExpandoObject();
			dyn.Foo = "bar";

			var interpreter = new Interpreter()
				.SetVariable("dyn", (object)dyn);

			Assert.That(interpreter.Eval("dyn.Foo"), Is.EqualTo(dyn.Foo));
		}

		[Test]
		public void Set_Property_of_an_ExpandoObject()
		{
			dynamic dyn = new ExpandoObject();
			var interpreter = new Interpreter(InterpreterOptions.Default | InterpreterOptions.LateBindObject)
				.SetVariable("dyn", (object)dyn);

			interpreter.Eval("dyn.Foo = 10");

			Assert.That(dyn.Foo, Is.EqualTo(10));
		}

		[Test]
		public void Get_Property_of_a_nested_anonymous()
		{
			dynamic dyn = new ExpandoObject();
			dyn.Sub = new { Foo = new { Bar = new { Foo2 = "bar" } } };
			var interpreter = new Interpreter().SetVariable("dyn", (object)dyn);
			Assert.That(interpreter.Eval("dyn.Sub.Foo.Bar.Foo2"), Is.EqualTo(dyn.Sub.Foo.Bar.Foo2));
		}

		[Test]
		public void Set_Property_of_a_nested_ExpandoObject()
		{
			dynamic dyn = new ExpandoObject();
			dyn.Sub = new ExpandoObject();
			dyn.Sub.Foo = "bar";

			var interpreter = new Interpreter().SetVariable("dyn", (object)dyn);

			interpreter.Eval("dyn.Sub.Foo = \"foobar\"");

			Assert.That(dyn.Sub.Foo, Is.EqualTo("foobar"));
		}

		[Test]
		public void Get_Property_of_a_nested_ExpandoObject()
		{
			dynamic dyn = new ExpandoObject();
			dyn.Sub = new ExpandoObject();
			dyn.Sub.Foo = "bar";

			var interpreter = new Interpreter()
					.SetVariable("dyn", (object)dyn);

			Assert.That(interpreter.Eval("dyn.Sub.Foo"), Is.EqualTo(dyn.Sub.Foo));
		}

		//[Test]
		//public void Set_Property_of_an_ExpandoObject()
		//{
		//	dynamic dyn = new ExpandoObject();

		//	var interpreter = new Interpreter()
		//		.SetVariable("dyn", dyn);

		//	interpreter.Eval("dyn.Foo = 6");

		//	Assert.That(dyn.Foo, Is.EqualTo(6));
		//}

		[Test]
		public void Standard_properties_have_precedence_over_dynamic_properties()
		{
			var dyn = new TestDynamicClass();
			dyn.RealProperty = "bar";

			var interpreter = new Interpreter()
				.SetVariable("dyn", dyn);

			Assert.That(interpreter.Eval("dyn.RealProperty"), Is.EqualTo(dyn.RealProperty));
		}

		[Test]
		public void Invoke_Method_of_an_ExpandoObject()
		{
			dynamic dyn = new ExpandoObject();
			dyn.Foo = new Func<string>(() => "bar");

			var interpreter = new Interpreter()
				.SetVariable("dyn", dyn);

			Assert.That(interpreter.Eval("dyn.Foo()"), Is.EqualTo(dyn.Foo()));
		}

		[Test]
		public void Invoke_Method_of_a_nested_ExpandoObject()
		{
			dynamic dyn = new ExpandoObject();
			dyn.Sub = new ExpandoObject();
			dyn.Sub.Foo = new Func<string>(() => "bar");

			var interpreter = new Interpreter()
					.SetVariable("dyn", dyn);

			Assert.That(interpreter.Eval("dyn.Sub.Foo()"), Is.EqualTo(dyn.Sub.Foo()));
		}

		[Test]
		public void Invoke_Method_of_a_nested_ExpandoObject_WithAnonymousType()
		{
			dynamic dyn = new ExpandoObject();
			dyn.Sub = new ExpandoObject();
			dyn.Sub.Foo = new { Func = new Func<string>(() => "bar") };

			var interpreter = new Interpreter()
					.SetVariable("dyn", dyn);

			Assert.That(interpreter.Eval("dyn.Sub.Foo.Func()"), Is.EqualTo(dyn.Sub.Foo.Func()));
		}

		[Test]
		public void Standard_methods_have_precedence_over_dynamic_methods()
		{
			var dyn = new TestDynamicClass();

			var interpreter = new Interpreter()
				.SetVariable("dyn", dyn);

			Assert.That(interpreter.Eval("dyn.ToString()"), Is.EqualTo(dyn.ToString()));
		}

		[Test]
		public void Case_Insensitive_Dynamic_Members()
		{
			dynamic dyn = new ExpandoObject();
			dyn.Bar = 10;

			var interpreter = new Interpreter();
			Assert.Throws<RuntimeBinderException>(() => interpreter.Eval("dyn.BAR", new Parameter("dyn", dyn)));
		}

		[Test]
		public void Get_value_of_a_nested_array()
		{
			dynamic dyn = new ExpandoObject();
			dyn.Sub = new int[] { 42 };
			var interpreter = new Interpreter().SetVariable("dyn", (object)dyn);
			Assert.That(interpreter.Eval("dyn.Sub[0]"), Is.EqualTo(dyn.Sub[0]));
		}

		[Test]
		public void Get_value_of_a_nested_array_from_anonymous_type()
		{
			dynamic dyn = new ExpandoObject();
			dyn.Sub = new { Foo = new int[] { 42 }, Bar = new { Sub = new int[] { 43 } } };
			var interpreter = new Interpreter().SetVariable("dyn", (object)dyn);
			Assert.That(interpreter.Eval("dyn.Sub.Foo[0]"), Is.EqualTo(dyn.Sub.Foo[0]));
			Assert.That(interpreter.Eval("dyn.Sub.Bar.Sub[0]"), Is.EqualTo(dyn.Sub.Bar.Sub[0]));
			Assert.That(interpreter.Eval("dyn.Sub.Bar.Sub.Length"), Is.EqualTo(dyn.Sub.Bar.Sub.Length));
		}

		[Test]
		public void Get_value_of_an_array_of_anonymous_type()
		{
			dynamic dyn = new ExpandoObject();
			var anonType1 = new { Foo = string.Empty };
			var anonType2 = new { Foo = "string.Empty" };
			var nullAnonType = anonType1;
			nullAnonType = null;
			dyn.Sub = new
			{
				Arg1 = anonType1,
				Arg2 = anonType2,
				Arg3 = nullAnonType,
				Arr = new[] { anonType1, anonType2, nullAnonType },
				ObjArr = new object[] { "Test", anonType1 }
			};
			var interpreter = new Interpreter().SetVariable("dyn", (object)dyn);
			Assert.That(interpreter.Eval("dyn.Sub.Arg1.Foo"), Is.SameAs(dyn.Sub.Arg1.Foo));
			Assert.That(interpreter.Eval("dyn.Sub.Arg2.Foo"), Is.SameAs(dyn.Sub.Arg2.Foo));
			Assert.Throws<RuntimeBinderException>(() => Console.WriteLine(dyn.Sub.Arg3.Foo));
			Assert.Throws<RuntimeBinderException>(() => interpreter.Eval("dyn.Sub.Arg3.Foo"));
			Assert.That(interpreter.Eval("dyn.Sub.Arr[0].Foo"), Is.SameAs(dyn.Sub.Arr[0].Foo));
			Assert.That(interpreter.Eval("dyn.Sub.Arr[1].Foo"), Is.SameAs(dyn.Sub.Arr[1].Foo));

			Assert.Throws<RuntimeBinderException>(() => Console.WriteLine(dyn.Sub.Arr[2].Foo));
			Assert.Throws<RuntimeBinderException>(() => interpreter.Eval("dyn.Sub.Arr[2].Foo"));

			Assert.That(interpreter.Eval("dyn.Sub.ObjArr[0]"), Is.SameAs(dyn.Sub.ObjArr[0]));
			Assert.That(interpreter.Eval("dyn.Sub.ObjArr[0].Length"), Is.EqualTo(dyn.Sub.ObjArr[0].Length));

			Assert.That(interpreter.Eval("dyn.Sub.ObjArr[1]"), Is.SameAs(dyn.Sub.ObjArr[1]));
			Assert.That(interpreter.Eval("dyn.Sub.ObjArr[1].Foo"), Is.SameAs(dyn.Sub.ObjArr[1].Foo));
		}

		[Test]
		public void Set_value_of_a_dynamic_object()
		{
			dynamic dyn = new ExpandoObject();
			dyn.Foo = new int[5];
			dyn.Bar = new Dictionary<string, int>();

			var interpreter = new Interpreter().SetVariable("dyn", (object)dyn);
			interpreter.Eval("dyn.Foo[2] = 5");
			interpreter.Eval("dyn.Bar[\"foo\"] = 50");

			Assert.That(dyn.Foo[2], Is.EqualTo(5));
			Assert.That(dyn.Bar["foo"], Is.EqualTo(50));
		}

		[Test]
		public void Get_value_of_a_nested_array_error()
		{
			dynamic dyn = new ExpandoObject();
			dyn.Sub = new int[] { 42 };
			dyn.Bar = 123;
			var interpreter = new Interpreter().SetVariable("dyn", (object)dyn);
			Assert.Throws<RuntimeBinderException>(() => interpreter.Eval("dyn.Bar[0]")); // use index for a property that is not an array
			Assert.Throws<RuntimeBinderException>(() => interpreter.Eval("dyn.Sub[\"hello\"]")); // use as an index an invalid type (e.g. string)
			Assert.Throws<ParseException>(() => interpreter.Eval("dyn.Sub[0"));// pass some invalid syntax
			Assert.Throws<ParseException>(() => interpreter.Eval("dyn.Sub 0]")); // pass some invalid syntax
			Assert.Throws<ParseException>(() => interpreter.Eval("dyn.Sub[[0]]")); // pass some invalid syntax
			Assert.Throws<IndexOutOfRangeException>(() => interpreter.Eval("dyn.Sub[1]")); // get an out of bound element
		}

		[Test]
		public void Test_With_Standard_Object()
		{
			var myInstance = DateTime.Now;

			var methodInfo = myInstance.GetType().GetMethod("ToUniversalTime");

			var methodCallExpression = Expression.Call(Expression.Constant(myInstance), methodInfo);
			var expression = Expression.Lambda(methodCallExpression);

			Assert.That(expression.Compile().DynamicInvoke(), Is.EqualTo(myInstance.ToUniversalTime()));
		}

		[Test]
		public void Test_With_Dynamic_Object()
		{
			dynamic myInstance = new ExpandoObject();
			myInstance.MyMethod = new Func<string>(() => "hello world");

			var binder = Binder.InvokeMember(
				CSharpBinderFlags.None,
				"MyMethod",
				null,
				this.GetType(),
				new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.Constant, null) });

			var methodCallExpression = Expression.Dynamic(binder, typeof(object), Expression.Constant(myInstance));
			var expression = Expression.Lambda(methodCallExpression);

			Assert.That(expression.Compile().DynamicInvoke(), Is.EqualTo(myInstance.MyMethod()));
		}

		[Test]
		public void Test_With_Dynamic_Object_By_Index_Access()
		{
			DynamicIndexAccess globals = new DynamicIndexAccess();
			Interpreter interpreter = new Interpreter()
				.SetVariable("Values", new DynamicIndexAccess());

			Assert.That(interpreter.Eval<string>("Values[\"Hello\"]"), Is.EqualTo(globals.Values["Hello"]));
		}

		[Test]
		public void Comparison_with_dynamic_properties()
		{
			dynamic dyn = new ExpandoObject();
			dyn.Foo = 500;

			var interpreter = new Interpreter()
				.SetVariable("dyn", (object)dyn);

			Assert.That(interpreter.Eval("dyn.Foo == 500"), Is.EqualTo(dyn.Foo == 500));
			Assert.That(interpreter.Eval("500 == dyn.Foo"), Is.EqualTo(500 == dyn.Foo));

			Assert.That(interpreter.Eval("dyn.Foo != 200"), Is.EqualTo(dyn.Foo != 200));
			Assert.That(interpreter.Eval("200 != dyn.Foo"), Is.EqualTo(200 != dyn.Foo));

			Assert.That(interpreter.Eval("dyn.Foo > 200"), Is.EqualTo(dyn.Foo > 200));
			Assert.That(interpreter.Eval("600 > dyn.Foo"), Is.EqualTo(600 > dyn.Foo));

			Assert.That(interpreter.Eval("dyn.Foo < 600"), Is.EqualTo(dyn.Foo < 600));
			Assert.That(interpreter.Eval("200 < dyn.Foo"), Is.EqualTo(200 < dyn.Foo));

			Assert.That(interpreter.Eval("dyn.Foo >= 500"), Is.EqualTo(dyn.Foo >= 500));
			Assert.That(interpreter.Eval("500 >= dyn.Foo"), Is.EqualTo(500 >= dyn.Foo));

			Assert.That(interpreter.Eval("dyn.Foo <= 500"), Is.EqualTo(dyn.Foo <= 500));
			Assert.That(interpreter.Eval("500 <= dyn.Foo"), Is.EqualTo(500 <= dyn.Foo));

			Assert.That(interpreter.Eval("dyn.Foo + 500"), Is.EqualTo(dyn.Foo + 500));
			Assert.That(interpreter.Eval("500 + dyn.Foo"), Is.EqualTo(500 + dyn.Foo));
		}

		[Test]
		public void Conditional_with_dynamic_properties()
		{
			dynamic dyn = new ExpandoObject();
			dyn.Foo = 500;
			dyn.Bar = 100;

			var interpreter = new Interpreter()
				.SetVariable("dyn", (object)dyn);

			Assert.That(interpreter.Eval("dyn.Bar>0?100:200"), Is.EqualTo(dyn.Bar > 0 ? 100 : 200));
		}

		[Test]
		public void AndAlso_with_dynamic_properties()
		{
			dynamic dyn = new ExpandoObject();
			dyn.Foo = 500;
			dyn.Bar = 100;

			var interpreter = new Interpreter()
				.SetVariable("dyn", (object)dyn);

			Assert.That(interpreter.Eval("dyn.Foo == 500 && dyn.Bar == 100"), Is.EqualTo(dyn.Foo == 500 && dyn.Bar == 100));
			Assert.That(interpreter.Eval("dyn.Foo == 500 && dyn.Bar != 100"), Is.EqualTo(dyn.Foo == 500 && dyn.Bar != 100));
			Assert.That(interpreter.Eval("dyn.Foo != 500 && dyn.Bar == 100"), Is.EqualTo(dyn.Foo != 500 && dyn.Bar == 100));
			Assert.That(interpreter.Eval("dyn.Foo != 500 && dyn.Bar != 100"), Is.EqualTo(dyn.Foo != 500 && dyn.Bar != 100));

			//check non dynamic right side
			Assert.That(interpreter.Eval("dyn.Foo == 500 && 100 == 100"), Is.EqualTo(dyn.Foo == 500 && 100 == 100));
			Assert.That(interpreter.Eval("dyn.Foo == 500 && 100 != 100"), Is.EqualTo(dyn.Foo == 500 && 100 != 100));
			Assert.That(interpreter.Eval("dyn.Foo != 500 && 100 == 100"), Is.EqualTo(dyn.Foo != 500 && 100 == 100));
			Assert.That(interpreter.Eval("dyn.Foo != 500 && 100 != 100"), Is.EqualTo(dyn.Foo != 500 && 100 != 100));

			//check non dynamic left side
			Assert.That(interpreter.Eval("100 == 100 && dyn.Foo == 500"), Is.EqualTo(100 == 100 && dyn.Foo == 500));
			Assert.That(interpreter.Eval("100 != 100 && dyn.Foo == 500"), Is.EqualTo(100 != 100 && dyn.Foo == 500));
			Assert.That(interpreter.Eval("100 == 100 && dyn.Foo != 500"), Is.EqualTo(100 == 100 && dyn.Foo != 500));
			Assert.That(interpreter.Eval("100 != 100 && dyn.Foo != 500"), Is.EqualTo(100 != 100 && dyn.Foo != 500));
		}

		[Test]
		public void OrElse_with_dynamic_properties()
		{
			dynamic dyn = new ExpandoObject();
			dyn.Foo = 500;
			dyn.Bar = 100;

			var interpreter = new Interpreter()
				.SetVariable("dyn", (object)dyn);

			Assert.That(interpreter.Eval("dyn.Foo == 500 || dyn.Bar == 100"), Is.EqualTo(dyn.Foo == 500 || dyn.Bar == 100));
			Assert.That(interpreter.Eval("dyn.Foo == 500 || dyn.Bar != 100"), Is.EqualTo(dyn.Foo == 500 || dyn.Bar != 100));
			Assert.That(interpreter.Eval("dyn.Foo != 500 || dyn.Bar == 100"), Is.EqualTo(dyn.Foo != 500 || dyn.Bar == 100));
			Assert.That(interpreter.Eval("dyn.Foo != 500 || dyn.Bar != 100"), Is.EqualTo(dyn.Foo != 500 || dyn.Bar != 100));

			//check non dynamic right side
			Assert.That(interpreter.Eval("dyn.Foo == 500 || 100 == 100"), Is.EqualTo(dyn.Foo == 500 || 100 == 100));
			Assert.That(interpreter.Eval("dyn.Foo == 500 || 100 != 100"), Is.EqualTo(dyn.Foo == 500 || 100 != 100));
			Assert.That(interpreter.Eval("dyn.Foo != 500 || 100 == 100"), Is.EqualTo(dyn.Foo != 500 || 100 == 100));
			Assert.That(interpreter.Eval("dyn.Foo != 500 || 100 != 100"), Is.EqualTo(dyn.Foo != 500 || 100 != 100));

			//check non dynamic left side
			Assert.That(interpreter.Eval("100 == 100 || dyn.Foo == 500"), Is.EqualTo(100 == 100 || dyn.Foo == 500));
			Assert.That(interpreter.Eval("100 != 100 || dyn.Foo == 500"), Is.EqualTo(100 != 100 || dyn.Foo == 500));
			Assert.That(interpreter.Eval("100 == 100 || dyn.Foo != 500"), Is.EqualTo(100 == 100 || dyn.Foo != 500));
			Assert.That(interpreter.Eval("100 != 100 || dyn.Foo != 500"), Is.EqualTo(100 != 100 || dyn.Foo != 500));
		}

		[Test]
		public void Math_with_dynamic_properties()
		{
			dynamic dyn = new ExpandoObject();
			dyn.Foo = 500;

			var interpreter = new Interpreter()
				.SetVariable("dyn", (object)dyn);

			Assert.That(interpreter.Eval("dyn.Foo + 500"), Is.EqualTo(dyn.Foo + 500));
			Assert.That(interpreter.Eval("500 + dyn.Foo"), Is.EqualTo(500 + dyn.Foo));

			Assert.That(interpreter.Eval("dyn.Foo - 500"), Is.EqualTo(dyn.Foo - 500));
			Assert.That(interpreter.Eval("500 - dyn.Foo"), Is.EqualTo(500 - dyn.Foo));

			Assert.That(interpreter.Eval("dyn.Foo / 500"), Is.EqualTo(dyn.Foo / 500));
			Assert.That(interpreter.Eval("500 / dyn.Foo"), Is.EqualTo(500 / dyn.Foo));

			Assert.That(interpreter.Eval("dyn.Foo * 500"), Is.EqualTo(dyn.Foo * 500));
			Assert.That(interpreter.Eval("500 * dyn.Foo"), Is.EqualTo(500 * dyn.Foo));

			Assert.That(interpreter.Eval("dyn.Foo % 500"), Is.EqualTo(dyn.Foo % 500));
			Assert.That(interpreter.Eval("500 % dyn.Foo"), Is.EqualTo(500 % dyn.Foo));
		}

		public class ClassWithObjectProperties
		{
			public object Foo => 100;
			public object Bar() => 100;
		}

		[Test]
		public void ObjectLateBinding()
		{


			var classWithObjectProperties = new ClassWithObjectProperties();


			Assert.Throws<ParseException>(() =>
			{

				var noLateBindingInterpreter = new Interpreter();
				var noLateBindingDel = noLateBindingInterpreter.ParseAsDelegate<Func<ClassWithObjectProperties, int>>("d.Foo+d.Bar()", new[] { "d" });
				var noLateBindingResult = noLateBindingDel.Invoke(classWithObjectProperties);

			});


			var lateBindingInterpreter = new Interpreter(InterpreterOptions.Default | InterpreterOptions.LateBindObject);
			var lateBindingInterpreterDel = lateBindingInterpreter.ParseAsDelegate<Func<ClassWithObjectProperties, int>>("d.Foo+d.Bar()", new[] { "d" });
			var lateBindingResult = lateBindingInterpreterDel.Invoke(classWithObjectProperties);
			Assert.That(lateBindingResult, Is.EqualTo((dynamic)classWithObjectProperties.Foo + (dynamic)classWithObjectProperties.Bar()));


			lateBindingInterpreter.SetVariable("d", classWithObjectProperties);
			var evalResult = lateBindingInterpreter.Eval("d.Foo+d.Bar()");
			Assert.That(evalResult.GetType(), Is.EqualTo(typeof(int)));
			Assert.That(evalResult, Is.EqualTo((dynamic)classWithObjectProperties.Foo + (dynamic)classWithObjectProperties.Bar()));

		}


		[Test]
		public void Bitwise_with_dynamic_properties()
		{
			dynamic dyn = new ExpandoObject();
			dyn.Foo = 500;

			var interpreter = new Interpreter()
				.SetVariable("dyn", (object)dyn);

			Assert.That(interpreter.Eval("dyn.Foo & 500"), Is.EqualTo(dyn.Foo & 500));
			Assert.That(interpreter.Eval("500 & dyn.Foo"), Is.EqualTo(500 & dyn.Foo));

			Assert.That(interpreter.Eval("dyn.Foo | 500"), Is.EqualTo(dyn.Foo | 500));
			Assert.That(interpreter.Eval("500 | dyn.Foo"), Is.EqualTo(500 | dyn.Foo));

			Assert.That(interpreter.Eval("dyn.Foo ^ 500"), Is.EqualTo(dyn.Foo ^ 500));
			Assert.That(interpreter.Eval("500 ^ dyn.Foo"), Is.EqualTo(500 ^ dyn.Foo));

		}

		[Test]
		public void Unary_with_dynamic_properties()
		{
			dynamic dyn = new ExpandoObject();
			dyn.Foo = true;
			dyn.Bar = 500;

			var interpreter = new Interpreter()
				.SetVariable("dyn", (object)dyn);

			Assert.That(interpreter.Eval("!dyn.Foo"), Is.EqualTo(!dyn.Foo));
			Assert.That(interpreter.Eval("-dyn.Bar"), Is.EqualTo(-dyn.Bar));
		}

		[Test]
		public void Static_method_call_with_dynamic_arg()
		{
			dynamic dyn = new ExpandoObject();
			dyn.Foo = "test";

			var myClass = new MyClass();

			var interpreter = new Interpreter()
				.SetVariable("dyn", (object)dyn);

			Assert.That(interpreter.Eval("string.IsNullOrEmpty(dyn.Foo)"), Is.EqualTo(string.IsNullOrEmpty(dyn.Foo)));
		}


		[Test]
		public void Method_call_with_dynamic_arg()
		{
			dynamic dyn = new ExpandoObject();
			dyn.Foo = "test";

			var myClass = new MyClass();

			var interpreter = new Interpreter()
				.SetVariable("dyn", (object)dyn)
				.SetVariable("myClass", myClass);

			Assert.That(interpreter.Eval("myClass.MyMethod(dyn.Foo)"), Is.EqualTo(myClass.MyMethod(dyn.Foo)));
		}

		public class MyClass
		{
			public string MyMethod(string input)
			{
				return input;
			}
		}

		public class TestDynamicClass : DynamicObject
		{
			public string RealProperty { get; set; }

			public override DynamicMetaObject GetMetaObject(Expression parameter)
			{
				throw new Exception("This should not be called");
			}
			public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
			{
				throw new Exception("This should not be called");
			}
			public override bool TryGetMember(GetMemberBinder binder, out object result)
			{
				throw new Exception("This should not be called");
			}
		}

		public class DynamicIndexAccess : DynamicObject
		{
			public dynamic Values
			{
				get
				{
					return _values;
				}
			}
			private readonly IReadOnlyDictionary<string, object> _values;

			public DynamicIndexAccess()
			{
				var values = new Dictionary<string, object>();
				values.Add("Hello", "Hello World!");
				_values = values;
			}

			public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
			{
				return _values.TryGetValue((string)indexes[0], out result);
			}

			public override bool TryGetMember(GetMemberBinder binder, out object result)
			{
				return _values.TryGetValue(binder.Name, out result);
			}
		}
	}
}
