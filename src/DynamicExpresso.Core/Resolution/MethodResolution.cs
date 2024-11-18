using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security;
using DynamicExpresso.Exceptions;
using DynamicExpresso.Parsing;
using DynamicExpresso.Reflection;
using DynamicExpresso.Resources;

namespace DynamicExpresso.Resolution
{
	internal static class MethodResolution
	{
		public static MethodData[] FindBestMethod(IEnumerable<MethodBase> methods, Expression[] args)
		{
			return FindBestMethod(methods.Select(MethodData.Gen), args);
		}

		public static MethodData[] FindBestMethod(IEnumerable<MethodData> methods, Expression[] args)
		{
			var applicable = methods.
				Where(m => CheckIfMethodIsApplicableAndPrepareIt(m, args)).
				ToArray();
			if (applicable.Length > 1)
			{
				var bestCandidates = applicable
					.Where(m => applicable.All(n => m == n || MethodHasPriority(args, m, n)))
					.ToArray();

				// bestCandidates.Length == 0 means that no applicable method has priority
				// we don't return bestCandidates to prevent callers from thinking no method was found
				if (bestCandidates.Length > 0)
					return bestCandidates;
			}

			return applicable;
		}

		public static bool CheckIfMethodIsApplicableAndPrepareIt(MethodData method, Expression[] args)
		{
			if (method.Parameters.Count(y => !y.HasDefaultValue && !ReflectionExtensions.HasParamsArrayType(y)) > args.Length)
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

					if (ReflectionExtensions.HasParamsArrayType(parameterDeclaration))
					{
						paramsArrayTypeFound = parameterType;
					}

					declaredWorkingParameters++;
				}

				if (paramsArrayPromotedArgument == null && (paramsArrayTypeFound == null || args.Length == method.Parameters.Length))
				{
					if (parameterType.IsGenericParameter)
					{
						// an interpreter expression can only be matched to a parameter of type Func
						if (currentArgument is InterpreterExpression)
							return false;

						promotedArgs.Add(currentArgument);
						continue;
					}

					var promoted = ExpressionUtils.PromoteExpression(currentArgument, parameterType);
					if (promoted != null)
					{
						promotedArgs.Add(promoted);
						continue;
					}
				}

				if (paramsArrayTypeFound != null)
				{
					var paramsArrayElementType = paramsArrayTypeFound.GetElementType();
					if (paramsArrayElementType.IsGenericParameter)
					{
						paramsArrayPromotedArgument = paramsArrayPromotedArgument ?? new List<Expression>();
						paramsArrayPromotedArgument.Add(currentArgument);
						continue;
					}

					var promoted = ExpressionUtils.PromoteExpression(currentArgument, paramsArrayElementType);
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
					throw ParseException.Create(-1, ErrorMessages.ParamsArrayTypeNotAnArray);

				if (paramsArrayElementType.IsGenericParameter)
				{
					var actualTypes = paramsArrayPromotedArgument.Select(_ => _.Type).Distinct().ToArray();
					if (actualTypes.Length != 1)
						throw ParseException.Create(-1, ErrorMessages.MethodTypeParametersCantBeInferred, method.MethodBase);

					paramsArrayElementType = actualTypes[0];
				}

				promotedArgs.Add(Expression.NewArrayInit(paramsArrayElementType, paramsArrayPromotedArgument));
			}

			// Add default params, if needed.
			promotedArgs.AddRange(method.Parameters.Skip(promotedArgs.Count).Select<ParameterInfo, Expression>(x =>
			{
				if (x.HasDefaultValue)
				{
					var parameterType = TypeUtils.GetConcreteTypeForGenericMethod(x.ParameterType, promotedArgs, method);

					return Expression.Constant(x.DefaultValue, parameterType);
				}


				if (ReflectionExtensions.HasParamsArrayType(x))
				{
					method.HasParamsArray = true;
					return Expression.NewArrayInit(x.ParameterType.GetElementType());
				}

				throw new Exception("No default value found!");
			}));

			method.PromotedParameters = promotedArgs.ToArray();

			if (method.MethodBase != null && method.MethodBase.IsGenericMethodDefinition &&
				method.MethodBase is MethodInfo)
			{
				var genericMethod = MakeGenericMethod(method);
				if (genericMethod == null)
					return false;

				// we have all the type information we can get, update interpreter expressions and evaluate them
				var actualMethodParameters = genericMethod.GetParameters();
				for (var i = 0; i < method.PromotedParameters.Length; i++)
				{
					if (method.PromotedParameters[i] is InterpreterExpression ie)
					{
						var actualParamInfo = actualMethodParameters[i];
						var lambdaExpr = ie.EvalAs(actualParamInfo.ParameterType);
						if (lambdaExpr == null)
							return false;

						method.PromotedParameters[i] = lambdaExpr;

						// we have inferred all types, update the method definition
						genericMethod = MakeGenericMethod(method);
					}
				}

				method.MethodBase = genericMethod;
			}

			return true;
		}

		private static bool MethodHasPriority(Expression[] args, MethodData method, MethodData otherMethod)
		{
			var better = false;

			// check conversion from argument list to parameter list
			for (int i = 0, m = 0, o = 0; i < args.Length; i++)
			{
				var arg = args[i];
				var methodParam = method.Parameters[m];
				var otherMethodParam = otherMethod.Parameters[o];
				var methodParamType = ReflectionExtensions.GetParameterType(methodParam);
				var otherMethodParamType = ReflectionExtensions.GetParameterType(otherMethodParam);

				if (methodParamType.ContainsGenericParameters)
					methodParamType = method.PromotedParameters[i].Type;

				if (otherMethodParamType.ContainsGenericParameters)
					otherMethodParamType = otherMethod.PromotedParameters[i].Type;

				var c = TypeUtils.CompareConversions(arg.Type, methodParamType, otherMethodParamType);
				if (c < 0)
					return false;
				if (c > 0)
					better = true;

				if (!ReflectionExtensions.HasParamsArrayType(methodParam)) m++;
				if (!ReflectionExtensions.HasParamsArrayType(otherMethodParam)) o++;
			}

			if (better)
				return true;

			if (method.MethodBase != null &&
				otherMethod.MethodBase != null &&
				!method.MethodBase.IsGenericMethod &&
				otherMethod.MethodBase.IsGenericMethod)
				return true;

			if (!method.HasParamsArray && otherMethod.HasParamsArray)
				return true;

			// if a method has a params parameter, it can have less parameters than the number of arguments
			if (method.HasParamsArray && otherMethod.HasParamsArray && method.Parameters.Length > otherMethod.Parameters.Length)
				return true;

			if (method is IndexerData indexer && otherMethod is IndexerData otherIndexer)
			{
				var declaringType = indexer.Indexer.DeclaringType;
				var otherDeclaringType = otherIndexer.Indexer.DeclaringType;

				var isOtherIndexerIsInParentType = otherDeclaringType.IsAssignableFrom(declaringType);
				if (isOtherIndexerIsInParentType)
				{
					var isIndexerIsInDescendantType = !declaringType.IsAssignableFrom(otherDeclaringType);
					return isIndexerIsInDescendantType;
				}
			}

			return better;
		}

		private static MethodInfo MakeGenericMethod(MethodData method)
		{
			var methodInfo = (MethodInfo)method.MethodBase;
			var actualGenericArgs = ExtractActualGenericArguments(
				method.Parameters.Select(p => p.ParameterType).ToArray(),
				method.PromotedParameters.Select(p => p.Type).ToArray());

			var genericArgs = methodInfo.GetGenericArguments()
				.Select(p => actualGenericArgs.TryGetValue(p.Name, out var typ) ? typ : typeof(object))
				.ToArray();

			MethodInfo genericMethod = null;
			try
			{
				genericMethod = methodInfo.MakeGenericMethod(genericArgs);
			}
			catch (ArgumentException e) when (e.InnerException is VerificationException)
			{
				// this exception is thrown when a generic argument violates the generic constraints
				return null;
			}

			return genericMethod;
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
					if (!actualType.IsGenericParameter)
						extractedGenericTypes[requestedType.Name] = actualType;
				}
				else if (requestedType.IsArray && actualType.IsArray)
				{
					var innerGenericTypes = ExtractActualGenericArguments(
						new[] { requestedType.GetElementType() },
						new[] { actualType.GetElementType()
					});

					foreach (var innerGenericType in innerGenericTypes)
						extractedGenericTypes[innerGenericType.Key] = innerGenericType.Value;
				}
				else if (requestedType.ContainsGenericParameters)
				{
					if (actualType.IsGenericParameter)
					{
						extractedGenericTypes[actualType.Name] = requestedType;
					}
					else
					{
						var requestedInnerGenericArgs = requestedType.GetGenericArguments();
						var actualInnerGenericArgs = actualType.GetGenericArguments();
						if (requestedInnerGenericArgs.Length != actualInnerGenericArgs.Length)
							continue;

						var innerGenericTypes = ExtractActualGenericArguments(requestedInnerGenericArgs, actualInnerGenericArgs);
						foreach (var innerGenericType in innerGenericTypes)
							extractedGenericTypes[innerGenericType.Key] = innerGenericType.Value;
					}
				}
			}

			return extractedGenericTypes;
		}
	}
}
