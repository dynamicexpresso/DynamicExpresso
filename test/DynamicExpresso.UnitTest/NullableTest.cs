using System;
using NUnit.Framework;
// ReSharper disable ConvertNullableToShortForm
// ReSharper disable PossibleNullReferenceException

// ReSharper disable ConvertToConstant.Local
// ReSharper disable BuiltInTypeReferenceStyle
// ReSharper disable SuggestVarOrType_BuiltInTypes

namespace DynamicExpresso.UnitTest
{
	[TestFixture]
	public class NullableTest
	{
		[Test]
		public void NullableInt32_NullableInt32()
		{
			var a = 5;
			var b = 43;

			var interpreter = new Interpreter();
			interpreter.SetVariable("a", a, typeof(Nullable<Int32>));
			interpreter.SetVariable("b", b, typeof(Nullable<Int32>));

			// Addition
			Verify(interpreter, "a + b", (Nullable<Int32>) a + b);

			// Subtraction
			Verify(interpreter, "a - b", (Nullable<Int32>) a - b);

			// Division
			Verify(interpreter, "a / b", (Nullable<Int32>) a / b);

			// Multiplication
			Verify(interpreter, "a * b", (Nullable<Int32>) a * b);
		}

		[Test]
		public void NullableInt32_NullableInt32_with_left_null()
		{
			Nullable<Int32> a = null;
			var b = 43;

			var interpreter = new Interpreter();
			interpreter.SetVariable("a", a, typeof(Nullable<Int32>));
			interpreter.SetVariable("b", b, typeof(Nullable<Int32>));
			var expectedReturnType = typeof(Nullable<Int32>);

			// Addition
			Verify(interpreter, "a + b", a + b);

			// Subtraction
			Verify(interpreter, "a - b", a - b);

			// Division
			Verify(interpreter, "a / b", a / b);

			// Multiplication
			Verify(interpreter, "a * b", a * b);
		}

		[Test]
		public void NullableInt32_NullableInt32_with_right_null()
		{
			var a = 85;
			Nullable<Int32> b = null;

			var interpreter = new Interpreter();
			interpreter.SetVariable("a", a, typeof(Nullable<Int32>));
			interpreter.SetVariable("b", b, typeof(Nullable<Int32>));
			var expectedReturnType = typeof(Nullable<Int32>);

			// Addition
			Verify(interpreter, "a + b", a + b);

			// Subtraction
			Verify(interpreter, "a - b", a - b);

			// Division
			Verify(interpreter, "a / b", a / b);

			// Multiplication
			Verify(interpreter, "a * b", a * b);
		}

		[Test]
		public void NullableDouble_NullableDouble()
		{
			var a = 5.7;
			var b = 43.2;

			var interpreter = new Interpreter();
			interpreter.SetVariable("a", a, typeof(Nullable<Double>));
			interpreter.SetVariable("b", b, typeof(Nullable<Double>));

			// Addition
			Verify(interpreter, "a + b", (Nullable<Double>) a + b);

			// Subtraction
			Verify(interpreter, "a - b", (Nullable<Double>) a - b);

			// Division
			Verify(interpreter, "a / b", (Nullable<Double>) a / b);

			// Multiplication
			Verify(interpreter, "a * b", (Nullable<Double>) a * b);
		}

		[Test]
		public void Int32_NullableDouble()
		{
			var a = 5;
			var b = 43.5;

			var interpreter = new Interpreter();
			interpreter.SetVariable("a", a, typeof(Int32));
			interpreter.SetVariable("b", b, typeof(Nullable<Double>));

			// Addition
			Verify(interpreter, "a + b", (Nullable<Double>) a + b);

			// Subtraction
			Verify(interpreter, "a - b", (Nullable<Double>) a - b);

			// Division
			Verify(interpreter, "a / b", (Nullable<Double>) a / b);

			// Multiplication
			Verify(interpreter, "a * b", (Nullable<Double>) a * b);
		}

		[Test]
		public void NullableInt32_Double()
		{
			var a = 5;
			var b = 43.5;

			var interpreter = new Interpreter();
			interpreter.SetVariable("a", a, typeof(Nullable<Int32>));
			interpreter.SetVariable("b", b, typeof(Double));

			// Addition
			Verify(interpreter, "a + b", (Nullable<Double>) a + b);

			// Subtraction
			Verify(interpreter, "a - b", (Nullable<Double>) a - b);

			// Division
			Verify(interpreter, "a / b", (Nullable<Double>) a / b);

			// Multiplication
			Verify(interpreter, "a * b", (Nullable<Double>) a * b);
		}

		[Test]
		public void NullableDateTimeOffset_DatetimeOffset()
		{
			var a = DateTimeOffset.Now;
			DateTimeOffset? b = DateTimeOffset.Now.AddDays(1);
			var c = b.Value;

			var interpreter = new Interpreter();
			interpreter.SetVariable("a", a, typeof(DateTimeOffset));
			interpreter.SetVariable("b", b, typeof(Nullable<DateTimeOffset>));
			interpreter.SetVariable("c", c, typeof(DateTimeOffset));
			var expectedReturnType = typeof(bool);

			Verify(interpreter, "a < b", a < b);
			Verify(interpreter, "a > b", a > b);
			Verify(interpreter, "a == b", a == b);
			Verify(interpreter, "a != b", a != b);
			Verify(interpreter, "b == b", b == b);
			Verify(interpreter, "b != c", b != c);
		}

		private static void Verify<T>(Interpreter interpreter, string expression, T expected)
		{
			var parsed = interpreter.Parse(expression);
			Assert.AreEqual(expected, parsed.Compile().DynamicInvoke());
			Assert.AreEqual(typeof(T), parsed.Expression.Type);
		}
	}
}
