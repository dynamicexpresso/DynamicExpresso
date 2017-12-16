using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DynamicExpresso.Reflection
{
	internal static class ReflectionExtensions
	{
		public static DelegateInfo GetDelegateInfo(Type delegateType, params string[] parametersNames)
		{
			MethodInfo method = delegateType.GetMethod("Invoke");
			if (method == null)
				throw new ArgumentException("The specified type is not a delegate");

			var delegateParameters = method.GetParameters();
			var parameters = new Parameter[delegateParameters.Length];

			bool useCustomNames = parametersNames != null && parametersNames.Length > 0;

			if (useCustomNames && parametersNames.Length != parameters.Length)
				throw new ArgumentException(string.Format("Provided parameters names doesn't match delegate parameters, {0} parameters expected.", parameters.Length));

			for (int i = 0; i < parameters.Length; i++)
			{
				var paramName = useCustomNames ? parametersNames[i] : delegateParameters[i].Name;
				var paramType = delegateParameters[i].ParameterType;

				parameters[i] = new Parameter(paramName, paramType);
			}

			return new DelegateInfo(method.ReturnType, parameters);
		}

		public static IEnumerable<MethodInfo> GetExtensionMethods(Type type)
		{
			if (type.IsSealed && type.IsAbstract && !type.IsGenericType && !type.IsNested)
			{
				var query = from method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
										where method.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false)
										select method;
				return query;
			}

			return Enumerable.Empty<MethodInfo>();
		}

		public class DelegateInfo
		{
			public DelegateInfo(Type returnType, Parameter[] parameters)
			{
				ReturnType = returnType;
				Parameters = parameters;
			}

			public Type ReturnType { get; private set; }
			public Parameter[] Parameters { get; private set; }
		}
	}
}
