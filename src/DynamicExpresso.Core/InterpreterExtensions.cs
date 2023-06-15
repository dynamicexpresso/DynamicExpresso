using System;
using System.Linq;
using System.Linq.Expressions;

namespace DynamicExpresso
{
	/// <summary>
	/// Interpreter extensions.
	/// </summary>
	public static class InterpreterExtensions
	{
		/// <summary>
		/// Compiles lambda with used parameters.
		/// </summary>
		public static Delegate Compile(this ParseResult parseResult)
		{
			return parseResult.LambdaExpression().Compile();
		}

		/// <summary>
		/// Compiles lambda with declared parameters.
		/// </summary>
		public static TDelegate Compile<TDelegate>(this ParseResult parseResult)
		{
			return parseResult.LambdaExpression<TDelegate>().Compile();
		}

		/// <summary>
		/// Compiles lambda with declared parameters.
		/// </summary>
		public static TDelegate Compile<TDelegate>(this ParseResult<TDelegate> parseResult)
		{
			return Compile<TDelegate>((ParseResult)parseResult);
		}

		/// <summary>
		/// Convert parse result to a lambda expression with used parameters.
		/// </summary>
		public static LambdaExpression LambdaExpression(this ParseResult parseResult)
		{
			return Expression.Lambda(parseResult.Expression, parseResult.UsedParameters.Select(_ => _.Expression).ToArray());
		}

		/// <summary>
		/// Convert parse result to a lambda expression with declared parameters.
		/// </summary>
		public static Expression<TDelegate> LambdaExpression<TDelegate>(this ParseResult parseResult)
		{
			return Expression.Lambda<TDelegate>(parseResult.Expression, parseResult.DeclaredParameters.Select(_ => _.Expression).ToArray());
		}

		/// <summary>
		/// Convert parse result to a lambda expression with declared parameters.
		/// </summary>
		public static Expression<TDelegate> LambdaExpression<TDelegate>(this ParseResult<TDelegate> parseResult)
		{
			return ((ParseResult)parseResult).LambdaExpression<TDelegate>();
		}

		/// <summary>
		/// Convert parse result to a lambda expression with declared parameters.
		/// </summary>
		public static LambdaExpression LambdaExpression(this ParseResult parseResult, Type delegateType)
		{
			return Expression.Lambda(delegateType, parseResult.Expression, parseResult.DeclaredParameters.Select(_ => _.Expression).ToArray());
		}
	}
}
