using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DynamicExpresso.UnitTest
{
	[TestFixture]
    public class FakeDynamicMemberAccess
    {
		private class DictMemberAccessProvider<TDict> : IMemberAccessProvider
		{
			readonly HashSet<string> _allowedProperties;
			public DictMemberAccessProvider(HashSet<string> allowedProperties = null)
			{
				_allowedProperties = allowedProperties;
			}

			public bool TryGetMemberAccess(Expression leftHand, string identifier, out Expression result)
			{
				if (leftHand.Type != typeof(TDict))
				{
					result = null;
					return false;
				}
				if (_allowedProperties != null && _allowedProperties.Contains(identifier) == false)
				{
					result = null;
					return false;
				}

				result = Expression.Convert(
					Expression.Call(leftHand,
					typeof(TDict).GetMethod("get_Item", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public),
					Expression.Constant(identifier)
					), typeof(int));
				return true;
			}
		}
		private class TestDict : Dictionary<string, int>
		{
			public int Y { get; set; }
		}

		[Test]
		public void Parse_Dictionary()
		{
			var interpreter = new Interpreter()
			{
				MemberAccessProvider = new DictMemberAccessProvider<Dictionary<string, int>>()
			};
			Dictionary<string, int> context = new Dictionary<string, int>() { { "x", 5 }, { "y", 6 } };

			var cb = interpreter.ParseAsDelegate<Func<Dictionary<string, int>, int>>("x + y", "this");
			Assert.AreEqual(11, cb(context));

			interpreter.SetVariable("this", context);
			Assert.AreEqual(5, interpreter.Eval("x"));
			Assert.AreEqual(6, interpreter.Eval("this.y"));
			Assert.AreEqual(30, interpreter.Eval("x * this.y"));
		}

		[Test]
		public void NonExistentMembersFallThrough()
		{
			var interpreter = new Interpreter(InterpreterOptions.DefaultCaseInsensitive)
			{
				MemberAccessProvider = new DictMemberAccessProvider<TestDict>(new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) { "x" })
			};
			TestDict context = new TestDict() { { "x", 5 } };
			context.Y = 10;

			var cb = interpreter.ParseAsDelegate<Func<TestDict, int>>("x + y", "this");
			Assert.AreEqual(15, cb(context));

			interpreter.SetVariable("this", context);
			Assert.AreEqual(5, interpreter.Eval("x"));
			Assert.AreEqual(10, interpreter.Eval("this.y"));
		}

		[Test]
		public void AccessIsSetAtParseTime()
		{
			var interpreter = new Interpreter(InterpreterOptions.DefaultCaseInsensitive)
			{
				MemberAccessProvider = new DictMemberAccessProvider<TestDict>(new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) { "x" })
			};
			TestDict context = new TestDict() { { "x", 5 } };
			context.Y = 10;

			var cb = interpreter.ParseAsDelegate<Func<TestDict, int>>("x + y", "this");
			Assert.AreEqual(15, cb(context));
			context["y"] = 30;
			Assert.AreEqual(15, cb(context));
		}
    }
}
