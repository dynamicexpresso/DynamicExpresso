using DynamicExpresso.Exceptions;
using System.Dynamic;
using NUnit.Framework;

namespace DynamicExpresso.UnitTest
{
	[TestFixture]
	public class VisitorsTest
	{
		private const string DynamicTypeSeed = "Enumerable.ToArray(Enumerable.Repeat(typeof(string), p.Name.Length))[0]";
		private const string DynamicReflection = DynamicTypeSeed + ".Assembly";
		private const string LambdaDynamicReflection = "Enumerable.ToArray(Enumerable.Select(new int[]{1}, x => " + DynamicReflection + "))";
		private const string NestedLambdaDynamicReflection = "Enumerable.ToArray(Enumerable.Select(new int[]{1}, y => " + LambdaDynamicReflection + "))";

		/// <summary>
		/// Test related to GHSA-v37m-7mgv-vwv9 security advisory.
		/// </summary>
		[TestCase(DynamicReflection)]
		[TestCase(LambdaDynamicReflection)]
		[TestCase(NestedLambdaDynamicReflection)]
		public void By_default_dynamic_reflection_is_not_permitted_in_lambda_bodies(string expression)
		{
			var target = new Interpreter(InterpreterOptions.Default | InterpreterOptions.LambdaExpressions);
			dynamic bag = new ExpandoObject();
			bag.Name = "abcd";
			var parameter = new Parameter("p", typeof(ExpandoObject), (object)bag);

			Assert.Throws<ReflectionNotAllowedException>(() => target.Eval(expression, parameter));
		}

		/// <summary>
		/// Test related to GHSA-v37m-7mgv-vwv9 security advisory.
		/// </summary>
		[Test]
		public void Dynamic_reflection_can_be_enabled_in_lambda_bodies()
		{
			var target = new Interpreter(InterpreterOptions.Default | InterpreterOptions.LambdaExpressions)
				.EnableReflection();
			dynamic bag = new ExpandoObject();
			bag.Name = "abcd";
			var parameter = new Parameter("p", typeof(ExpandoObject), (object)bag);

			Assert.That(target.Eval(LambdaDynamicReflection, parameter), Is.EqualTo(new object[] { typeof(string).Assembly }));
		}

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
