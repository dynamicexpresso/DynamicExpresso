using System;
using System.Reflection;

namespace DynamicExpresso.Reflection
{
	internal class SimpleParameterInfo : ParameterInfo
	{
		public SimpleParameterInfo(Type parameterType)
		{
			ClassImpl = parameterType;
			DefaultValueImpl = null;
		}

		public override bool HasDefaultValue => false;

		public override string ToString()
		{
			return base.ToString();
		}
	}
}
