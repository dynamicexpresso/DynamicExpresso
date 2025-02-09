using System;
using System.Globalization;
using System.Threading;
using DynamicExpresso.Exceptions;
using NUnit.Framework;

namespace DynamicExpresso.UnitTest
{
	[TestFixture]
	public class LiteralsTest
	{
		[Test]
		public void Alphabetic_Literals()
		{
			var target = new Interpreter();

			Assert.That(target.Eval("\"ciao\""), Is.EqualTo("ciao"));
			Assert.That(target.Eval("'c'"), Is.EqualTo('c'));
		}

		[Test]
		public void True_False_Literals()
		{
			var target = new Interpreter();

			Assert.That((bool)target.Eval("true"), Is.True);
			Assert.That((bool)target.Eval("false"), Is.False);
		}

		[Test]
		public void Numeric_Literals()
		{
			var target = new Interpreter();

			Assert.That(target.Eval("0"), Is.EqualTo(0));
			Assert.That(target.Eval("0.0"), Is.EqualTo(0.0));
			Assert.That(target.Eval("45"), Is.EqualTo(45));
			Assert.That(target.Eval("45u"), Is.EqualTo(45));
			Assert.That(target.Eval("-45u"), Is.EqualTo(-45u));
			Assert.That(target.Eval("-565"), Is.EqualTo(-565));
			Assert.That(target.Eval("23423423423434"), Is.EqualTo(23423423423434));
			Assert.That(target.Eval("45.5"), Is.EqualTo(45.5));
			Assert.That(target.Eval("-0.5"), Is.EqualTo(-0.5));
			Assert.That(target.Eval(".2"), Is.EqualTo(.2));
			Assert.That(target.Eval("-.2"), Is.EqualTo(-.2));
			Assert.That(target.Eval("+.2"), Is.EqualTo(+.2));
			Assert.That(target.Eval(".02"), Is.EqualTo(.02));
			Assert.That(target.Eval("-.02"), Is.EqualTo(-.02));
			Assert.That(target.Eval("+.02"), Is.EqualTo(+.02));
			Assert.That(target.Eval(".20"), Is.EqualTo(.20));
			Assert.That(target.Eval("-.20"), Is.EqualTo(-.20));
			Assert.That(target.Eval("+.20"), Is.EqualTo(+.20));
			Assert.That(target.Eval(".201"), Is.EqualTo(.201));
			Assert.That(target.Eval("-.201"), Is.EqualTo(-.201));
			Assert.That(target.Eval("+.201"), Is.EqualTo(+.201));
			Assert.That(target.Eval("2e+201"), Is.EqualTo(2e+201));
			Assert.That(target.Eval("2e+20"), Is.EqualTo(2e+20));

			// f suffix (single)
			Assert.That(target.Eval("4f"), Is.EqualTo(4f));
			Assert.That(target.Eval("45F"), Is.EqualTo(45F));
			Assert.That(target.Eval("45.8f"), Is.EqualTo(45.8f));
			Assert.That(target.Eval("45.8F"), Is.EqualTo(45.8F));
			Assert.That(target.Eval(" 45.8F "), Is.EqualTo(45.8F));
			Assert.That(target.Eval(".2f"), Is.EqualTo(.2f));
			Assert.That(target.Eval(".2F"), Is.EqualTo(.2F));
			Assert.That(target.Eval("-.2f"), Is.EqualTo(-.2f));
			Assert.That(target.Eval("-.2F"), Is.EqualTo(-.2F));

			// m suffix (decimal)
			Assert.That(target.Eval("5M"), Is.EqualTo(5M));
			Assert.That(target.Eval("254m"), Is.EqualTo(254m));
			Assert.That(target.Eval("45.232M"), Is.EqualTo(45.232M));
			Assert.That(target.Eval("45.232m"), Is.EqualTo(45.232m));
			Assert.That(target.Eval(".022M"), Is.EqualTo(.022M));
			Assert.That(target.Eval(".022m"), Is.EqualTo(.022m));
			Assert.That(target.Eval("-.022m"), Is.EqualTo(-.022m));
			Assert.That(target.Eval("-.022M"), Is.EqualTo(-.022M));
		}

		[Test]
		public void Numeric_Literals_DefaultTypes()
		{
			var target = new Interpreter();

			Assert.That(target.Eval("81"), Is.InstanceOf<int>());
			Assert.That(target.Eval("81.5"), Is.InstanceOf<double>());
			Assert.That(target.Eval("23423423423434"), Is.InstanceOf<long>());
		}

		[Test]
		public void Numeric_Literals_DefaultLong()
		{
			var target = new Interpreter();

			target.SetDefaultNumberType(DefaultNumberType.Long);

			Assert.That(target.Eval("45"), Is.InstanceOf<long>());

			Assert.That(target.Eval("0"), Is.EqualTo(0L));
			Assert.That(target.Eval("0.0"), Is.EqualTo(0.0));
			Assert.That(target.Eval("45"), Is.EqualTo(45L));
			Assert.That(target.Eval("45u"), Is.EqualTo(45L));
			Assert.That(target.Eval("-45u"), Is.EqualTo(-45L));
			Assert.That(target.Eval("23423423423434"), Is.EqualTo(23423423423434L));
			Assert.That(target.Eval("45.5"), Is.EqualTo(45.5));
			Assert.That(target.Eval("-0.5"), Is.EqualTo(-0.5));
			Assert.That(target.Eval(".2"), Is.EqualTo(.2));
			Assert.That(target.Eval("-.2"), Is.EqualTo(-.2));
			Assert.That(target.Eval("+.2"), Is.EqualTo(+.2));
			Assert.That(target.Eval(".02"), Is.EqualTo(.02));
			Assert.That(target.Eval("-.02"), Is.EqualTo(-.02));
			Assert.That(target.Eval("+.02"), Is.EqualTo(+.02));
			Assert.That(target.Eval(".20"), Is.EqualTo(.20));
			Assert.That(target.Eval("-.20"), Is.EqualTo(-.20));
			Assert.That(target.Eval("+.20"), Is.EqualTo(+.20));
			Assert.That(target.Eval(".201"), Is.EqualTo(.201));
			Assert.That(target.Eval("-.201"), Is.EqualTo(-.201));
			Assert.That(target.Eval("+.201"), Is.EqualTo(+.201));
			Assert.That(target.Eval("2e+201"), Is.EqualTo(2e+201));
			Assert.That(target.Eval("2e+20"), Is.EqualTo(2e+20));

			// f suffix (single)
			Assert.That(target.Eval("4f"), Is.EqualTo(4f));
			Assert.That(target.Eval("45F"), Is.EqualTo(45F));
			Assert.That(target.Eval("45.8f"), Is.EqualTo(45.8f));
			Assert.That(target.Eval("45.8F"), Is.EqualTo(45.8F));
			Assert.That(target.Eval(" 45.8F "), Is.EqualTo(45.8F));
			Assert.That(target.Eval(".2f"), Is.EqualTo(.2f));
			Assert.That(target.Eval(".2F"), Is.EqualTo(.2F));
			Assert.That(target.Eval("-.2f"), Is.EqualTo(-.2f));
			Assert.That(target.Eval("-.2F"), Is.EqualTo(-.2F));

			// m suffix (decimal)
			Assert.That(target.Eval("5M"), Is.EqualTo(5M));
			Assert.That(target.Eval("254m"), Is.EqualTo(254m));
			Assert.That(target.Eval("45.232M"), Is.EqualTo(45.232M));
			Assert.That(target.Eval("45.232m"), Is.EqualTo(45.232m));
			Assert.That(target.Eval(".022M"), Is.EqualTo(.022M));
			Assert.That(target.Eval(".022m"), Is.EqualTo(.022m));
			Assert.That(target.Eval("-.022m"), Is.EqualTo(-.022m));
			Assert.That(target.Eval("-.022M"), Is.EqualTo(-.022M));
		}

		[Test]
		public void Numeric_Literals_DefaultSingle()
		{
			var target = new Interpreter();

			target.SetDefaultNumberType(DefaultNumberType.Single);

			Assert.That(target.Eval("45"), Is.InstanceOf<Single>());
			Assert.That(target.Eval("10/3"), Is.EqualTo(10F / 3f));

			Assert.That(target.Eval("0"), Is.EqualTo(0F));
			Assert.That(target.Eval("0.0"), Is.EqualTo(0.0F));
			Assert.That(target.Eval("45"), Is.EqualTo(45F));
			Assert.That(target.Eval("-565"), Is.EqualTo(-565F));
			Assert.That(target.Eval("23423423423434"), Is.EqualTo(23423423423434F));
			Assert.That(target.Eval("45.5"), Is.EqualTo(45.5F));
			Assert.That(target.Eval("-0.5"), Is.EqualTo(-0.5F));
			Assert.That(target.Eval(".2"), Is.EqualTo(.2F));
			Assert.That(target.Eval("-.2"), Is.EqualTo(-.2F));
			Assert.That(target.Eval("+.2"), Is.EqualTo(+.2F));
			Assert.That(target.Eval(".02"), Is.EqualTo(.02F));
			Assert.That(target.Eval("-.02"), Is.EqualTo(-.02F));
			Assert.That(target.Eval("+.02"), Is.EqualTo(+.02F));
			Assert.That(target.Eval(".20"), Is.EqualTo(.20F));
			Assert.That(target.Eval("-.20"), Is.EqualTo(-.20F));
			Assert.That(target.Eval("+.20"), Is.EqualTo(+.20F));
			Assert.That(target.Eval(".201"), Is.EqualTo(.201F));
			Assert.That(target.Eval("-.201"), Is.EqualTo(-.201F));
			Assert.That(target.Eval("+.201"), Is.EqualTo(+.201F));

			// f suffix (single)
			Assert.That(target.Eval("4f"), Is.EqualTo(4f));
			Assert.That(target.Eval("45F"), Is.EqualTo(45F));
			Assert.That(target.Eval("45.8f"), Is.EqualTo(45.8f));
			Assert.That(target.Eval("45.8F"), Is.EqualTo(45.8F));
			Assert.That(target.Eval(" 45.8F "), Is.EqualTo(45.8F));
			Assert.That(target.Eval(".2f"), Is.EqualTo(.2f));
			Assert.That(target.Eval(".2F"), Is.EqualTo(.2F));
			Assert.That(target.Eval("-.2f"), Is.EqualTo(-.2f));
			Assert.That(target.Eval("-.2F"), Is.EqualTo(-.2F));

			// m suffix (decimal)
			Assert.That(target.Eval("5M"), Is.EqualTo(5M));
			Assert.That(target.Eval("254m"), Is.EqualTo(254m));
			Assert.That(target.Eval("45.232M"), Is.EqualTo(45.232M));
			Assert.That(target.Eval("45.232m"), Is.EqualTo(45.232m));
			Assert.That(target.Eval(".022M"), Is.EqualTo(.022M));
			Assert.That(target.Eval(".022m"), Is.EqualTo(.022m));
			Assert.That(target.Eval("-.022m"), Is.EqualTo(-.022m));
			Assert.That(target.Eval("-.022M"), Is.EqualTo(-.022M));
		}

		[Test]
		public void Numeric_Literals_DefaultDouble()
		{
			var target = new Interpreter();

			target.SetDefaultNumberType(DefaultNumberType.Double);

			Assert.That(target.Eval("45"), Is.InstanceOf<double>());
			Assert.That(target.Eval("10/3"), Is.EqualTo(10D / 3D));

			Assert.That(target.Eval("0"), Is.EqualTo(0D));
			Assert.That(target.Eval("0.0"), Is.EqualTo(0.0D));
			Assert.That(target.Eval("45"), Is.EqualTo(45D));
			Assert.That(target.Eval("-565"), Is.EqualTo(-565D));
			Assert.That(target.Eval("23423423423434"), Is.EqualTo(23423423423434D));
			Assert.That(target.Eval("45.5"), Is.EqualTo(45.5D));
			Assert.That(target.Eval("-0.5"), Is.EqualTo(-0.5D));
			Assert.That(target.Eval(".2"), Is.EqualTo(.2D));
			Assert.That(target.Eval("-.2"), Is.EqualTo(-.2D));
			Assert.That(target.Eval("+.2"), Is.EqualTo(+.2D));
			Assert.That(target.Eval(".02"), Is.EqualTo(.02D));
			Assert.That(target.Eval("-.02"), Is.EqualTo(-.02D));
			Assert.That(target.Eval("+.02"), Is.EqualTo(+.02D));
			Assert.That(target.Eval(".20"), Is.EqualTo(.20D));
			Assert.That(target.Eval("-.20"), Is.EqualTo(-.20D));
			Assert.That(target.Eval("+.20"), Is.EqualTo(+.20D));
			Assert.That(target.Eval(".201"), Is.EqualTo(.201D));
			Assert.That(target.Eval("-.201"), Is.EqualTo(-.201D));
			Assert.That(target.Eval("+.201"), Is.EqualTo(+.201D));
			Assert.That(target.Eval("2e+201"), Is.EqualTo(2e+201));
			Assert.That(target.Eval("2e+20"), Is.EqualTo(2e+20));

			// f suffix (single)
			Assert.That(target.Eval("4f"), Is.EqualTo(4f));
			Assert.That(target.Eval("45F"), Is.EqualTo(45F));
			Assert.That(target.Eval("45.8f"), Is.EqualTo(45.8f));
			Assert.That(target.Eval("45.8F"), Is.EqualTo(45.8F));
			Assert.That(target.Eval(" 45.8F "), Is.EqualTo(45.8F));
			Assert.That(target.Eval(".2f"), Is.EqualTo(.2f));
			Assert.That(target.Eval(".2F"), Is.EqualTo(.2F));
			Assert.That(target.Eval("-.2f"), Is.EqualTo(-.2f));
			Assert.That(target.Eval("-.2F"), Is.EqualTo(-.2F));

			// m suffix (decimal)
			Assert.That(target.Eval("5M"), Is.EqualTo(5M));
			Assert.That(target.Eval("254m"), Is.EqualTo(254m));
			Assert.That(target.Eval("45.232M"), Is.EqualTo(45.232M));
			Assert.That(target.Eval("45.232m"), Is.EqualTo(45.232m));
			Assert.That(target.Eval(".022M"), Is.EqualTo(.022M));
			Assert.That(target.Eval(".022m"), Is.EqualTo(.022m));
			Assert.That(target.Eval("-.022m"), Is.EqualTo(-.022m));
			Assert.That(target.Eval("-.022M"), Is.EqualTo(-.022M));
		}

		[Test]
		public void Numeric_Literals_DefaultDecimal()
		{
			var target = new Interpreter();

			target.SetDefaultNumberType(DefaultNumberType.Decimal);

			Assert.That(target.Eval("45"), Is.InstanceOf<Decimal>());
			Assert.That(target.Eval("10/3"), Is.EqualTo(10M / 3M));

			Assert.That(target.Eval("0"), Is.EqualTo(0M));
			Assert.That(target.Eval("0.0"), Is.EqualTo(0.0M));
			Assert.That(target.Eval("45"), Is.EqualTo(45M));
			Assert.That(target.Eval("-565"), Is.EqualTo(-565M));
			Assert.That(target.Eval("23423423423434"), Is.EqualTo(23423423423434M));
			Assert.That(target.Eval("45.5"), Is.EqualTo(45.5M));
			Assert.That(target.Eval("-0.5"), Is.EqualTo(-0.5M));
			Assert.That(target.Eval(".2"), Is.EqualTo(.2M));
			Assert.That(target.Eval("-.2"), Is.EqualTo(-.2M));
			Assert.That(target.Eval("+.2"), Is.EqualTo(+.2M));
			Assert.That(target.Eval(".02"), Is.EqualTo(.02M));
			Assert.That(target.Eval("-.02"), Is.EqualTo(-.02M));
			Assert.That(target.Eval("+.02"), Is.EqualTo(+.02M));
			Assert.That(target.Eval(".20"), Is.EqualTo(.20M));
			Assert.That(target.Eval("-.20"), Is.EqualTo(-.20M));
			Assert.That(target.Eval("+.20"), Is.EqualTo(+.20M));
			Assert.That(target.Eval(".201"), Is.EqualTo(.201M));
			Assert.That(target.Eval("-.201"), Is.EqualTo(-.201M));
			Assert.That(target.Eval("+.201"), Is.EqualTo(+.201M));

			// f suffix (single)
			Assert.That(target.Eval("4f"), Is.EqualTo(4f));
			Assert.That(target.Eval("45F"), Is.EqualTo(45F));
			Assert.That(target.Eval("45.8f"), Is.EqualTo(45.8f));
			Assert.That(target.Eval("45.8F"), Is.EqualTo(45.8F));
			Assert.That(target.Eval(" 45.8F "), Is.EqualTo(45.8F));
			Assert.That(target.Eval(".2f"), Is.EqualTo(.2f));
			Assert.That(target.Eval(".2F"), Is.EqualTo(.2F));
			Assert.That(target.Eval("-.2f"), Is.EqualTo(-.2f));
			Assert.That(target.Eval("-.2F"), Is.EqualTo(-.2F));

			// m suffix (decimal)
			Assert.That(target.Eval("5M"), Is.EqualTo(5M));
			Assert.That(target.Eval("254m"), Is.EqualTo(254m));
			Assert.That(target.Eval("45.232M"), Is.EqualTo(45.232M));
			Assert.That(target.Eval("45.232m"), Is.EqualTo(45.232m));
			Assert.That(target.Eval(".022M"), Is.EqualTo(.022M));
			Assert.That(target.Eval(".022m"), Is.EqualTo(.022m));
			Assert.That(target.Eval("-.022m"), Is.EqualTo(-.022m));
			Assert.That(target.Eval("-.022M"), Is.EqualTo(-.022M));
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
			Assert.Throws<ParseException>(() => target.Eval("4.2 F"));
			Assert.Throws<ParseException>(() => target.Eval("6.2 f"));
			Assert.Throws<ParseException>(() => target.Eval("2 F"));
			Assert.Throws<ParseException>(() => target.Eval("2 f"));
			Assert.Throws<ParseException>(() => target.Eval("2 M"));
			Assert.Throws<ParseException>(() => target.Eval("2 m"));
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
			Assert.Throws<ParseException>(() => target.Eval("45df"));
			Assert.Throws<ParseException>(() => target.Eval("45md"));
			Assert.Throws<ParseException>(() => target.Eval("45uu"));
			Assert.Throws<ParseException>(() => target.Eval("45ud"));
			Assert.Throws<ParseException>(() => target.Eval("45du"));
			Assert.Throws<ParseException>(() => target.Eval("45ull"));
		}

		[Test]
		public void Invalid_Numeric_Literals_wrong_suffix_x()
		{
			var target = new Interpreter();

			Assert.Throws<ParseException>(() => target.Eval("45.5x"));
			Assert.Throws<ParseException>(() => target.Eval("45G"));
		}

		[Test]
		public void Binary_Literals()
		{
			var target = new Interpreter();

			Assert.That(target.Eval("0b101ul"), Is.EqualTo(0b101ul));
			Assert.That(target.Eval("0B1111l"), Is.EqualTo(0B1111L));
			Assert.That(target.Eval("4+0b10+2"), Is.EqualTo(8));

			Assert.Throws<ParseException>(() => target.Eval("0b"));
			Assert.Throws<ParseException>(() => target.Eval("0b12"));
			Assert.Throws<ParseException>(() => target.Eval("0b10.10"));
			Assert.Throws<ParseException>(() => target.Eval("0b10d"));
			Assert.Throws<ParseException>(() => target.Eval("0b10e"));
		}

		[Test]
		public void Hexadecimal_Literals()
		{
			var target = new Interpreter();

			Assert.That(target.Eval("0x012EFul"), Is.EqualTo(0x012EFul));
			Assert.That(target.Eval("0XAAe2l"), Is.EqualTo(0XAAe2L));
			Assert.That(target.Eval("4+0xA1+5"), Is.EqualTo(170));
			Assert.That(target.Eval("4+(0xA1)+5"), Is.EqualTo(170));

			Assert.Throws<ParseException>(() => target.Eval("0x"));
			Assert.Throws<ParseException>(() => target.Eval("0x1Gl"));
			Assert.Throws<ParseException>(() => target.Eval("0x12.12"));
		}

		[Test]
		public void Calling_System_Method_On_Literals()
		{
			var target = new Interpreter();

			Assert.That(target.Eval("\"ciao\".GetType()"), Is.EqualTo("ciao".GetType()));
			Assert.That(target.Eval("'c'.GetType()"), Is.EqualTo('c'.GetType()));
			Assert.That(target.Eval("true.GetType()"), Is.EqualTo(true.GetType()));
			Assert.That(target.Eval("false.GetType()"), Is.EqualTo(false.GetType()));

			Assert.That(target.Eval("45.GetType()"), Is.EqualTo(45.GetType()));
			Assert.That(target.Eval("23423423423434.GetType()"), Is.EqualTo(23423423423434.GetType()));
			Assert.That(target.Eval("45.5.GetType()"), Is.EqualTo(45.5.GetType()));
			Assert.That(target.Eval("45.8f.GetType()"), Is.EqualTo(45.8f.GetType()));
			Assert.That(target.Eval("45.232M.GetType()"), Is.EqualTo(45.232M.GetType()));

			// Note: in C# I cannot compile "-565.GetType()" , I need to add parentheses
			Assert.That(target.Eval("-565.GetType()"), Is.EqualTo((-565).GetType()));
			Assert.That(target.Eval("-0.5.GetType()"), Is.EqualTo((-0.5).GetType()));

			Assert.That(target.Eval("-.5.GetType()"), Is.EqualTo((-.5).GetType()));
			Assert.That(target.Eval("-.5f.GetType()"), Is.EqualTo((-.5f).GetType()));

			Assert.That(target.Eval("+.5.GetType()"), Is.EqualTo((+.5).GetType()));
			Assert.That(target.Eval("+.5f.GetType()"), Is.EqualTo((+.5f).GetType()));
		}

		[Test]
		public void Long_strings()
		{
			var target = new Interpreter();
			Assert.That(
				target.Eval("\"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.\""), Is.EqualTo("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum."));
		}

		[Test]
		public void Null_Keyword()
		{
			var target = new Interpreter();
			Assert.That(target.Eval("null"), Is.Null);
		}

		[Test]
		public void Empty_String()
		{
			var target = new Interpreter();
			Assert.That(target.Eval("\"\""), Is.EqualTo(string.Empty));
		}

		[Test]
		public void Whitespace_String()
		{
			var target = new Interpreter();
			Assert.That(target.Eval("\" \""), Is.EqualTo(" "));
			Assert.That(target.Eval("\" \t \""), Is.EqualTo(" \t "));
			Assert.That(target.Eval("\"   \""), Is.EqualTo("   "));
			Assert.That(target.Eval("\" \r\n\""), Is.EqualTo(" \r\n"));
		}

		[Test]
		public void Single_quote_inside_a_string()
		{
			var target = new Interpreter();

			Assert.That(target.Eval("\"l'aquila\""), Is.EqualTo("l'aquila"));
		}

		[Test]
		public void Non_Ascii_Chars()
		{
			var target = new Interpreter();

			Assert.That(target.Eval("\"汉语/漢語\""), Is.EqualTo("汉语/漢語"));
			Assert.That(target.Eval("'汉'"), Is.EqualTo('汉'));

			for (char c = char.MinValue; c < char.MaxValue; c++)
			{
				if (c != '\"' && c != '\\')
					Assert.That(target.Eval(string.Format("\"{0}\"", c)), Is.EqualTo(new string(c, 1)), string.Format("Failed to parse string literals \"{0}\".", c));
				if (c != '\'' && c != '\\')
					Assert.That(target.Eval(string.Format("'{0}'", c)), Is.EqualTo(c), string.Format("Failed to parse char literals '{0}'.", c));
			}
		}

		[Test]
		public void Escape_chars_in_char()
		{
			var target = new Interpreter();

			Assert.That(target.Eval("'\\''"), Is.EqualTo('\''));
			Assert.That(target.Eval("'\\\"'"), Is.EqualTo('\"'));
			Assert.That(target.Eval("'\\\\'"), Is.EqualTo('\\'));
			Assert.That(target.Eval("'\\0'"), Is.EqualTo('\0'));
			Assert.That(target.Eval("'\\a'"), Is.EqualTo('\a'));
			Assert.That(target.Eval("'\\b'"), Is.EqualTo('\b'));
			Assert.That(target.Eval("'\\f'"), Is.EqualTo('\f'));
			Assert.That(target.Eval("'\\n'"), Is.EqualTo('\n'));
			Assert.That(target.Eval("'\\r'"), Is.EqualTo('\r'));
			Assert.That(target.Eval("'\\t'"), Is.EqualTo('\t'));
			Assert.That(target.Eval("'\\v'"), Is.EqualTo('\v'));
		}

		[Test]
		public void Escape_chars_in_string()
		{
			var target = new Interpreter();

			Assert.That(target.Eval("\"\\'\""), Is.EqualTo("\'"));
			Assert.That(target.Eval("\"\\\"\""), Is.EqualTo("\""));
			Assert.That(target.Eval("\"\\\\\""), Is.EqualTo("\\"));
			Assert.That(target.Eval("\"\\0\""), Is.EqualTo("\0"));
			Assert.That(target.Eval("\"\\a\""), Is.EqualTo("\a"));
			Assert.That(target.Eval("\"\\b\""), Is.EqualTo("\b"));
			Assert.That(target.Eval("\"\\f\""), Is.EqualTo("\f"));
			Assert.That(target.Eval("\"\\n\""), Is.EqualTo("\n"));
			Assert.That(target.Eval("\"\\r\""), Is.EqualTo("\r"));
			Assert.That(target.Eval("\"\\t\""), Is.EqualTo("\t"));
			Assert.That(target.Eval("\"\\v\""), Is.EqualTo("\v"));

			Assert.That(target.Eval("\"L\\'aquila\\r\\n\\tè\\tbella.\""), Is.EqualTo("L\'aquila\r\n\tè\tbella."));
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

			Assert.That(target.Parse("\"some string\"").ReturnType, Is.EqualTo(typeof(string)));
			Assert.That(target.Parse("\"\"").ReturnType, Is.EqualTo(typeof(string)));
			Assert.That(target.Parse("234").ReturnType, Is.EqualTo(typeof(int)));
			Assert.That(target.Parse("-234").ReturnType, Is.EqualTo(typeof(int)));
			Assert.That(target.Parse("123u").ReturnType, Is.EqualTo(typeof(uint)));
			Assert.That(target.Parse("123U").ReturnType, Is.EqualTo(typeof(uint)));
			Assert.That(target.Parse("-123l").ReturnType, Is.EqualTo(typeof(long)));
			Assert.That(target.Parse("123l").ReturnType, Is.EqualTo(typeof(long)));
			Assert.That(target.Parse("123L").ReturnType, Is.EqualTo(typeof(long)));
			Assert.That(target.Parse("123UL").ReturnType, Is.EqualTo(typeof(ulong)));
			Assert.That(target.Parse("123Ul").ReturnType, Is.EqualTo(typeof(ulong)));
			Assert.That(target.Parse("123uL").ReturnType, Is.EqualTo(typeof(ulong)));
			Assert.That(target.Parse("123ul").ReturnType, Is.EqualTo(typeof(ulong)));
			Assert.That(target.Parse("123LU").ReturnType, Is.EqualTo(typeof(ulong)));
			Assert.That(target.Parse("123Lu").ReturnType, Is.EqualTo(typeof(ulong)));
			Assert.That(target.Parse("123lU").ReturnType, Is.EqualTo(typeof(ulong)));
			Assert.That(target.Parse("123lu").ReturnType, Is.EqualTo(typeof(ulong)));
			Assert.That(target.Parse("234.54").ReturnType, Is.EqualTo(typeof(double)));
			Assert.That(target.Parse(".9").ReturnType, Is.EqualTo(typeof(double)));
			Assert.That(target.Parse("-.9").ReturnType, Is.EqualTo(typeof(double)));
			Assert.That(target.Parse("234d").ReturnType, Is.EqualTo(typeof(double)));
			Assert.That(target.Parse("234D").ReturnType, Is.EqualTo(typeof(double)));
			Assert.That(target.Parse("4.5f").ReturnType, Is.EqualTo(typeof(float)));
			Assert.That(target.Parse("4.5F").ReturnType, Is.EqualTo(typeof(float)));
			Assert.That(target.Parse(".5f").ReturnType, Is.EqualTo(typeof(float)));
			Assert.That(target.Parse(".5F").ReturnType, Is.EqualTo(typeof(float)));
			Assert.That(target.Parse("234.48m").ReturnType, Is.EqualTo(typeof(decimal)));
			Assert.That(target.Parse("234.48M").ReturnType, Is.EqualTo(typeof(decimal)));
			Assert.That(target.Parse(".48m").ReturnType, Is.EqualTo(typeof(decimal)));
			Assert.That(target.Parse(".48M").ReturnType, Is.EqualTo(typeof(decimal)));
			Assert.That(target.Parse("null").ReturnType, Is.EqualTo(typeof(object)));

			Assert.That(target.Eval("45.5").GetType(), Is.EqualTo((45.5).GetType()));
			Assert.That(target.Eval("45.8f").GetType(), Is.EqualTo((45.8f).GetType()));
			Assert.That(target.Eval("45.232M").GetType(), Is.EqualTo((45.232M).GetType()));
			Assert.That(target.Eval("2e+201").GetType(), Is.EqualTo((2e+201).GetType()));
		}

		[Test]
		public void Thread_Culture_WithUS_Culture_is_ignored_for_literals()
		{
			var originalCulture = Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
			var target = new Interpreter();
			Assert.That(target.Eval("45.5"), Is.EqualTo(45.5));
			Thread.CurrentThread.CurrentCulture = originalCulture;
		}

		[Test]
		public void Thread_Culture_WithIT_Culture_is_ignored_for_literals()
		{
			var originalCulture = Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("it-IT");
			var target = new Interpreter();
			Assert.That(target.Eval("45.5"), Is.EqualTo(45.5));
			Thread.CurrentThread.CurrentCulture = originalCulture;
		}

	}
}
