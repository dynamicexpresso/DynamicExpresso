using System;
using NUnit.Framework;

namespace DynamicExpresso.UnitTest
{
	[TestFixture]
	public class FunctionTest
	{
        [Test]
		public void Calling_Function()
		{
			var target = new Interpreter();
			Func<double, double, double> pow = (x, y) => Math.Pow(x, y);
			target.SetFunction("pow", pow);

			Assert.That(target.Eval("pow(5, 2)"), Is.EqualTo(25));
		}

		[Test]
		public void Chaining_Functions()
		{
			var target = new Interpreter();
			Func<double, double, double> pow = (x, y) => Math.Pow(x, y);
			target.SetFunction("pow", pow);

			Assert.That(target.Eval("pow(5, 2).ToString()"), Is.EqualTo("25"));
		}
    }
}
