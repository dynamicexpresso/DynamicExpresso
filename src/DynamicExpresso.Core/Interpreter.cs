using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace DynamicExpresso
{
    public class Interpreter
    {
        ParserSettings _settings = new ParserSettings();

        public Interpreter SetVariable(string name, object value)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name");

            return SetExpression(name, Expression.Constant(value));
        }

        public Interpreter SetVariable(string name, object value, Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name");

            return SetExpression(name, Expression.Constant(value, type));
        }

        public Interpreter SetExpression(string name, Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name");

            _settings.Keywords.Add(name, expression);

            return this;
        }

        public Interpreter Using(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            return Using(type, type.Name);
        }

        public Interpreter Using(Type type, string typeName)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            if (string.IsNullOrWhiteSpace(typeName))
                throw new ArgumentNullException("typeName");

            _settings.KnownTypes.Add(typeName, type);

            return this;
        }

        public Function Parse(string expressionText, params FunctionParam[] parameters)
        {
            var arguments = (from p in parameters
                            select ParameterExpression.Parameter(p.Type, p.Name)).ToArray();

            var parser = new ExpressionParser(expressionText, arguments, _settings);
            var expression = parser.Parse();

            var lambdaExp = Expression.Lambda(expression, arguments);

            return new Function(lambdaExp);
        }

        public object Eval(string expressionText, params FunctionParam[] parameters)
        {
            return Parse(expressionText, parameters).Invoke(parameters);
        }
    }
}
