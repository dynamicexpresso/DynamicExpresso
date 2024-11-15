using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

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
				new[] { "x", "y" },
				detectedIdentifiers.UnknownIdentifiers.ToArray());
		}

		[Test]
		public void Detect_unknown_identifiers_with_complete_variable_name()
		{
			var target = new Interpreter();

			var detectedIdentifiers = target.DetectIdentifiers("Contact.Personal.Year_of_birth = 1987",
				DetectorOptions.IncludeChildren);

			CollectionAssert.AreEqual(
				new[] { "Contact.Personal.Year_of_birth" },
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
		[TestCase("x + y")]
		[TestCase("x + y + 654")]
		[TestCase("x + y + 654.564")]
		[TestCase("x.method + y[0]")]
		[TestCase("x+y")]
		[TestCase("x[y]")]
		[TestCase("x.method1.method2(y)")]
		[TestCase("x + y + \"z\"")]
		[TestCase("x + y + \"lorem ipsum\"")]
		[TestCase(@"x + y + ""literal \""2""")]
		[TestCase("x + y + \"\"")]
		[TestCase("x + y + 'z'")]
		[TestCase("x + y + '\\a'")]
		[TestCase("x + y + '\\''")]
		[TestCase("x+y")]
		public void Detect_identifiers_inside_other_expressions(string testCase)
		{
			var target = new Interpreter();

			var detectedIdentifiers = target.DetectIdentifiers(testCase);

			Assert.AreEqual(2, detectedIdentifiers.UnknownIdentifiers.Count());
			Assert.AreEqual("x", detectedIdentifiers.UnknownIdentifiers.ElementAt(0));
			Assert.AreEqual("y", detectedIdentifiers.UnknownIdentifiers.ElementAt(1));
		}

		[Test]
		public void Detect_identifiers_inside_lambda_expression_GitHub_Issue_226()
		{
			var target = new Interpreter(InterpreterOptions.Default | InterpreterOptions.LambdaExpressions);
			target.SetVariable("list", new List<string>());

			var detectedIdentifiers = target.DetectIdentifiers("list.Any(x => x == null)");
			Assert.IsEmpty(detectedIdentifiers.UnknownIdentifiers);

			Assert.AreEqual(3, detectedIdentifiers.Identifiers.Count());

			Assert.AreEqual("list", detectedIdentifiers.Identifiers.ElementAt(0).Name);
			Assert.AreEqual(typeof(List<string>), detectedIdentifiers.Identifiers.ElementAt(0).Expression.Type);

			Assert.AreEqual("x", detectedIdentifiers.Identifiers.ElementAt(1).Name);
			Assert.AreEqual(typeof(object), detectedIdentifiers.Identifiers.ElementAt(1).Expression.Type);

			Assert.AreEqual("null", detectedIdentifiers.Identifiers.ElementAt(2).Name);
			Assert.AreEqual(typeof(object), detectedIdentifiers.Identifiers.ElementAt(2).Expression.Type);
		}

		[Test]
		public void Detect_identifiers_inside_lambda_expression_2()
		{
			var target = new Interpreter(InterpreterOptions.Default | InterpreterOptions.LambdaExpressions);

			var detectedIdentifiers = target.DetectIdentifiers("x => x + 5");
			Assert.IsEmpty(detectedIdentifiers.UnknownIdentifiers);

			Assert.AreEqual(1, detectedIdentifiers.Identifiers.Count());

			Assert.AreEqual("x", detectedIdentifiers.Identifiers.ElementAt(0).Name);
			Assert.AreEqual(typeof(object), detectedIdentifiers.Identifiers.ElementAt(0).Expression.Type);
		}

		[Test]
		public void Detect_identifiers_inside_lambda_expression_multiple_params()
		{
			var target = new Interpreter(InterpreterOptions.Default | InterpreterOptions.LambdaExpressions);

			var detectedIdentifiers = target.DetectIdentifiers("(x, _1y) => x + _1y");
			Assert.IsEmpty(detectedIdentifiers.UnknownIdentifiers);

			Assert.AreEqual(2, detectedIdentifiers.Identifiers.Count());

			Assert.AreEqual("x", detectedIdentifiers.Identifiers.ElementAt(0).Name);
			Assert.AreEqual(typeof(object), detectedIdentifiers.Identifiers.ElementAt(0).Expression.Type);

			Assert.AreEqual("_1y", detectedIdentifiers.Identifiers.ElementAt(1).Name);
			Assert.AreEqual(typeof(object), detectedIdentifiers.Identifiers.ElementAt(1).Expression.Type);
		}

		[Test]
		public void Detect_identifiers_inside_lambda_expression_multiple_params_with_type()
		{
			var target = new Interpreter(InterpreterOptions.Default | InterpreterOptions.LambdaExpressions);

			var detectedIdentifiers = target.DetectIdentifiers("(int x, string @class) => x + @class");
			Assert.IsEmpty(detectedIdentifiers.UnknownIdentifiers);

			Assert.AreEqual(2, detectedIdentifiers.Types.Count());
			Assert.AreEqual("int", detectedIdentifiers.Types.ElementAt(0).Name);
			Assert.AreEqual(typeof(int), detectedIdentifiers.Types.ElementAt(0).Type);
			Assert.AreEqual("string", detectedIdentifiers.Types.ElementAt(1).Name);
			Assert.AreEqual(typeof(string), detectedIdentifiers.Types.ElementAt(1).Type);

			Assert.AreEqual(2, detectedIdentifiers.Identifiers.Count());

			Assert.AreEqual("x", detectedIdentifiers.Identifiers.ElementAt(0).Name);
			Assert.AreEqual(typeof(int), detectedIdentifiers.Identifiers.ElementAt(0).Expression.Type);

			Assert.AreEqual("@class", detectedIdentifiers.Identifiers.ElementAt(1).Name);
			Assert.AreEqual(typeof(string), detectedIdentifiers.Identifiers.ElementAt(1).Expression.Type);
		}

		[Test]
		public void Detect_identifiers_inside_lambda_expression_duplicate_param_name()
		{
			var target = new Interpreter(InterpreterOptions.Default | InterpreterOptions.LambdaExpressions);

			var detectedIdentifiers =
				target.DetectIdentifiers(
					"(x, int y, z, int a) => x.Select(z => z + y).Select((string a, string b) => b)");
			Assert.IsEmpty(detectedIdentifiers.UnknownIdentifiers);

			Assert.AreEqual(2, detectedIdentifiers.Types.Count());
			Assert.AreEqual("int", detectedIdentifiers.Types.ElementAt(0).Name);
			Assert.AreEqual(typeof(int), detectedIdentifiers.Types.ElementAt(0).Type);
			Assert.AreEqual("string", detectedIdentifiers.Types.ElementAt(1).Name);
			Assert.AreEqual(typeof(string), detectedIdentifiers.Types.ElementAt(1).Type);

			Assert.AreEqual(5, detectedIdentifiers.Identifiers.Count());

			Assert.AreEqual("x", detectedIdentifiers.Identifiers.ElementAt(0).Name);
			Assert.AreEqual(typeof(object), detectedIdentifiers.Identifiers.ElementAt(0).Expression.Type);

			Assert.AreEqual("y", detectedIdentifiers.Identifiers.ElementAt(1).Name);
			Assert.AreEqual(typeof(int), detectedIdentifiers.Identifiers.ElementAt(1).Expression.Type);

			Assert.AreEqual("z", detectedIdentifiers.Identifiers.ElementAt(2).Name);
			Assert.AreEqual(typeof(object), detectedIdentifiers.Identifiers.ElementAt(2).Expression.Type);

			Assert.AreEqual("a", detectedIdentifiers.Identifiers.ElementAt(3).Name);
			Assert.AreEqual(typeof(object), detectedIdentifiers.Identifiers.ElementAt(3).Expression.Type);

			Assert.AreEqual("b", detectedIdentifiers.Identifiers.ElementAt(4).Name);
			Assert.AreEqual(typeof(string), detectedIdentifiers.Identifiers.ElementAt(4).Expression.Type);
		}

		[Test]
		[TestCase("@class")]
		[TestCase("français_holé")]
		[TestCase("中文")]
		[TestCase("_1中0文")]
		[TestCase("日本語")]
		[TestCase("русский")]
		public void Detect_all_identifiers_including_not_ascii(string identifier)
		{
			var code = $"1 + {identifier}.Method()";

			var target = new Interpreter(InterpreterOptions.Default | InterpreterOptions.LambdaExpressions);
			var detectedIdentifiers = target.DetectIdentifiers(code);

			Assert.AreEqual(1, detectedIdentifiers.UnknownIdentifiers.Count());
			Assert.AreEqual(identifier, detectedIdentifiers.UnknownIdentifiers.ElementAt(0));
		}

		[Test]
		public void Dont_detect_members_with_at()
		{
			var code = "@class.@if()";

			var target = new Interpreter(InterpreterOptions.Default | InterpreterOptions.LambdaExpressions);
			var detectedIdentifiers = target.DetectIdentifiers(code);

			// @class should be detected as an identifier, but not the @if because it's a member
			Assert.AreEqual(1, detectedIdentifiers.UnknownIdentifiers.Count());
			Assert.AreEqual("@class", detectedIdentifiers.UnknownIdentifiers.ElementAt(0));
		}


		[Test]
		[TestCase("1L")]
		[TestCase("2M")]
		[TestCase("3.0D")]
		[TestCase("4.0F")]
		[TestCase("6.7e-8")]
		[TestCase("9U")]
		[TestCase("10ul")]
		[TestCase("11lu")]
		public void Dont_detect_numbers_with_suffix(string code)
		{
			var target = new Interpreter(InterpreterOptions.Default | InterpreterOptions.LambdaExpressions);
			var detectedIdentifiers = target.DetectIdentifiers(code);
			Assert.IsEmpty(detectedIdentifiers.UnknownIdentifiers);
		}
	}
}
