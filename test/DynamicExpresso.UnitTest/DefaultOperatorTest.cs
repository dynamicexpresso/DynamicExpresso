using NUnit.Framework;

namespace DynamicExpresso.UnitTest
{
	[TestFixture]
	public class DefaultOperatorTest
	{
		[Test]
		public void Default_value_type()
		{
			var target = new Interpreter();

			Assert.AreEqual(default(int), target.Eval("default(int)"));
			Assert.AreEqual(default(double), target.Eval("default(double)"));
			Assert.AreEqual(default(System.DateTime), target.Eval("default(DateTime)"));

			Assert.AreEqual(typeof(int), target.Eval("default(int)").GetType());
			Assert.AreEqual(typeof(double), target.Eval("default(double)").GetType());
			Assert.AreEqual(typeof(System.DateTime), target.Eval("default(DateTime)").GetType());
		}

		[Test]
		public void Default_reference_type()
		{
			var target = new Interpreter();

			Assert.AreEqual(default(string), target.Eval("default(string)"));
		}

		[Test]
		public void Default_nullable_type()
		{
			var target = new Interpreter();

			Assert.AreEqual(default(int?), target.Eval("default(int?)"));
			Assert.AreEqual(default(double?), target.Eval("default(double?)"));
			Assert.AreEqual(default(System.DateTime?), target.Eval("default(DateTime?)"));
		}
	}
}
