using System;
using NUnit.Framework;
using System.Collections.Generic;
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
			Assert.AreEqual((-1-.1-0.1).ToString(), interpreter.Eval("(-1-.1-0.1).ToString()"));
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

			var x = new { var1 = "hallo", var2 = (String) null };
			interpreter.SetVariable("x", x);
			Assert.AreEqual("hallo", interpreter.Eval("x.var1?.ToString()"));
			Assert.AreEqual(null, interpreter.Eval("x.var2?.ToString()"));
		}
	}
}
