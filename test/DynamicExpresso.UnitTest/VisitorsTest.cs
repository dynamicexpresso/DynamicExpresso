using NUnit.Framework;

namespace DynamicExpresso.UnitTest
{
	[TestFixture]
	public class VisitorsTest
	{
		[Test]
		public void By_default_reflection_is_not_permitted()
		{
			var target = new Interpreter();

			Assert.Throws<ReflectionNotAllowedException>(() => target.Parse("typeof(double).GetMethods()"));
			Assert.Throws<ReflectionNotAllowedException>(() => target.Parse("typeof(double).Assembly"));

			Assert.Throws<ReflectionNotAllowedException>(() => target.Parse("x.GetType().GetMethods()", new Parameter("x", typeof(X))));
			Assert.Throws<ReflectionNotAllowedException>(() => target.Parse("x.GetType().Assembly", new Parameter("x", typeof(X))));
		}

		[Test]
		public void By_default_reflection_to_get_name_is_permitted()
		{
			var target = new Interpreter();

			Assert.AreEqual("Double", target.Eval("typeof(double).Name"));
			Assert.AreEqual("X", target.Eval("x.GetType().Name", new Parameter("x", typeof(X), new X())));
		}

		[Test]
		public void Reflection_can_be_enabled()
		{
			var target = new Interpreter()
				.EnableReflection();

			Assert.AreEqual(typeof(double).GetMethods(), target.Eval("typeof(double).GetMethods()"));
			Assert.AreEqual(typeof(double).Assembly, target.Eval("typeof(double).Assembly"));

			var x = new X();
			Assert.AreEqual(x.GetType().GetMethods(), target.Eval("x.GetType().GetMethods()", new Parameter("x", x)));
			Assert.AreEqual(x.GetType().Assembly, target.Eval("x.GetType().Assembly", new Parameter("x", x)));
		}

		public class X { }
	}
}
