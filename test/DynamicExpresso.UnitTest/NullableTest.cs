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
			var expectedReturnType = typeof(Nullable<Int32>);

			// Addition
			var expected = a + b;
			var lambda = interpreter.Parse("a + b");
			Assert.AreEqual(expected, lambda.Invoke());
			Assert.AreEqual(expectedReturnType, lambda.ReturnType);

			// Subtraction
			expected = a - b;
			lambda = interpreter.Parse("a - b");
			Assert.AreEqual(expected, lambda.Invoke());
			Assert.AreEqual(expectedReturnType, lambda.ReturnType);

			// Division
			expected = a / b;
			lambda = interpreter.Parse("a / b");
			Assert.AreEqual(expected, lambda.Invoke());
			Assert.AreEqual(expectedReturnType, lambda.ReturnType);

			// Multiplication
			expected = a * b;
			lambda = interpreter.Parse("a * b");
			Assert.AreEqual(expected, lambda.Invoke());
			Assert.AreEqual(expectedReturnType, lambda.ReturnType);
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
			var expected = a + b;
			var lambda = interpreter.Parse("a + b");
			Assert.AreEqual(expected, lambda.Invoke());
			Assert.AreEqual(expectedReturnType, lambda.ReturnType);

			// Subtraction
			expected = a - b;
			lambda = interpreter.Parse("a - b");
			Assert.AreEqual(expected, lambda.Invoke());
			Assert.AreEqual(expectedReturnType, lambda.ReturnType);

			// Division
			expected = a / b;
			lambda = interpreter.Parse("a / b");
			Assert.AreEqual(expected, lambda.Invoke());
			Assert.AreEqual(expectedReturnType, lambda.ReturnType);

			// Multiplication
			expected = a * b;
			lambda = interpreter.Parse("a * b");
			Assert.AreEqual(expected, lambda.Invoke());
			Assert.AreEqual(expectedReturnType, lambda.ReturnType);
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
			var expected = a + b;
			var lambda = interpreter.Parse("a + b");
			Assert.AreEqual(expected, lambda.Invoke());
			Assert.AreEqual(expectedReturnType, lambda.ReturnType);

			// Subtraction
			expected = a - b;
			lambda = interpreter.Parse("a - b");
			Assert.AreEqual(expected, lambda.Invoke());
			Assert.AreEqual(expectedReturnType, lambda.ReturnType);

			// Division
			expected = a / b;
			lambda = interpreter.Parse("a / b");
			Assert.AreEqual(expected, lambda.Invoke());
			Assert.AreEqual(expectedReturnType, lambda.ReturnType);

			// Multiplication
			expected = a * b;
			lambda = interpreter.Parse("a * b");
			Assert.AreEqual(expected, lambda.Invoke());
			Assert.AreEqual(expectedReturnType, lambda.ReturnType);
		}

		[Test]
		public void NullableDouble_NullableDouble()
		{
			var a = 5.7;
			var b = 43.2;

			var interpreter = new Interpreter();
			interpreter.SetVariable("a", a, typeof(Nullable<Double>));
			interpreter.SetVariable("b", b, typeof(Nullable<Double>));
			var expectedReturnType = typeof(Nullable<Double>);

			// Addition
			var expected = a + b;
			var lambda = interpreter.Parse("a + b");
			Assert.AreEqual(expected, lambda.Invoke());
			Assert.AreEqual(expectedReturnType, lambda.ReturnType);

			// Subtraction
			expected = a - b;
			lambda = interpreter.Parse("a - b");
			Assert.AreEqual(expected, lambda.Invoke());
			Assert.AreEqual(expectedReturnType, lambda.ReturnType);

			// Division
			expected = a / b;
			lambda = interpreter.Parse("a / b");
			Assert.AreEqual(expected, lambda.Invoke());
			Assert.AreEqual(expectedReturnType, lambda.ReturnType);

			// Multiplication
			expected = a * b;
			lambda = interpreter.Parse("a * b");
			Assert.AreEqual(expected, lambda.Invoke());
			Assert.AreEqual(expectedReturnType, lambda.ReturnType);
		}

		[Test]
		public void Int32_NullableDouble()
		{
			var a = 5;
			var b = 43.5;

			var interpreter = new Interpreter();
			interpreter.SetVariable("a", a, typeof(Int32));
			interpreter.SetVariable("b", b, typeof(Nullable<Double>));
			var expectedReturnType = typeof(Nullable<Double>);

			// Addition
			var expected = a + b;
			var lambda = interpreter.Parse("a + b");
			Assert.AreEqual(expected, lambda.Invoke());
			Assert.AreEqual(expectedReturnType, lambda.ReturnType);

			// Subtraction
			expected = a - b;
			lambda = interpreter.Parse("a - b");
			Assert.AreEqual(expected, lambda.Invoke());
			Assert.AreEqual(expectedReturnType, lambda.ReturnType);

			// Division
			expected = a / b;
			lambda = interpreter.Parse("a / b");
			Assert.AreEqual(expected, lambda.Invoke());
			Assert.AreEqual(expectedReturnType, lambda.ReturnType);

			// Multiplication
			expected = a * b;
			lambda = interpreter.Parse("a * b");
			Assert.AreEqual(expected, lambda.Invoke());
			Assert.AreEqual(expectedReturnType, lambda.ReturnType);
		}

		[Test]
		public void NullableInt32_Double()
		{
			var a = 5;
			var b = 43.5;

			var interpreter = new Interpreter();
			interpreter.SetVariable("a", a, typeof(Nullable<Int32>));
			interpreter.SetVariable("b", b, typeof(Double));
			var expectedReturnType = typeof(Nullable<Double>);

			// Addition
			var expected = a + b;
			var lambda = interpreter.Parse("a + b");
			Assert.AreEqual(expected, lambda.Invoke());
			Assert.AreEqual(expectedReturnType, lambda.ReturnType);

			// Subtraction
			expected = a - b;
			lambda = interpreter.Parse("a - b");
			Assert.AreEqual(expected, lambda.Invoke());
			Assert.AreEqual(expectedReturnType, lambda.ReturnType);

			// Division
			expected = a / b;
			lambda = interpreter.Parse("a / b");
			Assert.AreEqual(expected, lambda.Invoke());
			Assert.AreEqual(expectedReturnType, lambda.ReturnType);

			// Multiplication
			expected = a * b;
			lambda = interpreter.Parse("a * b");
			Assert.AreEqual(expected, lambda.Invoke());
			Assert.AreEqual(expectedReturnType, lambda.ReturnType);
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

			var expected = a < b;
			var lambda = interpreter.Parse("a < b");
			Assert.AreEqual(expected, lambda.Invoke());
			Assert.AreEqual(expectedReturnType, lambda.ReturnType);

			expected = a > b;
			lambda = interpreter.Parse("a > b");
			Assert.AreEqual(expected, lambda.Invoke());
			Assert.AreEqual(expectedReturnType, lambda.ReturnType);

			expected = a == b;
			lambda = interpreter.Parse("a == b");
			Assert.AreEqual(expected, lambda.Invoke());
			Assert.AreEqual(expectedReturnType, lambda.ReturnType);

			expected = a != b;
			lambda = interpreter.Parse("a != b");
			Assert.AreEqual(expected, lambda.Invoke());
			Assert.AreEqual(expectedReturnType, lambda.ReturnType);

			expected = b == c;
			lambda = interpreter.Parse("b == b");
			Assert.AreEqual(expected, lambda.Invoke());
			Assert.AreEqual(expectedReturnType, lambda.ReturnType);

			expected = b != c;
			lambda = interpreter.Parse("b != c");
			Assert.AreEqual(expected, lambda.Invoke());
			Assert.AreEqual(expectedReturnType, lambda.ReturnType);
		}
	}
}
