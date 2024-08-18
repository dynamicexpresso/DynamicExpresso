using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;

namespace DynamicExpresso.Reflection
{
	internal static class TypeUtils
	{
		public static bool IsNullableType(Type type)
		{
			return Nullable.GetUnderlyingType(type) != null;
		}

		public static bool IsDynamicType(Type type)
		{
			return typeof(IDynamicMetaObjectProvider).IsAssignableFrom(type);
		}

		public static Type GetNonNullableType(Type type)
		{
			return IsNullableType(type) ? type.GetGenericArguments()[0] : type;
		}

		public static string GetTypeName(Type type)
		{
			var baseType = GetNonNullableType(type);
			var s = baseType.Name;
			if (type != baseType) s += '?';
			return s;
		}

		public static bool IsNumericType(Type type)
		{
			return GetNumericTypeKind(type) != 0;
		}

		public static bool IsSignedIntegralType(Type type)
		{
			return GetNumericTypeKind(type) == 2;
		}

		public static bool IsUnsignedIntegralType(Type type)
		{
			return GetNumericTypeKind(type) == 3;
		}

		private static int GetNumericTypeKind(Type type)
		{
			type = GetNonNullableType(type);
			if (type.IsEnum) return 0;
			switch (Type.GetTypeCode(type))
			{
				case TypeCode.Char:
				case TypeCode.Single:
				case TypeCode.Double:
				case TypeCode.Decimal:
					return 1;
				case TypeCode.SByte:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
					return 2;
				case TypeCode.Byte:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
					return 3;
				default:
					return 0;
			}
		}

		public static bool IsCompatibleWith(Type source, Type target)
		{
			if (source == target)
			{
				return true;
			}

			if (target.IsGenericParameter)
			{
				return true;
			}

			if (!target.IsValueType)
			{
				return target.IsAssignableFrom(source);
			}
			var st = GetNonNullableType(source);
			var tt = GetNonNullableType(target);
			if (st != source && tt == target) return false;
			var sc = st.IsEnum ? TypeCode.Object : Type.GetTypeCode(st);
			var tc = tt.IsEnum ? TypeCode.Object : Type.GetTypeCode(tt);
			switch (sc)
			{
				case TypeCode.SByte:
					switch (tc)
					{
						case TypeCode.SByte:
						case TypeCode.Int16:
						case TypeCode.Int32:
						case TypeCode.Int64:
						case TypeCode.Single:
						case TypeCode.Double:
						case TypeCode.Decimal:
							return true;
					}
					break;
				case TypeCode.Byte:
					switch (tc)
					{
						case TypeCode.Byte:
						case TypeCode.Int16:
						case TypeCode.UInt16:
						case TypeCode.Int32:
						case TypeCode.UInt32:
						case TypeCode.Int64:
						case TypeCode.UInt64:
						case TypeCode.Single:
						case TypeCode.Double:
						case TypeCode.Decimal:
							return true;
					}
					break;
				case TypeCode.Int16:
					switch (tc)
					{
						case TypeCode.Int16:
						case TypeCode.Int32:
						case TypeCode.Int64:
						case TypeCode.Single:
						case TypeCode.Double:
						case TypeCode.Decimal:
							return true;
					}
					break;
				case TypeCode.UInt16:
					switch (tc)
					{
						case TypeCode.UInt16:
						case TypeCode.Int32:
						case TypeCode.UInt32:
						case TypeCode.Int64:
						case TypeCode.UInt64:
						case TypeCode.Single:
						case TypeCode.Double:
						case TypeCode.Decimal:
							return true;
					}
					break;
				case TypeCode.Int32:
					switch (tc)
					{
						case TypeCode.Int32:
						case TypeCode.Int64:
						case TypeCode.Single:
						case TypeCode.Double:
						case TypeCode.Decimal:
							return true;
					}
					break;
				case TypeCode.UInt32:
					switch (tc)
					{
						case TypeCode.UInt32:
						case TypeCode.Int64:
						case TypeCode.UInt64:
						case TypeCode.Single:
						case TypeCode.Double:
						case TypeCode.Decimal:
							return true;
					}
					break;
				case TypeCode.Int64:
					switch (tc)
					{
						case TypeCode.Int64:
						case TypeCode.Single:
						case TypeCode.Double:
						case TypeCode.Decimal:
							return true;
					}
					break;
				case TypeCode.UInt64:
					switch (tc)
					{
						case TypeCode.UInt64:
						case TypeCode.Single:
						case TypeCode.Double:
						case TypeCode.Decimal:
							return true;
					}
					break;
				case TypeCode.Single:
					switch (tc)
					{
						case TypeCode.Single:
						case TypeCode.Double:
							return true;
					}
					break;
				default:
					if (st == tt) return true;
					break;
			}
			return false;
		}

		// Return 1 if s -> t1 is a better conversion than s -> t2
		// Return -1 if s -> t2 is a better conversion than s -> t1
		// Return 0 if neither conversion is better
		public static int CompareConversions(Type s, Type t1, Type t2)
		{
			if (t1 == t2) return 0;
			if (s == t1) return 1;
			if (s == t2) return -1;

			var assignableT1 = t1.IsAssignableFrom(s);
			var assignableT2 = t2.IsAssignableFrom(s);
			if (assignableT1 && !assignableT2) return 1;
			if (assignableT2 && !assignableT1) return -1;

			var compatibleT1T2 = IsCompatibleWith(t1, t2);
			var compatibleT2T1 = IsCompatibleWith(t2, t1);
			if (compatibleT1T2 && !compatibleT2T1) return 1;
			if (compatibleT2T1 && !compatibleT1T2) return -1;

			if (IsSignedIntegralType(t1) && IsUnsignedIntegralType(t2)) return 1;
			if (IsSignedIntegralType(t2) && IsUnsignedIntegralType(t1)) return -1;

			return 0;
		}

		/// <summary>
		/// Returns null if <paramref name="t"/> is an Array type.  Needed because the <seealso cref="Microsoft.CSharp.RuntimeBinder.Binder"/> lookup methods fail with a <seealso cref="InvalidCastException"/> if the array type is used.
		/// Everything still miraculously works on the array if null is given for the type.
		/// </summary>
		/// <param name="t"></param>
		/// <returns></returns>
		public static Type RemoveArrayType(Type t)
		{
			if (t == null || t.IsArray)
			{
				return null;
			}
			return t;
		}

		// from http://stackoverflow.com/a/1075059/209727
		public static Type FindAssignableGenericType(Type givenType, Type constructedGenericType)
		{
			var genericTypeDefinition = constructedGenericType.GetGenericTypeDefinition();
			var interfaceTypes = givenType.GetInterfaces();

			foreach (var it in interfaceTypes)
			{
				if (it.IsGenericType && it.GetGenericTypeDefinition() == genericTypeDefinition)
				{
					return it;
				}
			}

			if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericTypeDefinition)
			{
				// the given type has the same generic type of the fully constructed generic type
				//  => check if the generic arguments are compatible (e.g. Nullable<int> and Nullable<DateTime>: int is not compatible with DateTime)
				var givenTypeGenericsArgs = givenType.GenericTypeArguments;
				var constructedGenericsArgs = constructedGenericType.GenericTypeArguments;
				if (givenTypeGenericsArgs.Zip(constructedGenericsArgs, (g, c) => TypeUtils.IsCompatibleWith(g, c)).Any(compatible => !compatible))
					return null;

				return givenType;
			}

			var baseType = givenType.BaseType;
			if (baseType == null)
				return null;

			return FindAssignableGenericType(baseType, genericTypeDefinition);
		}

		public static Type GetConcreteTypeForGenericMethod(Type type, List<Expression> promotedArgs, MethodData method)
		{
			if (type.IsGenericType)
			{
				//Generic<T> type
				var genericArguments = type.GetGenericArguments();
				var concreteTypeParameters = new Type[genericArguments.Length];

				for (var i = 0; i < genericArguments.Length; i++)
				{
					concreteTypeParameters[i] = GetConcreteTypeForGenericMethod(genericArguments[i], promotedArgs, method);
				}

				return type.GetGenericTypeDefinition().MakeGenericType(concreteTypeParameters);
			}
			else if (type.ContainsGenericParameters)
			{
				//T case
				//try finding an actual parameter for the generic
				for (var i = 0; i < promotedArgs.Count; i++)
				{
					if (method.Parameters[i].ParameterType == type)
					{
						return promotedArgs[i].Type;
					}
				}
			}

			return type;//already a concrete type
		}
	}
}
