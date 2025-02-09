using System.Linq;
using NUnit.Framework;

namespace DynamicExpresso.UnitTest
{
	[TestFixture]
	public class IdentifiersTest
	{
		class Customer
		{
			public string Name { get; set; }

			public string GetName()
			{
				return Name;
			}
		}

		[Test]
		public void Default_identifiers_are_saved_inside_the_interpreter()
		{
			var target = new Interpreter();

			Assert.That(target.Identifiers.Any(p => p.Name == "true"), Is.True);
			Assert.That(target.Identifiers.Any(p => p.Name == "false"), Is.True);
			Assert.That(target.Identifiers.Any(p => p.Name == "null"), Is.True);
		}

		[Test]
		public void Registered_custom_identifiers_are_saved_inside_the_interpreter()
		{
			var target = new Interpreter();

			target.SetVariable("x", null);

			Assert.That(target.Identifiers.Any(p => p.Name == "x"), Is.True);
		}

		[Test]
		public void Getting_the_list_of_used_identifiers()
		{
			var target = new Interpreter()
											.SetVariable("x", 23);

			var lambda = target.Parse("x > a || true == b", new Parameter("a", 1), new Parameter("b", false));

			Assert.That(lambda.Identifiers.Count(), Is.EqualTo(2));
			Assert.That(lambda.Identifiers.ElementAt(0).Name, Is.EqualTo("x"));
			Assert.That(lambda.Identifiers.ElementAt(1).Name, Is.EqualTo("true"));
		}

		[Test]
		public void This_identifier_variable()
		{
			const string Name = "John";

			var interpreter = new Interpreter();

			interpreter.SetVariable("this", new Customer {Name = Name});

			Assert.That(interpreter.Eval("this.Name"), Is.EqualTo(Name));
			Assert.That(interpreter.Eval("this.GetName()"), Is.EqualTo(Name));

			Assert.That(interpreter.Eval("Name"), Is.EqualTo(Name));
			Assert.That(interpreter.Eval("GetName()"), Is.EqualTo(Name));
		}

		[Test]
		public void This_identifier_parameter()
		{
			const string Name = "John";

			var context = new Customer {Name = Name};
			var parameter = new Parameter("this", context.GetType());
			var interpreter = new Interpreter();

			Assert.That(interpreter.Parse("this.Name", parameter).Invoke(context), Is.EqualTo(Name));
			Assert.That(interpreter.Parse("this.GetName()", parameter).Invoke(context), Is.EqualTo(Name));

			Assert.That(interpreter.Parse("Name", parameter).Invoke(context), Is.EqualTo(Name));
			Assert.That(interpreter.Parse("GetName()", parameter).Invoke(context), Is.EqualTo(Name));
		}
	}
}
