using System;
using System.Linq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Collections;

namespace DynamicExpresso.UnitTest
{
	[TestFixture]
	public class ReferencedTypesPropertyTest
	{
		[Test]
		public void Getting_a_list_of_known_types()
		{
			var target = new Interpreter();

			Assert.IsTrue(target.ReferencedTypes.Any(p => p.Name == "string"));
			Assert.IsTrue(target.ReferencedTypes.Any(p => p.Name == "int"));
			Assert.IsTrue(target.ReferencedTypes.Any(p => p.Name == "Guid"));
		}

		[Test]
		public void Known_types_should_be_empty_with_InterpreterOptions_None()
		{
			var target = new Interpreter(InterpreterOptions.None);

			Assert.IsFalse(target.ReferencedTypes.Any());
		}

		[Test]
		public void Registering_custom_known_types()
		{
			var target = new Interpreter(InterpreterOptions.None);

			target.Reference(typeof(FakeClass));

			Assert.IsTrue(target.ReferencedTypes.Any(p => p.Type == typeof(FakeClass)));
		}

		public class FakeClass
		{
		}
	}
}
