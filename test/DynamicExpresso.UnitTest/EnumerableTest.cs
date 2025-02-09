using System.Linq;
using NUnit.Framework;
using System.Collections.Generic;

namespace DynamicExpresso.UnitTest
{
	[TestFixture]
	public class EnumerableTest
	{
		[Test]
		public void Invoke_enumerable_extensions()
		{
			var x = new int[] { 10, 30, 4 };

			var target = new Interpreter()
									.Reference(typeof(System.Linq.Enumerable))
									.SetVariable("x", x);

			Assert.That(target.Eval("x.Count()"), Is.EqualTo(x.Count()));
			Assert.That(target.Eval("x.Average()"), Is.EqualTo(x.Average()));
			Assert.That(target.Eval("x.First()"), Is.EqualTo(x.First()));
			Assert.That(target.Eval("x.Last()"), Is.EqualTo(x.Last()));
			Assert.That(target.Eval("x.Max()"), Is.EqualTo(x.Max()));
			Assert.That(target.Eval<IEnumerable<int>>("x.Skip(2)").ToArray(), Is.EqualTo(x.Skip(2).ToArray()).AsCollection);
			Assert.That(target.Eval<IEnumerable<int>>("x.Skip(2)").ToArray(), Is.EqualTo(x.Skip(2).ToArray()).AsCollection);
		}
	}
}
