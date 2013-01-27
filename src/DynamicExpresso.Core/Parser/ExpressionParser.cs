using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Globalization;

// Code based on the Dynamic.cs file of the DynamicQuery sample by Microsoft
// http://msdn.microsoft.com/en-us/vstudio/bb894665.aspx 
// http://weblogs.asp.net/scottgu/archive/2008/01/07/dynamic-linq-part-1-using-the-linq-dynamic-query-library.aspx
//
// Copyright (C) Microsoft Corporation.  All rights reserved.

namespace DynamicExpresso
{
    internal class ExpressionParser
    {
        const NumberStyles ParseLiteralNumberStyle = NumberStyles.AllowLeadingSign;
        const NumberStyles ParseLiteralUnsignedNumberStyle = NumberStyles.None;
        const NumberStyles ParseLiteralDecimalNumberStyle = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;
        static CultureInfo ParseCulture = CultureInfo.InvariantCulture;

        struct Token
        {
            public TokenId id;
            public string text;
            public int pos;
        }

        enum TokenId
        {
            Unknown,
            End,
            Identifier,
            StringLiteral,
            IntegerLiteral,
            RealLiteral,
            Exclamation,
            Percent,
            OpenParen,
            CloseParen,
            Asterisk,
            Plus,
            Comma,
            Minus,
            Dot,
            Slash,
            Colon,
            LessThan,
            GreaterThan,
            Question,
            OpenBracket,
            CloseBracket,
            Bar,
            ExclamationEqual,
            DoubleAmphersand,
            LessThanEqual,
            DoubleEqual,
            GreaterThanEqual,
            DoubleBar
        }

        interface ILogicalSignatures
        {
            void F(bool x, bool y);
            void F(bool? x, bool? y);
        }

        interface IArithmeticSignatures
        {
            void F(int x, int y);
            void F(uint x, uint y);
            void F(long x, long y);
            void F(ulong x, ulong y);
            void F(float x, float y);
            void F(double x, double y);
            void F(decimal x, decimal y);
            void F(int? x, int? y);
            void F(uint? x, uint? y);
            void F(long? x, long? y);
            void F(ulong? x, ulong? y);
            void F(float? x, float? y);
            void F(double? x, double? y);
            void F(decimal? x, decimal? y);
        }

        interface IRelationalSignatures : IArithmeticSignatures
        {
            void F(string x, string y);
            void F(char x, char y);
            void F(DateTime x, DateTime y);
            void F(TimeSpan x, TimeSpan y);
            void F(char? x, char? y);
            void F(DateTime? x, DateTime? y);
            void F(TimeSpan? x, TimeSpan? y);
        }

        interface IEqualitySignatures : IRelationalSignatures
        {
            void F(bool x, bool y);
            void F(bool? x, bool? y);
        }

        interface IAddSignatures : IArithmeticSignatures
        {
            void F(DateTime x, TimeSpan y);
            void F(TimeSpan x, TimeSpan y);
            void F(DateTime? x, TimeSpan? y);
            void F(TimeSpan? x, TimeSpan? y);
        }

        interface ISubtractSignatures : IAddSignatures
        {
            void F(DateTime x, DateTime y);
            void F(DateTime? x, DateTime? y);
        }

        interface INegationSignatures
        {
            void F(int x);
            void F(long x);
            void F(float x);
            void F(double x);
            void F(decimal x);
            void F(int? x);
            void F(long? x);
            void F(float? x);
            void F(double? x);
            void F(decimal? x);
        }

        interface INotSignatures
        {
            void F(bool x);
            void F(bool? x);
        }

        //interface IEnumerableSignatures
        //{
        //    void Where(bool predicate);
        //    void Any();
        //    void Any(bool predicate);
        //    void All(bool predicate);
        //    void Count();
        //    void Count(bool predicate);
        //    void Min(object selector);
        //    void Max(object selector);
        //    void Sum(int selector);
        //    void Sum(int? selector);
        //    void Sum(long selector);
        //    void Sum(long? selector);
        //    void Sum(float selector);
        //    void Sum(float? selector);
        //    void Sum(double selector);
        //    void Sum(double? selector);
        //    void Sum(decimal selector);
        //    void Sum(decimal? selector);
        //    void Average(int selector);
        //    void Average(int? selector);
        //    void Average(long selector);
        //    void Average(long? selector);
        //    void Average(float selector);
        //    void Average(float? selector);
        //    void Average(double selector);
        //    void Average(double? selector);
        //    void Average(decimal selector);
        //    void Average(decimal? selector);
        //}

        ParserSettings _settings;

        Dictionary<string, Expression> _parameters;
        //Dictionary<Expression, string> _literals;

        //ParameterExpression it;

        string text;
        int textPos;
        int textLen;
        char ch;
        Token token;

        public ExpressionParser(string expression, ParameterExpression[] parameters, ParserSettings settings)
        {
            _settings = settings;

            _parameters = new Dictionary<string, Expression>();
            //_literals = new Dictionary<Expression, string>();

            ProcessParameters(parameters);

            text = expression;
            textLen = text.Length;
            SetTextPos(0);
            NextToken();
        }

        void AddParameter(string name, Expression value)
        {
            if (_parameters.ContainsKey(name))
                throw ParseError(ErrorMessages.DuplicateIdentifier, name);
            _parameters.Add(name, value);
        }

        void ProcessParameters(IEnumerable<ParameterExpression> parameters)
        {
            foreach (ParameterExpression pe in parameters)
                if (!String.IsNullOrEmpty(pe.Name))
                    AddParameter(pe.Name, pe);
            //if (parameters.Count() == 1 && String.IsNullOrEmpty(parameters.First().Name))
            //    it = parameters.First();
        }

        public Expression Parse()
        {
            Expression expr = ParsePipelineInit();

            ValidateToken(TokenId.End, ErrorMessages.SyntaxError);
            return expr;
        }

        Expression ParsePipelineInit()
        {
            return ParseConditional();
        }

        // ?: operator
        Expression ParseConditional()
        {
            int errorPos = token.pos;
            Expression expr = ParseLogicalOr();
            if (token.id == TokenId.Question)
            {
                NextToken();
                Expression expr1 = ParsePipelineInit();
                ValidateToken(TokenId.Colon, ErrorMessages.ColonExpected);
                NextToken();
                Expression expr2 = ParsePipelineInit();
                expr = GenerateConditional(expr, expr1, expr2, errorPos);
            }
            return expr;
        }

        // || operator
        Expression ParseLogicalOr()
        {
            Expression left = ParseLogicalAnd();
            while (token.id == TokenId.DoubleBar)
            {
                Token op = token;
                NextToken();
                Expression right = ParseLogicalAnd();
                CheckAndPromoteOperands(typeof(ILogicalSignatures), op.text, ref left, ref right, op.pos);
                left = Expression.OrElse(left, right);
            }
            return left;
        }

        // && operator
        Expression ParseLogicalAnd()
        {
            Expression left = ParseComparison();
            while (token.id == TokenId.DoubleAmphersand)
            {
                Token op = token;
                NextToken();
                Expression right = ParseComparison();
                CheckAndPromoteOperands(typeof(ILogicalSignatures), op.text, ref left, ref right, op.pos);
                left = Expression.AndAlso(left, right);
            }
            return left;
        }

        // ==, !=, >, >=, <, <= operators
        Expression ParseComparison()
        {
            Expression left = ParseAdditive();
            while (token.id == TokenId.DoubleEqual ||token.id == TokenId.ExclamationEqual  ||
                token.id == TokenId.GreaterThan || token.id == TokenId.GreaterThanEqual ||
                token.id == TokenId.LessThan || token.id == TokenId.LessThanEqual)
            {
                Token op = token;
                NextToken();
                Expression right = ParseAdditive();
                bool isEquality = op.id == TokenId.DoubleEqual || op.id == TokenId.ExclamationEqual;
                if (isEquality && !left.Type.IsValueType && !right.Type.IsValueType)
                {
                    if (left.Type != right.Type)
                    {
                        if (left.Type.IsAssignableFrom(right.Type))
                        {
                            right = Expression.Convert(right, left.Type);
                        }
                        else if (right.Type.IsAssignableFrom(left.Type))
                        {
                            left = Expression.Convert(left, right.Type);
                        }
                        else
                        {
                            throw IncompatibleOperandsError(op.text, left, right, op.pos);
                        }
                    }
                }
                else if (IsEnumType(left.Type) || IsEnumType(right.Type))
                {
                    if (left.Type != right.Type)
                    {
                        Expression e;
                        if ((e = PromoteExpression(right, left.Type, true)) != null)
                        {
                            right = e;
                        }
                        else if ((e = PromoteExpression(left, right.Type, true)) != null)
                        {
                            left = e;
                        }
                        else
                        {
                            throw IncompatibleOperandsError(op.text, left, right, op.pos);
                        }
                    }
                }
                else
                {
                    CheckAndPromoteOperands(isEquality ? typeof(IEqualitySignatures) : typeof(IRelationalSignatures),
                        op.text, ref left, ref right, op.pos);
                }
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

        // +, -, & operators
        Expression ParseAdditive()
        {
            Expression left = ParseMultiplicative();
            while (token.id == TokenId.Plus || token.id == TokenId.Minus)
            {
                Token op = token;
                NextToken();
                Expression right = ParseMultiplicative();
                switch (op.id)
                {
                    case TokenId.Plus:
                        if (left.Type == typeof(string) || right.Type == typeof(string))
                        {
                            left = GenerateStringConcat(left, right);
                        }
                        else
                        {
                            CheckAndPromoteOperands(typeof(IAddSignatures), op.text, ref left, ref right, op.pos);
                            left = GenerateAdd(left, right);
                        }
                        break;
                    case TokenId.Minus:
                        CheckAndPromoteOperands(typeof(ISubtractSignatures), op.text, ref left, ref right, op.pos);
                        left = GenerateSubtract(left, right);
                        break;
                }
            }
            return left;
        }

        // *, /, %, mod operators
        Expression ParseMultiplicative()
        {
            Expression left = ParseUnary();
            while (token.id == TokenId.Asterisk || token.id == TokenId.Slash ||
                token.id == TokenId.Percent || TokenIdentifierIs("mod"))
            {
                Token op = token;
                NextToken();
                Expression right = ParseUnary();
                CheckAndPromoteOperands(typeof(IArithmeticSignatures), op.text, ref left, ref right, op.pos);
                switch (op.id)
                {
                    case TokenId.Asterisk:
                        left = Expression.Multiply(left, right);
                        break;
                    case TokenId.Slash:
                        left = Expression.Divide(left, right);
                        break;
                    case TokenId.Percent:
                    case TokenId.Identifier:
                        left = Expression.Modulo(left, right);
                        break;
                }
            }
            return left;
        }

        // -, ! unary operators
        Expression ParseUnary()
        {
            if (token.id == TokenId.Minus || token.id == TokenId.Exclamation)
            {
                Token op = token;
                NextToken();
                if (op.id == TokenId.Minus && (token.id == TokenId.IntegerLiteral ||
                    token.id == TokenId.RealLiteral))
                {
                    token.text = "-" + token.text;
                    token.pos = op.pos;
                    return ParsePrimary();
                }
                Expression expr = ParseUnary();
                if (op.id == TokenId.Minus)
                {
                    CheckAndPromoteOperand(typeof(INegationSignatures), op.text, ref expr, op.pos);
                    expr = Expression.Negate(expr);
                }
                else
                {
                    CheckAndPromoteOperand(typeof(INotSignatures), op.text, ref expr, op.pos);
                    expr = Expression.Not(expr);
                }
                return expr;
            }
            return ParsePrimary();
        }

        Expression ParsePrimary()
        {
            var tokenPos = token.pos;
            Expression expr = ParsePrimaryStart();
            while (true)
            {
                if (token.id == TokenId.Dot)
                {
                    NextToken();
                    expr = ParseMemberAccess(null, expr);
                }
                else if (token.id == TokenId.OpenBracket)
                {
                    expr = ParseElementAccess(expr);
                }
                else if (token.id == TokenId.OpenParen)
                {
                    LambdaExpression lambda = expr as LambdaExpression;
                    if (lambda != null) return ParseLambdaInvocation(lambda, tokenPos);

                    if (typeof(Delegate).IsAssignableFrom(expr.Type))
                        expr = ParseDelegateInvocation(expr, tokenPos);
                }
                else
                {
                    break;
                }
            }
            return expr;
        }

        Expression ParsePrimaryStart()
        {
            switch (token.id)
            {
                case TokenId.Identifier:
                    return ParseIdentifier();
                case TokenId.StringLiteral:
                    return ParseStringLiteral();
                case TokenId.IntegerLiteral:
                    return ParseIntegerLiteral();
                case TokenId.RealLiteral:
                    return ParseRealLiteral();
                case TokenId.OpenParen:
                    return ParseParenExpression();
                default:
                    throw ParseError(ErrorMessages.ExpressionExpected);
            }
        }

        Expression ParseStringLiteral()
        {
            ValidateToken(TokenId.StringLiteral);
            char quote = token.text[0];
            string s = token.text.Substring(1, token.text.Length - 2);
            int start = 0;
            while (true)
            {
                int i = s.IndexOf(quote, start);
                if (i < 0) break;
                s = s.Remove(i, 1);
                start = i + 1;
            }
            if (quote == '\'')
            {
                if (s.Length != 1)
                    throw ParseError(ErrorMessages.InvalidCharacterLiteral);
                NextToken();
                return CreateLiteral(s[0], s);
            }
            NextToken();
            return CreateLiteral(s, s);
        }

        Expression ParseIntegerLiteral()
        {
            ValidateToken(TokenId.IntegerLiteral);
            string text = token.text;
            if (text[0] != '-')
            {
                ulong value;
                if (!UInt64.TryParse(text, ParseLiteralUnsignedNumberStyle, ParseCulture, out value))
                    throw ParseError(ErrorMessages.InvalidIntegerLiteral, text);
                NextToken();
                if (value <= (ulong)Int32.MaxValue) return CreateLiteral((int)value, text);
                if (value <= (ulong)UInt32.MaxValue) return CreateLiteral((uint)value, text);
                if (value <= (ulong)Int64.MaxValue) return CreateLiteral((long)value, text);
                return CreateLiteral(value, text);
            }
            else
            {
                long value;
                if (!Int64.TryParse(text, ParseLiteralNumberStyle, ParseCulture, out value))
                    throw ParseError(ErrorMessages.InvalidIntegerLiteral, text);
                NextToken();
                if (value >= Int32.MinValue && value <= Int32.MaxValue)
                    return CreateLiteral((int)value, text);
                return CreateLiteral(value, text);
            }
        }

        Expression ParseRealLiteral()
        {
            ValidateToken(TokenId.RealLiteral);
            string text = token.text;
            object value = null;
            char last = text[text.Length - 1];
            if (last == 'F' || last == 'f')
            {
                float f;
                if (float.TryParse(text.Substring(0, text.Length - 1), ParseLiteralDecimalNumberStyle, ParseCulture, out f)) 
                    value = f;
            }
            else if (last == 'M' || last == 'm')
            {
                decimal dc;
                if (decimal.TryParse(text.Substring(0, text.Length - 1), ParseLiteralDecimalNumberStyle, ParseCulture, out dc)) 
                    value = dc;
            }
            else
            {
                double d;
                if (double.TryParse(text, ParseLiteralDecimalNumberStyle, ParseCulture, out d)) 
                    value = d;
            }
            if (value == null) throw ParseError(ErrorMessages.InvalidRealLiteral, text);
            NextToken();
            return CreateLiteral(value, text);
        }

        Expression CreateLiteral(object value, string text)
        {
            ConstantExpression expr = Expression.Constant(value);
            //_literals.Add(expr, text);
            return expr;
        }

        Expression ParseParenExpression()
        {
            ValidateToken(TokenId.OpenParen, ErrorMessages.OpenParenExpected);
            NextToken();
            Expression e = ParsePipelineInit();
            ValidateToken(TokenId.CloseParen, ErrorMessages.CloseParenOrOperatorExpected);
            NextToken();
            return e;
        }

        Expression ParseIdentifier()
        {
            ValidateToken(TokenId.Identifier);

            //if (token.text == ParserConstants.keywordIt)
            //    return ParseIt();
            if (token.text == ParserConstants.keywordNew)
                return ParseNew();

            Type knownType;
            if (_settings.KnownTypes.TryGetValue(token.text, out knownType))
            {
                return ParseTypeKeyword(knownType);
            }

            Expression keywordExpression;
            if (_settings.Keywords.TryGetValue(token.text, out keywordExpression))
            {
                //LambdaExpression lambda = keywordExpression as LambdaExpression;
                //if (lambda != null) return ParseLambdaInvocation(lambda);

                NextToken();
                return keywordExpression;
            }

            Expression parameterExpression;
            if (_parameters.TryGetValue(token.text, out parameterExpression))
            {
                //LambdaExpression lambda = parameterExpression as LambdaExpression;
                //if (lambda != null) return ParseLambdaInvocation(lambda);

                NextToken();
                return parameterExpression;
            }

            //if (it != null) 
            //    return ParseMemberAccess(null, it);

            throw ParseError(ErrorMessages.UnknownIdentifier, token.text);
        }

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

        Expression GenerateConditional(Expression test, Expression expr1, Expression expr2, int errorPos)
        {
            if (test.Type != typeof(bool))
                throw ParseError(errorPos, ErrorMessages.FirstExprMustBeBool);
            if (expr1.Type != expr2.Type)
            {
                Expression expr1as2 = expr2 != ParserConstants.nullLiteral ? PromoteExpression(expr1, expr2.Type, true) : null;
                Expression expr2as1 = expr1 != ParserConstants.nullLiteral ? PromoteExpression(expr2, expr1.Type, true) : null;
                if (expr1as2 != null && expr2as1 == null)
                {
                    expr1 = expr1as2;
                }
                else if (expr2as1 != null && expr1as2 == null)
                {
                    expr2 = expr2as1;
                }
                else
                {
                    string type1 = expr1 != ParserConstants.nullLiteral ? expr1.Type.Name : "null";
                    string type2 = expr2 != ParserConstants.nullLiteral ? expr2.Type.Name : "null";
                    if (expr1as2 != null && expr2as1 != null)
                        throw ParseError(errorPos, ErrorMessages.BothTypesConvertToOther, type1, type2);
                    throw ParseError(errorPos, ErrorMessages.NeitherTypeConvertsToOther, type1, type2);
                }
            }
            return Expression.Condition(test, expr1, expr2);
        }

        Expression ParseNew()
        {
            NextToken();
            ValidateToken(TokenId.Identifier, ErrorMessages.IdentifierExpected);

            Type newType;
            if (!_settings.KnownTypes.TryGetValue(token.text, out newType))
                throw ParseError(token.pos, ErrorMessages.UnknownIdentifier, token.text);

            NextToken();
            var args = ParseArgumentList();

            var constructor = newType.GetConstructor(args.Select(p => p.Type).ToArray());
            if (constructor == null)
                throw ParseError(token.pos, ErrorMessages.NoApplicableConstructor, newType);

            return Expression.MemberInit(Expression.New(constructor, args));
        }

        Expression ParseLambdaInvocation(LambdaExpression lambda, int errorPos)
        {
            //int errorPos = token.pos;
            //NextToken();
            Expression[] args = ParseArgumentList();
            MethodBase method;
            if (FindMethod(lambda.Type, "Invoke", false, args, out method) != 1)
                throw ParseError(errorPos, ErrorMessages.ArgsIncompatibleWithLambda);
            return Expression.Invoke(lambda, args);
        }

        Expression ParseDelegateInvocation(Expression delegateExp, int errorPos)
        {
            //int errorPos = token.pos;
            //NextToken();
            Expression[] args = ParseArgumentList();
            MethodBase method;
            if (FindMethod(delegateExp.Type, "Invoke", false, args, out method) != 1)
                throw ParseError(errorPos, ErrorMessages.ArgsIncompatibleWithLambda);
            return Expression.Invoke(delegateExp, args);
        }

        Expression ParseTypeKeyword(Type type)
        {
            int errorPos = token.pos;
            NextToken();
            if (token.id == TokenId.Question)
            {
                if (!type.IsValueType || IsNullableType(type))
                    throw ParseError(errorPos, ErrorMessages.TypeHasNoNullableForm, GetTypeName(type));
                type = typeof(Nullable<>).MakeGenericType(type);
                NextToken();
            }
            if (token.id == TokenId.OpenParen)
            {
                Expression[] args = ParseArgumentList();
                MethodBase method;
                switch (FindBestMethod(type.GetConstructors(), args, out method))
                {
                    case 0:
                        if (args.Length == 1)
                            return GenerateConversion(args[0], type, errorPos);
                        throw ParseError(errorPos, ErrorMessages.NoMatchingConstructor, GetTypeName(type));
                    case 1:
                        return Expression.New((ConstructorInfo)method, args);
                    default:
                        throw ParseError(errorPos, ErrorMessages.AmbiguousConstructorInvocation, GetTypeName(type));
                }
            }

            ValidateToken(TokenId.Dot, ErrorMessages.DotOrOpenParenExpected);
            NextToken();
            return ParseMemberAccess(type, null);
        }

        Expression GenerateConversion(Expression expr, Type type, int errorPos)
        {
            Type exprType = expr.Type;
            if (exprType == type) return expr;
            if (exprType.IsValueType && type.IsValueType)
            {
                if ((IsNullableType(exprType) || IsNullableType(type)) &&
                    GetNonNullableType(exprType) == GetNonNullableType(type))
                    return Expression.Convert(expr, type);
                if ((IsNumericType(exprType) || IsEnumType(exprType)) &&
                    (IsNumericType(type)) || IsEnumType(type))
                    return Expression.ConvertChecked(expr, type);
            }
            if (exprType.IsAssignableFrom(type) || type.IsAssignableFrom(exprType) ||
                exprType.IsInterface || type.IsInterface)
                return Expression.Convert(expr, type);
            throw ParseError(errorPos, ErrorMessages.CannotConvertValue,
                GetTypeName(exprType), GetTypeName(type));
        }

        Expression ParseMemberAccess(Type type, Expression instance)
        {
            if (instance != null) type = instance.Type;
            int errorPos = token.pos;
            string id = GetIdentifier();
            NextToken();
            if (token.id == TokenId.OpenParen)
            {
                return ParseMethodInvocation(type, instance, errorPos, id);
            }
            else
            {
                MemberInfo member = FindPropertyOrField(type, id, instance == null);
                if (member == null)
                    throw ParseError(errorPos, ErrorMessages.UnknownPropertyOrField,
                        id, GetTypeName(type));
                return member is PropertyInfo ?
                    Expression.Property(instance, (PropertyInfo)member) :
                    Expression.Field(instance, (FieldInfo)member);
            }
        }

        private Expression ParseMethodInvocation(Type type, Expression instance, int errorPos, string id)
        {
            //if (instance != null && type != typeof(string))
            //{
            //    Type enumerableType = FindGenericType(typeof(IEnumerable<>), type);
            //    if (enumerableType != null)
            //    {
            //        Type elementType = enumerableType.GetGenericArguments()[0];
            //        return ParseAggregate(instance, elementType, id, errorPos);
            //    }
            //}
            Expression[] args = ParseArgumentList();

            //Type[] argsType = args.Select(p => p.Type).ToArray();
            //return Expression.Call(instance, id, argsType, args);

            MethodBase mb;
            switch (FindMethod(type, id, instance == null, args, out mb))
            {
                case 0:
                    throw ParseError(errorPos, ErrorMessages.NoApplicableMethod, id, GetTypeName(type));
                case 1:
                    MethodInfo method = (MethodInfo)mb;
                    //if (!IsPredefinedType(method.DeclaringType))
                    //    throw ParseError(errorPos, Res.MethodsAreInaccessible, GetTypeName(method.DeclaringType));

                    if (method.ReturnType == typeof(void))
                    {
                        throw ParseError(errorPos, ErrorMessages.MethodIsVoid, id, GetTypeName(method.DeclaringType));
                    }

                    return Expression.Call(instance, method, args);
                default:
                    throw ParseError(errorPos, ErrorMessages.AmbiguousMethodInvocation, id, GetTypeName(type));
            }
        }

        static Type FindGenericType(Type generic, Type type)
        {
            while (type != null && type != typeof(object))
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == generic) return type;
                if (generic.IsInterface)
                {
                    foreach (Type intfType in type.GetInterfaces())
                    {
                        Type found = FindGenericType(generic, intfType);
                        if (found != null) return found;
                    }
                }
                type = type.BaseType;
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

        Expression[] ParseArgumentList()
        {
            ValidateToken(TokenId.OpenParen, ErrorMessages.OpenParenExpected);
            NextToken();
            Expression[] args = token.id != TokenId.CloseParen ? ParseArguments() : new Expression[0];
            ValidateToken(TokenId.CloseParen, ErrorMessages.CloseParenOrCommaExpected);
            NextToken();
            return args;
        }

        Expression[] ParseArguments()
        {
            List<Expression> argList = new List<Expression>();
            while (true)
            {
                argList.Add(ParsePipelineInit());
                if (token.id != TokenId.Comma) break;
                NextToken();
            }
            return argList.ToArray();
        }

        Expression ParseElementAccess(Expression expr)
        {
            int errorPos = token.pos;
            ValidateToken(TokenId.OpenBracket, ErrorMessages.OpenParenExpected);
            NextToken();
            Expression[] args = ParseArguments();
            ValidateToken(TokenId.CloseBracket, ErrorMessages.CloseBracketOrCommaExpected);
            NextToken();
            if (expr.Type.IsArray)
            {
                if (expr.Type.GetArrayRank() != 1 || args.Length != 1)
                    throw ParseError(errorPos, ErrorMessages.CannotIndexMultiDimArray);
                Expression index = PromoteExpression(args[0], typeof(int), true);
                if (index == null)
                    throw ParseError(errorPos, ErrorMessages.InvalidIndex);
                return Expression.ArrayIndex(expr, index);
            }
            else
            {
                MethodBase mb;
                switch (FindIndexer(expr.Type, args, out mb))
                {
                    case 0:
                        throw ParseError(errorPos, ErrorMessages.NoApplicableIndexer,
                            GetTypeName(expr.Type));
                    case 1:
                        return Expression.Call(expr, (MethodInfo)mb, args);
                    default:
                        throw ParseError(errorPos, ErrorMessages.AmbiguousIndexerInvocation,
                            GetTypeName(expr.Type));
                }
            }
        }

        static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        static Type GetNonNullableType(Type type)
        {
            return IsNullableType(type) ? type.GetGenericArguments()[0] : type;
        }

        static string GetTypeName(Type type)
        {
            Type baseType = GetNonNullableType(type);
            string s = baseType.Name;
            if (type != baseType) s += '?';
            return s;
        }

        static bool IsNumericType(Type type)
        {
            return GetNumericTypeKind(type) != 0;
        }

        static bool IsSignedIntegralType(Type type)
        {
            return GetNumericTypeKind(type) == 2;
        }

        static bool IsUnsignedIntegralType(Type type)
        {
            return GetNumericTypeKind(type) == 3;
        }

        static int GetNumericTypeKind(Type type)
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

        static bool IsEnumType(Type type)
        {
            return GetNonNullableType(type).IsEnum;
        }

        void CheckAndPromoteOperand(Type signatures, string opName, ref Expression expr, int errorPos)
        {
            Expression[] args = new Expression[] { expr };
            MethodBase method;
            if (FindMethod(signatures, "F", false, args, out method) != 1)
                throw ParseError(errorPos, ErrorMessages.IncompatibleOperand,
                    opName, GetTypeName(args[0].Type));
            expr = args[0];
        }

        void CheckAndPromoteOperands(Type signatures, string opName, ref Expression left, ref Expression right, int errorPos)
        {
            Expression[] args = new Expression[] { left, right };
            MethodBase method;
            if (FindMethod(signatures, "F", false, args, out method) != 1)
                throw IncompatibleOperandsError(opName, left, right, errorPos);
            left = args[0];
            right = args[1];
        }

        Exception IncompatibleOperandsError(string opName, Expression left, Expression right, int pos)
        {
            return ParseError(pos, ErrorMessages.IncompatibleOperands,
                opName, GetTypeName(left.Type), GetTypeName(right.Type));
        }

        MemberInfo FindPropertyOrField(Type type, string memberName, bool staticAccess)
        {
            BindingFlags flags = BindingFlags.Public | BindingFlags.DeclaredOnly |
                (staticAccess ? BindingFlags.Static : BindingFlags.Instance);
            foreach (Type t in SelfAndBaseTypes(type))
            {
                MemberInfo[] members = t.FindMembers(MemberTypes.Property | MemberTypes.Field, flags, Type.FilterName, memberName);
                if (members.Length != 0) 
                    return members[0];
            }
            return null;
        }

        int FindMethod(Type type, string methodName, bool staticAccess, Expression[] args, out MethodBase method)
        {
            BindingFlags flags = BindingFlags.Public | BindingFlags.DeclaredOnly |
                (staticAccess ? BindingFlags.Static : BindingFlags.Instance);
            foreach (Type t in SelfAndBaseTypes(type))
            {
                MemberInfo[] members = t.FindMembers(MemberTypes.Method, flags, Type.FilterName, methodName);
                int count = FindBestMethod(members.Cast<MethodBase>(), args, out method);
                if (count != 0) 
                    return count;
            }
            method = null;
            return 0;
        }

        int FindIndexer(Type type, Expression[] args, out MethodBase method)
        {
            foreach (Type t in SelfAndBaseTypes(type))
            {
                MemberInfo[] members = t.GetDefaultMembers();
                if (members.Length != 0)
                {
                    IEnumerable<MethodBase> methods = members.
                        OfType<PropertyInfo>().
                        Select(p => (MethodBase)p.GetGetMethod()).
                        Where(m => m != null);
                    int count = FindBestMethod(methods, args, out method);
                    if (count != 0) return count;
                }
            }
            method = null;
            return 0;
        }

        static IEnumerable<Type> SelfAndBaseTypes(Type type)
        {
            if (type.IsInterface)
            {
                List<Type> types = new List<Type>();
                AddInterface(types, type);
                return types;
            }
            return SelfAndBaseClasses(type);
        }

        static IEnumerable<Type> SelfAndBaseClasses(Type type)
        {
            while (type != null)
            {
                yield return type;
                type = type.BaseType;
            }
        }

        static void AddInterface(List<Type> types, Type type)
        {
            if (!types.Contains(type))
            {
                types.Add(type);
                foreach (Type t in type.GetInterfaces()) AddInterface(types, t);
            }
        }

        class MethodData
        {
            public MethodBase MethodBase;
            public ParameterInfo[] Parameters;
            public Expression[] Args;
        }

        int FindBestMethod(IEnumerable<MethodBase> methods, Expression[] args, out MethodBase method)
        {
            MethodData[] applicable = methods.
                Select(m => new MethodData { MethodBase = m, Parameters = m.GetParameters() }).
                Where(m => CheckIfMethodIsApplicableAndPrepareIt(m, args)).
                ToArray();
            if (applicable.Length > 1)
            {
                applicable = applicable.
                    Where(m => applicable.All(n => m == n || IsBetterThan(args, m, n))).
                    ToArray();
            }
            if (applicable.Length == 1)
            {
                MethodData md = applicable[0];
                for (int i = 0; i < args.Length; i++) args[i] = md.Args[i];
                method = md.MethodBase;
            }
            else
            {
                method = null;
            }
            return applicable.Length;
        }

        bool CheckIfMethodIsApplicableAndPrepareIt(MethodData method, Expression[] args)
        {
            if (method.Parameters.Length != args.Length) 
                return false;

            Expression[] promotedArgs = new Expression[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                ParameterInfo pi = method.Parameters[i];
                if (pi.IsOut) 
                    return false;

                Expression promoted;
                if (pi.ParameterType.IsGenericParameter)
                {
                    promoted = args[i];
                }
                else
                {
                    promoted = PromoteExpression(args[i], pi.ParameterType, true);
                    if (promoted == null)
                        return false;
                }

                promotedArgs[i] = promoted;
            }
            method.Args = promotedArgs;

            if (method.MethodBase.IsGenericMethodDefinition &&
                method.MethodBase is MethodInfo)
            {
                var methodInfo = ((MethodInfo)method.MethodBase);
                var genericArgsType = new Type[methodInfo.GetGenericArguments().Length];
                int genericArgsTypeIndex = 0;
                for (int i = 0; i < method.Args.Length; i++)
                {
                    if (method.Parameters[i].ParameterType.IsGenericParameter)
                        genericArgsType[genericArgsTypeIndex++] = method.Args[i].Type;
                }

                method.MethodBase = methodInfo.MakeGenericMethod(genericArgsType);
            }

            return true;
        }

        Expression PromoteExpression(Expression expr, Type type, bool exact)
        {
            if (expr.Type == type) return expr;
            if (expr is ConstantExpression)
            {
                ConstantExpression ce = (ConstantExpression)expr;
                if (ce == ParserConstants.nullLiteral)
                {
                    if (!type.IsValueType || IsNullableType(type))
                        return Expression.Constant(null, type);
                }
                //else
                //{
                //    string text;
                //    if (_literals.TryGetValue(ce, out text))
                //    {
                //        Type target = GetNonNullableType(type);
                //        Object value = null;
                //        switch (Type.GetTypeCode(ce.Type))
                //        {
                //            case TypeCode.Int32:
                //            case TypeCode.UInt32:
                //            case TypeCode.Int64:
                //            case TypeCode.UInt64:
                //                value = ParseNumber(text, target);
                //                break;
                //            case TypeCode.Double:
                //                if (target == typeof(decimal)) value = ParseNumber(text, target);
                //                break;
                //            case TypeCode.String:
                //                value = ParseEnum(text, target);
                //                break;
                //        }
                //        if (value != null)
                //            return Expression.Constant(value, type);
                //    }
                //}
            }
            if (IsCompatibleWith(expr.Type, type))
            {
                if (type.IsValueType || exact) return Expression.Convert(expr, type);
                return expr;
            }
            return null;
        }

        object ParseNumber(string text, Type type)
        {
            switch (Type.GetTypeCode(GetNonNullableType(type)))
            {
                case TypeCode.SByte:
                    sbyte sb;
                    if (sbyte.TryParse(text, ParseLiteralNumberStyle, ParseCulture, out sb)) return sb;
                    break;
                case TypeCode.Byte:
                    byte b;
                    if (byte.TryParse(text, ParseLiteralNumberStyle, ParseCulture, out b)) return b;
                    break;
                case TypeCode.Int16:
                    short s;
                    if (short.TryParse(text, ParseLiteralNumberStyle, ParseCulture, out s)) return s;
                    break;
                case TypeCode.UInt16:
                    ushort us;
                    if (ushort.TryParse(text, ParseLiteralUnsignedNumberStyle, ParseCulture, out us)) return us;
                    break;
                case TypeCode.Int32:
                    int i;
                    if (int.TryParse(text, ParseLiteralNumberStyle, ParseCulture, out i)) return i;
                    break;
                case TypeCode.UInt32:
                    uint ui;
                    if (uint.TryParse(text, ParseLiteralUnsignedNumberStyle, ParseCulture, out ui)) return ui;
                    break;
                case TypeCode.Int64:
                    long l;
                    if (long.TryParse(text, ParseLiteralNumberStyle, ParseCulture, out l)) return l;
                    break;
                case TypeCode.UInt64:
                    ulong ul;
                    if (ulong.TryParse(text, ParseLiteralUnsignedNumberStyle, ParseCulture, out ul)) return ul;
                    break;
                case TypeCode.Single:
                    float f;
                    if (float.TryParse(text, ParseLiteralDecimalNumberStyle, ParseCulture, out f)) return f;
                    break;
                case TypeCode.Double:
                    double d;
                    if (double.TryParse(text, ParseLiteralDecimalNumberStyle, ParseCulture, out d)) return d;
                    break;
                case TypeCode.Decimal:
                    decimal e;
                    if (decimal.TryParse(text, ParseLiteralDecimalNumberStyle, ParseCulture, out e)) return e;
                    break;
            }
            return null;
        }

        static object ParseEnum(string name, Type type)
        {
            if (type.IsEnum)
            {
                MemberInfo[] memberInfos = type.FindMembers(MemberTypes.Field,
                    BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Static,
                    Type.FilterNameIgnoreCase, name);
                if (memberInfos.Length != 0) return ((FieldInfo)memberInfos[0]).GetValue(null);
            }
            return null;
        }

        static bool IsCompatibleWith(Type source, Type target)
        {
            if (source == target) return true;
            if (!target.IsValueType) return target.IsAssignableFrom(source);
            Type st = GetNonNullableType(source);
            Type tt = GetNonNullableType(target);
            if (st != source && tt == target) return false;
            TypeCode sc = st.IsEnum ? TypeCode.Object : Type.GetTypeCode(st);
            TypeCode tc = tt.IsEnum ? TypeCode.Object : Type.GetTypeCode(tt);
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

        static bool IsBetterThan(Expression[] args, MethodData m1, MethodData m2)
        {
            bool better = false;
            for (int i = 0; i < args.Length; i++)
            {
                int c = CompareConversions(args[i].Type,
                    m1.Parameters[i].ParameterType,
                    m2.Parameters[i].ParameterType);
                if (c < 0) return false;
                if (c > 0) better = true;
            }
            return better;
        }

        // Return 1 if s -> t1 is a better conversion than s -> t2
        // Return -1 if s -> t2 is a better conversion than s -> t1
        // Return 0 if neither conversion is better
        static int CompareConversions(Type s, Type t1, Type t2)
        {
            if (t1 == t2) return 0;
            if (s == t1) return 1;
            if (s == t2) return -1;
            bool t1t2 = IsCompatibleWith(t1, t2);
            bool t2t1 = IsCompatibleWith(t2, t1);
            if (t1t2 && !t2t1) return 1;
            if (t2t1 && !t1t2) return -1;
            if (IsSignedIntegralType(t1) && IsUnsignedIntegralType(t2)) return 1;
            if (IsSignedIntegralType(t2) && IsUnsignedIntegralType(t1)) return -1;
            return 0;
        }

        Expression GenerateEqual(Expression left, Expression right)
        {
            return Expression.Equal(left, right);
        }

        Expression GenerateNotEqual(Expression left, Expression right)
        {
            return Expression.NotEqual(left, right);
        }

        Expression GenerateGreaterThan(Expression left, Expression right)
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

        Expression GenerateGreaterThanEqual(Expression left, Expression right)
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

        Expression GenerateLessThan(Expression left, Expression right)
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

        Expression GenerateLessThanEqual(Expression left, Expression right)
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

        Expression GenerateAdd(Expression left, Expression right)
        {
            if (left.Type == typeof(string) && right.Type == typeof(string))
            {
                return GenerateStaticMethodCall("Concat", left, right);
            }
            return Expression.Add(left, right);
        }

        Expression GenerateSubtract(Expression left, Expression right)
        {
            return Expression.Subtract(left, right);
        }

        Expression GenerateStringConcat(Expression left, Expression right)
        {
            return Expression.Call(
                null,
                typeof(string).GetMethod("Concat", new[] { typeof(object), typeof(object) }),
                new[] { left, right });
        }

        MethodInfo GetStaticMethod(string methodName, Expression left, Expression right)
        {
            return left.Type.GetMethod(methodName, new[] { left.Type, right.Type });
        }

        Expression GenerateStaticMethodCall(string methodName, Expression left, Expression right)
        {
            return Expression.Call(null, GetStaticMethod(methodName, left, right), new[] { left, right });
        }

        void SetTextPos(int pos)
        {
            textPos = pos;
            ch = textPos < textLen ? text[textPos] : '\0';
        }

        void NextChar()
        {
            if (textPos < textLen) textPos++;
            ch = textPos < textLen ? text[textPos] : '\0';
        }

        void NextToken()
        {
            while (Char.IsWhiteSpace(ch)) NextChar();
            TokenId t;
            int tokenPos = textPos;
            switch (ch)
            {
                case '!':
                    NextChar();
                    if (ch == '=')
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
                    if (ch == '&')
                    {
                        NextChar();
                        t = TokenId.DoubleAmphersand;
                    }
                    else
                    {
                        throw ParseError(textPos, ErrorMessages.InvalidCharacter, ch);
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
                    if (ch == '=')
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
                    if (ch == '=')
                    {
                        NextChar();
                        t = TokenId.DoubleEqual;
                    }
                    else
                    {
                        throw ParseError(textPos, ErrorMessages.InvalidCharacter, ch);
                    }
                    break;
                case '>':
                    NextChar();
                    if (ch == '=')
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
                    t = TokenId.Question;
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
                    if (ch == '|')
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
                case '\'':
                    char quote = ch;
                    do
                    {
                        NextChar();
                        while (textPos < textLen && ch != quote) NextChar();
                        if (textPos == textLen)
                            throw ParseError(textPos, ErrorMessages.UnterminatedStringLiteral);
                        NextChar();
                    } while (ch == quote);
                    t = TokenId.StringLiteral;
                    break;
                default:
                    if (Char.IsLetter(ch) || ch == '@' || ch == '_')
                    {
                        do
                        {
                            NextChar();
                        } while (Char.IsLetterOrDigit(ch) || ch == '_');
                        t = TokenId.Identifier;
                        break;
                    }
                    if (Char.IsDigit(ch))
                    {
                        t = TokenId.IntegerLiteral;
                        do
                        {
                            NextChar();
                        } while (Char.IsDigit(ch));
                        if (ch == '.')
                        {
                            t = TokenId.RealLiteral;
                            NextChar();
                            ValidateDigit();
                            do
                            {
                                NextChar();
                            } while (Char.IsDigit(ch));
                        }
                        if (ch == 'E' || ch == 'e')
                        {
                            t = TokenId.RealLiteral;
                            NextChar();
                            if (ch == '+' || ch == '-') NextChar();
                            ValidateDigit();
                            do
                            {
                                NextChar();
                            } while (Char.IsDigit(ch));
                        }
                        if (ch == 'F' || ch == 'f' || ch == 'M' || ch == 'm')
                            NextChar();
                        break;
                    }
                    if (textPos == textLen)
                    {
                        t = TokenId.End;
                        break;
                    }
                    throw ParseError(textPos, ErrorMessages.InvalidCharacter, ch);
            }
            token.id = t;
            token.text = text.Substring(tokenPos, textPos - tokenPos);
            token.pos = tokenPos;
        }

        bool TokenIdentifierIs(string id)
        {
            return token.id == TokenId.Identifier && String.Equals(id, token.text, StringComparison.OrdinalIgnoreCase);
        }

        string GetIdentifier()
        {
            ValidateToken(TokenId.Identifier, ErrorMessages.IdentifierExpected);
            string id = token.text;
            if (id.Length > 1 && id[0] == '@') id = id.Substring(1);
            return id;
        }

        void ValidateDigit()
        {
            if (!Char.IsDigit(ch)) throw ParseError(textPos, ErrorMessages.DigitExpected);
        }

        void ValidateToken(TokenId t, string errorMessage)
        {
            if (token.id != t) throw ParseError(errorMessage);
        }

        void ValidateToken(TokenId t)
        {
            if (token.id != t) throw ParseError(ErrorMessages.SyntaxError);
        }

        Exception ParseError(string format, params object[] args)
        {
            return ParseError(token.pos, format, args);
        }

        Exception ParseError(int pos, string format, params object[] args)
        {
            return new ParseException(string.Format(format, args), pos);
        }
    }
}
