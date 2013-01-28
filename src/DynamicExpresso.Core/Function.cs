using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace DynamicExpresso
{
    public class Function
    {
        readonly LambdaExpression _lambdaExpression;

        public Function(LambdaExpression lambdaExpression)
        {
            if (lambdaExpression == null)
                throw new ArgumentNullException("lambdaExpression");

            _lambdaExpression = lambdaExpression;
        }

        public LambdaExpression LambdaExpression
        {
            get { return _lambdaExpression; }
        }

        public Type ReturnType
        {
            get { return _lambdaExpression.ReturnType; }
        }

        public FunctionParam[] Parameters
        {
            get
            {
                return _lambdaExpression.Parameters
                        .Select(p => new FunctionParam(p.Name, p.Type))
                        .ToArray();
            }
        }

        public object Invoke(params FunctionParam[] parameters)
        {
            var args = (from dp in _lambdaExpression.Parameters
                       join rp in parameters
                        on dp.Name equals rp.Name
                       select rp.Value).ToArray();

            return Invoke(args);
        }

        public object Invoke(params object[] args)
        {
            try
            {
                return _lambdaExpression.Compile().DynamicInvoke(args);
            }
            catch (TargetInvocationException exc)
            {
                if (exc.InnerException != null)
                {
                    exc.InnerException.PreserveStackTrace();
                    throw exc.InnerException;
                }
                else
                    throw;
            }
        }

        public override string ToString()
        {
            return _lambdaExpression.ToString();
        }
    }
}
