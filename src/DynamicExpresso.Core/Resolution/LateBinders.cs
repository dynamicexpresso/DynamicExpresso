using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using DynamicExpresso.Reflection;
using Microsoft.CSharp.RuntimeBinder;

namespace DynamicExpresso.Resolution
{
	internal class LateGetMemberCallSiteBinder : CallSiteBinder
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
	}

	/// <summary>
	/// Binds to a method invocation of an instance as late as possible.  This allows the use of anonymous types on dynamic values.
	/// </summary>
	internal class LateInvokeMethodCallSiteBinder : CallSiteBinder
	{
		private readonly string _methodName;

		public LateInvokeMethodCallSiteBinder(string methodName)
		{
			_methodName = methodName;
		}

		public override Expression Bind(object[] args, ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel)
		{
			var binderM = Binder.InvokeMember(
				CSharpBinderFlags.None,
				_methodName,
				null,
				TypeUtils.RemoveArrayType(args[0]?.GetType()),
				parameters.Select(x => CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null))
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
