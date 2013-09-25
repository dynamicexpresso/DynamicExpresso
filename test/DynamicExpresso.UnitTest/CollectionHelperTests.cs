using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace DynamicExpresso.UnitTest
{
	[TestClass]
	public class CollectionHelperTests
	{
		[TestMethod]
		public void Where()
		{
			var target = new Interpreter();

			target.SetVariable("IntCollectionHelper", new CollectionHelper<int>());

			var list = new List<int> { 1, 10, 19, 21 };

			var results = target.Eval("IntCollectionHelper.Where(list, \"x > 19\")", new Parameter("list", list))
									as IEnumerable<int>;

			Assert.AreEqual(1, results.Count());
			Assert.AreEqual(21, results.First());
		}
	}

	public class CollectionHelper<T>
	{
		readonly Interpreter _interpreter;

		public CollectionHelper()
		{
			_interpreter = new Interpreter();
		}

		public IEnumerable<T> Where(IEnumerable<T> values, string expression)
		{
			var predicate = _interpreter.Parse<Func<T, bool>>(expression, "x");

			return values.Where(predicate);
		}
	}
}
