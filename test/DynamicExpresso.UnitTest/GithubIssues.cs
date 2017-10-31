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

        /**
         * Once the bug is fixed, this test should fail.
         **/
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void GitHub_Issue_39_Simple_Reassignment()
        {
            var target = new Interpreter().SetVariable("x", 3);
            Assert.AreEqual(3, target.Eval("x"));
            target.Eval("x = 23");
            Assert.AreEqual(23, target.Eval("x"));
        }

        /**
         * Once the bug is fixed, this test should fail.
         **/
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void GitHub_Issue_39_Array_Reassignment()
        {
            var arr = new[] { 1 };
            arr[0] = 2;
            var target = new Interpreter().SetVariable("arr", arr);
            Assert.AreEqual(2, target.Eval("arr[0]"));
            target.Eval("arr[0] = 3");
            Assert.AreNotEqual(2, target.Eval("arr[0]"));
            Assert.AreEqual(3, target.Eval("arr[0]"));
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void GitHub_Issue_39_List_Reassignment()
        {
            var list = new List<int>(new[] { 1 });
            list[0] = 2;
            var target = new Interpreter().SetVariable("list", list);
            Assert.AreEqual(2, target.Eval("list[0]"));
            target.Eval("list[0] = 3");
            Assert.AreNotEqual(2, target.Eval("list[0]"));
            Assert.AreEqual(3, target.Eval("list[0]"));
        }
    }
}
