using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DynamicExpresso.UnitTest
{
	[TestFixture]
	public class ThreadingTest
	{
		private static void DynamicExpresso_Interpreter_Eval1()
		{
			var parser = new Interpreter();
			parser.SetVariable("C0", 5);
			parser.SetVariable("x", 50);
			Assert.That(parser.Identifiers.First(i => i.Name == "C0").Expression.ToString(), Is.EqualTo("5"));
			Assert.That(parser.Identifiers.First(i => i.Name == "x").Expression.ToString(), Is.EqualTo("50"));

			var result = parser.Eval<double>("x*C0");
			Assert.That(result, Is.EqualTo(250d));
		}

		private static void DynamicExpresso_Interpreter_Eval2()
		{
			var parser = new Interpreter();
			parser.SetVariable("C0", 5);
			parser.SetVariable("y", 250);
			Assert.That(parser.Identifiers.First(i => i.Name == "C0").Expression.ToString(), Is.EqualTo("5"));
			Assert.That(parser.Identifiers.First(i => i.Name == "y").Expression.ToString(), Is.EqualTo("250"));

			var result = parser.Eval<double>("y/C0");
			Assert.That(result, Is.EqualTo(50d));
		}

		private static void DynamicExpresso_Interpreter_Eval_Sequence()
		{
			for (var i = 0; i < 10; i++)
			{
				DynamicExpresso_Interpreter_Eval1();
				DynamicExpresso_Interpreter_Eval2();
			}
		}

		[Test]
		public async Task DynamicExpresso_Interpreter_Eval_Threading()
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

			const int NumTasks = 5;

			var tasksToRun = new List<Task>();
			for (var i = 0; i < NumTasks; i++)
			{
				var taskToRunLater = new Task(DynamicExpresso_Interpreter_Eval_Sequence);
				tasksToRun.Add(taskToRunLater);
			}

			foreach (var taskToRunLater in tasksToRun)
			{
				taskToRunLater.Start();
			}

			foreach (var taskToRunLater in tasksToRun)
			{
				await taskToRunLater;
			}
		}

		[Test]
		public void Should_Pass_Parallel_Eval()
		{
			var target = new Interpreter();
			var conds = Enumerable.Repeat("Country != \"France\"", 10);
			var parameters = new[] { new Parameter("Country", "Italy") };
			Parallel.ForEach(conds, exp =>
			{
				Assert.That(target.Eval(exp, parameters), Is.True);
			});
		}

		[Test]
		public void Should_Fail_Parallel_Eval()
		{
			var target = new Interpreter();
			var conds = Enumerable.Repeat("Country != \"France\"", 10);

			Parallel.ForEach(conds, exp =>
			{
				var parameters = new[] { new Parameter("Country", "Italy") };
				Assert.That(target.Eval(exp, parameters), Is.True);
			});
		}
	}
}
