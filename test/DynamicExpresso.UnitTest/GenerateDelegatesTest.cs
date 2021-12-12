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

			var func = target.Parse<Func<double, double, double>>("Math.Pow(x, y) + 5", "x", "y").Compile();

			Assert.AreEqual(Math.Pow(10, 2) + 5, func(10, 2));

            func = target.Parse<Func<double, double, double>>("Math.Pow(x, y) + .5", "x", "y").Compile();
            Assert.AreEqual(Math.Pow(10, 2) + .5, func(10, 2));
        }

		[Test]
		public void Parse_To_a_Delegate_With_No_Parameters()
		{
			var target = new Interpreter();

			var func = target.Parse<Func<int>>("50").Compile();

			Assert.AreEqual(50, func());
		}

		[Test]
		public void Parse_To_a_Delegate_With_One_Parameter()
		{
			var target = new Interpreter();

			var func = target.Parse<Func<string, int>>("arg.Length").Compile();

			Assert.AreEqual(4, func("ciao"));
			Assert.AreEqual(9, func("123456879"));
		}

		[Test]
		public void Parse_To_a_Delegate_With_One_Parameter_With_Custom_Name()
		{
			var target = new Interpreter();

			var argumentName = "val"; // if not specified the delegate parameter is used which is "arg"
			var func = target.Parse<Func<string, int>>("val.Length", argumentName).Compile();

			Assert.AreEqual(4, func("ciao"));
			Assert.AreEqual(9, func("123456879"));
		}

		[Test]
		public void Parse_To_a_Delegate_With_Two_Parameters()
		{
			var target = new Interpreter();

			var func = target.Parse<Func<double, double, double>>("arg1 * arg2").Compile();

			Assert.AreEqual(6, func(3, 2));
			Assert.AreEqual(50, func(5, 10));
		}

		[Test]
		public void Parse_To_a_Delegate_With_Two_Parameters_With_Custom_Name()
		{
			var target = new Interpreter();

			var argumentNames = new [] { "x", "y" };
			var func = target.Parse<Func<double, double, double>>("x * y", argumentNames).Compile();

			Assert.AreEqual(6, func(3, 2));
			Assert.AreEqual(50, func(5, 10));
		}

		[Test]
		public void Parse_To_a_Custom_Delegate()
		{
			var target = new Interpreter();

			var func = target.Parse<MyCustomDelegate>("x + y.Length").Compile();

			Assert.AreEqual(7, func(3, "ciao"));
			Assert.AreEqual(10, func(5, "mondo"));
		}

		private delegate int MyCustomDelegate(int x, string y);

		[Test]
		public void Return_Type_Mismatch_Cause_An_Exception()
		{
			var target = new Interpreter();

			// expected a double but I return a string
			Assert.Throws<ParseException>(() => target.Parse<Func<double>>("\"ciao\""));
		}
	}
}
