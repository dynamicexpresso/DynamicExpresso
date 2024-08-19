using System;
using System.Linq.Expressions;
using DynamicExpresso.Parsing;
using DynamicExpresso.Reflection;

namespace DynamicExpresso.Resolution
{
	internal static class ExpressionUtils
	{
		public static Expression PromoteExpression(Expression expr, Type type, bool exact)
		{
			if (expr.Type == type) return expr;
			if (expr is ConstantExpression)
			{
				var ce = (ConstantExpression)expr;
				if (ce == ParserConstants.NullLiteralExpression)
				{
					if (type.ContainsGenericParameters)
						return null;
					if (!type.IsValueType || TypeUtils.IsNullableType(type))
						return Expression.Constant(null, type);
				}
			}

			if (expr is InterpreterExpression ie)
			{
				if (!ie.IsCompatibleWithDelegate(type))
					return null;

				if (!type.ContainsGenericParameters)
					return ie.EvalAs(type);

				return expr;
			}

			if (type.IsGenericType && !TypeUtils.IsNumericType(type))
			{
				var genericType = TypeUtils.FindAssignableGenericType(expr.Type, type);
				if (genericType != null)
					return Expression.Convert(expr, genericType);
			}

			if (TypeUtils.IsCompatibleWith(expr.Type, type) || expr is DynamicExpression)
			{
				if (type.IsValueType || exact)
				{
					return Expression.Convert(expr, type);
				}
				return expr;
			}

			return null;
		}
	}
}
