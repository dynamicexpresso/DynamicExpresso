using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace DynamicExpresso.UnitTest
{
	[TestFixture]
	public class ReferencedTypesPropertyTest
	{
		[Test]
		public void Getting_a_list_of_known_types()
		{
			var target = new Interpreter();

			Assert.That(target.ReferencedTypes.Any(p => p.Name == "string"), Is.True);
			Assert.That(target.ReferencedTypes.Any(p => p.Name == "int"), Is.True);
			Assert.That(target.ReferencedTypes.Any(p => p.Name == "Guid"), Is.True);
		}

		[Test]
		public void Known_types_should_be_empty_with_InterpreterOptions_None()
		{
			var target = new Interpreter(InterpreterOptions.None);

			Assert.That(target.ReferencedTypes.Any(), Is.False);
		}

		[Test]
		public void Registering_custom_known_types()
		{
			var target = new Interpreter(InterpreterOptions.None);

			target.Reference(typeof(FakeClass));

			Assert.That(target.ReferencedTypes.Any(p => p.Type == typeof(FakeClass)), Is.True);
		}

		public class FakeClass
		{
		}

		[Test]
		public void Registering_generic_types()
		{
			var target = new Interpreter(InterpreterOptions.None);

			var exception = Assert.Throws<ArgumentException>(() => target.Reference(typeof(List<string>)));
			Assert.That(exception.Message, Contains.Substring("List<>"));

			exception = Assert.Throws<ArgumentException>(() => target.Reference(typeof(Tuple<string, string, int>)));
			Assert.That(exception.Message, Contains.Substring("Tuple<,,>"));

			Assert.DoesNotThrow(() => target.Reference(typeof(List<>)));
			Assert.DoesNotThrow(() => target.Reference(typeof(Tuple<,,>)));
		}
	}
}
