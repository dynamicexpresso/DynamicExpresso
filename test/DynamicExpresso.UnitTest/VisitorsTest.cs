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
		public void By_default_late_bound_reflection_is_not_permitted()
		{
			var target = new Interpreter(InterpreterOptions.Default | InterpreterOptions.LateBindObject);
			var member = new Parameter("member", typeof(object), typeof(string).GetProperty("Length"));

			Assert.Throws<ReflectionNotAllowedException>(() => target.Eval("((object)typeof(string)).Assembly"));
			Assert.Throws<ReflectionNotAllowedException>(() => target.Eval("((object)typeof(string)).GetMethods()"));
			Assert.Throws<ReflectionNotAllowedException>(() => target.Eval("((object)typeof(string)).GetProperty(\"Length\")"));
			Assert.Throws<ReflectionNotAllowedException>(() => target.Eval("member.DeclaringType", member));
			Assert.Throws<ReflectionNotAllowedException>(() => target.Eval("member.GetValue(\"abc\")", member));
			Assert.Throws<ReflectionNotAllowedException>(() => target.Eval("member.Name = \"Other\"", member));
		}

		[Test]
		public void Late_bound_reflection_to_get_name_is_permitted()
		{
			var target = new Interpreter(InterpreterOptions.Default | InterpreterOptions.LateBindObject);
			var member = new Parameter("member", typeof(object), typeof(string).GetProperty("Length"));

			Assert.That(target.Eval("((object)typeof(string)).Name"), Is.EqualTo("String"));
			Assert.That(target.Eval("member.Name", member), Is.EqualTo("Length"));
		}

		[Test]
		public void Late_bound_reflection_can_be_enabled()
		{
			var target = new Interpreter(InterpreterOptions.Default | InterpreterOptions.LateBindObject)
				.EnableReflection();

			Assert.That(target.Eval("((object)typeof(string)).Assembly"), Is.EqualTo(typeof(string).Assembly));
			Assert.That(target.Eval("((object)typeof(string)).GetMethods()"), Is.EqualTo(typeof(string).GetMethods()));
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
