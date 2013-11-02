using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections;

namespace DynamicExpresso.UnitTest
{
	[TestClass]
	public class EnumerableTest
	{
		[TestMethod]
		public void Invoke_enumerable_extensions()
		{
			var x = new int[] { 10, 30, 4 };

			var target = new Interpreter()
									.Reference(typeof(System.Linq.Enumerable))
									.SetVariable("x", x);

			Assert.AreEqual(x.Count(), target.Eval("x.Count()"));
			Assert.AreEqual(x.Average(), target.Eval("x.Average()"));
			Assert.AreEqual(x.First(), target.Eval("x.First()"));
			Assert.AreEqual(x.Last(), target.Eval("x.Last()"));
			Assert.AreEqual(x.Max(), target.Eval("x.Max()"));
			CollectionAssert.AreEqual(x.Skip(2).ToArray(), target.Eval<IEnumerable<int>>("x.Skip(2)").ToArray());
			CollectionAssert.AreEqual(x.Skip(2).ToArray(), target.Eval<IEnumerable<int>>("x.Skip(2)").ToArray());
		}
	}
}
