using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace DynamicExpresso
{
	/// <summary>
	/// Lambda extensions.
	/// </summary>
	public static class InterpreterExtensions
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

		public static object Eval<T1>(this ExpressionInterpreter interpreter, string expression,
			Expression<Func<object, T1>> a1)
			=> interpreter.Eval(expression, a1.Value());

		public static object Eval<T1, T2>(this ExpressionInterpreter interpreter, string expression,
			Expression<Func<object, T1>> a1,
			Expression<Func<object, T2>> a2)
			=> interpreter.Eval(expression, a1.Value(), a2.Value());

		public static object Eval<T1, T2, T3>(this ExpressionInterpreter interpreter, string expression,
			Expression<Func<object, T1>> a1,
			Expression<Func<object, T2>> a2,
			Expression<Func<object, T3>> a3)
			=> interpreter.Eval(expression, a1.Value(), a2.Value(), a3.Value());

		public static object Eval<T1, T2, T3, T4>(this ExpressionInterpreter interpreter, string expression,
			Expression<Func<object, T1>> a1,
			Expression<Func<object, T2>> a2,
			Expression<Func<object, T3>> a3,
			Expression<Func<object, T4>> a4)
			=> interpreter.Eval(expression, a1.Value(), a2.Value(), a3.Value(), a4.Value());

		private static Parameter Value<T>(this Expression<Func<object, T>> parameter)
		{
			var objectMember = Expression.Convert(parameter.Body, typeof(object));
			var getterLambda = Expression.Lambda<Func<object>>(objectMember);
			var getter = getterLambda.Compile();

			return new Parameter(parameter.Parameters.First().Name, parameter.ReturnType, getter());
		}

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
