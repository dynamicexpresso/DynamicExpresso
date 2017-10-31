using System;
using NUnit.Framework;

namespace DynamicExpresso.UnitTest
{
	[TestFixture]
	public class StaticTest
	{
		[Test]
		public void Static_Properties_of_Base_Types()
		{
			var target = new Interpreter();

			Assert.AreEqual(int.MaxValue, target.Eval("Int32.MaxValue"));
			Assert.AreEqual(double.MaxValue, target.Eval("Double.MaxValue"));
			Assert.AreEqual(DateTime.MaxValue, target.Eval("DateTime.MaxValue"));
			Assert.AreEqual(DateTime.Today, target.Eval("DateTime.Today"));
			Assert.AreEqual(string.Empty, target.Eval("String.Empty"));
			Assert.AreEqual(bool.FalseString, target.Eval("Boolean.FalseString"));
			Assert.AreEqual(TimeSpan.TicksPerMillisecond, target.Eval("TimeSpan.TicksPerMillisecond"));
			Assert.AreEqual(Guid.Empty, target.Eval("Guid.Empty"));
		}

		[Test]
		public void Static_Methods_of_Base_Types()
		{
			var target = new Interpreter();

			Assert.AreEqual(TimeSpan.FromMilliseconds(2000.49), target.Eval("TimeSpan.FromMilliseconds(2000.49)"));
			Assert.AreEqual(DateTime.DaysInMonth(2094, 11), target.Eval("DateTime.DaysInMonth(2094, 11)"));
		}

		[Test]
		public void Math_Class()
		{
			var target = new Interpreter();

			Assert.AreEqual(Math.Pow(3, 4), target.Eval("Math.Pow(3, 4)"));
			Assert.AreEqual(Math.Sin(30.234), target.Eval("Math.Sin(30.234)"));
		}

		[Test]
		public void Convert_Class()
		{
			var target = new Interpreter();

			Assert.AreEqual(Convert.ToString(3), target.Eval("Convert.ToString(3)"));
			Assert.AreEqual(Convert.ToInt16("23"), target.Eval("Convert.ToInt16(\"23\")"));
		}

		[Test]
		public void CSharp_Primitive_Type_Keywords()
		{
			var target = new Interpreter();

			Assert.AreEqual(int.MaxValue, target.Eval("int.MaxValue"));
			Assert.AreEqual(double.MaxValue, target.Eval("double.MaxValue"));
			Assert.AreEqual(string.Empty, target.Eval("string.Empty"));
			Assert.AreEqual(bool.FalseString, target.Eval("bool.FalseString"));
			Assert.AreEqual(char.MinValue, target.Eval("char.MinValue"));
			Assert.AreEqual(byte.MinValue, target.Eval("byte.MinValue"));
		}

		[Test]
		public void Static_Properties_And_Methods_Of_Custom_Types()
		{
			var target = new Interpreter()
											.Reference(typeof(Uri))
											.Reference(typeof(MyTestService));

			Assert.AreEqual(Uri.UriSchemeHttp, target.Eval("Uri.UriSchemeHttp"));
			Assert.AreEqual(MyTestService.MyStaticMethod(), target.Eval("MyTestService.MyStaticMethod()"));
		}

		[Test]
		public void Type_Related_Static_Methods()
		{
			var target = new Interpreter()
											.Reference(typeof(Type));

			Assert.AreEqual(Type.GetType("System.Globalization.CultureInfo"), target.Eval("Type.GetType(\"System.Globalization.CultureInfo\")"));
			Assert.AreEqual(DateTime.Now.GetType(), target.Eval("DateTime.Now.GetType()"));
		}

		private class MyTestService
		{
			public static int MyStaticMethod()
			{
				return 23;
			}
		}
	}
}
