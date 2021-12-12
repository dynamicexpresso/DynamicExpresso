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
	}
}
