using DynamicExpresso.Exceptions;
using NUnit.Framework;
using System.Linq;

// ReSharper disable SpecifyACultureInStringConversionExplicitly

namespace DynamicExpresso.UnitTest
{
	[TestFixture]
	public class GithubIssues
	{
		[Test]
		public void GitHub_Issue_19()
		{
			var interpreter = new Interpreter();

			Assert.AreEqual(5.0.ToString(), interpreter.Eval("5.0.ToString()"));
			Assert.AreEqual((5).ToString(), interpreter.Eval("(5).ToString()"));
			Assert.AreEqual((5.0).ToString(), interpreter.Eval("(5.0).ToString()"));
			Assert.AreEqual(5.ToString(), interpreter.Eval("5.ToString()"));
		}

		[Test]
		public void GitHub_Issue_43()
		{
			var interpreter = new Interpreter();

			Assert.AreEqual((-.5).ToString(), interpreter.Eval("-.5.ToString()"));
			Assert.AreEqual((.1).ToString(), interpreter.Eval(".1.ToString()"));
			Assert.AreEqual((-1 - .1 - 0.1).ToString(), interpreter.Eval("(-1-.1-0.1).ToString()"));
		}

		[Test]
		public void GitHub_Issue_68()
		{
			var interpreter = new Interpreter();

			var array = new[] { 5, 10, 6 };

			interpreter.SetVariable("array", array);

			Assert.AreEqual(array.Contains(5), interpreter.Eval("array.Contains(5)"));
			Assert.AreEqual(array.Contains(3), interpreter.Eval("array.Contains(3)"));
		}

		[Test]
		public void GitHub_Issue_64()
		{
			var interpreter = new Interpreter();
			Assert.AreEqual(null, interpreter.Eval("null ?? null"));
			Assert.AreEqual("hallo", interpreter.Eval("\"hallo\" ?? null"));
			Assert.AreEqual("hallo", interpreter.Eval("null ?? \"hallo\""));
		}

		[Test]
		public void GitHub_Issue_65_Part1()
		{
			var interpreter = new Interpreter();

			var x = new
			{
				var1 = "hallo",
				var2 = (string)null
			};

			interpreter.SetVariable("x", x);
			Assert.AreEqual("hallo", interpreter.Eval("x.var1?.ToString()"));
			Assert.AreEqual(null, interpreter.Eval("x.var2?.ToString()"));
			Assert.AreEqual("allo", interpreter.Eval("x.var1?.Substring(1)"));
		}

		[Test]
		public void GitHub_Issue_88()
		{
			var interpreter = new Interpreter();

			interpreter.SetVariable("a", 1, typeof(int));
			interpreter.SetVariable("b", 1.2, typeof(double?));
			var result = interpreter.Eval("a + b");

			Assert.AreEqual(result, 2.2);
		}

		[Test]
		public void GitHub_Issue_128()
		{
			var target = new Interpreter();

			Assert.Throws<ParseException>(() => target.Eval("1+1*"));
			Assert.Throws<ParseException>(() => target.Eval("1+1*'a'"));
		}

		[Test]
		public void GitHub_Issue_133()
		{
			var interpreter = new Interpreter();

			Assert.AreEqual(10000000001, interpreter.Eval("1+1e10"));
			Assert.AreEqual(10000000001, interpreter.Eval("1+1e+10"));
			Assert.AreEqual(1.0000000001, interpreter.Eval("1+1e-10"));
			Assert.AreEqual(-20199999999, interpreter.Eval("1 - 2.02e10"));
			Assert.AreEqual(-20199999999, interpreter.Eval("1 - 2.02e+10"));
			Assert.AreEqual(0.999999999798, interpreter.Eval("1 - 2.02e-10"));
			Assert.AreEqual(1e-10, interpreter.Eval("1/1e+10"));

			interpreter.SetVariable("@Var1", 1);
			interpreter.SetVariable("@Var2", 1e+10);
			Assert.AreEqual(10000000001, interpreter.Eval("@Var1+@Var2"));

			interpreter.SetVariable("e", 2);
			Assert.AreEqual(10000000003, interpreter.Eval("@Var1+@Var2+e"));
		}
	}
}
