using System;
using NUnit.Framework;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;

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

            Assert.AreEqual((-0.5).ToString(), interpreter.Eval("-.5.ToString()"));
            Assert.AreEqual((0.1).ToString(), interpreter.Eval(".1.ToString()"));
            Assert.AreEqual((-1 - .1 - 0.1).ToString(), interpreter.Eval("(-1-.1-0.1).ToString()"));
        }
    }
}
