using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ExceptionServices;
using DynamicExpresso.Exceptions;

namespace DynamicExpresso
{
	/// <summary>
	/// Interpreter extensions.
	/// </summary>
	public static class InterpreterExtensions
	{
		/// <summary>
		/// Parse a text expression with expected return type.
		/// </summary>
		/// <param name="interpreter"></param>
		/// <param name="expressionText">Expression statement</param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		/// <exception cref="ParseException"></exception>
		public static ParseResult ParseWithReturnType<TReturnType>(
			this ExpressionInterpreter interpreter,
			string expressionText,
			params ParameterExpression[] parameters)
		{
			return interpreter.Parse(expressionText, typeof(TReturnType), parameters);
		}

		/// <summary>
		/// Compiles lambda with declared parameters.
		/// </summary>
		public static Delegate Compile(this ParseResult parseResult)
		{
			var lambdaExpression = Expression.Lambda(parseResult.Expression, parseResult.DeclaredParameters.ToArray());

			return lambdaExpression.Compile();
		}

		/// <summary>
		/// Compiles lambda with declared parameters.
		/// </summary>
		public static TDelegate Compile<TDelegate>(this ParseResult parseResult)
		{
			var lambdaExpression = Expression.Lambda<TDelegate>(parseResult.Expression, parseResult.DeclaredParameters.ToArray());

			return lambdaExpression.Compile();
		}

		/// <summary>
		/// Compiles lambda with declared parameters.
		/// </summary>
		public static TDelegate Compile<TDelegate>(this ParseResult<TDelegate> parseResult)
		{
			return Compile<TDelegate>((ParseResult) parseResult);
		}

		/// <summary>
		/// Convert parse result to expression.
		/// </summary>
		public static Expression<TDelegate> AsExpression<TDelegate>(this ParseResult parseResult)
		{
			return Expression.Lambda<TDelegate>(parseResult.Expression, parseResult.DeclaredParameters.ToArray());
		}

		/// <summary>
		/// Convert parse result to expression.
		/// </summary>
		public static Expression<TDelegate> AsExpression<TDelegate>(this ParseResult<TDelegate> parseResult)
		{
			return ((ParseResult) parseResult).AsExpression<TDelegate>();
		}

		/// <summary>
		/// Convert parse result to lambda expression.
		/// </summary>
		public static LambdaExpression AsLambdaExpression(this ParseResult parseResult, Type delegateType)
		{
			return Expression.Lambda(delegateType, parseResult.Expression, parseResult.DeclaredParameters.ToArray());
		}

		/// <summary>
		/// Evaluate expression.
		/// </summary>
		public static object Eval<T1>(this ExpressionInterpreter interpreter, string expression,
			Func<object, T1> a1)
			=> interpreter.Eval(expression, a1.Value());

		/// <summary>
		/// Evaluate expression.
		/// </summary>
		public static object Eval<T1, T2>(this ExpressionInterpreter interpreter, string expression,
			Func<object, T1> a1,
			Func<object, T2> a2)
			=> interpreter.Eval(expression, a1.Value(), a2.Value());

		/// <summary>
		/// Evaluate expression.
		/// </summary>
		public static object Eval<T1, T2, T3>(this ExpressionInterpreter interpreter, string expression,
			Func<object, T1> a1,
			Func<object, T2> a2,
			Func<object, T3> a3)
			=> interpreter.Eval(expression, a1.Value(), a2.Value(), a3.Value());

		/// <summary>
		/// Evaluate expression.
		/// </summary>
		public static object Eval<T1, T2, T3, T4>(this ExpressionInterpreter interpreter, string expression,
			Func<object, T1> a1,
			Func<object, T2> a2,
			Func<object, T3> a3,
			Func<object, T4> a4)
			=> interpreter.Eval(expression, a1.Value(), a2.Value(), a3.Value(), a4.Value());

		private static Parameter Value<T>(this Func<object, T> parameter)
		{
			return new Parameter(
				parameter.Method.GetParameters().First().Name,
				parameter.Method.ReturnType,
				parameter(default));
		}

		/// <summary>
		/// Evaluate expression.
		/// </summary>
		public static object Eval(this ExpressionInterpreter interpreter, string expression, params Parameter[] args)
		{
			try
			{
				return interpreter
					.Parse(expression, args.Select(x => Expression.Parameter(x.Type, x.Name)).ToArray())
					.Compile()
					.DynamicInvoke(args.Select(x => x.Value).ToArray());
			}
			catch (TargetInvocationException exc)
			{
				if (exc.InnerException != null)
					ExceptionDispatchInfo.Capture(exc.InnerException).Throw();

				throw;
			}
		}
	}
}
