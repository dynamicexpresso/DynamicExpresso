using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Globalization;

namespace DynamicExpresso.UnitTest
{
	[TestClass]
	public class LiteralsTest
	{
		[TestMethod]
		public void Alphabetic_Literals()
		{
			var target = new Interpreter();

			Assert.AreEqual("ciao", target.Eval("\"ciao\""));
			Assert.AreEqual('c', target.Eval("'c'"));
		}

		[TestMethod]
		public void True_False_Literals()
		{
			var target = new Interpreter();

			Assert.IsTrue((bool)target.Eval("true"));
			Assert.IsFalse((bool)target.Eval("false"));
		}

		[TestMethod]
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
			Assert.AreEqual(45.8f, target.Eval("45.8f"));
			Assert.AreEqual(45.8F, target.Eval("45.8F"));
			Assert.AreEqual(45.232M, target.Eval("45.232M"));
			Assert.AreEqual(45.232m, target.Eval("45.232m"));
		}

		[TestMethod]
		[ExpectedException(typeof(ParseException))]
		public void Invalid_Numeric_Literals_multiple_dots()
		{
			var target = new Interpreter();

			target.Eval("45.5.456");
		}

		[TestMethod]
		[ExpectedException(typeof(ParseException))]
		public void Invalid_Numeric_Literals_wrong_suffix_x()
		{
			var target = new Interpreter();

			target.Eval("45.5x");
		}

		[TestMethod]
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
		}

		[TestMethod]
		public void Long_strings()
		{
			var target = new Interpreter();
			Assert.AreEqual("Project not selected to build for this solution configuration", target.Eval("\"Project not selected to build for this solution configuration\""));
		}

		[TestMethod]
		public void Null_Keyword()
		{
			var target = new Interpreter();
			Assert.IsNull(target.Eval("null"));
		}

		[TestMethod]
		public void Empty_String()
		{
			var target = new Interpreter();
			Assert.AreEqual(string.Empty, target.Eval("\"\""));
		}

		[TestMethod]
		public void Whitespace_String()
		{
			var target = new Interpreter();
			Assert.AreEqual(" ", target.Eval("\" \""));
			Assert.AreEqual(" \t ", target.Eval("\" \t \""));
			Assert.AreEqual("   ", target.Eval("\"   \""));
			Assert.AreEqual(" \r\n", target.Eval("\" \r\n\""));
		}

		[TestMethod]
		public void Single_quote_inside_a_string()
		{
			var target = new Interpreter();

			Assert.AreEqual("l'aquila", target.Eval("\"l'aquila\""));
		}

		[TestMethod]
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

		[TestMethod]
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

		[TestMethod]
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

		[ExpectedException(typeof(ParseException))]
		[TestMethod]
		public void Invalid_Escape_char()
		{
			var target = new Interpreter();

			target.Eval("'\\'");
		}

		[ExpectedException(typeof(ParseException))]
		[TestMethod]
		public void Invalid_Escape_string()
		{
			var target = new Interpreter();

			target.Eval("\"\\\"");
		}

		[ExpectedException(typeof(ParseException))]
		[TestMethod]
		public void Character_Literal_Must_be_closed()
		{
			var target = new Interpreter();

			target.Eval("'1");
		}

		[ExpectedException(typeof(ParseException))]
		[TestMethod]
		public void String_Literal_Must_be_closed()
		{
			var target = new Interpreter();

			target.Eval("\"1");
		}

		[ExpectedException(typeof(ParseException))]
		[TestMethod]
		public void Character_Literal_Must_one_char_maximum()
		{
			var target = new Interpreter();

			target.Eval("'12'");
		}

		[ExpectedException(typeof(ParseException))]
		[TestMethod]
		public void Character_Literal_Must_one_char_minimum()
		{
			var target = new Interpreter();

			target.Eval("''");
		}

		[TestMethod]
		public void Should_Understand_ReturnType_Of_Literals()
		{
			var target = new Interpreter();

			Assert.AreEqual(typeof(string), target.Parse("\"some string\"").ReturnType);
			Assert.AreEqual(typeof(string), target.Parse("\"\"").ReturnType);
			Assert.AreEqual(typeof(int), target.Parse("234").ReturnType);
			Assert.AreEqual(typeof(double), target.Parse("234.54").ReturnType);
			Assert.AreEqual(typeof(float), target.Parse("4.5f").ReturnType);
			Assert.AreEqual(typeof(float), target.Parse("4.5F").ReturnType);
			Assert.AreEqual(typeof(decimal), target.Parse("234.48m").ReturnType);
			Assert.AreEqual(typeof(decimal), target.Parse("234.48M").ReturnType);
			Assert.AreEqual(typeof(object), target.Parse("null").ReturnType);

			Assert.AreEqual((45.5).GetType(), target.Eval("45.5").GetType());
			Assert.AreEqual((45.8f).GetType(), target.Eval("45.8f").GetType());
			Assert.AreEqual((45.232M).GetType(), target.Eval("45.232M").GetType());

		}

		[TestMethod]
		public void Thread_Culture_WithUS_Culture_is_ignored_for_literals()
		{
			var originalCulture = Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
			var target = new Interpreter();
			Assert.AreEqual(45.5, target.Eval("45.5"));
			Thread.CurrentThread.CurrentCulture = originalCulture;
		}

		[TestMethod]
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
