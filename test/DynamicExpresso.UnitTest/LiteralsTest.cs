using NUnit.Framework;
using System.Threading;
using System.Globalization;

namespace DynamicExpresso.UnitTest
{
	[TestFixture]
	public class LiteralsTest
	{
		[Test]
		public void Alphabetic_Literals()
		{
			var target = new Interpreter();

			Assert.AreEqual("ciao", target.Eval("\"ciao\""));
			Assert.AreEqual('c', target.Eval("'c'"));
		}

		[Test]
		public void True_False_Literals()
		{
			var target = new Interpreter();

			Assert.IsTrue((bool)target.Eval("true"));
			Assert.IsFalse((bool)target.Eval("false"));
		}

		[Test]
		public void Numeric_Literals()
		{
			var target = new Interpreter();

			Assert.AreEqual(0, target.Eval("0"));
			Assert.AreEqual(0.0, target.Eval("0.0"));
			Assert.AreEqual(45, target.Eval("45"));
			Assert.AreEqual(-565, target.Eval("-565"));
			Assert.AreEqual(23423423423434, target.Eval("23423423423434"));
			Assert.AreEqual(45.5, target.Eval("45.5"));
			Assert.AreEqual(-0.5, target.Eval("-0.5"));
			Assert.AreEqual(.2, target.Eval(".2"));
			Assert.AreEqual(-.2, target.Eval("-.2"));
			Assert.AreEqual(+.2, target.Eval("+.2"));
			Assert.AreEqual(.02, target.Eval(".02"));
			Assert.AreEqual(-.02, target.Eval("-.02"));
			Assert.AreEqual(+.02, target.Eval("+.02"));
			Assert.AreEqual(.20, target.Eval(".20"));
			Assert.AreEqual(-.20, target.Eval("-.20"));
			Assert.AreEqual(+.20, target.Eval("+.20"));
			Assert.AreEqual(.201, target.Eval(".201"));
			Assert.AreEqual(-.201, target.Eval("-.201"));
			Assert.AreEqual(+.201, target.Eval("+.201"));
			Assert.AreEqual(45.8f, target.Eval("45.8f"));
			Assert.AreEqual(45.8F, target.Eval("45.8F"));
			Assert.AreEqual(45.8F, target.Eval(" 45.8F "));
			Assert.AreEqual(.2f, target.Eval(".2f"));
			Assert.AreEqual(.2F, target.Eval(".2F"));
			Assert.AreEqual(-.2f, target.Eval("-.2f"));
			Assert.AreEqual(-.2F, target.Eval("-.2F"));
			Assert.AreEqual(45.232M, target.Eval("45.232M"));
			Assert.AreEqual(45.232m, target.Eval("45.232m"));
			Assert.AreEqual(.022M, target.Eval(".022M"));
			Assert.AreEqual(.022m, target.Eval(".022m"));
			Assert.AreEqual(-.022m, target.Eval("-.022m"));
			Assert.AreEqual(-.022M, target.Eval("-.022M"));
		}

		[Test]
		public void Invalid_Numeric_Literals_multiple_dots()
		{
			var target = new Interpreter();

			Assert.Throws<ParseException>(() => target.Eval("45.5.456"));
		}

		[Test]
		public void Invalid_Numeric_Literals_wrong_space()
		{
			var target = new Interpreter();

			Assert.Throws<ParseException>(() => target.Eval(".2 F"));
		}

		[Test]
		public void Invalid_Numeric_Literals_wrong_space_before_point()
		{
			var target = new Interpreter();

			Assert.Throws<ParseException>(() => target.Eval(". 2"));
		}

		[Test]
		public void Invalid_Numeric_Literals_multiple_suffix()
		{
			var target = new Interpreter();

			Assert.Throws<ParseException>(() => target.Eval("45.5M456F"));
		}

		[Test]
		public void Invalid_Numeric_Literals_wrong_suffix_x()
		{
			var target = new Interpreter();

			Assert.Throws<ParseException>(() => target.Eval("45.5x"));
		}

		[Test]
		public void Calling_System_Method_On_Literals()
		{
			var target = new Interpreter();

			Assert.AreEqual("ciao".GetType(), target.Eval("\"ciao\".GetType()"));
			Assert.AreEqual('c'.GetType(), target.Eval("'c'.GetType()"));
			Assert.AreEqual(true.GetType(), target.Eval("true.GetType()"));
			Assert.AreEqual(false.GetType(), target.Eval("false.GetType()"));

			Assert.AreEqual(45.GetType(), target.Eval("45.GetType()"));
			Assert.AreEqual(23423423423434.GetType(), target.Eval("23423423423434.GetType()"));
			Assert.AreEqual(45.5.GetType(), target.Eval("45.5.GetType()"));
			Assert.AreEqual(45.8f.GetType(), target.Eval("45.8f.GetType()"));
			Assert.AreEqual(45.232M.GetType(), target.Eval("45.232M.GetType()"));

			// Note: in C# I cannot compile "-565.GetType()" , I need to add parentheses
			Assert.AreEqual((-565).GetType(), target.Eval("-565.GetType()"));
			Assert.AreEqual((-0.5).GetType(), target.Eval("-0.5.GetType()"));

			Assert.AreEqual((-.5).GetType(), target.Eval("-.5.GetType()"));
			Assert.AreEqual((-.5f).GetType(), target.Eval("-.5f.GetType()"));

			Assert.AreEqual((+.5).GetType(), target.Eval("+.5.GetType()"));
			Assert.AreEqual((+.5f).GetType(), target.Eval("+.5f.GetType()"));
		}

		[Test]
		public void Long_strings()
		{
			var target = new Interpreter();
			Assert.AreEqual(
				"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.",
				target.Eval("\"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.\""));
		}

		[Test]
		public void Null_Keyword()
		{
			var target = new Interpreter();
			Assert.IsNull(target.Eval("null"));
		}

		[Test]
		public void Empty_String()
		{
			var target = new Interpreter();
			Assert.AreEqual(string.Empty, target.Eval("\"\""));
		}

		[Test]
		public void Whitespace_String()
		{
			var target = new Interpreter();
			Assert.AreEqual(" ", target.Eval("\" \""));
			Assert.AreEqual(" \t ", target.Eval("\" \t \""));
			Assert.AreEqual("   ", target.Eval("\"   \""));
			Assert.AreEqual(" \r\n", target.Eval("\" \r\n\""));
		}

		[Test]
		public void Single_quote_inside_a_string()
		{
			var target = new Interpreter();

			Assert.AreEqual("l'aquila", target.Eval("\"l'aquila\""));
		}

		[Test]
		public void Non_Ascii_Chars()
		{
			var target = new Interpreter();

			Assert.AreEqual("汉语/漢語", target.Eval("\"汉语/漢語\""));
			Assert.AreEqual('汉', target.Eval("'汉'"));

			for (char c = char.MinValue; c < char.MaxValue; c++)
			{
				if (c != '\"' && c != '\\')
					Assert.AreEqual(new string(c, 1), target.Eval(string.Format("\"{0}\"", c)), string.Format("Failed to parse string literals \"{0}\".", c));
				if (c != '\'' && c != '\\')
					Assert.AreEqual(c, target.Eval(string.Format("'{0}'", c)), string.Format("Failed to parse char literals '{0}'.", c));
			}
		}

		[Test]
		public void Escape_chars_in_char()
		{
			var target = new Interpreter();

			Assert.AreEqual('\'', target.Eval("'\\''"));
			Assert.AreEqual('\"', target.Eval("'\\\"'"));
			Assert.AreEqual('\\', target.Eval("'\\\\'"));
			Assert.AreEqual('\0', target.Eval("'\\0'"));
			Assert.AreEqual('\a', target.Eval("'\\a'"));
			Assert.AreEqual('\b', target.Eval("'\\b'"));
			Assert.AreEqual('\f', target.Eval("'\\f'"));
			Assert.AreEqual('\n', target.Eval("'\\n'"));
			Assert.AreEqual('\r', target.Eval("'\\r'"));
			Assert.AreEqual('\t', target.Eval("'\\t'"));
			Assert.AreEqual('\v', target.Eval("'\\v'"));
		}

		[Test]
		public void Escape_chars_in_string()
		{
			var target = new Interpreter();

			Assert.AreEqual("\'", target.Eval("\"\\'\""));
			Assert.AreEqual("\"", target.Eval("\"\\\"\""));
			Assert.AreEqual("\\", target.Eval("\"\\\\\""));
			Assert.AreEqual("\0", target.Eval("\"\\0\""));
			Assert.AreEqual("\a", target.Eval("\"\\a\""));
			Assert.AreEqual("\b", target.Eval("\"\\b\""));
			Assert.AreEqual("\f", target.Eval("\"\\f\""));
			Assert.AreEqual("\n", target.Eval("\"\\n\""));
			Assert.AreEqual("\r", target.Eval("\"\\r\""));
			Assert.AreEqual("\t", target.Eval("\"\\t\""));
			Assert.AreEqual("\v", target.Eval("\"\\v\""));

			Assert.AreEqual("L\'aquila\r\n\tè\tbella.", target.Eval("\"L\\'aquila\\r\\n\\tè\\tbella.\""));
		}

		[Test]
		public void Invalid_Escape_char()
		{
			var target = new Interpreter();

			Assert.Throws<ParseException>(() => target.Eval("'\\'"));
		}

		[Test]
		public void Invalid_Escape_string()
		{
			var target = new Interpreter();

			Assert.Throws<ParseException>(() => target.Eval("\"\\\""));
		}

		[Test]
		public void Character_Literal_Must_be_closed()
		{
			var target = new Interpreter();

			Assert.Throws<ParseException>(() => target.Eval("'1"));
		}

		[Test]
		public void String_Literal_Must_be_closed()
		{
			var target = new Interpreter();

			Assert.Throws<ParseException>(() => target.Eval("\"1"));
		}

		[Test]
		public void Character_Literal_Must_one_char_maximum()
		{
			var target = new Interpreter();

			Assert.Throws<ParseException>(() => target.Eval("'12'"));
		}

		[Test]
		public void Character_Literal_Must_one_char_minimum()
		{
			var target = new Interpreter();

			Assert.Throws<ParseException>(() => target.Eval("''"));
		}

		[Test]
		public void Should_Understand_ReturnType_Of_Literals()
		{
			var target = new Interpreter();

			Assert.AreEqual(typeof(string), target.Parse("\"some string\"").ReturnType);
			Assert.AreEqual(typeof(string), target.Parse("\"\"").ReturnType);
			Assert.AreEqual(typeof(int), target.Parse("234").ReturnType);
			Assert.AreEqual(typeof(double), target.Parse("234.54").ReturnType);
			Assert.AreEqual(typeof(double), target.Parse(".9").ReturnType);
			Assert.AreEqual(typeof(double), target.Parse("-.9").ReturnType);
			Assert.AreEqual(typeof(float), target.Parse("4.5f").ReturnType);
			Assert.AreEqual(typeof(float), target.Parse("4.5F").ReturnType);
			Assert.AreEqual(typeof(float), target.Parse(".5f").ReturnType);
			Assert.AreEqual(typeof(float), target.Parse(".5F").ReturnType);
			Assert.AreEqual(typeof(decimal), target.Parse("234.48m").ReturnType);
			Assert.AreEqual(typeof(decimal), target.Parse("234.48M").ReturnType);
			Assert.AreEqual(typeof(decimal), target.Parse(".48m").ReturnType);
			Assert.AreEqual(typeof(decimal), target.Parse(".48M").ReturnType);
			Assert.AreEqual(typeof(object), target.Parse("null").ReturnType);

			Assert.AreEqual((45.5).GetType(), target.Eval("45.5").GetType());
			Assert.AreEqual((45.8f).GetType(), target.Eval("45.8f").GetType());
			Assert.AreEqual((45.232M).GetType(), target.Eval("45.232M").GetType());

		}

		[Test]
		public void Thread_Culture_WithUS_Culture_is_ignored_for_literals()
		{
			var originalCulture = Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
			var target = new Interpreter();
			Assert.AreEqual(45.5, target.Eval("45.5"));
			Thread.CurrentThread.CurrentCulture = originalCulture;
		}

		[Test]
		public void Thread_Culture_WithIT_Culture_is_ignored_for_literals()
		{
			var originalCulture = Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("it-IT");
			var target = new Interpreter();
			Assert.AreEqual(45.5, target.Eval("45.5"));
			Thread.CurrentThread.CurrentCulture = originalCulture;
		}

	}
}
