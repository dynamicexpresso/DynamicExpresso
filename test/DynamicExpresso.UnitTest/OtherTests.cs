using System;
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

			Assert.That(target.Eval("     45\t\t  + 1 \r  \n"), Is.EqualTo(46));
		}

		[Test]
		public void Empty_Null_Withespace_Expression()
		{
			var target = new Interpreter();

			Assert.That(target.Eval(""), Is.EqualTo(null));
			Assert.That(target.Parse("").ReturnType, Is.EqualTo(typeof(void)));

			Assert.That(target.Eval(null), Is.EqualTo(null));
			Assert.That(target.Parse(null).ReturnType, Is.EqualTo(typeof(void)));

			Assert.That(target.Eval("  \t\t\r\n  \t   "), Is.EqualTo(null));
			Assert.That(target.Parse("  \t\t\r\n  \t   ").ReturnType, Is.EqualTo(typeof(void)));
		}

		[Test]
		public void Complex_expression()
		{
			var target = new Interpreter();

			var x = new MyTestService();
			var y = 5;
			var parameters = new[] {
                            new Parameter("x", x.GetType(), x),
                            new Parameter("y", y.GetType(), y),
                            };

			Assert.That(target.Eval("x.AProperty      >\t y && \r\n x.HelloWorld().Length == 10", parameters), Is.EqualTo(x.AProperty > y && x.HelloWorld().Length == 10));
			Assert.That(target.Eval("x.AProperty * (4 + 65) / x.AProperty", parameters), Is.EqualTo(x.AProperty * (4 + 65) / x.AProperty));

			Assert.That(target.Eval("Convert.ToString(x.AProperty * (4 + 65) / x.AProperty)", parameters), Is.EqualTo(Convert.ToString(x.AProperty * (4 + 65) / x.AProperty)));
		}

		[Test]
		public void Parse_An_Expression_And_Invoke_It_With_Different_Parameters()
		{
			var service = new MyTestService();

			var target = new Interpreter()
													.SetVariable("service", service);

			var func = target.Parse("x > 4 ? service.VoidMethod() : service.VoidMethod2()",
															new Parameter("x", typeof(int)));

			Assert.That(func.ReturnType, Is.EqualTo(typeof(void)));

			Assert.That(service.VoidMethodCalled, Is.EqualTo(0));
			Assert.That(service.VoidMethod2Called, Is.EqualTo(0));

			func.Invoke(new Parameter("x", 5));
			Assert.That(service.VoidMethodCalled, Is.EqualTo(1));
			Assert.That(service.VoidMethod2Called, Is.EqualTo(0));

			func.Invoke(new Parameter("x", 2));
			Assert.That(service.VoidMethodCalled, Is.EqualTo(1));
			Assert.That(service.VoidMethod2Called, Is.EqualTo(1));
		}

		[Test]
		public void Should_Understand_ReturnType_Of_expressions()
		{
			var target = new Interpreter();

			var x = new MyTestService();
			var y = 5;
			var parameters = new[] {
                            new Parameter("x", x.GetType(), x),
                            new Parameter("y", y.GetType(), y),
                            };

			Assert.That(target.Parse("x.AProperty > y && x.HelloWorld().Length == 10", parameters).ReturnType, Is.EqualTo(typeof(bool)));
			Assert.That(target.Parse("x.AProperty * (4 + 65) / x.AProperty", parameters).ReturnType, Is.EqualTo(typeof(int)));
		}

		[Test]
		public void Execute_the_same_function_multiple_times()
		{
			var target = new Interpreter();

			var functionX = target.Parse("Math.Pow(x, y) + 5",
													new Parameter("x", typeof(double)),
													new Parameter("y", typeof(double)));

			Assert.That(functionX.Invoke(15, 12), Is.EqualTo(Math.Pow(15, 12) + 5));
			Assert.That(functionX.Invoke(5, 1), Is.EqualTo(Math.Pow(5, 1) + 5));
			Assert.That(functionX.Invoke(11, 8), Is.EqualTo(Math.Pow(11, 8) + 5));
			Assert.That(functionX.Invoke(new Parameter("x", 3.0),
																													new Parameter("y", 4.0)), Is.EqualTo(Math.Pow(3, 4) + 5));
			Assert.That(functionX.Invoke(new Parameter("x", 9.0),
																													new Parameter("y", 2.0)), Is.EqualTo(Math.Pow(9, 2) + 5));
			Assert.That(functionX.Invoke(new Parameter("x", 1.0),
																													new Parameter("y", 3.0)), Is.EqualTo(Math.Pow(1, 3) + 5));
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

			string whereExpression = "customer.Age > 18 && customer.Gender == 'F'";

			var interpreter = new Interpreter();
			Func<Customer, bool> dynamicWhere = interpreter.ParseAsDelegate<Func<Customer, bool>>(whereExpression, "customer");

			Assert.That(customers.Where(dynamicWhere).Count(), Is.EqualTo(1));
		}

		[Test]
		public void Linq_Where2()
		{
			var prices = new [] { 5, 8, 6, 2 };

			var whereFunction = new Interpreter()
				.ParseAsDelegate<Func<int, bool>>("arg > 5");

			Assert.That(prices.Where(whereFunction).Count(), Is.EqualTo(2));
		}

		[Test]
		public void Linq_Queryable_Expression_Where()
		{
			IQueryable<Customer> customers = (new List<Customer> { 
				new Customer() { Name = "David", Age = 31, Gender = 'M' },
				new Customer() { Name = "Mary", Age = 29, Gender = 'F' },
				new Customer() { Name = "Jack", Age = 2, Gender = 'M' },
				new Customer() { Name = "Marta", Age = 1, Gender = 'F' },
				new Customer() { Name = "Moses", Age = 120, Gender = 'M' },
			}).AsQueryable();

			string whereExpression = "customer.Age > 18 && customer.Gender == 'F'";

			var interpreter = new Interpreter();
			Expression<Func<Customer, bool>> expression = interpreter.ParseAsExpression<Func<Customer, bool>>(whereExpression, "customer");

			Assert.That(customers.Where(expression).Count(), Is.EqualTo(1));
		}

		[Test]
		public void Multiple_Parentheses_Math_Expression()
		{
			var interpreter = new Interpreter();

			Assert.That(interpreter.Eval("2 + ((1 + 5) * (2 - 1))"), Is.EqualTo(2 + ((1 + 5) * (2 - 1))));
			Assert.That(interpreter.Eval("2 + ((((1 + 5))) * (((2 - 1))+5.5))"), Is.EqualTo(2 + ((((1 + 5))) * (((2 - 1)) + 5.5))));
		}

		[Test]
		public void Multiple_Parentheses_Cast_Expression()
		{
			var interpreter = new Interpreter();

			Assert.That(interpreter.Eval("((double)5).GetType().Name"), Is.EqualTo(((double)5).GetType().Name));
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
