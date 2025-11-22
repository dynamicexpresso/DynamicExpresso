using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ExceptionServices;
using DynamicExpresso.Reflection;
using DynamicExpresso.Resources;

namespace DynamicExpresso
{
	/// <summary>
	/// Represents a lambda expression that can be invoked. This class is thread safe.
	/// </summary>
	public class Lambda
	{
		private readonly Expression _expression;
		private readonly ParserArguments _parserArguments;

		// Delegate whose parameters are in UsedParameters order.
		private readonly Lazy<Delegate> _delegate;
		private readonly bool _preferInterpretation;

		// Snapshots taken at construction time so we don't re-enumerate and allocate on every call.
		private readonly Parameter[] _declaredParameters;
		private readonly Parameter[] _usedParameters;
		private readonly ParameterExpression[] _declaredParameterExpressions;

		// For each used parameter index, which declared parameter index it corresponds to.
		private readonly int[] _usedToDeclaredIndex;
		private readonly bool _allUsedAndInDeclaredOrder;
		private readonly Type[] _effectiveUsedTypes;
		private readonly bool[] _usedAllowsNull;
		private readonly int _declaredCount;
		private readonly int _usedCount;

		// Fast path: declared-order object[] -> result.
		private readonly Lazy<Func<object[], object>> _fastInvokerFromDeclared;

		internal Lambda(Expression expression, ParserArguments parserArguments, bool preferInterpretation = false)
		{
			_expression = expression ?? throw new ArgumentNullException(nameof(expression));
			_parserArguments = parserArguments ?? throw new ArgumentNullException(nameof(parserArguments));

			_preferInterpretation = preferInterpretation;

			// Snapshot parameters once: avoids repeated enumeration/allocation.
			var declaredParameters = _parserArguments.DeclaredParameters.ToArray();
			_declaredParameters = declaredParameters;
			_usedParameters = _parserArguments.UsedParameters.ToArray();

			_declaredParameterExpressions = _declaredParameters.Select(p => p.Expression).ToArray();

			_declaredCount = _declaredParameters.Length;
			_usedCount = _usedParameters.Length;

			_delegate = new Lazy<Delegate>(() =>
				Expression.Lambda(_expression, _usedParameters.Select(p => p.Expression).ToArray())
					.Compile(_preferInterpretation));

			// Precompute used-index -> declared-index mapping for the fast path.
			if (_usedCount > 0)
			{
				if (_declaredCount == 0)
					throw new InvalidOperationException("Used parameters exist but there are no declared parameters.");

				var nameToDeclaredIndex =
					new Dictionary<string, int>(_declaredCount, _parserArguments.Settings.KeyComparer);
				for (var i = 0; i < declaredParameters.Length; i++)
				{
					nameToDeclaredIndex[declaredParameters[i].Name] = i;
				}

				_usedToDeclaredIndex = new int[_usedCount];
				for (var i = 0; i < _usedCount; i++)
				{
					var usedName = _usedParameters[i].Name;
					if (!nameToDeclaredIndex.TryGetValue(usedName, out var declaredIndex))
						throw new InvalidOperationException(
							$"Used parameter '{usedName}' was not found in declared parameters.");

					_usedToDeclaredIndex[i] = declaredIndex;
				}
			}
			else
			{
				_usedToDeclaredIndex = Array.Empty<int>();
			}

			_allUsedAndInDeclaredOrder =
				_usedCount == _declaredCount &&
				Enumerable.Range(0, _usedCount).All(i => _usedToDeclaredIndex[i] == i);

			if (_usedCount == 0)
			{
				_effectiveUsedTypes = Array.Empty<Type>();
				_usedAllowsNull = Array.Empty<bool>();
			}
			else
			{
				_effectiveUsedTypes = new Type[_usedCount];
				_usedAllowsNull = new bool[_usedCount];

				for (var i = 0; i < _usedCount; i++)
				{
					var t = _usedParameters[i].Type;
					if (t == typeof(object))
					{
						_effectiveUsedTypes[i] = typeof(object);
						_usedAllowsNull[i] = true;
					}
					else
					{
						var underlying = Nullable.GetUnderlyingType(t);
						_effectiveUsedTypes[i] = underlying ?? t;
						_usedAllowsNull[i] = underlying != null || !t.IsValueType;
					}
				}
			}

			_fastInvokerFromDeclared = new Lazy<Func<object[], object>>(BuildFastInvokerFromDeclared);
		}

		public Expression Expression { get { return _expression; } }
		public bool CaseInsensitive { get { return _parserArguments.Settings.CaseInsensitive; } }
		public string ExpressionText { get { return _parserArguments.ExpressionText; } }
		public Type ReturnType { get { return Expression.Type; } }

		/// <summary>
		/// Gets the parameters actually used in the expression parsed.
		/// </summary>
		/// <value>The used parameters.</value>
		[Obsolete("Use UsedParameters or DeclaredParameters")]
		public IEnumerable<Parameter> Parameters { get { return _usedParameters; } }

		/// <summary>
		/// Gets the parameters actually used in the expression parsed.
		/// </summary>
		/// <value>The used parameters.</value>
		public IEnumerable<Parameter> UsedParameters { get { return _usedParameters; } }

		/// <summary>
		/// Gets the parameters declared when parsing the expression.
		/// </summary>
		/// <value>The declared parameters.</value>
		public IEnumerable<Parameter> DeclaredParameters { get { return _declaredParameters; } }

		public IEnumerable<ReferenceType> Types { get { return _parserArguments.UsedTypes; } }
		public IEnumerable<Identifier> Identifiers { get { return _parserArguments.UsedIdentifiers; } }

		public object Invoke()
		{
			if (_usedCount == 0)
			{
				return _fastInvokerFromDeclared.Value(Array.Empty<object>());
			}

			// Fallback: preserve the original behavior where missing parameters
			// TargetParameterCountException is likely to be thrown.
			return InvokeWithUsedParameters(Array.Empty<object>());
		}

		public object Invoke(params Parameter[] parameters)
		{
			return Invoke((IEnumerable<Parameter>)parameters);
		}

		/// <summary>
		/// Invoke the expression with the given named parameters.
		/// Parameters are matched by name against the parameters actually used in the expression.
		/// </summary>
		public object Invoke(IEnumerable<Parameter> parameters)
		{
			if (parameters == null)
				throw new ArgumentNullException(nameof(parameters));

			var paramList = parameters as IList<Parameter> ?? parameters.ToArray();
			var matchedValues = new List<object>(_usedCount);

			foreach (var used in _usedParameters)
			{
				foreach (var actual in paramList)
				{
					if (actual != null &&
					    used.Name.Equals(actual.Name, _parserArguments.Settings.KeyComparison))
					{
						matchedValues.Add(actual.Value);
					}
				}
			}

			if (_usedCount == 0)
			{
				return _fastInvokerFromDeclared.Value(Array.Empty<object>());
			}

			if (matchedValues.Count == _usedCount)
			{
				var declaredArgs = new object[_declaredCount];
				for (var i = 0; i < _usedCount; i++)
				{
					var declaredIndex = _usedToDeclaredIndex[i];
					declaredArgs[declaredIndex] = matchedValues[i];
				}

				return Invoke(declaredArgs);
			}

			return InvokeWithUsedParameters(matchedValues.ToArray());
		}

		/// <summary>
		/// Invoke the expression with the given parameter values.
		/// The values are in the same order as the parameters declared when parsing (DeclaredParameters).
		/// Only the parameters actually used in the expression are passed to the underlying delegate.
		/// </summary>
		/// <param name="args">Values for declared parameters, in declared order.</param>
		public object Invoke(params object[] args)
		{
			if (args == null)
			{
				return Invoke();
			}

			if (_declaredCount != args.Length)
				throw new InvalidOperationException(ErrorMessages.ArgumentCountMismatch);

			// No parameters are actually used: ignore any supplied values.
			if (_usedCount == 0)
			{
				return _fastInvokerFromDeclared.Value(Array.Empty<object>());
			}

			// Fast path: all values already directly assignable to the expected parameter types.
			if (CanUseFastInvoker(args))
			{
				try
				{
					return _fastInvokerFromDeclared.Value(args);
				}
				catch (TargetInvocationException exc)
				{
					if (exc.InnerException != null)
						ExceptionDispatchInfo.Capture(exc.InnerException).Throw();

					throw;
				}
			}

			var usedArgs = BuildUsedArgsFromDeclared(args);
			return InvokeWithUsedParameters(usedArgs);
		}

		/// <summary>
		/// orderedUsedArgs must be in UsedParameters order (the same order used to compile _delegate).
		/// This method preserves the original DynamicInvoke-based behavior, including exception types
		/// for mismatched argument counts and conversion failures.
		/// </summary>
		private object InvokeWithUsedParameters(object[] orderedArgs)
		{
			try
			{
				return _delegate.Value.DynamicInvoke(orderedArgs);
			}
			catch (TargetInvocationException exc)
			{
				if (exc.InnerException != null)
					ExceptionDispatchInfo.Capture(exc.InnerException).Throw();

				throw;
			}
		}

		private object[] BuildUsedArgsFromDeclared(object[] declaredArgs)
		{
			if (_usedCount == 0)
				return Array.Empty<object>();

			if (_allUsedAndInDeclaredOrder)
				return declaredArgs;

			var used = new object[_usedCount];
			for (var i = 0; i < _usedCount; i++)
			{
				var declaredIndex = _usedToDeclaredIndex[i];
				used[i] = declaredArgs[declaredIndex];
			}

			return used;
		}

		private bool CanUseFastInvoker(object[] declaredArgs)
		{
			if (_usedCount == 0)
				return true;

			if (declaredArgs == null || declaredArgs.Length != _declaredCount)
				return false;

			for (var i = 0; i < _usedCount; i++)
			{
				var declaredIndex = _usedToDeclaredIndex[i];
				var value = declaredArgs[declaredIndex];

				if (!IsDirectlyAssignable(i, value))
					return false;
			}

			return true;
		}

		private bool IsDirectlyAssignable(int usedIndex, object value)
		{
			if (_effectiveUsedTypes[usedIndex] == typeof(object))
				return true;

			if (value == null)
			{
				// null is allowed for reference types and Nullable<T>
				return _usedAllowsNull[usedIndex];
			}

			return _effectiveUsedTypes[usedIndex].IsInstanceOfType(value);
		}

		private Func<object[], object> BuildFastInvokerFromDeclared()
		{
			// Ensure the underlying delegate is compiled once.
			var del = _delegate.Value;
			var delType = del.GetType();
			var invokeMethod = delType.GetMethod("Invoke");
			if (invokeMethod == null)
				throw new InvalidOperationException("Delegate Invoke method not found.");

			var argsParam = Expression.Parameter(typeof(object[]), "args");
			var target = Expression.Constant(del, delType);

			Expression body;
			if (_usedCount == 0)
			{
				var call = Expression.Call(target, invokeMethod);
				body = call.Type == typeof(void)
					? Expression.Block(call, Expression.Constant(null, typeof(object)))
					: (Expression)Expression.Convert(call, typeof(object));
			}
			else
			{
				var callArgs = new Expression[_usedCount];

				for (var i = 0; i < _usedCount; i++)
				{
					var declaredIndex = _usedToDeclaredIndex[i];

					// args[declaredIndex]
					var indexExpr = Expression.Constant(declaredIndex);
					var accessExpr = Expression.ArrayIndex(argsParam, indexExpr);

					// We only use this fast path when IsDirectlyAssignable has already confirmed
					// that the runtime value is compatible with the target type, so this Convert
					// can't introduce new InvalidCastExceptions compared to DynamicInvoke.
					callArgs[i] = Expression.Convert(accessExpr, _usedParameters[i].Type);
				}

				var call = Expression.Call(target, invokeMethod, callArgs);
				body = call.Type == typeof(void)
					? Expression.Block(call, Expression.Constant(null, typeof(object)))
					: (Expression)Expression.Convert(call, typeof(object));
			}

			var lambda = Expression.Lambda<Func<object[], object>>(body, argsParam);
			return lambda.Compile(_preferInterpretation);
		}

		public override string ToString()
		{
			return ExpressionText;
		}

		/// <summary>
		/// Generate the given delegate by compiling the lambda expression.
		/// </summary>
		/// <typeparam name="TDelegate">
		/// The delegate to generate. Delegate parameters must match the ones defined
		/// when creating the expression, see DeclaredParameters.
		/// </typeparam>
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
		/// <typeparam name="TDelegate">
		/// The delegate to generate. Delegate parameters must match the ones defined
		/// when creating the expression, see DeclaredParameters.
		/// </typeparam>
		public Expression<TDelegate> LambdaExpression<TDelegate>()
		{
			return Expression.Lambda<TDelegate>(_expression, _declaredParameterExpressions);
		}

		internal LambdaExpression LambdaExpression(Type delegateType)
		{
			var parameterExpressions = _declaredParameterExpressions;
			var types = delegateType.GetGenericArguments();

			// return type
			var genericType = delegateType.GetGenericTypeDefinition();
			if (genericType == ReflectionExtensions.GetFuncType(parameterExpressions.Length))
				types[types.Length - 1] = _expression.Type;

			var inferredDelegateType = genericType.MakeGenericType(types);
			return Expression.Lambda(inferredDelegateType, _expression, parameterExpressions);
		}
	}
}
