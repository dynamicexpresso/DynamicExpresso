using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DynamicExpresso.Exceptions;
using NUnit.Framework;

namespace DynamicExpresso.UnitTest
{
	[TestFixture]
	public class OperatorsTest
	{
		[Test]
		public void Multiplicative_Operators()
		{
			var target = new Interpreter();

			Assert.AreEqual(2 * 4, target.Eval("2 * 4"));
			Assert.AreEqual(8 / 2, target.Eval("8 / 2"));
			Assert.AreEqual(7 % 3, target.Eval("7 % 3"));
		}

		[Test]
		public void Additive_Operators()
		{
			var target = new Interpreter();

			Assert.AreEqual(45 + 5, target.Eval("45 + 5"));
			Assert.AreEqual(45 - 5, target.Eval("45 - 5"));
			Assert.AreEqual(1.0 - 0.5, target.Eval("1.0 - 0.5"));
		}

		[Test]
		public void Unary_Operators()
		{
			var target = new Interpreter();

			Assert.AreEqual(-45, target.Eval("-45"));
			Assert.AreEqual(5, target.Eval("+5"));
			Assert.AreEqual(false, target.Eval("!true"));
		}

		[Test]
		public void Unary_Cast_Operator()
		{
			var target = new Interpreter();

			var x = 51.5;
			target.SetVariable("x", x);

			Assert.AreEqual((int)x, target.Eval("(int)x"));
			Assert.AreEqual(typeof(int), target.Parse("(int)x").ReturnType);
			Assert.AreEqual(typeof(object), target.Parse("(object)x").ReturnType);
			Assert.AreEqual((double)84 + 9 * 8, target.Eval("(double)84 + 9 *8"));
		}

		[Test]
		public void Numeric_Operators_Priority()
		{
			var target = new Interpreter();

			Assert.AreEqual(8 / 2 + 2, target.Eval("8 / 2 + 2"));
			Assert.AreEqual(8 + 2 / 2, target.Eval("8 + 2 / 2"));
		}

		[Test]
		public void Typeof_Operator()
		{
			var target = new Interpreter();

			Assert.AreEqual(typeof(string), target.Eval("typeof(string)"));
		}

		[Test]
		public void Is_Operator()
		{
			object a = "string value";
			object b = 64;
			var target = new Interpreter()
				.SetVariable("a", a, typeof(object))
				.SetVariable("b", b, typeof(object));

			// ReSharper disable once ConditionIsAlwaysTrueOrFalse
			Assert.AreEqual(a is string, target.Eval("a is string"));
			Assert.AreEqual(typeof(bool), target.Parse("a is string").ReturnType);
			// ReSharper disable once ConditionIsAlwaysTrueOrFalse
			Assert.AreEqual(b is string, target.Eval("b is string"));
			// ReSharper disable once ConditionIsAlwaysTrueOrFalse
			Assert.AreEqual(b is int, target.Eval("b is int"));
		}

		[Test]
		public void As_Operator()
		{
			object a = "string value";
			object b = 64;
			var target = new Interpreter()
				.SetVariable("a", a, typeof(object))
				.SetVariable("b", b, typeof(object));

			// ReSharper disable once TryCastAlwaysSucceeds
			Assert.AreEqual(a as string, target.Eval("a as string"));
			Assert.AreEqual(typeof(string), target.Parse("a as string").ReturnType);
			// ReSharper disable once ExpressionIsAlwaysNull
			Assert.AreEqual(b as string, target.Eval("b as string"));
			Assert.AreEqual(typeof(string), target.Parse("b as string").ReturnType);
		}

		[Test]
		public void Type_Operators()
		{
			var target = new Interpreter();

			Assert.AreEqual(typeof(string) != typeof(int), target.Eval("typeof(string) != typeof(int)"));
			// ReSharper disable once EqualExpressionComparison
			Assert.AreEqual(typeof(string) == typeof(string), target.Eval("typeof(string) == typeof(string)"));
		}

		[Test]
		public void String_Concatenation()
		{
			var target = new Interpreter();

			Assert.AreEqual("ciao " + "mondo", target.Eval("\"ciao \" + \"mondo\""));
		}

		[Test]
		public void String_Concatenation_with_type_conversion()
		{
			var target = new Interpreter();

			Assert.AreEqual("ciao " + 1981, target.Eval("\"ciao \" + 1981"));
			Assert.AreEqual(1981 + "ciao ", target.Eval("1981 + \"ciao \""));
		}

		[Test]
		public void String_Concatenation_check_string_method()
		{
			MethodInfo expectedMethod = typeof(string)
				.GetMethod(nameof(String.Concat), new[] {typeof(string), typeof(string)});
            
			Interpreter interpreter = new Interpreter();

			string expressionText = "\"ciao \" + 1981";

			Lambda lambda = interpreter.Parse(expressionText);
            
			MethodCallExpression methodCallExpression = lambda.Expression as MethodCallExpression;
            
			Assert.IsNotNull(methodCallExpression);
			Assert.AreEqual(expectedMethod, methodCallExpression.Method);
		}

		[Test]
		public void String_Concatenation_with_null()
		{
			Interpreter interpreter = new Interpreter();
			
			string expressionText = "\"ciao \" + null";
			Assert.AreEqual("ciao ", interpreter.Eval(expressionText));
			
			Func<String> someFunc = () => null;
			interpreter.SetFunction("someFunc", someFunc);
			expressionText = "\"ciao \" + someFunc()";
			Assert.AreEqual("ciao ", interpreter.Eval(expressionText));
			
			Func<Object> someFuncObject = () => null;
			interpreter.SetFunction("someFuncObject", someFuncObject);
			expressionText = "\"ciao \" + someFuncObject()";
			Assert.AreEqual("ciao ", interpreter.Eval(expressionText));

			expressionText = "someFunc() + \"123\" + null + \"678\" + someFuncObject()";
			Assert.AreEqual("123678", interpreter.Eval(expressionText));
		}
		
		private class MyClass
		{
			public override string ToString()
			{
				return "MyClassStr";
			}
		}

		private class MyClassNullToString : MyClass
		{
			public override string ToString()
			{
				return null;
			}
		}

		[Test]
		public void String_Concatenation_with_overridden_ToString()
		{
			Interpreter interpreter = new Interpreter()
				.SetVariable("myClass", new MyClass())
				.SetVariable("myClassNullToString", new MyClassNullToString());
		
			Assert.AreEqual("ciao MyClassStr", interpreter.Eval("\"ciao \" + myClass"));
			Assert.AreEqual("ciao ", interpreter.Eval("\"ciao \" + myClassNullToString"));
		}

		[Test]
		public void Numeric_Expression()
		{
			var target = new Interpreter();

			Assert.AreEqual(8 / (2 + 2), target.Eval("8 / (2 + 2)"));
			Assert.AreEqual(58 / (2 * (8 + 2)), target.Eval(" 58 / (2 * (8 + 2))"));

			Assert.AreEqual(-(8 / (2 + 2)), target.Eval("-(8 / (2 + 2))"));
			Assert.AreEqual(+(8 / (2 + 2)), target.Eval("+(8 / (2 + 2))"));
		}

		[Test]
		public void Comparison_Operators()
		{
			var target = new Interpreter();

			Assert.IsFalse((bool)target.Eval("0 > 3"));
			Assert.IsFalse((bool)target.Eval("0 >= 3"));
			Assert.IsTrue((bool)target.Eval("3 < 5"));
			Assert.IsTrue((bool)target.Eval("3 <= 5"));
			Assert.IsFalse((bool)target.Eval("5 == 3"));
			Assert.IsTrue((bool)target.Eval("5 == 5"));
			Assert.IsTrue((bool)target.Eval("5 >= 5"));
			Assert.IsTrue((bool)target.Eval("5 <= 5"));
			Assert.IsTrue((bool)target.Eval("5m >= 5m"));
			Assert.IsTrue((bool)target.Eval("5f <= 5f"));
			Assert.IsTrue((bool)target.Eval("5 != 3"));
			Assert.IsTrue((bool)target.Eval("\"dav\" == \"dav\""));
			Assert.IsFalse((bool)target.Eval("\"dav\" == \"jack\""));
		}

		[Test]
		public void Comparison_Operators_with_different_types()
		{
			var target = new Interpreter();

			Assert.AreEqual(3 < 5f, target.Eval("3 < 5f"));
			Assert.AreEqual(3f < 5, target.Eval("3f < 5"));
			Assert.AreEqual(43 > 5m, target.Eval("43 > 5m"));
			Assert.AreEqual(34 == 34m, target.Eval("34 == 34m"));
		}

		[Test]
		public void Assignment_Operator_Equal()
		{
			var x = new TypeWithProperty();

			var target = new Interpreter()
				.SetVariable("x", x);

			// simple assignment
			target.Eval("x.Property1 = 156");
			Assert.AreEqual(156, x.Property1);

			// assignment without space
			target.Eval("x.Property1=156");
			Assert.AreEqual(156, x.Property1);

			// assignment with many spaces
			target.Eval("x.Property1     =    156");
			Assert.AreEqual(156, x.Property1);

			// assignment should return the assigned value
			var returnValue = target.Eval("x.Property1 = 81");
			Assert.AreEqual(81, x.Property1);
			Assert.AreEqual(x.Property1, returnValue);

			// assignment can be chained
			target.Eval("x.Property1 = x.Property2 = 2014");
			Assert.AreEqual(2014, x.Property1);
			Assert.AreEqual(x.Property1, x.Property2);

			// assignment can be nested with other operators
			returnValue = target.Eval("x.Property1 = (486 + 4) * 10");
			Assert.AreEqual(4900, x.Property1);
			Assert.AreEqual(x.Property1, returnValue);

			// right member is not modified
			x.Property2 = 2;
			target.Eval("x.Property1 = x.Property2 * 10");
			Assert.AreEqual(20, x.Property1);
			Assert.AreEqual(2, x.Property2);
		}

		[Test]
		public void Null_coalescing()
		{
			var interpreter = new Interpreter();
			Assert.AreEqual(null, interpreter.Eval("null ?? null"));
			Assert.AreEqual("hallo", interpreter.Eval("\"hallo\" ?? null"));
			Assert.AreEqual("hallo", interpreter.Eval("null ?? \"hallo\""));

			interpreter.SetVariable("x", null, typeof(string));
			interpreter.SetVariable("y", "hello", typeof(string));
			Assert.AreEqual(null, interpreter.Eval("x ?? null"));
			Assert.AreEqual("hallo", interpreter.Eval("x ?? \"hallo\""));
			Assert.AreEqual("hello", interpreter.Eval("y ?? \"hallo\""));
		}

		[Test]
		public void Null_coalescing_precedence()
		{
			var interpreter = new Interpreter();
			interpreter.SetVariable("x", null, typeof(double?));

			Assert.AreEqual(50.5, interpreter.Eval("x * 2 ?? 50.5"));
		}

		// ReSharper disable once UnusedAutoPropertyAccessor.Local
		private class TypeWithProperty { public int Property1 { get; set; } public int Property2 { get; set; } }

		[Test]
		public void Can_assign_a_parameter()
		{
			var target = new Interpreter();

			var result = target.Eval<int>("x = 5", new Parameter("x", 0));

			Assert.AreEqual(5, result);
		}

		[Test]
		public void Cannot_assign_a_variable()
		{
			var target = new Interpreter()
				.SetVariable("x", 0);

			Assert.Throws<ParseException>(() => target.Parse("x = 5"));
		}

		[Test]
		public void Assignment_Operators_can_be_disabled()
		{
			var target = new Interpreter()
				.EnableAssignment(AssignmentOperators.None);

			Assert.AreEqual(AssignmentOperators.None, target.AssignmentOperators);

			Assert.Throws<AssignmentOperatorDisabledException>(() => target.Parse("x = 5", new Parameter("x", 0)));
		}

		[Test]
		public void Can_compare_numeric_parameters_of_different_compatible_types()
		{
			var target = new Interpreter();

			double x1 = 5;
			Assert.AreEqual(true, target.Eval("x > 3", new Parameter("x", x1)));
			double x2 = 1;
			Assert.AreEqual(false, target.Eval("x > 3", new Parameter("x", x2)));
			decimal x3 = 5;
			Assert.AreEqual(true, target.Eval("x > 3", new Parameter("x", x3)));
			decimal x4 = 1;
			Assert.AreEqual(false, target.Eval("x > 3", new Parameter("x", x4)));
			int x5 = 1;
			double y1 = 10;
			Assert.AreEqual(true, target.Eval("x < y", new Parameter("x", x5), new Parameter("y", y1)));
			double x6 = 0;
			Assert.AreEqual(true, target.Eval("x == 0", new Parameter("x", x6)));
		}

		[Test]
		public void Can_compare_enum_parameters()
		{
			var target = new Interpreter();

			InterpreterOptions x = InterpreterOptions.CaseInsensitive;
			InterpreterOptions y = InterpreterOptions.CaseInsensitive;

			Assert.AreEqual(x == y, target.Eval("x == y", new Parameter("x", x), new Parameter("y", y)));

			y = InterpreterOptions.CommonTypes;
			Assert.AreEqual(x != y, target.Eval("x != y", new Parameter("x", x), new Parameter("y", y)));
		}

		[Test]
		public void Conditional_Operators()
		{
			var target = new Interpreter();

			Assert.IsTrue((bool)target.Eval("0 > 3 || true"));
			Assert.IsFalse((bool)target.Eval("0 > 3 && 4 < 6"));
		}

		[Test]
		public void Logical_ExclusiveOr()
		{
			var target = new Interpreter();
			Assert.IsTrue((bool)target.Eval("true ^ false"));
			Assert.IsTrue((bool)target.Eval("false ^ true"));
			Assert.IsFalse((bool)target.Eval("true ^ true"));
			Assert.IsFalse((bool)target.Eval("false ^ false"));

			Assert.AreEqual(2, target.Eval("1 ^ 3"));
			Assert.AreEqual(3, target.Eval("1 ^ 2"));
		}

		[Test]
		public void Conditional_And()
		{
			var target = new Interpreter();
			Assert.IsFalse((bool)target.Eval("true && false"));
			Assert.IsFalse((bool)target.Eval("false && true"));
			Assert.IsTrue((bool)target.Eval("true && true"));
			Assert.IsFalse((bool)target.Eval("false && false"));
		}

		[Test]
		public void Logical_And()
		{
			var target = new Interpreter();
			Assert.IsFalse((bool)target.Eval("true & false"));
			Assert.IsFalse((bool)target.Eval("false & true"));
			Assert.IsTrue((bool)target.Eval("true & true"));
			Assert.IsFalse((bool)target.Eval("false & false"));

			Assert.AreEqual(1, target.Eval("1 & 3"));
			Assert.AreEqual(0, target.Eval("1 & 2"));
		}

		[Test]
		public void Conditional_Or()
		{
			var target = new Interpreter();
			Assert.IsTrue((bool)target.Eval("true || false"));
			Assert.IsTrue((bool)target.Eval("false || true"));
			Assert.IsTrue((bool)target.Eval("true || true"));
			Assert.IsFalse((bool)target.Eval("false || false"));
		}

		[Test]
		public void Logical_Or()
		{
			var target = new Interpreter();
			Assert.IsTrue((bool)target.Eval("true | false"));
			Assert.IsTrue((bool)target.Eval("false | true"));
			Assert.IsTrue((bool)target.Eval("true | true"));
			Assert.IsFalse((bool)target.Eval("false | false"));

			Assert.AreEqual(3, target.Eval("1 | 3"));
			Assert.AreEqual(3, target.Eval("1 | 2"));
		}

		[Test]
		public void Operators_Precedence()
		{
			// Precedence:
			// &, ^, |, &&, ||
			// From: https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/

			var target = new Interpreter();
			Assert.AreEqual(false ^ false | true & true, target.Eval("false ^ false | true & true"));
			Assert.AreEqual(false & false ^ false | true, target.Eval("false & false ^ false | true"));
			Assert.AreEqual(true ^ true & false, target.Eval("true ^ true & false"));
			Assert.AreEqual(true ^ true && false, target.Eval("true ^ true && false"));
		}

		[Test]
		public void If_Operators()
		{
			var target = new Interpreter();

			// ReSharper disable once UnreachableCode
			Assert.AreEqual(10 > 3 ? "yes" : "no", target.Eval("10 > 3 ? \"yes\" : \"no\""));
			// ReSharper disable once UnreachableCode
			Assert.AreEqual(10 < 3 ? "yes" : "no", target.Eval("10 < 3 ? \"yes\" : \"no\""));
		}

		[Test]
		public void Operator_LessGreater_Is_Not_Supported()
		{
			var target = new Interpreter();

			Assert.Throws<ParseException>(() => target.Parse("5 <> 4"));
		}

		[Test]
		public void Implicit_conversion_operator_for_lambda()
		{
			var target = new Interpreter()
				.SetVariable("x", new TypeWithImplicitConversion(10));

			var func = target.ParseAsDelegate<Func<int>>("x");
			var val = func();

			Assert.AreEqual(10, val);
		}

		private struct TypeWithImplicitConversion
		{
			private int _value;

			public TypeWithImplicitConversion(byte value)
			{
				_value = value;
			}

			public static implicit operator int(TypeWithImplicitConversion d)
			{
				return d._value;
			}
		}

		[Test]
		public void Can_use_overloaded_operators_on_struct()
		{
			var target = new Interpreter();

			var x = new StructWithOverloadedBinaryOperators(3);
			target.SetVariable("x", x);

			var y = "5";
			Assert.IsFalse(x == y);
			Assert.IsFalse(target.Eval<bool>("x == y", new Parameter("y", y)));

			y = "3";
			Assert.IsTrue(x == y);
			Assert.IsTrue(target.Eval<bool>("x == y", new Parameter("y", y)));

			Assert.IsFalse(target.Eval<bool>("x == \"4\""));
			Assert.IsTrue(target.Eval<bool>("x == \"3\""));

			Assert.IsTrue(!x == "-3");
			Assert.IsTrue(target.Eval<bool>("!x == \"-3\""));

			var z = new StructWithOverloadedBinaryOperators(10);
			Assert.IsTrue((x + z) == "13");
			Assert.IsTrue(target.Eval<bool>("(x + z) == \"13\"", new Parameter("z", z)));
		}

		private struct StructWithOverloadedBinaryOperators
		{
			private readonly int _value;

			public StructWithOverloadedBinaryOperators(int value)
			{
				_value = value;
			}

			public static bool operator ==(StructWithOverloadedBinaryOperators instance, string value)
			{
				return instance._value.ToString().Equals(value);
			}

			public static bool operator !=(StructWithOverloadedBinaryOperators instance, string value)
			{
				return !instance._value.ToString().Equals(value);
			}

			public static StructWithOverloadedBinaryOperators operator +(StructWithOverloadedBinaryOperators left, StructWithOverloadedBinaryOperators right)
			{
				return new StructWithOverloadedBinaryOperators(left._value + right._value);
			}

			public static StructWithOverloadedBinaryOperators operator !(StructWithOverloadedBinaryOperators instance)
			{
				return new StructWithOverloadedBinaryOperators(-instance._value);
			}

			public override bool Equals(object obj)
			{
				if (obj == null)
					return false;
				if (obj is StructWithOverloadedBinaryOperators)
				{
					return _value.Equals(((StructWithOverloadedBinaryOperators)obj)._value);
				}
				return base.Equals(obj);
			}

			public override int GetHashCode()
			{
				return _value.GetHashCode();
			}
		}

		[Test]
		public void Can_use_overloaded_operators_on_class()
		{
			var target = new Interpreter();

			var x = new ClassWithOverloadedBinaryOperators(3);
			target.SetVariable("x", x);

			string y = "5";
			Assert.IsFalse(x == y);
			Assert.IsFalse(target.Eval<bool>("x == y", new Parameter("y", y)));

			y = "3";
			Assert.IsTrue(x == y);
			Assert.IsTrue(target.Eval<bool>("x == y", new Parameter("y", y)));

			Assert.IsFalse(target.Eval<bool>("x == \"4\""));
			Assert.IsTrue(target.Eval<bool>("x == \"3\""));

			Assert.IsTrue(!x == "-3");
			Assert.IsTrue(target.Eval<bool>("!x == \"-3\""));

			var z = new ClassWithOverloadedBinaryOperators(10);
			Assert.IsTrue((x + z) == "13");
			Assert.IsTrue(target.Eval<bool>("(x + z) == \"13\"", new Parameter("z", z)));
		}

		private class ClassWithOverloadedBinaryOperators
		{
			private readonly int _value;

			public ClassWithOverloadedBinaryOperators(int value)
			{
				_value = value;
			}

			public static bool operator ==(ClassWithOverloadedBinaryOperators instance, string value)
			{
				return ReferenceEquals(instance, null) == false
				       && instance._value.ToString().Equals(value);
			}

			public static bool operator !=(ClassWithOverloadedBinaryOperators instance, string value)
			{
				if (ReferenceEquals(instance, null))
					return value != null;

				return !instance._value.ToString().Equals(value);
			}

			public static ClassWithOverloadedBinaryOperators operator +(ClassWithOverloadedBinaryOperators left, ClassWithOverloadedBinaryOperators right)
			{
				return new ClassWithOverloadedBinaryOperators(left._value + right._value);
			}

			public static ClassWithOverloadedBinaryOperators operator !(ClassWithOverloadedBinaryOperators instance)
			{
				return new ClassWithOverloadedBinaryOperators(-instance._value);
			}

			public static ClassWithOverloadedBinaryOperators operator *(ClassWithOverloadedBinaryOperators left, int right)
			{
				return new ClassWithOverloadedBinaryOperators(left._value * right);
			}

			public static ClassWithOverloadedBinaryOperators operator *(int left, ClassWithOverloadedBinaryOperators right)
			{
				return new ClassWithOverloadedBinaryOperators(left * right._value);
			}

			public override bool Equals(object obj)
			{
				if (obj == null)
					return false;
				var operators = obj as ClassWithOverloadedBinaryOperators;
				return operators != null ? _value.Equals(operators._value) : base.Equals(obj);
			}

			public override int GetHashCode()
			{
				return _value.GetHashCode();
			}
		}

		private class DerivedClassWithOverloadedBinaryOperators : ClassWithOverloadedBinaryOperators
		{
			public DerivedClassWithOverloadedBinaryOperators(int value) : base(value)
			{
			}
		}

		[Test]
		public void Can_use_overloaded_operators_on_derived_class()
		{
			var target = new Interpreter();

			var x = new DerivedClassWithOverloadedBinaryOperators(3);
			target.SetVariable("x", x);

			string y = "5";
			Assert.IsFalse(x == y);
			Assert.IsFalse(target.Eval<bool>("x == y", new Parameter("y", y)));

			y = "3";
			Assert.IsTrue(x == y);
			Assert.IsTrue(target.Eval<bool>("x == y", new Parameter("y", y)));

			Assert.IsFalse(target.Eval<bool>("x == \"4\""));
			Assert.IsTrue(target.Eval<bool>("x == \"3\""));

			Assert.IsTrue(!x == "-3");
			Assert.IsTrue(target.Eval<bool>("!x == \"-3\""));

			var z = new DerivedClassWithOverloadedBinaryOperators(10);
			Assert.IsTrue((x + z) == "13");
			Assert.IsTrue(target.Eval<bool>("(x + z) == \"13\"", new Parameter("z", z)));

			Assert.IsTrue((x * 4) == "12");
			Assert.IsTrue(target.Eval<bool>("(x * 4) == \"12\""));

			// ensure a user defined operator can be found if it's on the right side operand's type
			Assert.IsTrue((4 * x) == "12");
			Assert.IsTrue(target.Eval<bool>("(4 * x) == \"12\""));
		}

		[Test]
		public void Can_mix_overloaded_operators()
		{
			var target = new Interpreter();

			var x = new ClassWithOverloadedBinaryOperators(3);
			target.SetVariable("x", x);

			// ensure we don't trigger an ambiguous operator exception
			var z = new DerivedClassWithOverloadedBinaryOperators(10);
			Assert.IsTrue((x + z) == "13");
			Assert.IsTrue(target.Eval<bool>("(x + z) == \"13\"", new Parameter("z", z)));
		}


		[Test]
		public void Throw_an_exception_if_a_custom_type_doesnt_define_equal_operator()
		{
			var target = new Interpreter();

			var x = new TypeWithoutOverloadedBinaryOperators(3);
			target.SetVariable("x", x);

			var y = "5";

			var ex = Assert.Throws<ParseException>(() => target.Parse("x == y", new Parameter("y", y)));
			Assert.IsInstanceOf<InvalidOperationException>(ex.InnerException);
		}

		[Test]
		public void Throw_an_exception_if_a_custom_type_doesnt_define_plus_operator()
		{
			var target = new Interpreter();

			var x = new TypeWithoutOverloadedBinaryOperators(3);
			target.SetVariable("x", x);

			var y = 5;

			var ex = Assert.Throws<ParseException>(() => target.Parse("x + y", new Parameter("y", y)));
			Assert.IsInstanceOf<InvalidOperationException>(ex.InnerException);
		}

		[Test]
		public void Throw_an_exception_if_a_custom_type_doesnt_define_not_operator()
		{
			var target = new Interpreter();

			var x = new TypeWithoutOverloadedBinaryOperators(3);
			target.SetVariable("x", x);

			var ex = Assert.Throws<ParseException>(() => target.Parse("!x"));
			Assert.IsInstanceOf<InvalidOperationException>(ex.InnerException);
		}

		private struct TypeWithoutOverloadedBinaryOperators
		{
			// ReSharper disable once UnusedParameter.Local
			public TypeWithoutOverloadedBinaryOperators(int value)
			{
			}
		}
	}
}