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
            Stopwatch stopwatch = Stopwatch.StartNew();
           
            for (int i = 0; i < 1000; i++)
            {
                new Interpreter(InterpreterOptions.Default);      
            }

            Assert.Less(stopwatch.ElapsedMilliseconds,200);
          }
    }
}