using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace DynamicExpresso.UnitTest
{
	[TestFixture]
	public class CollectionHelperTests
	{
		[Test]
		public void Where()
		{
			var target = new Interpreter();

			target.SetVariable("IntCollectionHelper", new CollectionHelper<int>());

			var list = new List<int> { 1, 10, 19, 21 };

			var results = target.Eval("IntCollectionHelper.Where(list, \"x > 19\")", new Parameter("list", list))
									as IEnumerable<int>;

			Assert.That(results.Count(), Is.EqualTo(1));
			Assert.That(results.First(), Is.EqualTo(21));
		}
	}

	public class CollectionHelper<T>
	{
		private readonly Interpreter _interpreter;

		public CollectionHelper()
		{
			_interpreter = new Interpreter();
		}

		public IEnumerable<T> Where(IEnumerable<T> values, string expression)
		{
			var predicate = _interpreter.ParseAsDelegate<Func<T, bool>>(expression, "x");

			return values.Where(predicate);
		}
	}
}
