using System.Diagnostics;
using NUnit.Framework;

namespace DynamicExpresso.UnitTest
{
	[TestFixture]
	public class PerformanceTest
	{
		[Test]
		public void InterperterCreation()
		{
			// TODO Study if there is a better way to test performance

			var stopwatch = Stopwatch.StartNew();

			for (var i = 0; i < 1000; i++)
			{
				new Interpreter(InterpreterOptions.Default);
			}

			Assert.Less(stopwatch.ElapsedMilliseconds, 200);
		}
	}
}