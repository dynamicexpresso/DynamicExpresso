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
        public void Literals()
        {
            var target = new Interpreter();

            Assert.AreEqual("ciao", target.Eval("\"ciao\""));
            Assert.AreEqual('c', target.Eval("'c'"));
            Assert.IsNull(target.Eval("null"));
            Assert.IsTrue((bool)target.Eval("true"));
            Assert.IsFalse((bool)target.Eval("false"));

            Assert.AreEqual(45, target.Eval("45"));
            Assert.AreEqual(23423423423434, target.Eval("23423423423434"));
            Assert.AreEqual(45.5, target.Eval("45.5"));
            Assert.AreEqual((45.5).GetType(), target.Eval("45.5").GetType());
            Assert.AreEqual(45.8f, target.Eval("45.8f"));
            Assert.AreEqual((45.8f).GetType(), target.Eval("45.8f").GetType());
            Assert.AreEqual(45.232M, target.Eval("45.232M"));
            Assert.AreEqual((45.232M).GetType(), target.Eval("45.232M").GetType());
        }

        [TestMethod]
        public void Should_Understand_ReturnType_Of_Literlars()
        {
            var target = new Interpreter();

            Assert.AreEqual(typeof(string), target.Parse("\"some string\"").ReturnType);
            Assert.AreEqual(typeof(int), target.Parse("234").ReturnType);
            Assert.AreEqual(typeof(object), target.Parse("null").ReturnType);
        }

        [TestMethod]
        public void Literals_WithUS_Culture()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            var target = new Interpreter();
            Assert.AreEqual(45.5, target.Eval("45.5"));
        }

        [TestMethod]
        public void Literals_WithIT_Culture()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("it-IT");
            var target = new Interpreter();
            Assert.AreEqual(45.5, target.Eval("45.5"));
        }

    }
}
