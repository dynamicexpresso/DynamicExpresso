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
	}
}
