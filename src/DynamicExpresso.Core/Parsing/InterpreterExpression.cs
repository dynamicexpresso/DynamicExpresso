using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DynamicExpresso.Exceptions;
using DynamicExpresso.Reflection;
using DynamicExpresso.Resources;

namespace DynamicExpresso.Parsing
{
	internal class InterpreterExpression : Expression
	{
		private readonly Interpreter _interpreter;
		private readonly string _expressionText;
		private readonly IList<Parameter> _parameters;
		private Type _type;

		public InterpreterExpression(ParserArguments parserArguments, string expressionText, params ParameterWithPosition[] parameters)
		{
			var settings = parserArguments.Settings.Clone();
			_interpreter = new Interpreter(settings);
			_expressionText = expressionText;
			_parameters = parameters;

			// Take the parent expression's parameters and set them as an identifier that
			// can be accessed by any lower call
			// note: this doesn't impact the initial settings, because they're cloned
			foreach (var dp in parserArguments.DeclaredParameters)
			{
				// Have to mark the parameter as "Used" otherwise we can get a compilation error.
				parserArguments.TryGetParameters(dp.Name, out var pe);
				_interpreter.SetIdentifier(new Identifier(dp.Name, pe));
			}

			foreach (var myParameter in parameters)
			{
				if (settings.Identifiers.ContainsKey(myParameter.Name))
				{
					throw new ParseException(string.Format(ErrorMessages.DuplicateLocalParameterDeclaration, myParameter.Name), myParameter.Position);
				}
			}

			// prior to evaluation, we don't know the generic arguments types
			_type = ReflectionExtensions.GetFuncType(parameters.Length);
		}

		public IList<Parameter> Parameters
		{
			get { return _parameters; }
		}

		public override Type Type
		{
			get { return _type; }
		}

		internal LambdaExpression EvalAs(Type delegateType)
		{
			if (!IsCompatibleWithDelegate(delegateType))
				return null;

			var lambdaExpr = _interpreter.ParseAsExpression(delegateType, _expressionText, _parameters.Select(p => p.Name).ToArray());
			_type = lambdaExpr.Type;
			return lambdaExpr;
		}

		internal bool IsCompatibleWithDelegate(Type target)
		{
			if (!target.IsGenericType || target.BaseType != typeof(MulticastDelegate))
				return false;

			var genericTypeDefinition = target.GetGenericTypeDefinition();
			return genericTypeDefinition == ReflectionExtensions.GetFuncType(_parameters.Count)
				|| genericTypeDefinition == ReflectionExtensions.GetActionType(_parameters.Count);
		}
	}
}
