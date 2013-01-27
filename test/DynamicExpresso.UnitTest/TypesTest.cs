using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;
using System.Collections.Generic;

namespace DynamicExpresso.UnitTest
{
    [TestClass]
    public class TypesTest
    {
        [TestMethod]
        public void Default_Types()
        {
            var target = new Interpreter();

            Dictionary<string, Type> predefinedTypes = new Dictionary<string, Type>{
                    {"Object", typeof(Object)},
                    {"object", typeof(Object)},
                    {"Boolean", typeof(Boolean)},
                    {"bool", typeof(Boolean)},
                    {"Char", typeof(Char)},
                    {"char", typeof(Char)},
                    {"String", typeof(String)},
                    {"string", typeof(String)},
                    {"SByte", typeof(SByte)},
                    {"Byte", typeof(Byte)},
                    {"byte", typeof(Byte)},
                    {"Int16", typeof(Int16)},
                    {"UInt16", typeof(UInt16)},
                    {"Int32", typeof(Int32)},
                    {"int", typeof(Int32)},
                    {"UInt32", typeof(UInt32)},
                    {"Int64", typeof(Int64)},
                    {"long", typeof(Int64)},
                    {"UInt64", typeof(UInt64)},
                    {"Single", typeof(Single)},
                    {"Double", typeof(Double)},
                    {"double", typeof(Double)},
                    {"Decimal", typeof(Decimal)},
                    {"decimal", typeof(Decimal)},
                    {"DateTime", typeof(DateTime)},
                    {"TimeSpan", typeof(TimeSpan)},
                    {"Guid", typeof(Guid)},
                    {"Math", typeof(Math)},
                    {"Convert", typeof(Convert)},
                };

            foreach (var t in predefinedTypes)
                Assert.AreEqual(t.Value, target.Eval(string.Format("typeof({0})", t.Key)));
        }

        [TestMethod]
        public void Custom_Types()
        {
            var target = new Interpreter().Using(typeof(Uri));

            Assert.AreEqual(typeof(Uri), target.Eval("typeof(Uri)"));
        }

        [TestMethod]
        public void Custom_Type_Constructor()
        {
            var target = new Interpreter()
                            .Using(typeof(MyDataContract));

            Assert.AreEqual(new MyDataContract("davide").Name, target.Eval("new MyDataContract(\"davide\").Name"));
            Assert.AreEqual(new MyDataContract(44, 88).Name, target.Eval("new MyDataContract(44 , 88).Name"));
        }

        [Ignore]
        [TestMethod]
        public void Generic_Types()
        {
            var target = new Interpreter()
                            .Using(typeof(MyTestClass<>));

            Assert.AreEqual(new MyTestClass<string>("davide").ToString(), target.Eval("new MyTestClass<string>(\"davide\").ToString()"));
            Assert.AreEqual(new MyTestClass<int>(44).ToString(), target.Eval("new MyTestClass<int>(44).ToString()"));
        }

        [TestMethod]
        public void Custom_Type_Alias()
        {
            var target = new Interpreter()
                            .Using(typeof(MyDataContract), "DC");

            Assert.AreEqual(typeof(MyDataContract), target.Parse("new DC(\"davide\")").ReturnType);
        }

        [TestMethod]
        public void Custom_Enum()
        {
            var target = new Interpreter()
                            .Using(typeof(CalendarAlgorithmType));

            Assert.AreEqual(CalendarAlgorithmType.LunisolarCalendar, target.Eval("CalendarAlgorithmType.LunisolarCalendar"));
            Assert.AreEqual(CalendarAlgorithmType.SolarCalendar, target.Eval("CalendarAlgorithmType.SolarCalendar"));
        }

        class MyDataContract
        {
            public MyDataContract(string name)
            {
                Name = name;
            }

            public MyDataContract(int x, int y)
            {
                Name = string.Format("{0} - {1}", x, y);
            }

            public string Name { get; set; }
        }

        public class MyTestClass<T>
        {
            T _val;
            public MyTestClass(T val)
            {
                _val = val;
            }

            public override string ToString()
            {
                if (_val == null)
                    return "null";

                return _val.ToString();
            }
        }
    }
}
