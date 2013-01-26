using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace DynamicExpresso
{
    public class Function
    {
        readonly LambdaExpression _lamdaExpression;

        public Function(LambdaExpression lamdaExpression)
        {
            _lamdaExpression = lamdaExpression;
        }

        public Type ReturnType
        {
            get { return _lamdaExpression.ReturnType; }
        }

        public FunctionParam[] Parameters
        {
            get
            {
                return _lamdaExpression.Parameters
                        .Select(p => new FunctionParam(p.Name, p.Type))
                        .ToArray();
            }
        }

        public object Invoke(params FunctionParam[] parameters)
        {
            var args = (from dp in _lamdaExpression.Parameters
                       join rp in parameters
                        on dp.Name equals rp.Name
                       select rp.Value).ToArray();

            return Invoke(args);
        }

        public object Invoke(params object[] args)
        {
            return _lamdaExpression.Compile().DynamicInvoke(args);
        }
    }
}
