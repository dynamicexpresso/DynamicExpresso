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

			Assert.That(target.Eval("Int32.MaxValue"), Is.EqualTo(int.MaxValue));
			Assert.That(target.Eval("Double.MaxValue"), Is.EqualTo(double.MaxValue));
			Assert.That(target.Eval("DateTime.MaxValue"), Is.EqualTo(DateTime.MaxValue));
			Assert.That(target.Eval("DateTime.Today"), Is.EqualTo(DateTime.Today));
			Assert.That(target.Eval("String.Empty"), Is.EqualTo(string.Empty));
			Assert.That(target.Eval("Boolean.FalseString"), Is.EqualTo(bool.FalseString));
			Assert.That(target.Eval("TimeSpan.TicksPerMillisecond"), Is.EqualTo(TimeSpan.TicksPerMillisecond));
			Assert.That(target.Eval("Guid.Empty"), Is.EqualTo(Guid.Empty));
		}

		[Test]
		public void Static_Methods_of_Base_Types()
		{
			var target = new Interpreter();

			Assert.That(target.Eval("TimeSpan.FromMilliseconds(2000.49)"), Is.EqualTo(TimeSpan.FromMilliseconds(2000.49)));
			Assert.That(target.Eval("DateTime.DaysInMonth(2094, 11)"), Is.EqualTo(DateTime.DaysInMonth(2094, 11)));
		}

		[Test]
		public void Math_Class()
		{
			var target = new Interpreter();

			Assert.That(target.Eval("Math.Pow(3, 4)"), Is.EqualTo(Math.Pow(3, 4)));
			Assert.That(target.Eval("Math.Sin(30.234)"), Is.EqualTo(Math.Sin(30.234)));
		}

		[Test]
		public void Convert_Class()
		{
			var target = new Interpreter();

			Assert.That(target.Eval("Convert.ToString(3)"), Is.EqualTo(Convert.ToString(3)));
			Assert.That(target.Eval("Convert.ToInt16(\"23\")"), Is.EqualTo(Convert.ToInt16("23")));
		}

		[Test]
		public void CSharp_Primitive_Type_Keywords()
		{
			var target = new Interpreter();

			Assert.That(target.Eval("int.MaxValue"), Is.EqualTo(int.MaxValue));
			Assert.That(target.Eval("double.MaxValue"), Is.EqualTo(double.MaxValue));
			Assert.That(target.Eval("string.Empty"), Is.EqualTo(string.Empty));
			Assert.That(target.Eval("bool.FalseString"), Is.EqualTo(bool.FalseString));
			Assert.That(target.Eval("char.MinValue"), Is.EqualTo(char.MinValue));
			Assert.That(target.Eval("byte.MinValue"), Is.EqualTo(byte.MinValue));
		}

		[Test]
		public void Static_Properties_And_Methods_Of_Custom_Types()
		{
			var target = new Interpreter()
											.Reference(typeof(Uri))
											.Reference(typeof(MyTestService));

			Assert.That(target.Eval("Uri.UriSchemeHttp"), Is.EqualTo(Uri.UriSchemeHttp));
			Assert.That(target.Eval("MyTestService.MyStaticMethod()"), Is.EqualTo(MyTestService.MyStaticMethod()));
		}

		[Test]
		public void Type_Related_Static_Methods()
		{
			var target = new Interpreter()
											.Reference(typeof(Type));

			Assert.That(target.Eval("Type.GetType(\"System.Globalization.CultureInfo\")"), Is.EqualTo(Type.GetType("System.Globalization.CultureInfo")));
			Assert.That(target.Eval("DateTime.Now.GetType()"), Is.EqualTo(DateTime.Now.GetType()));
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
