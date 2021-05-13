using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace DynamicExpresso.UnitTest
{
	[TestFixture]
	public class LambdaExpressionTest
	{
		[Test]
		public void Where_inferred_parameter_type()
		{
			var target = new Interpreter();
			var list = new List<int> { 1, 10, 19, 21 };
			target.SetVariable("myList", list);

			var results = target.Eval<IEnumerable<int>>("myList.Where(x => x >= 19)");

			Assert.AreEqual(2, results.Count());
			Assert.AreEqual(new[] { 19, 21 }, results);
		}

		[Test]
		public void Where_explicit_parameter_type()
		{
			var target = new Interpreter();
			var list = new List<int> { 1, 10, 19, 21 };
			target.SetVariable("myList", list);

			var results = target.Eval<IEnumerable<int>>("myList.Where((int x) => x >= 19)");

			Assert.AreEqual(2, results.Count());
			Assert.AreEqual(new[] { 19, 21 }, results);
		}

		[Test]
		public void Select_inferred_return_type()
		{
			var target = new Interpreter();
			var list = new List<int> { 1, 10, 19, 21 };
			target.SetVariable("myList", list);

			var results = target.Eval<IEnumerable<char>>("myList.Select(i => i.ToString()).Select(str => str[0])");

			Assert.AreEqual(4, results.Count());
			Assert.AreEqual(new[] { '1', '1', '1', '2' }, results);
		}

		[Test]
		public void Where_select()
		{
			var target = new Interpreter();
			var list = new List<string> { "this", "is", "awesome" };
			target.SetVariable("myList", list);

			var results = target.Eval<IEnumerable<string>>("myList.Where(str => str.Length > 5).Select(str => str.ToUpper())");

			Assert.AreEqual(1, results.Count());
			Assert.AreEqual(new[] { "AWESOME" }, results);
		}

		[Test]
		public void Select_many()
		{
			var target = new Interpreter();
			var list = new List<string> { "ab", "cd" };
			target.SetVariable("myList", list);

			var results = target.Eval<IEnumerable<char>>("myList.SelectMany(str => str)");

			Assert.AreEqual(4, results.Count());
			Assert.AreEqual(new[] { 'a', 'b', 'c', 'd' }, results);
		}

		[Test]
		public void Sum()
		{
			var target = new Interpreter();
			var list = new List<int> { 1, 2, 3 };
			target.SetVariable("myList", list);

			var results = target.Eval<int>("myList.Sum()");

			Assert.AreEqual(6, results);
		}

		[Test]
		public void Sum_string_length()
		{
			var target = new Interpreter();
			var list = new List<string> { "abc", "dfe", "test" };
			target.SetVariable("myList", list);

			var results = target.Eval<int>("myList.Sum(str => str.Length)");

			Assert.AreEqual(10, results);
		}
	}
}
