using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq.Expressions;

namespace DynamicExpresso.UnitTest
{
    [TestClass]
    public class VariablesTest
    {
        [TestMethod]
        public void Variables()
        {
            var target = new Interpreter()
                            .SetVariable("myk", 23);

            Assert.AreEqual(23, target.Eval("myk"));
            Assert.AreEqual(typeof(int), target.Parse("myk").ReturnType);
        }

        [TestMethod]
        public void Null_Variables()
        {
            var target = new Interpreter()
                            .SetVariable("myk", null);

            Assert.AreEqual(null, target.Eval("myk"));
            Assert.AreEqual(true, target.Eval("myk == null"));
            Assert.AreEqual(typeof(object), target.Parse("myk").ReturnType);
        }

        [TestMethod]
        public void Null_Variables_With_Type_Specified()
        {
            var target = new Interpreter()
                            .SetVariable("myk", null, typeof(string));

            Assert.AreEqual(null, target.Eval("myk"));
            Assert.AreEqual(true, target.Eval("myk == null"));
            Assert.AreEqual(typeof(string), target.Parse("myk").ReturnType);
        }

        [TestMethod]
        public void Keywords_with_lambda()
        {
            Expression<Func<double, double, double>> pow = (x, y) => Math.Pow(x, y);
            var target = new Interpreter()
                        .SetExpression("pow", pow);

            Assert.AreEqual(9.0, target.Eval("pow(3, 2)"));
        }

        [TestMethod]
        public void Keywords_with_delegate()
        {
            Func<double, double, double> pow = (x, y) => Math.Pow(x, y);
            var target = new Interpreter()
                        .SetVariable("pow", pow);

            Assert.AreEqual(9.0, target.Eval("pow(3, 2)"));
        }

    }
}
