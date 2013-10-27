using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace DynamicExpresso.UnitTest
{
	[TestClass]
	public class OtherTests
	{
		[TestMethod]
		public void Space_Characters_Are_Ignored()
		{
			var target = new Interpreter();

			Assert.AreEqual(46, target.Eval("     45\t\t  + 1 \r  \n"));
		}

		[TestMethod]
		public void Empty_Null_Withespace_Expression()
		{
			var target = new Interpreter();

			Assert.AreEqual(null, target.Eval(""));
			Assert.AreEqual(typeof(void), target.Parse("").ReturnType);

			Assert.AreEqual(null, target.Eval(null));
			Assert.AreEqual(typeof(void), target.Parse(null).ReturnType);

			Assert.AreEqual(null, target.Eval("  \t\t\r\n  \t   "));
			Assert.AreEqual(typeof(void), target.Parse("  \t\t\r\n  \t   ").ReturnType);
		}

		[TestMethod]
		public void Complex_expression()
		{
			var target = new Interpreter();

			var x = new MyTestService();
			var y = 5;
			var parameters = new[] {
                            new Parameter("x", x.GetType(), x),
                            new Parameter("y", y.GetType(), y),
                            };

			Assert.AreEqual(x.AProperty > y && x.HelloWorld().Length == 10, target.Eval("x.AProperty      >\t y && \r\n x.HelloWorld().Length == 10", parameters));
			Assert.AreEqual(x.AProperty * (4 + 65) / x.AProperty, target.Eval("x.AProperty * (4 + 65) / x.AProperty", parameters));

			Assert.AreEqual(Convert.ToString(x.AProperty * (4 + 65) / x.AProperty), target.Eval("Convert.ToString(x.AProperty * (4 + 65) / x.AProperty)", parameters));
		}

		[TestMethod]
		public void Parse_An_Expression_And_Invoke_It_With_Different_Parameters()
		{
			var service = new MyTestService();

			var target = new Interpreter()
													.SetVariable("service", service);

			var func = target.Parse("x > 4 ? service.VoidMethod() : service.VoidMethod2()",
															new Parameter("x", typeof(int)));

			Assert.AreEqual(typeof(void), func.ReturnType);

			Assert.AreEqual(0, service.VoidMethodCalled);
			Assert.AreEqual(0, service.VoidMethod2Called);

			func.Invoke(new Parameter("x", 5));
			Assert.AreEqual(1, service.VoidMethodCalled);
			Assert.AreEqual(0, service.VoidMethod2Called);

			func.Invoke(new Parameter("x", 2));
			Assert.AreEqual(1, service.VoidMethodCalled);
			Assert.AreEqual(1, service.VoidMethod2Called);
		}

		[TestMethod]
		public void Should_Understand_ReturnType_Of_expressions()
		{
			var target = new Interpreter();

			var x = new MyTestService();
			var y = 5;
			var parameters = new[] {
                            new Parameter("x", x.GetType(), x),
                            new Parameter("y", y.GetType(), y),
                            };

			Assert.AreEqual(typeof(bool), target.Parse("x.AProperty > y && x.HelloWorld().Length == 10", parameters).ReturnType);
			Assert.AreEqual(typeof(int), target.Parse("x.AProperty * (4 + 65) / x.AProperty", parameters).ReturnType);
		}

		[TestMethod]
		[ExpectedException(typeof(ParseException))]
		public void Unknown_Keyword_Is_Not_Supported()
		{
			var target = new Interpreter();

			target.Parse("unkkeyword");
		}

		[ExpectedException(typeof(InvalidOperationException))]
		[TestMethod]
		public void SystemExceptions_are_preserved_using_delegate_variable()
		{
			var target = new Interpreter();

			Func<string> testException = new Func<string>(() =>
			{
				throw new InvalidOperationException("Test");
			});

			target.SetVariable("testException", testException);

			target.Eval("testException()");
		}

		[ExpectedException(typeof(MyException))]
		[TestMethod]
		public void CustomExceptions_WithoutSerializationConstructor_are_preserved()
		{
			var target = new Interpreter();

			Func<string> testException = new Func<string>(() =>
			{
				throw new MyException("Test");
			});

			target.SetVariable("testException", testException);

			target.Eval("testException()");
		}

		[ExpectedException(typeof(NotImplementedException))]
		[TestMethod]
		public void SystemExceptions_are_preserved_using_method_invocation()
		{
			var target = new Interpreter();
			target.SetVariable("a", new MyTestService());

			target.Eval("a.ThrowException()");
		}

		[TestMethod]
		public void Execute_the_same_function_multiple_times()
		{
			var target = new Interpreter();

			var functionX = target.Parse("Math.Pow(x, y) + 5",
													new Parameter("x", typeof(double)),
													new Parameter("y", typeof(double)));

			Assert.AreEqual(Math.Pow(15, 12) + 5, functionX.Invoke(15, 12));
			Assert.AreEqual(Math.Pow(5, 1) + 5, functionX.Invoke(5, 1));
			Assert.AreEqual(Math.Pow(11, 8) + 5, functionX.Invoke(11, 8));
			Assert.AreEqual(Math.Pow(3, 4) + 5, functionX.Invoke(new Parameter("x", 3.0),
																													new Parameter("y", 4.0)));
			Assert.AreEqual(Math.Pow(9, 2) + 5, functionX.Invoke(new Parameter("x", 9.0),
																													new Parameter("y", 2.0)));
			Assert.AreEqual(Math.Pow(1, 3) + 5, functionX.Invoke(new Parameter("x", 1.0),
																													new Parameter("y", 3.0)));
		}

		public class MyException : Exception
		{
			public MyException(string message) : base(message) { }
		}

		class MyTestService
		{
			public DateTime AField = DateTime.Now;
			public DateTime AFIELD = DateTime.UtcNow;

			public int AProperty
			{
				get { return 769; }
			}

			public int APROPERTY
			{
				get { return 887; }
			}

			public string ThrowException()
			{
				throw new NotImplementedException("AppException");
			}

			public string HelloWorld()
			{
				return "Ciao mondo";
			}

			public string HELLOWORLD()
			{
				return "HELLO";
			}

			public int VoidMethodCalled { get; set; }
			public void VoidMethod()
			{
				System.Diagnostics.Debug.WriteLine("VoidMethod called");
				VoidMethodCalled++;
			}

			public int VoidMethod2Called { get; set; }
			public void VoidMethod2()
			{
				System.Diagnostics.Debug.WriteLine("VoidMethod2 called");
				VoidMethod2Called++;
			}
		}

		class Customer
		{
			public string Name { get; set; }
			public int Age { get; set; }
			public char Gender { get; set; }
		}

		[TestMethod]
		public void Linq_Where()
		{
			var customers = new List<Customer> { 
                                    new Customer() { Name = "David", Age = 31, Gender = 'M' },
                                    new Customer() { Name = "Mary", Age = 29, Gender = 'F' },
                                    new Customer() { Name = "Jack", Age = 2, Gender = 'M' },
                                    new Customer() { Name = "Marta", Age = 1, Gender = 'F' },
                                    new Customer() { Name = "Moses", Age = 120, Gender = 'M' },
                                    };

			string whereExpression = "customer.Age > 18 && customer.Gender == 'F'";

			var interpreter = new Interpreter();
			Func<Customer, bool> dynamicWhere = interpreter.Parse<Func<Customer, bool>>(whereExpression, "customer");

			Assert.AreEqual(1, customers.Where(dynamicWhere).Count());
		}
	}
}
