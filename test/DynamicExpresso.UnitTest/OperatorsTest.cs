using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DynamicExpresso.UnitTest
{
    [TestClass]
    public class OperatorsTest
    {
        [TestMethod]
        public void Numeric_Operators()
        {
            var target = new Interpreter();

            Assert.AreEqual(45 + 5, target.Eval("45 + 5"));
            Assert.AreEqual(45 - 5, target.Eval("45 - 5"));
            Assert.AreEqual(1.0 - 0.5, target.Eval("1.0 - 0.5"));
            Assert.AreEqual(2 * 4, target.Eval("2 * 4"));
            Assert.AreEqual(8 / 2, target.Eval("8 / 2"));
            Assert.AreEqual(7 % 3, target.Eval("7 % 3"));
        }

        [TestMethod]
        public void Numeric_Operators_Priority()
        {
            var target = new Interpreter();

            Assert.AreEqual(8 / 2 + 2, target.Eval("8 / 2 + 2"));
            Assert.AreEqual(8 + 2 / 2, target.Eval("8 + 2 / 2"));
        }

        [TestMethod]
        public void Typeof_Operator()
        {
            var target = new Interpreter();

            Assert.AreEqual(typeof(string), target.Eval("typeof(string)"));
        }

        [TestMethod]
        public void Is_Operator()
        {
            object a = "string value";
            object b = 64;
            var target = new Interpreter()
                                .SetVariable("a", a, typeof(object))
                                .SetVariable("b", b, typeof(object));

            Assert.AreEqual(a is string, target.Eval("a is string"));
            Assert.AreEqual(typeof(bool), target.Parse("a is string").ReturnType);
            Assert.AreEqual(b is string, target.Eval("b is string"));
            Assert.AreEqual(b is int, target.Eval("b is int"));
        }

        [TestMethod]
        public void As_Operator()
        {
            object a = "string value";
            object b = 64;
            var target = new Interpreter()
                                .SetVariable("a", a, typeof(object))
                                .SetVariable("b", b, typeof(object));

            Assert.AreEqual(a as string, target.Eval("a as string"));
            Assert.AreEqual(typeof(string), target.Parse("a as string").ReturnType);
            Assert.AreEqual(b as string, target.Eval("b as string"));
            Assert.AreEqual(typeof(string), target.Parse("b as string").ReturnType);
        }

        [TestMethod]
        public void Type_Operators()
        {
            var target = new Interpreter();

            Assert.AreEqual(typeof(string) != typeof(int), target.Eval("typeof(string) != typeof(int)"));
            Assert.AreEqual(typeof(string) == typeof(string), target.Eval("typeof(string) == typeof(string)"));
        }

        [TestMethod]
        public void String_Concatenation()
        {
            var target = new Interpreter();

            Assert.AreEqual("ciao " + "mondo", target.Eval("\"ciao \" + \"mondo\""));
        }

        [TestMethod]
        public void Numeric_Expression()
        {
            var target = new Interpreter();

            Assert.AreEqual(8 / (2 + 2), target.Eval("8 / (2 + 2)"));
            Assert.AreEqual(58 / (2 * (8 + 2)), target.Eval(" 58 / (2 * (8 + 2))"));
        }

        [TestMethod]
        public void Comparison_Operators()
        {
            var target = new Interpreter();

            Assert.IsFalse((bool)target.Eval("0 > 3"));
            Assert.IsFalse((bool)target.Eval("0 >= 3"));
            Assert.IsTrue((bool)target.Eval("3 < 5"));
            Assert.IsTrue((bool)target.Eval("3 <= 5"));
            Assert.IsFalse((bool)target.Eval("5 == 3"));
            Assert.IsTrue((bool)target.Eval("5 == 5"));
            Assert.IsTrue((bool)target.Eval("5 >= 5"));
            Assert.IsTrue((bool)target.Eval("5 <= 5"));
            Assert.IsTrue((bool)target.Eval("5 != 3"));
            Assert.IsTrue((bool)target.Eval("\"dav\" == \"dav\""));
            Assert.IsFalse((bool)target.Eval("\"dav\" == \"jack\""));
        }

        [TestMethod]
        public void Logical_Operators()
        {
            var target = new Interpreter();

            Assert.IsTrue((bool)target.Eval("0 > 3 || true"));
            Assert.IsFalse((bool)target.Eval("0 > 3 && 4 < 6"));
        }

        [TestMethod]
        public void If_Operators()
        {
            var target = new Interpreter();

            Assert.AreEqual(10 > 3 ? "yes" : "no", target.Eval("10 > 3 ? \"yes\" : \"no\""));
            Assert.AreEqual(10 < 3 ? "yes" : "no", target.Eval("10 < 3 ? \"yes\" : \"no\""));
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void Operator_Equal_Is_Not_Supported()
        {
            var target = new Interpreter();

            target.Parse("5 = 4");
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void Operator_LessGreater_Is_Not_Supported()
        {
            var target = new Interpreter();

            target.Parse("5 <> 4");
        }
    }
}
