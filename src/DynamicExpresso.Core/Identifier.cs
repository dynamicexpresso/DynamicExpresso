using System;
using System.Collections.Generic;
using System.Linq.Expressions;

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
	/// </summary>
	internal class MethodGroupExpression : Expression
	{
		private readonly List<Delegate> _overloads = new List<Delegate>();

		internal IReadOnlyCollection<Delegate> Overloads
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
			// don't register the same overload twice
			if (_overloads.IndexOf(overload) == -1)
				_overloads.Add(overload);
		}
	}
}
