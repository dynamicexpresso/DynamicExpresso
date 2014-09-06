﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using DynamicExpresso.Exceptions;

namespace DynamicExpresso
{
	/// <summary>
	/// Represents a lambda expression that can be invoked. This class is thread safe.
	/// </summary>
	public class Lambda
	{
		readonly LambdaExpression _lambdaExpression;
		readonly Delegate _delegate;

		public Lambda(LambdaExpression lambdaExpression)
		{
			if (lambdaExpression == null)
				throw new ArgumentNullException("lambdaExpression");

			_lambdaExpression = lambdaExpression;
			_delegate = _lambdaExpression.Compile();
		}

		public LambdaExpression LambdaExpression
		{
			get { return _lambdaExpression; }
		}

		public Type ReturnType
		{
			get { return _lambdaExpression.ReturnType; }
		}

		public Parameter[] Parameters
		{
			get
			{
				return _lambdaExpression.Parameters
								.Select(p => new Parameter(p.Name, p.Type))
								.ToArray();
			}
		}

		public object Invoke()
		{
			return Invoke(new object[0]);
		}

		public object Invoke(params Parameter[] parameters)
		{
			var args = (from dp in _lambdaExpression.Parameters
									join rp in parameters
									 on dp.Name equals rp.Name
									select rp.Value).ToArray();

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
			return _lambdaExpression.ToString();
		}
	}
}
