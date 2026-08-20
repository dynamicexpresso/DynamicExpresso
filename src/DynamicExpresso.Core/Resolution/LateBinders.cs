using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using DynamicExpresso.Exceptions;
using DynamicExpresso.Reflection;
using Microsoft.CSharp.RuntimeBinder;
using MemberInfo = System.Reflection.MemberInfo;

namespace DynamicExpresso.Resolution
{
	internal interface IConvertibleToWritableBinder
	{
		CallSiteBinder ToWritableBinder();
	}

	internal class LateGetMemberCallSiteBinder : CallSiteBinder, IConvertibleToWritableBinder
	{
		private readonly string _propertyOrFieldName;
		private readonly bool _reflectionEnabled;

		public LateGetMemberCallSiteBinder(string propertyOrFieldName, bool reflectionEnabled)
		{
			_propertyOrFieldName = propertyOrFieldName;
			_reflectionEnabled = reflectionEnabled;
		}

		public override Expression Bind(object[] args, ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel)
		{
			// there's only one argument: the instance on which the member is accessed
			LateBindingSecurity.ValidateReflectionTarget(args[0], _propertyOrFieldName, _reflectionEnabled, true);
			var binder = Binder.GetMember(
				CSharpBinderFlags.None,
				_propertyOrFieldName,
				TypeUtils.RemoveArrayType(args[0]?.GetType()),
				parameters.Select(x => CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null))
			);
			return binder.Bind(args, parameters, returnLabel);
		}

		public CallSiteBinder ToWritableBinder()
		{
			return new LateSetMemberCallSiteBinder(_propertyOrFieldName, _reflectionEnabled);
		}
	}

	internal class LateSetMemberCallSiteBinder : CallSiteBinder
	{
		private readonly string _propertyOrFieldName;
		private readonly bool _reflectionEnabled;

		public LateSetMemberCallSiteBinder(string propertyOrFieldName, bool reflectionEnabled)
		{
			_propertyOrFieldName = propertyOrFieldName;
			_reflectionEnabled = reflectionEnabled;
		}

		public override Expression Bind(object[] args, ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel)
		{
			// there are two arguments: the instance on which the member is set and the value to set
			LateBindingSecurity.ValidateReflectionTarget(args[0], _propertyOrFieldName, _reflectionEnabled, false);
			var binder = Binder.SetMember(
				CSharpBinderFlags.None,
				_propertyOrFieldName,
				TypeUtils.RemoveArrayType(args[0]?.GetType()),
				parameters.Select(x => CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null))
			);
			return binder.Bind(args, parameters, returnLabel);
		}
	}

	/// <summary>
	/// Binds to a method invocation of an instance as late as possible.  This allows the use of anonymous types on dynamic values.
	/// </summary>
	internal class LateInvokeMethodCallSiteBinder : CallSiteBinder
	{
		private readonly string _methodName;
		private readonly bool _isStatic;
		private readonly bool _reflectionEnabled;

		public LateInvokeMethodCallSiteBinder(string methodName, bool isStatic, bool reflectionEnabled)
		{
			_methodName = methodName;
			_isStatic = isStatic;
			_reflectionEnabled = reflectionEnabled;
		}

		public override Expression Bind(object[] args, ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel)
		{
			// if the method is static, the first argument is the type containing the method,
			// otherwise it's the instance on which the method is called
			if (!_isStatic)
				LateBindingSecurity.ValidateReflectionTarget(args[0], _methodName, _reflectionEnabled, false);

			var context = _isStatic ? (Type)args[0] : args[0]?.GetType();
			var argumentInfo = parameters.Select(x => CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)).ToArray();
			if (_isStatic)
			{
				// instruct the compiler that we already know the containing type of the method
				argumentInfo[0] = CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.IsStaticType, null);
			}

			var binderM = Binder.InvokeMember(
				CSharpBinderFlags.None,
				_methodName,
				null,
				TypeUtils.RemoveArrayType(context),
				argumentInfo
			);
			return binderM.Bind(args, parameters, returnLabel);
		}
	}

	internal static class LateBindingSecurity
	{
		public static void ValidateReflectionTarget(object target, string memberName, bool reflectionEnabled, bool allowNameMember)
		{
			if (!reflectionEnabled
				&& (target is Type || target is MemberInfo)
				&& (!allowNameMember || memberName != "Name"))
			{
				throw new ReflectionNotAllowedException();
			}
		}
	}

	/// <summary>
	/// Binds to a delegate invocation as late as possible.  This allows the use of delegates with dynamic arguments.
	/// </summary>
	internal class LateInvokeDelegateCallSiteBinder : CallSiteBinder
	{
		public LateInvokeDelegateCallSiteBinder()
		{
		}

		public override Expression Bind(object[] args, ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel)
		{
			var argumentInfo = parameters.Select(x => CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)).ToArray();

			// the first argument is the delegate to invoke: instruct the compiler that we already know its type
			argumentInfo[0] = CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null);

			var binderM = Binder.Invoke(
				CSharpBinderFlags.None,
				null,
				argumentInfo
			);
			return binderM.Bind(args, parameters, returnLabel);
		}
	}

	/// <summary>
	/// Binds to an items invocation of an instance as late as possible.  This allows the use of anonymous types on dynamic values.
	/// </summary>
	internal class LateGetIndexCallSiteBinder : CallSiteBinder, IConvertibleToWritableBinder
	{
		public override Expression Bind(object[] args, ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel)
		{
			// there are two arguments: the instance on which the member is set and the value of the indexer
			var binder = Binder.GetIndex(
				CSharpBinderFlags.None,
				TypeUtils.RemoveArrayType(args[0]?.GetType()),
				parameters.Select(x => CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null))
			);
			return binder.Bind(args, parameters, returnLabel);
		}

		public CallSiteBinder ToWritableBinder()
		{
			return new LateSetIndexCallSiteBinder();
		}
	}

	internal class LateSetIndexCallSiteBinder : CallSiteBinder
	{
		public override Expression Bind(object[] args, ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel)
		{
			// there are three arguments: the instance on which the member is set, the value of the indexer, and the value to set
			var binder = Binder.SetIndex(
				CSharpBinderFlags.None,
				TypeUtils.RemoveArrayType(args[0]?.GetType()),
				parameters.Select(x => CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null))
			);
			return binder.Bind(args, parameters, returnLabel);
		}
	}
}
