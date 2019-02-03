using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DynamicExpresso.Exceptions;
using DynamicExpresso.Resources;

namespace DynamicExpresso.Parsing
{
	internal class Expressions
	{
		private readonly BindingFlags _bindingCase;
		private readonly MemberFilter _memberFilterCase;

		public Expressions(ParserArguments arguments)
		{
			_bindingCase = arguments.Settings.CaseInsensitive ? BindingFlags.IgnoreCase : BindingFlags.Default;
			_memberFilterCase = arguments.Settings.CaseInsensitive ? Type.FilterNameIgnoreCase : Type.FilterName;
		}

		public static Expression CreateLiteral(object value)
		{
			var expr = Expression.Constant(value);

			return expr;
		}

		public static Expression GenerateConditional(Expression test, Expression expr1, Expression expr2, int errorPos)
		{
			if (test.Type != typeof(bool))
				throw ParseException.Create(errorPos, ErrorMessages.FirstExprMustBeBool);
			if (expr1.Type != expr2.Type)
			{
				var expr1As2 = expr2 != ParserConstants.NullLiteralExpression ? PromoteExpression(expr1, expr2.Type, true) : null;
				var expr2As1 = expr1 != ParserConstants.NullLiteralExpression ? PromoteExpression(expr2, expr1.Type, true) : null;
				if (expr1As2 != null && expr2As1 == null)
				{
					expr1 = expr1As2;
				}
				else if (expr2As1 != null && expr1As2 == null)
				{
					expr2 = expr2As1;
				}
				else
				{
					var type1 = expr1 != ParserConstants.NullLiteralExpression ? expr1.Type.Name : "null";
					var type2 = expr2 != ParserConstants.NullLiteralExpression ? expr2.Type.Name : "null";
					if (expr1As2 != null)
						throw ParseException.Create(errorPos, ErrorMessages.BothTypesConvertToOther, type1, type2);

					throw ParseException.Create(errorPos, ErrorMessages.NeitherTypeConvertsToOther, type1, type2);
				}
			}
			return Expression.Condition(test, expr1, expr2);
		}

		public static Expression GenerateConversion(Expression expr, Type type, int errorPos)
		{
			var exprType = expr.Type;
			if (exprType == type)
			{
				return expr;
			}

			//if (exprType.IsValueType && type.IsValueType)
			//{
			//	if ((IsNullableType(exprType) || IsNullableType(type)) &&
			//			GetNonNullableType(exprType) == GetNonNullableType(type))
			//		return Expression.Convert(expr, type);
			//	if ((IsNumericType(exprType) || IsEnumType(exprType)) &&
			//			(IsNumericType(type)) || IsEnumType(type))
			//		return Expression.ConvertChecked(expr, type);
			//}

			//if (exprType.IsAssignableFrom(type) || type.IsAssignableFrom(exprType) ||
			//				exprType.IsInterface || type.IsInterface)
			//{
			//	return Expression.Convert(expr, type);
			//}

			try
			{
				return Expression.ConvertChecked(expr, type);
			}
			catch (InvalidOperationException)
			{
				throw ParseException.Create(errorPos, ErrorMessages.CannotConvertValue,
					Types.GetTypeName(exprType), Types.GetTypeName(type));
			}
		}

		public static Expression GenerateDynamicProperty(Type type, Expression instance, string propertyOrFieldName)
		{
#if NETSTANDARD2_0
			throw new NotImplementedException("Dynamic types are not supported in .NET Standard build");
#else
			var binder = Microsoft.CSharp.RuntimeBinder.Binder.GetMember(
				Microsoft.CSharp.RuntimeBinder.CSharpBinderFlags.None,
				propertyOrFieldName,
				type,
				new[] { Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo.Create(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfoFlags.None, null) }
				);

			return Expression.Dynamic(binder, typeof(object), instance);
#endif
		}

		public static Expression GenerateDynamicMethodInvocation(Type type, Expression instance, string methodName, Expression[] args)
		{
#if NETSTANDARD2_0
			throw new NotImplementedException("Dynamic types are not supported in .NET Standard build");
#else
			var argsDynamic = args.ToList();
			argsDynamic.Insert(0, instance);
			var binderM = Microsoft.CSharp.RuntimeBinder.Binder.InvokeMember(
				Microsoft.CSharp.RuntimeBinder.CSharpBinderFlags.None,
				methodName,
				null,
				type,
				argsDynamic.Select(x => Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo.Create(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfoFlags.None, null))
				);

			return Expression.Dynamic(binderM, typeof(object), argsDynamic);
#endif
		}

		public static bool IsDynamicExpression(Expression instance)
		{
			return instance != null && instance.NodeType == ExpressionType.Dynamic;
		}

		public void CheckAndPromoteOperand(Type signatures, ref Expression expr)
		{
			var args = new[] { expr };

			args = PrepareOperandArguments(signatures, args);

			expr = args[0];
		}

		public void CheckAndPromoteOperands(Type signatures, ref Expression left, ref Expression right)
		{
			var args = new[] { left, right };

			args = PrepareOperandArguments(signatures, args);

			left = args[0];
			right = args[1];
		}

		private Expression[] PrepareOperandArguments(Type signatures, Expression[] args)
		{
			var applicableMethods = FindMethods(signatures, "F", false, args);
			if (applicableMethods.Length == 1)
				return applicableMethods[0].PromotedParameters;

			return args;
		}

		private MemberInfo FindPropertyOrField(Type type, string memberName, bool staticAccess)
		{
			var flags = BindingFlags.Public | BindingFlags.DeclaredOnly |
					(staticAccess ? BindingFlags.Static : BindingFlags.Instance) | _bindingCase;

			foreach (var t in Types.SelfAndBaseTypes(type))
			{
				var members = t.FindMembers(MemberTypes.Property | MemberTypes.Field, flags, _memberFilterCase, memberName);
				if (members.Length != 0)
					return members[0];
			}
			return null;
		}

		public Expression GeneratePropertyOrFieldExpression(Type type, Expression instance, int errorPos, string propertyOrFieldName)
		{
			var member = FindPropertyOrField(type, propertyOrFieldName, instance == null);
			if (member != null)
			{
				return member is PropertyInfo ?
						Expression.Property(instance, (PropertyInfo)member) :
						Expression.Field(instance, (FieldInfo)member);
			}

			if (Types.IsDynamicType(type) || Expressions.IsDynamicExpression(instance))
				return Expressions.GenerateDynamicProperty(type, instance, propertyOrFieldName);

			throw ParseException.Create(errorPos, ErrorMessages.UnknownPropertyOrField, propertyOrFieldName, Types.GetTypeName(type));
		}

		public bool PrepareDelegateInvoke(Type type, ref Expression[] args)
		{
			var applicableMethods = FindMethods(type, "Invoke", false, args);
			if (applicableMethods.Length != 1)
				return false;

			args = applicableMethods[0].PromotedParameters;

			return true;
		}

		public MethodData[] FindMethods(Type type, string methodName, bool staticAccess, Expression[] args)
		{
			//var exactMethod = type.GetMethod(methodName, args.Select(p => p.Type).ToArray());
			//if (exactMethod != null)
			//{
			//	return new MethodData[] { new MethodData(){ MethodBase = exactMethod, Parameters = exactMethod.GetParameters(), PromotedParameters = args} };
			//}

			var flags = BindingFlags.Public | BindingFlags.DeclaredOnly |
					(staticAccess ? BindingFlags.Static : BindingFlags.Instance) | _bindingCase;
			foreach (var t in Types.SelfAndBaseTypes(type))
			{
				var members = t.FindMembers(MemberTypes.Method, flags, _memberFilterCase, methodName);
				var applicableMethods = FindBestMethod(members.Cast<MethodBase>(), args);

				if (applicableMethods.Length > 0)
					return applicableMethods;
			}

			return new MethodData[0];
		}

		public static MethodData[] FindIndexer(Type type, Expression[] args)
		{
			foreach (var t in Types.SelfAndBaseTypes(type))
			{
				MemberInfo[] members = t.GetDefaultMembers();
				if (members.Length != 0)
				{
					IEnumerable<MethodData> methods = members.
							OfType<PropertyInfo>().
							Select(p => (MethodData) new IndexerData(p));

					var applicableMethods = FindBestMethod(methods, args);
					if (applicableMethods.Length > 0)
						return applicableMethods;
				}
			}

			return new MethodData[0];
		}

		public static MethodData[] FindBestMethod(IEnumerable<MethodBase> methods, Expression[] args)
		{
			return FindBestMethod(methods.Select(MethodData.Gen), args);
		}

		private static MethodData[] FindBestMethod(IEnumerable<MethodData> methods, Expression[] args)
		{
			var applicable = methods.
					Where(m => CheckIfMethodIsApplicableAndPrepareIt(m, args)).
					ToArray();
			if (applicable.Length > 1)
			{
				return applicable.
						Where(m => applicable.All(n => m == n || MethodHasPriority(args, m, n))).
						ToArray();
			}

			return applicable;
		}

		private static bool CheckIfMethodIsApplicableAndPrepareIt(MethodData method, Expression[] args)
		{
			if (method.Parameters.Count(y => !y.HasDefaultValue) > args.Length)
				return false;

			var promotedArgs = new List<Expression>();
			var declaredWorkingParameters = 0;

			Type paramsArrayTypeFound = null;
			List<Expression> paramsArrayPromotedArgument = null;

			foreach (var currentArgument in args)
			{
				Type parameterType;

				if (paramsArrayTypeFound != null)
				{
					parameterType = paramsArrayTypeFound;
				}
				else
				{
					if (declaredWorkingParameters >= method.Parameters.Length)
					{
						return false;
					}

					var parameterDeclaration = method.Parameters[declaredWorkingParameters];
					if (parameterDeclaration.IsOut)
					{
						return false;
					}

					parameterType = parameterDeclaration.ParameterType;

					if (HasParamsArrayType(parameterDeclaration))
					{
						paramsArrayTypeFound = parameterType;
					}

					declaredWorkingParameters++;
				}

				if (paramsArrayPromotedArgument == null)
				{
					if (parameterType.IsGenericParameter)
					{
						promotedArgs.Add(currentArgument);
						continue;
					}

					var promoted = PromoteExpression(currentArgument, parameterType, true);
					if (promoted != null)
					{
						promotedArgs.Add(promoted);
						continue;
					}
				}

				if (paramsArrayTypeFound != null)
				{
					var promoted = PromoteExpression(currentArgument, paramsArrayTypeFound.GetElementType(), true);
					if (promoted != null)
					{
						paramsArrayPromotedArgument = paramsArrayPromotedArgument ?? new List<Expression>();
						paramsArrayPromotedArgument.Add(promoted);
						continue;
					}
				}

				return false;
			}

			if (paramsArrayPromotedArgument != null)
			{
				method.HasParamsArray = true;
				var paramsArrayElementType = paramsArrayTypeFound.GetElementType();
				if (paramsArrayElementType == null)
					throw new Exception("Type is not an array, element not found");
				promotedArgs.Add(Expression.NewArrayInit(paramsArrayElementType, paramsArrayPromotedArgument));
			}

			// Add default params, if needed.
			promotedArgs.AddRange(method.Parameters.Skip(promotedArgs.Count).Select(x => Expression.Constant(x.DefaultValue, x.ParameterType)));

			method.PromotedParameters = promotedArgs.ToArray();

			if (method.MethodBase != null && method.MethodBase.IsGenericMethodDefinition &&
					method.MethodBase is MethodInfo)
			{
				var methodInfo = (MethodInfo)method.MethodBase;

				var actualGenericArgs = ExtractActualGenericArguments(
					method.Parameters.Select(p => p.ParameterType).ToArray(),
					method.PromotedParameters.Select(p => p.Type).ToArray());

				var genericArgs = methodInfo.GetGenericArguments()
					.Select(p => actualGenericArgs[p.Name])
					.ToArray();

				method.MethodBase = methodInfo.MakeGenericMethod(genericArgs);
			}

			return true;
		}

		private static Dictionary<string, Type> ExtractActualGenericArguments(
			Type[] methodGenericParameters,
			Type[] methodActualParameters)
		{
			var extractedGenericTypes = new Dictionary<string, Type>();

			for (var i = 0; i < methodGenericParameters.Length; i++)
			{
				var requestedType = methodGenericParameters[i];
				var actualType = methodActualParameters[i];

				if (requestedType.IsGenericParameter)
				{
					extractedGenericTypes[requestedType.Name] = actualType;
				}
				else if (requestedType.ContainsGenericParameters)
				{
					var innerGenericTypes = ExtractActualGenericArguments(requestedType.GetGenericArguments(), actualType.GetGenericArguments());

					foreach (var innerGenericType in innerGenericTypes)
						extractedGenericTypes[innerGenericType.Key] = innerGenericType.Value;
				}
			}

			return extractedGenericTypes;
		}

		public static Expression PromoteExpression(Expression expr, Type type, bool exact)
		{
			if (expr.Type == type) return expr;
			if (expr is ConstantExpression)
			{
				var ce = (ConstantExpression)expr;
				if (ce == ParserConstants.NullLiteralExpression)
				{
					if (!type.IsValueType || Types.IsNullableType(type))
						return Expression.Constant(null, type);
				}
			}

			if (type.IsGenericType && !Types.IsNumericType(type))
			{
				var genericType = FindAssignableGenericType(expr.Type, type.GetGenericTypeDefinition());
				if (genericType != null)
					return Expression.Convert(expr, genericType);
			}

			if (Types.IsCompatibleWith(expr.Type, type))
			{
				if (type.IsValueType || exact)
				{
					return Expression.Convert(expr, type);
				}
				return expr;
			}

			return null;
		}

		public static bool IsWritable(Expression expression)
		{
			switch (expression.NodeType)
			{
				case ExpressionType.Index:
					PropertyInfo indexer = ((IndexExpression)expression).Indexer;
					return indexer == null || indexer.CanWrite;
				case ExpressionType.MemberAccess:
					MemberInfo member = ((MemberExpression)expression).Member;
					var prop = member as PropertyInfo;
					if (prop != null)
						return prop.CanWrite;
					else
					{
						var field = (FieldInfo)member;
						return !(field.IsInitOnly || field.IsLiteral);
					}
				case ExpressionType.Parameter:
					return true;
			}

			return false;
		}

		// from http://stackoverflow.com/a/1075059/209727
		private static Type FindAssignableGenericType(Type givenType, Type genericTypeDefinition)
		{
			var interfaceTypes = givenType.GetInterfaces();

			foreach (var it in interfaceTypes)
			{
				if (it.IsGenericType && it.GetGenericTypeDefinition() == genericTypeDefinition)
				{
					return it;
				}
			}

			if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericTypeDefinition)
				return givenType;

			var baseType = givenType.BaseType;
			if (baseType == null)
				return null;

			return FindAssignableGenericType(baseType, genericTypeDefinition);
		}

		private static bool HasParamsArrayType(ParameterInfo parameterInfo)
		{
			return parameterInfo.IsDefined(typeof(ParamArrayAttribute), false);
		}

		private static Type GetParameterType(ParameterInfo parameterInfo)
		{
			var isParamsArray = HasParamsArrayType(parameterInfo);
			var type = isParamsArray
				? parameterInfo.ParameterType.GetElementType()
				: parameterInfo.ParameterType;
			return type;
		}

		private static bool MethodHasPriority(Expression[] args, MethodData method, MethodData otherMethod)
		{
			if (method.HasParamsArray == false && otherMethod.HasParamsArray)
				return true;
			if (method.HasParamsArray && otherMethod.HasParamsArray == false)
				return false;

			//if (m1.Parameters.Length > m2.Parameters.Length)
			//	return true;
			//else if (m1.Parameters.Length < m2.Parameters.Length)
			//	return false;

			var better = false;
			for (var i = 0; i < args.Length; i++)
			{
				var methodParam = method.Parameters[i];
				var otherMethodParam = otherMethod.Parameters[i];
				var methodParamType = GetParameterType(methodParam);
				var otherMethodParamType = GetParameterType(otherMethodParam);
				var c = Types.CompareConversions(args[i].Type, methodParamType, otherMethodParamType);
				if (c < 0)
					return false;
				if (c > 0)
					better = true;
				if (HasParamsArrayType(methodParam) || HasParamsArrayType(otherMethodParam))
					break;
			}
			return better;
		}

		public static Expression GenerateEqual(Expression left, Expression right)
		{
			return Expression.Equal(left, right);
		}

		public static Expression GenerateNotEqual(Expression left, Expression right)
		{
			return Expression.NotEqual(left, right);
		}

		public static Expression GenerateGreaterThan(Expression left, Expression right)
		{
			if (left.Type == typeof(string))
			{
				return Expression.GreaterThan(
						GenerateStaticMethodCall("Compare", left, right),
						Expression.Constant(0)
				);
			}
			return Expression.GreaterThan(left, right);
		}

		public static Expression GenerateGreaterThanEqual(Expression left, Expression right)
		{
			if (left.Type == typeof(string))
			{
				return Expression.GreaterThanOrEqual(
						GenerateStaticMethodCall("Compare", left, right),
						Expression.Constant(0)
				);
			}
			return Expression.GreaterThanOrEqual(left, right);
		}

		public static Expression GenerateLessThan(Expression left, Expression right)
		{
			if (left.Type == typeof(string))
			{
				return Expression.LessThan(
						GenerateStaticMethodCall("Compare", left, right),
						Expression.Constant(0)
				);
			}
			return Expression.LessThan(left, right);
		}

		public static Expression GenerateLessThanEqual(Expression left, Expression right)
		{
			if (left.Type == typeof(string))
			{
				return Expression.LessThanOrEqual(
						GenerateStaticMethodCall("Compare", left, right),
						Expression.Constant(0)
				);
			}
			return Expression.LessThanOrEqual(left, right);
		}

		public static Expression GenerateAdd(Expression left, Expression right)
		{
			return Expression.Add(left, right);
		}

		public static Expression GenerateSubtract(Expression left, Expression right)
		{
			return Expression.Subtract(left, right);
		}

		public static Expression GenerateStringConcat(Expression left, Expression right)
		{
			var concatMethod = typeof(string).GetMethod("Concat", new[] { typeof(object), typeof(object) });
			if (concatMethod == null)
				throw new Exception("String concat method not found");

			var rightObj =
				right.Type.IsValueType
				? Expression.ConvertChecked(right, typeof(object))
				: right;
			var leftObj =
				left.Type.IsValueType
				? Expression.ConvertChecked(left, typeof(object))
				: left;

			return Expression.Call(
					null,
					concatMethod,
					new[] { leftObj, rightObj });
		}

		private static MethodInfo GetStaticMethod(string methodName, Expression left, Expression right)
		{
			return left.Type.GetMethod(methodName, new[] { left.Type, right.Type });
		}

		private static Expression GenerateStaticMethodCall(string methodName, Expression left, Expression right)
		{
			return Expression.Call(null, GetStaticMethod(methodName, left, right), new[] { left, right });
		}

		internal class MethodData
		{
			public MethodBase MethodBase;
			public ParameterInfo[] Parameters;
			public Expression[] PromotedParameters;
			public bool HasParamsArray;

			public static MethodData Gen(MethodBase method)
			{
				return new MethodData
				{
					MethodBase = method,
					Parameters = method.GetParameters()
				};
			}
		}

		internal class IndexerData : MethodData
		{
			public readonly PropertyInfo Indexer;

			public IndexerData(PropertyInfo indexer)
			{
				Indexer = indexer;

				var method = indexer.GetGetMethod();
				if (method != null)
				{
					Parameters = method.GetParameters();
				}
				else
				{
					method = indexer.GetSetMethod();
					Parameters = RemoveLast(method.GetParameters());
				}
			}

			private static T[] RemoveLast<T>(T[] array)
			{
				T[] result = new T[array.Length - 1];
				Array.Copy(array, 0, result, 0, result.Length);
				return result;
			}
		}
	}
}
