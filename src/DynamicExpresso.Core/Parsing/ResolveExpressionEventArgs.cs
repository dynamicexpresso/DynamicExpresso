using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;

namespace DynamicExpresso.Parsing
{
	public class ResolveExpressionEventArgs : EventArgs
	{
		private ResolveExpressionType _eventType;
		private Type _type;
		private Expression _instance;
		private string _name;
		private Expression[] _args;
		private MemberFilter _memberFilter;
		private BindingFlags _bindingFlags;
		private Expression _resolvedExpression;

		public ResolveExpressionType EventType
		{
			get { return _eventType; }
		}
		public Type Type
		{
			get { return _type; }
		}
		public Expression Instance
		{
			get { return _instance; }
		}
		public string Name
		{
			get { return _name; }
		}
		public IEnumerable<Expression> Arguments
		{
			get { return _args; }
		}
		public MemberFilter MemberFilter
		{
			get { return _memberFilter; }
		}
		public BindingFlags BindingFlags
		{
			get { return _bindingFlags; }
		}
		public bool IsResolved
		{
			get { return _resolvedExpression != null; }
		}
		internal Expression ResolvedExpression
		{
			get { return _resolvedExpression; }
		}

		public ResolveExpressionEventArgs(ResolveExpressionType eventType, Type type, Expression instance, string name, Expression[] args, MemberFilter memberFilter, BindingFlags bindingFlags)
		{
			_eventType = eventType;
			_type = type;
			_instance = instance;
			_name = name;
			_args = args;
			_memberFilter = memberFilter;
			_bindingFlags = bindingFlags;
		}

		public void Resolve(Expression expression)
		{
			if (IsResolved)
			{ 
				throw new InvalidOperationException(string.Format(
					ErrorMessages.ResolveExpressionTwice, 
					_name, 
					_type, 
					_instance, 
					_resolvedExpression));
			}
			_resolvedExpression = expression;
		}
	}
}
