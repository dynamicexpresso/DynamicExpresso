﻿using System;
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

		readonly LambdaExpression _lambdaExpression;
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
			_lambdaExpression = Expression.Lambda(_expression, Parameters.Select(p => p.Expression).ToArray());
			_delegate = _lambdaExpression.Compile();
		}

		public Expression Expression { get { return _expression; } }
		public bool CaseInsensitive { get { return _parserArguments.CaseInsensitive; } }
		public string ExpressionText { get { return _parserArguments.ExpressionText; } }
		public Type ReturnType { get { return _lambdaExpression.ReturnType; } }
		public IEnumerable<Parameter> Parameters { get { return _parserArguments.UsedParameters; } }
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
			var args = (from declaredParameter in Parameters
									join actualParameter in parameters
									 on declaredParameter.Name equals actualParameter.Name
									select actualParameter.Value).ToArray();

			return Invoke(args);
		}

		public object Invoke(params object[] args)
		{
			if (_delegate == null)
			{
				throw new InvalidOperationException("Lambda not compiled. Call Compile() method first.");
			}

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

		public TDelegate Compile<TDelegate>(IEnumerable<Parameter> parameters)
		{
			var lambdaExpression = Expression.Lambda<TDelegate>(_expression, parameters.Select(p => p.Expression).ToArray());
			return lambdaExpression.Compile();
		}
	}
}
