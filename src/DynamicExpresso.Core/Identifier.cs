using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

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
			// remove any existing delegate with the exact same signature
			RemoveDelegateSignature(overload);
			_overloads.Add(overload);
		}

		private void RemoveDelegateSignature(Delegate overload)
		{
			_overloads.RemoveAll(del => HasSameSignature(overload.Method, del.Method));
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
	}
}
