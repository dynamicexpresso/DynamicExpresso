using DynamicExpresso.Exceptions;
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

			Assert.That(target.Eval("typeof(double).Name"), Is.EqualTo("Double"));
			Assert.That(target.Eval("x.GetType().Name", new Parameter("x", typeof(X), new X())), Is.EqualTo("X"));
		}

		[Test]
		public void Reflection_can_be_enabled()
		{
			var target = new Interpreter()
				.EnableReflection();

			Assert.That(target.Eval("typeof(double).GetMethods()"), Is.EqualTo(typeof(double).GetMethods()));
			Assert.That(target.Eval("typeof(double).Assembly"), Is.EqualTo(typeof(double).Assembly));

			var x = new X();
			Assert.That(target.Eval("x.GetType().GetMethods()", new Parameter("x", x)), Is.EqualTo(x.GetType().GetMethods()));
			Assert.That(target.Eval("x.GetType().Assembly", new Parameter("x", x)), Is.EqualTo(x.GetType().Assembly));
		}

		public class X { }
	}
}
