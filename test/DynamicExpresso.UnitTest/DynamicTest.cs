using Microsoft.CSharp.RuntimeBinder;
using NUnit.Framework;
using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Collections.Generic;
using DynamicExpresso.Exceptions;

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

			Assert.AreEqual(dyn.Foo, interpreter.Eval("dyn.Foo"));
		}

		[Test]
		public void Get_Property_of_a_nested_ExpandoObject()
		{
			dynamic dyn = new ExpandoObject();
			dyn.Sub = new ExpandoObject();
			dyn.Sub.Foo = "bar";

			var interpreter = new Interpreter()
					.SetVariable("dyn", (object)dyn);

			Assert.AreEqual(dyn.Sub.Foo, interpreter.Eval("dyn.Sub.Foo"));
		}

		//[Test]
		//public void Set_Property_of_an_ExpandoObject()
		//{
		//	dynamic dyn = new ExpandoObject();

		//	var interpreter = new Interpreter()
		//		.SetVariable("dyn", dyn);

		//	interpreter.Eval("dyn.Foo = 6");

		//	Assert.AreEqual(6, dyn.Foo);
		//}

		[Test]
		public void Standard_properties_have_precedence_over_dynamic_properties()
		{
			var dyn = new TestDynamicClass();
			dyn.RealProperty = "bar";

			var interpreter = new Interpreter()
				.SetVariable("dyn", dyn);

			Assert.AreEqual(dyn.RealProperty, interpreter.Eval("dyn.RealProperty"));
		}

		[Test]
		public void Invoke_Method_of_an_ExpandoObject()
		{
			dynamic dyn = new ExpandoObject();
			dyn.Foo = new Func<string>(() => "bar");

			var interpreter = new Interpreter()
				.SetVariable("dyn", dyn);

			Assert.AreEqual(dyn.Foo(), interpreter.Eval("dyn.Foo()"));
		}

		[Test]
		public void Invoke_Method_of_a_nested_ExpandoObject()
		{
			dynamic dyn = new ExpandoObject();
			dyn.Sub = new ExpandoObject();
			dyn.Sub.Foo = new Func<string>(() => "bar");

			var interpreter = new Interpreter()
					.SetVariable("dyn", dyn);

			Assert.AreEqual(dyn.Sub.Foo(), interpreter.Eval("dyn.Sub.Foo()"));
		}

		[Test]
		public void Standard_methods_have_precedence_over_dynamic_methods()
		{
			var dyn = new TestDynamicClass();

			var interpreter = new Interpreter()
				.SetVariable("dyn", dyn);

			Assert.AreEqual(dyn.ToString(), interpreter.Eval("dyn.ToString()"));
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
			dyn.Sub = new int[] {42};
			var interpreter = new Interpreter().SetVariable("dyn", (object)dyn);
			Assert.AreEqual(dyn.Sub[0], interpreter.Eval("dyn.Sub[0]"));
		}

		[Test]
		public void Get_value_of_a_nested_array_error()
		{
			dynamic dyn = new ExpandoObject();
			dyn.Sub = new int[] {42};
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

			Assert.AreEqual(myInstance.ToUniversalTime(), expression.Compile().DynamicInvoke());
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

			Assert.AreEqual(myInstance.MyMethod(), expression.Compile().DynamicInvoke());
		}

		[Test]
		public void Test_With_Dynamic_Object_By_Index_Access()
		{
			DynamicIndexAccess globals = new DynamicIndexAccess();
			Interpreter interpreter = new Interpreter()
				.SetVariable("Values", new DynamicIndexAccess());
			
			Assert.AreEqual(globals.Values["Hello"], interpreter.Eval<string>("Values[\"Hello\"]"));
		}

		[Test]
		public void Comparison_with_dynamic_properties()
		{
			dynamic dyn = new ExpandoObject();
			dyn.Foo = 500;

			var interpreter = new Interpreter()
				.SetVariable("dyn", (object)dyn);

			Assert.AreEqual(dyn.Foo == 500, interpreter.Eval("dyn.Foo == 500"));
			Assert.AreEqual(500 == dyn.Foo, interpreter.Eval("500 == dyn.Foo"));

			Assert.AreEqual(dyn.Foo != 200, interpreter.Eval("dyn.Foo != 200"));
			Assert.AreEqual(200 != dyn.Foo, interpreter.Eval("200 != dyn.Foo"));

			Assert.AreEqual(dyn.Foo > 200, interpreter.Eval("dyn.Foo > 200"));
			Assert.AreEqual(600 > dyn.Foo, interpreter.Eval("600 > dyn.Foo"));

			Assert.AreEqual(dyn.Foo < 600, interpreter.Eval("dyn.Foo < 600"));
			Assert.AreEqual(200 < dyn.Foo, interpreter.Eval("200 < dyn.Foo"));

			Assert.AreEqual(dyn.Foo >= 500, interpreter.Eval("dyn.Foo >= 500"));
			Assert.AreEqual(500 >= dyn.Foo, interpreter.Eval("500 >= dyn.Foo"));

			Assert.AreEqual(dyn.Foo <= 500, interpreter.Eval("dyn.Foo <= 500"));
			Assert.AreEqual(500 <= dyn.Foo, interpreter.Eval("500 <= dyn.Foo"));

			Assert.AreEqual(dyn.Foo + 500, interpreter.Eval("dyn.Foo + 500"));
			Assert.AreEqual(500 + dyn.Foo, interpreter.Eval("500 + dyn.Foo"));
		}

		[Test]
		public void Conditional_with_dynamic_properties()
		{
			dynamic dyn = new ExpandoObject();
			dyn.Foo = 500;
			dyn.Bar = 100;

			var interpreter = new Interpreter()
				.SetVariable("dyn", (object)dyn);

			Assert.AreEqual(dyn.Bar>0?100:200, interpreter.Eval("dyn.Bar>0?100:200"));
		}

		[Test]
		public void AndAlso_with_dynamic_properties()
		{
			dynamic dyn = new ExpandoObject();
			dyn.Foo = 500;
			dyn.Bar = 100;

			var interpreter = new Interpreter()
				.SetVariable("dyn", (object)dyn);

			Assert.AreEqual(dyn.Foo == 500 && dyn.Bar == 100, interpreter.Eval("dyn.Foo == 500 && dyn.Bar == 100"));
			Assert.AreEqual(dyn.Foo == 500 && dyn.Bar != 100, interpreter.Eval("dyn.Foo == 500 && dyn.Bar != 100"));
			Assert.AreEqual(dyn.Foo != 500 && dyn.Bar == 100, interpreter.Eval("dyn.Foo != 500 && dyn.Bar == 100"));
			Assert.AreEqual(dyn.Foo != 500 && dyn.Bar != 100, interpreter.Eval("dyn.Foo != 500 && dyn.Bar != 100"));

			//check non dynamic right side
			Assert.AreEqual(dyn.Foo == 500 && 100 == 100, interpreter.Eval("dyn.Foo == 500 && 100 == 100"));
			Assert.AreEqual(dyn.Foo == 500 && 100 != 100, interpreter.Eval("dyn.Foo == 500 && 100 != 100"));
			Assert.AreEqual(dyn.Foo != 500 && 100 == 100, interpreter.Eval("dyn.Foo != 500 && 100 == 100"));
			Assert.AreEqual(dyn.Foo != 500 && 100 != 100, interpreter.Eval("dyn.Foo != 500 && 100 != 100"));

			//check non dynamic left side
			Assert.AreEqual(100 == 100 && dyn.Foo == 500, interpreter.Eval("100 == 100 && dyn.Foo == 500"));
			Assert.AreEqual(100 != 100 && dyn.Foo == 500, interpreter.Eval("100 != 100 && dyn.Foo == 500"));
			Assert.AreEqual(100 == 100 && dyn.Foo != 500, interpreter.Eval("100 == 100 && dyn.Foo != 500"));
			Assert.AreEqual(100 != 100 && dyn.Foo != 500, interpreter.Eval("100 != 100 && dyn.Foo != 500"));
		}

		[Test]
		public void OrElse_with_dynamic_properties()
		{
			dynamic dyn = new ExpandoObject();
			dyn.Foo = 500;
			dyn.Bar = 100;

			var interpreter = new Interpreter()
				.SetVariable("dyn", (object)dyn);

			Assert.AreEqual(dyn.Foo == 500 || dyn.Bar == 100, interpreter.Eval("dyn.Foo == 500 || dyn.Bar == 100"));
			Assert.AreEqual(dyn.Foo == 500 || dyn.Bar != 100, interpreter.Eval("dyn.Foo == 500 || dyn.Bar != 100"));
			Assert.AreEqual(dyn.Foo != 500 || dyn.Bar == 100, interpreter.Eval("dyn.Foo != 500 || dyn.Bar == 100"));
			Assert.AreEqual(dyn.Foo != 500 || dyn.Bar != 100, interpreter.Eval("dyn.Foo != 500 || dyn.Bar != 100"));

			//check non dynamic right side
			Assert.AreEqual(dyn.Foo == 500 || 100 == 100, interpreter.Eval("dyn.Foo == 500 || 100 == 100"));
			Assert.AreEqual(dyn.Foo == 500 || 100 != 100, interpreter.Eval("dyn.Foo == 500 || 100 != 100"));
			Assert.AreEqual(dyn.Foo != 500 || 100 == 100, interpreter.Eval("dyn.Foo != 500 || 100 == 100"));
			Assert.AreEqual(dyn.Foo != 500 || 100 != 100, interpreter.Eval("dyn.Foo != 500 || 100 != 100"));

			//check non dynamic left side
			Assert.AreEqual(100 == 100 || dyn.Foo == 500, interpreter.Eval("100 == 100 || dyn.Foo == 500"));
			Assert.AreEqual(100 != 100 || dyn.Foo == 500, interpreter.Eval("100 != 100 || dyn.Foo == 500"));
			Assert.AreEqual(100 == 100 || dyn.Foo != 500, interpreter.Eval("100 == 100 || dyn.Foo != 500"));
			Assert.AreEqual(100 != 100 || dyn.Foo != 500, interpreter.Eval("100 != 100 || dyn.Foo != 500"));
		}

		[Test]
		public void Math_with_dynamic_properties()
		{
			dynamic dyn = new ExpandoObject();
			dyn.Foo = 500;

			var interpreter = new Interpreter()
				.SetVariable("dyn", (object)dyn);

			Assert.AreEqual(dyn.Foo + 500, interpreter.Eval("dyn.Foo + 500"));
			Assert.AreEqual(500 + dyn.Foo, interpreter.Eval("500 + dyn.Foo"));

			Assert.AreEqual(dyn.Foo - 500, interpreter.Eval("dyn.Foo - 500"));
			Assert.AreEqual(500 - dyn.Foo, interpreter.Eval("500 - dyn.Foo"));

			Assert.AreEqual(dyn.Foo / 500, interpreter.Eval("dyn.Foo / 500"));
			Assert.AreEqual(500 / dyn.Foo, interpreter.Eval("500 / dyn.Foo"));

			Assert.AreEqual(dyn.Foo * 500, interpreter.Eval("dyn.Foo * 500"));
			Assert.AreEqual(500 * dyn.Foo, interpreter.Eval("500 * dyn.Foo"));

			Assert.AreEqual(dyn.Foo % 500, interpreter.Eval("dyn.Foo % 500"));
			Assert.AreEqual(500 % dyn.Foo, interpreter.Eval("500 % dyn.Foo"));
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
						

			Assert.Throws<ParseException>(() => {

				var noLateBindingInterpreter = new Interpreter();
				var noLateBindingDel = noLateBindingInterpreter.ParseAsDelegate<Func<ClassWithObjectProperties, int>>("d.Foo+d.Bar()", new[] { "d" });
				var noLateBindingResult = noLateBindingDel.Invoke(classWithObjectProperties);

			});


			var lateBindingInterpreter = new Interpreter(InterpreterOptions.Default|InterpreterOptions.LateBindObject);
			var lateBindingInterpreterDel = lateBindingInterpreter.ParseAsDelegate<Func<ClassWithObjectProperties, int>>("d.Foo+d.Bar()", new[] { "d" });
			var lateBindingResult = lateBindingInterpreterDel.Invoke(classWithObjectProperties);
			Assert.AreEqual((dynamic)classWithObjectProperties.Foo + (dynamic)classWithObjectProperties.Bar(), lateBindingResult);


			lateBindingInterpreter.SetVariable("d", classWithObjectProperties);
			var evalResult = lateBindingInterpreter.Eval("d.Foo+d.Bar()");
			Assert.IsTrue(evalResult.GetType() == typeof(int));
			Assert.AreEqual((dynamic)classWithObjectProperties.Foo + (dynamic)classWithObjectProperties.Bar(), evalResult);

		}
	

		[Test]
		public void Bitwise_with_dynamic_properties()
		{
			dynamic dyn = new ExpandoObject();
			dyn.Foo = 500;

			var interpreter = new Interpreter()
				.SetVariable("dyn", (object)dyn);

			Assert.AreEqual(dyn.Foo & 500, interpreter.Eval("dyn.Foo & 500"));
			Assert.AreEqual(500 & dyn.Foo, interpreter.Eval("500 & dyn.Foo"));

			Assert.AreEqual(dyn.Foo | 500, interpreter.Eval("dyn.Foo | 500"));
			Assert.AreEqual(500 | dyn.Foo, interpreter.Eval("500 | dyn.Foo"));

			Assert.AreEqual(dyn.Foo ^ 500, interpreter.Eval("dyn.Foo ^ 500"));
			Assert.AreEqual(500 ^ dyn.Foo, interpreter.Eval("500 ^ dyn.Foo"));
			
		}

		[Test]
		public void Unary_with_dynamic_properties()
		{
			dynamic dyn = new ExpandoObject();
			dyn.Foo = true;
			dyn.Bar = 500;

			var interpreter = new Interpreter()
				.SetVariable("dyn", (object)dyn);

			Assert.AreEqual(!dyn.Foo, interpreter.Eval("!dyn.Foo"));
			Assert.AreEqual(-dyn.Bar, interpreter.Eval("-dyn.Bar"));



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
