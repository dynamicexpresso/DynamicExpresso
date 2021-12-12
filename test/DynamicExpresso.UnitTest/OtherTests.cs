﻿using System;
using NUnit.Framework;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace DynamicExpresso.UnitTest
{
	[TestFixture]
	public class OtherTests
	{
		[Test]
		public void Space_Characters_Are_Ignored()
		{
			var target = new Interpreter();

			Assert.AreEqual(46, target.Eval("     45\t\t  + 1 \r  \n"));
		}

		[Test]
		public void Empty_Null_Withespace_Expression()
		{
			var target = new Interpreter();

			Assert.AreEqual(null, target.Eval(""));
			Assert.AreEqual(typeof(void), target.Parse("").Expression.Type);

			Assert.AreEqual(null, target.Eval(null));
			Assert.AreEqual(typeof(void), target.Parse(null).Expression.Type);

			Assert.AreEqual(null, target.Eval("  \t\t\r\n  \t   "));
			Assert.AreEqual(typeof(void), target.Parse("  \t\t\r\n  \t   ").Expression.Type);
		}

		[Test]
		public void Complex_expression()
		{
			var target = new Interpreter();

			var x = new MyTestService();
			var y = 5;
			var parameters = new[] {new Parameter("x", x.GetType(), x), new Parameter("y", y.GetType(), y)};

			Assert.AreEqual(x.AProperty > y && x.HelloWorld().Length == 10, target.Eval("x.AProperty      >\t y && \r\n x.HelloWorld().Length == 10", parameters));
			Assert.AreEqual(x.AProperty * (4 + 65) / x.AProperty, target.Eval("x.AProperty * (4 + 65) / x.AProperty", parameters));

			Assert.AreEqual(Convert.ToString(x.AProperty * (4 + 65) / x.AProperty), target.Eval("Convert.ToString(x.AProperty * (4 + 65) / x.AProperty)", parameters));
		}

		[Test]
		public void Parse_An_Expression_And_Invoke_It_With_Different_Parameters()
		{
			var service = new MyTestService();

			var target = new Interpreter()
													.SetVariable("service", service);

			var parseResult = target.Parse(
				"x > 4 ? service.VoidMethod() : service.VoidMethod2()",
				Expression.Parameter(typeof(int), "x"));

			var func = parseResult.Compile();

			Assert.AreEqual(typeof(void), parseResult.Expression.Type);

			Assert.AreEqual(0, service.VoidMethodCalled);
			Assert.AreEqual(0, service.VoidMethod2Called);

			func.DynamicInvoke(5);
			Assert.AreEqual(1, service.VoidMethodCalled);
			Assert.AreEqual(0, service.VoidMethod2Called);

			func.DynamicInvoke(2);
			Assert.AreEqual(1, service.VoidMethodCalled);
			Assert.AreEqual(1, service.VoidMethod2Called);
		}

		[Test]
		public void Should_Understand_ReturnType_Of_expressions()
		{
			var target = new Interpreter();

			var x = new MyTestService();
			var y = 5;
			var parameters = new[] {Expression.Parameter(x.GetType(), "x"), Expression.Parameter(y.GetType(), "y"),};

			Assert.AreEqual(typeof(bool), target.Parse("x.AProperty > y && x.HelloWorld().Length == 10", parameters).Expression.Type);
			Assert.AreEqual(typeof(int), target.Parse("x.AProperty * (4 + 65) / x.AProperty", parameters).Expression.Type);
		}

		[Test]
		public void Execute_the_same_function_multiple_times()
		{
			var target = new Interpreter();

			var functionX = target.Parse("Math.Pow(x, y) + 5",
					Expression.Parameter(typeof(double), "x"),
					Expression.Parameter(typeof(double), "y"))
				.Compile();

			Assert.AreEqual(Math.Pow(15, 12) + 5, functionX.DynamicInvoke(15, 12));
			Assert.AreEqual(Math.Pow(5, 1) + 5, functionX.DynamicInvoke(5, 1));
			Assert.AreEqual(Math.Pow(11, 8) + 5, functionX.DynamicInvoke(11, 8));
			Assert.AreEqual(Math.Pow(3, 4) + 5, functionX.DynamicInvoke(3.0, 4.0));
			Assert.AreEqual(Math.Pow(9, 2) + 5, functionX.DynamicInvoke(9.0, 2.0));
			Assert.AreEqual(Math.Pow(1, 3) + 5, functionX.DynamicInvoke(1.0, 3.0));
		}

		[Test]
		public void Linq_Where()
		{
			var customers = new List<Customer> {
                                    new Customer() { Name = "David", Age = 31, Gender = 'M' },
                                    new Customer() { Name = "Mary", Age = 29, Gender = 'F' },
                                    new Customer() { Name = "Jack", Age = 2, Gender = 'M' },
                                    new Customer() { Name = "Marta", Age = 1, Gender = 'F' },
                                    new Customer() { Name = "Moses", Age = 120, Gender = 'M' },
                                    };

			var whereExpression = "customer.Age > 18 && customer.Gender == 'F'";

			var interpreter = new Interpreter();
			var dynamicWhere = interpreter.Parse<Func<Customer, bool>>(whereExpression, "customer").Compile();

			Assert.AreEqual(1, customers.Where(dynamicWhere).Count());
		}

		[Test]
		public void Linq_Where2()
		{
			var prices = new [] { 5, 8, 6, 2 };

			var whereFunction = new Interpreter().Parse<Func<int, bool>>("arg > 5").Compile();

			Assert.AreEqual(2, prices.Where(whereFunction).Count());
		}

		[Test]
		public void Linq_Queryable_Expression_Where()
		{
			var customers = (new List<Customer> {
				new Customer() { Name = "David", Age = 31, Gender = 'M' },
				new Customer() { Name = "Mary", Age = 29, Gender = 'F' },
				new Customer() { Name = "Jack", Age = 2, Gender = 'M' },
				new Customer() { Name = "Marta", Age = 1, Gender = 'F' },
				new Customer() { Name = "Moses", Age = 120, Gender = 'M' },
			}).AsQueryable();

			var whereExpression = "customer.Age > 18 && customer.Gender == 'F'";

			var interpreter = new Interpreter();
			var expression = interpreter.Parse<Func<Customer, bool>>(whereExpression, "customer").AsExpression();

			Assert.AreEqual(1, customers.Where(expression).Count());
		}

		[Test]
		public void Multiple_Parentheses_Math_Expression()
		{
			var interpreter = new Interpreter();

			Assert.AreEqual(2 + ((1 + 5) * (2 - 1)), interpreter.Eval("2 + ((1 + 5) * (2 - 1))"));
			Assert.AreEqual(2 + ((((1 + 5))) * (((2 - 1)) + 5.5)), interpreter.Eval("2 + ((((1 + 5))) * (((2 - 1))+5.5))"));
		}

		[Test]
		public void Multiple_Parentheses_Cast_Expression()
		{
			var interpreter = new Interpreter();

			Assert.AreEqual(((double)5).GetType().Name, interpreter.Eval("((double)5).GetType().Name"));
		}

		private class MyTestService
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

		private class Customer
		{
			public string Name { get; set; }
			public int Age { get; set; }
			public char Gender { get; set; }
		}
	}
}
