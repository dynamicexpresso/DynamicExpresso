using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;
using System.Threading;
using System.Linq.Expressions;

namespace DynamicExpresso.UnitTest
{
    [TestClass]
    public class ExpressionEngine_Test
    {
        [TestMethod]
        public void Eval_Literals()
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
        public void Parse_Should_Understand_ReturnType_Of_Literlars()
        {
            var target = new Interpreter();

            Assert.AreEqual(typeof(string), target.Parse("\"some string\"").ReturnType);
            Assert.AreEqual(typeof(int), target.Parse("234").ReturnType);
            Assert.AreEqual(typeof(object), target.Parse("null").ReturnType);
        }

        [TestMethod]
        public void Eval_New_Of_Base_Type()
        {
            var target = new Interpreter();

            Assert.AreEqual(new DateTime(2015, 1, 24), target.Eval("new DateTime(2015, 1, 24)"));
            Assert.AreEqual(new string('a', 10), target.Eval("new string('a', 10)"));
        }

        [TestMethod]
        public void Eval_New_Of_Custom_Type()
        {
            var target = new Interpreter();

            target.Using(typeof(Uri));

            Assert.AreEqual(new Uri("http://www.google.com"), target.Eval("new Uri(\"http://www.google.com\")"));
        }

        [TestMethod]
        public void Eval_New_And_Member_Access()
        {
            var target = new Interpreter();

            Assert.AreEqual(new DateTime(2015, 1, 24).Month, target.Eval("new DateTime(2015,   1, 24).Month"));
            Assert.AreEqual(new DateTime(2015, 1, 24).Month + 34, target.Eval("new DateTime( 2015, 1, 24).Month + 34"));
        }

        [TestMethod]
        public void Eval_Literals_WithUS_Culture()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            var target = new Interpreter();
            Assert.AreEqual(45.5, target.Eval("45.5"));
        }

        [TestMethod]
        public void Eval_Literals_WithIT_Culture()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("it-IT");
            var target = new Interpreter();
            Assert.AreEqual(45.5, target.Eval("45.5"));
        }

        [TestMethod]
        public void Eval_Numeric_Operators()
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
        public void Eval_Numeric_Operators_Priority()
        {
            var target = new Interpreter();

            Assert.AreEqual(8 / 2 + 2, target.Eval("8 / 2 + 2"));
            Assert.AreEqual(8 + 2 / 2, target.Eval("8 + 2 / 2"));
        }

        [TestMethod]
        public void Eval_String_Concatenation()
        {
            var target = new Interpreter();

            Assert.AreEqual("ciao " + "mondo", target.Eval("\"ciao \" + \"mondo\""));
        }

        [TestMethod]
        public void Eval_Numeric_Expression()
        {
            var target = new Interpreter();

            Assert.AreEqual(8 / (2 + 2), target.Eval("8 / (2 + 2)"));
            Assert.AreEqual(58 / (2 * (8 + 2)), target.Eval(" 58 / (2 * (8 + 2))"));
        }

        [TestMethod]
        public void Eval_Comparison_Operators()
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
        public void Eval_Logical_Operators()
        {
            var target = new Interpreter();

            Assert.IsTrue((bool)target.Eval("0 > 3 || true"));
            Assert.IsFalse((bool)target.Eval("0 > 3 && 4 < 6"));
        }

        [TestMethod]
        public void Eval_Static_Properties_of_Base_Types()
        {
            var target = new Interpreter();

            Assert.AreEqual(Int32.MaxValue, target.Eval("Int32.MaxValue"));
            Assert.AreEqual(Double.MaxValue, target.Eval("Double.MaxValue"));
            Assert.AreEqual(DateTime.MaxValue, target.Eval("DateTime.MaxValue"));
            Assert.AreEqual(DateTime.Today, target.Eval("DateTime.Today"));
            Assert.AreEqual(String.Empty, target.Eval("String.Empty"));
            Assert.AreEqual(Boolean.FalseString, target.Eval("Boolean.FalseString"));
            Assert.AreEqual(TimeSpan.TicksPerMillisecond, target.Eval("TimeSpan.TicksPerMillisecond"));
            Assert.AreEqual(Guid.Empty, target.Eval("Guid.Empty"));
        }

        [TestMethod]
        public void Eval_Static_Methods_of_Base_Types()
        {
            var target = new Interpreter();

            Assert.AreEqual(TimeSpan.FromMilliseconds(2000.49), target.Eval("TimeSpan.FromMilliseconds(2000.49)"));
            Assert.AreEqual(DateTime.DaysInMonth(2094, 11), target.Eval("DateTime.DaysInMonth(2094, 11)"));
        }

        [TestMethod]
        public void Eval_Math_Class_Operators()
        {
            var target = new Interpreter();

            Assert.AreEqual(Math.Pow(3, 4), target.Eval("Math.Pow(3, 4)"));
            Assert.AreEqual(Math.Sin(30.234), target.Eval("Math.Sin(30.234)"));
        }

        [TestMethod]
        public void Eval_CSharp_Primitive_Type_Keywords()
        {
            var target = new Interpreter();

            Assert.AreEqual(int.MaxValue, target.Eval("int.MaxValue"));
            Assert.AreEqual(double.MaxValue, target.Eval("double.MaxValue"));
            Assert.AreEqual(string.Empty, target.Eval("string.Empty"));
            Assert.AreEqual(bool.FalseString, target.Eval("bool.FalseString"));
            Assert.AreEqual(char.MinValue, target.Eval("char.MinValue"));
            Assert.AreEqual(byte.MinValue, target.Eval("byte.MinValue"));
        }

        [TestMethod]
        public void Eval_string_format()
        {
            var target = new Interpreter();

            Assert.AreEqual(string.Format("ciao {0}, today is {1}", "mondo", DateTime.Today),
                            target.Eval("string.Format(\"ciao {0}, today is {1}\", \"mondo\", DateTime.Today.ToString())"));
        }

        [Ignore]
        [TestMethod]
        public void Eval_string_format_With_Object_Params()
        {
            var target = new Interpreter();

            Assert.AreEqual(string.Format("ciao mondo, today is {0}", DateTime.Today),
                            target.Eval("string.Format(\"ciao {0}, today is {1}\", \"mondo\", DateTime.Today))"));
        }

        [TestMethod]
        public void Eval_variables()
        {
            var target = new Interpreter()
                            .SetVariable("myk", 23);

            Assert.AreEqual(23, target.Eval("myk"));
        }

        [TestMethod]
        public void Eval_custom_keywords_with_lambda()
        {
            var target = new Interpreter()
                        .SetFunction<Func<double, double, double>>("pow", (x, y) => Math.Pow(x, y));

            Assert.AreEqual(9.0, target.Eval("pow(3, 2)"));
        }

        [TestMethod]
        public void Eval_primitive_parameters()
        {
            var target = new Interpreter();

            double x = 2;
            string y = "param y";
            var parameters = new[] {
                            new FunctionParameter("x", x.GetType(), x),
                            new FunctionParameter("y", y.GetType(), y)
                            };

            Assert.AreEqual(x, target.Eval("x", parameters));
            Assert.AreEqual(x + x + x, target.Eval("x+x+x", parameters));
            Assert.AreEqual(x * x, target.Eval("x * x", parameters));
            Assert.AreEqual(y, target.Eval("y", parameters));
            Assert.AreEqual(y.Length + x, target.Eval("y.Length + x", parameters));
        }

        [TestMethod]
        public void Eval_complex_parameters()
        {
            var target = new Interpreter();

            var x = new MyTestService();
            var y = "davide";
            var z = 5;
            var w = DateTime.Today;
            var parameters = new[] {
                            new FunctionParameter("x", x.GetType(), x),
                            new FunctionParameter("y", y.GetType(), y),
                            new FunctionParameter("z", z.GetType(), z),
                            new FunctionParameter("w", w.GetType(), w)
                            };

            Assert.AreEqual(x, target.Eval("x", parameters));
            Assert.AreEqual(x.SomeNumber + 1, target.Eval("x.SomeNumber + 1", parameters));
            Assert.AreEqual(x.HelloWorld(), target.Eval("x.HelloWorld()", parameters));
            Assert.AreEqual(x.CallMethod(y, z, w), target.Eval("x.CallMethod( y, z,w)", parameters));
        }

        [Ignore]
        [TestMethod]
        public void Eval_parameters_with_delegate()
        {
            var target = new Interpreter();

            Func<double, double, double> pow = (x, y) => Math.Pow(x, y);

            var parameters = new[] {
                            new FunctionParameter("pow", pow.GetType(), pow)
                            };

            Assert.AreEqual(9.0, target.Eval("pow(3, 2)", parameters));
        }

        [TestMethod]
        public void Eval_If_Operators()
        {
            var target = new Interpreter();

            Assert.AreEqual(10 > 3 ? "yes" : "no", target.Eval("10 > 3 ? \"yes\" : \"no\""));
            Assert.AreEqual(10 < 3 ? "yes" : "no", target.Eval("10 < 3 ? \"yes\" : \"no\""));
        }

        [TestMethod]
        public void Eval_complex_expression()
        {
            var target = new Interpreter();

            var x = new MyTestService();
            var y = 5;
            var parameters = new[] {
                            new FunctionParameter("x", x.GetType(), x),
                            new FunctionParameter("y", y.GetType(), y),
                            };

            Assert.AreEqual(x.SomeNumber > y && x.HelloWorld().Length == 10, target.Eval("x.SomeNumber > y && x.HelloWorld().Length == 10", parameters));
            Assert.AreEqual(x.SomeNumber * (4 + 65) / x.SomeNumber, target.Eval("x.SomeNumber * (4 + 65) / x.SomeNumber", parameters));
        }

        [TestMethod]
        public void Parse_Should_Understand_ReturnType_Of_expressions()
        {
            var target = new Interpreter();

            var x = new MyTestService();
            var y = 5;
            var parameters = new[] {
                            new FunctionParameter("x", x.GetType(), x),
                            new FunctionParameter("y", y.GetType(), y),
                            };

            Assert.AreEqual(typeof(bool), target.Parse("x.SomeNumber > y && x.HelloWorld().Length == 10", parameters).ReturnType);
            Assert.AreEqual(typeof(int), target.Parse("x.SomeNumber * (4 + 65) / x.SomeNumber", parameters).ReturnType);
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

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void Unknown_Keyword_Is_Not_Supported()
        {
            var target = new Interpreter();

            target.Parse("unkkeyword");
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void Unknown_New_Type_Is_Not_Supported()
        {
            var target = new Interpreter();

            target.Parse("new unkkeyword()");
        }

        [TestMethod]
        public void Eval_Static_Properties_And_Methods_Of_Custom_Types()
        {
            var target = new Interpreter()
                            .Using(typeof(Uri))
                            .Using(typeof(MyTestService));

            Assert.AreEqual(Uri.UriSchemeHttp, target.Eval("Uri.UriSchemeHttp"));
            Assert.AreEqual(MyTestService.MyStaticMethod(), target.Eval("MyTestService.MyStaticMethod()"));
        }

        [TestMethod]
        public void It_should_be_possible_to_execute_the_same_function_multiple_times()
        {
            var target = new Interpreter();

            var functionX = target.Parse("Math.Pow(x, y) + 5",
                                new FunctionParameter("x", typeof(double)),
                                new FunctionParameter("y", typeof(double)));

            Assert.AreEqual(Math.Pow(15, 12) + 5, functionX.Invoke(15, 12));
            Assert.AreEqual(Math.Pow(5, 1) + 5, functionX.Invoke(5, 1));
            Assert.AreEqual(Math.Pow(11, 8) + 5, functionX.Invoke(11, 8));
            Assert.AreEqual(Math.Pow(3, 4) + 5, functionX.Invoke(new FunctionParameter("x", 3.0),
                                                                new FunctionParameter("y", 4.0)));
            Assert.AreEqual(Math.Pow(9, 2) + 5, functionX.Invoke(new FunctionParameter("x", 9.0),
                                                                new FunctionParameter("y", 2.0)));
            Assert.AreEqual(Math.Pow(1, 3) + 5, functionX.Invoke(new FunctionParameter("x", 1.0),
                                                                new FunctionParameter("y", 3.0)));
        }

        // Missing tests
        // --------------
        // - Enum
        // - Type class
        // - Nullable types
        // - exception during parse and eval
        // - is operator
        // - typeof operator
        // - performance test (memory/cpu/threads/handles)
        // - difference between externals and keywords of the ParserSettings?
    }

    public class MyTestService
    {
        public int SomeNumber
        {
            get { return 769; }
        }

        public string HelloWorld()
        {
            return "Ciao mondo";
        }

        public string CallMethod(string param1, int param2, DateTime param3)
        {
            return string.Format("{0} {1} {2}", param1, param2, param3);
        }

        public static int MyStaticMethod()
        {
            return 23;
        }
    }
}
