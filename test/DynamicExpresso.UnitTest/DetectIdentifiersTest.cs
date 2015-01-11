using System;
using System.Linq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Collections;

namespace DynamicExpresso.UnitTest
{
	[TestFixture]
	public class DetectIdentifiersTest
	{
		[Test]
		public void Detect_identifiers_empty()
		{
			var target = new Interpreter();

			var detectedIdentifiers = target.DetectIdentifiers("");

			Assert.AreEqual(0, detectedIdentifiers.UnknownIdentifiers.Count());
			Assert.AreEqual(0, detectedIdentifiers.Identifiers.Count());
			Assert.AreEqual(0, detectedIdentifiers.Types.Count());
		}

		[Test]
		public void Detect_identifiers_null()
		{
			var target = new Interpreter();

			var detectedIdentifiers = target.DetectIdentifiers(null);

			Assert.AreEqual(0, detectedIdentifiers.UnknownIdentifiers.Count());
			Assert.AreEqual(0, detectedIdentifiers.Identifiers.Count());
			Assert.AreEqual(0, detectedIdentifiers.Types.Count());
		}

		[Test]
		public void Detect_unknown_identifiers()
		{
			var target = new Interpreter();

			var detectedIdentifiers = target.DetectIdentifiers("x + y");

			CollectionAssert.AreEqual(
				new []{ "x", "y"}, 
				detectedIdentifiers.UnknownIdentifiers.ToArray());
		}

		[Test]
		public void Should_detect_various_format_of_identifiers()
		{
			var target = new Interpreter();

			var validNames = new[] { "x", "y", "_z", "x23", "asdas_afsdf" };

			foreach (var name in validNames)
			{
				var detectedIdentifiers = target.DetectIdentifiers(name);

				CollectionAssert.AreEqual(
					new[] { name },
					detectedIdentifiers.UnknownIdentifiers.ToArray());
			}
		}

		[Test]
		public void An_Unknown_Identifier_used_multiple_times_is_detected_only_once()
		{
			var target = new Interpreter();

			var detectedIdentifiers = target.DetectIdentifiers("x + x");

			CollectionAssert.AreEqual(
				new[] { "x" },
				detectedIdentifiers.UnknownIdentifiers.ToArray());
		}

		[Test]
		public void With_case_insensitive_An_Unknown_Identifier_used_multiple_times_is_detected_only_once()
		{
			var target = new Interpreter(InterpreterOptions.DefaultCaseInsensitive);

			var detectedIdentifiers = target.DetectIdentifiers("x + X");

			CollectionAssert.AreEqual(
				new[] { "x" },
				detectedIdentifiers.UnknownIdentifiers.ToArray());
		}

		[Test]
		public void A_Known_Identifier_used_multiple_times_is_detected_only_once()
		{
			var target = new Interpreter()
				.SetVariable("x", 213);

			var detectedIdentifiers = target.DetectIdentifiers("x + x");

			CollectionAssert.AreEqual(
				new[] { "x" },
				detectedIdentifiers.Identifiers.Select(p => p.Name).ToArray());
		}

		[Test]
		public void With_case_insensitive_A_Known_Identifier_used_multiple_times_is_detected_only_once()
		{
			var target = new Interpreter(InterpreterOptions.DefaultCaseInsensitive)
				.SetVariable("x", 213);

			var detectedIdentifiers = target.DetectIdentifiers("x + X");

			CollectionAssert.AreEqual(
				new[] { "x" },
				detectedIdentifiers.Identifiers.Select(p => p.Name).ToArray());
		}

		[Test]
		public void A_Known_Type_used_multiple_times_is_detected_only_once()
		{
			var target = new Interpreter();

			var detectedIdentifiers = target.DetectIdentifiers("string.Empty + string.Empty");

			CollectionAssert.AreEqual(
				new[] { "string" },
				detectedIdentifiers.Types.Select(p => p.Name).ToArray());
		}

		[Test]
		public void With_case_insensitive_A_Known_Type_used_multiple_times_is_detected_only_once()
		{
			var target = new Interpreter(InterpreterOptions.DefaultCaseInsensitive);

			var detectedIdentifiers = target.DetectIdentifiers("string.Empty + STRING.Empty");

			CollectionAssert.AreEqual(
				new[] { "string" },
				detectedIdentifiers.Types.Select(p => p.Name).ToArray());
		}

		[Test]
		public void Detect_known_identifiers_variables()
		{
			var target = new Interpreter()
				.SetVariable("x", 3)
				.SetVariable("y", 4);

			var detectedIdentifiers = target.DetectIdentifiers("x + y");

			Assert.AreEqual(2, detectedIdentifiers.Identifiers.Count());
			Assert.AreEqual("x", detectedIdentifiers.Identifiers.ElementAt(0).Name);
			Assert.AreEqual("y", detectedIdentifiers.Identifiers.ElementAt(1).Name);
		}

		[Test]
		public void Detect_known_identifiers_types()
		{
			var target = new Interpreter();

			var detectedIdentifiers = target.DetectIdentifiers("string.Empty");

			Assert.AreEqual(1, detectedIdentifiers.Types.Count());
			Assert.AreEqual("string", detectedIdentifiers.Types.ElementAt(0).Name);
			Assert.AreEqual(typeof(string), detectedIdentifiers.Types.ElementAt(0).Type);
		}

		[Test]
		public void Detect_identifiers_inside_other_expressions()
		{
			var testCases = new[] {
				"x + y",
				"x + y + 654",
				"x + y + 654.564",
				"x.method + y[0]",
				"x+y",
				"x[y]",
				"x.method1.method2(y)",
				"x + y + \"z\"",
				"x + y + \"lorem ipsum\"",
				@"x + y + ""literal \""2""",
				"x + y + \"\"",
				"x + y + 'z'",
				"x + y + '\\a'",
				"x + y + '\\''",
				"x+y",
			};

			foreach (var testCase in testCases)
			{
				var target = new Interpreter();

				var detectedIdentifiers = target.DetectIdentifiers(testCase);

				Assert.AreEqual("x", detectedIdentifiers.UnknownIdentifiers.ElementAt(0));
				Assert.AreEqual("y", detectedIdentifiers.UnknownIdentifiers.ElementAt(1));
				Assert.AreEqual(2, detectedIdentifiers.UnknownIdentifiers.Count());
			}
		}
	}
}
