using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace DynamicExpresso
{
    public class ExpressionEngine
    {
        ParserSettings _settings;

        public ExpressionEngine()
            :this(new ParserSettings())
        {
        }

        public ExpressionEngine(ParserSettings settings)
        {
            _settings = settings;
        }

        public ExpressionDefinition Parse(string expressionText, params ExpressionParameter[] parameters)
        {
            var arguments = (from p in parameters
                            select ParameterExpression.Parameter(p.Type, p.Name)).ToArray();

            var parser = new ExpressionParser(expressionText, arguments, _settings);
            var expression = parser.Parse();

            var lambdaExp = Expression.Lambda(expression, arguments);

            return new ExpressionDefinition(lambdaExp);
        }

        public object Eval(string expressionText, params ExpressionParameter[] parameters)
        {
            return Parse(expressionText, parameters).Eval(parameters);
        }
    }
}
