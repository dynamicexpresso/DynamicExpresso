using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DynamicExpresso.UnitTest
{
	[TestClass]
	public class OperatorsTest
	{
		[TestMethod]
		public void Multiplicative_Operators()
		{
			var target = new Interpreter();

			Assert.AreEqual(2 * 4, target.Eval("2 * 4"));
			Assert.AreEqual(8 / 2, target.Eval("8 / 2"));
			Assert.AreEqual(7 % 3, target.Eval("7 % 3"));
		}

		[TestMethod]
		public void Additive_Operators()
		{
			var target = new Interpreter();

			Assert.AreEqual(45 + 5, target.Eval("45 + 5"));
			Assert.AreEqual(45 - 5, target.Eval("45 - 5"));
			Assert.AreEqual(1.0 - 0.5, target.Eval("1.0 - 0.5"));
		}

		[TestMethod]
		public void Unary_Operators()
		{
			var target = new Interpreter();

			Assert.AreEqual(-45, target.Eval("-45"));
			Assert.AreEqual(5, target.Eval("+5"));
			Assert.AreEqual(false, target.Eval("!true"));
		}

		[TestMethod]
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

		[TestMethod]
		public void Numeric_Operators_Priority()
		{
			var target = new Interpreter();

			Assert.AreEqual(8 / 2 + 2, target.Eval("8 / 2 + 2"));
			Assert.AreEqual(8 + 2 / 2, target.Eval("8 + 2 / 2"));
		}

		[TestMethod]
		public void Typeof_Operator()
		{
			var target = new Interpreter();

			Assert.AreEqual(typeof(string), target.Eval("typeof(string)"));
		}

		[TestMethod]
		public void Is_Operator()
		{
			object a = "string value";
			object b = 64;
			var target = new Interpreter()
													.SetVariable("a", a, typeof(object))
													.SetVariable("b", b, typeof(object));

			Assert.AreEqual(a is string, target.Eval("a is string"));
			Assert.AreEqual(typeof(bool), target.Parse("a is string").ReturnType);
			Assert.AreEqual(b is string, target.Eval("b is string"));
			Assert.AreEqual(b is int, target.Eval("b is int"));
		}

		[TestMethod]
		public void As_Operator()
		{
			object a = "string value";
			object b = 64;
			var target = new Interpreter()
													.SetVariable("a", a, typeof(object))
													.SetVariable("b", b, typeof(object));

			Assert.AreEqual(a as string, target.Eval("a as string"));
			Assert.AreEqual(typeof(string), target.Parse("a as string").ReturnType);
			Assert.AreEqual(b as string, target.Eval("b as string"));
			Assert.AreEqual(typeof(string), target.Parse("b as string").ReturnType);
		}

		[TestMethod]
		public void Type_Operators()
		{
			var target = new Interpreter();

			Assert.AreEqual(typeof(string) != typeof(int), target.Eval("typeof(string) != typeof(int)"));
			Assert.AreEqual(typeof(string) == typeof(string), target.Eval("typeof(string) == typeof(string)"));
		}

		[TestMethod]
		public void String_Concatenation()
		{
			var target = new Interpreter();

			Assert.AreEqual("ciao " + "mondo", target.Eval("\"ciao \" + \"mondo\""));
		}

		[TestMethod]
		public void Numeric_Expression()
		{
			var target = new Interpreter();

			Assert.AreEqual(8 / (2 + 2), target.Eval("8 / (2 + 2)"));
			Assert.AreEqual(58 / (2 * (8 + 2)), target.Eval(" 58 / (2 * (8 + 2))"));

			Assert.AreEqual(-(8 / (2 + 2)), target.Eval("-(8 / (2 + 2))"));
			Assert.AreEqual(+(8 / (2 + 2)), target.Eval("+(8 / (2 + 2))"));
		}

		[TestMethod]
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
			Assert.IsTrue((bool)target.Eval("5 != 3"));
			Assert.IsTrue((bool)target.Eval("\"dav\" == \"dav\""));
			Assert.IsFalse((bool)target.Eval("\"dav\" == \"jack\""));
		}

		[TestMethod]
		public void Can_compare_parameters_of_different_compatible_types()
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

		[TestMethod]
		public void Logical_Operators()
		{
			var target = new Interpreter();

			Assert.IsTrue((bool)target.Eval("0 > 3 || true"));
			Assert.IsFalse((bool)target.Eval("0 > 3 && 4 < 6"));
		}

		[TestMethod]
		public void If_Operators()
		{
			var target = new Interpreter();

			Assert.AreEqual(10 > 3 ? "yes" : "no", target.Eval("10 > 3 ? \"yes\" : \"no\""));
			Assert.AreEqual(10 < 3 ? "yes" : "no", target.Eval("10 < 3 ? \"yes\" : \"no\""));
		}

		[TestMethod]
		[ExpectedException(typeof(ParseException))]
		public void Operator_Equal_Is_Not_Supported()
		{
			var target = new Interpreter();

			target.Parse("5 = 4");
		}

		[TestMethod]
		[ExpectedException(typeof(ParseException))]
		public void Operator_LessGreater_Is_Not_Supported()
		{
			var target = new Interpreter();

			target.Parse("5 <> 4");
		}

		[TestMethod]
		public void Implicit_conversion_operator_for_lambda()
		{
			var target = new Interpreter()
					.SetVariable("x", new TypeWithImplicitConversion(10));

			var func = target.Parse<Func<int>>("x");
			int val = func();

			Assert.AreEqual(10, val);
		}

		struct TypeWithImplicitConversion
		{
			private int _value;

			public TypeWithImplicitConversion(byte value)
			{
				this._value = value;
			}

			public static implicit operator int(TypeWithImplicitConversion d)
			{
				return d._value;
			}
		}

		[TestMethod]
		public void Can_use_overloaded_binary_operators()
		{
			var target = new Interpreter();

			var x = new TypeWithOverloadedBinaryOperators(3);
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

			var z = new TypeWithOverloadedBinaryOperators(10);
			Assert.IsTrue((x + z) == "13");
			Assert.IsTrue(target.Eval<bool>("(x + z) == \"13\"", new Parameter("z", z)));
		}

		struct TypeWithOverloadedBinaryOperators
		{
			private int _value;

			public TypeWithOverloadedBinaryOperators(int value)
			{
				_value = value;
			}

			public static bool operator ==(TypeWithOverloadedBinaryOperators instance, string value)
			{
				return instance._value.ToString().Equals(value);
			}

			public static bool operator !=(TypeWithOverloadedBinaryOperators instance, string value)
			{
				return !instance._value.ToString().Equals(value);
			}

			public static TypeWithOverloadedBinaryOperators operator +(TypeWithOverloadedBinaryOperators left, TypeWithOverloadedBinaryOperators right)
			{
				return new TypeWithOverloadedBinaryOperators(left._value + right._value);
			}

			public static TypeWithOverloadedBinaryOperators operator !(TypeWithOverloadedBinaryOperators instance)
			{
				return new TypeWithOverloadedBinaryOperators(-instance._value);
			}

			public override bool Equals(object obj)
			{
				if (obj == null)
					return false;
				if (obj is TypeWithOverloadedBinaryOperators)
				{
					return this._value.Equals(((TypeWithOverloadedBinaryOperators)obj)._value);
				}
				return base.Equals(obj);
			}

			public override int GetHashCode()
			{
				return _value.GetHashCode();
			}
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void Throw_an_exception_if_a_custom_type_doesnt_define_equal_operator()
		{
			var target = new Interpreter();

			var x = new TypeWithoutOverloadedBinaryOperators(3);
			target.SetVariable("x", x);

			string y = "5";
			target.Parse("x == y", new Parameter("y", y));
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void Throw_an_exception_if_a_custom_type_doesnt_define_plus_operator()
		{
			var target = new Interpreter();

			var x = new TypeWithoutOverloadedBinaryOperators(3);
			target.SetVariable("x", x);

			int y = 5;
			target.Parse("x + y", new Parameter("y", y));
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void Throw_an_exception_if_a_custom_type_doesnt_define_not_operator()
		{
			var target = new Interpreter();

			var x = new TypeWithoutOverloadedBinaryOperators(3);
			target.SetVariable("x", x);

			target.Parse("!x");
		}

		struct TypeWithoutOverloadedBinaryOperators
		{
			private int _value;

			public TypeWithoutOverloadedBinaryOperators(int value)
			{
				_value = value;
			}
		}

	}
}
