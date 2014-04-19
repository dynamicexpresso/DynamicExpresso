using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace DynamicExpresso.UnitTest
{
	[TestClass]
	public class DynamicTest
	{
		//[TestMethod]
		//public void Read_Property_of_an_ExpandoObject()
		//{
		//	dynamic dyn = new ExpandoObject();
		//	dyn.Foo = "bar";

		//	var interpreter = new Interpreter()
		//		.SetVariable("dyn", dyn);

		//	Assert.AreEqual(dyn.Foo, interpreter.Eval("dyn.Foo"));
		//}

		//[TestMethod]
		//public void Invoke_Method_of_an_ExpandoObject()
		//{
		//	dynamic dyn = new ExpandoObject();
		//	dyn.Foo = new Func<string>(() => "bar");

		//	var interpreter = new Interpreter()
		//		.SetVariable("dyn", dyn);

		//	Assert.AreEqual(dyn.Foo(), interpreter.Eval("dyn.Foo()")); 
		//}

		//[TestMethod]
		//public void Case_Insensitive_Dynamic_Members()
		//{
		//	dynamic dyn = new ExpandoObject();
		//	dyn.Bar = 10;

		//	var result = new Interpreter()
		//		.Eval("dyn.BAR", new Parameter("dyn", dyn));

		//	Assert.AreEqual(10, result);
		//}

		//[TestMethod]
		//public void Test_With_Standard_Object()
		//{
		//	var myInstance = DateTime.Now;

		//	var methodInfo = myInstance.GetType().GetMethod("ToUniversalTime");

		//	var methodCallExpression = Expression.Call(Expression.Constant(myInstance), methodInfo);
		//	var expression = Expression.Lambda(methodCallExpression);

		//	Assert.AreEqual(myInstance.ToUniversalTime(), expression.Compile().DynamicInvoke());
		//}

		//[TestMethod]
		//public void Test_With_Dynamic_Object()
		//{
		//	dynamic myInstance = new ExpandoObject();
		//	myInstance.MyMethod = new Func<string>(() => "hello world");

		//	var binder = Binder.InvokeMember(
		//		CSharpBinderFlags.None,
		//		"MyMethod",
		//		null,
		//		this.GetType(),
		//		new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.Constant, null) });

		//	var methodCallExpression = Expression.Dynamic(binder, typeof(object), Expression.Constant(myInstance));
		//	var expression = Expression.Lambda(methodCallExpression);

		//	Assert.AreEqual(myInstance.MyMethod(), expression.Compile().DynamicInvoke());
		//}
	}
}
