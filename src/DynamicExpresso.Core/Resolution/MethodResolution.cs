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
		public static IList<MethodData> FindBestMethod(IEnumerable<MethodBase> methods, Expression[] args)
		{
			return FindBestMethod(methods.Select(MethodData.Gen), args);
		}

		public static IList<MethodData> FindBestMethod(IEnumerable<MethodData> methods, Expression[] args)
		{
			List<MethodData> applicable = null;
			bool allowPartial = true;
			foreach (var method in methods)
			{
				var matchLevel = CheckMethodMatchAndPrepareIt(!allowPartial, method, args);
				if (matchLevel == MethodMatchLevel.Exact || allowPartial && matchLevel == MethodMatchLevel.Partial)
				{
					allowPartial = matchLevel != MethodMatchLevel.Exact;
					if (applicable?.Count > 0)
					{
						if (IsBetterThanAllCandidates(method, applicable, args))
						{
							applicable.Clear();
						}
						else
						{
							//Are the existing ones better?
							bool skip = false;
							foreach (var priorCandidate in applicable)
							{
								if (MethodHasPriority(args, priorCandidate, method))
								{
									//The current method is not as good as the existing candidate(s)
									skip = true;
									break;
								}
							}
							if (skip)
							{
								continue;
							}
						}
					}
					if (applicable == null)
					{
						applicable = new List<MethodData>();
					}
					applicable.Add(method);
					if (!allowPartial && applicable.Count > 1)
					{
						//We've hit 2 exact matches so no need to go any farther.
						break;
					}
				}
			}
			return applicable as IList<MethodData> ?? Array.Empty<MethodData>();
		}

		private static bool IsBetterThanAllCandidates(MethodData candidate, List<MethodData> otherCandidates, Expression[] args)
		{
			foreach (var other in otherCandidates)
			{
				if (candidate != other && !MethodHasPriority(args, candidate, other))
					return false;
			}

			return true;
		}

		public enum MethodMatchLevel
		{
			None = 0,
			Partial = 1,
			Exact = 2
		}

		public static bool CheckIfMethodIsApplicableAndPrepareIt(MethodData method, Expression[] args)
		{
			return CheckMethodMatchAndPrepareIt(false, method, args) != MethodMatchLevel.None;
		}

		public static MethodMatchLevel CheckMethodMatchAndPrepareIt(bool isExactMatchRequired, MethodData method, Expression[] args)
		{
			int count = 0;
			foreach (var y in method.Parameters)
			{
				if (!y.HasDefaultValue && !ReflectionExtensions.HasParamsArrayType(y))
				{
					if (++count > args.Length)
					{
						return MethodMatchLevel.None;
					}
				}
			}

			var promotedArgs = new List<Expression>(method.Parameters.Count);
			var declaredWorkingParameters = 0;

			Type paramsArrayTypeFound = null;
			List<Expression> paramsArrayPromotedArgument = null;

			MethodMatchLevel matchLevel = MethodMatchLevel.Exact;
			foreach (var currentArgument in args)
			{
				Type parameterType;

				if (paramsArrayTypeFound != null)
				{
					parameterType = paramsArrayTypeFound;
				}
				else
				{
					if (declaredWorkingParameters >= method.Parameters.Count)
					{
						return MethodMatchLevel.None;
					}

					var parameterDeclaration = method.Parameters[declaredWorkingParameters];
					if (parameterDeclaration.IsOut)
					{
						return MethodMatchLevel.None;
					}

					parameterType = parameterDeclaration.ParameterType;

					if (ReflectionExtensions.HasParamsArrayType(parameterDeclaration))
					{
						paramsArrayTypeFound = parameterType;
						if (isExactMatchRequired)
						{
							return MethodMatchLevel.Partial;
						}
						matchLevel = MethodMatchLevel.Partial;
					}

					declaredWorkingParameters++;
				}

				if (paramsArrayPromotedArgument == null && (paramsArrayTypeFound == null || args.Length == method.Parameters.Count))
				{
					if (parameterType.IsGenericParameter)
					{
						// an interpreter expression can only be matched to a parameter of type Func
						if (currentArgument is InterpreterExpression)
							return MethodMatchLevel.None;

						promotedArgs.Add(currentArgument);
						continue;
					}

					var promoted = ExpressionUtils.PromoteExpression(currentArgument, parameterType);
					if (promoted != null)
					{
						if (promoted != currentArgument)
						{
							if (isExactMatchRequired)
							{
								return MethodMatchLevel.Partial;
							}
							matchLevel = MethodMatchLevel.Partial;
						}
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

				return MethodMatchLevel.None;
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
			foreach (var parameter in method.Parameters.Skip(promotedArgs.Count))
			{
				if (parameter.HasDefaultValue)
				{
					var parameterType = TypeUtils.GetConcreteTypeForGenericMethod(parameter.ParameterType, promotedArgs, method);

					var defaultValue = parameter.DefaultValue;
					if (defaultValue is null && parameterType.IsValueType)
						defaultValue = Activator.CreateInstance(parameterType);

					promotedArgs.Add(Expression.Constant(defaultValue, parameterType));
				}
				else if (ReflectionExtensions.HasParamsArrayType(parameter))
				{
					method.HasParamsArray = true;
					promotedArgs.Add(Expression.NewArrayInit(parameter.ParameterType.GetElementType()));
				}
				else
				{
					throw new Exception("No default value found!");
				}
			}

			method.PromotedParameters = promotedArgs;

			if (method.MethodBase != null && method.MethodBase.IsGenericMethodDefinition &&
				method.MethodBase is MethodInfo)
			{
				var genericMethod = MakeGenericMethod(method);
				if (genericMethod == null)
					return MethodMatchLevel.None;

				// we have all the type information we can get, update interpreter expressions and evaluate them
				var actualMethodParameters = genericMethod.GetParameters();
				for (var i = 0; i < method.PromotedParameters.Count; i++)
				{
					if (method.PromotedParameters[i] is InterpreterExpression ie)
					{
						var actualParamInfo = actualMethodParameters[i];
						var lambdaExpr = ie.EvalAs(actualParamInfo.ParameterType);
						if (lambdaExpr == null)
							return MethodMatchLevel.None;

						method.PromotedParameters[i] = lambdaExpr;

						// we have inferred all types, update the method definition
						genericMethod = MakeGenericMethod(method);
					}
				}

				method.MethodBase = genericMethod;
			}

			return matchLevel;
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
			if (method.HasParamsArray && otherMethod.HasParamsArray && method.Parameters.Count > otherMethod.Parameters.Count)
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
