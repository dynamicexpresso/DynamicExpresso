using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace DynamicExpresso.UnitTest
{
	[TestFixture]
	public class IdentifiersTest
	{
		[Test]
		public void Default_identifiers_are_saved_inside_the_interpreter()
		{
			var target = new Interpreter();

			Assert.IsTrue(target.Identifiers.Any(p => p.Name == "true"));
			Assert.IsTrue(target.Identifiers.Any(p => p.Name == "false"));
			Assert.IsTrue(target.Identifiers.Any(p => p.Name == "null"));
		}

		[Test]
		public void Registered_custom_identifiers_are_saved_inside_the_interpreter()
		{
			var target = new Interpreter();

			target.SetVariable("x", null);

			Assert.IsTrue(target.Identifiers.Any(p => p.Name == "x"));
		}

		[Test]
		public void Getting_the_list_of_used_identifiers()
		{
			var target = new Interpreter()
											.SetVariable("x", 23);

			var lambda = target.Parse("x > a || true == b", new Parameter("a", 1), new Parameter("b", false));

			Assert.AreEqual(2, lambda.Identifiers.Count());
			Assert.AreEqual("x", lambda.Identifiers.ElementAt(0).Name);
			Assert.AreEqual("true", lambda.Identifiers.ElementAt(1).Name);
		}

		[Test]
		public void This_identifier_variable()
		{
			const string Name = "John";
			var context = new KeyValuePair<string, string>(nameof(Name), Name);

			var interpreter = new Interpreter();

			interpreter.SetVariable("this", context);

			Assert.AreEqual(Name, interpreter.Eval("this.Value"));
			Assert.AreEqual(nameof(Name), interpreter.Eval("this.Key"));

			Assert.AreEqual(Name, interpreter.Eval("Value"));
			Assert.AreEqual(nameof(Name), interpreter.Eval("Key"));
		}

		[Test]
		public void This_identifier_parameter()
		{
			const string Name = "John";
			var context = new KeyValuePair<string, string>(nameof(Name), Name);
			var parameter = new Parameter("this", context.GetType());

			var interpreter = new Interpreter();

			Assert.AreEqual(Name, interpreter.Parse("this.Value", parameter).Invoke(context));
			Assert.AreEqual(nameof(Name), interpreter.Parse("this.Key", parameter).Invoke(context));

			Assert.AreEqual(Name, interpreter.Parse("Value", parameter).Invoke(context));
			Assert.AreEqual(nameof(Name), interpreter.Parse("Key", parameter).Invoke(context));
		}
	}
}
