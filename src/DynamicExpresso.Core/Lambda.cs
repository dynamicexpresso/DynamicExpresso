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
		private readonly InvocationContext _invocation;

		internal Lambda(Expression expression, ParserArguments parserArguments, bool preferInterpretation = false)
		{
			_expression = expression ?? throw new ArgumentNullException(nameof(expression));
			_parserArguments = parserArguments ?? throw new ArgumentNullException(nameof(parserArguments));

			var settings = _parserArguments.Settings;
			_invocation = new InvocationContext(
				expression,
				_parserArguments.DeclaredParameters.ToArray(),
				_parserArguments.UsedParameters.ToArray(),
				settings.KeyComparison,
				settings.KeyComparer,
				preferInterpretation);
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
		public IEnumerable<Parameter> Parameters { get { return _invocation.UsedParameters; } }

		/// <summary>
		/// Gets the parameters actually used in the expression parsed.
		/// </summary>
		/// <value>The used parameters.</value>
		public IEnumerable<Parameter> UsedParameters { get { return _invocation.UsedParameters; } }

		/// <summary>
		/// Gets the parameters declared when parsing the expression.
		/// </summary>
		/// <value>The declared parameters.</value>
		public IEnumerable<Parameter> DeclaredParameters { get { return _invocation.DeclaredParameters; } }

		public IEnumerable<ReferenceType> Types { get { return _parserArguments.UsedTypes; } }
		public IEnumerable<Identifier> Identifiers { get { return _parserArguments.UsedIdentifiers; } }

		public object Invoke()
		{
			return _invocation.InvokeNoArgs();
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
			return _invocation.InvokeFromNamed(parameters);
		}

		/// <summary>
		/// Invoke the expression with the given parameter values.
		/// The values are in the same order as the parameters declared when parsing (DeclaredParameters).
		/// Only the parameters actually used in the expression are passed to the underlying delegate.
		/// </summary>
		/// <param name="args">Values for declared parameters, in declared order.</param>
		public object Invoke(params object[] args)
		{
			return _invocation.InvokeFromDeclared(args);
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
			return Expression.Lambda<TDelegate>(_expression, _invocation.DeclaredParameterExpressions);
		}

		internal LambdaExpression LambdaExpression(Type delegateType)
		{
			var parameterExpressions = _invocation.DeclaredParameterExpressions;
			var types = delegateType.GetGenericArguments();

			// return type
			var genericType = delegateType.GetGenericTypeDefinition();
			if (genericType == ReflectionExtensions.GetFuncType(parameterExpressions.Length))
				types[types.Length - 1] = _expression.Type;

			var inferredDelegateType = genericType.MakeGenericType(types);
			return Expression.Lambda(inferredDelegateType, _expression, parameterExpressions);
		}

		private sealed class InvocationContext
		{
			private readonly Expression _expression;
			private readonly Parameter[] _declaredParameters;
			private readonly Parameter[] _usedParameters;
			private readonly StringComparison _keyComparison;
			private readonly IEqualityComparer<string> _keyComparer;
			private readonly bool _preferInterpretation;

			// used index -> declared index
			private readonly int[] _usedToDeclaredIndex;

			// Delegate with parameters in UsedParameters order.
			private readonly Lazy<Delegate> _delegate;

			public InvocationContext(
				Expression expression,
				Parameter[] declaredParameters,
				Parameter[] usedParameters,
				StringComparison keyComparison,
				IEqualityComparer<string> keyComparer,
				bool preferInterpretation)
			{
				_expression = expression ?? throw new ArgumentNullException(nameof(expression));
				_declaredParameters = declaredParameters ?? Array.Empty<Parameter>();
				_usedParameters = usedParameters ?? Array.Empty<Parameter>();
				_keyComparison = keyComparison;
				_keyComparer = keyComparer ?? StringComparer.InvariantCulture;
				_preferInterpretation = preferInterpretation;

				DeclaredParameterExpressions = _declaredParameters.Select(p => p.Expression).ToArray();

				_delegate = new Lazy<Delegate>(() =>
					Expression.Lambda(_expression, _usedParameters.Select(p => p.Expression).ToArray())
						.Compile(_preferInterpretation));

				_usedToDeclaredIndex = BuildUsedToDeclaredIndex(_declaredParameters, _usedParameters, _keyComparer);
			}

			public Parameter[] DeclaredParameters => _declaredParameters;
			public Parameter[] UsedParameters => _usedParameters;
			public ParameterExpression[] DeclaredParameterExpressions { get; }

			public object InvokeNoArgs()
			{
				return InvokeWithUsedParameters(Array.Empty<object>());
			}

			public object InvokeFromDeclared(object[] args)
			{
				if (args == null)
					return InvokeNoArgs();

				if (_declaredParameters.Length != args.Length)
					throw new InvalidOperationException(ErrorMessages.ArgumentCountMismatch);

				if (_usedParameters.Length == 0)
					return InvokeWithUsedParameters(Array.Empty<object>());

				var usedArgs = BuildUsedArgsFromDeclared(args);
				return InvokeWithUsedParameters(usedArgs);
			}

			public object InvokeFromNamed(IEnumerable<Parameter> parameters)
			{
				if (parameters == null)
					throw new ArgumentNullException(nameof(parameters));

				if (_usedParameters.Length == 0)
					return InvokeWithUsedParameters(Array.Empty<object>());

				var paramList = parameters as IList<Parameter> ?? parameters.ToArray();
				var matchedValues = new List<object>(_usedParameters.Length);

				foreach (var used in _usedParameters)
				{
					foreach (var actual in paramList)
					{
						if (actual != null &&
						    string.Equals(used.Name, actual.Name, _keyComparison))
						{
							matchedValues.Add(actual.Value);
						}
					}
				}

				return InvokeWithUsedParameters(matchedValues.ToArray());
			}

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

			private static int[] BuildUsedToDeclaredIndex(
				IReadOnlyList<Parameter> declaredParameters,
				IReadOnlyList<Parameter> usedParameters,
				IEqualityComparer<string> keyComparer)
			{
				if (usedParameters.Count == 0)
					return Array.Empty<int>();

				if (declaredParameters.Count == 0)
					throw new InvalidOperationException("Used parameters exist but there are no declared parameters.");

				var nameToDeclaredIndex = new Dictionary<string, int>(declaredParameters.Count, keyComparer);
				for (var i = 0; i < declaredParameters.Count; i++)
				{
					nameToDeclaredIndex[declaredParameters[i].Name] = i;
				}

				var map = new int[usedParameters.Count];
				for (var i = 0; i < usedParameters.Count; i++)
				{
					var usedName = usedParameters[i].Name;
					if (!nameToDeclaredIndex.TryGetValue(usedName, out var declaredIndex))
						throw new InvalidOperationException(
							$"Used parameter '{usedName}' was not found in declared parameters.");

					map[i] = declaredIndex;
				}

				return map;
			}

			private object[] BuildUsedArgsFromDeclared(object[] declaredArgs)
			{
				if (_usedParameters.Length == 0)
					return Array.Empty<object>();

				var used = new object[_usedParameters.Length];
				for (var i = 0; i < _usedParameters.Length; i++)
				{
					var declaredIndex = _usedToDeclaredIndex[i];
					used[i] = declaredArgs[declaredIndex];
				}

				return used;
			}

		}
	}
}
