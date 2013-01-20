using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace DynamicExpresso
{
    public class ExpressionDefinition
    {
        readonly LambdaExpression _lamdaExpression;

        public ExpressionDefinition(LambdaExpression lamdaExpression)
        {
            _lamdaExpression = lamdaExpression;
        }

        public Type ReturnType
        {
            get { return _lamdaExpression.ReturnType; }
        }

        public object Eval(params ExpressionParameter[] parameters)
        {
            var args = (from dp in _lamdaExpression.Parameters
                       join rp in parameters
                        on dp.Name equals rp.Name
                       select rp.Value).ToArray();

            return _lamdaExpression.Compile().DynamicInvoke(args);
        }
    }
}
