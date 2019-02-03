using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using DynamicExpresso.Exceptions;
using DynamicExpresso.Resources;

// Code based on the Dynamic.cs file of the DynamicQuery sample by Microsoft
// http://msdn.microsoft.com/en-us/vstudio/bb894665.aspx
// http://weblogs.asp.net/scottgu/archive/2008/01/07/dynamic-linq-part-1-using-the-linq-dynamic-query-library.aspx
//
// Copyright (C) Microsoft Corporation.  All rights reserved.

namespace DynamicExpresso.Parsing
{
	internal class Parser
	{
		public static Expression Parse(ParserArguments arguments)
		{
			return new Parser(arguments).Parse();
		}

		private const NumberStyles ParseLiteralNumberStyle = NumberStyles.AllowLeadingSign;
		private const NumberStyles ParseLiteralUnsignedNumberStyle = NumberStyles.AllowLeadingSign;
		private const NumberStyles ParseLiteralDecimalNumberStyle = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;
		private static readonly CultureInfo ParseCulture = CultureInfo.InvariantCulture;

		private readonly ParserArguments _arguments;

		// Working context implementation
		//ParameterExpression it;

		private int _parsePosition;
		private readonly string _expressionText;
		private readonly int _expressionTextLength;
		private char _parseChar;
		private Token _token;

		private readonly Expressions _expressions;

		private Parser(ParserArguments arguments)
		{
			_arguments = arguments;
			_expressions = new Expressions(_arguments);

			_expressionText = arguments.ExpressionText ?? string.Empty;
			_expressionTextLength = _expressionText.Length;
			SetTextPos(0);
			NextToken();
		}

		private Expression Parse()
		{
			Expression expr = ParseExpressionSegment(_arguments.ExpressionReturnType);

			ValidateToken(TokenId.End, ErrorMessages.SyntaxError);
			return expr;
		}

		private Expression ParseExpressionSegment(Type returnType)
		{
			int errorPos = _token.pos;
			var expression = ParseExpressionSegment();

			if (returnType != typeof(void))
			{
				return Expressions.GenerateConversion(expression, returnType, errorPos);
			}

			return expression;
		}

		private Expression ParseExpressionSegment()
		{
			// The following methods respect the operator precedence as defined in
			// MSDN C# "Operator precedence and associativity"
			// https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/

			return ParseAssignment();
		}

		// = operator
		private Expression ParseAssignment()
		{
			var left = ParseConditional();
			if (_token.id == TokenId.Equal)
			{
				if (!_arguments.Settings.AssignmentOperators.HasFlag(AssignmentOperators.AssignmentEqual))
					throw new AssignmentOperatorDisabledException("=", _token.pos);

				if (!Expressions.IsWritable(left))
					throw ParseException.Create(_token.pos, ErrorMessages.ExpressionMustBeWritable);

				NextToken();

				var right = ParseAssignment();
				var promoted = Expressions.PromoteExpression(right, left.Type, true);
				if (promoted == null)
					throw ParseException.Create(_token.pos, ErrorMessages.CannotConvertValue,
						Types.GetTypeName(right.Type), Types.GetTypeName(left.Type));

				left = Expression.Assign(left, promoted);
			}
			return left;
		}

		// ?: operator
		private Expression ParseConditional()
		{
			var errorPos = _token.pos;
			var expr = ParseConditionalOr();
			if (_token.id == TokenId.QuestionQuestion)
			{
				NextToken();
				var exprRight = ParseExpressionSegment();
				expr = Expressions.GenerateConditional(Expressions.GenerateEqual(expr, ParserConstants.NullLiteralExpression), exprRight, expr, errorPos);
			}
			else if (_token.id == TokenId.Question)
			{
				NextToken();
				var expr1 = ParseExpressionSegment();
				ValidateToken(TokenId.Colon, ErrorMessages.ColonExpected);
				NextToken();
				var expr2 = ParseExpressionSegment();
				expr = Expressions.GenerateConditional(expr, expr1, expr2, errorPos);
			}
			return expr;
		}

		// || operator
		private Expression ParseConditionalOr()
		{
			var left = ParseConditionalAnd();
			while (_token.id == TokenId.DoubleBar)
			{
				NextToken();
				var right = ParseConditionalAnd();
				_expressions.CheckAndPromoteOperands(typeof(ParseSignatures.ILogicalSignatures), ref left, ref right);
				left = Expression.OrElse(left, right);
			}
			return left;
		}

		// && operator
		private Expression ParseConditionalAnd()
		{
			var left = ParseLogicalOr();
			while (_token.id == TokenId.DoubleAmphersand)
			{
				NextToken();
				var right = ParseLogicalOr();
				_expressions.CheckAndPromoteOperands(typeof(ParseSignatures.ILogicalSignatures), ref left, ref right);
				left = Expression.AndAlso(left, right);
			}
			return left;
		}

		// | operator
		private Expression ParseLogicalOr()
		{
			var left = ParseLogicalXor();
			while (_token.id == TokenId.Bar)
			{
				NextToken();
				var right = ParseLogicalXor();
				_expressions.CheckAndPromoteOperands(typeof(ParseSignatures.ILogicalSignatures), ref left, ref right);
				left = Expression.Or(left, right);
			}
			return left;
		}

		// ^ operator
		private Expression ParseLogicalXor()
		{
			var left = ParseLogicalAnd();
			while (_token.id == TokenId.Caret)
			{
				NextToken();
				var right = ParseLogicalAnd();
				_expressions.CheckAndPromoteOperands(typeof(ParseSignatures.ILogicalSignatures), ref left, ref right);
				left = Expression.ExclusiveOr(left, right);
			}
			return left;
		}

		// & operator
		private Expression ParseLogicalAnd()
		{
			var left = ParseComparison();
			while (_token.id == TokenId.Amphersand)
			{
				NextToken();
				var right = ParseComparison();
				_expressions.CheckAndPromoteOperands(typeof(ParseSignatures.ILogicalSignatures), ref left, ref right);
				left = Expression.And(left, right);
			}
			return left;
		}

		// ==, !=, >, >=, <, <= operators
		private Expression ParseComparison()
		{
			var left = ParseTypeTesting();
			while (_token.id == TokenId.DoubleEqual || _token.id == TokenId.ExclamationEqual ||
					_token.id == TokenId.GreaterThan || _token.id == TokenId.GreaterThanEqual ||
					_token.id == TokenId.LessThan || _token.id == TokenId.LessThanEqual)
			{
				var op = _token;
				NextToken();
				var right = ParseAdditive();
				var isEquality = op.id == TokenId.DoubleEqual || op.id == TokenId.ExclamationEqual;

				//if (isEquality && !left.Type.IsValueType && !right.Type.IsValueType)
				//{
				//	if (left.Type != right.Type)
				//	{
				//		if (left.Type.IsAssignableFrom(right.Type))
				//		{
				//			right = Expression.Convert(right, left.Type);
				//		}
				//		else if (right.Type.IsAssignableFrom(left.Type))
				//		{
				//			left = Expression.Convert(left, right.Type);
				//		}
				//		else
				//		{
				//			throw ParseException.Create(op.pos, ErrorMessages.IncompatibleOperands,
				//				op.text, GetTypeName(left.Type), GetTypeName(right.Type));
				//		}
				//	}
				//}
				//else if (IsEnumType(left.Type) || IsEnumType(right.Type))
				//{
				//	if (left.Type != right.Type)
				//	{
				//		Expression e;
				//		if ((e = PromoteExpression(right, left.Type, true)) != null)
				//		{
				//			right = e;
				//		}
				//		else if ((e = PromoteExpression(left, right.Type, true)) != null)
				//		{
				//			left = e;
				//		}
				//		else
				//		{
				//			throw ParseException.Create(op.pos, ErrorMessages.IncompatibleOperands,
				//				op.text, GetTypeName(left.Type), GetTypeName(right.Type));
				//		}
				//	}
				//}
				//else
				//{
				//	CheckAndPromoteOperands(isEquality ? typeof(ParseSignatures.IEqualitySignatures) : typeof(ParseSignatures.IRelationalSignatures),
				//			op.text, ref left, ref right, op.pos);
				//}

				_expressions.CheckAndPromoteOperands(
					isEquality ? typeof(ParseSignatures.IEqualitySignatures) : typeof(ParseSignatures.IRelationalSignatures),
					ref left,
					ref right);

				switch (op.id)
				{
					case TokenId.DoubleEqual:
						left = Expressions.GenerateEqual(left, right);
						break;
					case TokenId.ExclamationEqual:
						left = Expressions.GenerateNotEqual(left, right);
						break;
					case TokenId.GreaterThan:
						left = Expressions.GenerateGreaterThan(left, right);
						break;
					case TokenId.GreaterThanEqual:
						left = Expressions.GenerateGreaterThanEqual(left, right);
						break;
					case TokenId.LessThan:
						left = Expressions.GenerateLessThan(left, right);
						break;
					case TokenId.LessThanEqual:
						left = Expressions.GenerateLessThanEqual(left, right);
						break;
				}
			}
			return left;
		}

		// is, as operators
		private Expression ParseTypeTesting()
		{
			var left = ParseAdditive();
			while (_token.text == ParserConstants.KeywordIs
					|| _token.text == ParserConstants.KeywordAs)
			{
				var typeOperator = _token.text;

				var op = _token;
				NextToken();

				Type knownType;
				if (!_arguments.TryGetKnownType(_token.text, out knownType))
					throw ParseException.Create(op.pos, ErrorMessages.TypeIdentifierExpected);

				if (typeOperator == ParserConstants.KeywordIs)
					left = Expression.TypeIs(left, knownType);
				else if (typeOperator == ParserConstants.KeywordAs)
					left = Expression.TypeAs(left, knownType);
				else
					throw ParseException.Create(_token.pos, ErrorMessages.SyntaxError);

				NextToken();
			}

			return left;
		}

		// +, -, & operators
		private Expression ParseAdditive()
		{
			var left = ParseMultiplicative();
			while (_token.id == TokenId.Plus || _token.id == TokenId.Minus)
			{
				var op = _token;
				NextToken();
				var right = ParseMultiplicative();
				switch (op.id)
				{
					case TokenId.Plus:
						if (left.Type == typeof(string) || right.Type == typeof(string))
						{
							left = Expressions.GenerateStringConcat(left, right);
						}
						else
						{
							_expressions.CheckAndPromoteOperands(typeof(ParseSignatures.IAddSignatures), ref left, ref right);
							left = Expressions.GenerateAdd(left, right);
						}
						break;
					case TokenId.Minus:
						_expressions.CheckAndPromoteOperands(typeof(ParseSignatures.ISubtractSignatures), ref left, ref right);
						left = Expressions.GenerateSubtract(left, right);
						break;
				}
			}
			return left;
		}

		// *, /, % operators
		private Expression ParseMultiplicative()
		{
			var left = ParseUnary();
			while (_token.id == TokenId.Asterisk || _token.id == TokenId.Slash ||
					_token.id == TokenId.Percent)
			{
				var op = _token;
				NextToken();
				var right = ParseUnary();

				_expressions.CheckAndPromoteOperands(typeof(ParseSignatures.IArithmeticSignatures), ref left, ref right);

				switch (op.id)
				{
					case TokenId.Asterisk:
						left = Expression.Multiply(left, right);
						break;
					case TokenId.Slash:
						left = Expression.Divide(left, right);
						break;
					case TokenId.Percent:
						left = Expression.Modulo(left, right);
						break;
				}
			}
			return left;
		}

		// +,-, ! unary operators
		private Expression ParseUnary()
		{
			if (_token.id == TokenId.Minus || _token.id == TokenId.Exclamation || _token.id == TokenId.Plus)
			{
				var op = _token;
				NextToken();
				if (_token.id == TokenId.IntegerLiteral ||
						_token.id == TokenId.RealLiteral)
				{
					if (op.id == TokenId.Minus)
					{
						_token.text = "-" + _token.text;
						_token.pos = op.pos;
						return ParsePrimary();
					}

					if (op.id == TokenId.Plus)
					{
						_token.text = "+" + _token.text;
						_token.pos = op.pos;
						return ParsePrimary();
					}
				}
				var expr = ParseUnary();
				if (op.id == TokenId.Minus)
				{
					_expressions.CheckAndPromoteOperand(typeof(ParseSignatures.INegationSignatures), ref expr);
					expr = Expression.Negate(expr);
				}
				else if (op.id == TokenId.Plus)
				{

				}
				else if (op.id == TokenId.Exclamation)
				{
					_expressions.CheckAndPromoteOperand(typeof(ParseSignatures.INotSignatures), ref expr);
					expr = Expression.Not(expr);
				}
				return expr;
			}
			return ParsePrimary();
		}

		private Expression ParsePrimary()
		{
			var tokenPos = _token.pos;
			var expr = ParsePrimaryStart();
			while (true)
			{
				if (_token.id == TokenId.Dot)
				{
					NextToken();
					expr = ParseMemberAccess(null, expr);
				}
				else if(_token.id == TokenId.QuestionDot)
				{
					NextToken();
					expr = Expressions.GenerateConditional(Expressions.GenerateEqual(expr, ParserConstants.NullLiteralExpression), ParserConstants.NullLiteralExpression, ParseMemberAccess(null, expr), _token.pos);
				}
				else if (_token.id == TokenId.OpenBracket)
				{
					expr = ParseElementAccess(expr);
				}
				else if (_token.id == TokenId.OpenParen)
				{
					var lambda = expr as LambdaExpression;
					if (lambda != null)
						return ParseLambdaInvocation(lambda, tokenPos);

					if (typeof(Delegate).IsAssignableFrom(expr.Type))
						expr = ParseDelegateInvocation(expr, tokenPos);
					else
						throw ParseException.Create(tokenPos, ErrorMessages.InvalidMethodCall, Types.GetTypeName(expr.Type));
				}
				else
				{
					break;
				}
			}
			return expr;
		}

		private Expression ParsePrimaryStart()
		{
			switch (_token.id)
			{
				case TokenId.Identifier:
					return ParseIdentifier();
				case TokenId.CharLiteral:
					return ParseCharLiteral();
				case TokenId.StringLiteral:
					return ParseStringLiteral();
				case TokenId.IntegerLiteral:
					return ParseIntegerLiteral();
				case TokenId.RealLiteral:
					return ParseRealLiteral();
				case TokenId.OpenParen:
					return ParseParenExpression();
				case TokenId.End:
					return Expression.Empty();
				default:
					throw ParseException.Create(_token.pos, ErrorMessages.ExpressionExpected);
			}
		}

		private Expression ParseCharLiteral()
		{
			ValidateToken(TokenId.CharLiteral);
			var s = _token.text.Substring(1, _token.text.Length - 2);

			s = EvalEscapeStringLiteral(s);

			if (s.Length != 1)
				throw ParseException.Create(_token.pos, ErrorMessages.InvalidCharacterLiteral);

			NextToken();
			return Expressions.CreateLiteral(s[0]);
		}

		private Expression ParseStringLiteral()
		{
			ValidateToken(TokenId.StringLiteral);
			var s = _token.text.Substring(1, _token.text.Length - 2);

			s = EvalEscapeStringLiteral(s);

			//int start = 0;
			//while (true)
			//{
			//    int i = s.IndexOf(quote, start);
			//    if (i < 0)
			//        break;
			//    s = s.Remove(i, 1);
			//    start = i + 1;
			//}

			NextToken();
			return Expressions.CreateLiteral(s);
		}

		private string EvalEscapeStringLiteral(string source)
		{
			var builder = new StringBuilder();

			for (int i = 0; i < source.Length; i++)
			{
				var c = source[i];
				if (c == '\\')
				{
					if ((i + 1) == source.Length)
						throw ParseException.Create(_token.pos, ErrorMessages.InvalidEscapeSequence);

					builder.Append(EvalEscapeChar(source[++i]));
				}
				else
					builder.Append(c);
			}

			return builder.ToString();
		}

		private char EvalEscapeChar(char source)
		{
			switch (source)
			{
				case '\'':
					return '\'';
				case '"':
					return '"';
				case '\\':
					return '\\';
				case '0':
					return '\0';
				case 'a':
					return '\a';
				case 'b':
					return '\b';
				case 'f':
					return '\f';
				case 'n':
					return '\n';
				case 'r':
					return '\r';
				case 't':
					return '\t';
				case 'v':
					return '\v';
				default:
					throw ParseException.Create(_token.pos, ErrorMessages.InvalidEscapeSequence);
			}
		}

		private Expression ParseIntegerLiteral()
		{
			ValidateToken(TokenId.IntegerLiteral);
			var text = _token.text;
			if (text[0] != '-')
			{
				if (!ulong.TryParse(text, ParseLiteralUnsignedNumberStyle, ParseCulture, out ulong value))
					throw ParseException.Create(_token.pos, ErrorMessages.InvalidIntegerLiteral, text);

				NextToken();

				if (value <= int.MaxValue)
					return Expressions.CreateLiteral((int)value);
				if (value <= uint.MaxValue)
					return Expressions.CreateLiteral((uint)value);
				if (value <= long.MaxValue)
					return Expressions.CreateLiteral((long)value);

				return Expressions.CreateLiteral(value);
			}
			else
			{
				if (!long.TryParse(text, ParseLiteralNumberStyle, ParseCulture, out long value))
					throw ParseException.Create(_token.pos, ErrorMessages.InvalidIntegerLiteral, text);

				NextToken();

				if (value >= int.MinValue && value <= int.MaxValue)
					return Expressions.CreateLiteral((int)value);

				return Expressions.CreateLiteral(value);
			}
		}

		private Expression ParseRealLiteral()
		{
			ValidateToken(TokenId.RealLiteral);
			var text = _token.text;
			object value = null;
			var last = text[text.Length - 1];

			if (last == 'F' || last == 'f')
			{
				if (float.TryParse(text.Substring(0, text.Length - 1), ParseLiteralDecimalNumberStyle, ParseCulture, out float f))
					value = f;
			}
			else if (last == 'M' || last == 'm')
			{
				if (decimal.TryParse(text.Substring(0, text.Length - 1), ParseLiteralDecimalNumberStyle, ParseCulture, out decimal dc))
					value = dc;
			}
			else
			{
				if (double.TryParse(text, ParseLiteralDecimalNumberStyle, ParseCulture, out double d))
					value = d;
			}

			if (value == null)
				throw ParseException.Create(_token.pos, ErrorMessages.InvalidRealLiteral, text);

			NextToken();

			return Expressions.CreateLiteral(value);
		}

		private Expression ParseParenExpression()
		{
			ValidateToken(TokenId.OpenParen, ErrorMessages.OpenParenExpected);
			NextToken();
			Expression innerParenthesesExpression = ParseExpressionSegment();
			ValidateToken(TokenId.CloseParen, ErrorMessages.CloseParenOrOperatorExpected);

			var constExp = innerParenthesesExpression as ConstantExpression;
			if (constExp != null && constExp.Value is Type)
			{
				NextToken();
				var nextExpression = ParseExpressionSegment();

				// cast: (constExp)nextExpression
				return Expression.Convert(nextExpression, (Type)constExp.Value);
			}

			NextToken();
			return innerParenthesesExpression;
		}

		private Expression ParseIdentifier()
		{
			ValidateToken(TokenId.Identifier);

			// Working context implementation
			//if (token.text == ParserConstants.keywordIt)
			//    return ParseIt();

			if (_token.text == ParserConstants.KeywordNew)
				return ParseNew();
			if (_token.text == ParserConstants.KeywordTypeof)
				return ParseTypeof();

			if (_arguments.TryGetIdentifier(_token.text, out Expression keywordExpression))
			{
				NextToken();
				return keywordExpression;
			}

			if (_arguments.TryGetParameters(_token.text, out ParameterExpression parameterExpression))
			{
				NextToken();
				return parameterExpression;
			}

			if (_arguments.TryGetKnownType(_token.text, out Type knownType))
			{
				return ParseTypeKeyword(knownType);
			}

			// Working context implementation
			//if (it != null)
			//    return ParseMemberAccess(null, it);

			throw new UnknownIdentifierException(_token.text, _token.pos);
		}

		// Working context implementation
		//Expression ParseIt()
		//{
		//    if (it == null)
		//        throw ParseError(ErrorMessages.NoItInScope);
		//    NextToken();
		//    return it;
		//}

		//Expression ParseIif()
		//{
		//    int errorPos = token.pos;
		//    NextToken();
		//    Expression[] args = ParseArgumentList();
		//    if (args.Length != 3)
		//        throw ParseError(errorPos, ErrorMessages.IifRequiresThreeArgs);
		//    return GenerateConditional(args[0], args[1], args[2], errorPos);
		//}

		private Expression ParseTypeof()
		{
			var errorPos = _token.pos;
			NextToken();
			var args = ParseArgumentList();
			if (args.Length != 1)
				throw ParseException.Create(errorPos, ErrorMessages.TypeofRequiresOneArg);

			var constExp = args[0] as ConstantExpression;
			if (constExp == null || !(constExp.Value is Type))
				throw ParseException.Create(errorPos, ErrorMessages.TypeofRequiresAType);

			return constExp;
		}

		private Expression ParseNew()
		{
			NextToken();
			ValidateToken(TokenId.Identifier, ErrorMessages.IdentifierExpected);

			Type newType;
			if (!_arguments.TryGetKnownType(_token.text, out newType))
				throw new UnknownIdentifierException(_token.text, _token.pos);

			NextToken();
			var args = ParseArgumentList();

			var constructor = newType.GetConstructor(args.Select(p => p.Type).ToArray());
			if (constructor == null)
				throw ParseException.Create(_token.pos, ErrorMessages.NoApplicableConstructor, newType);

			return Expression.MemberInit(Expression.New(constructor, args));
		}

		private Expression ParseLambdaInvocation(LambdaExpression lambda, int errorPos)
		{
			var args = ParseArgumentList();

			if (!_expressions.PrepareDelegateInvoke(lambda.Type, ref args))
				throw ParseException.Create(errorPos, ErrorMessages.ArgsIncompatibleWithLambda);

			return Expression.Invoke(lambda, args);
		}

		private Expression ParseDelegateInvocation(Expression delegateExp, int errorPos)
		{
			var args = ParseArgumentList();

			if (!_expressions.PrepareDelegateInvoke(delegateExp.Type, ref args))
				throw ParseException.Create(errorPos, ErrorMessages.ArgsIncompatibleWithDelegate);

			return Expression.Invoke(delegateExp, args);
		}

		private Expression ParseTypeKeyword(Type type)
		{
			var errorPos = _token.pos;
			NextToken();
			if (_token.id == TokenId.Question)
			{
				if (!type.IsValueType || Types.IsNullableType(type))
					throw ParseException.Create(errorPos, ErrorMessages.TypeHasNoNullableForm, Types.GetTypeName(type));
				type = typeof(Nullable<>).MakeGenericType(type);
				NextToken();
			}

			//if (token.id == TokenId.OpenParen)
			//{
			//    return ParseTypeConstructor(type, errorPos);
			//}

			if (_token.id == TokenId.CloseParen)
			{
				return Expression.Constant(type);
			}

			ValidateToken(TokenId.Dot, ErrorMessages.DotOrOpenParenExpected);
			NextToken();
			return ParseMemberAccess(type, null);
		}

		//private Expression ParseTypeConstructor(Type type, int errorPos)
		//{
		//    Expression[] args = ParseArgumentList();
		//    MethodBase method;
		//    switch (FindBestMethod(type.GetConstructors(), args, out method))
		//    {
		//        case 0:
		//            if (args.Length == 1)
		//                return GenerateConversion(args[0], type, errorPos);
		//            throw ParseError(errorPos, ErrorMessages.NoMatchingConstructor, GetTypeName(type));
		//        case 1:
		//            return Expression.New((ConstructorInfo)method, args);
		//        default:
		//            throw ParseError(errorPos, ErrorMessages.AmbiguousConstructorInvocation, GetTypeName(type));
		//    }
		//}

		private Expression ParseMemberAccess(Type type, Expression instance)
		{
			if (instance != null) type = instance.Type;
			var errorPos = _token.pos;
			var id = GetIdentifier();
			NextToken();
			if (_token.id == TokenId.OpenParen)
				return ParseMethodInvocation(type, instance, errorPos, id);

			return _expressions.GeneratePropertyOrFieldExpression(type, instance, errorPos, id);
		}

		private Expression ParseMethodInvocation(Type type, Expression instance, int errorPos, string methodName)
		{
			var args = ParseArgumentList();

			var methodInvocationExpression = ParseNormalMethodInvocation(type, instance, errorPos, methodName, args);
			if (methodInvocationExpression == null && instance != null)
			{
				methodInvocationExpression = ParseExtensionMethodInvocation(type, instance, errorPos, methodName, args);
			}

			if (methodInvocationExpression != null)
				return methodInvocationExpression;

			if (Types.IsDynamicType(type) || Expressions.IsDynamicExpression(instance))
				return Expressions.GenerateDynamicMethodInvocation(type, instance, methodName, args);

			throw new NoApplicableMethodException(methodName, Types.GetTypeName(type), errorPos);
		}

		private Expression ParseExtensionMethodInvocation(Type type, Expression instance, int errorPos, string id, Expression[] args)
		{
			var extensionMethodsArguments = new Expression[args.Length + 1];
			extensionMethodsArguments[0] = instance;
			args.CopyTo(extensionMethodsArguments, 1);

			var matchMethods = _arguments.GetExtensionMethods(id);
			var extensionMethods = Expressions.FindBestMethod(matchMethods, extensionMethodsArguments);
			if (extensionMethods.Length > 1)
				throw ParseException.Create(errorPos, ErrorMessages.AmbiguousMethodInvocation, id, Types.GetTypeName(type));

			if (extensionMethods.Length == 1)
			{
				var method = extensionMethods[0];

				extensionMethodsArguments = method.PromotedParameters;

				return Expression.Call((MethodInfo)method.MethodBase, extensionMethodsArguments);
			}

			return null;
		}

		private Expression ParseNormalMethodInvocation(Type type, Expression instance, int errorPos, string id, Expression[] args)
		{
			var applicableMethods = _expressions.FindMethods(type, id, instance == null, args);
			if (applicableMethods.Length > 1)
				throw ParseException.Create(errorPos, ErrorMessages.AmbiguousMethodInvocation, id, Types.GetTypeName(type));

			if (applicableMethods.Length == 1)
			{
				var method = applicableMethods[0];

				return Expression.Call(instance, (MethodInfo)method.MethodBase, method.PromotedParameters);
			}

			return null;
		}

		//Expression ParseAggregate(Expression instance, Type elementType, string methodName, int errorPos)
		//{
		//    ParameterExpression outerIt = it;
		//    ParameterExpression innerIt = Expression.Parameter(elementType, "");
		//    it = innerIt;
		//    Expression[] args = ParseArgumentList();
		//    it = outerIt;
		//    MethodBase signature;
		//    if (FindMethod(typeof(IEnumerableSignatures), methodName, false, args, out signature) != 1)
		//        throw ParseError(errorPos, ErrorMessages.NoApplicableAggregate, methodName);
		//    Type[] typeArgs;
		//    if (signature.Name == "Min" || signature.Name == "Max")
		//    {
		//        typeArgs = new Type[] { elementType, args[0].Type };
		//    }
		//    else
		//    {
		//        typeArgs = new Type[] { elementType };
		//    }
		//    if (args.Length == 0)
		//    {
		//        args = new Expression[] { instance };
		//    }
		//    else
		//    {
		//        args = new Expression[] { instance, Expression.Lambda(args[0], innerIt) };
		//    }
		//    return Expression.Call(typeof(Enumerable), signature.Name, typeArgs, args);
		//}

		private Expression[] ParseArgumentList()
		{
			ValidateToken(TokenId.OpenParen, ErrorMessages.OpenParenExpected);
			NextToken();
			var args = _token.id != TokenId.CloseParen ? ParseArguments() : new Expression[0];
			ValidateToken(TokenId.CloseParen, ErrorMessages.CloseParenOrCommaExpected);
			NextToken();
			return args;
		}

		private Expression[] ParseArguments()
		{
			var argList = new List<Expression>();
			while (true)
			{
				argList.Add(ParseExpressionSegment());
				if (_token.id != TokenId.Comma) break;
				NextToken();
			}
			return argList.ToArray();
		}

		private Expression ParseElementAccess(Expression expr)
		{
			var errorPos = _token.pos;
			ValidateToken(TokenId.OpenBracket, ErrorMessages.OpenParenExpected);
			NextToken();
			var args = ParseArguments();
			ValidateToken(TokenId.CloseBracket, ErrorMessages.CloseBracketOrCommaExpected);
			NextToken();
			if (expr.Type.IsArray)
			{
				if (expr.Type.GetArrayRank() != args.Length)
					throw ParseException.Create(errorPos, ErrorMessages.IncorrectNumberOfIndexes);

				for (int i = 0; i < args.Length; i++)
				{
					args[i] = Expressions.PromoteExpression(args[i], typeof(int), true);
					if (args[i] == null)
						throw ParseException.Create(errorPos, ErrorMessages.InvalidIndex);
				}

				return Expression.ArrayAccess(expr, args);
			}

			var applicableMethods = Expressions.FindIndexer(expr.Type, args);
			if (applicableMethods.Length == 0)
			{
				throw ParseException.Create(errorPos, ErrorMessages.NoApplicableIndexer,
						Types.GetTypeName(expr.Type));
			}

			if (applicableMethods.Length > 1)
			{
				throw ParseException.Create(errorPos, ErrorMessages.AmbiguousIndexerInvocation,
						Types.GetTypeName(expr.Type));
			}

			var indexer = (Expressions.IndexerData) applicableMethods[0];
			return Expression.Property(expr, indexer.Indexer, indexer.PromotedParameters);
		}

		// ******************************************
		// ******************************************
		// ******************************************

		private void SetTextPos(int pos)
		{
			_parsePosition = pos;
			_parseChar = _parsePosition < _expressionTextLength ? _expressionText[_parsePosition] : '\0';
		}

		private void NextChar()
		{
			if (_parsePosition < _expressionTextLength)
				_parsePosition++;

			_parseChar = _parsePosition < _expressionTextLength ? _expressionText[_parsePosition] : '\0';
		}

		private void PreviousChar()
		{
			SetTextPos(_parsePosition - 1);
		}

		private void NextToken()
		{
			while (char.IsWhiteSpace(_parseChar))
				NextChar();

			TokenId t;
			var tokenPos = _parsePosition;
			switch (_parseChar)
			{
				case '!':
					NextChar();
					if (_parseChar == '=')
					{
						NextChar();
						t = TokenId.ExclamationEqual;
					}
					else
					{
						t = TokenId.Exclamation;
					}
					break;
				case '%':
					NextChar();
					t = TokenId.Percent;
					break;
				case '&':
					NextChar();
					if (_parseChar == '&')
					{
						NextChar();
						t = TokenId.DoubleAmphersand;
					}
					else
					{
						t = TokenId.Amphersand;
					}
					break;
				case '(':
					NextChar();
					t = TokenId.OpenParen;
					break;
				case ')':
					NextChar();
					t = TokenId.CloseParen;
					break;
				case '*':
					NextChar();
					t = TokenId.Asterisk;
					break;
				case '+':
					NextChar();
					t = TokenId.Plus;
					break;
				case ',':
					NextChar();
					t = TokenId.Comma;
					break;
				case '-':
					NextChar();
					t = TokenId.Minus;
					break;
				case '.':
					NextChar();

					if (char.IsDigit(_parseChar))
					{
						t = TokenId.RealLiteral;
						do
						{
							NextChar();
						} while (char.IsDigit(_parseChar));
						if (_parseChar == 'E' || _parseChar == 'e')
						{
							t = TokenId.RealLiteral;
							NextChar();
							if (_parseChar == '+' || _parseChar == '-')
								NextChar();
							ValidateDigit();
							do
							{
								NextChar();
							} while (char.IsDigit(_parseChar));
						}
						if (_parseChar == 'F' || _parseChar == 'f' || _parseChar == 'M' || _parseChar == 'm')
							NextChar();
						break;
					}

					t = TokenId.Dot;
					break;
				case '/':
					NextChar();
					t = TokenId.Slash;
					break;
				case ':':
					NextChar();
					t = TokenId.Colon;
					break;
				case '<':
					NextChar();
					if (_parseChar == '=')
					{
						NextChar();
						t = TokenId.LessThanEqual;
					}
					else
					{
						t = TokenId.LessThan;
					}
					break;
				case '=':
					NextChar();
					if (_parseChar == '=')
					{
						NextChar();
						t = TokenId.DoubleEqual;
					}
					else
					{
						t = TokenId.Equal;
					}
					break;
				case '>':
					NextChar();
					if (_parseChar == '=')
					{
						NextChar();
						t = TokenId.GreaterThanEqual;
					}
					else
					{
						t = TokenId.GreaterThan;
					}
					break;
				case '?':
					NextChar();
					if (_parseChar == '.')
					{
						NextChar();
						t = TokenId.QuestionDot;
					} else if(_parseChar == '?')
					{
						NextChar();
						t = TokenId.QuestionQuestion;
					} else
					{
						t = TokenId.Question;
					}
					break;
				case '[':
					NextChar();
					t = TokenId.OpenBracket;
					break;
				case ']':
					NextChar();
					t = TokenId.CloseBracket;
					break;
				case '|':
					NextChar();
					if (_parseChar == '|')
					{
						NextChar();
						t = TokenId.DoubleBar;
					}
					else
					{
						t = TokenId.Bar;
					}
					break;
				case '"':
					NextChar();
					bool isEscapeS = false;
					bool isEndS = _parseChar == '\"';
					while (_parsePosition < _expressionTextLength && !isEndS)
					{
						isEscapeS = _parseChar == '\\' && !isEscapeS;
						NextChar();
						isEndS = (_parseChar == '\"' && !isEscapeS);
					}

					if (_parsePosition == _expressionTextLength)
						throw ParseException.Create(_parsePosition, ErrorMessages.UnterminatedStringLiteral);

					NextChar();

					t = TokenId.StringLiteral;
					break;
				case '\'':
					NextChar();
					bool isEscapeC = false;
					bool isEndC = false;
					while (_parsePosition < _expressionTextLength && !isEndC)
					{
						isEscapeC = _parseChar == '\\' && !isEscapeC;
						NextChar();
						isEndC = (_parseChar == '\'' && !isEscapeC);
					}

					if (_parsePosition == _expressionTextLength)
						throw ParseException.Create(_parsePosition, ErrorMessages.UnterminatedStringLiteral);

					NextChar();

					t = TokenId.CharLiteral;
					break;
				case '^':
					NextChar();
					t = TokenId.Caret;
					break;
				default:

					if (char.IsLetter(_parseChar) || _parseChar == '@' || _parseChar == '_')
					{
						do
						{
							NextChar();
						} while (char.IsLetterOrDigit(_parseChar) || _parseChar == '_');
						t = TokenId.Identifier;
						break;
					}

					if (char.IsDigit(_parseChar))
					{
						t = TokenId.IntegerLiteral;
						do
						{
							NextChar();
						} while (char.IsDigit(_parseChar));

						if (_parseChar == '.')
						{
							NextChar();
							if (char.IsDigit(_parseChar))
							{
								t = TokenId.RealLiteral;
								do
								{
									NextChar();
								} while (char.IsDigit(_parseChar));
							}
							else
							{
								PreviousChar();
								break;
							}
						}

						if (_parseChar == 'E' || _parseChar == 'e')
						{
							t = TokenId.RealLiteral;
							NextChar();
							if (_parseChar == '+' || _parseChar == '-')
								NextChar();
							ValidateDigit();
							do
							{
								NextChar();
							} while (char.IsDigit(_parseChar));
						}

						if (_parseChar == 'F' || _parseChar == 'f' || _parseChar == 'M' || _parseChar == 'm')
						{
							t = TokenId.RealLiteral;
							NextChar();
						}

						break;
					}
					if (_parsePosition == _expressionTextLength)
					{
						t = TokenId.End;
						break;
					}
					throw ParseException.Create(_parsePosition, ErrorMessages.InvalidCharacter, _parseChar);
			}
			_token.id = t;
			_token.text = _expressionText.Substring(tokenPos, _parsePosition - tokenPos);
			_token.pos = tokenPos;
		}

		private string GetIdentifier()
		{
			ValidateToken(TokenId.Identifier, ErrorMessages.IdentifierExpected);
			var id = _token.text;
			if (id.Length > 1 && id[0] == '@')
				id = id.Substring(1);
			return id;
		}

		private void ValidateDigit()
		{
			if (!char.IsDigit(_parseChar))
				throw ParseException.Create(_parsePosition, ErrorMessages.DigitExpected);
		}

		// ReSharper disable once UnusedParameter.Local
		private void ValidateToken(TokenId t, string errorMessage)
		{
			if (_token.id != t)
				throw ParseException.Create(_token.pos, errorMessage);
		}

		// ReSharper disable once UnusedParameter.Local
		private void ValidateToken(TokenId t)
		{
			if (_token.id != t)
				throw ParseException.Create(_token.pos, ErrorMessages.SyntaxError);
		}
	}
}
