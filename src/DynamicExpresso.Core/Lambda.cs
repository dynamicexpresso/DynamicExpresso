using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using DynamicExpresso.Exceptions;
using DynamicExpresso.Reflection;
using DynamicExpresso.Parsing;

namespace DynamicExpresso
{
	/// <summary>
	/// Represents a lambda expression that can be invoked. This class is thread safe.
	/// </summary>
	public class Lambda
	{
		readonly Expression _expression;
		readonly ParserArguments _parserArguments;

		readonly Delegate _delegate;

		internal Lambda(Expression expression, ParserArguments parserArguments)
		{
			if (expression == null)
				throw new ArgumentNullException("expression");
			if (parserArguments == null)
				throw new ArgumentNullException("parserArguments");

			_expression = expression;
			_parserArguments = parserArguments;

			// Note: I always compile the generic lambda. Maybe in the future this can be a setting because if I generate a typed delegate this compilation is not required.
			var lambdaExpression = Expression.Lambda(_expression, _parserArguments.UsedParameters.Select(p => p.Expression).ToArray());
			_delegate = lambdaExpression.Compile();
		}

		public Expression Expression { get { return _expression; } }
		public bool CaseInsensitive { get { return _parserArguments.Settings.CaseInsensitive; } }
		public string ExpressionText { get { return _parserArguments.ExpressionText; } }
		public Type ReturnType { get { return _delegate.Method.ReturnType; } }

		/// <summary>
		/// Gets the parameters actually used in the expression parsed.
		/// </summary>
		/// <value>The used parameters.</value>
		[Obsolete("Use UsedParameters or DeclaredParameters")]
		public IEnumerable<Parameter> Parameters { get { return _parserArguments.UsedParameters; } }

		/// <summary>
		/// Gets the parameters actually used in the expression parsed.
		/// </summary>
		/// <value>The used parameters.</value>
		public IEnumerable<Parameter> UsedParameters { get { return _parserArguments.UsedParameters; } }
		/// <summary>
		/// Gets the parameters declared when parsing the expression.
		/// </summary>
		/// <value>The declared parameters.</value>
		public IEnumerable<Parameter> DeclaredParameters { get { return _parserArguments.DeclaredParameters; } }

		public IEnumerable<ReferenceType> Types { get { return _parserArguments.UsedTypes; } }
		public IEnumerable<Identifier> Identifiers { get { return _parserArguments.UsedIdentifiers; } }

		public object Invoke()
		{
			return Invoke(new object[0]);
		}

		public object Invoke(params Parameter[] parameters)
		{
			return Invoke((IEnumerable<Parameter>)parameters);
		}

		public object Invoke(IEnumerable<Parameter> parameters)
		{
			var args = (from usedParameter in UsedParameters
				from actualParameter in parameters
				where usedParameter.Name.Equals(actualParameter.Name, _parserArguments.Settings.KeyComparison)
				select actualParameter.Value)
				.ToArray();

			return Invoke(args);
		}

		public object Invoke(params object[] args)
		{
			try
			{
				return _delegate.DynamicInvoke(args);
			}
			catch (TargetInvocationException exc)
			{
				if (exc.InnerException != null)
				{
					exc.InnerException.PreserveStackTrace();
					throw exc.InnerException;
				}
				else
					throw;
			}
		}

		public override string ToString()
		{
			return ExpressionText;
		}

		/// <summary>
		/// Generate the given delegate by compiling the lambda expression.
		/// </summary>
		/// <typeparam name="TDelegate">The delegate to generate. Delegate parameters must match the one defined when creating the expression.</typeparam>
		public TDelegate Compile<TDelegate>()
		{
			var lambdaExpression = LambdaExpression<TDelegate>();
			return lambdaExpression.Compile();
		}

		[Obsolete("Use Compile<TDelegate>()")]
		public TDelegate Compile<TDelegate>(IEnumerable<Parameter> parameters)
		{
			var lambdaExpression = Expression.Lambda<TDelegate>(_expression, parameters.Select(p => p.Expression).ToArray());
			return lambdaExpression.Compile();
		}

		/// <summary>
		/// Generate a lambda expression.
		/// </summary>
		/// <returns>The lambda expression.</returns>
		/// <typeparam name="TDelegate">The delegate to generate. Delegate parameters must match the one defined when creating the expression.</typeparam>
		public Expression<TDelegate> LambdaExpression<TDelegate>()
		{
			return Expression.Lambda<TDelegate>(_expression, DeclaredParameters.Select(p => p.Expression).ToArray());
		}

		/*
		/// <summary>
		/// Generate a lambda expression with the given delegate and allowing to specify parameters.
		/// </summary>
		/// <returns>The lambda expression.</returns>
		/// <param name="parameters">Parameters must match the one defined in the TDelegate and in the expressino parsed.</param>
		/// <typeparam name="TDelegate">The delegate to generate.</typeparam>
		public Expression<TDelegate> LambdaExpression<TDelegate>(IEnumerable<Parameter> parameters)
		{
			return Expression.Lambda<TDelegate>(_expression, parameters.Select(p => p.Expression).ToArray());
		}
		*/
	}
}
