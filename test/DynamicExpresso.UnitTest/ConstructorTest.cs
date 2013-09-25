using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DynamicExpresso.UnitTest
{
	[TestClass]
	public class ConstructorTest
	{
		[TestMethod]
		public void New_Of_Base_Type()
		{
			var target = new Interpreter();

			Assert.AreEqual(new DateTime(2015, 1, 24), target.Eval("new DateTime(2015, 1, 24)"));
			Assert.AreEqual(new string('a', 10), target.Eval("new string('a', 10)"));
		}

		[TestMethod]
		public void New_Of_Custom_Type()
		{
			var target = new Interpreter();

			target.Reference(typeof(Uri));

			Assert.AreEqual(new Uri("http://www.google.com"), target.Eval("new Uri(\"http://www.google.com\")"));
		}

		[TestMethod]
		public void New_And_Member_Access()
		{
			var target = new Interpreter();

			Assert.AreEqual(new DateTime(2015, 1, 24).Month, target.Eval("new DateTime(2015,   1, 24).Month"));
			Assert.AreEqual(new DateTime(2015, 1, 24).Month + 34, target.Eval("new DateTime( 2015, 1, 24).Month + 34"));
		}

		[TestMethod]
		[ExpectedException(typeof(ParseException))]
		public void Constructor_invocation_without_new_is_not_supported()
		{
			var target = new Interpreter();

			target.Parse("DateTime(2010, 5, 23)");
		}

		[TestMethod]
		[ExpectedException(typeof(ParseException))]
		public void Unknown_New_Type_Is_Not_Supported()
		{
			var target = new Interpreter();

			target.Parse("new unkkeyword()");
		}
	}
}
