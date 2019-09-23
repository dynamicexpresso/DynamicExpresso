using System;
using NUnit.Framework;
using static DynamicExpresso.UnitTest.TypesTest;
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
        public void Nullable_Enum_with_left_null()
        {
            MyCustomEnum? a = MyCustomEnum.Value1;
            MyCustomEnum b = MyCustomEnum.Value1;

            var interpreter = new Interpreter();
            interpreter.SetVariable("a", a, typeof(MyCustomEnum));
            interpreter.SetVariable("b", b, typeof(Nullable<MyCustomEnum>));
            var expectedReturnType = typeof(bool);

            var expected = a == b;
            var lambda = interpreter.Parse("a == b");
            Assert.AreEqual(expected, lambda.Invoke());
            Assert.AreEqual(expectedReturnType, lambda.ReturnType);
        }

        [Test]
        public void Nullable_Enum_with_right_null()
        {
            MyCustomEnum a = MyCustomEnum.Value1;
            MyCustomEnum? b = MyCustomEnum.Value1;

            var interpreter = new Interpreter();
            interpreter.SetVariable("a", a, typeof(Nullable<MyCustomEnum>));
            interpreter.SetVariable("b", b, typeof(MyCustomEnum));
            var expectedReturnType = typeof(bool);

            var expected = a == b;
            var lambda = interpreter.Parse("a == b");
            Assert.AreEqual(expected, lambda.Invoke());
            Assert.AreEqual(expectedReturnType, lambda.ReturnType);
        }


        [Test]
        public void Nullable_Enum_Nullable_Enum()
        {
            MyCustomEnum? a = MyCustomEnum.Value1;
            MyCustomEnum? b = MyCustomEnum.Value1;

            var interpreter = new Interpreter();
            interpreter.SetVariable("a", a, typeof(Nullable<MyCustomEnum>));
            interpreter.SetVariable("b", b, typeof(Nullable<MyCustomEnum>));
            var expectedReturnType = typeof(bool);

            var expected = a == b;
            var lambda = interpreter.Parse("a == b");
            Assert.AreEqual(expected, lambda.Invoke());
            Assert.AreEqual(expectedReturnType, lambda.ReturnType);
        }
    }
}
