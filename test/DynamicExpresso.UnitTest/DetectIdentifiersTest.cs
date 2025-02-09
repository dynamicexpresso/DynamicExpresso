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

			Assert.That(detectedIdentifiers.UnknownIdentifiers.Count(), Is.EqualTo(0));
			Assert.That(detectedIdentifiers.Identifiers.Count(), Is.EqualTo(0));
			Assert.That(detectedIdentifiers.Types.Count(), Is.EqualTo(0));
		}

		[Test]
		public void Detect_identifiers_null()
		{
			var target = new Interpreter();

			var detectedIdentifiers = target.DetectIdentifiers(null);

			Assert.That(detectedIdentifiers.UnknownIdentifiers.Count(), Is.EqualTo(0));
			Assert.That(detectedIdentifiers.Identifiers.Count(), Is.EqualTo(0));
			Assert.That(detectedIdentifiers.Types.Count(), Is.EqualTo(0));
		}

		[Test]
		public void Detect_unknown_identifiers()
		{
			var target = new Interpreter();

			var detectedIdentifiers = target.DetectIdentifiers("x + y");

			Assert.That(
				detectedIdentifiers.UnknownIdentifiers,
				Is.EqualTo(new[] { "x", "y" }));
		}

		[Test]
		public void Detect_unknown_identifiers_with_complete_variable_name()
		{
			var target = new Interpreter();

			var detectedIdentifiers = target.DetectIdentifiers("Contact.Personal.Year_of_birth = 1987",
				DetectorOptions.IncludeChildren);

			Assert.That(
				detectedIdentifiers.UnknownIdentifiers,
				Is.EqualTo(new[] { "Contact.Personal.Year_of_birth" }));
		}

		[Test]
		public void Should_detect_various_format_of_identifiers()
		{
			var target = new Interpreter();

			var validNames = new[] { "x", "y", "_z", "x23", "asdas_afsdf" };

			foreach (var name in validNames)
			{
				var detectedIdentifiers = target.DetectIdentifiers(name);

				Assert.That(
					detectedIdentifiers.UnknownIdentifiers,
					Is.EqualTo(new[] { name }));
			}
		}

		[Test]
		public void An_Unknown_Identifier_used_multiple_times_is_detected_only_once()
		{
			var target = new Interpreter();

			var detectedIdentifiers = target.DetectIdentifiers("x + x");

			Assert.That(
				detectedIdentifiers.UnknownIdentifiers,
				Is.EqualTo(new[] { "x" }));
		}

		[Test]
		public void With_case_insensitive_An_Unknown_Identifier_used_multiple_times_is_detected_only_once()
		{
			var target = new Interpreter(InterpreterOptions.DefaultCaseInsensitive);

			var detectedIdentifiers = target.DetectIdentifiers("x + X");

			Assert.That(
				detectedIdentifiers.UnknownIdentifiers,
				Is.EqualTo(new[] { "x" }));
		}

		[Test]
		public void A_Known_Identifier_used_multiple_times_is_detected_only_once()
		{
			var target = new Interpreter()
				.SetVariable("x", 213);

			var detectedIdentifiers = target.DetectIdentifiers("x + x");

			Assert.That(
				detectedIdentifiers.Identifiers.Select(p => p.Name).ToArray(),
				Is.EqualTo(new[] { "x" }));
		}

		[Test]
		public void With_case_insensitive_A_Known_Identifier_used_multiple_times_is_detected_only_once()
		{
			var target = new Interpreter(InterpreterOptions.DefaultCaseInsensitive)
				.SetVariable("x", 213);

			var detectedIdentifiers = target.DetectIdentifiers("x + X");

			Assert.That(
				detectedIdentifiers.Identifiers.Select(p => p.Name).ToArray(),
				Is.EqualTo(new[] { "x" }));
		}

		[Test]
		public void A_Known_Type_used_multiple_times_is_detected_only_once()
		{
			var target = new Interpreter();

			var detectedIdentifiers = target.DetectIdentifiers("string.Empty + string.Empty");

			Assert.That(
				detectedIdentifiers.Types.Select(p => p.Name).ToArray(),
				Is.EqualTo(new[] { "string" }));
		}

		[Test]
		public void With_case_insensitive_A_Known_Type_used_multiple_times_is_detected_only_once()
		{
			var target = new Interpreter(InterpreterOptions.DefaultCaseInsensitive);

			var detectedIdentifiers = target.DetectIdentifiers("string.Empty + STRING.Empty");

			Assert.That(
				detectedIdentifiers.Types.Select(p => p.Name).ToArray(),
				Is.EqualTo(new[] { "string" }));
		}

		[Test]
		public void Detect_known_identifiers_variables()
		{
			var target = new Interpreter()
				.SetVariable("x", 3)
				.SetVariable("y", 4);

			var detectedIdentifiers = target.DetectIdentifiers("x + y");

			Assert.That(detectedIdentifiers.Identifiers.Count(), Is.EqualTo(2));
			Assert.That(detectedIdentifiers.Identifiers.ElementAt(0).Name, Is.EqualTo("x"));
			Assert.That(detectedIdentifiers.Identifiers.ElementAt(1).Name, Is.EqualTo("y"));
		}

		[Test]
		public void Detect_known_identifiers_types()
		{
			var target = new Interpreter();

			var detectedIdentifiers = target.DetectIdentifiers("string.Empty");

			Assert.That(detectedIdentifiers.Types.Count(), Is.EqualTo(1));
			Assert.That(detectedIdentifiers.Types.ElementAt(0).Name, Is.EqualTo("string"));
			Assert.That(detectedIdentifiers.Types.ElementAt(0).Type, Is.EqualTo(typeof(string)));
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

			Assert.That(detectedIdentifiers.UnknownIdentifiers.Count(), Is.EqualTo(2));
			Assert.That(detectedIdentifiers.UnknownIdentifiers.ElementAt(0), Is.EqualTo("x"));
			Assert.That(detectedIdentifiers.UnknownIdentifiers.ElementAt(1), Is.EqualTo("y"));
		}

		[Test]
		public void Detect_identifiers_inside_lambda_expression_GitHub_Issue_226()
		{
			var target = new Interpreter(InterpreterOptions.Default | InterpreterOptions.LambdaExpressions);
			target.SetVariable("list", new List<string>());

			var detectedIdentifiers = target.DetectIdentifiers("list.Any(x => x == null)");
			Assert.That(detectedIdentifiers.UnknownIdentifiers, Is.Empty);

			Assert.That(detectedIdentifiers.Identifiers.Count(), Is.EqualTo(3));

			Assert.That(detectedIdentifiers.Identifiers.ElementAt(0).Name, Is.EqualTo("list"));
			Assert.That(detectedIdentifiers.Identifiers.ElementAt(0).Expression.Type, Is.EqualTo(typeof(List<string>)));

			Assert.That(detectedIdentifiers.Identifiers.ElementAt(1).Name, Is.EqualTo("x"));
			Assert.That(detectedIdentifiers.Identifiers.ElementAt(1).Expression.Type, Is.EqualTo(typeof(object)));

			Assert.That(detectedIdentifiers.Identifiers.ElementAt(2).Name, Is.EqualTo("null"));
			Assert.That(detectedIdentifiers.Identifiers.ElementAt(2).Expression.Type, Is.EqualTo(typeof(object)));
		}

		[Test]
		public void Detect_identifiers_inside_lambda_expression_2()
		{
			var target = new Interpreter(InterpreterOptions.Default | InterpreterOptions.LambdaExpressions);

			var detectedIdentifiers = target.DetectIdentifiers("x => x + 5");
			Assert.That(detectedIdentifiers.UnknownIdentifiers, Is.Empty);

			Assert.That(detectedIdentifiers.Identifiers.Count(), Is.EqualTo(1));

			Assert.That(detectedIdentifiers.Identifiers.ElementAt(0).Name, Is.EqualTo("x"));
			Assert.That(detectedIdentifiers.Identifiers.ElementAt(0).Expression.Type, Is.EqualTo(typeof(object)));
		}

		[Test]
		public void Detect_identifiers_inside_lambda_expression_multiple_params()
		{
			var target = new Interpreter(InterpreterOptions.Default | InterpreterOptions.LambdaExpressions);

			var detectedIdentifiers = target.DetectIdentifiers("(x, _1y) => x + _1y");
			Assert.That(detectedIdentifiers.UnknownIdentifiers, Is.Empty);

			Assert.That(detectedIdentifiers.Identifiers.Count(), Is.EqualTo(2));

			Assert.That(detectedIdentifiers.Identifiers.ElementAt(0).Name, Is.EqualTo("x"));
			Assert.That(detectedIdentifiers.Identifiers.ElementAt(0).Expression.Type, Is.EqualTo(typeof(object)));

			Assert.That(detectedIdentifiers.Identifiers.ElementAt(1).Name, Is.EqualTo("_1y"));
			Assert.That(detectedIdentifiers.Identifiers.ElementAt(1).Expression.Type, Is.EqualTo(typeof(object)));
		}

		[Test]
		public void Detect_identifiers_inside_lambda_expression_multiple_params_with_type()
		{
			var target = new Interpreter(InterpreterOptions.Default | InterpreterOptions.LambdaExpressions);

			var detectedIdentifiers = target.DetectIdentifiers("(int x, string @class) => x + @class");
			Assert.That(detectedIdentifiers.UnknownIdentifiers, Is.Empty);

			Assert.That(detectedIdentifiers.Types.Count(), Is.EqualTo(2));
			Assert.That(detectedIdentifiers.Types.ElementAt(0).Name, Is.EqualTo("int"));
			Assert.That(detectedIdentifiers.Types.ElementAt(0).Type, Is.EqualTo(typeof(int)));
			Assert.That(detectedIdentifiers.Types.ElementAt(1).Name, Is.EqualTo("string"));
			Assert.That(detectedIdentifiers.Types.ElementAt(1).Type, Is.EqualTo(typeof(string)));

			Assert.That(detectedIdentifiers.Identifiers.Count(), Is.EqualTo(2));

			Assert.That(detectedIdentifiers.Identifiers.ElementAt(0).Name, Is.EqualTo("x"));
			Assert.That(detectedIdentifiers.Identifiers.ElementAt(0).Expression.Type, Is.EqualTo(typeof(int)));

			Assert.That(detectedIdentifiers.Identifiers.ElementAt(1).Name, Is.EqualTo("@class"));
			Assert.That(detectedIdentifiers.Identifiers.ElementAt(1).Expression.Type, Is.EqualTo(typeof(string)));
		}

		[Test]
		public void Detect_identifiers_inside_lambda_expression_duplicate_param_name()
		{
			var target = new Interpreter(InterpreterOptions.Default | InterpreterOptions.LambdaExpressions);

			var detectedIdentifiers =
				target.DetectIdentifiers(
					"(x, int y, z, int a) => x.Select(z => z + y).Select((string a, string b) => b)");
			Assert.That(detectedIdentifiers.UnknownIdentifiers, Is.Empty);

			Assert.That(detectedIdentifiers.Types.Count(), Is.EqualTo(2));
			Assert.That(detectedIdentifiers.Types.ElementAt(0).Name, Is.EqualTo("int"));
			Assert.That(detectedIdentifiers.Types.ElementAt(0).Type, Is.EqualTo(typeof(int)));
			Assert.That(detectedIdentifiers.Types.ElementAt(1).Name, Is.EqualTo("string"));
			Assert.That(detectedIdentifiers.Types.ElementAt(1).Type, Is.EqualTo(typeof(string)));

			Assert.That(detectedIdentifiers.Identifiers.Count(), Is.EqualTo(5));

			Assert.That(detectedIdentifiers.Identifiers.ElementAt(0).Name, Is.EqualTo("x"));
			Assert.That(detectedIdentifiers.Identifiers.ElementAt(0).Expression.Type, Is.EqualTo(typeof(object)));

			Assert.That(detectedIdentifiers.Identifiers.ElementAt(1).Name, Is.EqualTo("y"));
			Assert.That(detectedIdentifiers.Identifiers.ElementAt(1).Expression.Type, Is.EqualTo(typeof(int)));

			Assert.That(detectedIdentifiers.Identifiers.ElementAt(2).Name, Is.EqualTo("z"));
			Assert.That(detectedIdentifiers.Identifiers.ElementAt(2).Expression.Type, Is.EqualTo(typeof(object)));

			Assert.That(detectedIdentifiers.Identifiers.ElementAt(3).Name, Is.EqualTo("a"));
			Assert.That(detectedIdentifiers.Identifiers.ElementAt(3).Expression.Type, Is.EqualTo(typeof(object)));

			Assert.That(detectedIdentifiers.Identifiers.ElementAt(4).Name, Is.EqualTo("b"));
			Assert.That(detectedIdentifiers.Identifiers.ElementAt(4).Expression.Type, Is.EqualTo(typeof(string)));
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

			Assert.That(detectedIdentifiers.UnknownIdentifiers.Count(), Is.EqualTo(1));
			Assert.That(detectedIdentifiers.UnknownIdentifiers.ElementAt(0), Is.EqualTo(identifier));
		}

		[Test]
		public void Dont_detect_members_with_at()
		{
			var code = "@class.@if()";

			var target = new Interpreter(InterpreterOptions.Default | InterpreterOptions.LambdaExpressions);
			var detectedIdentifiers = target.DetectIdentifiers(code);

			// @class should be detected as an identifier, but not the @if because it's a member
			Assert.That(detectedIdentifiers.UnknownIdentifiers.Count(), Is.EqualTo(1));
			Assert.That(detectedIdentifiers.UnknownIdentifiers.ElementAt(0), Is.EqualTo("@class"));
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
			Assert.That(detectedIdentifiers.UnknownIdentifiers, Is.Empty);
		}
	}
}
