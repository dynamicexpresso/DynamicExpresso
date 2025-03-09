using BenchmarkDotNet.Attributes;
using DynamicExpresso;
using NCalc;

namespace Benchmark;

public static class Program
{
	public static void Main()
	{
		BenchmarkDotNet.Running.BenchmarkRunner.Run<DynamicExpressoVsNCalcBenchmark>();
		//PerformanceProfile();
	}

	public static void PerformanceProfile()
	{
		var interpreter = new Interpreter();
		interpreter.SetFunction("Al", (Func<string, bool>)(id => false));

		var stopwatch = new System.Diagnostics.Stopwatch();
		stopwatch.Start();

		var expressionText = string.Join(" || ", Enumerable.Range(1, 20).Select(i => $"Al(\"id{i}\")"));
		for (var i = 0; i < 1000; i++)
		{
			var t = interpreter.Eval<bool>(expressionText);
			Console.Write(t ? "1" : "0");
		}

		Console.WriteLine();
		Console.WriteLine(stopwatch.ElapsedMilliseconds);
	}
}

public class DynamicExpressoVsNCalcBenchmark
{
	private string _expressionText;
	private Interpreter _dyn;

	[Params(5, 20)]
	public int N;

	[GlobalSetup]
	public void Setup()
	{
		_dyn = new Interpreter();
		_dyn.SetFunction("Al", (Func<string, bool>)(id => false));
		_expressionText = string.Join(" || ", Enumerable.Range(1, N).Select(i => $"Al(\"id{i}\")"));
	}

	[Benchmark]
	public bool DynamicExpresso()
	{
		return _dyn.Eval<bool>(_expressionText);
	}

	[Benchmark]
	public bool Ncalc()
	{
		var expression = new Expression(_expressionText);
		expression.Functions["Al"] = (args) =>
		{
			return false;
		};

		return (bool)expression.Evaluate();
	}
}
