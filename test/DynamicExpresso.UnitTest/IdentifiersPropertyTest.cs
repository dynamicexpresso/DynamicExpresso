using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections;

namespace DynamicExpresso.UnitTest
{
	[TestClass]
	public class IdentifiersPropertyTest
	{
		[TestMethod]
		public void Getting_a_list_of_known_identifiers()
		{
			var target = new Interpreter();

			Assert.IsTrue(target.Identifiers.Any(p => p.Name == "true"));
			Assert.IsTrue(target.Identifiers.Any(p => p.Name == "false"));
			Assert.IsTrue(target.Identifiers.Any(p => p.Name == "null"));
		}

		[TestMethod]
		public void Registering_custom_identifiers()
		{
			var target = new Interpreter();

			target.SetVariable("x", null);

			Assert.IsTrue(target.Identifiers.Any(p => p.Name == "x"));
		}
	}
}
