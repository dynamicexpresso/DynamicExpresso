using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace DynamicExpresso.UnitTest
{
	[TestClass]
	public class GithubIssues
	{
		[TestMethod]
		public void GitHub_Issue_19()
		{
			var interpreter = new Interpreter();

			Assert.AreEqual(5.0.ToString(), interpreter.Eval("5.0.ToString()"));
			Assert.AreEqual((5).ToString(), interpreter.Eval("(5).ToString()"));
			Assert.AreEqual((5.0).ToString(), interpreter.Eval("(5.0).ToString()"));
			Assert.AreEqual(5.ToString(), interpreter.Eval("5.ToString()"));
		}
	}
}
