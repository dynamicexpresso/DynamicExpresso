using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using DynamicExpresso.Reflection;
using Microsoft.CSharp.RuntimeBinder;

namespace DynamicExpresso.Resolution
{
	internal interface IConvertibleToWritableBinder
	{
		CallSiteBinder ToWritableBinder();
	}

	internal class LateGetMemberCallSiteBinder : CallSiteBinder, IConvertibleToWritableBinder
	{
		private readonly string _propertyOrFieldName;

		public LateGetMemberCallSiteBinder(string propertyOrFieldName)
		{
			_propertyOrFieldName = propertyOrFieldName;
		}

		public override Expression Bind(object[] args, ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel)
		{
			var binder = Binder.GetMember(
				CSharpBinderFlags.None,
				_propertyOrFieldName,
				TypeUtils.RemoveArrayType(args[0]?.GetType()),
				new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }
			);
			return binder.Bind(args, parameters, returnLabel);
		}

		public CallSiteBinder ToWritableBinder()
		{
			return new LateSetMemberCallSiteBinder(_propertyOrFieldName);
		}
	}

	internal class LateSetMemberCallSiteBinder : CallSiteBinder
	{
		private readonly string _propertyOrFieldName;

		public LateSetMemberCallSiteBinder(string propertyOrFieldName)
		{
			_propertyOrFieldName = propertyOrFieldName;
		}

		public override Expression Bind(object[] args, ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel)
		{
			var binder = Binder.SetMember(
				CSharpBinderFlags.None,
				_propertyOrFieldName,
				TypeUtils.RemoveArrayType(args[0]?.GetType()),
				new[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null), // instruct the compiler that we already know the type of the value
				}
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

		public LateInvokeMethodCallSiteBinder(string methodName, bool isStatic)
		{
			_methodName = methodName;
			_isStatic = isStatic;
		}

		public override Expression Bind(object[] args, ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel)
		{
			// if the method is static, the first argument is the type containing the method,
			// otherwise it's the instance on which the method is called
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
			// the first argument is the delegate to invoke
			var _delegate = (Delegate)args[0];
			var argumentInfo = parameters.Select(x => CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)).ToArray();

			// instruct the compiler that we already know the delegate's type
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
	internal class LateInvokeIndexCallSiteBinder : CallSiteBinder
	{
		public override Expression Bind(object[] args, ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel)
		{
			var binder = Binder.GetIndex(
				CSharpBinderFlags.None,
				TypeUtils.RemoveArrayType(args[0]?.GetType()),
				parameters.Select(x => CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null))
			);
			return binder.Bind(args, parameters, returnLabel);
		}
	}
}
