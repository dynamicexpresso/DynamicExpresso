using DynamicExpresso.Exceptions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DynamicExpresso.UnitTest
{
	[TestFixture]
	public class LambdaExpressionTest
	{
		private const InterpreterOptions _options = InterpreterOptions.Default | InterpreterOptions.LambdaExpressions;

		[Test]
		public void Invalid_Lambda_Should_Produce_a_ParseException()
		{
			var target = new Interpreter(_options);
			var list = new List<string> { "abc", "dfe", "test" };
			target.SetVariable("list", list);

			Assert.Throws<ParseException>(() => target.Parse("list.Select(str => str.Legnth)") );
		}

		[Test]
		public void Check_Lambda_Return_Type()
		{
			var target = new Interpreter(_options);
			var list = new List<string> { "abc", "dfe", "test" };
			target.SetVariable("list", list);

			var lambda = target.Parse("list.Select(str => str.Length)");

			Assert.AreEqual(typeof(IEnumerable<int>), lambda.ReturnType);
		}

		[Test]
		public void Where_inferred_parameter_type()
		{
			var target = new Interpreter(_options);
			var list = new List<int> { 1, 10, 19, 21 };
			target.SetVariable("myList", list);

			var results = target.Eval<IEnumerable<int>>("myList.Where(x => x >= 19)");

			Assert.AreEqual(2, results.Count());
			Assert.AreEqual(new[] { 19, 21 }, results);
		}

		[Test]
		public void Where_explicit_parameter_type()
		{
			var target = new Interpreter(_options);
			var list = new List<int> { 1, 10, 19, 21 };
			target.SetVariable("myList", list);

			var results = target.Eval<IEnumerable<int>>("myList.Where((int x) => x >= 19)");

			Assert.AreEqual(2, results.Count());
			Assert.AreEqual(new[] { 19, 21 }, results);
		}

		[Test]
		public void Select_inferred_return_type()
		{
			var target = new Interpreter(_options);
			var list = new List<int> { 1, 10, 19, 21 };
			target.SetVariable("myList", list);

			var results = target.Eval<IEnumerable<char>>("myList.Select(i => i.ToString()).Select(str => str[0])");

			Assert.AreEqual(4, results.Count());
			Assert.AreEqual(new[] { '1', '1', '1', '2' }, results);
		}

		[Test]
		public void Where_select()
		{
			var target = new Interpreter(_options);
			var list = new List<string> { "this", "is", "awesome" };
			target.SetVariable("myList", list);

			var results = target.Eval<IEnumerable<string>>("myList.Where(str => str.Length > 5).Select(str => str.ToUpper())");

			Assert.AreEqual(1, results.Count());
			Assert.AreEqual(new[] { "AWESOME" }, results);
		}

		[Test]
		public void Lambda_expression_to_delegate()
		{
			var target = new Interpreter(_options);
			var lambda = target.Eval<Func<string, string>>("str => str.ToUpper()");
			Assert.AreEqual("TEST", lambda.Invoke("test"));
		}

		[Test]
		public void Lambda_expression_no_arguments()
		{
			var target = new Interpreter(_options);
			var lambda = target.Eval<Func<int>>("() => 5 + 6");
			Assert.AreEqual(11, lambda.Invoke());
		}

		[Test]
		public void Lambda_expression_to_delegate_multi_params()
		{
			var target = new Interpreter(_options);
			target.SetVariable("increment", 3);
			var lambda = target.Eval<Func<int, string, string>>("(i, str) => str.ToUpper() + (i + increment)");
			Assert.AreEqual("TEST8", lambda.Invoke(5, "test"));
		}

		[Test]
		public void Select_many_str()
		{
			var target = new Interpreter(_options);
			var list = new List<string> { "ab", "cd" };
			target.SetVariable("myList", list);

			var results = target.Eval<IEnumerable<char>>("myList.SelectMany(str => str)");

			Assert.AreEqual(4, results.Count());
			Assert.AreEqual(new[] { 'a', 'b', 'c', 'd' }, results);
		}

		[Test]
		public void Select_many()
		{
			var target = new Interpreter(_options);
			var list = new[]{
				new { Strings = new[] { "ab", "cd" } },
				new { Strings = new[] { "ef", "gh" } },
			};

			target.SetVariable("myList", list);

			var results = target.Eval<IEnumerable<string>>("myList.SelectMany(obj => obj.Strings)");

			Assert.AreEqual(4, results.Count());
			Assert.AreEqual(new[] { "ab", "cd", "ef", "gh" }, results);
		}

		[Test]
		public void Nested_lambda()
		{
			var target = new Interpreter(_options);
			var list = new List<string> { "ab", "cd" };
			target.SetVariable("myList", list);

			var results = target.Eval<IEnumerable<char>>("myList.Select(str => str.SingleOrDefault(c => c == 'd')).Where(c => c != '\0')");

			Assert.AreEqual(1, results.Count());
			Assert.AreEqual(new[] { 'd' }, results);
		}

		[Test]
		public void Lambda_candidate_is_generic_parameter()
		{
			var target = new Interpreter(_options).Reference(typeof(ExtensionMethodExt));
			var str = "cd";
			target.SetVariable("str", str);

			var result = target.Eval<char>("str.MySingleOrDefault(c => c == 'd')");
			Assert.AreEqual(str.SingleOrDefault(c => c == 'd'), result);
		}

		[Test]
		public void Lambda_candidate_with_multiple_parameters()
		{
			var target = new Interpreter(_options).Reference(typeof(ExtensionMethodExt));
			var str = "cd";
			target.SetVariable("str", str);

			var result = target.Eval<char>("str.WithSeveralParams((c) => c == 'd')");
			Assert.AreEqual('d', result);

			result = target.Eval<char>("str.WithSeveralParams((c, i) => c == 'd')");
			Assert.AreEqual('d', result);

			result = target.Eval<char>("str.WithSeveralParams((c, i, str) => c == 'd')");
			Assert.AreEqual('d', result);
		}

		[Test]
		public void Sum()
		{
			var target = new Interpreter(_options);
			var list = new List<int> { 1, 2, 3 };
			target.SetVariable("myList", list);

			var results = target.Eval<int>("myList.Sum()");

			Assert.AreEqual(6, results);
		}

		[Test]
		public void Max()
		{
			var target = new Interpreter(_options);
			var list = new List<int> { 1, 2, 3 };
			target.SetVariable("myList", list);

			var results = target.Eval<int>("myList.Max()");

			Assert.AreEqual(3, results);
		}

		[Test]
		public void Sum_string_length()
		{
			var target = new Interpreter(_options);
			var list = new List<string> { "abc", "dfe", "test" };
			target.SetVariable("myList", list);

			var results = target.Eval<int>("myList.Sum(str => str.Length)");

			Assert.AreEqual(10, results);
		}

		[Test]
		public void Parent_scope_variable()
		{
			var target = new Interpreter(_options);
			var list = new List<int> { 1, 2, 3 };
			target.SetVariable("myList", list);
			target.SetVariable("increment", 3);

			var results = target.Eval<IEnumerable<int>>("myList.Select(i => i + increment)");

			Assert.AreEqual(3, results.Count());
			Assert.AreEqual(new[] { 4, 5, 6 }, results);
		}

		[Test]
		public void Lambda_with_multiple_params()
		{
			var target = new Interpreter(_options);
			var list = new List<string> { "aaaaa", "bbbb", "ccc", "ddd" };
			target.SetVariable("myList", list);

			var results = target.Eval<IEnumerable<string>>("myList.TakeWhile((item, idx) => idx <= 2 && item.Length >= 3)");

			Assert.AreEqual(3, results.Count());
			Assert.AreEqual(new[] { "aaaaa", "bbbb", "ccc" }, results);
		}

		[Test]
		public void Two_lambda_parameters()
		{
			var target = new Interpreter(_options);
			var list = new List<string> { "aaaaa", "bbbb", "ccc", "ddd" };
			target.SetVariable("myList", list);
			var results = target.Eval<Dictionary<string, int>>("myList.ToDictionary(str => str, str => str.Length)");

			Assert.AreEqual(4, results.Count);
			Assert.AreEqual(list.ToDictionary(str => str, str => str.Length), results);
		}

		[Test]
		public void Zip()
		{
			var target = new Interpreter(_options);
			var strList = new List<string> { "aa", "bb", "cc", "dd" };
			var intList = new List<int> { 1, 2, 3 };
			target.SetVariable("strList", strList);
			target.SetVariable("intList", intList);
			var results = target.Eval<IEnumerable<string>>("strList.Zip(intList, (str, i) => str + i)");

			Assert.AreEqual(3, results.Count());
			Assert.AreEqual(strList.Zip(intList, (str, i) => str + i), results);
		}
	}

	/// <summary>
	/// Ensure that a lambda expression is matched to a parameter of type delegate
	/// (so the 1st overload shouldn't be considered during resolution)
	/// </summary>
	internal static class ExtensionMethodExt
	{
		public static TSource MySingleOrDefault<TSource>(this IEnumerable<TSource> source, TSource defaultValue)
		{
			return source.SingleOrDefault();
		}

		public static TSource MySingleOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
		{
			return source.SingleOrDefault(predicate);
		}

		public static TSource WithSeveralParams<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
		{
			return source.SingleOrDefault(predicate);
		}

		public static TSource WithSeveralParams<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
		{
			return source.SingleOrDefault(_ => predicate(_, 0));
		}

		public static TSource WithSeveralParams<TSource>(this IEnumerable<TSource> source, Func<TSource, int, string, bool> predicate)
		{
			return source.SingleOrDefault(_ => predicate(_, 0, string.Empty));
		}
	}
}
