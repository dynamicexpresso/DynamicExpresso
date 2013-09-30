using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace DynamicExpresso.UnitTest
{
	[TestClass]
    public class InvalidExpressionTest
	{
		[TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void Not_existing_variable()
		{
			var target = new Interpreter();

			target.Eval("not_existing");
		}

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void Not_existing_function()
        {
            var target = new Interpreter();

            target.Eval("pippo()");
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void Not_valid_function()
        {
            var target = new Interpreter();

            target.Eval("2()");
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void Not_valid_expression()
        {
            var target = new Interpreter();

            target.Eval("'5' + 3 /(asda");
        }
	}
}
