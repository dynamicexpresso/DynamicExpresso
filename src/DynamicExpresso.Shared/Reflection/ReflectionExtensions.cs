using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

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
#if !WINDOWS_UWP
            if (type.IsSealed && type.IsAbstract && !type.IsGenericType && !type.IsNested)
#else
            if (type.GetTypeInfo().IsSealed && type.GetTypeInfo().IsAbstract && !type.GetTypeInfo().IsGenericType && !type.GetTypeInfo().IsNested)
#endif
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


    public static class UwpSyntacticReflectionExtensions
    {

#if WINDOWS_UWP
        public static TypeCode GetTypeCode(this Type type)
        {
            if (type == null) return TypeCode.Empty;

            TypeCode result;
            if (type.IsEnum()) type = Enum.GetUnderlyingType(type);

            if (typeCodeLookup.TryGetValue(type, out result)) return result;
            
            return TypeCode.Object;
        }

        static readonly Dictionary<Type, TypeCode> typeCodeLookup = new Dictionary<Type, TypeCode>
        {
            {typeof(bool), TypeCode.Boolean },
            {typeof(byte), TypeCode.Byte },
            {typeof(char), TypeCode.Char},
            {typeof(DateTime), TypeCode.DateTime},
            {typeof(decimal), TypeCode.Decimal},
            {typeof(double), TypeCode.Double },
            {typeof(short), TypeCode.Int16 },
            {typeof(int), TypeCode.Int32 },
            {typeof(long), TypeCode.Int64 },
            {typeof(object), TypeCode.Object},
            {typeof(sbyte), TypeCode.SByte },
            {typeof(float), TypeCode.Single },
            {typeof(string), TypeCode.String },
            {typeof(ushort), TypeCode.UInt16 },
            {typeof(uint), TypeCode.UInt32 },
            {typeof(ulong), TypeCode.UInt64 }
        };

        public static bool IsSealed(this Type type)
        {
            return type.GetTypeInfo().IsSealed;
        }

        public static bool IsAbstract(this Type type)
        {
            return type.GetTypeInfo().IsAbstract;
        }

        public static bool IsGenericType(this Type type)
        {
            return type.GetTypeInfo().IsGenericType;
        }

        public static bool IsEnum(this Type type)
        {
            return type.GetTypeInfo().IsEnum;
        }

        public static bool IsValueType(this Type type)
        {
            return type.GetTypeInfo().IsValueType;
        }

        public static bool IsInterface(this Type type)
        {
            return type.GetTypeInfo().IsInterface;
        }

        public static bool ContainsGenericParameters(this Type type)
        {
            return type.GetTypeInfo().ContainsGenericParameters;
        }

        public static Type BaseType(this Type type)
        {
            return type.GetTypeInfo().BaseType;
        }
#endif
    }



}
