using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using DynamicExpresso.Reflection;

namespace DynamicExpresso
{
	public class Identifier
	{
		public Expression Expression { get; private set; }
		public string Name { get; private set; }

		public Identifier(string name, Expression expression)
		{
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException("name");

			if (expression == null)
				throw new ArgumentNullException("expression");

			Expression = expression;
			Name = name;
		}
	}

	internal class FunctionIdentifier : Identifier
	{
		internal FunctionIdentifier(string name, Delegate value) : base(name, new MethodGroupExpression(value))
		{
		}

		internal void AddOverload(Delegate overload)
		{
			((MethodGroupExpression)Expression).AddOverload(overload);
		}
	}

	/// <summary>
	/// Custom expression that simulates a method group (ie. a group of methods with the same name).
	/// It's used when custom functions are added to the interpreter via <see cref="Interpreter.SetFunction(string, Delegate)"/>.
	/// </summary>
	internal class MethodGroupExpression : Expression
	{
		public class Overload
		{
			public Delegate Delegate { get; }

			private MethodBase _method;
			public MethodBase Method
			{
				get
				{
					if (_method == null)
						_method = Delegate.Method;

					return _method;
				}
			}

			// we'll most likely never need this: it was needed before https://github.com/dotnet/roslyn/pull/53402
			private MethodBase _invokeMethod;
			public MethodBase InvokeMethod
			{
				get
				{
					if (_invokeMethod == null)
						_invokeMethod = MemberFinder.FindInvokeMethod(Delegate.GetType());

					return _invokeMethod;
				}
			}

			public Overload(Delegate @delegate)
			{
				Delegate = @delegate;
			}
		}

		private readonly List<Overload> _overloads = new List<Overload>();

		internal IReadOnlyCollection<Overload> Overloads
		{
			get
			{
				return _overloads.AsReadOnly();
			}
		}

		internal MethodGroupExpression(Delegate overload)
		{
			AddOverload(overload);
		}

		internal void AddOverload(Delegate overload)
		{
			// remove any existing delegate with the exact same signature
			RemoveDelegateSignature(overload);
			_overloads.Add(new Overload(overload));
		}

		private void RemoveDelegateSignature(Delegate overload)
		{
			_overloads.RemoveAll(del => HasSameSignature(overload.Method, del.Delegate.Method));
		}

		private static bool HasSameSignature(MethodInfo method, MethodInfo other)
		{
			if (method.ReturnType != other.ReturnType)
				return false;

			var param = method.GetParameters();
			var oParam = other.GetParameters();
			if (param.Length != oParam.Length)
				return false;

			for (var i = 0; i < param.Length; i++)
			{
				var p = param[i];
				var q = oParam[i];
				if (p.ParameterType != q.ParameterType || p.HasDefaultValue != q.HasDefaultValue)
					return false;
			}

			return true;
		}

		/// <summary>
		/// The resolution process will find the best overload for the given arguments,
		/// which we then need to match to the correct delegate.
		/// </summary>
		internal Delegate FindUsedOverload(bool usedInvokeMethod, MethodData methodData)
		{
			foreach (var overload in _overloads)
			{
				if (usedInvokeMethod)
				{
					if (methodData.MethodBase == overload.InvokeMethod)
						return overload.Delegate;
				}
				else
				{
					if (methodData.MethodBase == overload.Method)
						return overload.Delegate;
				}
			}

			// this should never happen
			throw new InvalidOperationException("No overload matches the method");
		}
	}
}
