using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace DynamicExpresso
{
	/// <summary>
	/// Represents a lambda expression that can be invoked. This class is thread safe.
	/// </summary>
	public class Lambda : ParseResult
	{
		private readonly bool _caseInsensitive;
		private readonly StringComparison _keyComparison;
		private readonly Lazy<Delegate> _delegate;

		internal Lambda(Expression expression, ParserArguments parserArguments) : base(expression, parserArguments)
		{
			_caseInsensitive = parserArguments.Settings.CaseInsensitive;
			_keyComparison = parserArguments.Settings.KeyComparison;
			_delegate = new Lazy<Delegate>(() => this.Compile());
		}

		public bool CaseInsensitive => _caseInsensitive;

		public object Invoke()
		{
			return InvokeWithUsedParameters(new object[0]);
		}

		public object Invoke(params Parameter[] parameters)
		{
			return Invoke((IEnumerable<Parameter>)parameters);
		}

		public object Invoke(IEnumerable<Parameter> parameters)
		{
			var args = (from usedParameter in UsedParameters
						from actualParameter in parameters
						where usedParameter.Name.Equals(actualParameter.Name, _keyComparison)
						select actualParameter.Value)
				.ToArray();

			return InvokeWithUsedParameters(args);
		}

		/// <summary>
		/// Invoke the expression with the given parameters values.
		/// </summary>
		/// <param name="args">Order of parameters must be the same of the parameters used during parse (DeclaredParameters).</param>
		/// <returns></returns>
		public object Invoke(params object[] args)
		{
			var parameters = new List<Parameter>();
			var declaredParameters = DeclaredParameters.ToArray();

			if (args != null)
			{
				if (declaredParameters.Length != args.Length)
					throw new InvalidOperationException("Arguments count mismatch.");

				for (var i = 0; i < args.Length; i++)
				{
					var parameter = new Parameter(
						declaredParameters[i].Name,
						declaredParameters[i].Type,
						args[i]);

					parameters.Add(parameter);
				}
			}

			return Invoke(parameters);
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
	}
}
