using System;
using NUnit.Framework;

namespace DynamicExpresso.UnitTest
{
	[TestFixture]
	public class ConstructorTest
	{
		[Test]
		public void New_Of_Base_Type()
		{
			var target = new Interpreter();

			Assert.AreEqual(new DateTime(2015, 1, 24), target.Eval("new DateTime(2015, 1, 24)"));
			Assert.AreEqual(new string('a', 10), target.Eval("new string('a', 10)"));
		}

		[Test]
		public void New_Of_Custom_Type()
		{
			var target = new Interpreter();

			target.Reference(typeof(Uri));

			Assert.AreEqual(new Uri("http://www.google.com"), target.Eval("new Uri(\"http://www.google.com\")"));
		}

		[Test]
		public void New_And_Member_Access()
		{
			var target = new Interpreter();

			Assert.AreEqual(new DateTime(2015, 1, 24).Month, target.Eval("new DateTime(2015,   1, 24).Month"));
			Assert.AreEqual(new DateTime(2015, 1, 24).Month + 34, target.Eval("new DateTime( 2015, 1, 24).Month + 34"));
		}

		[Test]
		public void Constructor_invocation_without_new_is_not_supported()
		{
			var target = new Interpreter();

            Assert.Throws<ParseException>(() => target.Parse("DateTime(2010, 5, 23)"));
        }

		[Test]
		public void Unknown_New_Type_Is_Not_Supported()
		{
			var target = new Interpreter();

            Assert.Throws<UnknownIdentifierException>(() => target.Parse("new unkkeyword()"));
		}
	}
}
