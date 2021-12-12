using System;
using System.Linq;
using System.Linq.Expressions;

namespace DynamicExpresso
{
	/// <summary>
	/// Lambda extensions.
	/// </summary>
	public static class LambdaExtensions
	{
		/// <summary>
		/// Compiles lambda with declared parameters.
		/// </summary>
		public static Delegate Compile(this ParseResult parseResult)
		{
			var lambdaExpression = Expression.Lambda(parseResult.Expression, parseResult.DeclaredParameters.ToArray());

			return lambdaExpression.Compile();
		}

		public static TDelegate Compile<TDelegate>(this ParseResult parseResult)
		{
			var lambdaExpression = Expression.Lambda<TDelegate>(parseResult.Expression, parseResult.DeclaredParameters.ToArray());

			return lambdaExpression.Compile();
		}

		public static TDelegate Compile<TDelegate>(this ParseResult<TDelegate> parseResult)
		{
			return Compile<TDelegate>((ParseResult) parseResult);
		}

		public static Expression<TDelegate> AsExpression<TDelegate>(this ParseResult parseResult)
		{
			return Expression.Lambda<TDelegate>(parseResult.Expression, parseResult.DeclaredParameters.ToArray());
		}

		public static Expression<TDelegate> AsExpression<TDelegate>(this ParseResult<TDelegate> parseResult)
		{
			return ((ParseResult) parseResult).AsExpression<TDelegate>();
		}

		public static LambdaExpression AsLambdaExpression(this ParseResult parseResult, Type delegateType)
		{
			return Expression.Lambda(delegateType, parseResult.Expression, parseResult.DeclaredParameters.ToArray());
		}

		public static object Eval(this Interpreter interpreter, string expression, params Parameter[] args)
		{
			return interpreter
				.Parse(expression, args.Select(x => x.Expression).ToArray())
				.Compile()
				.DynamicInvoke(args.Select(x => x.Value).ToArray());
		}

		public static TReturnType Eval<TReturnType>(this Interpreter interpreter, string expression, params Parameter[] args)
		{
			return (TReturnType) Eval(interpreter, expression, args);
		}
	}
}
