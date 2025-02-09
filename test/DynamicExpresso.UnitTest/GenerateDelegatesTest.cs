using System;
using DynamicExpresso.Exceptions;
using NUnit.Framework;

namespace DynamicExpresso.UnitTest
{
	[TestFixture]
	public class GenerateDelegatesTest
	{
		[Test]
		public void Parse_To_a_Delegate()
		{
			var target = new Interpreter();

			var func = target.ParseAsDelegate<Func<double, double, double>>("Math.Pow(x, y) + 5", "x", "y");

			Assert.That(func(10, 2), Is.EqualTo(Math.Pow(10, 2) + 5));

            func = target.ParseAsDelegate<Func<double, double, double>>("Math.Pow(x, y) + .5", "x", "y");
            Assert.That(func(10, 2), Is.EqualTo(Math.Pow(10, 2) + .5));
        }

		[Test]
		public void Parse_To_a_Delegate_With_No_Parameters()
		{
			var target = new Interpreter();

			var func = target.ParseAsDelegate<Func<int>>("50");

			Assert.That(func(), Is.EqualTo(50));
		}

		[Test]
		public void Parse_To_a_Delegate_With_One_Parameter()
		{
			var target = new Interpreter();

			var func = target.ParseAsDelegate<Func<string, int>>("arg.Length");

			Assert.That(func("ciao"), Is.EqualTo(4));
			Assert.That(func("123456879"), Is.EqualTo(9));
		}

		[Test]
		public void Parse_To_a_Delegate_With_One_Parameter_With_Custom_Name()
		{
			var target = new Interpreter();

			var argumentName = "val"; // if not specified the delegate parameter is used which is "arg"
			var func = target.ParseAsDelegate<Func<string, int>>("val.Length", argumentName);

			Assert.That(func("ciao"), Is.EqualTo(4));
			Assert.That(func("123456879"), Is.EqualTo(9));
		}

		[Test]
		public void Parse_To_a_Delegate_With_Two_Parameters()
		{
			var target = new Interpreter();

			var func = target.ParseAsDelegate<Func<double, double, double>>("arg1 * arg2");

			Assert.That(func(3, 2), Is.EqualTo(6));
			Assert.That(func(5, 10), Is.EqualTo(50));
		}

		[Test]
		public void Parse_To_a_Delegate_With_Two_Parameters_With_Custom_Name()
		{
			var target = new Interpreter();

			var argumentNames = new [] { "x", "y" };
			var func = target.ParseAsDelegate<Func<double, double, double>>("x * y", argumentNames);

			Assert.That(func(3, 2), Is.EqualTo(6));
			Assert.That(func(5, 10), Is.EqualTo(50));
		}

		[Test]
		public void Parse_To_a_Custom_Delegate()
		{
			var target = new Interpreter();

			var func = target.ParseAsDelegate<MyCustomDelegate>("x + y.Length");

			Assert.That(func(3, "ciao"), Is.EqualTo(7));
			Assert.That(func(5, "mondo"), Is.EqualTo(10));
		}

		private delegate int MyCustomDelegate(int x, string y);

		[Test]
		public void Return_Type_Mismatch_Cause_An_Exception()
		{
			var target = new Interpreter();

			// expected a double but I return a string
			Assert.Throws<ParseException>(() => target.ParseAsDelegate<Func<double>>("\"ciao\""));
		}
	}
}
