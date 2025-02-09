using System;
using System.Collections.Generic;
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

			Assert.That(target.Eval("2 * 4"), Is.EqualTo(2 * 4));
			Assert.That(target.Eval("8 / 2"), Is.EqualTo(8 / 2));
			Assert.That(target.Eval("7 % 3"), Is.EqualTo(7 % 3));
		}

		[Test]
		public void Additive_Operators()
		{
			var target = new Interpreter();

			Assert.That(target.Eval("45 + 5"), Is.EqualTo(45 + 5));
			Assert.That(target.Eval("45 - 5"), Is.EqualTo(45 - 5));
			Assert.That(target.Eval("1.0 - 0.5"), Is.EqualTo(1.0 - 0.5));
		}

		[Test]
		public void Unary_Operators()
		{
			var target = new Interpreter();

			Assert.That(target.Eval("-45"), Is.EqualTo(-45));
			Assert.That(target.Eval("+5"), Is.EqualTo(5));
			Assert.That(target.Eval("!true"), Is.EqualTo(false));

			Assert.That(target.Eval("~2"), Is.EqualTo(~2));
			Assert.That(target.Eval("~2ul"), Is.EqualTo(~2ul));
		}

		[Test]
		public void Shift_Operators()
		{
			var target = new Interpreter();
			var x = 0b_1100_1001_0000_0000_0000_0000_0001_0001;
			target.SetVariable("x", x);

			Assert.That(target.Eval("x >> 4"), Is.EqualTo(x >> 4));
			Assert.That(target.Eval("x << 4"), Is.EqualTo(x << 4));

			// ensure they can be chained
			Assert.That(target.Eval("x >> 1 >> 1 >> 1"), Is.EqualTo(x >> 1 >> 1 >> 1));
			Assert.That(target.Eval("x << 1 << 1 << 1"), Is.EqualTo(x << 1 << 1 << 1));

			// ensure priority
			Assert.That(target.Eval<bool>("1 << 4 < 16"), Is.False);
			Assert.That(target.Eval<bool>("1 << 4 < 17"), Is.True);
		}

		[Test]
		public void Numeric_Logical_And()
		{
			var target = new Interpreter();
			target.Reference(typeof(Convert));

			var a = 0b_1111_1000;
			var b = 0b_0001_1100;
			target.SetVariable("a", a);
			target.SetVariable("b", b);

			Assert.That(target.Eval("a & b"), Is.EqualTo(a & b));
		}

		[Test]
		public void Numeric_Logical_Or()
		{
			var target = new Interpreter();
			target.Reference(typeof(Convert));

			var a = 0b_1111_1000;
			var b = 0b_0001_1100;
			target.SetVariable("a", a);
			target.SetVariable("b", b);

			Assert.That(target.Eval("a | b"), Is.EqualTo(a | b));
		}

		[Test]
		public void Numeric_Logical_Xor()
		{
			var target = new Interpreter();
			target.Reference(typeof(Convert));

			var a = 0b_1111_1000;
			var b = 0b_0001_1100;
			target.SetVariable("a", a);
			target.SetVariable("b", b);

			Assert.That(target.Eval("a ^ b"), Is.EqualTo(a ^ b));
		}

		[Test, Ignore("Current operator resolution doesn't lift int to uint")]
		public void Bitwise_operations_uint_int()
		{
			var target = new Interpreter();

			// ensure we can resolve operators between uint and int
			var x = 0b_1111_1000u;
			target.SetVariable("x", x);

			Assert.That(target.Eval<uint>("~x"), Is.EqualTo(~x));
			Assert.That(target.Eval<uint>("x >> 4"), Is.EqualTo(x >> 4));
			Assert.That(target.Eval<uint>("x << 4"), Is.EqualTo(x << 4));

			Assert.That(target.Eval<uint>("x & 4"), Is.EqualTo(x & 4));
			Assert.That(target.Eval<uint>("x | 4"), Is.EqualTo(x | 4));
			Assert.That(target.Eval<uint>("x ^ 4"), Is.EqualTo(x ^ 4));
		}

		[Test]
		public void Unary_Cast_Operator()
		{
			var target = new Interpreter();

			var x = 51.5;
			target.SetVariable("x", x);

			Assert.That(target.Eval("(int)x"), Is.EqualTo((int)x));
			Assert.That(target.Parse("(int)x").ReturnType, Is.EqualTo(typeof(int)));
			Assert.That(target.Parse("(object)x").ReturnType, Is.EqualTo(typeof(object)));
			Assert.That(target.Eval("(double)84 + 9 *8"), Is.EqualTo((double)84 + 9 * 8));
		}

		[Test]
		public void Numeric_Operators_Priority()
		{
			var target = new Interpreter();

			Assert.That(target.Eval("8 / 2 + 2"), Is.EqualTo(8 / 2 + 2));
			Assert.That(target.Eval("8 + 2 / 2"), Is.EqualTo(8 + 2 / 2));
		}

		[Test]
		public void Typeof_Operator()
		{
			var target = new Interpreter();

			Assert.Throws<ParseException>(() => target.Eval("typeof(int??)"));
			Assert.Throws<ParseException>(() => target.Eval("typeof(string?)"));

			Assert.That(target.Eval("typeof(string)"), Is.EqualTo(typeof(string)));
			Assert.That(target.Eval("typeof(int)"), Is.EqualTo(typeof(int)));
			Assert.That(target.Eval("typeof(int?)"), Is.EqualTo(typeof(int?)));
		}

		[Test]
		public void Typeof_Operator_Arrays()
		{
			var target = new Interpreter();

			Assert.That(target.Eval("typeof(int[])"), Is.EqualTo(typeof(int[])));
			Assert.That(target.Eval("typeof(int?[])"), Is.EqualTo(typeof(int?[])));
			Assert.That(target.Eval("typeof(int?[,,][][,])"), Is.EqualTo(typeof(int?[,,][][,])));
		}

		[Test]
		public void Typeof_Operator_Generics()
		{
			var target = new Interpreter();
			target.Reference(typeof(IEnumerable<>), "IEnumerable");
			target.Reference(typeof(Dictionary<,>), "Dictionary");

			Assert.That(target.Eval("typeof(IEnumerable<int>)"), Is.EqualTo(typeof(IEnumerable<int>)));
			Assert.That(target.Eval("typeof(IEnumerable<IEnumerable<int?[]>>)"), Is.EqualTo(typeof(IEnumerable<IEnumerable<int?[]>>)));
			Assert.That(target.Eval("typeof(IEnumerable<>)"), Is.EqualTo(typeof(IEnumerable<>)));

			Assert.That(target.Eval("typeof(Dictionary<int,string>[,])"), Is.EqualTo(typeof(Dictionary<int, string>[,])));
			Assert.That(target.Eval("typeof(Dictionary<int, IEnumerable<int[]>>)"), Is.EqualTo(typeof(Dictionary<int, IEnumerable<int[]>>)));
			Assert.That(target.Eval("typeof(Dictionary<,>)"), Is.EqualTo(typeof(Dictionary<,>)));
		}

		[Test]
		public void Typeof_Operator_Generics_Arity()
		{
			var target = new Interpreter();
			target.Reference(typeof(Tuple<>));
			target.Reference(typeof(Tuple<,>));
			target.Reference(typeof(Tuple<,,>));
			Assert.That(target.Eval("typeof(Tuple<>)"), Is.EqualTo(typeof(Tuple<>)));
			Assert.That(target.Eval("typeof(Tuple<,>)"), Is.EqualTo(typeof(Tuple<,>)));
			Assert.That(target.Eval("typeof(Tuple<,,>)"), Is.EqualTo(typeof(Tuple<,,>)));
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
			Assert.That(target.Eval("a is string"), Is.EqualTo(a is string));
			Assert.That(target.Parse("a is string").ReturnType, Is.EqualTo(typeof(bool)));
			// ReSharper disable once ConditionIsAlwaysTrueOrFalse
			Assert.That(target.Eval("b is string"), Is.EqualTo(b is string));
			// ReSharper disable once ConditionIsAlwaysTrueOrFalse
			Assert.That(target.Eval("b is int"), Is.EqualTo(b is int));
		}

		[Test]
		public void Is_Operator_Generics()
		{
			object a = Tuple.Create(1);
			object b = Tuple.Create(1, 2);
			var target = new Interpreter()
				.SetVariable("a", a, typeof(object))
				.SetVariable("b", b, typeof(object));

			target.Reference(typeof(Tuple<>));
			target.Reference(typeof(Tuple<,>));

			Assert.That(target.Eval("a is Tuple<int>"), Is.EqualTo(true));
			Assert.That(target.Parse("a is Tuple<int>").ReturnType, Is.EqualTo(typeof(bool)));
			Assert.That(target.Eval("b is Tuple<int,int>"), Is.EqualTo(true));
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
			Assert.That(target.Eval("a as string"), Is.EqualTo(a as string));
			Assert.That(target.Parse("a as string").ReturnType, Is.EqualTo(typeof(string)));
			// ReSharper disable once ExpressionIsAlwaysNull
			Assert.That(target.Eval("b as string"), Is.EqualTo(b as string));
			Assert.That(target.Parse("b as string").ReturnType, Is.EqualTo(typeof(string)));
		}

		[Test]
		public void Type_Operators()
		{
			var target = new Interpreter();

			Assert.That(target.Eval("typeof(string) != typeof(int)"), Is.EqualTo(typeof(string) != typeof(int)));
			// ReSharper disable once EqualExpressionComparison
			Assert.That(target.Eval("typeof(string) == typeof(string)"), Is.EqualTo(typeof(string) == typeof(string)));
		}

		[Test]
		public void String_Concatenation()
		{
			var target = new Interpreter();

			Assert.That(target.Eval("\"ciao \" + \"mondo\""), Is.EqualTo("ciao " + "mondo"));
		}

		[Test]
		public void String_Concatenation_with_type_conversion()
		{
			var target = new Interpreter();

			Assert.That(target.Eval("\"ciao \" + 1981"), Is.EqualTo("ciao " + 1981));
			Assert.That(target.Eval("1981 + \"ciao \""), Is.EqualTo(1981 + "ciao "));
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

			Assert.That(methodCallExpression, Is.Not.Null);
			Assert.That(methodCallExpression.Method, Is.EqualTo(expectedMethod));
		}

		[Test]
		public void String_Concatenation_with_null()
		{
			Interpreter interpreter = new Interpreter();
			
			string expressionText = "\"ciao \" + null";
			Assert.That(interpreter.Eval(expressionText), Is.EqualTo("ciao "));
			
			Func<String> someFunc = () => null;
			interpreter.SetFunction("someFunc", someFunc);
			expressionText = "\"ciao \" + someFunc()";
			Assert.That(interpreter.Eval(expressionText), Is.EqualTo("ciao "));
			
			Func<Object> someFuncObject = () => null;
			interpreter.SetFunction("someFuncObject", someFuncObject);
			expressionText = "\"ciao \" + someFuncObject()";
			Assert.That(interpreter.Eval(expressionText), Is.EqualTo("ciao "));

			expressionText = "someFunc() + \"123\" + null + \"678\" + someFuncObject()";
			Assert.That(interpreter.Eval(expressionText), Is.EqualTo("123678"));
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
		
			Assert.That(interpreter.Eval("\"ciao \" + myClass"), Is.EqualTo("ciao MyClassStr"));
			Assert.That(interpreter.Eval("\"ciao \" + myClassNullToString"), Is.EqualTo("ciao "));
		}

		[Test]
		public void Numeric_Expression()
		{
			var target = new Interpreter();

			Assert.That(target.Eval("8 / (2 + 2)"), Is.EqualTo(8 / (2 + 2)));
			Assert.That(target.Eval(" 58 / (2 * (8 + 2))"), Is.EqualTo(58 / (2 * (8 + 2))));

			Assert.That(target.Eval("-(8 / (2 + 2))"), Is.EqualTo(-(8 / (2 + 2))));
			Assert.That(target.Eval("+(8 / (2 + 2))"), Is.EqualTo(+(8 / (2 + 2))));
		}

		[Test]
		public void Comparison_Operators()
		{
			var target = new Interpreter();

			Assert.That((bool)target.Eval("0 > 3"), Is.False);
			Assert.That((bool)target.Eval("0 >= 3"), Is.False);
			Assert.That((bool)target.Eval("3 < 5"), Is.True);
			Assert.That((bool)target.Eval("3 <= 5"), Is.True);
			Assert.That((bool)target.Eval("5 == 3"), Is.False);
			Assert.That((bool)target.Eval("5 == 5"), Is.True);
			Assert.That((bool)target.Eval("5 >= 5"), Is.True);
			Assert.That((bool)target.Eval("5 <= 5"), Is.True);
			Assert.That((bool)target.Eval("5m >= 5m"), Is.True);
			Assert.That((bool)target.Eval("5f <= 5f"), Is.True);
			Assert.That((bool)target.Eval("5 != 3"), Is.True);
			Assert.That((bool)target.Eval("\"dav\" == \"dav\""), Is.True);
			Assert.That((bool)target.Eval("\"dav\" == \"jack\""), Is.False);
		}

		[Test]
		public void Comparison_Operators_with_different_types()
		{
			var target = new Interpreter();

			Assert.That(target.Eval("3 < 5f"), Is.EqualTo(3 < 5f));
			Assert.That(target.Eval("3f < 5"), Is.EqualTo(3f < 5));
			Assert.That(target.Eval("43 > 5m"), Is.EqualTo(43 > 5m));
			Assert.That(target.Eval("34 == 34m"), Is.EqualTo(34 == 34m));
		}

		[Test]
		public void Assignment_Operator_Equal()
		{
			var x = new TypeWithProperty();

			var target = new Interpreter()
				.SetVariable("x", x);

			// simple assignment
			target.Eval("x.Property1 = 156");
			Assert.That(x.Property1, Is.EqualTo(156));

			// assignment without space
			target.Eval("x.Property1=156");
			Assert.That(x.Property1, Is.EqualTo(156));

			// assignment with many spaces
			target.Eval("x.Property1     =    156");
			Assert.That(x.Property1, Is.EqualTo(156));

			// assignment should return the assigned value
			var returnValue = target.Eval("x.Property1 = 81");
			Assert.That(x.Property1, Is.EqualTo(81));
			Assert.That(returnValue, Is.EqualTo(x.Property1));

			// assignment can be chained
			target.Eval("x.Property1 = x.Property2 = 2014");
			Assert.That(x.Property1, Is.EqualTo(2014));
			Assert.That(x.Property2, Is.EqualTo(x.Property1));

			// assignment can be nested with other operators
			returnValue = target.Eval("x.Property1 = (486 + 4) * 10");
			Assert.That(x.Property1, Is.EqualTo(4900));
			Assert.That(returnValue, Is.EqualTo(x.Property1));

			// right member is not modified
			x.Property2 = 2;
			target.Eval("x.Property1 = x.Property2 * 10");
			Assert.That(x.Property1, Is.EqualTo(20));
			Assert.That(x.Property2, Is.EqualTo(2));
		}

		[Test]
		public void Null_coalescing()
		{
			var interpreter = new Interpreter();
			Assert.That(interpreter.Eval("null ?? null"), Is.EqualTo(null));
			Assert.That(interpreter.Eval("\"hallo\" ?? null"), Is.EqualTo("hallo"));
			Assert.That(interpreter.Eval("null ?? \"hallo\""), Is.EqualTo("hallo"));

			interpreter.SetVariable("x", null, typeof(string));
			interpreter.SetVariable("y", "hello", typeof(string));
			Assert.That(interpreter.Eval("x ?? null"), Is.EqualTo(null));
			Assert.That(interpreter.Eval("x ?? \"hallo\""), Is.EqualTo("hallo"));
			Assert.That(interpreter.Eval("y ?? \"hallo\""), Is.EqualTo("hello"));
		}

		[Test]
		public void Null_coalescing_precedence()
		{
			var interpreter = new Interpreter();
			interpreter.SetVariable("x", null, typeof(double?));

			Assert.That(interpreter.Eval("x * 2 ?? 50.5"), Is.EqualTo(50.5));
		}

		// ReSharper disable once UnusedAutoPropertyAccessor.Local
		private class TypeWithProperty { public int Property1 { get; set; } public int Property2 { get; set; } }

		[Test]
		public void Can_assign_a_parameter()
		{
			var target = new Interpreter();

			var result = target.Eval<int>("x = 5", new Parameter("x", 0));

			Assert.That(result, Is.EqualTo(5));
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

			Assert.That(target.AssignmentOperators, Is.EqualTo(AssignmentOperators.None));

			Assert.Throws<AssignmentOperatorDisabledException>(() => target.Parse("x = 5", new Parameter("x", 0)));
		}

		[Test]
		public void Can_compare_numeric_parameters_of_different_compatible_types()
		{
			var target = new Interpreter();

			double x1 = 5;
			Assert.That(target.Eval("x > 3", new Parameter("x", x1)), Is.EqualTo(true));
			double x2 = 1;
			Assert.That(target.Eval("x > 3", new Parameter("x", x2)), Is.EqualTo(false));
			decimal x3 = 5;
			Assert.That(target.Eval("x > 3", new Parameter("x", x3)), Is.EqualTo(true));
			decimal x4 = 1;
			Assert.That(target.Eval("x > 3", new Parameter("x", x4)), Is.EqualTo(false));
			int x5 = 1;
			double y1 = 10;
			Assert.That(target.Eval("x < y", new Parameter("x", x5), new Parameter("y", y1)), Is.EqualTo(true));
			double x6 = 0;
			Assert.That(target.Eval("x == 0", new Parameter("x", x6)), Is.EqualTo(true));
		}

		[Test]
		public void Can_compare_enum_parameters()
		{
			var target = new Interpreter();

			InterpreterOptions x = InterpreterOptions.CaseInsensitive;
			InterpreterOptions y = InterpreterOptions.CaseInsensitive;

			Assert.That(target.Eval("x == y", new Parameter("x", x), new Parameter("y", y)), Is.EqualTo(x == y));

			y = InterpreterOptions.CommonTypes;
			Assert.That(target.Eval("x != y", new Parameter("x", x), new Parameter("y", y)), Is.EqualTo(x != y));
		}

		[Test]
		public void Conditional_Operators()
		{
			var target = new Interpreter();

			Assert.That((bool)target.Eval("0 > 3 || true"), Is.True);
			Assert.That((bool)target.Eval("0 > 3 && 4 < 6"), Is.False);
		}

		[Test]
		public void Logical_ExclusiveOr()
		{
			var target = new Interpreter();
			Assert.That((bool)target.Eval("true ^ false"), Is.True);
			Assert.That((bool)target.Eval("false ^ true"), Is.True);
			Assert.That((bool)target.Eval("true ^ true"), Is.False);
			Assert.That((bool)target.Eval("false ^ false"), Is.False);

			Assert.That(target.Eval("1 ^ 3"), Is.EqualTo(2));
			Assert.That(target.Eval("1 ^ 2"), Is.EqualTo(3));
		}

		[Test]
		public void Conditional_And()
		{
			var target = new Interpreter();
			Assert.That((bool)target.Eval("true && false"), Is.False);
			Assert.That((bool)target.Eval("false && true"), Is.False);
			Assert.That((bool)target.Eval("true && true"), Is.True);
			Assert.That((bool)target.Eval("false && false"), Is.False);
		}

		[Test]
		public void Logical_And()
		{
			var target = new Interpreter();
			Assert.That((bool)target.Eval("true & false"), Is.False);
			Assert.That((bool)target.Eval("false & true"), Is.False);
			Assert.That((bool)target.Eval("true & true"), Is.True);
			Assert.That((bool)target.Eval("false & false"), Is.False);

			Assert.That(target.Eval("1 & 3"), Is.EqualTo(1));
			Assert.That(target.Eval("1 & 2"), Is.EqualTo(0));
		}

		[Test]
		public void Conditional_Or()
		{
			var target = new Interpreter();
			Assert.That((bool)target.Eval("true || false"), Is.True);
			Assert.That((bool)target.Eval("false || true"), Is.True);
			Assert.That((bool)target.Eval("true || true"), Is.True);
			Assert.That((bool)target.Eval("false || false"), Is.False);
		}

		[Test]
		public void Logical_Or()
		{
			var target = new Interpreter();
			Assert.That((bool)target.Eval("true | false"), Is.True);
			Assert.That((bool)target.Eval("false | true"), Is.True);
			Assert.That((bool)target.Eval("true | true"), Is.True);
			Assert.That((bool)target.Eval("false | false"), Is.False);

			Assert.That(target.Eval("1 | 3"), Is.EqualTo(3));
			Assert.That(target.Eval("1 | 2"), Is.EqualTo(3));
		}

		[Test]
		public void Operators_Precedence()
		{
			// Precedence:
			// &, ^, |, &&, ||
			// From: https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/

			var target = new Interpreter();
			Assert.That(target.Eval("false ^ false | true & true"), Is.EqualTo(false ^ false | true & true));
			Assert.That(target.Eval("false & false ^ false | true"), Is.EqualTo(false & false ^ false | true));
			Assert.That(target.Eval("true ^ true & false"), Is.EqualTo(true ^ true & false));
			Assert.That(target.Eval("true ^ true && false"), Is.EqualTo(true ^ true && false));
		}

		[Test]
		public void If_Operators()
		{
			var target = new Interpreter();

			// ReSharper disable once UnreachableCode
			Assert.That(target.Eval("10 > 3 ? \"yes\" : \"no\""), Is.EqualTo(10 > 3 ? "yes" : "no"));
			// ReSharper disable once UnreachableCode
			Assert.That(target.Eval("10 < 3 ? \"yes\" : \"no\""), Is.EqualTo(10 < 3 ? "yes" : "no"));
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

			Assert.That(val, Is.EqualTo(10));
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
			Assert.That(x == y, Is.False);
			Assert.That(target.Eval<bool>("x == y", new Parameter("y", y)), Is.False);

			y = "3";
			Assert.That(x == y, Is.True);
			Assert.That(target.Eval<bool>("x == y", new Parameter("y", y)), Is.True);

			Assert.That(target.Eval<bool>("x == \"4\""), Is.False);
			Assert.That(target.Eval<bool>("x == \"3\""), Is.True);

			Assert.That(!x == "-3", Is.True);
			Assert.That(target.Eval<bool>("!x == \"-3\""), Is.True);

			var z = new StructWithOverloadedBinaryOperators(10);
			Assert.That((x + z) == "13", Is.True);
			Assert.That(target.Eval<bool>("(x + z) == \"13\"", new Parameter("z", z)), Is.True);
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
			Assert.That(x == y, Is.False);
			Assert.That(target.Eval<bool>("x == y", new Parameter("y", y)), Is.False);

			y = "3";
			Assert.That(x == y, Is.True);
			Assert.That(target.Eval<bool>("x == y", new Parameter("y", y)), Is.True);

			Assert.That(target.Eval<bool>("x == \"4\""), Is.False);
			Assert.That(target.Eval<bool>("x == \"3\""), Is.True);

			Assert.That(!x == "-3", Is.True);
			Assert.That(target.Eval<bool>("!x == \"-3\""), Is.True);

			var z = new ClassWithOverloadedBinaryOperators(10);
			Assert.That((x + z) == "13", Is.True);
			Assert.That(target.Eval<bool>("(x + z) == \"13\"", new Parameter("z", z)), Is.True);
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
			Assert.That(x == y, Is.False);
			Assert.That(target.Eval<bool>("x == y", new Parameter("y", y)), Is.False);

			y = "3";
			Assert.That(x == y, Is.True);
			Assert.That(target.Eval<bool>("x == y", new Parameter("y", y)), Is.True);

			Assert.That(target.Eval<bool>("x == \"4\""), Is.False);
			Assert.That(target.Eval<bool>("x == \"3\""), Is.True);

			Assert.That(!x == "-3", Is.True);
			Assert.That(target.Eval<bool>("!x == \"-3\""), Is.True);

			var z = new DerivedClassWithOverloadedBinaryOperators(10);
			Assert.That((x + z) == "13", Is.True);
			Assert.That(target.Eval<bool>("(x + z) == \"13\"", new Parameter("z", z)), Is.True);

			Assert.That((x * 4) == "12", Is.True);
			Assert.That(target.Eval<bool>("(x * 4) == \"12\""), Is.True);

			// ensure a user defined operator can be found if it's on the right side operand's type
			Assert.That((4 * x) == "12", Is.True);
			Assert.That(target.Eval<bool>("(4 * x) == \"12\""), Is.True);
		}

		[Test]
		public void Can_mix_overloaded_operators()
		{
			var target = new Interpreter();

			var x = new ClassWithOverloadedBinaryOperators(3);
			target.SetVariable("x", x);

			// ensure we don't trigger an ambiguous operator exception
			var z = new DerivedClassWithOverloadedBinaryOperators(10);
			Assert.That((x + z) == "13", Is.True);
			Assert.That(target.Eval<bool>("(x + z) == \"13\"", new Parameter("z", z)), Is.True);
		}


		[Test]
		public void Throw_an_exception_if_a_custom_type_doesnt_define_equal_operator()
		{
			var target = new Interpreter();

			var x = new TypeWithoutOverloadedBinaryOperators(3);
			target.SetVariable("x", x);

			var y = "5";

			var ex = Assert.Throws<ParseException>(() => target.Parse("x == y", new Parameter("y", y)));
			Assert.That(ex.InnerException, Is.InstanceOf<InvalidOperationException>());
		}

		[Test]
		public void Throw_an_exception_if_a_custom_type_doesnt_define_plus_operator()
		{
			var target = new Interpreter();

			var x = new TypeWithoutOverloadedBinaryOperators(3);
			target.SetVariable("x", x);

			var y = 5;

			var ex = Assert.Throws<ParseException>(() => target.Parse("x + y", new Parameter("y", y)));
			Assert.That(ex.InnerException, Is.InstanceOf<InvalidOperationException>());
		}

		[Test]
		public void Throw_an_exception_if_a_custom_type_doesnt_define_not_operator()
		{
			var target = new Interpreter();

			var x = new TypeWithoutOverloadedBinaryOperators(3);
			target.SetVariable("x", x);

			var ex = Assert.Throws<ParseException>(() => target.Parse("!x"));
			Assert.That(ex.InnerException, Is.InstanceOf<InvalidOperationException>());
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
