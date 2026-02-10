using BenchmarkDotNet.Attributes;

namespace DynamicExpresso.Benchmarks;

[MemoryDiagnoser]
public class LambdaBenchmarks
{
	private const string ExpressionText =
		"((a + b) * c - (double)d / (e + f + 1)) + Math.Max(a, b)";

	private Interpreter _interpreter = null!;

	private Lambda _lambda = null!;

	private object[] _args = null!;
	private Parameter[] _declared = null!;
	private Parameter[] _parameterValues = null!;

	private const int A = 1;
	private const int B = 2;
	private const int C = 3;
	private const int D = 4;
	private const int E = 5;
	private const int F = 6;

	[GlobalSetup]
	public void Setup()
	{
		_interpreter = new Interpreter();

		_declared = new[]
		{
			new Parameter("a", typeof(int)),
			new Parameter("b", typeof(int)),
			new Parameter("c", typeof(int)),
			new Parameter("d", typeof(int)),
			new Parameter("e", typeof(int)),
			new Parameter("f", typeof(int))
		};

		_lambda = _interpreter.Parse(
			ExpressionText,
			typeof(double),
			_declared);

		_args = new object[] { A, B, C, D, E, F };

		_parameterValues = new[]
		{
			new Parameter("a", typeof(int), A),
			new Parameter("b", typeof(int), B),
			new Parameter("c", typeof(int), C),
			new Parameter("d", typeof(int), D),
			new Parameter("e", typeof(int), E),
			new Parameter("f", typeof(int), F)
		};
	}

	[Benchmark(Description = "Invoke cached lambda (object[])")]
	public double Invoke_ObjectArray()
	{
		double sum = 0;
		for (var i = 0; i < 100_000; i++)
		{
			sum += (double)_lambda.Invoke(_args);
		}

		return sum;
	}

	[Benchmark(Description = "Invoke cached lambda (IEnumerable<Parameter>)")]
	public double Invoke_ParametersEnumerable()
	{
		double sum = 0;
		for (var i = 0; i < 100_000; i++)
		{
			sum += (double)_lambda.Invoke(_parameterValues);
		}

		return sum;
	}

	[Benchmark(Description = "Eval (IEnumerable<Parameter>)")]
	public double Eval_ParametersEnumerable()
	{
		double sum = 0;
		for (var i = 0; i < 100_000; i++)
		{
			sum += _interpreter.Eval<double>(ExpressionText, _parameterValues);
		}

		return sum;
	}
}
