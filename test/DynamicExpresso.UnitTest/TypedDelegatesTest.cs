using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq.Expressions;

namespace DynamicExpresso.UnitTest
{
	[TestClass]
	public class TypedDelegatesTest
	{
		[TestMethod]
		public void Parse_To_a_Delegate_With_No_Parameters()
		{
			var target = new Interpreter();

			var func = target.Parse<Func<int>>("50");

			Assert.AreEqual(50, func());
		}

		[TestMethod]
		public void Parse_To_a_Delegate_With_One_Parameter()
		{
			var target = new Interpreter();

			var func = target.Parse<Func<string, int>>("arg.Length");

			Assert.AreEqual(4, func("ciao"));
			Assert.AreEqual(9, func("123456879"));
		}

		[TestMethod]
		public void Parse_To_a_Delegate_With_One_Parameter_With_Custom_Name()
		{
			var target = new Interpreter();

			var argumentName = "val";
			var func = target.Parse<Func<string, int>>("val.Length", argumentName);

			Assert.AreEqual(4, func("ciao"));
			Assert.AreEqual(9, func("123456879"));
		}

		[TestMethod]
		public void Parse_To_a_Delegate_With_Two_Parameters()
		{
			var target = new Interpreter();

			var func = target.Parse<Func<double, double, double>>("arg1 * arg2");

			Assert.AreEqual(6, func(3, 2));
			Assert.AreEqual(50, func(5, 10));
		}

		[TestMethod]
		public void Parse_To_a_Delegate_With_Two_Parameters_With_Custom_Name()
		{
			var target = new Interpreter();

			var argumentNames = new string[] { "x", "y" };
			var func = target.Parse<Func<double, double, double>>("x * y", argumentNames);

			Assert.AreEqual(6, func(3, 2));
			Assert.AreEqual(50, func(5, 10));
		}

		[TestMethod]
		public void Parse_To_a_Custom_Delegate()
		{
			var target = new Interpreter();

			var func = target.Parse<MyCustomDelegate>("x + y.Length");

			Assert.AreEqual(7, func(3, "ciao"));
			Assert.AreEqual(10, func(5, "mondo"));
		}

		delegate int MyCustomDelegate(int x, string y);

		[ExpectedException(typeof(ArgumentException))]
		[TestMethod]
		public void Return_Type_Mismatch_Cause_An_Exception()
		{
			var target = new Interpreter();

			// expected a double but I return a string
			var func = target.Parse<Func<double>>("\"ciao\"");
		}
	}
}
