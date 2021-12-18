using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using DynamicExpresso.Exceptions;
using DynamicExpresso.Resources;
using Microsoft.CSharp.RuntimeBinder;

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
		private const NumberStyles ParseLiteralDoubleNumberStyle = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent;
		private static readonly CultureInfo ParseCulture = CultureInfo.InvariantCulture;

		private readonly ParserArguments _arguments;

		private static readonly MethodInfo _concatMethod = GetConcatMethod();
		private static readonly MethodInfo _toStringMethod = GetToStringMethod();

		private static MethodInfo GetConcatMethod()
		{
			var methodInfo = typeof(string).GetMethod("Concat", new[] {typeof(string), typeof(string)});
			if (methodInfo == null)
			{
				throw new Exception("String concat method not found");
			}

			return methodInfo;
		}

		private static MethodInfo GetToStringMethod()
		{
			var toStringMethod = typeof(object).GetMethod("ToString", Type.EmptyTypes);
			if (toStringMethod == null)
			{
				throw new Exception("ToString method not found");
			}

			return toStringMethod;
		}

		// Working context implementation
		//ParameterExpression it;

		private int _parsePosition;
		private readonly string _expressionText;
		private readonly int _expressionTextLength;
		private char _parseChar;
		private Token _token;

		private readonly BindingFlags _bindingCase;
		private readonly MemberFilter _memberFilterCase;

		private readonly DefaultNumberType _defaultNumberType;

		private Parser(ParserArguments arguments)
		{
			_arguments = arguments;

			_bindingCase = arguments.Settings.CaseInsensitive ? BindingFlags.IgnoreCase : BindingFlags.Default;
			_memberFilterCase = arguments.Settings.CaseInsensitive ? Type.FilterNameIgnoreCase : Type.FilterName;

			_defaultNumberType = arguments.Settings.DefaultNumberType;

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
				return GenerateConversion(expression, returnType, errorPos);
			}

			return expression;
		}

		private Expression ParseExpressionSegment()
		{
			// The following methods respect the operator precedence as defined in
			// MSDN C# "Operator precedence and associativity"
			// https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/

			try
			{
				if (_arguments.Settings.LambdaExpressions)
				{
					var lambdaExpr = ParseLambdaExpression();
					if (lambdaExpr != null)
						return lambdaExpr;
				}

				return ParseAssignment();
			}
			catch (InvalidOperationException ex)
			{
				throw WrapWithParseException(_token.pos, ErrorMessages.InvalidOperation, ex);
			}
		}

		// => operator
		private Expression ParseLambdaExpression()
		{
			// in case the expression is not a lambda, we have to restart parsing
			var originalPos = _token.pos;

			try
			{
				var parameters = ParseLambdaParameterList();
				ValidateToken(TokenId.LambdaArrow);

				var startExpr = _parsePosition;

				var parenCount = 0;
				var inLambdaBody = true;
				while (inLambdaBody)
				{
					NextToken();
					if (_token.id == TokenId.End)
						inLambdaBody = false;
					if (_token.id == TokenId.OpenParen)
						parenCount++;
					if (_token.id == TokenId.CloseParen)
						parenCount--;

					// lambda is a function parameter
					if (parenCount == 0 && _token.id == TokenId.Comma)
						inLambdaBody = false;

					// body closure
					if (parenCount < 0)
						inLambdaBody = false;
				}

				var lambdaBodyExp = _expressionText.Substring(startExpr, _token.pos - startExpr);
				return new InterpreterExpression(_arguments, lambdaBodyExp, parameters);
			}
			catch (ParseException)
			{
				// not a lambda, return to the saved position
				SetTextPos(originalPos);
				NextToken();

				return null;
			}
		}

		private Parameter[] ParseLambdaParameterList()
		{
			var hasOpenParen = _token.id == TokenId.OpenParen;
			if (hasOpenParen)
				NextToken();

			var parameters = _token.id != TokenId.CloseParen ? ParseLambdaParameters() : new Parameter[0];
			if (hasOpenParen)
			{
				ValidateToken(TokenId.CloseParen, ErrorMessages.CloseParenOrCommaExpected);
				NextToken();
			}

			return parameters;
		}

		private Parameter[] ParseLambdaParameters()
		{
			var argList = new List<Parameter>();
			while (true)
			{
				argList.Add(ParseLambdaParameter());
				if (_token.id != TokenId.Comma) break;
				NextToken();
			}
			return argList.ToArray();
		}

		private Parameter ParseLambdaParameter()
		{
			ValidateToken(TokenId.Identifier);
			var name = _token.text;

			if (TryParseKnownType(name, out var type))
			{
				ValidateToken(TokenId.Identifier);
				name = _token.text;
			}
			else
			{
				type = typeof(object);
			}

			NextToken();
			return new Parameter(name, type);
		}

		// = operator
		private Expression ParseAssignment()
		{
			var left = ParseConditional();
			if (_token.id == TokenId.Equal)
			{
				if (!_arguments.Settings.AssignmentOperators.HasFlag(AssignmentOperators.AssignmentEqual))
					throw new AssignmentOperatorDisabledException("=", _token.pos);


				if (!IsWritable(left))
					throw CreateParseException(_token.pos, ErrorMessages.ExpressionMustBeWritable);

				NextToken();
				var right = ParseAssignment();

				var promoted = PromoteExpression(right, left.Type, true);
				if (promoted == null)
					throw CreateParseException(_token.pos, ErrorMessages.CannotConvertValue,
						GetTypeName(right.Type), GetTypeName(left.Type));

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
				expr = GenerateConditional(GenerateEqual(expr, ParserConstants.NullLiteralExpression), exprRight, expr, errorPos);
			}
			else if (_token.id == TokenId.Question)
			{
				NextToken();
				var expr1 = ParseExpressionSegment();
				ValidateToken(TokenId.Colon, ErrorMessages.ColonExpected);
				NextToken();
				var expr2 = ParseExpressionSegment();
				expr = GenerateConditional(expr, expr1, expr2, errorPos);
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
				CheckAndPromoteOperands(typeof(ParseSignatures.ILogicalSignatures), ref left, ref right);
				left = GenerateBinary(ExpressionType.OrElse, left, right);
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
				CheckAndPromoteOperands(typeof(ParseSignatures.ILogicalSignatures), ref left, ref right);
				left = GenerateBinary(ExpressionType.AndAlso, left, right);
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
				CheckAndPromoteOperands(typeof(ParseSignatures.ILogicalSignatures), ref left, ref right);
				left = GenerateBinary(ExpressionType.Or, left, right);
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
				CheckAndPromoteOperands(typeof(ParseSignatures.ILogicalSignatures), ref left, ref right);
				left = GenerateBinary(ExpressionType.ExclusiveOr, left, right);
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
				CheckAndPromoteOperands(typeof(ParseSignatures.ILogicalSignatures), ref left, ref right);
				left = GenerateBinary(ExpressionType.And, left, right);
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
				var right = ParseShift();
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
				//			throw CreateParseException(op.pos, ErrorMessages.IncompatibleOperands,
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
				//			throw CreateParseException(op.pos, ErrorMessages.IncompatibleOperands,
				//				op.text, GetTypeName(left.Type), GetTypeName(right.Type));
				//		}
				//	}
				//}
				//else
				//{
				//	CheckAndPromoteOperands(isEquality ? typeof(ParseSignatures.IEqualitySignatures) : typeof(ParseSignatures.IRelationalSignatures),
				//			op.text, ref left, ref right, op.pos);
				//}

				CheckAndPromoteOperands(
					isEquality ? typeof(ParseSignatures.IEqualitySignatures) : typeof(ParseSignatures.IRelationalSignatures),
					ref left,
					ref right);

				switch (op.id)
				{
					case TokenId.DoubleEqual:
						left = GenerateEqual(left, right);
						break;
					case TokenId.ExclamationEqual:
						left = GenerateNotEqual(left, right);
						break;
					case TokenId.GreaterThan:
						left = GenerateGreaterThan(left, right);
						break;
					case TokenId.GreaterThanEqual:
						left = GenerateGreaterThanEqual(left, right);
						break;
					case TokenId.LessThan:
						left = GenerateLessThan(left, right);
						break;
					case TokenId.LessThanEqual:
						left = GenerateLessThanEqual(left, right);
						break;
				}
			}
			return left;
		}

		// is, as operators
		private Expression ParseTypeTesting()
		{
			var left = ParseShift();
			while (_token.text == ParserConstants.KeywordIs
			       || _token.text == ParserConstants.KeywordAs)
			{
				var typeOperator = _token.text;

				var op = _token;
				NextToken();

				Type knownType;
				if (!TryParseKnownType(_token.text, out knownType))
					throw CreateParseException(op.pos, ErrorMessages.TypeIdentifierExpected);

				if (typeOperator == ParserConstants.KeywordIs)
					left = Expression.TypeIs(left, knownType);
				else if (typeOperator == ParserConstants.KeywordAs)
					left = Expression.TypeAs(left, knownType);
				else
					throw CreateParseException(_token.pos, ErrorMessages.SyntaxError);

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
							left = GenerateStringConcat(left, right);
						}
						else
						{
							CheckAndPromoteOperands(typeof(ParseSignatures.IAddSignatures), ref left, ref right);
							left = GenerateBinary(ExpressionType.Add, left, right);
						}
						break;
					case TokenId.Minus:
						CheckAndPromoteOperands(typeof(ParseSignatures.ISubtractSignatures), ref left, ref right);
						left = GenerateBinary(ExpressionType.Subtract, left, right);
						break;
				}
			}
			return left;
		}

		// << , >> operators
		private Expression ParseShift()
		{
			var left = ParseAdditive();
			while (IsShiftOperator(out var shiftType))
			{
				NextToken();
				var right = ParseAdditive();
				CheckAndPromoteOperands(typeof(ParseSignatures.IShiftSignatures), ref left, ref right);
				left = GenerateBinary(shiftType, left, right);
			}
			return left;
		}

		/// <summary>
		/// Returns true if and only if the current token is in fact a shift operator.
		/// In that case <paramref name="shiftType"/> is set to the proper expression type.
		/// If the function returns false, <paramref name="shiftType"/> shouldn't be used.
		/// </summary>
		public bool IsShiftOperator(out ExpressionType shiftType)
		{
			// >> is not a token, because it conflicts with generics such as List<List<int>>
			if (_token.id == TokenId.GreaterThan && _parseChar == '>')
			{
				NextToken(); // consume next >
				shiftType = ExpressionType.RightShift;
				return true;
			}
			// << could be a token, but is not for symmetry
			else if (_token.id == TokenId.LessThan && _parseChar == '<')
			{
				NextToken(); // consume next < 
				shiftType = ExpressionType.LeftShift;
				return true;
			}

			// dummy expression type that shouldn't be used
			shiftType = ExpressionType.DebugInfo;
			return false;
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

				CheckAndPromoteOperands(typeof(ParseSignatures.IArithmeticSignatures), ref left, ref right);

				switch (op.id)
				{
					case TokenId.Asterisk:
						left = GenerateBinary(ExpressionType.Multiply, left, right);
						break;
					case TokenId.Slash:
						left = GenerateBinary(ExpressionType.Divide, left, right);
						break;
					case TokenId.Percent:
						left = GenerateBinary(ExpressionType.Modulo, left, right);
						break;
				}
			}
			return left;
		}

		// +,-, ! unary operators
		private Expression ParseUnary()
		{
			if (_token.id == TokenId.Minus || _token.id == TokenId.Plus ||
				_token.id == TokenId.Exclamation || _token.id == TokenId.Tilde)
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
					CheckAndPromoteOperand(typeof(ParseSignatures.INegationSignatures), ref expr);
					expr = GenerateUnary(ExpressionType.Negate, expr);
				}
				else if (op.id == TokenId.Plus)
				{

				}
				else if (op.id == TokenId.Exclamation)
				{
					CheckAndPromoteOperand(typeof(ParseSignatures.INotSignatures), ref expr);
					expr = GenerateUnary(ExpressionType.Not, expr);
				}
				else if (op.id == TokenId.Tilde)
				{
					CheckAndPromoteOperand(typeof(ParseSignatures.IBitwiseComplementSignatures), ref expr);
					expr = GenerateUnary(ExpressionType.OnesComplement, expr);
				}
				return expr;
			}
			return ParsePrimary();
		}

		private Expression GenerateUnary(ExpressionType unaryType, Expression expr)
		{
			if (IsDynamicExpression(expr))
				return GenerateUnaryDynamic(unaryType, expr);

			// find the overloaded unary operator
			string opName;
			switch (unaryType)
			{
				case ExpressionType.Negate: opName = "op_UnaryNegation"; break;
				case ExpressionType.Not: opName = "op_LogicalNot"; break;
				case ExpressionType.OnesComplement: opName = "op_OnesComplement"; break;
				default: opName = null; break;
			}

			var applicableMethod = FindUnaryOperator(opName, expr);

			MethodInfo operatorMethod = null;
			if (applicableMethod != null)
			{
				operatorMethod = applicableMethod.MethodBase as MethodInfo;
				expr = applicableMethod.PromotedParameters[0];
			}

			// if no operator was found, the default Linq resolution will occur
			return Expression.MakeUnary(unaryType, expr, null, operatorMethod);
		}

		private Expression GenerateUnaryDynamic(ExpressionType unaryType, Expression expr)
		{
			var binder = Microsoft.CSharp.RuntimeBinder.Binder.UnaryOperation(
				CSharpBinderFlags.None,
				unaryType,
				typeof(Parser),
				new CSharpArgumentInfo[] {
				CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)}
				);

			return Expression.Dynamic(binder, typeof(object), expr);
		}

		private MethodData FindUnaryOperator(string operatorName, Expression expr)
		{
			if (operatorName == null)
				return null;

			var errorPos = _token.pos;
			var type = expr.Type;
			var args = new[] { expr };

			// try to find the user defined operator on both operands
			var applicableMethods = FindMethods(type, operatorName, true, args);
			if (applicableMethods.Length > 1)
				throw CreateParseException(errorPos, ErrorMessages.AmbiguousUnaryOperatorInvocation, operatorName, GetTypeName(type));

			MethodData userDefinedOperator = null;
			if (applicableMethods.Length == 1)
				userDefinedOperator = applicableMethods[0];

			return userDefinedOperator;
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
					expr = ParseMemberAccess(expr);
				}
				// special case for ?. and ?[ operators
				else if (_token.id == TokenId.Question && (_parseChar == '.' || _parseChar == '['))
				{
					NextToken();

					if (_token.id == TokenId.Dot)
					{
						NextToken();

						// ?. operator changes value types to nullable types
						// the member access should be resolved on the underlying type
						var memberAccess = GenerateNullableTypeConversion(ParseMemberAccess(GenerateGetNullableValue(expr)));
						var nullExpr = ParserConstants.NullLiteralExpression;
						CheckAndPromoteOperands(typeof(ParseSignatures.IEqualitySignatures), ref expr, ref nullExpr);
						expr = GenerateConditional(GenerateEqual(expr, nullExpr), ParserConstants.NullLiteralExpression, memberAccess, _token.pos);
					}
					else if (_token.id == TokenId.OpenBracket)
					{
						// ?[ operator changes value types to nullable types
						// the member access should be resolved on the underlying type
						var elementAccess = GenerateNullableTypeConversion(ParseElementAccess(GenerateGetNullableValue(expr)));
						var nullExpr = ParserConstants.NullLiteralExpression;
						CheckAndPromoteOperands(typeof(ParseSignatures.IEqualitySignatures), ref expr, ref nullExpr);
						expr = GenerateConditional(GenerateEqual(expr, nullExpr), ParserConstants.NullLiteralExpression, elementAccess, _token.pos);
					}
				}
				else if (_token.id == TokenId.OpenBracket)
				{
					expr = ParseElementAccess(expr);
				}
				else if (_token.id == TokenId.OpenParen)
				{
					if (expr is LambdaExpression lambda)
						return ParseLambdaInvocation(lambda, tokenPos);

					if (expr is MethodGroupExpression methodGroup)
						expr = ParseMethodGroupInvocation(methodGroup, tokenPos);
					else if (typeof(Delegate).IsAssignableFrom(expr.Type))
						expr = ParseDelegateInvocation(expr, tokenPos);
					else
						throw CreateParseException(tokenPos, ErrorMessages.InvalidMethodCall, GetTypeName(expr.Type));
				}
				else
				{
					break;
				}
			}
			return expr;
		}

		/// <summary>
		/// Generate a call to the Value property of the Nullable type */
		/// </summary>
		private Expression GenerateGetNullableValue(Expression expr)
		{
			if (!IsNullableType(expr.Type))
				return expr;
			return GeneratePropertyOrFieldExpression(expr.Type, expr, _token.pos, "Value");
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
					throw CreateParseException(_token.pos, ErrorMessages.ExpressionExpected);
			}
		}

		private Expression ParseCharLiteral()
		{
			ValidateToken(TokenId.CharLiteral);
			var s = _token.text.Substring(1, _token.text.Length - 2);

			s = EvalEscapeStringLiteral(s);

			if (s.Length != 1)
				throw CreateParseException(_token.pos, ErrorMessages.InvalidCharacterLiteral);

			NextToken();
			return CreateLiteral(s[0]);
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
			return CreateLiteral(s);
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
						throw CreateParseException(_token.pos, ErrorMessages.InvalidEscapeSequence);

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
					throw CreateParseException(_token.pos, ErrorMessages.InvalidEscapeSequence);
			}
		}

		private Expression ParseIntegerLiteral()
		{
			ValidateToken(TokenId.IntegerLiteral);
			var text = _token.text;

			var isUnsigned = false;
			var isLong = false;
			var numberEnd = text.Length - 1;
			for (; numberEnd >= 0; numberEnd--)
			{
				var chr = text[numberEnd];
				if (chr == 'U' || chr == 'u')
					isUnsigned = true;
				else if (chr == 'L' || chr == 'l')
					isLong = true;
				else
					break;
			}

			text = text.Substring(0, numberEnd + 1);

			// No suffix find, verify if DefaultNumberType.Long is specified
			if (_defaultNumberType == DefaultNumberType.Long) isLong = true;

			if (text[0] != '-')
			{
				if (!ulong.TryParse(text, ParseLiteralUnsignedNumberStyle, ParseCulture, out ulong value))
					throw CreateParseException(_token.pos, ErrorMessages.InvalidIntegerLiteral, text);

				NextToken();

				if (!isUnsigned && !isLong && value <= int.MaxValue)
					return CreateLiteral((int)value);
				if (!isLong && value <= uint.MaxValue)
					return CreateLiteral((uint)value);
				if (!isUnsigned && value <= long.MaxValue)
					return CreateLiteral((long)value);

				return CreateLiteral(value);
			}
			else
			{
				if (!long.TryParse(text, ParseLiteralNumberStyle, ParseCulture, out long value))
					throw CreateParseException(_token.pos, ErrorMessages.InvalidIntegerLiteral, text);

				NextToken();

				if (!isLong && value >= int.MinValue && value <= int.MaxValue)
					return CreateLiteral((int)value);

				return CreateLiteral(value);
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
			else if (last == 'D' || last == 'd')
			{
				if (double.TryParse(text.Substring(0, text.Length - 1), ParseLiteralDoubleNumberStyle, ParseCulture, out double d))
					value = d;
			}
			else
			{
				// No suffix find, use DefaultNumberType settigns if specified (Double default)
				if (_defaultNumberType == DefaultNumberType.Decimal)
				{
					if (decimal.TryParse(text, ParseLiteralDecimalNumberStyle, ParseCulture, out decimal dc))
						value = dc;
				}
				else if (_defaultNumberType == DefaultNumberType.Single)
				{
					if (float.TryParse(text, ParseLiteralDecimalNumberStyle, ParseCulture, out float f))
						value = f;
				}
				else
				{
					if (double.TryParse(text, ParseLiteralDoubleNumberStyle, ParseCulture, out double d))
						value = d;
				}
			}

			if (value == null)
				throw CreateParseException(_token.pos, ErrorMessages.InvalidRealLiteral, text);

			NextToken();

			return CreateLiteral(value);
		}

		private static Expression CreateLiteral(object value)
		{
			var expr = Expression.Constant(value);

			return expr;
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
			if (_token.text == ParserConstants.KeywordDefault)
				return ParseDefaultOperator();

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

			if (TryParseKnownType(_token.text, out Type knownType))
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
				throw CreateParseException(errorPos, ErrorMessages.TypeofRequiresOneArg);

			var constExp = args[0] as ConstantExpression;
			if (constExp == null || !(constExp.Value is Type))
				throw CreateParseException(errorPos, ErrorMessages.TypeofRequiresAType);

			return constExp;
		}

		private Expression ParseDefaultOperator()
		{
			NextToken();

			ValidateToken(TokenId.OpenParen, ErrorMessages.OpenParenExpected);
			NextToken();
			ValidateToken(TokenId.Identifier);
			var type = ParseKnownType();
			ValidateToken(TokenId.CloseParen, ErrorMessages.CloseParenOrCommaExpected);
			NextToken();

			return Expression.Default(type);
		}

		private Expression GenerateConditional(Expression test, Expression expr1, Expression expr2, int errorPos)
		{
			if (IsDynamicExpression(test))
				return GenerateConditionalDynamic(test, expr1, expr2,errorPos);

			if (test.Type != typeof(bool))
				throw CreateParseException(errorPos, ErrorMessages.FirstExprMustBeBool);
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
						throw CreateParseException(errorPos, ErrorMessages.BothTypesConvertToOther, type1, type2);

					throw CreateParseException(errorPos, ErrorMessages.NeitherTypeConvertsToOther, type1, type2);
				}
			}
			return Expression.Condition(test, expr1, expr2);
		}

		private Expression GenerateConditionalDynamic(Expression test, Expression expr1, Expression expr2, int errorPos)
		{

			var casted = Expression.Convert(test, typeof(bool));

			return GenerateConditional(casted, expr1, expr2, errorPos);

		}

		private Expression ParseNew()
		{
			NextToken();
			ValidateToken(TokenId.Identifier, ErrorMessages.IdentifierExpected);

			var newType = ParseKnownType();
			var args = new Expression[0];
			if (_token.id == TokenId.OpenParen)
				args = ParseArgumentList();
			else
			{
				// no aguments: expect an object initializer
				ValidateToken(TokenId.OpenCurlyBracket, ErrorMessages.OpenCurlyBracketExpected);
			}

			var applicableConstructors = FindBestMethod(newType.GetConstructors(), args);
			if (applicableConstructors.Length == 0)
				throw CreateParseException(_token.pos, ErrorMessages.NoApplicableConstructor, newType);

			if (applicableConstructors.Length > 1)
				throw CreateParseException(_token.pos, ErrorMessages.AmbiguousConstructorInvocation, newType);

			var constructor = applicableConstructors[0];
			var newExpr = Expression.New((ConstructorInfo)constructor.MethodBase, constructor.PromotedParameters);

			var memberBindings = new MemberBinding[0];
			if (_token.id == TokenId.OpenCurlyBracket)
				memberBindings = ParseObjectInitializer(newType);

			return Expression.MemberInit(newExpr, memberBindings);
		}

		private MemberBinding[] ParseObjectInitializer(Type newType)
		{
			ValidateToken(TokenId.OpenCurlyBracket, ErrorMessages.OpenCurlyBracketExpected);
			NextToken();
			var bindings = ParseMemberInitializerList(newType);
			ValidateToken(TokenId.CloseCurlyBracket, ErrorMessages.CloseCurlyBracketExpected);
			NextToken();
			return bindings;
		}

		private MemberBinding[] ParseMemberInitializerList(Type newType)
		{
			var bindingList = new List<MemberBinding>();
			while (true)
			{
				if (_token.id == TokenId.CloseCurlyBracket) break;
				ValidateToken(TokenId.Identifier, ErrorMessages.IdentifierExpected);

				var propertyOrFieldName = _token.text;
				var member = FindPropertyOrField(newType, propertyOrFieldName, false);
				if (member == null)
					throw CreateParseException(_token.pos, ErrorMessages.UnknownPropertyOrField, propertyOrFieldName, GetTypeName(newType));

				NextToken();

				ValidateToken(TokenId.Equal, ErrorMessages.EqualExpected);
				NextToken();

				var value = ParseExpressionSegment();
				bindingList.Add(Expression.Bind(member, value));

				if (_token.id != TokenId.Comma) break;
				NextToken();
			}
			return bindingList.ToArray();
		}

		private Expression ParseLambdaInvocation(LambdaExpression lambda, int errorPos)
		{
			return ParseInvocation(lambda, errorPos, ErrorMessages.ArgsIncompatibleWithLambda);
		}

		private Expression ParseDelegateInvocation(Expression delegateExp, int errorPos)
		{
			return ParseInvocation(delegateExp, errorPos, ErrorMessages.ArgsIncompatibleWithDelegate);
		}

		private Expression ParseInvocation(Expression expr, int errorPos, string error)
		{
			var args = ParseArgumentList();

			var invokeMethod = FindInvokeMethod(expr.Type);
			if (invokeMethod == null || !CheckIfMethodIsApplicableAndPrepareIt(invokeMethod, args))
				throw CreateParseException(errorPos, error);

			return Expression.Invoke(expr, invokeMethod.PromotedParameters);
		}

		private Expression ParseMethodGroupInvocation(MethodGroupExpression methodGroup, int errorPos)
		{
			var args = ParseArgumentList();

			// find the best delegates that can be used with the provided arguments
			var candidates = methodGroup.Overloads
				.Select(_ => new
				{
					Delegate = _,
					Method = _.Method,
					InvokeMethod = FindInvokeMethod(_.GetType()),
				})
				.ToList();

			var applicableMethods = FindBestMethod(candidates.Select(_ => _.InvokeMethod), args);

			// no method found: retry with the delegate's method
			// (the parameters might be different, e.g. params array, default value, etc)
			if (applicableMethods.Length == 0)
				applicableMethods = FindBestMethod(candidates.Select(_ => _.Method), args);

			if (applicableMethods.Length == 0)
				throw CreateParseException(errorPos, ErrorMessages.ArgsIncompatibleWithDelegate);

			if (applicableMethods.Length > 1)
				throw CreateParseException(errorPos, ErrorMessages.AmbiguousDelegateInvocation);

			var applicableMethod = applicableMethods[0];
			var usedDeledate = candidates.Single(_ => new[] { _.Method, _.InvokeMethod?.MethodBase }.Any(m => m == applicableMethod.MethodBase)).Delegate;
			return Expression.Invoke(Expression.Constant(usedDeledate), applicableMethod.PromotedParameters);
		}

		private Type ParseKnownType()
		{
			var name = _token.text;
			var errorPos = _token.pos;
			if (!TryParseKnownType(name, out var type))
				throw new UnknownIdentifierException(name, errorPos);

			return type;
		}

		private bool TryParseKnownType(string name, out Type type)
		{
			// if the type is unknown, we need to restart parsing 
			var originalPos = _token.pos;
			_arguments.TryGetKnownType(name, out type);

			NextToken();
			if (_token.id == TokenId.LessThan)
			{
				var typeArguments = ParseTypeArgumentList();
				var rank = typeArguments.Count;

				// if no type was registered with the simple name, try the full generic name
				if (type == null && !_arguments.TryGetKnownType(name + $"`{rank}", out type))
					return false;

				if (rank != type.GetGenericArguments().Length)
					throw new ArgumentException($"The number of generic arguments provided doesn't equal the arity of the generic type definition.");

				// there are actual type arguments: instantiate the proper generic type
				if (typeArguments.All(_ => _ != null))
					type = type.MakeGenericType(typeArguments.ToArray());

				NextToken();
			}

			type = ParseTypeModifiers(type);
			if (type == null)
			{
				// type name couldn't be parsed: restore position
				SetTextPos(originalPos);
				NextToken();

				return false;
			}

			return true;
		}

		// we found a known type identifier, check if it has some modifiers
		private Type ParseTypeModifiers(Type type)
		{
			// type modifiers require the base type to be known
			if (type == null)
				return null;

			var errorPos = _token.pos;
			if (_token.id == TokenId.Question)
			{
				if (!type.IsValueType || IsNullableType(type))
					throw CreateParseException(errorPos, ErrorMessages.TypeHasNoNullableForm, GetTypeName(type));
				type = typeof(Nullable<>).MakeGenericType(type);

				NextToken();
				type = ParseTypeModifiers(type);
			}
			else if (_token.id == TokenId.OpenBracket)
			{
				type = ParseArrayRankSpecifier(type);
			}

			return type;
		}

		private Type ParseArrayRankSpecifier(Type type)
		{
			ValidateToken(TokenId.OpenBracket);

			// An array type of the form T[R][R1]...[Rn] is an array with rank R and an element type T[R1]...[Rn]
			// => we need to parse all rank specifiers in one pass, and create the array from right to left
			var ranks = new Stack<int>();
			while (_token.id == TokenId.OpenBracket)
			{
				NextToken();
				var rank = 1;
				while (_token.id == TokenId.Comma)
				{
					rank++;
					NextToken();
				}

				ValidateToken(TokenId.CloseBracket, ErrorMessages.CloseBracketOrCommaExpected);
				ranks.Push(rank);
				NextToken();
			}

			while (ranks.Count > 0)
			{
				var rank = ranks.Pop();
				type = rank == 1 ? type.MakeArrayType() : type.MakeArrayType(rank);
			}

			return type;
		}

		private List<Type> ParseTypeArgumentList()
		{
			ValidateToken(TokenId.LessThan);
			NextToken();

			List<Type> args;
			if (_token.id == TokenId.Identifier)
				args = ParseTypeArguments();
			else
			{
				var arity = ParseUnboundTypeArity();
				args = new List<Type>(new Type[arity]);
			}

			ValidateToken(TokenId.GreaterThan, ErrorMessages.CloseTypeArgumentListExpected);
			return args;
		}

		private List<Type> ParseTypeArguments()
		{
			var genericArguments = new List<Type>();
			while (true)
			{
				ValidateToken(TokenId.Identifier);
				genericArguments.Add(ParseKnownType());
				if (_token.id != TokenId.Comma) break;
				NextToken();
			}

			return genericArguments;
		}

		private int ParseUnboundTypeArity()
		{
			var rank = 1;
			while (_token.id == TokenId.Comma)
			{
				rank++;
				NextToken();
			}

			return rank;
		}

		private Expression ParseTypeKeyword(Type type)
		{
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

		private Expression GenerateConversion(Expression expr, Type type, int errorPos)
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

			// generic type was not fully resolved; try to find a type in the inheritance hierarchy
			// that matches the generic type definition
			if (type.ContainsGenericParameters)
			{
				var typeDef = type.GetGenericTypeDefinition();
				var interfaces = exprType.GetInterfaces().ToList();
				for (var nextType = exprType.BaseType; nextType != null; nextType = nextType.BaseType)
				{
					interfaces.Add(nextType);
				}

				foreach (var inherited in interfaces)
				{
					if (inherited.IsGenericType && inherited.GetGenericTypeDefinition() == typeDef)
						type = inherited;
				}
			}

			try
			{
				if (expr is InterpreterExpression ie)
				{
					return ie.EvalAs(type);
				}

				return Expression.ConvertChecked(expr, type);
			}
			catch (InvalidOperationException)
			{
				throw CreateParseException(errorPos, ErrorMessages.CannotConvertValue,
					GetTypeName(exprType), GetTypeName(type));
			}
		}

		private Expression ParseMemberAccess(Expression instance)
		{
			return ParseMemberAccess(null, instance);
		}

		private Expression ParseMemberAccess(Type type, Expression instance)
		{
			if (instance != null) type = instance.Type;
			var errorPos = _token.pos;
			var id = GetIdentifier();
			NextToken();
			if (_token.id == TokenId.OpenParen)
				return ParseMethodInvocation(type, instance, errorPos, id);

			return GeneratePropertyOrFieldExpression(type, instance, errorPos, id);
		}

		private Expression GeneratePropertyOrFieldExpression(Type type, Expression instance, int errorPos, string propertyOrFieldName)
		{
			var member = FindPropertyOrField(type, propertyOrFieldName, instance == null);
			if (member != null)
			{
				return member is PropertyInfo ?
					Expression.Property(instance, (PropertyInfo)member) :
					Expression.Field(instance, (FieldInfo)member);
			}

			if (IsDynamicType(type) || IsDynamicExpression(instance))
				return ParseDynamicProperty(type, instance, propertyOrFieldName);

			throw CreateParseException(errorPos, ErrorMessages.UnknownPropertyOrField, propertyOrFieldName, GetTypeName(type));
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

			if (IsDynamicType(type) || IsDynamicExpression(instance))
				return ParseDynamicMethodInvocation(type, instance, methodName, args);

			throw new NoApplicableMethodException(methodName, GetTypeName(type), errorPos);
		}

		private Expression ParseExtensionMethodInvocation(Type type, Expression instance, int errorPos, string id, Expression[] args)
		{
			var extensionMethodsArguments = new Expression[args.Length + 1];
			extensionMethodsArguments[0] = instance;
			args.CopyTo(extensionMethodsArguments, 1);

			var extensionMethods = FindExtensionMethods(id, extensionMethodsArguments);
			if (extensionMethods.Length > 1)
				throw CreateParseException(errorPos, ErrorMessages.AmbiguousMethodInvocation, id, GetTypeName(type));

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
			var applicableMethods = FindMethods(type, id, instance == null, args);
			if (applicableMethods.Length > 1)
				throw CreateParseException(errorPos, ErrorMessages.AmbiguousMethodInvocation, id, GetTypeName(type));

			if (applicableMethods.Length == 1)
			{
				var method = applicableMethods[0];

				return Expression.Call(instance, (MethodInfo)method.MethodBase, method.PromotedParameters);
			}

			return null;
		}

		//static Type FindGenericType(Type generic, Type type)
		//{
		//	while (type != null && type != typeof(object))
		//	{
		//		if (type.IsGenericType && type.GetGenericTypeDefinition() == generic) return type;
		//		if (generic.IsInterface)
		//		{
		//			foreach (Type intfType in type.GetInterfaces())
		//			{
		//				Type found = FindGenericType(generic, intfType);
		//				if (found != null) return found;
		//			}
		//		}
		//		type = type.BaseType;
		//	}
		//	return null;
		//}

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

		private static Expression ParseDynamicProperty(Type type, Expression instance, string propertyOrFieldName)
		{
			var binder = Microsoft.CSharp.RuntimeBinder.Binder.GetMember(
				Microsoft.CSharp.RuntimeBinder.CSharpBinderFlags.None,
				propertyOrFieldName,
				type,
				new[] { Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo.Create(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfoFlags.None, null) }
			);

			return Expression.Dynamic(binder, typeof(object), instance);
		}

		private static Expression ParseDynamicMethodInvocation(Type type, Expression instance, string methodName, Expression[] args)
		{
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
		}

		private static Expression ParseDynamicIndex(Type type, Expression instance, Expression[] args)
		{
			var argsDynamic = args.ToList();
			argsDynamic.Insert(0, instance);
			var binder = Microsoft.CSharp.RuntimeBinder.Binder.GetIndex(
				Microsoft.CSharp.RuntimeBinder.CSharpBinderFlags.None,
				type,
				argsDynamic.Select(x => Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo.Create(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfoFlags.None, null))
			);
			return Expression.Dynamic(binder, typeof(object), argsDynamic);
		}

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
					throw CreateParseException(errorPos, ErrorMessages.IncorrectNumberOfIndexes);

				for (int i = 0; i < args.Length; i++)
				{
					args[i] = PromoteExpression(args[i], typeof(int), true);
					if (args[i] == null)
						throw CreateParseException(errorPos, ErrorMessages.InvalidIndex);
				}

				return Expression.ArrayAccess(expr, args);
			}

			if (IsDynamicType(expr.Type) || IsDynamicExpression(expr))
				return ParseDynamicIndex(expr.Type, expr, args);

			var applicableMethods = FindIndexer(expr.Type, args);
			if (applicableMethods.Length == 0)
			{
				throw CreateParseException(errorPos, ErrorMessages.NoApplicableIndexer,
					GetTypeName(expr.Type));
			}

			if (applicableMethods.Length > 1)
			{
				throw CreateParseException(errorPos, ErrorMessages.AmbiguousIndexerInvocation,
					GetTypeName(expr.Type));
			}

			var indexer = (IndexerData)applicableMethods[0];
			return Expression.Property(expr, indexer.Indexer, indexer.PromotedParameters);
		}

		private static bool IsNullableType(Type type)
		{
			return Nullable.GetUnderlyingType(type) != null;
		}

		private static bool IsDynamicType(Type type)
		{
			return typeof(IDynamicMetaObjectProvider).IsAssignableFrom(type);
		}

		private bool IsDynamicExpression(Expression instance)
		{
			return instance != null &&
				(instance.NodeType == ExpressionType.Dynamic ||
				(_arguments.Settings.LateBindObject && instance.Type == typeof(object)));
		}

		private static Type GetNonNullableType(Type type)
		{
			return IsNullableType(type) ? type.GetGenericArguments()[0] : type;
		}

		private static string GetTypeName(Type type)
		{
			var baseType = GetNonNullableType(type);
			var s = baseType.Name;
			if (type != baseType) s += '?';
			return s;
		}

		static bool IsNumericType(Type type)
		{
			return GetNumericTypeKind(type) != 0;
		}

		private static bool IsSignedIntegralType(Type type)
		{
			return GetNumericTypeKind(type) == 2;
		}

		private static bool IsUnsignedIntegralType(Type type)
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

		//static bool IsEnumType(Type type)
		//{
		//	return GetNonNullableType(type).IsEnum;
		//}

		private void CheckAndPromoteOperand(Type signatures, ref Expression expr)
		{
			var args = new[] { expr };

			args = PrepareOperandArguments(signatures, args);

			expr = args[0];
		}

		private void CheckAndPromoteOperands(Type signatures, ref Expression left, ref Expression right)
		{
			if ((IsNullableType(left.Type) || IsNullableType(right.Type)) && (GetNonNullableType(left.Type) == right.Type || GetNonNullableType(right.Type) == left.Type))
			{
				left = GenerateNullableTypeConversion(left);
				right = GenerateNullableTypeConversion(right);
			}

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

			foreach (var t in SelfAndBaseTypes(type))
			{
				var members = t.FindMembers(MemberTypes.Property | MemberTypes.Field, flags, _memberFilterCase, memberName);
				if (members.Length != 0)
					return members[0];
			}
			return null;
		}

		private MethodData[] FindMethods(Type type, string methodName, bool staticAccess, Expression[] args)
		{
			var flags = BindingFlags.Public | BindingFlags.DeclaredOnly |
			            (staticAccess ? BindingFlags.Static : BindingFlags.Instance) | _bindingCase;
			foreach (var t in SelfAndBaseTypes(type))
			{
				var members = t.FindMembers(MemberTypes.Method, flags, _memberFilterCase, methodName);
				var applicableMethods = FindBestMethod(members.Cast<MethodBase>(), args);

				if (applicableMethods.Length > 0)
					return applicableMethods;
			}

			return new MethodData[0];
		}

		private MethodData FindInvokeMethod(Type type)
		{
			var flags = BindingFlags.Public | BindingFlags.DeclaredOnly |
						BindingFlags.Instance | _bindingCase;
			foreach (var t in SelfAndBaseTypes(type))
			{
				var method = t.FindMembers(MemberTypes.Method, flags, _memberFilterCase, "Invoke")
					.Cast<MethodBase>()
					.SingleOrDefault();

				if (method != null)
					return MethodData.Gen(method);
			}

			return null;
		}

		private MethodData[] FindExtensionMethods(string methodName, Expression[] args)
		{
			var matchMethods = _arguments.GetExtensionMethods(methodName);

			return FindBestMethod(matchMethods, args);
		}

		private MethodData[] FindIndexer(Type type, Expression[] args)
		{
			foreach (var t in SelfAndBaseTypes(type))
			{
				MemberInfo[] members = t.GetDefaultMembers();
				if (members.Length != 0)
				{
					IEnumerable<MethodData> methods = members.
						OfType<PropertyInfo>().
						Select(p => (MethodData)new IndexerData(p));

					var applicableMethods = FindBestMethod(methods, args);
					if (applicableMethods.Length > 0)
						return applicableMethods;
				}
			}

			return new MethodData[0];
		}

		private static IEnumerable<Type> SelfAndBaseTypes(Type type)
		{
			if (type.IsInterface)
			{
				var types = new List<Type>();
				AddInterface(types, type);

				types.Add(typeof(object));

				return types;
			}
			return SelfAndBaseClasses(type);
		}

		private static IEnumerable<Type> SelfAndBaseClasses(Type type)
		{
			while (type != null)
			{
				yield return type;
				type = type.BaseType;
			}
		}

		private static void AddInterface(List<Type> types, Type type)
		{
			if (!types.Contains(type))
			{
				types.Add(type);
				foreach (Type t in type.GetInterfaces())
				{
					AddInterface(types, t);
				}
			}
		}

		private static MethodData[] FindBestMethod(IEnumerable<MethodBase> methods, Expression[] args)
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
			if (method.Parameters.Count(y => !y.HasDefaultValue && !HasParamsArrayType(y)) > args.Length)
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
						// an interpreter expression can only be matched to a parameter of type Func
						if (currentArgument is InterpreterExpression)
							return false;

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
					var paramsArrayElementType = paramsArrayTypeFound.GetElementType();
					if (paramsArrayElementType.IsGenericParameter)
					{
						paramsArrayPromotedArgument = paramsArrayPromotedArgument ?? new List<Expression>();
						paramsArrayPromotedArgument.Add(currentArgument);
						continue;
					}

					var promoted = PromoteExpression(currentArgument, paramsArrayElementType, true);
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
					throw CreateParseException(-1, ErrorMessages.ParamsArrayTypeNotAnArray);

				if (paramsArrayElementType.IsGenericParameter)
				{
					var actualTypes = paramsArrayPromotedArgument.Select(_ => _.Type).Distinct().ToArray();
					if (actualTypes.Length != 1)
						throw CreateParseException(-1, ErrorMessages.MethodTypeParametersCantBeInferred, method.MethodBase);

					paramsArrayElementType = actualTypes[0];
				}

				promotedArgs.Add(Expression.NewArrayInit(paramsArrayElementType, paramsArrayPromotedArgument));
			}

			// Add default params, if needed.
			promotedArgs.AddRange(method.Parameters.Skip(promotedArgs.Count).Select<ParameterInfo, Expression>(x =>
			{
				if (x.HasDefaultValue)
					return Expression.Constant(x.DefaultValue, x.ParameterType);

				if (HasParamsArrayType(x))
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
						var delegateType = actualParamInfo.ParameterType;
						if (delegateType.GetGenericArguments().Length != ie.Parameters.Count + 1)
							return false;

						method.PromotedParameters[i] = ie.EvalAs(delegateType);

						// we have inferred all types, update the method definition
						genericMethod = MakeGenericMethod(method);
					}
				}

				method.MethodBase = genericMethod;
			}

			return true;
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

			return methodInfo.MakeGenericMethod(genericArgs);
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
						var innerGenericTypes = ExtractActualGenericArguments(requestedType.GetGenericArguments(), actualType.GetGenericArguments());

						foreach (var innerGenericType in innerGenericTypes)
							extractedGenericTypes[innerGenericType.Key] = innerGenericType.Value;
					}
				}
			}

			return extractedGenericTypes;
		}

		private static Expression PromoteExpression(Expression expr, Type type, bool exact)
		{
			if (expr.Type == type) return expr;
			if (expr is ConstantExpression)
			{
				var ce = (ConstantExpression)expr;
				if (ce == ParserConstants.NullLiteralExpression)
				{
					if (!type.IsValueType || IsNullableType(type))
						return Expression.Constant(null, type);
				}
			}

			if (expr is InterpreterExpression ie)
			{
				if (!ie.IsCompatibleWithDelegate(type))
					return null;

				return expr;
			}

			if (type.IsGenericType && !IsNumericType(type))
			{
				var genericType = FindAssignableGenericType(expr.Type, type.GetGenericTypeDefinition());
				if (genericType != null)
					return Expression.Convert(expr, genericType);
			}

			if (IsCompatibleWith(expr.Type, type))
			{
				if (type.IsValueType || exact)
				{
					return Expression.Convert(expr, type);
				}
				return expr;
			}

			return null;
		}

		private static bool IsCompatibleWith(Type source, Type target)
		{
			if (source == target)
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

		private static bool IsWritable(Expression expression)
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
			var better = false;

			// check conversion from argument list to parameter list
			for (int i = 0, m = 0, o = 0; i < args.Length; i++)
			{
				var arg = args[i];
				var methodParam = method.Parameters[m];
				var otherMethodParam = otherMethod.Parameters[o];
				var methodParamType = GetParameterType(methodParam);
				var otherMethodParamType = GetParameterType(otherMethodParam);

				if (methodParamType.ContainsGenericParameters)
					methodParamType = method.PromotedParameters[i].Type;

				if (otherMethodParamType.ContainsGenericParameters)
					otherMethodParamType = otherMethod.PromotedParameters[i].Type;

				var c = CompareConversions(arg.Type, methodParamType, otherMethodParamType);
				if (c < 0)
					return false;
				if (c > 0)
					better = true;

				if (!HasParamsArrayType(methodParam)) m++;
				if (!HasParamsArrayType(otherMethodParam)) o++;
			}

			if (better)
				return true;

			if (!method.MethodBase.IsGenericMethod && otherMethod.MethodBase.IsGenericMethod)
				return true;

			if (!method.HasParamsArray && otherMethod.HasParamsArray)
				return true;

			// if a method has a params parameter, it can have less parameters than the number of arguments
			if (method.HasParamsArray && otherMethod.HasParamsArray && method.Parameters.Length > otherMethod.Parameters.Length)
				return true;

			return better;
		}

		// Return 1 if s -> t1 is a better conversion than s -> t2
		// Return -1 if s -> t2 is a better conversion than s -> t1
		// Return 0 if neither conversion is better
		private static int CompareConversions(Type s, Type t1, Type t2)
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

		private Expression GenerateEqual(Expression left, Expression right)
		{
			return GenerateBinary(ExpressionType.Equal, left, right);
		}

		private Expression GenerateNotEqual(Expression left, Expression right)
		{
			return GenerateBinary(ExpressionType.NotEqual, left, right);
		}

		private Expression GenerateGreaterThan(Expression left, Expression right)
		{
			if (left.Type == typeof(string))
			{
				return Expression.GreaterThan(
					GenerateStaticMethodCall("Compare", left, right),
					Expression.Constant(0)
				);
			}
			return GenerateBinary(ExpressionType.GreaterThan, left, right);
		}

		

		private Expression GenerateGreaterThanEqual(Expression left, Expression right)
		{
			if (left.Type == typeof(string))
			{
				return Expression.GreaterThanOrEqual(
					GenerateStaticMethodCall("Compare", left, right),
					Expression.Constant(0)
				);
			}
			return GenerateBinary(ExpressionType.GreaterThanOrEqual, left, right);
		}

		private Expression GenerateLessThan(Expression left, Expression right)
		{
			if (left.Type == typeof(string))
			{
				return Expression.LessThan(
					GenerateStaticMethodCall("Compare", left, right),
					Expression.Constant(0)
				);
			}

			return GenerateBinary(ExpressionType.LessThan, left, right);
		}

		private Expression GenerateLessThanEqual(Expression left, Expression right)
		{
			if (left.Type == typeof(string))
			{
				return Expression.LessThanOrEqual(
					GenerateStaticMethodCall("Compare", left, right),
					Expression.Constant(0)
				);
			}
			return GenerateBinary(ExpressionType.LessThanOrEqual, left, right);
		}

		private Expression GenerateBinary(ExpressionType binaryType, Expression left, Expression right)
		{

			if (IsDynamicExpression(left) || IsDynamicExpression(right))
				return GenerateBinaryDynamic(binaryType, left, right);

			// find the overloaded binary operator
			string opName;

			var liftToNull = true;
			switch (binaryType)
			{
				case ExpressionType.OrElse: opName = "op_BitwiseOr"; break;
				case ExpressionType.Or: opName = "op_BitwiseOr"; break;
				case ExpressionType.ExclusiveOr: opName = "op_ExclusiveOr"; break;
				case ExpressionType.AndAlso: opName = "op_BitwiseAnd"; break;
				case ExpressionType.And: opName = "op_BitwiseAnd"; break;
				case ExpressionType.Add: opName = "op_Addition"; break;
				case ExpressionType.Subtract: opName = "op_Subtraction"; break;
				case ExpressionType.Multiply: opName = "op_Multiply"; break;
				case ExpressionType.Divide: opName = "op_Division"; break;
				case ExpressionType.Modulo: opName = "op_Modulus"; break;
				case ExpressionType.RightShift: opName = "op_RightShift"; break;
				case ExpressionType.LeftShift: opName = "op_LeftShift"; break;
				case ExpressionType.Equal: opName = "op_Equality"; liftToNull = false; break;
				case ExpressionType.NotEqual: opName = "op_Inequality"; liftToNull = false; break;
				case ExpressionType.GreaterThan: opName = "op_GreaterThan"; liftToNull = false; break;
				case ExpressionType.GreaterThanOrEqual: opName = "op_GreaterThanOrEqual"; liftToNull = false; break;
				case ExpressionType.LessThan: opName = "op_LessThan"; liftToNull = false; break;
				case ExpressionType.LessThanOrEqual: opName = "op_LessThanOrEqual"; liftToNull = false; break;
				default: opName = null; break;
			}

			var applicableMethod = FindBinaryOperator(opName, left, right);

			MethodInfo operatorMethod = null;
			if (applicableMethod != null)
			{
				operatorMethod = applicableMethod.MethodBase as MethodInfo;
				left = applicableMethod.PromotedParameters[0];
				right = applicableMethod.PromotedParameters[1];
			}

			// if no operator was found, the default Linq resolution will occur
			return Expression.MakeBinary(binaryType, left, right, liftToNull, operatorMethod);
		}

		private Expression GenerateBinaryDynamic(ExpressionType binaryType,Expression left, Expression right)
		{
			//binary binder for dynamic type does not support AndAlso and OrElse as valid operations
			if (binaryType == ExpressionType.AndAlso || binaryType == ExpressionType.OrElse)
			{
				if (IsDynamicExpression(left))
					left = Expression.Convert(left, typeof(bool));
				if (IsDynamicExpression(right))
					right = Expression.Convert(right, typeof(bool));

				return Expression.MakeBinary(binaryType, left, right);
			}

			var binder = Microsoft.CSharp.RuntimeBinder.Binder.BinaryOperation(
				CSharpBinderFlags.None,
				binaryType,
				typeof(Parser),
				new CSharpArgumentInfo[] {
				CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
				CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)}
				);

			return Expression.Dynamic(binder, typeof(object), left, right);
		}

		private MethodData FindBinaryOperator(string operatorName, Expression left, Expression right)
		{
			if (operatorName == null)
				return null;

			var errorPos = _token.pos;
			var leftType = left.Type;
			var rightType = right.Type;
			var error = CreateParseException(errorPos, ErrorMessages.AmbiguousBinaryOperatorInvocation, operatorName, GetTypeName(leftType), GetTypeName(rightType));

			var args = new[] { left, right };

			MethodData userDefinedOperator = null;

			// try to find the user defined operator on both operands
			var opOnLeftType = FindMethods(leftType, operatorName, true, args);
			if (opOnLeftType.Length > 1)
				throw error;

			if (opOnLeftType.Length == 1)
				userDefinedOperator = opOnLeftType[0];

			if (leftType != rightType)
			{
				var opOnRightType = FindMethods(rightType, operatorName, true, args);
				if (opOnRightType.Length > 1)
					throw error;

				MethodData rightOperator = null;
				if (opOnRightType.Length == 1)
					rightOperator = opOnRightType[0];

				// we found a matching user defined operator on either type, but it might be the same method
				if (userDefinedOperator != null && rightOperator != null && !ReferenceEquals(userDefinedOperator.MethodBase, rightOperator.MethodBase))
					throw error;

				// we didn't find an operator on the left type, but we found one on the right type
				if (userDefinedOperator == null && rightOperator != null)
					userDefinedOperator = rightOperator;
			}

			return userDefinedOperator;
		}

		private static Expression GenerateStringConcat(Expression left, Expression right)
		{
			var leftObj = GenerateStringConcatOperand(left);
			var rightObj = GenerateStringConcatOperand(right);

			return Expression.Call(
				null,
				_concatMethod,
				new[] { leftObj, rightObj });
		}

		private static Expression ToStringOrNull(Expression expression)
		{
			var nullableExpression = IsNullableType(expression.Type) ?
				expression :
				GenerateNullableTypeConversion(expression);

			var expressionTypeNullConstant = Expression.Constant(null, nullableExpression.Type);
			var stringNullConstant = Expression.Constant(null, typeof(string));

			Expression condition = Expression.Equal(
				nullableExpression,
				expressionTypeNullConstant);

			return Expression.Condition(
				condition,
				stringNullConstant,
				Expression.Call(expression, _toStringMethod));
		}

		private static Expression GenerateStringConcatOperand(Expression expression)
		{
			return expression.Type != typeof(string)
				? ToStringOrNull(expression)
				: expression;
		}

		private static MethodInfo GetStaticMethod(string methodName, Expression left, Expression right)
		{
			return left.Type.GetMethod(methodName, new[] { left.Type, right.Type });
		}

		private static Expression GenerateStaticMethodCall(string methodName, Expression left, Expression right)
		{
			return Expression.Call(null, GetStaticMethod(methodName, left, right), new[] { left, right });
		}

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
				case '~':
					NextChar();
					t = TokenId.Tilde;
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
					else if (_parseChar == '>')
					{
						NextChar();
						t = TokenId.LambdaArrow;
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
					if (_parseChar == '?')
					{
						NextChar();
						t = TokenId.QuestionQuestion;
					}
					else
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
				case '{':
					NextChar();
					t = TokenId.OpenCurlyBracket;
					break;
				case '}':
					NextChar();
					t = TokenId.CloseCurlyBracket;
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
						throw CreateParseException(_parsePosition, ErrorMessages.UnterminatedStringLiteral);

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
						throw CreateParseException(_parsePosition, ErrorMessages.UnterminatedStringLiteral);

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
						//RealLiteral if DefaultNumberType settings is set to real type
						if (_defaultNumberType == DefaultNumberType.Single || _defaultNumberType == DefaultNumberType.Double || _defaultNumberType == DefaultNumberType.Decimal)
						{
							t = TokenId.RealLiteral;
						}
						else
						{
							//IntegerLiteral by default
							t = TokenId.IntegerLiteral;
						}

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

						if (_parseChar == 'D' || _parseChar == 'd' || _parseChar == 'F' || _parseChar == 'f' || _parseChar == 'M' || _parseChar == 'm')
						{
							t = TokenId.RealLiteral;
							NextChar();
						}

						// 'U' | 'u' | 'L' | 'l' | 'UL' | 'Ul' | 'uL' | 'ul' | 'LU' | 'Lu' | 'lU' | 'lu'
						if (_parseChar == 'U' || _parseChar == 'u')
						{
							NextChar();
							if (_parseChar == 'L' || _parseChar == 'l')
								NextChar();
						}
						else if (_parseChar == 'L' || _parseChar == 'l')
						{
							NextChar();
							if (_parseChar == 'U' || _parseChar == 'u')
								NextChar();
						}

						break;
					}
					if (_parsePosition == _expressionTextLength)
					{
						t = TokenId.End;
						break;
					}
					throw CreateParseException(_parsePosition, ErrorMessages.InvalidCharacter, _parseChar);
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
				throw CreateParseException(_parsePosition, ErrorMessages.DigitExpected);
		}

		// ReSharper disable once UnusedParameter.Local
		private void ValidateToken(TokenId t, string errorMessage)
		{
			if (_token.id != t)
				throw CreateParseException(_token.pos, errorMessage);
		}

		// ReSharper disable once UnusedParameter.Local
		private void ValidateToken(TokenId t)
		{
			if (_token.id != t)
				throw CreateParseException(_token.pos, ErrorMessages.SyntaxError);
		}

		private static Exception WrapWithParseException(int pos, string msg, Exception ex)
		{
			return new ParseException(msg, pos, ex);
		}

		private static Exception CreateParseException(int pos, string format, params object[] args)
		{
			return new ParseException(string.Format(format, args), pos);
		}

		private static Expression GenerateNullableTypeConversion(Expression expr)
		{
			var exprType = expr.Type;

			if (IsNullableType(exprType) ||
			    !exprType.IsValueType)
			{
				return expr;
			}

			var conversionType = typeof(Nullable<>).MakeGenericType(exprType);
			return Expression.ConvertChecked(expr, conversionType);
		}

		/// <summary>
		/// Expression that wraps over an interpreter. This is used when parsing a lambda expression 
		/// definition, because we don't know the parameters type before resolution.
		/// </summary>
		private class InterpreterExpression : Expression
		{
			private readonly Interpreter _interpreter;
			private readonly string _expressionText;
			private readonly IList<Parameter> _parameters;
			private Type _type;

			public InterpreterExpression(ParserArguments parserArguments, string expressionText, params Parameter[] parameters)
			{
				_interpreter = new Interpreter(parserArguments.Settings.Clone());
				_expressionText = expressionText;
				_parameters = parameters;

				// convert the parent's parameters to variables
				// note: this doesn't impact the initial settings, because they're cloned
				foreach (var pe in parserArguments.DeclaredParameters)
				{
					_interpreter.SetVariable(pe.Name, pe.Value, pe.Type);
				}

				// prior to evaluation, we don't know the generic arguments types
				_type = typeof(Func<>).Assembly.GetType($"System.Func`{parameters.Length + 1}");
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
				var lambdaExpr = _interpreter.ParseAsExpression(delegateType, _expressionText, _parameters.Select(p => p.Name).ToArray());
				_type = lambdaExpr.Type;
				return lambdaExpr;
			}

			internal bool IsCompatibleWithDelegate(Type target)
			{
				return target.IsGenericType
					&& target.BaseType == typeof(MulticastDelegate)
					&& target.GetGenericArguments().Length == _parameters.Count + 1;
			}
		}

		private class MethodData
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

			public override string ToString()
			{
				return MethodBase.ToString();
			}
		}

		private class IndexerData : MethodData
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
