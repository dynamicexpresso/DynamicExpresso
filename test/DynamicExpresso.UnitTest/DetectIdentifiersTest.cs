using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections;

namespace DynamicExpresso.UnitTest
{
	[TestClass]
	public class DetectIdentifiersTest
	{
		[Ignore]
		[TestMethod]
		public void Detect_identifiers_empty()
		{
			var target = new Interpreter();

			var detectedIdentifiers = target.DetectIdentifiers("");

			Assert.AreEqual(0, detectedIdentifiers.UnknownIdentifiers.Count());
			Assert.AreEqual(0, detectedIdentifiers.KnownIdentifiers.Count());
		}

		[Ignore]
		[TestMethod]
		public void Detect_identifiers_null()
		{
			var target = new Interpreter();

			var detectedIdentifiers = target.DetectIdentifiers(null);

			Assert.AreEqual(0, detectedIdentifiers.UnknownIdentifiers.Count());
			Assert.AreEqual(0, detectedIdentifiers.KnownIdentifiers.Count());
		}

		[Ignore]
		[TestMethod]
		public void Detect_unknown_identifiers()
		{
			var target = new Interpreter();

			var detectedIdentifiers = target.DetectIdentifiers("x + y");

			Assert.AreEqual(2, detectedIdentifiers.UnknownIdentifiers.Count());
			Assert.AreEqual("x", detectedIdentifiers.UnknownIdentifiers.ElementAt(0));
			Assert.AreEqual("y", detectedIdentifiers.UnknownIdentifiers.ElementAt(1));
			Assert.AreEqual(0, detectedIdentifiers.KnownIdentifiers.Count());
		}

		[Ignore]
		[TestMethod]
		public void Detect_known_identifiers()
		{
			var target = new Interpreter()
				.SetVariable("x", 3)
				.SetVariable("y", 4);

			var detectedIdentifiers = target.DetectIdentifiers("x + y");

			Assert.AreEqual(0, detectedIdentifiers.UnknownIdentifiers.Count());
			Assert.AreEqual(2, detectedIdentifiers.KnownIdentifiers.Count());
			Assert.AreEqual("x", detectedIdentifiers.KnownIdentifiers.ElementAt(0));
			Assert.AreEqual("y", detectedIdentifiers.KnownIdentifiers.ElementAt(1));
		}
	}
}
