using System;
using System.Globalization;
using System.Reflection;

namespace DynamicExpresso.Reflection
{
	/// <summary>
	/// A simple implementation of <see cref="MethodBase"/> that only provides the method signature (ie. the parameter types).
	/// </summary>
	internal class SimpleMethodSignature : MethodBase
	{
		private class SimpleParameterInfo : ParameterInfo
		{
			public SimpleParameterInfo(Type parameterType)
			{
				ClassImpl = parameterType;
				DefaultValueImpl = null;
			}

			public override bool HasDefaultValue => false;
		}

		public override MethodAttributes Attributes { get; } = MethodAttributes.Public;
		public override MemberTypes MemberType { get; } = MemberTypes.Method;

		private readonly ParameterInfo[] _parameterInfos;
		public SimpleMethodSignature(params Type[] parameterTypes)
		{
			_parameterInfos = new ParameterInfo[parameterTypes.Length];
			for (var i = 0; i < parameterTypes.Length; i++)
			{
				_parameterInfos[i] = new SimpleParameterInfo(parameterTypes[i]);
			}
		}

		public override ParameterInfo[] GetParameters()
		{
			return _parameterInfos;
		}

		public override RuntimeMethodHandle MethodHandle => throw new NotImplementedException();
		public override string Name => throw new NotImplementedException();
		public override Type DeclaringType => throw new NotImplementedException();
		public override Type ReflectedType => throw new NotImplementedException();

		public override object[] GetCustomAttributes(bool inherit)
		{
			throw new NotImplementedException();
		}

		public override object[] GetCustomAttributes(Type attributeType, bool inherit)
		{
			throw new NotImplementedException();
		}

		public override MethodImplAttributes GetMethodImplementationFlags()
		{
			throw new NotImplementedException();
		}

		public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		public override bool IsDefined(Type attributeType, bool inherit)
		{
			throw new NotImplementedException();
		}
	}
}
